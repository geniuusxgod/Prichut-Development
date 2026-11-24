using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Prichut.Resort.Game
{
    // Prepoji objekty budov zo sceny Main (PRIH-11) s hrou. Scena sa nemeni, hladaju sa podla mena.
    public sealed class IslandView : GameView
    {
        private static readonly Dictionary<BuildingId, string> SceneNames = new Dictionary<BuildingId, string>
        {
            { BuildingId.Reception, "Reception" },
            { BuildingId.Restaurant, "Restaurant" },
            { BuildingId.BeachBar, "BeachBar" }
        };

        public readonly Dictionary<BuildingId, BuildingView> Buildings = new Dictionary<BuildingId, BuildingView>();
        private RectTransform overlay;

        protected override void Build()
        {
            Camera cam = Camera.main;
            if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null) cam.gameObject.AddComponent<Physics2DRaycaster>();
            overlay = UiKit.CreateCanvas("IslandOverlay", 80);

            foreach (KeyValuePair<BuildingId, string> pair in SceneNames)
            {
                GameObject go = GameObject.Find(pair.Value);
                if (go == null || go.GetComponent<SpriteRenderer>() == null)
                {
                    Debug.LogWarning("Building object '" + pair.Value + "' not found in the scene.");
                    continue;
                }
                Register(pair.Key, go);
            }
        }

        public BuildingView Register(BuildingId id, GameObject go)
        {
            BuildingView view = go.GetComponent<BuildingView>();
            if (view == null) view = go.AddComponent<BuildingView>();
            view.Init(Game, id);
            if (go.GetComponent<BuildingHighlight>() == null) go.AddComponent<BuildingHighlight>().Init(view);
            if (go.GetComponent<BuildingLevelVisual>() == null) go.AddComponent<BuildingLevelVisual>().Init(view);
            var workerHolder = new GameObject("WorkerFigure_" + id);
            workerHolder.transform.SetParent(transform, false);
            workerHolder.AddComponent<WorkerFigure>().Init(view);
            workerHolder.AddComponent<BuildingPlate>().Init(view, overlay);
            Buildings[id] = view;
            return view;
        }
    }
}
