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

    [System.Serializable]
    public class AIConjurerArgs
    {
        public GameObject nextIntangible;
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
                    [UnitCategories.BuilderTier1] = new MasterQuota.Item
                    {
                        priority = 3,
                        limit = 10,
                        label = UnitCategories.BuilderTier1,
                        _inventory = inv
                    },
                    [UnitCategories.BuilderTier2] = new MasterQuota.Item
                    {
                        priority = 11,
                        limit = 5,
                        label = UnitCategories.BuilderTier2,
                        _inventory = inv
                    },
                    [UnitCategories.Dragon] = new MasterQuota.Item
                    {
                        priority = 99,
                        limit = 1,
                        label = UnitCategories.Dragon,
                        _inventory = inv
                    },
                    [UnitCategories.FactoryTier1] = new MasterQuota.Item
                    {
                        priority = 9,
                        limit = 8,
                        label = UnitCategories.FactoryTier1,
                        _inventory = inv
                    },
                    [UnitCategories.FactoryTier2] = new MasterQuota.Item
                    {
                        priority = 10,
                        limit = 6,
                        label = UnitCategories.FactoryTier2,
                        _inventory = inv
                    },
                    [UnitCategories.FortTier1] = new MasterQuota.Item
                    {
                        priority = 7,
                        limit = 10,
                        label = UnitCategories.FortTier1,
                        _inventory = inv
                    },
                    [UnitCategories.FortTier2] = new MasterQuota.Item
                    {
                        priority = 8,
                        limit = 10,
                        label = UnitCategories.FortTier2,
                        _inventory = inv
                    },
                    [UnitCategories.InfantryTier1] = new MasterQuota.Item
                    {
                        priority = 4,
                        limit = 200,
                        label = UnitCategories.InfantryTier1,
                        _inventory = inv
                    },
                    [UnitCategories.InfantryTier2] = new MasterQuota.Item
                    {
                        priority = 6,
                        limit = 100,
                        label = UnitCategories.InfantryTier2,
                        _inventory = inv
                    },
                    [UnitCategories.LodestoneTier1] = new MasterQuota.Item
                    {
                        priority = 2,
                        limit = 12,
                        label = UnitCategories.LodestoneTier1,
                        _inventory = inv
                    },
                    [UnitCategories.LodestoneTier2] = new MasterQuota.Item
                    {
                        priority = 5,
                        limit = 6,
                        label = UnitCategories.LodestoneTier2,
                        _inventory = inv
                    },
                    [UnitCategories.Monarch] = new MasterQuota.Item
                    {
                        priority = 100,
                        limit = 1,
                        label = UnitCategories.Monarch,
                        _inventory = inv
                    },
                    [UnitCategories.NavalTier1] = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 16,
                        label = UnitCategories.NavalTier1,
                        _inventory = inv
                    },
                    [UnitCategories.NavalTier2] = new MasterQuota.Item
                    {
                        priority = 21,
                        limit = 8,
                        label = UnitCategories.NavalTier2,
                        _inventory = inv
                    },
                    [UnitCategories.Scout] = new MasterQuota.Item
                    {
                        priority = 12,
                        limit = 10,
                        label = UnitCategories.Scout,
                        _inventory = inv
                    },
                    [UnitCategories.SiegeTier1] = new MasterQuota.Item
                    {
                        priority = 13,
                        limit = 10,
                        label = UnitCategories.SiegeTier1,
                        _inventory = inv
                    },
                    [UnitCategories.SiegeTier2] = new MasterQuota.Item
                    {
                        priority = 14,
                        limit = 8,
                        label = UnitCategories.SiegeTier2,
                        _inventory = inv
                    },
                    [UnitCategories.StalwartTier1] = new MasterQuota.Item
                    {
                        priority = 15,
                        limit = 100,
                        label = UnitCategories.StalwartTier1,
                        _inventory = inv
                    },
                    [UnitCategories.StalwartTier2] = new MasterQuota.Item
                    {
                        priority = 17,
                        limit = 50,
                        label = UnitCategories.StalwartTier2,
                        _inventory = inv
                    }
                }
            };
            return masterQuota;
        }
    }
}