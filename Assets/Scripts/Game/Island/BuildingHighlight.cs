using UnityEngine;

namespace Prichut.Resort.Game
{
    // Vyber budovy: kratky poskok (squash and stretch) a teply odtien, kym je budova vybrana.
    public sealed class BuildingHighlight : MonoBehaviour
    {
        private const float BounceTime = 0.45f;
        private BuildingView view;
        private Vector3 baseScale;
        private Color baseColor;
        private bool selected;
        private float bounce = -1f;

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

        // Po zmene levelu sa nova velkost stane zakladom animacie.
        private void RebaseScale()
        {
            baseScale = transform.localScale;
        }

        private void OnSelectionChanged(BuildingId? id)
        {
            bool now = id.HasValue && id.Value == view.Id;
            if (now && !selected) bounce = 0f;
            selected = now;
            if (!selected) view.Renderer.color = baseColor;
        }

        // Tlmena pruzina: najprv sa budova prikrci, potom vyskoci a usadi sa.
        public static Vector2 BounceScale(float t)
        {
            float k = Mathf.Clamp01(t / BounceTime);
            float wave = Mathf.Sin(k * Mathf.PI * 2.5f) * (1f - k);
            return new Vector2(1f - wave * 0.08f, 1f + wave * 0.12f);
        }

        private void Update()
        {
            if (bounce >= 0f)
            {
                bounce += Time.unscaledDeltaTime;
                Vector2 s = BounceScale(bounce);
                transform.localScale = new Vector3(baseScale.x * s.x, baseScale.y * s.y, baseScale.z);
                if (bounce >= BounceTime)
                {
                    bounce = -1f;
                    transform.localScale = baseScale;
                }
            }

            if (!selected) return;
            float glow = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
            view.Renderer.color = Color.Lerp(baseColor, new Color(1f, 0.94f, 0.75f, 1f), 0.25f + 0.2f * glow);
        }
    }
}
