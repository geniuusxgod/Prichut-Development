using System;

namespace Prichut.Resort
{
    // Jedna skriptovana vlna z tabulky Build §4.8. "guests" je len vizualny pocet postaviciek.
    [Serializable]
    public sealed class WaveDefinition
    {
        public int guests;
        public int rooms;
        public int drinks;
        public int food;
        public int relax;
        public int oceanView;

        public WaveDefinition()
        {
        }

        public WaveDefinition(int guests, int rooms, int drinks, int food, int relax, int oceanView)
        {
            this.guests = guests;
            this.rooms = rooms;
            this.drinks = drinks;
            this.food = food;
            this.relax = relax;
            this.oceanView = oceanView;
        }

        public int Demand(NeedId need)
        {
            switch (need)
            {
                case NeedId.Rooms: return rooms;
                case NeedId.Drinks: return drinks;
                case NeedId.Food: return food;
                case NeedId.Relax: return relax;
                default: return oceanView;
            }
        }
    }
}
