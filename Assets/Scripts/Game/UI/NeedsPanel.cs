using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S2 Wave Needs Panel, manazersky dashboard, nie quest list (UI handoff S2).
    // Bocny panel vedla ostrova: unesie 3, 4 aj 5 needov bez preskupenia layoutu.
    public sealed class NeedsPanel : GameView
    {
        public const float RowHeight = 64f;

        private sealed class Row
        {
            public NeedId need;
            public Image background;
            public Text name;
            public Text count;
            public Image barFill;
            public Text status;
            public Image workerBadge;
        }

        private Image panel;
        private Text title;
        private RectTransform rowsRoot;
        private readonly List<Row> rows = new List<Row>();

        protected override void Build()
        {
            UiKit.Stretch((RectTransform)transform);
            panel = UiKit.Panel(transform, "NeedsPanel");
            title = UiKit.Label(panel.transform, "Guest Needs", 22, UiKit.Ink, TextAnchor.MiddleLeft);
            UiKit.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -8f), new Vector2(280f, 34f));

            rowsRoot = UiKit.Rect("Rows", panel.transform);
            UiKit.Place(rowsRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -46f), new Vector2(290f, 5 * (RowHeight + 6f)));
            var layout = rowsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
        }

        protected override void OnSessionChanged()
        {
            if (panel == null) return;
            bool visible = Session.Wave != null &&
                           (WavePhaseRules.IsWaveRunning(Session.Phase) || Session.Phase == WavePhase.WavePaused);
            panel.gameObject.SetActive(visible);
            if (!visible) return;
            EnsureRows();
            Refresh();
        }

        // Panel cita filled/demand_total, nie len pocet cakajucich. Cakajuci a obsadene miesta su
        // samostatne udaje. Sudden rush zvysi total, dokoncena obsluha filled.
        public static string CountText(NeedRun run)
        {
            return run.filled + "/" + run.DemandTotal;
        }

        public static float Progress(NeedRun run)
        {
            return run.DemandTotal > 0 ? Mathf.Clamp01((float)run.filled / run.DemandTotal) : 1f;
        }

        public static string WaitingText(NeedRun run, int capacity)
        {
            return run.Waiting + " waiting  ·  " + run.Seated + "/" + capacity + " seats";
        }

        private void Refresh()
        {
            foreach (Row row in rows)
            {
                NeedRun run = Session.Wave.Find(row.need);
                if (run == null) continue;
                row.count.text = CountText(run);
                row.barFill.rectTransform.anchorMax = new Vector2(Progress(run), 1f);
                row.status.text = WaitingText(run, Session.CapacityOf(run.Building));
            }
        }

        // Riadky sa vytvoria podla needov vlny; novy need sa prida na koniec, ostatne ostanu na mieste.
        private void EnsureRows()
        {
            List<NeedRun> needs = Session.Wave.needs;
            bool same = rows.Count == needs.Count;
            for (int i = 0; same && i < needs.Count; i++) same = rows[i].need == needs[i].need;
            if (same) return;

            foreach (Row row in rows) Destroy(row.background.gameObject);
            rows.Clear();
            foreach (NeedRun need in needs) rows.Add(CreateRow(need.need));
            NeedVisuals.PlaceLeftPanel(panel.rectTransform, 92f + needs.Count * (RowHeight + 6f));
        }

        private Row CreateRow(NeedId need)
        {
            var row = new Row { need = need };
            row.background = UiKit.Panel(rowsRoot, "Need_" + need, "ui_panel", 3f);
            row.background.rectTransform.sizeDelta = new Vector2(290f, RowHeight);
            BuildingId building = ResortCatalog.BuildingOf(need);
            var button = row.background.gameObject.AddComponent<Button>();
            button.onClick.AddListener(() => Game.RequestWorkerAllocation(building));

            // Ocean View je premium need: nizky objem, vyssia vaha, vizualne odliseny zlatym okrajom.
            if (need == NeedId.OceanView)
            {
                var outline = row.background.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color32(255, 196, 40, 255);
                outline.effectDistance = new Vector2(3f, -3f);
            }

            Image icon = UiKit.Icon(row.background.transform, NeedVisuals.Icon(need), 46f);
            UiKit.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(46f, 46f));

            row.name = UiKit.Label(row.background.transform, ResortCatalog.NeedName(need), 19, UiKit.Ink);
            UiKit.Place(row.name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(62f, -6f), new Vector2(130f, 24f));
            row.count = UiKit.Label(row.background.transform, "", 21, UiKit.Ink, TextAnchor.MiddleRight);
            UiKit.Place(row.count.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -6f), new Vector2(90f, 24f));

            Image bar = UiKit.Fill(row.background.transform, "Bar", new Color32(230, 216, 196, 255));
            bar.raycastTarget = false;
            UiKit.Place(bar.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(62f, 26f), new Vector2(214f, 8f));
            row.barFill = UiKit.Fill(bar.transform, "Fill", new Color32(80, 190, 110, 255));
            row.barFill.raycastTarget = false;
            row.barFill.rectTransform.anchorMin = Vector2.zero;
            row.barFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            row.barFill.rectTransform.pivot = new Vector2(0f, 0.5f);
            row.barFill.rectTransform.offsetMin = Vector2.zero;
            row.barFill.rectTransform.offsetMax = Vector2.zero;

            row.status = UiKit.Label(row.background.transform, "", 15, UiKit.Muted, TextAnchor.MiddleLeft, FontStyle.Normal);
            row.status.supportRichText = true;
            UiKit.Place(row.status.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(62f, 4f), new Vector2(214f, 20f));
            return row;
        }
    }
}
