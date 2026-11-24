using UnityEngine;
using UnityEngine.UI;

namespace Prichut.Resort.Game
{
    // Zaklad modalnych obrazoviek: stlmene pozadie + karta. Modal je nad HUD aj panelmi.
    public abstract class ModalView : GameView
    {
        protected Image Backdrop { get; private set; }
        protected Image Card { get; private set; }

        protected void BuildModal(Vector2 cardSize, Vector2 anchor, Vector2 position, bool closeOnBackdrop)
        {
            UiKit.Stretch((RectTransform)transform);
            Backdrop = UiKit.Fill(transform, "Backdrop", UiKit.Dim);
            UiKit.Stretch(Backdrop.rectTransform, -400f);
            if (closeOnBackdrop)
            {
                var button = Backdrop.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(Close);
            }

            Card = UiKit.Panel(transform, "Card");
            UiKit.Place(Card.rectTransform, anchor, anchor, position, cardSize);
            SetOpen(false);
        }

        public bool IsOpen => Backdrop != null && Backdrop.gameObject.activeSelf;

        protected void SetOpen(bool open)
        {
            bool opening = open && !Card.gameObject.activeSelf;
            Backdrop.gameObject.SetActive(open);
            Card.gameObject.SetActive(open);
            if (opening) popTime = 0f;
        }

        private float popTime = -1f;

        // Jemne "pop" otvorenie karty: z 92 % na 100 % s malym prekmitom.
        protected virtual void LateUpdate()
        {
            if (popTime < 0f || Card == null) return;
            popTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(popTime / 0.22f);
            float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.03f;
            Card.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, t) * overshoot;
            if (t >= 1f)
            {
                Card.rectTransform.localScale = Vector3.one;
                popTime = -1f;
            }
        }

        public virtual void Close()
        {
            SetOpen(false);
        }
    }
}
