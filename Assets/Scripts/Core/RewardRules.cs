using System;

namespace Prichut.Resort
{
    public static class RewardRules
    {
        // Pocita jednotky needov vratane Sudden rush, nie vizualnych hosti.
        // Zaokruhlenie je explicitne: polovica sa zaokruhli smerom hore.
        public static int Coins(int needUnits, RatingTier tier, ResortConfig config)
        {
            float multiplier = config.coinsRatingMultiplier[Math.Min((int)tier, config.coinsRatingMultiplier.Length - 1)];
            double raw = (double)needUnits * config.coinsPerUnit * multiplier;
            return (int)Math.Round(raw, MidpointRounding.AwayFromZero);
        }
    }
}
