using System;
using UnityEngine;

namespace Prichut.Resort.Game
{
    // Jediny MonoBehaviour, ktory vlastni ResortSession. Tika simulaciu, synchronizuje HUD
    // (GameState z PRIH-12) a sklada obrazovky S1-S5/S9.
    public sealed class ResortGameController : MonoBehaviour
    {
        public ResortSession Session { get; private set; }
        public ResortConfig Config { get; private set; }
        public RectTransform GameLayer { get; private set; }
        public RectTransform ModalLayer { get; private set; }
        public IslandView Island { get; private set; }

        private GameState hudState;

        public event Action<BuildingId?> SelectionChanged;
        public event Action<BuildingId> WorkerAllocationRequested;
        public BuildingId? SelectedBuilding { get; private set; }

        private void Awake()
        {
            Config = ResortConfig.CreateDefault();
            Session = new ResortSession(Config);
            hudState = FindAnyObjectByType<GameState>();
        }

        private void Start()
        {
            GameLayer = UiKit.CreateCanvas("GameCanvas", 90);
            ModalLayer = UiKit.CreateCanvas("ModalCanvas", 120);
            Session.Changed += SyncHud;
            SyncHud();
            CreateViews();
        }

        private void OnDestroy()
        {
            if (Session != null) Session.Changed -= SyncHud;
        }

        private void Update()
        {
            Session.Tick(Time.deltaTime);
        }

        public T AddView<T>(Transform parent = null) where T : GameView
        {
            var go = new GameObject(typeof(T).Name, typeof(RectTransform));
            go.transform.SetParent(parent != null ? parent : transform, false);
            var view = go.AddComponent<T>();
            view.Bind(this);
            return view;
        }

        private void CreateViews()
        {
            Island = AddView<IslandView>();
            AddView<ForecastPanel>(GameLayer);
            AddView<StartWaveButton>(GameLayer);
            AddView<BuildingInfoWindow>(GameLayer);
        }

        // S3 sa otvara z S1 (priprava) aj z S2 (pocas vlny), jedna obrazovka, dva vstupy.
        public void RequestWorkerAllocation(BuildingId context)
        {
            if (!Session.CanMoveWorker) return;
            WorkerAllocationRequested?.Invoke(context);
        }

        public void Select(BuildingId? building)
        {
            if (SelectedBuilding == building) return;
            SelectedBuilding = building;
            SelectionChanged?.Invoke(building);
        }

        // HUD cita hodnoty z jedneho GameState; zdrojom pravdy je ResortSession.
        private void SyncHud()
        {
            if (hudState == null) return;
            hudState.SetEnergyCapacity(0);
            hudState.SetCoins(Session.State.coins);
            hudState.SetEnergy(Session.State.energy);
            hudState.SetResortValue(Session.State.resortValue);
        }
    }
}
