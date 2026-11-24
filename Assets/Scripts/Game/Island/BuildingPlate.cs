using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Stitok pod budovou (overlay UI, sleduje budovu na obrazovke). Dva odlisne udaje:
    // Worker indikator (je/nie je obsluha) a hostovske miesta (capacity); worker_slots nie je capacity.
    public sealed class BuildingPlate : MonoBehaviour
    {
        private BuildingView building;
        private RectTransform root;
        private RectTransform canvasRect;
        private Image workerBadge;
        private Text workerLabel;
        private RectTransform seatsRow;
        private readonly System.Collections.Generic.List<Image> seats = new System.Collections.Generic.List<Image>();

        public RectTransform Root => root;
        public BuildingView Building => building;

        public void Init(BuildingView view, RectTransform overlay)
        {
            building = view;
            canvasRect = overlay;
            root = UiKit.Rect("Plate_" + view.Id, overlay);
            root.sizeDelta = new Vector2(220f, 46f);
            root.anchorMin = root.anchorMax = Vector2.zero;
            root.pivot = new Vector2(0.5f, 1f);

            workerBadge = UiKit.Icon(root, "worker_badge", 40f);
            UiKit.Place(workerBadge.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(40f, 40f));
            workerLabel = UiKit.Label(root, "", 15, UiKit.White, TextAnchor.MiddleLeft);
            UiKit.Outline(workerLabel, new Color(0.2f, 0.1f, 0.05f, 0.85f), 1.2f);
            UiKit.Place(workerLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-6f, -2f), new Vector2(120f, 18f));

            // Hostovske miesta (capacity) su samostatny riadok bodiek, nie su to Workeri.
            Image seatsBackground = UiKit.Panel(root, "Seats", "ui_dark", 4f);
            seatsBackground.raycastTarget = false;
            seatsRow = seatsBackground.rectTransform;
            UiKit.Place(seatsRow, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(40f, 26f));
            var layout = seatsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 4, 4);
            layout.spacing = 2f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childAlignment = TextAnchor.MiddleLeft;
        }

        private void RefreshSeats(int capacity, int occupied)
        {
            while (seats.Count < capacity) seats.Add(UiKit.Icon(seatsRow, "seat_free", 16f));
            for (int i = 0; i < seats.Count; i++)
            {
                seats[i].gameObject.SetActive(i < capacity);
                seats[i].sprite = UiKit.Sprite(i < occupied ? "seat_taken" : "seat_free");
            }
            seatsRow.sizeDelta = new Vector2(12f + capacity * 18f, 26f);
        }

        private void LateUpdate()
        {
            if (building == null || building.Game == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector2 screen = cam.WorldToScreenPoint(building.BasePoint);
            // Lokalny bod je relativny k stredu safe area; kotva stitku je v jej lavom dolnom rohu.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, null, out Vector2 local))
                root.anchoredPosition = local + canvasRect.rect.size * 0.5f;
            Refresh();
        }

        // Obsadena a prazdna budova sa daju odlisit bez hadania; po presune sa obnovia hned.
        private void Refresh()
        {
            ResortSession session = building.Game.Session;
            bool staffed = WorkerRoster.IsStaffed(session.State, building.Id);
            workerBadge.sprite = UiKit.Sprite(staffed ? "worker_badge" : "worker_empty");
            BuildingState level = session.State.Building(building.Id);
            workerLabel.text = (staffed ? "" : "No worker  ") + (level != null && level.level > 1 ? "Lv " + level.level : "");

            NeedRun run = session.Wave != null ? session.Wave.Find(ResortCatalog.NeedOf(building.Id)) : null;
            bool running = run != null && (WavePhaseRules.IsWaveRunning(session.Phase) || session.Phase == WavePhase.WavePaused);
            RefreshSeats(session.CapacityOf(building.Id), running ? run.Seated : 0);
        }
    }
}
