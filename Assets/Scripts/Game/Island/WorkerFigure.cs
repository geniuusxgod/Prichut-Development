using UnityEngine;

namespace Prichut.Resort.Game
{
    // Anonymny Worker pri budove (PRIH-25/131): bez mena, rarity ci Chef/Bartender postavicky.
    // Ked budova Workera nema, postavicka tam nie je, budova vyzera skutocne neobsadena.
    public sealed class WorkerFigure : MonoBehaviour
    {
        private BuildingView building;
        private SpriteRenderer figure;

        public void Init(BuildingView view)
        {
            building = view;
            var go = new GameObject("Worker");
            go.transform.SetParent(transform, false);
            figure = go.AddComponent<SpriteRenderer>();
            figure.sprite = UiKit.Sprite("worker_figure");
            figure.sortingOrder = 7;
        }

        private void LateUpdate()
        {
            if (building == null || building.Game == null) return;
            ResortSession session = building.Game.Session;
            bool staffed = WorkerRoster.IsStaffed(session.State, building.Id);
            figure.enabled = staffed;
            if (!staffed) return;
            // Animacia len zobrazuje priradenie a rytmus prace, nikdy neriadi dokoncenie sluzby.
            NeedRun run = session.Wave != null ? session.Wave.Find(ResortCatalog.NeedOf(building.Id)) : null;
            bool working = run != null && run.Seated > 0 && session.Phase == WavePhase.WaveActive;
            float tempo = working ? 6f + 6f * session.SpeedOf(run) * session.Config.serviceTime : 1.5f;
            phase += Time.deltaTime * tempo;
            float hop = working ? Mathf.Abs(Mathf.Sin(phase)) * 0.08f : Mathf.Sin(phase) * 0.015f;
            figure.transform.position = building.BasePoint + new Vector3(0.75f, -0.15f + hop, 0f);
            figure.flipX = working && Mathf.Sin(phase * 0.5f) > 0f;
        }

        private float phase;
    }
}
