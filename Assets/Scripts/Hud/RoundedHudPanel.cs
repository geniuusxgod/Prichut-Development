using UnityEngine;
using UnityEngine.UI;

// Vector geometry keeps the pill border crisp at different Canvas scales.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class RoundedHudPanel : MaskableGraphic
{
    [SerializeField] private float cornerRadius = 18f;
    [SerializeField] private float borderWidth = 3f;
    [SerializeField] private Color borderColor = new Color32(70, 36, 16, 255);

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float radius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        AddRoundedRect(mesh, rect, radius, borderColor);
        float border = Mathf.Clamp(borderWidth, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        rect = Rect.MinMaxRect(rect.xMin + border, rect.yMin + border, rect.xMax - border, rect.yMax - border);
        AddRoundedRect(mesh, rect, Mathf.Max(0f, radius - border), color);
    }

    private static void AddRoundedRect(VertexHelper mesh, Rect rect, float radius, Color tint)
    {
        const int segments = 12;
        int center = mesh.currentVertCount;
        mesh.AddVert(new Vector3(rect.center.x, rect.center.y), tint, Vector2.zero);
        for (int corner = 0; corner < 4; corner++)
        {
            float x = corner == 0 || corner == 3 ? rect.xMax - radius : rect.xMin + radius;
            float y = corner < 2 ? rect.yMax - radius : rect.yMin + radius;
            for (int step = 0; step <= segments; step++)
            {
                float angle = (corner * 90f + step * 90f / segments) * Mathf.Deg2Rad;
                mesh.AddVert(new Vector3(x + Mathf.Cos(angle) * radius, y + Mathf.Sin(angle) * radius), tint, Vector2.zero);
            }
        }
        int count = 4 * (segments + 1);
        for (int i = 0; i < count; i++)
            mesh.AddTriangle(center, center + 1 + i, center + 1 + (i + 1) % count);
    }
}
