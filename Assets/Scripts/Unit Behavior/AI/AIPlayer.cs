using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Constants;

public class AIPlayer : MonoBehaviour
{
    private int unitLimit = 500;
    private UnitCategories currentNeedType;

    // @TODO: AI player needs certain quotas to try to reach:
    // e.g. base starting player main quota is like 7 mage builders, 100 infrantry, etc.
    // so need conditions come from like, if 1/7 mage builders, need is high, set that to the need type 
    // so the next time an AI builder is ready to dequeue it builds the current need condition
    // Sub-quotas also have to exist for like armies, like a suitable size force to send when <20% builders/mana whatever
    // is 5-7 infrantry, 1-2 siege or something, so when game is still in an early stage, the quota needed to constitute an
    // army is smaller and increases as mana/builders/other armies increase in count
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
    public MasterQuota profile;

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

    public enum ProfileTypes
    {
        Balanced, Turtle
    }
    public ProfileTypes profileType;
    public PlayerNumbers playerNumber;

    public void Init(BaseUnitScriptAI[] initialTotalUnits)
    {
        foreach (BaseUnitScriptAI unit in initialTotalUnits)
            profile.AddUnit(unit);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Init profile
        switch (profileType)
        {
            case ProfileTypes.Balanced:
                profile = NewBalancedProfile();
                break;
            default:
                profile = NewBalancedProfile();
                break;
        }

        // @TODO: need to be able to start with any unit configuration, e.g. 3 cabals, 1 temple, 10 executioners
        // So, need to compile all units for this team player and group by type, e.g. builders, infantry, fort units, etc.
        // Init units at start
        /* if (GameManagerScript.Instance.TryGetVirtualPlayer(playerNumber, out GameManagerScript.VirtualPlayer vp))
        {
            foreach (Transform child in vp.holder.transform)
                profile.AddUnit(child.gameObject.GetComponent<BaseUnitScriptAI>());
        } */

    }

    // Update is called once per frame
    void Update()
    {
        if (profile.totalUnitsCount < unitLimit)
        {
            DetermineNeedState();

            List<BaseUnitScriptAI> factories = profile.GetUnitsByTypes(UnitCategories.FactoryTier1, UnitCategories.FactoryTier2);
            // @TODO: if no builders?
            // @TODO: mobile builders vs factories

            // Tell builders to start building some units
            foreach (BaseUnitScriptAI factory in factories)
            {
                FactoryAI builderAI = factory.gameObject.GetComponent<FactoryAI>();
                if (!builderAI.isBuilding)
                {
                    // If this builder is not already conjuring something, pick a unit that meets the current need
                    BuildUnit[] opts = builderAI.buildUnitPrefabs.Where(x => x.unitCategory == currentNeedType).ToArray();
                    Debug.Log("opts: " + string.Join<BuildUnit>(", ", opts.ToArray()));
                    if (opts.Length > 0)
                    {
                        // Choose an individual unit by type at random to build next
                        GameObject toBld = opts[Random.Range(0, opts.Length)].intangiblePrefab;
                        builderAI.QueueBuild(toBld);
                    }
                }
            }

            // @TODO: AIs should avoid building gates and walls, which belong to FortTier1, also possibly scouts altogether

            // Cases:
            // if (builderUnits.Count < builderUnitCountLimit) => tell mobile builders to queue up some Factories
            // if (mobileBuilderUnits.Count < mobileBuilderUnitCountLimit) => tell Factories to queue up some mobile builder
        }

        // @TODO: if an army quota is met, assuming these units are currently roaming around base, tell them now to form up
        // and launch an attack at the average position represented by a snapshot of an opposing army 
    }

    private UnitCategories DetermineNeedState()
    {
        // Compile all needs in list
        List<MasterQuota.Item> needs = new List<MasterQuota.Item>();

        // @TODO: monarch can substitute need for builder at the beginning

        // Needs that require BuilderTier1
        if (profile.buildersTier1.count > 0)
        {
            // @TODO: ratios probably need to change over time so needs can alternate, like once a few lodestones are built,
            // can go ahead and build more factories, armies, etc. but as that capacity increases, so does the lodestone need 
            // again, so now the threshold has to bump up to like .7, repeat, then eventually all thresholds will be 1
            // @TODO: obviously tier 2 things should start taking higher priority as more tier 1 units fill up

            // Maybe ratio follows lodestone ratio? Like as a reflection of mana capacity. still needs more though of course

            // Lodestones Tier 1
            if (profile.lodestonesTier1.ratio < 0.2f)
                needs.Add(profile.lodestonesTier1);
            // Factories Tier 1
            if (profile.factoriesTier1.ratio < 0.2f)
                needs.Add(profile.factoriesTier1);
            // Factory Tier 2
            if (profile.factoriesTier2.ratio < 0.2f)
                needs.Add(profile.factoriesTier2);
            // Fort Tier 1
            if (profile.fortTier1.ratio < 0.2f)
                needs.Add(profile.fortTier1);
            // Fort Tier 2
            if (profile.fortTier2.ratio < 0.2f)
                needs.Add(profile.fortTier2);
            // Naval Tier 1
            if (profile.navalTier1.ratio < 0.2f)
                needs.Add(profile.navalTier1);
        }
        else if (profile.factoriesTier1.count > 0 || profile.factoriesTier2.count > 0)
        {
            // Obviously, if no builders exist, creating them takes top priority
            needs.Add(profile.buildersTier1);
        }

        // Needs that require FactoryTier1
        if (profile.factoriesTier1.count > 0)
        {
            // Scout? 
            // InfantryTier1
            if (profile.infantryTier1.ratio < 0.2f)
                needs.Add(profile.infantryTier1);
            // StalwartTier1
            if (profile.stalwartTier1.ratio < 0.2f)
                needs.Add(profile.stalwartTier1);
            // SiegeTier1
            if (profile.siegeTier1.ratio < 0.2f)
                needs.Add(profile.siegeTier1);
        }

        // Needs that require FactoryTier2
        if (profile.factoriesTier2.count > 0)
        {
            // InfantryTier2
            // StalwartTier2
            // SiegeTier2
            // BuilderTier2
            // @Note that factoriesTier2 can also build BuildersTier1
        }

        // Needs that require Builders Tier 2 (e.g. Acolyte, Dark Priest, etc)
        if (profile.buildersTier2.count > 0)
        {
            // Dragon
            // Lodestone Tier 2
            // Special? i.e. Grenadier?
        }

        // Sort needs by priority and return highest priority need
        needs.Sort(delegate (MasterQuota.Item x, MasterQuota.Item y)
        {
            return x.priority > y.priority ? 1 : -1;
        });
        Debug.Log("All needs: " + string.Join<MasterQuota.Item>(", ", needs.ToArray()));
        currentNeedType = needs[0].label;
        Debug.Log(playerNumber + " current need: " + currentNeedType);
        return needs[0].label;
    }

    private MasterQuota NewBalancedProfile()
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

    public void AddToTotal(BaseUnitScriptAI unitAI)
    {
        // Add this unit to MasterQuota
        profile.AddUnit(unitAI);
    }
}
