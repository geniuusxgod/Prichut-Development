using System;
using System.Collections.Generic;

namespace Prichut.Resort
{
    // Stav jedneho needu v aktivnej vlne (Build §4.4). Hra nesleduje osobny zoznam potrieb hosta -
    // pocita len jednotky needu. backlog = demand_arrived - filled (zahrna aj jednotky v obsluhe).
    [Serializable]
    public sealed class NeedRun
    {
        public NeedId need;
        public int baseTotal;
        public int regularArrived;
        public int filled;

        // FIFO rad cakajucich (cas prichodu kazdej jednotky) a oddelene miesta v obsluhe (progres 0..1).
        // Jednotka z radu nikdy neodide kvoli dlhemu cakaniu.
        public List<float> queue = new List<float>();
        public List<float> seats = new List<float>();

        public NeedRun()
        {
        }

        public NeedRun(NeedId need, int demandTotal)
        {
            this.need = need;
            baseTotal = Math.Max(0, demandTotal);
        }

        public BuildingId Building => ResortCatalog.BuildingOf(need);
        public int DemandTotal => baseTotal;
        public int DemandArrived => regularArrived;
        public int Backlog => DemandArrived - filled;
        public int Waiting => queue.Count;
        public int Seated => seats.Count;
    }
}
