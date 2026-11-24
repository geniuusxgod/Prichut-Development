using UnityEngine;

namespace Prichut.Resort.Game
{
    // Zvyraznenie vybranej budovy: jemne pulzovanie velkosti a teplejsi odtien.
    public sealed class BuildingHighlight : MonoBehaviour
    {
        private BuildingView view;
        private Vector3 baseScale;
        private Color baseColor;
        private bool selected;

        public void Init(BuildingView building)
        {
            view = building;
            baseScale = transform.localScale;
            baseColor = view.Renderer.color;
            view.Game.SelectionChanged += OnSelectionChanged;
        }

        private void OnDestroy()
        {
            if (view != null && view.Game != null) view.Game.SelectionChanged -= OnSelectionChanged;
        }

        // Po zmene levelu sa nova velkost stane zakladom pulzovania.
        private void RebaseScale()
        {
            baseScale = transform.localScale;
        }

        private void OnSelectionChanged(BuildingId? id)
        {
            selected = id.HasValue && id.Value == view.Id;
            if (!selected)
            {
                transform.localScale = baseScale;
                view.Renderer.color = baseColor;
            }
        }

        private void Update()
        {
            if (!selected) return;
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5f);
            transform.localScale = baseScale * (1.02f + 0.025f * pulse);
            view.Renderer.color = Color.Lerp(baseColor, new Color(1f, 0.93f, 0.7f, 1f), 0.35f + 0.25f * pulse);
        }
    }
}
