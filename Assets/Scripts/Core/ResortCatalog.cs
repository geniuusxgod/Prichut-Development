namespace Prichut.Resort
{
    // Typ potreby hosti (Build §4.4). Stabilne ID, poradie sa nemeni, uklada sa do save.
    public enum NeedId
    {
        Rooms = 0,
        Drinks = 1,
        Food = 2,
        Relax = 3,
        OceanView = 4
    }

    // Budovy prototypu: 3 od zaciatku, 2 ako unlock (Build §4.2).
    public enum BuildingId
    {
        Reception = 0,
        Restaurant = 1,
        BeachBar = 2,
        Pool = 3,
        Bungalows = 4
    }

    // Mapovanie budova -> need a zakladne texty. Jediny zdroj pravdy pre cely projekt.
    public static class ResortCatalog
    {
        public static readonly BuildingId[] AllBuildings =
        {
            BuildingId.Reception, BuildingId.Restaurant, BuildingId.BeachBar, BuildingId.Pool, BuildingId.Bungalows
        };

        public static readonly NeedId[] AllNeeds =
        {
            NeedId.Rooms, NeedId.Drinks, NeedId.Food, NeedId.Relax, NeedId.OceanView
        };

        public static NeedId NeedOf(BuildingId building)
        {
            switch (building)
            {
                case BuildingId.Reception: return NeedId.Rooms;
                case BuildingId.Restaurant: return NeedId.Food;
                case BuildingId.BeachBar: return NeedId.Drinks;
                case BuildingId.Pool: return NeedId.Relax;
                default: return NeedId.OceanView;
            }
        }

        public static BuildingId BuildingOf(NeedId need)
        {
            switch (need)
            {
                case NeedId.Rooms: return BuildingId.Reception;
                case NeedId.Food: return BuildingId.Restaurant;
                case NeedId.Drinks: return BuildingId.BeachBar;
                case NeedId.Relax: return BuildingId.Pool;
                default: return BuildingId.Bungalows;
            }
        }

        // Rooms, Drinks a Food su aktivne od zaciatku; Relax a Ocean View az po kupe budovy (WR9).
        public static bool IsStarter(BuildingId building)
        {
            return building == BuildingId.Reception || building == BuildingId.Restaurant ||
                   building == BuildingId.BeachBar;
        }

        public static string BuildingName(BuildingId building)
        {
            switch (building)
            {
                case BuildingId.Reception: return "Reception";
                case BuildingId.Restaurant: return "Restaurant";
                case BuildingId.BeachBar: return "Beach Bar";
                case BuildingId.Pool: return "Pool";
                default: return "Bungalows";
            }
        }

        public static string NeedName(NeedId need)
        {
            return need == NeedId.OceanView ? "Ocean View" : need.ToString();
        }
    }
}
