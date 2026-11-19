using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Kratke guest feedback toasty (UI handoff §D): warning, pozitivny "catching up" a hlasy hosti.
    // Toasty su len obraz udalosti zo session; nic neriadia.
    public sealed class ToastView : GameView
    {
        private const int MaxToasts = 3;
        private const float Lifetime = 2.8f;

        private sealed class Toast
        {
            public CanvasGroup group;
            public float age;
        }

        private RectTransform stack;
        private readonly List<Toast> toasts = new List<Toast>();

        protected override void Build()
        {
            UiKit.Stretch((RectTransform)transform);
            stack = UiKit.Rect("Toasts", transform);
            UiKit.Place(stack, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(60f, -128f), new Vector2(520f, 200f));
            var layout = stack.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
        }

        public void Show(string text, ToastKind kind)
        {
            if (stack == null) return;
            while (toasts.Count >= MaxToasts) Remove(toasts[0]);

            string pill = kind == ToastKind.Warning ? "ui_orange" : kind == ToastKind.Positive ? "ui_green" : "ui_dark";
            Image background = UiKit.Panel(stack, "Toast", pill);
            background.raycastTarget = false;
            background.rectTransform.sizeDelta = new Vector2(440f, 42f);
            string icon = kind == ToastKind.Warning ? "icon_warning" : kind == ToastKind.Positive ? "icon_recovering" : null;
            float x = 16f;
            if (icon != null)
            {
                Image image = UiKit.Icon(background.transform, icon, 26f);
                UiKit.Place(image.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(26f, 26f));
                x = 48f;
            }

            Text label = UiKit.Label(background.transform, text, 19, UiKit.White, TextAnchor.MiddleLeft);
            UiKit.Outline(label, new Color(0f, 0f, 0f, 0.3f), 1f);
            UiKit.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(440f - x - 10f, 40f));
            var group = background.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            toasts.Add(new Toast { group = group });
        }

        protected override void OnResortEvent(ResortEvent e)
        {
            string building = ResortCatalog.BuildingName(e.building);
            switch (e.type)
            {
                case ResortEventType.BottleneckStarted:
                    Show(building + " queue growing", ToastKind.Warning);
                    Show(GuestLines.Waiting(e.need), ToastKind.Info);
                    break;
                case ResortEventType.Recovering:
                    Show(building + " is catching up", ToastKind.Positive);
                    Show(GuestLines.Happy(e.need), ToastKind.Info);
                    break;
                case ResortEventType.ArrivalClosed:
                    Show("All guests arrived", ToastKind.Info);
                    break;
            }
        }

        private void Update()
        {
            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                Toast toast = toasts[i];
                toast.age += Time.unscaledDeltaTime;
                toast.group.alpha = Mathf.Clamp01((Lifetime - toast.age) / 0.4f) * Mathf.Clamp01(toast.age / 0.15f);
                if (toast.age >= Lifetime) Remove(toast);
            }
        }

        private void Remove(Toast toast)
        {
            toasts.Remove(toast);
            if (toast.group != null) Destroy(toast.group.gameObject);
        }
    }

    public enum ToastKind
    {
        Info,
        Positive,
        Warning
    }

    // Kratke hlasy hosti podla typu potreby.
    public static class GuestLines
    {
        public static string Waiting(NeedId need)
        {
            switch (need)
            {
                case NeedId.Rooms: return "\"Our room isn't ready yet...\"";
                case NeedId.Drinks: return "\"Drinks take a bit too long.\"";
                case NeedId.Food: return "\"Food takes a bit too long.\"";
                case NeedId.Relax: return "\"Is there a free lounger?\"";
                default: return "\"We came for the ocean view...\"";
            }
        }

        public static string Happy(NeedId need)
        {
            switch (need)
            {
                case NeedId.Rooms: return "\"Check-in was quick!\"";
                case NeedId.Drinks: return "\"Cocktails are flowing again!\"";
                case NeedId.Food: return "\"Dinner was great!\"";
                case NeedId.Relax: return "\"So relaxing by the pool.\"";
                default: return "\"What a view!\"";
            }
        }
    }
}
