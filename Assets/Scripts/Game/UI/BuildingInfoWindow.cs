using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Kontextove okno budovy (PRIH-15): aktualne udaje a akcie podla stavu hry.
    // Akcie volaju spolocne systemy (presun Workera PRIH-26, upgrade PRIH-38), ziadna druha implementacia.
    public sealed class BuildingInfoWindow : GameView
    {
        private Image card;
        private Text title;
        private Text details;
        private Button moveButton;
        private Button upgradeButton;
        private RectTransform actions;

        protected override void Build()
        {
            UiKit.Stretch((RectTransform)transform);
            card = UiKit.Panel(transform, "BuildingInfo");
            UiKit.Place(card.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f),
                new Vector2(520f, 150f));

            title = UiKit.Label(card.transform, "", 26, UiKit.Ink, TextAnchor.UpperLeft);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -12f), new Vector2(420f, 34f));
            details = UiKit.Label(card.transform, "", 18, UiKit.Muted, TextAnchor.UpperLeft, FontStyle.Normal);
            details.supportRichText = true;
            UiKit.Place(details.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -48f), new Vector2(480f, 50f));

            bool hasCloseIcon = UnityEngine.Resources.Load<UnityEngine.Sprite>("Game/icon_close") != null;
            Button close = UiKit.Button(card.transform, "Close",
                        hasCloseIcon ? null : "X", "ui_grey",
                        hasCloseIcon ? "icon_close" : null, () => Game.Select(null), 18);
            UiKit.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(48f, 44f));

            actions = UiKit.Rect("Actions", card.transform);
            UiKit.Place(actions, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(16f, 12f), new Vector2(488f, 52f));
            var row = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 10f;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;

            moveButton = UiKit.Button(actions, "MoveWorker", "Move Worker", "ui_blue", null, OnMoveWorker, 20);
            ((RectTransform)moveButton.transform).sizeDelta = new Vector2(190f, 50f);
            upgradeButton = UiKit.Button(actions, "Upgrade", "Upgrade", "ui_green", null, OnUpgrade, 20);
            ((RectTransform)upgradeButton.transform).sizeDelta = new Vector2(170f, 50f);

            Game.SelectionChanged += _ => OnSessionChanged();
        }

        public RectTransform Actions => actions;

        private void OnMoveWorker()
        {
            if (Game.SelectedBuilding.HasValue) Game.RequestWorkerAllocation(Game.SelectedBuilding.Value);
        }

        private void OnUpgrade()
        {
            if (Game.SelectedBuilding.HasValue) Game.RequestUpgrade(Game.SelectedBuilding.Value);
        }

        protected override void OnSessionChanged()
        {
            if (card == null) return;
            bool visible = Game.SelectedBuilding.HasValue && Session.Phase != WavePhase.WaveComplete &&
                           Session.Phase != WavePhase.Upgrade && Session.Phase != WavePhase.WavePaused;
            card.gameObject.SetActive(visible);
            if (!visible) return;

            BuildingId id = Game.SelectedBuilding.Value;
            BuildingState state = Session.State.Building(id);
            bool staffed = WorkerRoster.IsStaffed(Session.State, id);
            title.text = ResortCatalog.BuildingName(id) + "  <size=18>Lv " + state.level + "</size>";
            title.supportRichText = true;

            string worker = staffed ? "<color=#2E9650>Worker on duty - full speed</color>"
                : "<color=#B05A10>No Worker - running slowly</color>";
            string line = ResortCatalog.NeedName(ResortCatalog.NeedOf(id)) + "  ·  " + Session.CapacityOf(id) + " seats  ·  " + worker;
            NeedRun run = Session.Wave != null ? Session.Wave.Find(ResortCatalog.NeedOf(id)) : null;
            if (run != null && WavePhaseRules.IsWaveRunning(Session.Phase))
                line += "\nWaiting " + run.Waiting + "  ·  Served " + run.filled + "/" + run.DemandTotal;
            details.text = line;

            moveButton.gameObject.SetActive(Session.CanMoveWorker);
            // Pocas vlny sa upgrade neponuka, tlacidlo nie je ani zasedene, jednoducho tam nie je.
            upgradeButton.gameObject.SetActive(WavePhaseRules.CanUpgrade(Session.Phase) && !Session.IsMaxLevel(id));
        }
    }
}
