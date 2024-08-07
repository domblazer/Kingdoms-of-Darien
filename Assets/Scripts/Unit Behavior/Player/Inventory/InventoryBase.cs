using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class InventoryBase : MonoBehaviour
{
    public int totalManaIncome = 0;
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
        // @TODO: To keep track of all Intangibles and their changes in drain based on how many builders are attached, it might be better to keep track in a loop here and re-sum
        // the totalManaDrainPerSecond += unit.drainRate for each IntangibleUnit. That way, if an intangible was removed from the list, the totalManaDrainPerSecond would just be 
        // update with what is in the list, removing the need to subtract the drainRate
        int intangibleIncome = 0;
        foreach (IntangibleUnitBase intangible in intangibleUnits)
        {
            // Debug.Log("intangible drain rate: " + intangible.drainRate);
            // Debug.Log("intangible builder count " + intangible.builders.Count);
            // We are re-summing mana drain every frame just to keep up with the changing values of drainRate
            totalManaDrainPerSecond = 0;
            totalManaDrainPerSecond += intangible.drainRate;

            // @TODO: if the intangible drainRate is negative, it gets added to income
            // @TODO: rounding is going to create small errors over time, so we might end up with -1 drain instead of 0 b/c of the way the rounding went
            if (intangible.drainRate < 0)
                intangibleIncome += Mathf.RoundToInt(intangible.drainRate);
        }

        if (totalManaIncome > 0 && currentMana <= totalManaStorage && currentMana >= 0)
        {
            // Rate of change is function of income and drain per second
            rateOfChange = 1 / (totalManaIncome + intangibleIncome - totalManaDrainPerSecond);
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

    public void AddIntangible(IntangibleUnitBase unit)
    {
        // totalManaDrainPerSecond += unit.drainRate;
        intangibleUnits.Add(unit);

    }

    public void RemoveIntangible(IntangibleUnitBase unit, float flip = 1.0f)
    {
        // In this case, an intangible was removed because no Builder was on it and its mana was reversing up to this point
        /* if (flip < 0)
            totalManaIncome -= Mathf.RoundToInt(unit.drainRate);
        else
            totalManaDrainPerSecond -= unit.drainRate; */

        intangibleUnits.Remove(unit);
    }

    public void UpdateManaValues(int newIncome, int newDrain)
    {
        totalManaDrainPerSecond += newDrain;
        totalManaIncome += Mathf.RoundToInt(newIncome);
    }
    public void AddUnit(RTSUnit unit)
    {
        // Sum up mana storage and mana income from all units
        totalManaStorage += unit.manaStorage;
        totalManaIncome += unit.manaIncome;

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
        totalManaIncome -= unit.manaIncome;

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
