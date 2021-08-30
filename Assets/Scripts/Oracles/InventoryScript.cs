using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript : MonoBehaviour
{
    public int totalManaIncome = 0;
    public int totalManaStorage = 10;
    public int currentMana = 0;
    public int manaDrainRate = 0;

    public List<LodestoneScript> lodestones = new List<LodestoneScript>();
    public List<RTSUnit> totalUnits = new List<RTSUnit>();
    public List<IntangibleUnitScript> intangibleUnits = new List<IntangibleUnitScript>();

    public InventoryScript()
    {
        totalManaStorage = 10; // Everyone starts with at least 10 mana
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

    public void AddIntangible(IntangibleUnitScript unit)
    {
        intangibleUnits.Add(unit);
        int drain = Mathf.RoundToInt((unit.buildCost / unit.buildTime) * 10);
        manaDrainRate += drain;
    }

    public void RemoveIntangible(IntangibleUnitScript unit)
    {
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
}
