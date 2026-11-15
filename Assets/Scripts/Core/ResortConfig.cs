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

        // Startovaci stav hraca.
        public int startCoins = 0;
        public int startEnergy = 6;
        public int startResortValue = 100;

        // Na starte su 3 budovy a 2 Workeri, tato budova zacina neobsadena.
        public BuildingId initiallyEmpty = BuildingId.Restaurant;

        // WAVE_INCOMING: lod/shuttle, hostia vystupuju (transient, ~2-4 s).
        public float incomingSeconds = 2.5f;

        // Prichod (PRIH-110): wave_timer zatvara prichod, nie vlnu.
        public float waveTimer = 90f;

        // Bottleneck (PRIH-121): warning sleduje len rad cakajucich, nie hosti na miestach.
        public int bottleneckThreshold = 4;
        public float bottleneckHysteresis = 3f;

        // Satisfaction (PRIH-142): cakanie = prichod -> usadenie, cas obsluhy sa nepocita.
        public float patienceGrace = 10f;
        public float patienceFloorAfter = 30f;
        public float unitFloor = 0.25f;
        public float waveFloor = 0.40f;
        public float oceanViewWeight = 1.5f;

        // Rating (PRIH-145): 1-5 hviezd a Perfect. Najnizsi vysledok je 1 hviezda.
        public float[] ratingThresholds = { 0.40f, 0.55f, 0.70f, 0.80f, 0.90f, 1.00f };

        // Odmena (PRIH-151): Coins = jednotky vlny * coins_per_unit * nasobic ratingu.
        public int coinsPerUnit = 16;
        public float[] coinsRatingMultiplier = { 0.4f, 0.6f, 0.8f, 1.0f, 1.2f, 1.3f };

        // Resort Value rastie z obsluzenych jednotiek vlny (status, nikdy sa neminie).
        public float resortValuePerUnit = 1f;

        // Tabulka vln (Build §4.8). Po poslednej vlne sa opakuje posledna.
        public WaveDefinition[] waves =
        {
            new WaveDefinition(12, 8, 8, 6, 0, 0),
            new WaveDefinition(18, 12, 16, 10, 0, 0),
            new WaveDefinition(24, 16, 18, 14, 0, 0)
        };

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

        public WaveDefinition WaveAt(int waveNumber)
        {
            int index = Math.Max(0, Math.Min(waves.Length - 1, waveNumber - 1));
            return waves[index];
        }
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
