using UnityEngine;

namespace Prichut.Resort.Game
{
    // Vizualne urovne budovy (PRIH-49): vyssi level = vacsia budova a zlaty lem.
    // Rozdiel musi byt viditelny z odstupu na mobile; pocet miest ukazuje stitok budovy.
    public sealed class BuildingLevelVisual : MonoBehaviour
    {
        private static readonly float[] Scales = { 1f, 1.07f, 1.14f };
        private BuildingView view;
        private Vector3 baseScale;
        private SpriteRenderer glow;
        private int shownLevel;

        public void Init(BuildingView building)
        {
            view = building;
            baseScale = building.transform.localScale;
            var go = new GameObject("LevelGlow");
            go.transform.SetParent(building.transform, false);
            glow = go.AddComponent<SpriteRenderer>();
            glow.sprite = building.Renderer.sprite;
            glow.sortingOrder = building.Renderer.sortingOrder - 1;
            glow.transform.localScale = Vector3.one * 1.04f;
            glow.enabled = false;
        }

        public static float ScaleFor(int level)
        {
            return Scales[Mathf.Clamp(level - 1, 0, Scales.Length - 1)];
        }

        private void LateUpdate()
        {
            if (view == null || view.Game == null) return;
            BuildingState state = view.Game.Session.State.Building(view.Id);
            int level = state != null ? state.level : 1;
            if (level == shownLevel) return;
            shownLevel = level;
            view.transform.localScale = baseScale * ScaleFor(level);
            view.SendMessage("RebaseScale", SendMessageOptions.DontRequireReceiver);
            glow.enabled = level > 1;
            glow.color = level >= 3 ? new Color(1f, 0.82f, 0.25f, 0.9f) : new Color(1f, 0.95f, 0.7f, 0.7f);
        }
    }
}
