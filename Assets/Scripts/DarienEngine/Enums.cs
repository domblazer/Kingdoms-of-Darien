namespace DarienEngine
{
    public enum Directions : int
    {
        Forward = 180,
        Right = 90,
        Backwards = 0,
        Left = -90
    }

    public enum UnitCategories
    {
        Monarch,
        LodestoneTier1,
        LodestoneTier2,
        FactoryTier1,
        FactoryTier2,
        BuilderTier1,
        BuilderTier2,
        FortTier1,
        FortTier2,
        SiegeTier1,
        SiegeTier2,
        NavalTier1,
        NavalTier2,
        Dragon,
        Scout,
        StalwartTier1,
        StalwartTier2,
        InfantryTier1,
        InfantryTier2
    }

    public enum PlayerNumbers
    {
        Player1 = 1,
        Player2 = 2,
        Player3 = 3,
        Player4 = 4,
        Player5 = 5,
        Player6 = 6,
        Player7 = 7,
        Player8 = 8
    }

    public enum TeamNumbers
    {
        Team1 = 1,
        Team2 = 2,
        Team3 = 3,
        Team4 = 4,
        Team5 = 5,
        Team6 = 6,
        Team7 = 7,
        Team8 = 8
    }

    public enum Factions
    {
        Aramon,
        Taros,
        Veruna,
        Zhon,
        Creon
    }

    public enum CommandTypes
    {
        Move, 
        Attack, 
        Conjure,
        Patrol, 
        Guard
    }
}