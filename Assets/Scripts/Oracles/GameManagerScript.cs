using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constants;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance { get; private set; }

    public class VirtualPlayer
    {
        public GameObject holder;
        // @TODO: refactor UnitSelectionScript to Player, then PlayerBase->Player,PlayerAI 
        public AIPlayer playerScript;
        public InventoryScript inventory;
    }

    // e.g. "Team_1": Inventory
    [HideInInspector]
    public Dictionary<string, InventoryScript> Inventories = new Dictionary<string, InventoryScript>();
    private InventoryScript playerInventory;

    public Dictionary<int, VirtualPlayer> PlayerRoots = new Dictionary<int, VirtualPlayer>();

    private float manaRechargeRate = 0.1f;
    private float nextManaRecharge = 0;

    private GameObject currentHovering = null;

    private void Awake()
    {
        Instance = this;

        RTSUnit[] existingUnits = GameObject.FindObjectsOfType<RTSUnit>();
        if (existingUnits.Length > 0)
        {
            foreach (RTSUnit unit in existingUnits)
                GroupUnitUnderPlayer(unit);
        }
        else
        {
            // Create player holders based on config set in skirmesh menu, then init the monarchs
        }

    }

    private void GroupUnitUnderPlayer(RTSUnit unit)
    {
        GameObject _Holder;
        PlayerNumbers playerNumber = unit.playerNumber;
        // First see if we've stored this holder yet, get from dictionary
        if (TryGetVirtualPlayer(playerNumber, out VirtualPlayer virtualPlayer))
            _Holder = virtualPlayer.holder;
        // If we don't have the holder stored yet, find it
        else
        {
            _Holder = Functions.GetPlayerHolder(playerNumber);
            // If master unit holder doesn't exist yet, create it
            if (!_Holder)
                _Holder = InitNewPlayer(playerNumber);
        }
        // Child the unit to the holder object
        unit.transform.parent = _Holder.transform;
    }

    private GameObject InitNewPlayer(PlayerNumbers playerNumber)
    {
        GameObject _Holder = Functions.CreatePlayerHolder(playerNumber);
        // Add the holder to our dictionary for quick access later
        InventoryScript newInventory = _Holder.AddComponent<InventoryScript>();
        // @TODO: add the PlayerBase (share player and AIplayer) component
        AIPlayer player = _Holder.AddComponent<AIPlayer>();
        // @TODO: set player values, e.g. playerNumber
        // player.playerNumber = playerNumber;
        // player.Init(startingUnits);
        PlayerRoots.Add((int)playerNumber, new VirtualPlayer
        {
            holder = _Holder,
            playerScript = player,
            inventory = newInventory
        });
        return _Holder;
    }

    public bool TryGetVirtualPlayer(PlayerNumbers playerNumber, out VirtualPlayer virtualPlayer)
    {
        if (PlayerRoots.TryGetValue((int)playerNumber, out VirtualPlayer vp))
        {
            virtualPlayer = vp;
            return true;
        }
        virtualPlayer = null;
        return false;
    }

    private void Update()
    {

        // @TODO: manage all teams mana
        // @TODO: if mana is taking any amount of drain, need to noramalize that into the rate change with some formula I need to figure out,
        // like if drainRate surpasses rechargeRate, just drain at a slower pace, else recharge at a slower pace, type thing
        if (playerInventory.lodestones.Count > 0 && playerInventory.currentMana <= playerInventory.totalManaStorage && playerInventory.currentMana >= 0)
        {
            // Every 1/10th of a second, add the totalManaIncome to currentMana
            if (Time.time > nextManaRecharge)
            {
                playerInventory.currentMana += playerInventory.GetManaChangeRate();

                if (playerInventory.currentMana > playerInventory.totalManaStorage)
                    playerInventory.currentMana = playerInventory.totalManaStorage;
                else if (playerInventory.currentMana < 0)
                    playerInventory.currentMana = 0;

                nextManaRecharge = Time.time + manaRechargeRate;
            }
        }

        UIManager.Instance.SetManaUI(playerInventory);
    }

    public void SetHovering(GameObject obj)
    {
        currentHovering = obj;
    }

    public void ClearHovering()
    {
        currentHovering = null;
    }

    public bool IsHovering()
    {
        return currentHovering != null;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
}
