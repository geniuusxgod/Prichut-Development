using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Horny panel (PRIH-196): HUD z PRIH-12 ostava zdrojom hodnot (GameState + HudPresenter),
    // tu sa len zjednoti vzhlad s ostatnym UI: kremove plasky, velke farebne ikony, pismo Fredoka.
    public static class HudStyler
    {
        private const float CardHeight = 54f;

        public static void Apply()
        {
            GameObject bar = GameObject.Find("TopBar");
            if (bar == null) return;
            var rect = (RectTransform)bar.transform;
            rect.offsetMin = new Vector2(20f, -76f);
            rect.offsetMax = new Vector2(-20f, -16f);

            Style(bar.transform.Find("ResortValueCard"), "star_full", 250f);
            Style(bar.transform.Find("CoinsCard"), "coin", 170f);
            Style(bar.transform.Find("EnergyCard"), "icon_energy", 120f);

            // Rozostupy kariet vpravo (Coins vlavo od Energy).
            Transform coins = bar.transform.Find("CoinsCard");
            if (coins != null) ((RectTransform)coins).anchoredPosition = new Vector2(-136f, 0f);
        }

        private static void Style(Transform card, string icon, float width)
        {
            if (card == null) return;
            var rect = (RectTransform)card;
            rect.sizeDelta = new Vector2(width, CardHeight);

            var rounded = card.GetComponent<RoundedHudPanel>();
            if (rounded != null) rounded.enabled = false;
            if (card.Find("Background") == null)
            {
                Image background = UiKit.Panel(card, "Background", "ui_panel", 2.2f);
                background.raycastTarget = false;
                UiKit.Stretch(background.rectTransform);
                background.transform.SetAsFirstSibling();
            }

            Transform iconTransform = card.Find("Icon");
            if (iconTransform != null)
            {
                var image = iconTransform.GetComponent<Image>();
                image.sprite = UiKit.Sprite(icon);
                image.color = Color.white;
                var iconRect = (RectTransform)iconTransform;
                iconRect.anchoredPosition = new Vector2(-10f, 0f);
                iconRect.sizeDelta = new Vector2(46f, 46f);
            }

            Text value = card.GetComponentInChildren<Text>();
            if (value != null)
            {
                value.font = UiKit.Font;
                value.fontStyle = FontStyle.Normal;
                value.fontSize = 24;
                value.resizeTextMaxSize = 24;
                value.color = UiKit.Ink;
                value.rectTransform.offsetMin = new Vector2(44f, 4f);
                value.rectTransform.offsetMax = new Vector2(-12f, -4f);
            }
        }
    }
}
