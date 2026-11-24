#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Vytvori iba HUD; existujuce objekty ostrova nemeni.
public static class HudSceneBuilder
{
    [MenuItem("Resort Empire/Add HUD to Main Scene")]
    public static void Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/Main.unity" || scene.isDirty)
            throw new InvalidOperationException("Open the saved Main scene outside Play Mode first.");
        if (UnityEngine.Object.FindAnyObjectByType<HudPresenter>(FindObjectsInactive.Include) != null)
            throw new InvalidOperationException("A HUD already exists. Existing HUD was preserved.");

        Sprite coins = ImportIcon("dollar");
        Sprite energy = ImportIcon("lightning");
        Sprite resort = ImportIcon("star");
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) throw new InvalidOperationException("Built-in HUD font not found.");

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add PRIH-12 HUD");
        var stateObject = new GameObject("GameState", typeof(GameState));
        Undo.RegisterCreatedObjectUndo(stateObject, "Add GameState");
        var canvasObject = new GameObject("HudCanvas", typeof(RectTransform));
        canvasObject.SetActive(false);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Add HUD");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 590f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        var safeArea = Rect("SafeArea", canvasObject.transform);
        Stretch(safeArea);
        safeArea.gameObject.AddComponent<HudSafeArea>();
        var bar = Rect("TopBar", safeArea);
        bar.anchorMin = new Vector2(0f, 1f);
        bar.anchorMax = Vector2.one;
        bar.pivot = new Vector2(0.5f, 1f);
        bar.offsetMin = new Vector2(24f, -70f);
        bar.offsetMax = new Vector2(-24f, -24f);

        Text resortText = Card(bar, "ResortValue", "Resort Value: 700", resort,
            new Color32(255, 183, 0, 255), font, false, 0f, 250f);
        Text coinsText = Card(bar, "Coins", "12,450", coins,
            new Color32(0, 186, 25, 255), font, true, 150f, 138f);
        Text energyText = Card(bar, "Energy", "45/50", energy,
            new Color32(0, 179, 216, 255), font, true, 0f, 130f);
        var presenter = canvasObject.AddComponent<HudPresenter>();
        var serialized = new SerializedObject(presenter);
        serialized.FindProperty("gameState").objectReferenceValue = stateObject.GetComponent<GameState>();
        serialized.FindProperty("coinsText").objectReferenceValue = coinsText;
        serialized.FindProperty("energyText").objectReferenceValue = energyText;
        serialized.FindProperty("resortValueText").objectReferenceValue = resortText;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        canvasObject.SetActive(true);
        Canvas.ForceUpdateCanvases();
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("PRIH-12 HUD added to Main. Values are owned by the GameState object.");
    }

    private static Sprite ImportIcon(string name)
    {
        string path = "Assets/Art/UI/Hud/" + name + ".png";
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing HUD icon: " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        // The source icons are black alpha masks; import white RGB so UI tint can color them.
        importer.swizzleR = TextureImporterSwizzle.One;
        importer.swizzleG = TextureImporterSwizzle.One;
        importer.swizzleB = TextureImporterSwizzle.One;
        importer.SaveAndReimport();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) throw new InvalidOperationException("Cannot load HUD sprite: " + path);
        return sprite;
    }

    private static Text Card(Transform parent, string title, string initialValue, Sprite icon,
        Color accent, Font font, bool alignRight, float inset, float width)
    {
        var card = Rect(title + "Card", parent);
        card.anchorMin = new Vector2(alignRight ? 1f : 0f, 0.5f);
        card.anchorMax = card.anchorMin;
        card.pivot = new Vector2(alignRight ? 1f : 0f, 0.5f);
        card.anchoredPosition = new Vector2(alignRight ? -inset : inset, 0f);
        card.sizeDelta = new Vector2(width, 46f);
        var background = card.gameObject.AddComponent<RoundedHudPanel>();
        background.color = Color.white;
        background.raycastTarget = false;
        var iconRect = Rect("Icon", card);
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = iconRect.anchorMin;
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(14f, 0f);
        iconRect.sizeDelta = new Vector2(28f, 28f);
        var image = iconRect.gameObject.AddComponent<Image>();
        image.sprite = icon;
        image.color = accent;
        image.preserveAspect = true;
        image.raycastTarget = false;
        var value = Label("Value", card, initialValue, font, 21, new Color32(70, 36, 16, 255));
        value.fontStyle = FontStyle.Bold;
        Stretch(value.rectTransform);
        value.rectTransform.offsetMin = new Vector2(52f, 4f);
        value.rectTransform.offsetMax = new Vector2(-12f, -4f);
        return value;
    }

    private static Text Label(string name, Transform parent, string text, Font font, int size, Color color)
    {
        var label = Rect(name, parent).gameObject.AddComponent<Text>();
        label.font = font;
        label.text = text;
        label.color = color;
        label.fontSize = size;
        label.alignment = TextAnchor.MiddleLeft;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = size;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
