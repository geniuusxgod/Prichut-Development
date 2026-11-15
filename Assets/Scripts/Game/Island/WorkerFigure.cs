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
            figure.transform.position = building.BasePoint + new Vector3(0.75f, -0.15f, 0f);
        }
    }
}
