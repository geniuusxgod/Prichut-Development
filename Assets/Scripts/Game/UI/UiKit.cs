using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Runtime uGUI tovaren: vsetky obrazovky sa skladaju v kode, aby sa scena Main.unity
    // nemusela pri kazdej ulohe menit (ziadne merge konflikty v scene).
    public static class UiKit
    {
        public static readonly Color Ink = new Color32(70, 36, 16, 255);
        public static readonly Color Cream = new Color32(255, 246, 228, 255);
        public static readonly Color Muted = new Color32(120, 95, 75, 255);
        public static readonly Color Positive = new Color32(46, 150, 80, 255);
        public static readonly Color Warning = new Color32(214, 120, 20, 255);
        public static readonly Color White = Color.white;
        public static readonly Color Dim = new Color(0.05f, 0.1f, 0.15f, 0.55f);

        public static readonly Vector2 ReferenceResolution = new Vector2(1280f, 590f);

        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        private static Font font;
        private static Font bodyFont;

        // Fredoka (SIL Open Font License, Art/Resources/Fonts): okruhle pismo, sedi k ostrovu.
        // SemiBold pre nadpisy, cisla a tlacidla, Medium pre beznu vetu.
        public static Font Font
        {
            get
            {
                if (font == null) font = Resources.Load<Font>("Fonts/Fredoka-SemiBold");
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static Font BodyFont
        {
            get
            {
                if (bodyFont == null) bodyFont = Resources.Load<Font>("Fonts/Fredoka-Medium");
                return bodyFont != null ? bodyFont : Font;
            }
        }

        public static Sprite Sprite(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (Sprites.TryGetValue(name, out Sprite cached) && cached != null) return cached;
            Sprite sprite = Resources.Load<Sprite>("Game/" + name);
            if (sprite == null) Debug.LogWarning("Missing sprite Resources/Game/" + name);
            Sprites[name] = sprite;
            return sprite;
        }

        // Overlay canvas s rovnakym scalerom ako HUD (PRIH-12) a safe area kontajnerom.
        public static RectTransform CreateCanvas(string name, int sortingOrder)
        {
            EnsureEventSystem();
            var go = new GameObject(name, typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;
            go.AddComponent<GraphicRaycaster>();
            RectTransform safe = Rect("SafeArea", go.transform);
            Stretch(safe);
            safe.gameObject.AddComponent<HudSafeArea>();
            return safe;
        }

        public static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            UnityEngine.Object.DontDestroyOnLoad(go);
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Stretch(RectTransform rt, float inset = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        // Ukotvenie: anchor = pivot v rodicovi, pos = posun od kotvy, size = rozmer.
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Image Panel(Transform parent, string name, string sprite = "ui_panel", float cornerScale = 2f)
        {
            RectTransform rt = Rect(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = Sprite(sprite);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = cornerScale;
            return image;
        }

        public static Image Icon(Transform parent, string sprite, float size)
        {
            RectTransform rt = Rect("Icon_" + sprite, parent);
            rt.sizeDelta = new Vector2(size, size);
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = Sprite(sprite);
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        public static Image Fill(Transform parent, string name, Color color)
        {
            RectTransform rt = Rect(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text Label(Transform parent, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Bold)
        {
            RectTransform rt = Rect("Label", parent);
            var label = rt.gameObject.AddComponent<Text>();
            // Hrubka sa riesi rezom pisma, nie umelym bold/italic z Unity.
            label.font = style == FontStyle.Bold ? Font : BodyFont;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.fontStyle = FontStyle.Normal;
            label.lineSpacing = 1.05f;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        public static void Outline(Text label, Color color, float distance = 1.5f)
        {
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
        }

        // Tlacidlo = pill sprite + volitelna ikona + text. Text je biely s obrysom pre citatelnost.
        public static Button Button(Transform parent, string name, string text, string pill, string icon,
            Action onClick, int fontSize = 24)
        {
            Image background = Panel(parent, name, pill);
            var button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            ColorBlock colors = button.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.6f);
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(() => onClick());

            var row = background.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleCenter;
            row.spacing = 8f;
            row.padding = new RectOffset(16, 16, 4, 4);
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            if (!string.IsNullOrEmpty(icon)) Icon(background.transform, icon, fontSize + 4);
            if (!string.IsNullOrEmpty(text))
            {
                Text label = Label(background.transform, text, fontSize, pill == "ui_panel" ? Ink : White,
                    TextAnchor.MiddleCenter);
                label.horizontalOverflow = HorizontalWrapMode.Overflow;
                var fitter = label.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                if (pill != "ui_panel") Outline(label, new Color32(70, 36, 16, 200), 1.6f);
                label.rectTransform.anchoredPosition += new Vector2(0f, 2f);
            }

            return button;
        }

        public static string Number(int value)
        {
            return value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
