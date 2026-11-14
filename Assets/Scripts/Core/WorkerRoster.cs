using System.Collections.Generic;

namespace Prichut.Resort
{
    // Prototyp: anonymny Worker pool, vzdy o jedneho mensi nez pocet kupenych budov (Build §4.5).
    // Kazda budova ma 1 slot, Worker vzdy stoji na niektorej budove, lavicka nepriradenych neexistuje.
    public static class WorkerRoster
    {
        public static int OwnedBuildings(ResortProgress progress)
        {
            int count = 0;
            foreach (BuildingState b in progress.buildings)
                if (b.owned) count++;
            return count;
        }

        public static int WorkerCount(ResortProgress progress)
        {
            int count = 0;
            foreach (BuildingState b in progress.buildings)
                if (b.owned && b.staffed) count++;
            return count;
        }

        public static bool IsStaffed(ResortProgress progress, BuildingId building)
        {
            BuildingState state = progress.Building(building);
            return state != null && state.owned && state.staffed;
        }

        public static List<BuildingId> EmptyBuildings(ResortProgress progress)
        {
            var result = new List<BuildingId>();
            foreach (BuildingState b in progress.buildings)
                if (b.owned && !b.staffed) result.Add(b.id);
            return result;
        }

        // workers = kupene budovy - 1, kazdy priradeny na budove (3/2 -> 4/3 -> 5/4).
        public static void AssignInitial(ResortProgress progress, BuildingId emptyBuilding)
        {
            foreach (BuildingState b in progress.buildings)
                b.staffed = b.owned && b.id != emptyBuilding;
        }
    }
}
