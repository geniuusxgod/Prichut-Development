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
                    for (int i = 0; i < arrived; i++) ServiceSystem.Arrive(run, wave.time, capacity);
                    if (arrived > 0) Raise(new ResortEvent(ResortEventType.UnitsArrived, run.need, run.Building, arrived));
                }

                int served = ServiceSystem.Tick(run, wave.time, dt, SpeedOf(run), capacity);
                if (served > 0) Raise(new ResortEvent(ResortEventType.UnitsServed, run.need, run.Building, served));
            }

            // wave_timer zatvara PRICHOD, nie vlnu. Potom budovy dobiehaju rady (dobeh).
            if (!wave.arrivalClosed && wave.time >= Config.waveTimer) CloseArrival();
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
            Raise(new ResortEvent(ResortEventType.WaveResolved, value: State.wave.waveNumber));
            SetPhase(WavePhase.WaveComplete);
        }

        private void CloseArrival()
        {
            WaveRun wave = State.wave;
            wave.arrivalClosed = true;
            foreach (NeedRun run in wave.needs)
            {
                int flushed = ArrivalSystem.Flush(run);
                int capacity = CapacityOf(run.Building);
                for (int i = 0; i < flushed; i++) ServiceSystem.Arrive(run, wave.time, capacity);
            }
            Raise(new ResortEvent(ResortEventType.ArrivalClosed, value: wave.waveNumber));
        }

        public bool AllGuestsArrived => State.wave != null && State.wave.arrivalClosed;

        public int CapacityOf(BuildingId building)
        {
            BuildingState state = State.Building(building);
            return Config.CapacityForLevel(state != null ? state.level : 1);
        }

        public float SpeedOf(NeedRun run)
        {
            return ServiceRules.Speed(Config, true);
        }

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
