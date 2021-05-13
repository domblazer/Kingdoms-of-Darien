using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScript
{
    public int totalMana;
    public int currentMana;
    public int manaDrainRate;
    public int totalManaIncome = 0;

    public List<LodestoneScript> lodestones = new List<LodestoneScript>();

    public InventoryScript()
    {
        totalMana = 10; // Everyone starts with at least 10 mana
    }

    public void PlusTotalMana(int mana)
    {
        totalMana += mana;
    }

    public void MinusTotalMana(int mana)
    {
        totalMana -= mana;
    }

    public void PlusCurrentMana(int mana)
    {
        currentMana += mana;
    }

    public void MinusCurrentMana(int mana)
    {
        if (currentMana >= 0)
        {
            currentMana -= mana;
        }

    }

    public void AddLodestone(LodestoneScript lode)
    {
        totalMana += (int)lode.GetManaStorage();
        totalManaIncome += (int)lode.GetManaIncome();
        lodestones.Add(lode);
    }
}
