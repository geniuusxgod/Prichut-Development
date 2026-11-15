using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S3 Worker Allocation (PRIH-26): hrac zvoli obsadenu zdrojovu budovu, jej Worker prejde na jedinu
    // prazdnu. Jedno tuknutie, zadarmo, bez potvrdenia. Ukazuje aj cenu presunu, co ostane bez obsluhy.
    public sealed class WorkerAllocationSheet : ModalView
    {
        private Text title;
        private Text subtitle;
        private RectTransform cardsRoot;
        private readonly List<GameObject> cards = new List<GameObject>();
        private BuildingId context;

        protected override void Build()
        {
            BuildModal(new Vector2(900f, 300f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), true);
            title = UiKit.Label(Card.transform, "Move a Worker", 28, UiKit.Ink, TextAnchor.UpperLeft);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(700f, 36f));
            subtitle = UiKit.Label(Card.transform, "", 18, UiKit.Muted, TextAnchor.UpperLeft, FontStyle.Normal);
            subtitle.supportRichText = true;
            UiKit.Place(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -52f), new Vector2(780f, 26f));

            Button close = UiKit.Button(Card.transform, "Close", null, "ui_grey", "icon_close", Close, 18);
            UiKit.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -12f), new Vector2(52f, 46f));

            cardsRoot = UiKit.Rect("Buildings", Card.transform);
            UiKit.Place(cardsRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(860f, 190f));
            var row = cardsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 10f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;

            Game.WorkerAllocationRequested += Open;
        }

        protected override void OnDestroy()
        {
            if (Game != null) Game.WorkerAllocationRequested -= Open;
            base.OnDestroy();
        }

        public void Open(BuildingId building)
        {
            context = building;
            SetOpen(true);
            Rebuild();
        }

        protected override void OnSessionChanged()
        {
            if (!IsOpen) return;
            if (!Session.CanMoveWorker) Close();
        }

        private void Rebuild()
        {
            foreach (GameObject card in cards) Destroy(card);
            cards.Clear();

            List<BuildingId> empty = WorkerRoster.EmptyBuildings(Session.State);
            string target = empty.Count == 1 ? ResortCatalog.BuildingName(empty[0]) : "-";
            subtitle.text = "<b>" + target + "</b> has no Worker. Tap the building that gives its Worker - " +
                            "that one will run slowly instead.";

            foreach (BuildingState state in Session.State.buildings)
                if (state.owned) cards.Add(CreateCard(state.id));
        }

        private GameObject CreateCard(BuildingId id)
        {
            bool staffed = WorkerRoster.IsStaffed(Session.State, id);
            Image card = UiKit.Panel(cardsRoot, "Card_" + id, staffed ? "ui_panel" : "ui_grey", 3f);
            card.rectTransform.sizeDelta = new Vector2(160f, 180f);
            if (id == context)
            {
                var outline = card.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(60, 160, 225, 255);
                outline.effectDistance = new Vector2(4f, -4f);
            }

            Text name = UiKit.Label(card.transform, ResortCatalog.BuildingName(id), 18, UiKit.Ink, TextAnchor.UpperCenter);
            UiKit.Place(name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(150f, 24f));
            Image icon = UiKit.Icon(card.transform, NeedVisuals.Icon(ResortCatalog.NeedOf(id)), 44f);
            UiKit.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(-2f, -36f), new Vector2(44f, 44f));
            Image badge = UiKit.Icon(card.transform, staffed ? "worker_badge" : "worker_empty", 44f);
            UiKit.Place(badge.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, 1f), new Vector2(2f, -36f), new Vector2(44f, 44f));

            Text info = UiKit.Label(card.transform, CardInfo(id, staffed), 14, UiKit.Muted, TextAnchor.UpperCenter, FontStyle.Normal);
            info.supportRichText = true;
            UiKit.Place(info.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -84f), new Vector2(150f, 40f));

            if (staffed)
            {
                BuildingId source = id;
                Button give = UiKit.Button(card.transform, "Give", "Take Worker", "ui_blue", null, () => Move(source), 17);
                UiKit.Place((RectTransform)give.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(144f, 42f));
            }
            else
            {
                Text receives = UiKit.Label(card.transform, "Receives the Worker", 15, UiKit.Positive, TextAnchor.MiddleCenter);
                UiKit.Place(receives.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(150f, 30f));
            }

            return card.gameObject;
        }

        // Rozlisenie dvoch pricin toho isteho symptomu: chyba Worker (presun) vs. primala budova (upgrade).
        private string CardInfo(BuildingId id, bool staffed)
        {
            int capacity = Session.CapacityOf(id);
            NeedRun run = Session.Wave != null && Session.Phase != WavePhase.AwaitingWave
                ? Session.Wave.Find(ResortCatalog.NeedOf(id)) : null;
            string line = capacity + " seats";
            if (run != null) line += "  ·  " + run.Waiting + " waiting";
            if (run != null && run.Waiting > Session.Config.bottleneckThreshold)
                line += staffed ? "\n<color=#B05A10>Full - needs more seats</color>" : "\n<color=#B05A10>Slow - no Worker</color>";
            else
                line += staffed ? "\nFull speed" : "\nRunning slowly";
            return line;
        }

        private void Move(BuildingId source)
        {
            List<BuildingId> empty = WorkerRoster.EmptyBuildings(Session.State);
            if (!Session.MoveWorkerFrom(source)) return;
            if (empty.Count == 1 && Game.Toasts != null)
                Game.Toasts.Show("Worker moved to " + ResortCatalog.BuildingName(empty[0]), ToastKind.Info);
            Close();
        }
    }
}
