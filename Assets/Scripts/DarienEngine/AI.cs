namespace DarienEngine
{
    namespace AI
    {
        public enum AIProfileTypes
        {
            Balanced, Turtle
        }

        public static class AIProfiles
        {
            public static MasterQuota NewProfile(AIProfileTypes profileType)
            {
                switch (profileType)
                {
                    case AIProfileTypes.Balanced:
                        return NewBalancedProfile();
                    case AIProfileTypes.Turtle:

                        break;
                }
                return NewBalancedProfile();
            }

            private static MasterQuota NewBalancedProfile()
            {
                MasterQuota masterQuota = new MasterQuota
                {
                    lodestonesTier1 = new MasterQuota.Item
                    {
                        priority = 2,
                        limit = 8,
                        label = UnitCategories.LodestoneTier1
                    },
                    lodestonesTier2 = new MasterQuota.Item
                    {
                        priority = 5,
                        limit = 4,
                        label = UnitCategories.LodestoneTier2
                    },
                    factoriesTier1 = new MasterQuota.Item
                    {
                        priority = 1,
                        limit = 5,
                        label = UnitCategories.FactoryTier1
                    },
                    factoriesTier2 = new MasterQuota.Item
                    {
                        priority = 1,
                        limit = 8,
                        label = UnitCategories.FactoryTier2
                    },
                    buildersTier1 = new MasterQuota.Item
                    {
                        priority = 3,
                        limit = 10,
                        label = UnitCategories.BuilderTier1
                    },
                    buildersTier2 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 10,
                        label = UnitCategories.BuilderTier2
                    },
                    fortTier1 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 10,
                        label = UnitCategories.FortTier1
                    },
                    fortTier2 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 10,
                        label = UnitCategories.FortTier2
                    },
                    navalTier1 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 10,
                        label = UnitCategories.NavalTier1
                    },
                    navalTier2 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 10,
                        label = UnitCategories.NavalTier2
                    },
                    scout = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 5,
                        label = UnitCategories.Scout
                    },
                    infantryTier1 = new MasterQuota.Item
                    {
                        priority = 19,
                        limit = 300,
                        label = UnitCategories.InfantryTier1
                    },
                    infantryTier2 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 70,
                        label = UnitCategories.InfantryTier2
                    },
                    stalwartTier1 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 35,
                        label = UnitCategories.StalwartTier1
                    },
                    stalwartTier2 = new MasterQuota.Item
                    {
                        priority = 20,
                        limit = 25,
                        label = UnitCategories.StalwartTier2
                    },
                    // specialInfantry?
                    siegeTier1 = new MasterQuota.Item
                    {
                        priority = 21,
                        limit = 15,
                        label = UnitCategories.SiegeTier1
                    },
                    siegeTier2 = new MasterQuota.Item
                    {
                        priority = 21,
                        limit = 10,
                        label = UnitCategories.SiegeTier2
                    },
                    monarch = new MasterQuota.Item
                    {
                        priority = 29,
                        limit = 1,
                        label = UnitCategories.Monarch
                    },
                    dragon = new MasterQuota.Item
                    {
                        priority = 3,
                        limit = 1,
                        label = UnitCategories.Dragon
                    }
                };
                masterQuota.RefreshQuotaList();
                return masterQuota;
            }
        }
    }
}