using UnityEngine;
using DarienEngine.AI;
using System.Collections.Generic;

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
                GameManager.Instance.AIPlayers[unit.playerNumber].inventory.AddUnit(unit);
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
                GameManager.Instance.AIPlayers[unit.playerNumber].inventory.RemoveUnit(unit);
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
                GameManager.Instance.AIPlayers[playerNumber].inventory.AddIntangible(unit);
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
                GameManager.Instance.AIPlayers[playerNumber].inventory.RemoveIntangible(unit);
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

        // Rotate a vector 3 clockwise, ignoring y rotation
        public static Vector3 Rotate90CW(Vector3 aDir)
        {
            return new Vector3(aDir.z, 0, -aDir.x);
        }

        // Rotate a vector 3 counter-clockwise, ignoring y rotation
        public static Vector3 Rotate90CCW(Vector3 aDir)
        {
            return new Vector3(-aDir.z, 0, aDir.x);
        }

        public static float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);

            if (dir > 0.5f)
                return 1f;
            else if (dir < -0.5f)
                return -1f;
            else
                return 0f;
        }
    }

    public class RTSUnitComparer : IEqualityComparer<RTSUnit>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(RTSUnit x, RTSUnit y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Check whether the products' properties are equal.
            return x.uuid == y.uuid;
        }

        public int GetHashCode(RTSUnit unit)
        {
            // Check whether the object is null
            if (Object.ReferenceEquals(unit, null)) return 0;

            // Calculate the hash code for the unique field
            return unit.uuid.GetHashCode();
        }
    }
}