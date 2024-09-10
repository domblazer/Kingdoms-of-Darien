using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class InventoryBase : MonoBehaviour
{
    public int totalManaIncome = 0;
    public int normalManaIncome = 0;
    public int intangibleManaIncome = 0;
    public int totalManaStorage = 0;
    public int currentMana = 0;
    public float totalManaDrainPerSecond = 0;

    // Event emitter for inventory change (add/remove units)
    public event EventHandler<OnInventoryChangedEventArgs> OnUnitsChanged;
    public class OnInventoryChangedEventArgs : EventArgs
    {
        public bool wasAdded;
        public RTSUnit unitAffected;
        public List<RTSUnit> newTotalUnits;
        public Dictionary<UnitCategories, List<RTSUnit>> newGroupedUnits;
    }

    // Maintain totalUnits list and groupedUnits dictionary 
    public List<RTSUnit> totalUnits = new();
    public Dictionary<UnitCategories, List<RTSUnit>> groupedUnits = new();

    // Separate lists to maintain lodestones and intangibles
    public List<RTSUnit> lodestones = new();
    public List<IntangibleUnitBase> intangibleUnits = new();

    public int unitLimit = 500;
    public int totalUnitsCreated = 0;
    public int unitsLost = 0;
    private float rateOfChange = 0;
    private int increment = 1;
    private float nextManaChange = 0;
    public PlayerNumbers playerNumber;

    private void Start()
    {
        // @TODO: there should be a better way to register an event once all existing units have been loaded to trigger this function instead of just delaying by some seconds
        StartCoroutine(SetInitialMana());
    }

    IEnumerator SetInitialMana()
    {
        yield return new WaitForSeconds(0.75f);
        currentMana = totalManaStorage;
    }

    protected void UpdateMana(bool log = false)
    {
        // Mana is influenced by the list of intangibleUnits read at each frame
        intangibleManaIncome = 0;
        totalManaDrainPerSecond = 0;
        foreach (IntangibleUnitBase intangible in intangibleUnits)
        {
            // We are re-summing mana drain every frame just to keep up with the changing values of drainRate
            // We are only adding to the drain rate if there is at least one builder conjuring
            if (intangible.builders.Count > 0)
            {
                // @TODO: rounding is going to create small errors over time, so we might end up with -1 drain instead of 0 b/c of the way the rounding went
                totalManaDrainPerSecond += intangible.drainRate * intangible.builders.Count;
            }
            else
            {
                intangibleManaIncome += Mathf.RoundToInt(intangible.drainRate);
            }
        }
        totalManaIncome = normalManaIncome + intangibleManaIncome;

        if (totalManaIncome > 0 && currentMana <= totalManaStorage && currentMana >= 0)
        {
            // Rate of change is function of income and drain per second
            rateOfChange = 1 / (totalManaIncome + intangibleManaIncome - totalManaDrainPerSecond);
            if (rateOfChange < 0)
                increment = -1;
            else if (rateOfChange > 0)
                increment = 1;
            else
                increment = 0;

            if (log)
            {
                string str = "rateOfChange: " + rateOfChange + " \n" +
                "increment: " + increment;
                Debug.Log(str);
            }

            float rateAbs = Mathf.Abs(rateOfChange);
            if (Time.time > nextManaChange && rateAbs > 0)
            {
                currentMana += increment;
                nextManaChange = Time.time + rateAbs;
            }

            // Ensure currentMana stays in bounds
            if (currentMana >= totalManaStorage)
                currentMana = totalManaStorage;
            else if (currentMana <= 0)
                currentMana = 0;
        }
    }

    // @TODO: This logic should be in GameManager
    public void CheckWinLoseState()
    {
        bool victory = true;

        // @TODO: intangible count should be included here. Technically, an intangible with no builder still counts towards your victory condition
        // @TODO: do intangibles count toward unit count?

        if (totalUnits.Count == 0)
        {
            if (playerNumber == PlayerNumbers.Player1)
            {
                Debug.Log("Defeat.");
                StartCoroutine(HandleEndGame(false));
                return;
            }
            else if (GameManager.Instance.AIPlayers.TryGetValue(playerNumber, out AIPlayerContext aiPlayer))
            {
                aiPlayer.player.defeated = true;
            }

            // If any AI player is yet undefeated, victory has not been achieved
            foreach (KeyValuePair<PlayerNumbers, AIPlayerContext> player in GameManager.Instance.AIPlayers)
            {
                // @TODO: work in teams logic
                if (!player.Value.player.defeated)
                {
                    victory = false;
                    return;
                }
            }

            // If victory stays true until this point, we have victory
            if (victory)
            {
                Debug.Log("Victory.");
                StartCoroutine(HandleEndGame(true));
            }
        }
    }

    public IEnumerator HandleEndGame(bool victory)
    {
        if (victory)
            UIManager.Instance.ToggleVictoryText(true);
        else
            UIManager.Instance.ToggleDefeatText(true);

        // @TODO: disable any more click/keyboard events?

        // Pause briefly before exiting
        yield return new WaitForSeconds(5.0f);

        // @TODO: eventually work in intermediate score screen
        SceneManager.LoadScene("StartScene");
    }

    public void AddIntangible(IntangibleUnitBase unit)
    {
        intangibleUnits.Add(unit);
    }

    public void RemoveIntangible(IntangibleUnitBase unit)
    {
        intangibleUnits.Remove(unit);
    }

    public void AddUnit(RTSUnit unit)
    {
        // Sum up mana storage and mana income from all units
        totalManaStorage += unit.manaStorage;
        normalManaIncome += unit.manaIncome;

        totalUnits.Add(unit);
        // Try if unitType exists in groupedUnits dictionary
        if (groupedUnits.TryGetValue(unit.unitType, out List<RTSUnit> units))
        {
            // Initialize a new list for this key, if not already
            if (groupedUnits[unit.unitType] == null)
                groupedUnits[unit.unitType] = new List<RTSUnit>();
            groupedUnits[unit.unitType].Add(unit);
        }
        else
        {
            List<RTSUnit> newUnits = new() { unit };
            // If a unit is added whose type is not yet in the dictionary, assume to add new key-value for it
            groupedUnits.Add(unit.unitType, newUnits);
        }
        totalUnitsCreated++;
        // Fire unit added change event
        OnUnitsChanged?.Invoke(this, new OnInventoryChangedEventArgs
        {
            wasAdded = true,
            unitAffected = unit,
            newTotalUnits = totalUnits,
            newGroupedUnits = groupedUnits
        });
    }

    public void RemoveUnit(RTSUnit unit)
    {
        // Subtract mana storage and income values from removed units
        totalManaStorage -= unit.manaStorage;
        normalManaIncome -= unit.manaIncome;

        // Remove from totalUnits and groupedUnits
        totalUnits.Remove(unit);
        if (groupedUnits.TryGetValue(unit.unitType, out List<RTSUnit> units))
        {
            // Remove unit from direct list object reference
            groupedUnits[unit.unitType].Remove(unit);
        }
        unitsLost++;
        // Fire unit removed change event
        OnUnitsChanged?.Invoke(this, new OnInventoryChangedEventArgs
        {
            wasAdded = false,
            unitAffected = unit,
            newTotalUnits = totalUnits,
            newGroupedUnits = groupedUnits
        });
    }

    public List<RTSUnit> GetUnitsByType(UnitCategories type)
    {
        if (groupedUnits.TryGetValue(type, out List<RTSUnit> grouped))
            return grouped;
        return new List<RTSUnit>();
    }

    public List<RTSUnit> GetUnitsByTypes(params UnitCategories[] types)
    {
        List<RTSUnit> units = new();
        foreach (UnitCategories category in types)
            if (groupedUnits.TryGetValue(category, out List<RTSUnit> grouped))
                units.AddRange(grouped);
        return units;
    }
}
