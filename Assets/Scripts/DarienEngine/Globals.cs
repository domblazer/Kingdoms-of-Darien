using UnityEngine;
using DarienEngine.AI;

namespace DarienEngine
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

        public static GameObject GetOrCreatePlayerHolder(PlayerNumbers playerNum)
        {
            GameObject _Holder = GetPlayerHolder(playerNum);
            if (!_Holder)
                _Holder = CreatePlayerHolder(playerNum);
            return _Holder;
        }

        public static void AddUnitToPlayerContext(RTSUnit unit, bool addToHolder = true)
        {
            GameObject _Holder = null;
            if (unit.playerNumber == PlayerNumbers.Player1)
            {
                // Add this unit to the main (human) player context
                MainPlayerContext mainPlayer = GameManager.Instance.PlayerMain;
                _Holder = mainPlayer.holder;
                mainPlayer.inventory.AddUnit(unit);
            }
            else if (GameManager.Instance.AIPlayers.TryGetValue(unit.playerNumber, out AIPlayerContext aiPlayer))
            {
                // Add this unit to the AI player context
                _Holder = aiPlayer.holder;
                aiPlayer.inventory.AddUnit(unit);
            }
            else
                Debug.LogWarning("PlayerContextError: No player " + unit.playerNumber + " found for " + unit.unitName);
            if (addToHolder)
                unit.transform.parent = _Holder.transform;
        }

        public static void RemoveUnitFromPlayerContext(RTSUnit unit)
        {
            if (unit.playerNumber == PlayerNumbers.Player1)
                GameManager.Instance.PlayerMain.inventory.RemoveUnit(unit);
            else if (GameManager.Instance.AIPlayers.TryGetValue(unit.playerNumber, out AIPlayerContext aiPlayer))
                aiPlayer.inventory.RemoveUnit(unit);
        }

        public static void AddIntangibleToPlayerContext(IntangibleUnitBase unit, bool addToHolder = true)
        {
            GameObject _Holder = null;
            PlayerNumbers playerNumber = unit.builder.baseUnit.playerNumber;
            if (playerNumber == PlayerNumbers.Player1)
            {
                // Add this unit to the main (human) player context
                MainPlayerContext mainPlayer = GameManager.Instance.PlayerMain;
                _Holder = mainPlayer.holder;
                mainPlayer.inventory.AddIntangible(unit);
            }
            else if (GameManager.Instance.AIPlayers.TryGetValue(playerNumber, out AIPlayerContext aiPlayer))
            {
                // Add this unit to the AI player context
                _Holder = aiPlayer.holder;
                aiPlayer.inventory.AddIntangible(unit);
            }
            else
                Debug.LogWarning("PlayerContextError: No player " + playerNumber + " found for " + unit.name);
            if (addToHolder)
                unit.transform.parent = _Holder.transform;
        }

        public static void RemoveIntangibleFromPlayerContext(IntangibleUnitBase unit)
        {
            PlayerNumbers playerNumber = unit.builder.baseUnit.playerNumber;
            if (playerNumber == PlayerNumbers.Player1)
                GameManager.Instance.PlayerMain.inventory.RemoveIntangible(unit);
            else if (GameManager.Instance.AIPlayers.TryGetValue(playerNumber, out AIPlayerContext aiPlayer))
                aiPlayer.inventory.RemoveIntangible(unit);
        }

        public static RectTransform FindBuildMenu(RTSUnit unit)
        {
            string searchPath = null;
            if (unit.unitName == "Mage Builder")
                searchPath = "AraCanvas/BuildMenus/MageBuilderMenu";
            else if (unit.unitName == "Barracks")
                searchPath = "AraCanvas/BuildMenus/BarracksMenu";
            else if (unit.unitName == "Keep")
                searchPath = "AraCanvas/BuildMenus/KeepMenu";
            else if (unit.unitName == "Dark Mason")
                searchPath = "TaroCanvas/BuildMenus/DarkMasonMenu";
            else if (unit.unitName == "Cabal")
                searchPath = "TaroCanvas/BuildMenus/CabalMenu";
            return GameObject.Find(searchPath).GetComponent<RectTransform>();
        }
    }
}