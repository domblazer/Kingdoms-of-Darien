using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript Instance { get; private set; }
    // e.g. "Team_1": Inventory
    [HideInInspector]
    public Dictionary<string, InventoryScript> Inventories = new Dictionary<string, InventoryScript>();
    private InventoryScript playerInventory;

    private float manaRechargeRate = 0.1f;
    private float nextManaRecharge = 0;

    private GameObject currentHovering = null;

    private void Awake()
    {
        Instance = this;
        // @TODO: team management. "Player", "Team_2", "Team_3"
        // (note "Team_1" and "Player" are synonomous, but "Player" is used)
        Inventories.Add("Player", new InventoryScript());
        if (Inventories.TryGetValue("Player", out InventoryScript inv))
            playerInventory = inv;

        // @TODO: foreach(team in teamsFromStartScreen) Add(team,...)
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
