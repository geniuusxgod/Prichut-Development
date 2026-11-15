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
            workerLabel.text = staffed ? "" : "No worker";
        }
    }
}
