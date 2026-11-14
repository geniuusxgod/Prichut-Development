using System;
using System.Collections.Generic;

namespace Prichut.Resort
{
    [Serializable]
    public sealed class BuildingState
    {
        public BuildingId id;
        public bool owned;
        public int level = 1;
        public bool staffed;

        public BuildingState()
        {
        }

        public BuildingState(BuildingId id, bool owned)
        {
            this.id = id;
            this.owned = owned;
        }
    }

    // Kompletny stav hraca, jeden zdroj pravdy pre HUD, UI aj ulozenie.
    [Serializable]
    public sealed class ResortProgress
    {
        public int coins;
        public int energy;
        public int resortValue;
        public int completedWaves;
        public WavePhase phase = WavePhase.AwaitingWave;
        public WavePhase phaseBeforePause = WavePhase.WaveActive;
        public List<BuildingState> buildings = new List<BuildingState>();
        public WaveRun wave;

        public BuildingState Building(BuildingId id)
        {
            for (int i = 0; i < buildings.Count; i++)
                if (buildings[i].id == id) return buildings[i];
            return null;
        }

        public bool Owns(BuildingId id)
        {
            BuildingState state = Building(id);
            return state != null && state.owned;
        }

        public static ResortProgress NewGame(ResortConfig config)
        {
            var progress = new ResortProgress
            {
                coins = config.startCoins,
                energy = config.startEnergy,
                resortValue = config.startResortValue
            };
            foreach (BuildingId id in ResortCatalog.AllBuildings)
                progress.buildings.Add(new BuildingState(id, ResortCatalog.IsStarter(id)));
            WorkerRoster.AssignInitial(progress, config.initiallyEmpty);
            return progress;
        }
    }
}
