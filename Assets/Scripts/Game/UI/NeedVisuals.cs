using UnityEngine;

namespace Prichut.Resort.Game
{
    public static class NeedVisuals
    {
        public static string Icon(NeedId need)
        {
            switch (need)
            {
                case NeedId.Rooms: return "need_rooms";
                case NeedId.Drinks: return "need_drinks";
                case NeedId.Food: return "need_food";
                case NeedId.Relax: return "need_relax";
                default: return "need_oceanview";
            }
        }

        // Spolocne miesto laveho panela: S1 forecast a S2 needs panel sa striedaju na tom istom mieste,
        // aby ostrov ostal viditelny (UI handoff §4).
        public static RectTransform PlaceLeftPanel(RectTransform rt, float height)
        {
            return UiKit.Place(rt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -74f),
                new Vector2(310f, height));
        }
    }
}
