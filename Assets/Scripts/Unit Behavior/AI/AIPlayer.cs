using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIPlayer : MonoBehaviour
{
    private List<BaseUnitScriptAI> totalUnits = new List<BaseUnitScriptAI>();
    private Dictionary<RTSUnit.Categories, List<BaseUnitScriptAI>> groupedUnits = new Dictionary<RTSUnit.Categories, List<BaseUnitScriptAI>>()
    {
        // @TODO
        {RTSUnit.Categories.LodestoneTier1, new List<BaseUnitScriptAI>()},
        {RTSUnit.Categories.Scout, new List<BaseUnitScriptAI>()},
        {RTSUnit.Categories.FactoryTier1, new List<BaseUnitScriptAI>()},
        // {RTSUnit.Categories., new List<RTSUnit>()}, // "Mobile_Builders"
        {RTSUnit.Categories.InfantryTier1, new List<BaseUnitScriptAI>()}, // "Tier_01" e.g. Swordsmen, Crossbowmen, Zombies, Hunters, etc.
        {RTSUnit.Categories.StalwartTier1, new List<BaseUnitScriptAI>()}, // "Infantry_Tier_02" e.g. Titans, Barbarians, Crusaders,
        // {RTSUnit.Categories.Vanguard, new List<BaseUnitScriptAI>()}, // "Infantry_Tier_03" e.g. Taros: Blade Demon, Knights, Amazon Knights
        // {RTSUnit.Categories., new List<RTSUnit>()}, // "Cavalry_Tier_01" e.g. Black Knight, Horseman, 
        {RTSUnit.Categories.Siege, new List<BaseUnitScriptAI>()}, // i.e. long-range shooters; e.g. Fire Demons, Cannons, Balistas, etc.
        {RTSUnit.Categories.Naval, new List<BaseUnitScriptAI>()}
        // Naval -> Naval Builders, Naval Vanguard, Naval Siege, etc.
        // Flying 
    };
    private int unitLimit = 500;
    private RTSUnit.Categories currentNeedType;

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
            public int priority;
            public int count;
            public int limit;
            public RTSUnit.Categories label;
            public float ratio { get { return count / limit; } }
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
        public Item specialInfantry; // like assasin and stuff?
        public Item siegeTier1;
        public Item siegeTier2;
        public Item dragon;
        public Item monarch;
    }
    public MasterQuota profile;

    public enum ProfileTypes
    {
        Balanced, Turtle
    }
    public ProfileTypes profileType;
    public RTSUnit.PlayerNumbers playerNumber;

    // Start is called before the first frame update
    void Start()
    {
        // @TODO: need to be able to start with any unit configuration, e.g. 3 cabals, 1 temple, 10 executioners
        // So, need to compile all units for this team player and group by type, e.g. builders, infantry, fort units, etc.
        GameObject _Units = GameObject.Find("_Units_" + playerNumber);
        foreach (Transform child in _Units.transform)
        {
            // @TODO: group unit by category
            totalUnits.Add(child.gameObject.GetComponent<BaseUnitScriptAI>());
        }

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

    }

    // Update is called once per frame
    void Update()
    {
        if (totalUnits.Count < unitLimit)
        {
            DetermineNeedState();

            if (groupedUnits.TryGetValue(RTSUnit.Categories.FactoryTier1, out List<BaseUnitScriptAI> builderUnits))
            {
                // @TODO: mobile builders vs factories
                // Tell builders to start building some units
                foreach (BaseUnitScriptAI bld in builderUnits)
                {
                    // @TODO: AIs should avoid gates and walls, which belong to FortTier1

                    UnitBuilderAI builderAI = bld.gameObject.GetComponent<UnitBuilderAI>();
                    // If this builder is not already conjuring something
                    if (!builderAI.isBuilding)
                    {
                        UnitBuilderAI.BuildUnit[] buildOptions = builderAI.buildUnitPrefabs;
                        UnitBuilderAI.BuildUnit[] opts = buildOptions.Where(x => x.unitCategory == currentNeedType).ToArray();
                        GameObject toBld = opts[Random.Range(0, opts.Length)].intangiblePrefab;
                        builderAI.QueueBuild(toBld);
                    }
                }

                // Cases:
                // if (builderUnits.Count < builderUnitCountLimit) => tell mobile builders to queue up some Factories
                // if (mobileBuilderUnits.Count < mobileBuilderUnitCountLimit) => tell Factories to queue up some mobile builder
            }
        }

        // @TODO: if an army quota is met, assuming these units are currently roaming around base, tell them now to form up
        // and launch an attack at the average position represented by a snapshot of an opposing army 
    }

    private RTSUnit.Categories DetermineNeedState()
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

            // Lodestones Tier 1
            if (profile.lodestonesTier1.ratio < 0.2f)
                needs.Add(profile.lodestonesTier1);
            // Factories Tier 1
            if (profile.factoriesTier1.ratio < 0.2f)
                needs.Add(profile.factoriesTier1);
            // Fort Tier 1
            if (profile.fortTier1.ratio < 0.2f)
                needs.Add(profile.fortTier1);
            // Naval Tier 1
            if (profile.navalTier1.ratio < 0.2f)
                needs.Add(profile.navalTier1);

            // @TODO: obviously tier 2 things should start taking higher priority as more tier 1 units fill up
            // Fort Tier 2
            if (profile.fortTier2.ratio < 0.2f)
                needs.Add(profile.fortTier2);
            // Factory Tier 2
            if (profile.factoriesTier2.ratio < 0.2f)
                needs.Add(profile.factoriesTier2);
        }
        else
        {
            // Obviously, if no builders exist, creating them takes top priority
            // @TODO: buildersTier1 requires FactoryTier1 tho
            if (profile.factoriesTier1.count > 0)
                needs.Add(profile.buildersTier1);
            else
                needs.Add(profile.factoriesTier1);
        }

        // Needs that require FactoryTier1
        if (profile.factoriesTier1.count > 0)
        {
            // Scout
            // InfantryTier1
            // StalwartTier1
            // SiegeTier1
            // BuilderTier1
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
        currentNeedType = needs[0].label;
        return needs[0].label;
    }

    private MasterQuota NewBalancedProfile()
    {
        return new MasterQuota
        {
            lodestonesTier1 = new MasterQuota.Item
            {
                priority = 2,
                count = 0,
                limit = 7,
                label = RTSUnit.Categories.LodestoneTier1
            },
            lodestonesTier2 = new MasterQuota.Item
            {
                priority = 5,
                count = 0,
                limit = 4,
                label = RTSUnit.Categories.LodestoneTier2
            },
            factoriesTier1 = new MasterQuota.Item
            {
                priority = 1,
                count = 0,
                limit = 5,
                label = RTSUnit.Categories.FactoryTier1
            },

            dragon = new MasterQuota.Item
            {
                priority = 20,
                count = 0,
                limit = 1,
                label = RTSUnit.Categories.Dragon
            }
        };
    }
}
