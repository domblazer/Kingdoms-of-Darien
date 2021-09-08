using UnityEngine;

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
                GameManagerScript.MainPlayerContext mainPlayer = GameManagerScript.Instance.PlayerMain;
                _Holder = mainPlayer.holder;
                mainPlayer.inventory.AddUnit(unit);
            }
            else if (GameManagerScript.Instance.AIPlayers.TryGetValue(unit.playerNumber, out GameManagerScript.AIPlayerContext aiPlayer))
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

        public static void AddIntangibleToPlayerContext<T>(IntangibleUnitBase<T> unit, bool addToHolder = true)
        {
            GameObject _Holder = null;
            PlayerNumbers playerNumber = unit.builder.baseUnit.playerNumber;
            if (playerNumber == PlayerNumbers.Player1)
            {
                // Add this unit to the main (human) player context
                GameManagerScript.MainPlayerContext mainPlayer = GameManagerScript.Instance.PlayerMain;
                _Holder = mainPlayer.holder;
                mainPlayer.inventory.AddIntangible(unit as IntangibleUnitBase<PlayerConjurerArgs>);
            }
            else if (GameManagerScript.Instance.AIPlayers.TryGetValue(playerNumber, out GameManagerScript.AIPlayerContext aiPlayer))
            {
                // Add this unit to the AI player context
                _Holder = aiPlayer.holder;
                aiPlayer.inventory.AddIntangible(unit as IntangibleUnitBase<AIConjurerArgs>);
            }
            else
                Debug.LogWarning("PlayerContextError: No player " + playerNumber + " found for " + unit.name);
            if (addToHolder)
                unit.transform.parent = _Holder.transform;
        }

        public static void RemoveUnitFromPlayerContext(RTSUnit unit)
        {
            if (unit.playerNumber == PlayerNumbers.Player1)
                GameManagerScript.Instance.PlayerMain.inventory.RemoveUnit(unit);
            else if (GameManagerScript.Instance.AIPlayers.TryGetValue(unit.playerNumber, out GameManagerScript.AIPlayerContext aiPlayer))
                aiPlayer.inventory.RemoveUnit(unit);
        }
    }
}