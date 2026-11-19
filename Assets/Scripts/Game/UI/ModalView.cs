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
            Backdrop.gameObject.SetActive(open);
            Card.gameObject.SetActive(open);
        }

        public virtual void Close()
        {
            SetOpen(false);
        }
    }
}
