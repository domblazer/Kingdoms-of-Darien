using UnityEngine;
using System.Collections.Generic;

namespace DarienEngine.AI
{
    public enum AIProfileTypes
    {
        Balanced, Turtle
    }

    public class AIPlayerContext
    {
        public GameObject holder;
        public AIPlayer player;
        public InventoryAI inventory;
        public TeamNumbers team;
    }

    [System.Serializable]
    public class BuildUnit
    {
        public UnitCategories unitCategory;
        public GameObject intangiblePrefab;
        public override string ToString()
        {
            return intangiblePrefab ? intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName : "NULL";
        }
    }

    public interface IUnitBuilderAI
    {
        void QueueBuild(GameObject intangiblePrefab);
    }

    public static class AIProfiles
    {
        public static MasterQuota NewProfile(AIProfileTypes profileType, InventoryAI inv)
        {
            switch (profileType)
            {
                case AIProfileTypes.Balanced:
                    return NewBalancedProfile(inv);
                case AIProfileTypes.Turtle:

                    break;
            }
            return NewBalancedProfile(inv);
        }

        private static MasterQuota NewBalancedProfile(InventoryAI inv)
        {
            MasterQuota masterQuota = new MasterQuota
            {
                quota = new Dictionary<UnitCategories, MasterQuota.Item>()
                {
                    [UnitCategories.LodestoneTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.02f,
                        label = UnitCategories.LodestoneTier1,
                        _inventory = inv
                    },
                    [UnitCategories.LodestoneTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.01f,
                        label = UnitCategories.LodestoneTier2,
                        _inventory = inv
                    },
                    [UnitCategories.FactoryTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.01f,
                        label = UnitCategories.FactoryTier1,
                        _inventory = inv
                    },
                    [UnitCategories.FactoryTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.01f,
                        label = UnitCategories.FactoryTier2,
                        _inventory = inv
                    },
                    [UnitCategories.FortTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.015f,
                        label = UnitCategories.FortTier1,
                        _inventory = inv
                    },
                    [UnitCategories.FortTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.01f,
                        label = UnitCategories.FortTier2,
                        _inventory = inv
                    },
                    [UnitCategories.InfantryTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.5f,
                        label = UnitCategories.InfantryTier1,
                        _inventory = inv
                    },
                    [UnitCategories.InfantryTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.25f,
                        label = UnitCategories.InfantryTier2,
                        _inventory = inv
                    },
                    [UnitCategories.BuilderTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.014f,
                        label = UnitCategories.BuilderTier1,
                        _inventory = inv
                    },
                    [UnitCategories.BuilderTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.006f,
                        label = UnitCategories.BuilderTier2,
                        _inventory = inv
                    },
                    [UnitCategories.StalwartTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.01f,
                        label = UnitCategories.StalwartTier1,
                        _inventory = inv
                    },
                    [UnitCategories.StalwartTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.01f,
                        label = UnitCategories.StalwartTier2,
                        _inventory = inv
                    },
                    [UnitCategories.SiegeTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.05f,
                        label = UnitCategories.SiegeTier1,
                        _inventory = inv
                    },
                    [UnitCategories.SiegeTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.02f,
                        label = UnitCategories.SiegeTier2,
                        _inventory = inv
                    },
                    [UnitCategories.Dragon] = new MasterQuota.Item
                    {
                        targetRatio = 0.005f,
                        label = UnitCategories.Dragon,
                        _inventory = inv
                    },
                    [UnitCategories.Monarch] = new MasterQuota.Item
                    {
                        targetRatio = 0.005f,
                        label = UnitCategories.Monarch,
                        _inventory = inv
                    },
                    [UnitCategories.NavalTier1] = new MasterQuota.Item
                    {
                        targetRatio = 0.0075f,
                        label = UnitCategories.NavalTier1,
                        _inventory = inv
                    },
                    [UnitCategories.NavalTier2] = new MasterQuota.Item
                    {
                        targetRatio = 0.0075f,
                        label = UnitCategories.NavalTier2,
                        _inventory = inv
                    },
                    [UnitCategories.Scout] = new MasterQuota.Item
                    {
                        targetRatio = 0.005f,
                        label = UnitCategories.Scout,
                        _inventory = inv
                    }
                }
            };
            return masterQuota;
        }
    }
}