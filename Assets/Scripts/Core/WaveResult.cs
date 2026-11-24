using System;
using System.Collections.Generic;

namespace Prichut.Resort
{
    [Serializable]
    public sealed class NeedResult
    {
        public NeedId need;
        public int served;
        public int total;
        public float satisfaction;
        public float bottleneckSeconds;
        public float bottleneckUnstaffedSeconds;
        public float longestWait;
    }

    // Jedna vysledkova struktura dokoncenej vlny. S4 z nej len cita; odmena sa pripise prave raz.
    [Serializable]
    public sealed class WaveResult
    {
        public int waveNumber;
        public RatingTier rating;
        public float satisfaction;
        public int unitsServed;
        public int unitsTotal;
        public int guests;
        public int coins;
        public int energy;
        public int resortValueGain;
        public int resortValueBefore;
        public List<NeedResult> needs = new List<NeedResult>();
        public List<string> wentWell = new List<string>();
        public List<string> needsImprovement = new List<string>();

        // Odporucany upgrade = budova needu, ktory bol v BOTTLENECK najdlhsie (-1 = ziadny).
        public int recommendedBuilding = -1;

        // Posledna skriptovana vlna prototypu, S4 ju oslavi ako finale. Dalej pokracuje volna hra.
        public bool isFinale;

        public BuildingId? RecommendedBuilding =>
            recommendedBuilding >= 0 ? (BuildingId)recommendedBuilding : (BuildingId?)null;

        public int ResortValueAfter => resortValueBefore + resortValueGain;
    }

    public static class WaveResultBuilder
    {
        public static WaveResult Build(ResortProgress progress, ResortConfig config)
        {
            WaveRun wave = progress.wave;
            var result = new WaveResult
            {
                waveNumber = wave.waveNumber,
                guests = wave.guests,
                satisfaction = SatisfactionRules.WaveSatisfaction(wave.needs, config),
                resortValueBefore = progress.resortValue
            };
            result.rating = RatingRules.FromSatisfaction(result.satisfaction, config);
            result.isFinale = wave.waveNumber == config.WaveCount;

            foreach (NeedRun run in wave.needs)
            {
                result.unitsServed += run.filled;
                result.unitsTotal += run.DemandTotal;
                result.needs.Add(new NeedResult
                {
                    need = run.need,
                    served = run.filled,
                    total = run.DemandTotal,
                    satisfaction = run.contributionCount > 0 ? run.contributionSum / run.contributionCount : 1f,
                    bottleneckSeconds = run.bottleneckSeconds,
                    bottleneckUnstaffedSeconds = run.bottleneckUnstaffedSeconds,
                    longestWait = run.longestWait
                });
            }

            result.coins = RewardRules.Coins(result.unitsTotal, result.rating, config);
            result.resortValueGain = ResortValueGain(result.unitsTotal, result.rating, config);
            Explain(result);
            result.recommendedBuilding = Recommend(result, progress, config);
            return result;
        }

        // AC6: odporucany upgrade zodpoveda needu, ktory bol v BOTTLENECK najdlhsie. Budova na maxime
        // sa preskoci. Bez bottlenecku sa odporuci need s najnizsou spokojnostou, ak nie je 100 %.
        public static int Recommend(WaveResult result, ResortProgress progress, ResortConfig config)
        {
            NeedResult best = null;
            foreach (NeedResult need in result.needs)
            {
                if (!CanGrow(need.need, progress, config) || need.bottleneckSeconds <= 0f) continue;
                if (best == null || need.bottleneckSeconds > best.bottleneckSeconds) best = need;
            }

            if (best == null)
            {
                foreach (NeedResult need in result.needs)
                {
                    if (!CanGrow(need.need, progress, config) || need.satisfaction >= 0.999f) continue;
                    if (best == null || need.satisfaction < best.satisfaction) best = need;
                }
            }

            return best == null ? -1 : (int)ResortCatalog.BuildingOf(best.need);
        }

        private static bool CanGrow(NeedId need, ResortProgress progress, ResortConfig config)
        {
            BuildingState state = progress.Building(ResortCatalog.BuildingOf(need));
            return state != null && state.owned && state.level < config.MaxLevel;
        }

        // Resort Value je neubudajuci status; pravidlo prirastku je v konfiguracii.
        public static int ResortValueGain(int units, RatingTier tier, ResortConfig config)
        {
            float multiplier = config.coinsRatingMultiplier[Math.Min((int)tier, config.coinsRatingMultiplier.Length - 1)];
            return Math.Max(1, (int)Math.Round(units * config.resortValuePerUnit * multiplier, MidpointRounding.AwayFromZero));
        }

        // Rating vysvetluje sam seba: vzdy aspon jedna pozitivna veta, maky ton pri nizsom vysledku.
        private static void Explain(WaveResult result)
        {
            NeedResult worst = null;
            foreach (NeedResult need in result.needs)
            {
                if (need.bottleneckSeconds <= 0f && need.satisfaction >= 0.9f)
                    result.wentWell.Add(ResortCatalog.NeedName(need.need) + " flowed smoothly.");
                if (need.bottleneckSeconds > 0f && (worst == null || need.bottleneckSeconds > worst.bottleneckSeconds))
                    worst = need;
            }

            if (result.wentWell.Count == 0) result.wentWell.Add("Every guest was served - nobody left mid-wave.");

            if (worst != null)
            {
                string building = ResortCatalog.BuildingName(ResortCatalog.BuildingOf(worst.need));
                bool mostlyEmpty = worst.bottleneckUnstaffedSeconds > worst.bottleneckSeconds * 0.5f;
                result.needsImprovement.Add(mostlyEmpty
                    ? building + " had no Worker while its queue grew - move a Worker there (free)."
                    : building + " is too small for this crowd - more seats would help.");
            }
            else if (result.rating != RatingTier.Perfect)
            {
                result.needsImprovement.Add("A few guests waited a little - place Workers by the forecast.");
            }
        }
    }
}
