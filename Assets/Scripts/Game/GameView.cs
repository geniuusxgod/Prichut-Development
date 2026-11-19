using UnityEngine;

namespace Prichut.Resort.Game
{
    // Zaklad pre vsetky obrazovky: jeden controller, jeden ResortSession, ziadna vlastna kopia stavu.
    public abstract class GameView : MonoBehaviour
    {
        protected ResortGameController Game { get; private set; }
        protected ResortSession Session => Game.Session;

        public void Bind(ResortGameController game)
        {
            Game = game;
            Game.Session.Changed += OnSessionChanged;
            Game.Session.EventRaised += OnResortEvent;
            Build();
            OnSessionChanged();
        }

        protected virtual void OnDestroy()
        {
            if (Game == null || Game.Session == null) return;
            Game.Session.Changed -= OnSessionChanged;
            Game.Session.EventRaised -= OnResortEvent;
        }

        protected abstract void Build();

        protected virtual void OnSessionChanged()
        {
        }

        protected virtual void OnResortEvent(ResortEvent e)
        {
        }
    }
}
