using System;
using System.Collections.Generic;

namespace Prichut.Resort
{
    // Jeden controller core loopu (Build §4.3):
    // AWAITING_WAVE -> WAVE_INCOMING -> WAVE_ACTIVE (prichod/dobeh) -> WAVE_RESOLVING -> WAVE_COMPLETE -> UPGRADE.
    // Pauza uchova predchadzajuci stav. Generator, sluzby aj UI citaju ten isty stav, ziadne vlastne booleany.
    public sealed class ResortSession
    {
        public const float MaxStep = 0.1f;

        public ResortSession(ResortConfig config, ResortProgress progress = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            State = progress ?? ResortProgress.NewGame(config);
        }

        public ResortConfig Config { get; }
        public ResortProgress State { get; private set; }
        public WavePhase Phase => State.phase;
        public WaveRun Wave => State.wave;
        public int NextWaveNumber => State.completedWaves + 1;

        public event Action Changed;
        public event Action<ResortEvent> EventRaised;

        public bool CanStartWave => WavePhaseRules.CanStartWave(Phase);

        // Opakovany tap nevytvori druhu vlnu: start je mozny iba v AWAITING_WAVE.
        public bool StartWave()
        {
            if (!CanStartWave) return false;
            State.wave = CreateWave(NextWaveNumber);
            SetPhase(WavePhase.WaveIncoming);
            return true;
        }

        public bool Pause()
        {
            if (!WavePhaseRules.CanPause(Phase)) return false;
            State.phaseBeforePause = Phase;
            SetPhase(WavePhase.WavePaused);
            return true;
        }

        public bool Resume()
        {
            if (Phase != WavePhase.WavePaused) return false;
            SetPhase(State.phaseBeforePause);
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!WavePhaseRules.IsWaveRunning(Phase) || deltaTime <= 0f) return;
            while (deltaTime > 0f && WavePhaseRules.IsWaveRunning(Phase))
            {
                float step = Math.Min(deltaTime, MaxStep);
                Step(step);
                deltaTime -= step;
            }
            Changed?.Invoke();
        }

        private void Step(float dt)
        {
            WaveRun wave = State.wave;
            if (Phase == WavePhase.WaveIncoming)
            {
                wave.time += dt;
                if (wave.time >= Config.incomingSeconds)
                {
                    wave.time = 0f;
                    SetPhase(WavePhase.WaveActive);
                    Raise(new ResortEvent(ResortEventType.WaveStarted, value: wave.waveNumber));
                }
                return;
            }

            wave.time += dt;
            foreach (NeedRun run in wave.needs)
            {
                int capacity = CapacityOf(run.Building);
                if (!wave.arrivalClosed)
                {
                    int arrived = ArrivalSystem.Tick(run, Config, dt);
                    for (int i = 0; i < arrived; i++) ServiceSystem.Arrive(run, wave.time, capacity, Config);
                    if (arrived > 0) Raise(new ResortEvent(ResortEventType.UnitsArrived, run.need, run.Building, arrived));
                }

                int served = ServiceSystem.Tick(run, wave.time, dt, SpeedOf(run), capacity, Config);
                if (served > 0) Raise(new ResortEvent(ResortEventType.UnitsServed, run.need, run.Building, served));
            }

            // wave_timer zatvara PRICHOD, nie vlnu. Potom budovy dobiehaju rady (dobeh).
            if (!wave.arrivalClosed && wave.time >= Config.waveTimer) CloseArrival();
            UpdateStatuses(dt);
            if (IsWaveFinished(wave)) ResolveWave();
        }

        // Koniec vyzaduje uzavrety prichod a backlog = 0 pri vsetkych aktivnych needoch.
        // Prazdny rad s obsadenym miestom este vlnu neukonci.
        public static bool IsWaveFinished(WaveRun wave)
        {
            if (wave == null || !wave.arrivalClosed) return false;
            foreach (NeedRun run in wave.needs)
                if (run.Backlog > 0 || run.Waiting > 0 || run.Seated > 0) return false;
            return true;
        }

        // WAVE_RESOLVING sa vyvola prave raz, dalsi Step uz v stave WaveActive nebezi.
        private void ResolveWave()
        {
            if (Phase != WavePhase.WaveActive) return;
            SetPhase(WavePhase.WaveResolving);
            State.result = WaveResultBuilder.Build(State, Config);
            State.rewardClaimed = false;
            Raise(new ResortEvent(ResortEventType.WaveResolved, value: State.wave.waveNumber));
            SetPhase(WavePhase.WaveComplete);
        }

        private void UpdateStatuses(float dt)
        {
            WaveRun wave = State.wave;
            foreach (NeedRun run in wave.needs)
            {
                NeedStatus before = run.status;
                bool staffed = WorkerRoster.IsStaffed(State, run.Building);
                NeedStatus after = BottleneckSystem.Update(run, Config, dt, staffed, wave.arrivalClosed);
                if (after != before) RaiseStatus(run, after);
            }
        }

        private void RaiseStatus(NeedRun run, NeedStatus status)
        {
            ResortEventType type = status == NeedStatus.Bottleneck ? ResortEventType.BottleneckStarted
                : status == NeedStatus.Recovering ? ResortEventType.Recovering
                : status == NeedStatus.Satisfied ? ResortEventType.NeedSatisfied
                : ResortEventType.NeedOk;
            Raise(new ResortEvent(type, run.need, run.Building, run.Waiting));
        }

        private void CloseArrival()
        {
            WaveRun wave = State.wave;
            wave.arrivalClosed = true;
            foreach (NeedRun run in wave.needs)
            {
                int flushed = ArrivalSystem.Flush(run);
                int capacity = CapacityOf(run.Building);
                for (int i = 0; i < flushed; i++) ServiceSystem.Arrive(run, wave.time, capacity, Config);
            }
            Raise(new ResortEvent(ResortEventType.ArrivalClosed, value: wave.waveNumber));
        }

        // Interne meranie; cislo sa hracovi ukaze az na S4 (PRIH-144).
        public float CurrentSatisfaction =>
            State.wave == null ? 1f : SatisfactionRules.WaveSatisfaction(State.wave.needs, Config);

        public WaveResult LastResult => State.result;

        public bool CanClaimRewards => Phase == WavePhase.WaveComplete && State.result != null && !State.rewardClaimed;

        // Jedna transakcia vysledku: Coins, Energy a Resort Value sa pripisu prave raz.
        // Opakovany klik ani znovuotvorenie vysledku odmenu nezopakuje.
        public bool ClaimRewards()
        {
            if (!CanClaimRewards) return false;
            WaveResult result = State.result;
            State.coins += result.coins;
            State.energy += result.energy;
            State.resortValue += result.resortValueGain;
            State.rewardClaimed = true;
            State.completedWaves = Math.Max(State.completedWaves, result.waveNumber);
            Raise(new ResortEvent(ResortEventType.RewardsClaimed, value: result.coins));
            SetPhase(WavePhase.Upgrade);
            return true;
        }

        public bool AllGuestsArrived => State.wave != null && State.wave.arrivalClosed;

        public bool CanMoveWorker => WavePhaseRules.CanMoveWorker(Phase);

        // Presun Workera zo zdrojovej budovy na jedinu prazdnu. Rozbehnute obsluhy sa neresetuju -
        // rychlost oboch budov sa zmeni od dalsieho kroku (cielova zrychli, zdrojova spomali).
        public bool MoveWorkerFrom(BuildingId source)
        {
            if (!CanMoveWorker) return false;
            if (!WorkerRoster.TryMoveFrom(State, source, out BuildingId target)) return false;
            Raise(new ResortEvent(ResortEventType.WorkerMoved, ResortCatalog.NeedOf(target), target,
                (int)source));

            // AC2: presun na budovu v BOTTLENECK zmeni warning na RECOVERING v tej istej interakcii.
            // Je to ten isty stavovy mechanizmus (PRIH-20), do OK prejde az ked rad klesne pod prah.
            NeedRun run = State.wave != null && WavePhaseRules.IsWaveRunning(Phase)
                ? State.wave.Find(ResortCatalog.NeedOf(target)) : null;
            if (run != null && run.status == NeedStatus.Bottleneck)
            {
                BottleneckSystem.Enter(run, NeedStatus.Recovering, run.Waiting);
                RaiseStatus(run, NeedStatus.Recovering);
            }
            Changed?.Invoke();
            return true;
        }

        // S5 Later alebo dokonceny upgrade -> S1 v AWAITING_WAVE. Dalsiu vlnu spusti az Start Wave.
        public bool FinishUpgradeStep()
        {
            if (Phase != WavePhase.Upgrade) return false;
            State.wave = null;
            SetPhase(WavePhase.AwaitingWave);
            return true;
        }

        public bool IsMaxLevel(BuildingId building)
        {
            BuildingState state = State.Building(building);
            return state == null || state.level >= Config.MaxLevel;
        }

        // Cena dalsieho levelu; -1 ak je budova na maxime alebo nie je kupena.
        public int UpgradeCost(BuildingId building)
        {
            BuildingState state = State.Building(building);
            if (state == null || !state.owned || IsMaxLevel(building)) return -1;
            return Config.upgradeCost[Math.Min(state.level - 1, Config.upgradeCost.Length - 1)];
        }

        public int UpgradeResortValue(BuildingId building)
        {
            BuildingState state = State.Building(building);
            if (state == null) return 0;
            return Config.upgradeResortValue[Math.Min(state.level - 1, Config.upgradeResortValue.Length - 1)];
        }

        // Upgrade prebieha VYHRADNE medzi vlnami (WR6, Build §5.3.7) a stoji Coins.
        public bool CanUpgrade(BuildingId building)
        {
            int cost = UpgradeCost(building);
            return WavePhaseRules.CanUpgrade(Phase) && cost >= 0 && State.coins >= cost;
        }

        public bool Upgrade(BuildingId building)
        {
            if (!CanUpgrade(building)) return false;
            int cost = UpgradeCost(building);
            int gain = UpgradeResortValue(building);
            BuildingState state = State.Building(building);
            State.coins -= cost;
            state.level++;
            State.resortValue += gain;
            Raise(new ResortEvent(ResortEventType.BuildingUpgraded, ResortCatalog.NeedOf(building), building, state.level));
            Changed?.Invoke();
            return true;
        }

        // ---- Oblasti (Pool, Bungalows) ----

        public int GateWave(BuildingId area) =>
            area == BuildingId.Pool ? Config.poolGateWave : area == BuildingId.Bungalows ? Config.bungalowsGateWave : 0;

        public int UnlockCost(BuildingId area) =>
            area == BuildingId.Pool ? Config.poolCost : area == BuildingId.Bungalows ? Config.bungalowsCost : 0;

        public int UnlockResortValue(BuildingId area) =>
            area == BuildingId.Pool ? Config.poolResortValue : area == BuildingId.Bungalows ? Config.bungalowsResortValue : 0;

        // Locked = gate nesplneny; Available = spristupnene, ale nekupene; Unlocked = kupene.
        public AreaState AreaStateOf(BuildingId area)
        {
            if (State.Owns(area)) return AreaState.Unlocked;
            return State.completedWaves >= GateWave(area) ? AreaState.Available : AreaState.Locked;
        }

        // Oblast, ktoru este treba ponuknut cez S9 po S5 (kazda len raz automaticky).
        public BuildingId? PendingAreaOffer
        {
            get
            {
                foreach (BuildingId area in new[] { BuildingId.Pool, BuildingId.Bungalows })
                    if (AreaStateOf(area) == AreaState.Available && !State.offeredAreas.Contains(area)) return area;
                return null;
            }
        }

        public bool CanUnlock(BuildingId area)
        {
            return WavePhaseRules.CanUpgrade(Phase) && AreaStateOf(area) == AreaState.Available &&
                   State.coins >= UnlockCost(area);
        }

        // Kupa oblasti: budova sa stane aktivnou, jej need sa od dalsej vlny spawnuje (WR9 uz neblokuje).
        public bool Unlock(BuildingId area)
        {
            if (!CanUnlock(area)) return false;
            BuildingState state = State.Building(area);
            State.coins -= UnlockCost(area);
            state.owned = true;
            state.level = 1;
            // Worker prichadza v balíku s budovou a stoji rovno na nej (deficit ostava 1).
            state.staffed = true;
            State.resortValue += UnlockResortValue(area);
            MarkAreaOffered(area);
            Raise(new ResortEvent(ResortEventType.AreaUnlocked, ResortCatalog.NeedOf(area), area));
            Changed?.Invoke();
            return true;
        }

        public void MarkAreaOffered(BuildingId area)
        {
            if (!State.offeredAreas.Contains(area)) State.offeredAreas.Add(area);
        }

        public int CapacityOf(BuildingId building)
        {
            BuildingState state = State.Building(building);
            return Config.CapacityForLevel(state != null ? state.level : 1);
        }

        // Budova s Workerom bezi naplno (1.0), bez neho spomalene (0.33), nikdy nie na nule (WR2).
        public float SpeedOf(NeedRun run)
        {
            return ServiceRules.Speed(Config, WorkerRoster.IsStaffed(State, run.Building));
        }

        // Forecast v S1 cita rovnaku konfiguraciu ako generator demandu. Sudden rush v nom nie je.
        public List<NeedRun> Forecast()
        {
            return CreateNeeds(NextWaveNumber);
        }

        public int ForecastGuests => Config.WaveAt(NextWaveNumber).guests;

        // Need nekupenej budovy sa nespawnuje (WR9).
        public List<NeedRun> CreateNeeds(int waveNumber)
        {
            var needs = new List<NeedRun>();
            WaveDefinition definition = Config.WaveAt(waveNumber);
            foreach (NeedId need in ResortCatalog.AllNeeds)
            {
                if (!State.Owns(ResortCatalog.BuildingOf(need))) continue;
                int demand = definition.Demand(need);
                if (demand > 0) needs.Add(new NeedRun(need, demand));
            }
            return needs;
        }

        private WaveRun CreateWave(int waveNumber)
        {
            return new WaveRun
            {
                waveNumber = waveNumber,
                guests = Config.WaveAt(waveNumber).guests,
                needs = CreateNeeds(waveNumber)
            };
        }

        private void SetPhase(WavePhase phase)
        {
            if (State.phase == phase) return;
            State.phase = phase;
            Raise(new ResortEvent(ResortEventType.PhaseChanged, value: (int)phase));
            Changed?.Invoke();
        }

        private void Raise(ResortEvent e)
        {
            EventRaised?.Invoke(e);
        }
    }
}
