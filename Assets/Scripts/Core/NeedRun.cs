using System;

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
    }
}
