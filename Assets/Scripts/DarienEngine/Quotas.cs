using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DarienEngine
{
    // @DEPRECATED
    /* public class MasterQuota
    {
        public Dictionary<UnitCategories, Item> quota;
        public class Item
        {
            public InventoryAI _inventory;
            public List<RTSUnit> units { get { return _inventory.GetUnitsByType(label); } }
            // public int priority;
            public int count { get { return units.Count; } }
            public UnitCategories label;
            // Target ratio represents ideal distribution of units based on total unit limit
            public float targetRatio;
            // Ratio represents percentage Currently reflected
            public float ratio { get { return (float)count / (float)_inventory.totalUnits.Count; } }
            public float ratioDiff { get { return targetRatio - ratio; } }
            public override string ToString()
            {
                return label.ToString() + " (ratio: " + ratio + ", count: " + count + ", targetRatio: " + targetRatio + ")\n";
            }
        }

        public Item GetQuotaItem(UnitCategories category)
        {
            if (quota.TryGetValue(category, out Item quotaItem))
                return quotaItem;
            return null;
        }

        public AllQuotaItems GetAllQuotaItems()
        {
            return new AllQuotaItems
            {
                Monarch = GetQuotaItem(UnitCategories.Monarch),
                LodestoneTier1 = GetQuotaItem(UnitCategories.LodestoneTier1),
                LodestoneTier2 = GetQuotaItem(UnitCategories.LodestoneTier2),
                FactoryTier1 = GetQuotaItem(UnitCategories.FactoryTier1),
                FactoryTier2 = GetQuotaItem(UnitCategories.FactoryTier2),
                BuilderTier1 = GetQuotaItem(UnitCategories.BuilderTier1),
                BuilderTier2 = GetQuotaItem(UnitCategories.BuilderTier2),
                FortTier1 = GetQuotaItem(UnitCategories.FortTier1),
                FortTier2 = GetQuotaItem(UnitCategories.FortTier2),
                SiegeTier1 = GetQuotaItem(UnitCategories.SiegeTier1),
                SiegeTier2 = GetQuotaItem(UnitCategories.SiegeTier2),
                NavalTier1 = GetQuotaItem(UnitCategories.NavalTier1),
                NavalTier2 = GetQuotaItem(UnitCategories.NavalTier2),
                Dragon = GetQuotaItem(UnitCategories.Dragon),
                Scout = GetQuotaItem(UnitCategories.Scout),
                StalwartTier1 = GetQuotaItem(UnitCategories.StalwartTier1),
                StalwartTier2 = GetQuotaItem(UnitCategories.StalwartTier2),
                InfantryTier1 = GetQuotaItem(UnitCategories.InfantryTier1),
                InfantryTier2 = GetQuotaItem(UnitCategories.InfantryTier2)
            };
        }

        public ArmyQuota armyQuota;
    }

    public class AllQuotaItems
    {
        public MasterQuota.Item Monarch;
        public MasterQuota.Item LodestoneTier1;
        public MasterQuota.Item LodestoneTier2;
        public MasterQuota.Item FactoryTier1;
        public MasterQuota.Item FactoryTier2;
        public MasterQuota.Item BuilderTier1;
        public MasterQuota.Item BuilderTier2;
        public MasterQuota.Item FortTier1;
        public MasterQuota.Item FortTier2;
        public MasterQuota.Item SiegeTier1;
        public MasterQuota.Item SiegeTier2;
        public MasterQuota.Item NavalTier1;
        public MasterQuota.Item NavalTier2;
        public MasterQuota.Item Dragon;
        public MasterQuota.Item Scout;
        public MasterQuota.Item StalwartTier1;
        public MasterQuota.Item StalwartTier2;
        public MasterQuota.Item InfantryTier1;
        public MasterQuota.Item InfantryTier2;

        public override string ToString()
        {
            return Monarch.ToString() +
                LodestoneTier1.ToString() +
                LodestoneTier2.ToString() +
                FactoryTier1.ToString() +
                FactoryTier2.ToString() +
                BuilderTier1.ToString() +
                BuilderTier2.ToString() +
                FortTier1.ToString() +
                FortTier2.ToString() +
                SiegeTier1.ToString() +
                SiegeTier2.ToString() +
                NavalTier1.ToString() +
                NavalTier2.ToString() +
                Dragon.ToString() +
                Scout.ToString() +
                StalwartTier1.ToString() +
                StalwartTier2.ToString() +
                InfantryTier1.ToString() +
                InfantryTier2;
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
    }*/
}
