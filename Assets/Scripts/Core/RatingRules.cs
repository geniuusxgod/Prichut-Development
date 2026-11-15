using System;

namespace Prichut.Resort
{
    // Perfect je samostatny stav pri 100 %, nie siesta hviezda.
    public enum RatingTier
    {
        OneStar = 0,
        TwoStars = 1,
        ThreeStars = 2,
        FourStars = 3,
        FiveStars = 4,
        Perfect = 5
    }

    public static class RatingRules
    {
        private const float Epsilon = 0.0001f;

        // Na hraniciach sa zvoli vyssi tier; minimum je vzdy 1 hviezda (vyplyva z wave_floor).
        public static RatingTier FromSatisfaction(float satisfaction, ResortConfig config)
        {
            float[] t = config.ratingThresholds;
            for (int i = t.Length - 1; i >= 1; i--)
                if (satisfaction + Epsilon >= t[i]) return (RatingTier)i;
            return RatingTier.OneStar;
        }

        public static int Stars(RatingTier tier)
        {
            return Math.Min(5, (int)tier + 1);
        }

        public static bool IsPerfect(RatingTier tier)
        {
            return tier == RatingTier.Perfect;
        }

        public static string Label(RatingTier tier)
        {
            return tier == RatingTier.Perfect ? "Perfect Stay" : Stars(tier) + (Stars(tier) == 1 ? " star" : " stars");
        }
    }
}
