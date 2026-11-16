using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S5 Upgrade Suggestion, modal nadvazujuci na S4. Later ho preskoci a ide sa na S1.
    public sealed class UpgradeSheet : ModalView
    {
        private Text heading;
        private Text body;
        private Button later;

        protected override void Build()
        {
            BuildModal(new Vector2(640f, 300f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), false);
            heading = UiKit.Label(Card.transform, "Before the next wave", 28, UiKit.Ink, TextAnchor.UpperCenter);
            UiKit.Place(heading.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(580f, 40f));
            body = UiKit.Label(Card.transform, "", 19, UiKit.Muted, TextAnchor.UpperCenter, FontStyle.Normal);
            body.supportRichText = true;
            UiKit.Place(body.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(560f, 110f));

            later = UiKit.Button(Card.transform, "Later", "Continue", "ui_grey", null, OnLater, 24);
            UiKit.Place((RectTransform)later.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(240f, 62f));
        }

        protected override void OnSessionChanged()
        {
            if (Card == null) return;
            bool show = Session.Phase == WavePhase.Upgrade;
            if (show && !IsOpen) Fill();
            SetOpen(show);
        }

        private void Fill()
        {
            WaveResult result = Session.LastResult;
            body.text = result != null && result.needsImprovement.Count > 0
                ? result.needsImprovement[0] + "\n\nPlace your Workers by the forecast before you start."
                : "Your resort handled the wave well.\n\nCheck the forecast and place your Workers.";
        }

        private void OnLater()
        {
            Session.FinishUpgradeStep();
        }
    }
}
