namespace Prichut.Resort
{
    public enum ResortEventType
    {
        PhaseChanged,
        WaveStarted,
        UnitsArrived,
        UnitsServed,
        ArrivalClosed,
        WaveResolved,
        BottleneckStarted,
        Recovering,
        NeedOk,
        NeedSatisfied,
        WorkerMoved,
        RushStarted,
        BoostStarted,
        BoostEnded,
        RewardsClaimed,
        BuildingUpgraded,
        AreaUnlocked,
        ProgressReset
    }

    // Udalost pre UI, zvuky a toasty. Logika hry na udalostiach nezavisi.
    public struct ResortEvent
    {
        public ResortEventType type;
        public NeedId need;
        public BuildingId building;
        public int value;

        public ResortEvent(ResortEventType type, NeedId need = NeedId.Rooms,
            BuildingId building = BuildingId.Reception, int value = 0)
        {
            this.type = type;
            this.need = need;
            this.building = building;
            this.value = value;
        }
    }
}
