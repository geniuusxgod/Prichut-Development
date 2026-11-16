using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S4 Wave Complete (PRIH-35): moment uzavretia vlny. Zobrazuje rating, obsluzenie, satisfaction
    // (tu prvykrat ako cislo), odmeny, prirastok Resort Value a vysvetlenie vysledku.
    public sealed class WaveCompleteScreen : ModalView
    {
        private Text title;
        private readonly List<Image> stars = new List<Image>();
        private Image perfect;
        private Text ratingLabel;
        private Text served;
        private Text satisfaction;
        private Text coins;
        private Text energy;
        private Text resortValue;
        private Text wentWell;
        private Text improve;
        private Button claim;

        protected override void Build()
        {
            BuildModal(new Vector2(860f, 540f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), false);
            Transform card = Card.transform;

            title = UiKit.Label(card, "Wave Complete", 34, UiKit.Ink, TextAnchor.MiddleCenter);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(700f, 44f));

            RectTransform starRow = UiKit.Rect("Stars", card);
            UiKit.Place(starRow, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -64f), new Vector2(360f, 64f));
            var layout = starRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            for (int i = 0; i < 5; i++) stars.Add(UiKit.Icon(starRow, "star_empty", 62f));
            perfect = UiKit.Icon(card, "badge_perfect", 72f);
            UiKit.Place(perfect.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -128f), new Vector2(200f, 66f));
            ratingLabel = UiKit.Label(card, "", 22, UiKit.Muted, TextAnchor.MiddleCenter);
            UiKit.Place(ratingLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -134f), new Vector2(400f, 30f));

            served = Stat(card, new Vector2(-200f, -184f));
            satisfaction = Stat(card, new Vector2(200f, -184f));

            Image rewards = UiKit.Panel(card, "Rewards", "ui_dark", 3f);
            rewards.raycastTarget = false;
            UiKit.Place(rewards.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -232f), new Vector2(760f, 60f));
            coins = Reward(rewards.transform, "coin", new Vector2(-250f, 0f));
            energy = Reward(rewards.transform, "icon_energy", new Vector2(-40f, 0f));
            resortValue = Reward(rewards.transform, "star_full", new Vector2(200f, 0f));
            resortValue.rectTransform.sizeDelta = new Vector2(260f, 40f);

            wentWell = Column(card, "What went well", UiKit.Positive, new Vector2(-195f, -310f));
            improve = Column(card, "Needs improvement", UiKit.Warning, new Vector2(195f, -310f));

            claim = UiKit.Button(card, "Claim", "Claim Rewards", "ui_green", null, OnClaim, 28);
            UiKit.Place((RectTransform)claim.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(320f, 70f));
        }

        private static Text Stat(Transform parent, Vector2 position)
        {
            Text label = UiKit.Label(parent, "", 22, UiKit.Ink, TextAnchor.MiddleCenter);
            label.supportRichText = true;
            UiKit.Place(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(380f, 34f));
            return label;
        }

        private static Text Reward(Transform parent, string icon, Vector2 position)
        {
            Image image = UiKit.Icon(parent, icon, 38f);
            UiKit.Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position - new Vector2(70f, 0f), new Vector2(38f, 38f));
            Text label = UiKit.Label(parent, "", 24, UiKit.White, TextAnchor.MiddleLeft);
            UiKit.Place(label.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), position - new Vector2(44f, 0f), new Vector2(170f, 40f));
            return label;
        }

        private static Text Column(Transform parent, string heading, Color color, Vector2 position)
        {
            Text head = UiKit.Label(parent, heading, 20, color, TextAnchor.UpperLeft);
            UiKit.Place(head.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position, new Vector2(370f, 26f));
            Text body = UiKit.Label(parent, "", 17, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Normal);
            UiKit.Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), position + new Vector2(0f, -28f), new Vector2(370f, 96f));
            return body;
        }

        private void OnClaim()
        {
        }
    }
}
