using UnityEngine;

namespace Constants
{
    public static class Functions
    {
        public static GameObject GetPlayerHolder(PlayerNumbers playerNumber)
        {
            return GameObject.Find(GetPlayerHolderSelector(playerNumber));
        }

        public static string GetPlayerHolderSelector(PlayerNumbers playerNumber)
        {
            return "_Player" + (int)playerNumber;
        }

        public static GameObject CreatePlayerHolder(PlayerNumbers playerNumber)
        {
            return new GameObject(GetPlayerHolderSelector(playerNumber));
        }
    }

    public enum Directions : int
    {
        Forward = 180, Right = 90, Backwards = 0, Left = -90
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
}
