using UnityEngine;
using Screen = UnityEngine.Device.Screen;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public sealed class HudSafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;

    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        Apply(Screen.safeArea, Screen.width, Screen.height);
    }

    private void LateUpdate()
    {
        if (Screen.safeArea != lastSafeArea ||
            lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            Apply(Screen.safeArea, Screen.width, Screen.height);
        }
    }

    public static Rect Normalize(Rect safeArea, int width, int height)
    {
        if (width <= 0 || height <= 0) return new Rect(0f, 0f, 1f, 1f);
        Vector2 min = new Vector2(Mathf.Clamp01(safeArea.xMin / width), Mathf.Clamp01(safeArea.yMin / height));
        Vector2 max = new Vector2(Mathf.Clamp01(safeArea.xMax / width), Mathf.Clamp01(safeArea.yMax / height));
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void Apply(Rect safeArea, int width, int height)
    {
        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(width, height);
        Rect anchors = Normalize(safeArea, width, height);
        rectTransform.anchorMin = anchors.min;
        rectTransform.anchorMax = anchors.max;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
