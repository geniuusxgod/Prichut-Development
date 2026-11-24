using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S1 forecast (PRIH-14): typy a objemy nasledujucej vlny, aby hrac rozmiestnil Workerov pred startom.
    public sealed class ForecastPanel : GameView
    {
        private Image panel;
        private Text title;
        private RectTransform rows;
        private readonly List<GameObject> rowObjects = new List<GameObject>();

        protected override void Build()
        {
            UiKit.Stretch((RectTransform)transform);
            panel = UiKit.Panel(transform, "Forecast");
            title = UiKit.Label(panel.transform, "", 22, UiKit.Ink, TextAnchor.UpperLeft);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f),
                new Vector2(280f, 56f));

            rows = UiKit.Rect("Rows", panel.transform);
            UiKit.Place(rows, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -72f), new Vector2(286f, 300f));
            var layout = rows.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
        }

        protected override void OnSessionChanged()
        {
            if (panel == null) return;
            bool visible = Session.Phase == WavePhase.AwaitingWave;
            panel.gameObject.SetActive(visible);
            if (!visible) return;

            List<NeedRun> forecast = Session.Forecast();
            title.text = "Next wave  ·  Wave " + Session.NextWaveNumber + "\n<size=17>" +
                         Session.ForecastGuests + " guests are on the way</size>";
            title.supportRichText = true;
            NeedVisuals.PlaceLeftPanel(panel.rectTransform, 92f + forecast.Count * 56f);

            foreach (GameObject row in rowObjects) Destroy(row);
            rowObjects.Clear();
            foreach (NeedRun need in forecast) rowObjects.Add(CreateRow(need));
        }

        private GameObject CreateRow(NeedRun need)
        {
            BuildingId building = need.Building;
            Image row = UiKit.Panel(rows, "Row_" + need.need, "ui_panel", 3f);
            row.rectTransform.sizeDelta = new Vector2(286f, 50f);
            var button = row.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => Game.RequestWorkerAllocation(building));

            Image icon = UiKit.Icon(row.transform, NeedVisuals.Icon(need.need), 40f);
            UiKit.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(40f, 40f));
            Text name = UiKit.Label(row.transform, ResortCatalog.NeedName(need.need), 20, UiKit.Ink);
            UiKit.Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(56f, 0f), new Vector2(150f, 40f));
            Text volume = UiKit.Label(row.transform, need.DemandTotal.ToString(), 24, UiKit.Ink, TextAnchor.MiddleRight);
            UiKit.Place(volume.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(70f, 40f));
            return row.gameObject;
        }
    }
}
