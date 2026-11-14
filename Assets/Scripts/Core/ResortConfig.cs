using System;

namespace Prichut.Resort
{
    // Vsetky laditelne hodnoty prototypu na jednom mieste (Build §4.8). Cisla su TUNING, nie finalne.
    [Serializable]
    public sealed class ResortConfig
    {
        // Obsluha budov (PRIH-116): capacity podla levelu, spolocny service_time a utilization.
        public int[] capacityByLevel = { 3, 5, 7 };
        public float serviceTime = 12f;
        public float utilizationStaffed = 1f;
        public float utilizationEmpty = 0.33f;

        public static ResortConfig CreateDefault()
        {
            return new ResortConfig();
        }

        public int CapacityForLevel(int level)
        {
            int index = Math.Max(0, Math.Min(capacityByLevel.Length - 1, level - 1));
            return capacityByLevel[index];
        }

        public int MaxLevel => capacityByLevel.Length;
    }

    public static class ServiceRules
    {
        // Rychlost postupu jednej obsluhy za sekundu = utilization / service_time.
        // Plati okamzite aj pre rozbehnute obsluhy (Build §4.4 "Obsluha je priebeh").
        public static float Speed(ResortConfig config, bool staffed)
        {
            float utilization = staffed ? config.utilizationStaffed : config.utilizationEmpty;
            return utilization / config.serviceTime;
        }
    }
}
