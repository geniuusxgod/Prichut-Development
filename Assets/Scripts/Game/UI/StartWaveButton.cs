using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // S1 hlavne CTA (PRIH-13): vola jeden controller a respektuje AWAITING_WAVE.
    // Mimo pripravy tlacidlo nie je, nie zasedene, jednoducho tam nie je.
    public sealed class StartWaveButton : GameView
    {
        private Button button;
        private Text status;

        protected override void Build()
        {
            var root = (RectTransform)transform;
            UiKit.Stretch(root);

            button = UiKit.Button(root, "StartWave", "Start Wave", "ui_green", "icon_play", OnTap, 30);
            UiKit.Place((RectTransform)button.transform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 24f), new Vector2(290f, 84f));

            Image pill = UiKit.Panel(root, "Status", "ui_panel", 2.2f);
            pill.raycastTarget = false;
            UiKit.Place(pill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f),
                new Vector2(340f, 50f));
            status = UiKit.Label(pill.transform, "", 21, UiKit.Ink, TextAnchor.MiddleCenter);
            UiKit.Stretch(status.rectTransform);
        }

        private void OnTap()
        {
            // Dvojity tap nespusti druhy generator, StartWave vrati false mimo AWAITING_WAVE.
            Session.StartWave();
        }

        protected override void OnSessionChanged()
        {
            if (button == null) return;
            button.gameObject.SetActive(Session.CanStartWave);
            status.text = StatusText();
        }

        private string StatusText()
        {
            switch (Session.Phase)
            {
                case WavePhase.AwaitingWave:
                    return Session.NextWaveNumber > Session.Config.WaveCount
                        ? "Ready for Guests, free play"
                        : "Wave " + Session.NextWaveNumber + " of " + Session.Config.WaveCount + ": ready for guests";
                case WavePhase.WaveIncoming: return "Guests are arriving";
                case WavePhase.WaveActive: return "Wave " + Session.Wave.waveNumber + " in progress";
                case WavePhase.WavePaused: return "Paused";
                default: return "Wave complete";
            }
        }
    }
}
