using System;
using System.Collections.Generic;

namespace Prichut.Resort
{
    // Wave Satisfaction sa akumuluje z cakania kazdej jednotky needu a vyhodnoti sa az na konci (Build §4.4).
    public static class SatisfactionRules
    {
        // Do grace 100 %, potom linearny pokles do floor_after, dalej drzi unit_floor (WR4).
        public static float Contribution(float waitSeconds, ResortConfig config)
        {
            if (waitSeconds <= config.patienceGrace) return 1f;
            if (waitSeconds >= config.patienceFloorAfter) return config.unitFloor;
            float t = (waitSeconds - config.patienceGrace) / (config.patienceFloorAfter - config.patienceGrace);
            return 1f + (config.unitFloor - 1f) * t;
        }

        public static float Weight(NeedId need, ResortConfig config)
        {
            return need == NeedId.OceanView ? config.oceanViewWeight : 1f;
        }

        // Wave Satisfaction = max(wave_floor, sum(prispevok * vaha) / sum(vaha)). Nulovy demand = 100 %.
        public static float WaveSatisfaction(IEnumerable<NeedRun> needs, ResortConfig config)
        {
            double weighted = 0d;
            double weights = 0d;
            foreach (NeedRun run in needs)
            {
                float weight = Weight(run.need, config);
                weighted += run.contributionSum * weight;
                weights += run.contributionCount * weight;
            }

            if (weights <= 0d) return 1f;
            return Math.Max(config.waveFloor, (float)(weighted / weights));
        }

        // Pevne pravidlo WR1: unit_floor < wave_floor < hranica 3 hviezd.
        public static bool FloorsAreValid(ResortConfig config, float threeStarThreshold)
        {
            return config.unitFloor < config.waveFloor && config.waveFloor < threeStarThreshold;
        }
    }
}
