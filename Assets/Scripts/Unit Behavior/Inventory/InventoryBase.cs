using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class InventoryBase : MonoBehaviour
{
    public int totalManaIncome = 0;
    public int totalManaStorage = 10;
    public int currentMana = 0;
    public int manaDrainRate = 0;

    private float manaRechargeRate = 0.1f;
    private float nextManaRecharge = 0;

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
    public List<RTSUnit> totalUnits = new List<RTSUnit>();
    public Dictionary<UnitCategories, List<RTSUnit>> groupedUnits = new Dictionary<UnitCategories, List<RTSUnit>>();

    // Separate lists to maintain lodestones and intangibles
    public List<RTSUnit> lodestones = new List<RTSUnit>();
    public List<IntangibleUnitBase> intangibleUnits = new List<IntangibleUnitBase>();

    public int unitLimit = 500;

    protected void UpdateMana()
    {
        // @TODO: manage all teams mana
        // @TODO: if mana is taking any amount of drain, need to noramalize that into the rate change with some formula I need to figure out,
        // like if drainRate surpasses rechargeRate, just drain at a slower pace, else recharge at a slower pace, type thing
        if (lodestones.Count > 0 && currentMana <= totalManaStorage && currentMana >= 0)
        {
            // Every 1/10th of a second, add the totalManaIncome to currentMana
            if (Time.time > nextManaRecharge)
            {
                currentMana += GetManaChangeRate();

                if (currentMana > totalManaStorage)
                    currentMana = totalManaStorage;
                else if (currentMana < 0)
                    currentMana = 0;

                nextManaRecharge = Time.time + manaRechargeRate;
            }
        }
    }

    public void PlusTotalMana(int mana)
    {
        totalManaStorage += mana;
    }

    public void MinusTotalMana(int mana)
    {
        totalManaStorage -= mana;
    }

    public void PlusCurrentMana(int mana)
    {
        currentMana += mana;
    }

    public void MinusCurrentMana(int mana)
    {
        // @TODO: add this to the sum/rechargeRate
        if (currentMana >= 0)
        {
            currentMana -= mana;
        }

    }

    public void AddIntangible(IntangibleUnitBase unit)
    {
        manaDrainRate += unit.drainRate;
        intangibleUnits.Add(unit);
    }

    public void RemoveIntangible(IntangibleUnitBase unit)
    {
        manaDrainRate -= unit.drainRate;
        intangibleUnits.Remove(unit);
    }

    protected void AddLodestone(RTSUnit lodeUnit)
    {
        totalManaStorage += lodeUnit.manaStorage;
        totalManaIncome += lodeUnit.manaIncome;
        lodestones.Add(lodeUnit);
    }

    protected void RemoveLodestone(RTSUnit lodeUnit)
    {
        totalManaStorage -= lodeUnit.manaStorage;
        totalManaIncome -= lodeUnit.manaIncome;
        lodestones.Remove(lodeUnit);
    }

    public int GetManaChangeRate()
    {
        return totalManaIncome - manaDrainRate;
    }

    public void AddUnit(RTSUnit unit)
    {
        // Special Add for Lodestones
        if (unit.unitType == UnitCategories.LodestoneTier1 || unit.unitType == UnitCategories.LodestoneTier2)
            AddLodestone(unit);

        totalUnits.Add(unit);
        // Try if unitType exists in groupedUnits dictionary
        if (groupedUnits.TryGetValue(unit.unitType, out List<RTSUnit> units))
        {
            // Initialize a new list for this key, if not already
            if (units == null)
                units = new List<RTSUnit>();
            units.Add(unit);
        }
        else
        {
            List<RTSUnit> newUnits = new List<RTSUnit>();
            newUnits.Add(unit);
            // If a unit is added whose type is not yet in the dictionary, assume to add new key-value for it
            groupedUnits.Add(unit.unitType, newUnits);
        }
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
        // Remove lodestone 
        if (unit.unitType == UnitCategories.LodestoneTier1 || unit.unitType == UnitCategories.LodestoneTier2)
            RemoveLodestone(unit);
        // Remove from totalUnits and groupedUnits
        totalUnits.Remove(unit);
        if (groupedUnits.TryGetValue(unit.unitType, out List<RTSUnit> units))
            units.Remove(unit);
        else
            Debug.LogWarning("Error removing unit from grouped inventory.");
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
        List<RTSUnit> units = new List<RTSUnit>();
        foreach (UnitCategories category in types)
            if (groupedUnits.TryGetValue(category, out List<RTSUnit> grouped))
                units.AddRange(grouped);
        return units;
    }
}
