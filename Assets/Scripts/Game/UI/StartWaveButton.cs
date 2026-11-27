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

            // Hlavne CTA je okruhle tlacidlo s vlnou: jeden jasny symbol v dosahu palca, bez textu.
            Image wave = UiKit.Icon(root, "btn_wave", 124f);
            wave.name = "StartWave";
            wave.raycastTarget = true;
            button = wave.gameObject.AddComponent<Button>();
            button.targetGraphic = wave;
            button.onClick.AddListener(OnTap);
            UiKit.Place(wave.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-28f, 24f), new Vector2(124f, 124f));

            Image pill = UiKit.Panel(root, "Status", "ui_panel", 2.2f);
            pill.raycastTarget = false;
            UiKit.Place(pill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f),
                new Vector2(340f, 50f));
            status = UiKit.Label(pill.transform, "", 21, UiKit.Ink, TextAnchor.MiddleCenter);
            UiKit.Stretch(status.rectTransform);
        }

        // Jemne "dychanie" tlacidla, aby bolo jasne, co je hlavna akcia.
        private void Update()
        {
            if (button == null || !button.gameObject.activeSelf) return;
            float pulse = 1f + 0.04f * Mathf.Sin(Time.unscaledTime * 3f);
            button.transform.localScale = Vector3.one * pulse;
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
