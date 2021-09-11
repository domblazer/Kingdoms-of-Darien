using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class InventoryBase<T> : MonoBehaviour
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

    public List<LodestoneScript> lodestones = new List<LodestoneScript>();
    public List<RTSUnit> totalUnits = new List<RTSUnit>();
    public Dictionary<UnitCategories, List<RTSUnit>> groupedUnits = new Dictionary<UnitCategories, List<RTSUnit>>();
    public List<IntangibleUnitBase<T>> intangibleUnits = new List<IntangibleUnitBase<T>>();

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

    public void AddIntangible(IntangibleUnitBase<T> unit)
    {
        // @TODO
        intangibleUnits.Add(unit);
        int drain = Mathf.RoundToInt((unit.buildCost / unit.buildTime) * 10);
        manaDrainRate += drain;
    }

    public void RemoveIntangible(IntangibleUnitBase<T> unit)
    {
        // @TODO
        intangibleUnits.Remove(unit);
        int drain = Mathf.RoundToInt((unit.buildCost / unit.buildTime) * 10);
        manaDrainRate -= drain;
    }

    public void AddLodestone(LodestoneScript lode)
    {
        totalManaStorage += (int)lode.GetManaStorage();
        totalManaIncome += (int)lode.GetManaIncome();
        lodestones.Add(lode);
    }

    public int GetManaChangeRate()
    {
        return totalManaIncome - manaDrainRate;
    }

    public void AddUnit(RTSUnit unit)
    {
        // Special Add for Lodestones
        if (unit.unitType == UnitCategories.LodestoneTier1 || unit.unitType == UnitCategories.LodestoneTier2)
            AddLodestone(unit.GetComponent<LodestoneScript>());
        // Try if unitType exists in groupedUnits dictionary
        if (groupedUnits.TryGetValue(unit.unitType, out List<RTSUnit> units))
        {
            totalUnits.Add(unit);
            // Initialize a new list for this key, if not already
            if (units == null)
                units = new List<RTSUnit>();
            units.Add(unit);
        }
        else
        {
            totalUnits.Add(unit);
            List<RTSUnit> newUnits = new List<RTSUnit>();
            newUnits.Add(unit);
            // If a unit is added whose type is not yet in the dictionary, assume to add new key-value for it
            groupedUnits.Add(unit.unitType, newUnits);
        }
        // Fire unit change event
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
        // @TODO: remove lodestone 
        totalUnits.Remove(unit);
        if (groupedUnits.TryGetValue(unit.unitType, out List<RTSUnit> units))
            units.Remove(unit);
        else
            Debug.LogWarning("Error removing unit from grouped inventory.");

        OnUnitsChanged?.Invoke(this, new OnInventoryChangedEventArgs
        {
            wasAdded = false,
            unitAffected = unit,
            newTotalUnits = totalUnits,
            newGroupedUnits = groupedUnits
        });
    }
}
