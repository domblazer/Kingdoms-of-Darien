using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DarienEngine
{
    public class MasterQuota
    {
        public class Item
        {
            public List<BaseUnitScriptAI> units = new List<BaseUnitScriptAI>();
            public int priority;
            public int count { get { return units.Count; } }
            public int limit;
            public UnitCategories label;
            public float ratio { get { return count / limit; } }
            public bool quotaFull { get { return count == limit; } }
            public override string ToString()
            {
                return label.ToString();
            }
        }
        // @TODO: axe all this Item shit and bind this with InventoryAI
        public Item lodestonesTier1;
        public Item lodestonesTier2;
        public Item factoriesTier1;
        public Item factoriesTier2;
        public Item buildersTier1;
        public Item buildersTier2;
        public Item fortTier1;
        public Item fortTier2;
        public Item navalTier1;
        public Item navalTier2;
        public Item scout;
        public Item infantryTier1;
        public Item infantryTier2;
        public Item stalwartTier1;
        public Item stalwartTier2;
        public Item specialInfantry; // like assasin, grenadier, etc.
        public Item siegeTier1;
        public Item siegeTier2;
        public Item dragon;
        public Item monarch;
        public List<Item> quotaItemList = new List<Item>();
        public int totalUnitsCount { get { return quotaItemList[0] != null ? quotaItemList.Sum(x => x != null ? x.count : 0) : 0; } }

        public ArmyQuota armyQuota;

        public void RefreshQuotaList()
        {
            quotaItemList.Clear();
            quotaItemList.Add(lodestonesTier1);
            quotaItemList.Add(lodestonesTier2);
            quotaItemList.Add(factoriesTier1);
            quotaItemList.Add(factoriesTier2);
            quotaItemList.Add(buildersTier1);
            quotaItemList.Add(buildersTier2);
            quotaItemList.Add(fortTier1);
            quotaItemList.Add(fortTier2);
            quotaItemList.Add(navalTier1);
            quotaItemList.Add(navalTier2);
            quotaItemList.Add(scout);
            quotaItemList.Add(infantryTier1);
            quotaItemList.Add(infantryTier2);
            quotaItemList.Add(stalwartTier1);
            quotaItemList.Add(stalwartTier2);
            quotaItemList.Add(specialInfantry);
            quotaItemList.Add(siegeTier1);
            quotaItemList.Add(siegeTier2);
            quotaItemList.Add(dragon);
            quotaItemList.Add(monarch);
        }

        public void AddUnit(BaseUnitScriptAI unit)
        {
            Item item = quotaItemList.Find(x => x.label == unit.unitType);
            if (item != null && !item.quotaFull)
                item.units.Add(unit);
            else
                Debug.LogWarning("Unit limit for type " + unit.unitType + " has been reached, last unit (" + unit.name + ") should not have been created and was not added to the AI player context.");
        }

        public List<BaseUnitScriptAI> GetUnitsByType(UnitCategories type)
        {
            return quotaItemList.Find(x => x.label == type).units;
        }

        public List<BaseUnitScriptAI> GetUnitsByTypes(params UnitCategories[] types)
        {
            return quotaItemList.Find(x => types.Contains(x.label)).units;
        }

        public List<BaseUnitScriptAI> GetTotalUnits()
        {
            List<BaseUnitScriptAI> total = new List<BaseUnitScriptAI>();
            quotaItemList.ForEach(x => total.Concat(x.units));
            return total;
        }

        public List<Item> ToList()
        {
            return quotaItemList;
        }
    }

    // @TODO: need to have some kind of two-way binding between ArmyQuota and MasterQuota
    public class ArmyQuota
    {
        public class Item
        {
            public int count;
            public int limit;
            public bool quotaFull { get { return count == limit; } }
        }
        public Item infantryTier1;
        public Item infantryTier2;
        public Item stalwartTier1;
        public Item stalwartTier2;
        public Item specialInfantry;
        public Item siegeTier1;
        public Item siegeTier2;

        public bool Ready()
        {
            return infantryTier1.quotaFull && infantryTier2.quotaFull && stalwartTier1.quotaFull &&
                stalwartTier2.quotaFull && specialInfantry.quotaFull && siegeTier1.quotaFull && siegeTier2.quotaFull;
        }
    }
}
