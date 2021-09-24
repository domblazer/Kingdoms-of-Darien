using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;
using DarienEngine.AI;

public class AIPlayer : MonoBehaviour
{
    private int unitLimit = 500;
    private UnitCategories currentNeed;
    private List<UnitCategories> allCurrentNeeds;

    // @TODO: AI player needs certain quotas to try to reach:
    // e.g. base starting player main quota is like 7 mage builders, 100 infrantry, etc.
    // so need conditions come from like, if 1/7 mage builders, need is high, set that to the need type 
    // so the next time an AI builder is ready to dequeue it builds the current need condition
    // Sub-quotas also have to exist for like armies, like a suitable size force to send when <20% builders/mana whatever
    // is 5-7 infrantry, 1-2 siege or something, so when game is still in an early stage, the quota needed to constitute an
    // army is smaller and increases as mana/builders/other armies increase in count
    public MasterQuota profile;
    public AIProfileTypes profileType;
    public PlayerNumbers playerNumber;
    public TeamNumbers teamNumber;
    public InventoryAI inventory;

    public void Init(InventoryAI inv)
    {
        inventory = inv;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Init profile
        profile = AIProfiles.NewProfile(profileType, inventory);
    }

    // Update is called once per frame
    void Update()
    {
        if (inventory.totalUnits.Count < unitLimit)
        {
            allCurrentNeeds = DetermineNeedState();
            // currentNeed = allCurrentNeeds[0];

            List<RTSUnit> factories = inventory.GetUnitsByTypes(UnitCategories.FactoryTier1, UnitCategories.FactoryTier2);
            List<RTSUnit> builders = inventory.GetUnitsByTypes(UnitCategories.BuilderTier1, UnitCategories.BuilderTier2);

            // Tell factories to start building some units
            foreach (BaseUnitAI factory in factories)
            {
                FactoryAI factoryAI = factory.gameObject.GetComponent<FactoryAI>();
                // If this builder is not already conjuring something, pick a unit that meets the current need
                if (!factoryAI.isBuilding)
                {
                    GameObject unitToBuild = SelectValidUnit(factoryAI.buildUnitPrefabs);
                    if (unitToBuild)
                        factoryAI.QueueBuild(unitToBuild);
                }
            }

            // @TODO: if builders/factories are already working on some units of currentNeed, they can pick from second or third need
            // So maybe: intangibleUnits.Contains(x => x.finalUnit.unitCategory == currentNeedType).Length > 0 ? use needs[1] or needs[2]?

            // Give orders to builders
            foreach (BaseUnitAI builder in builders)
            {
                BuilderAI builderAI = builder.gameObject.GetComponent<BuilderAI>();
                // Only queue builders who are not in a roaming interval, and not in a build routing with at least 1 unit in the queue
                if (!builderAI.isInRoamInterval && !builderAI.isBuilding && !builderAI.baseUnit.isParking && builderAI.masterBuildQueue.Count < 1)
                {
                    GameObject unitToBuild = SelectValidUnit(builderAI.buildUnitPrefabs);
                    if (unitToBuild)
                        builderAI.QueueBuild(unitToBuild);
                }
            }

            // @TODO: AIs should avoid building gates and walls, which belong to FortTier1, also possibly scouts altogether
        }

        // @TODO: if an army quota is met, assuming these units are currently roaming around base, tell them now to form up
        // and launch an attack at the average position represented by a snapshot of an opposing army 
    }

    private GameObject SelectValidUnit(BuildUnit[] fromUnits)
    {
        // Get a unique list of unit categories available in buildUnitPrefabs; HashSet creates unique values
        // @TODO: occurs to me that this should maybe be set on start in baseUnitAi
        HashSet<UnitCategories> availableTypes = new HashSet<UnitCategories>();
        foreach (BuildUnit cat in fromUnits)
            availableTypes.Add(cat.unitCategory);

        // Factories filter out needs that don't pertain to them and return the first element from resulting sequence
        IEnumerable<UnitCategories> adjustedNeeds = allCurrentNeeds.Intersect<UnitCategories>(availableTypes);
        if (adjustedNeeds.Count() > 0)
        {
            UnitCategories adjustedCurrentNeed = adjustedNeeds.First<UnitCategories>();
            // Get all unit prefabs that meet current need
            BuildUnit[] opts = fromUnits.Where(x => x.unitCategory == adjustedCurrentNeed).ToArray();
            // Return a random unit prefab from this option list
            if (opts.Length > 0)
                return opts[Random.Range(0, opts.Length)].intangiblePrefab;
        }
        return null;
    }

    private List<UnitCategories> DetermineNeedState()
    {
        // Compile all needs in list
        List<MasterQuota.Item> needs = new List<MasterQuota.Item>();

        // @TODO: monarch can substitute need for builder at the beginning

        // Getting all quota items upfront eliminates 50% of unnessesary calls to GetQuotaItem()
        AllQuotaItems quotaItems = profile.GetAllQuotaItems();

        // Needs that require BuilderTier1
        if (quotaItems.BuilderTier1.count > 0)
        {
            // @TODO: ratios probably need to change over time so needs can alternate, like once a few lodestones are built,
            // can go ahead and build more factories, armies, etc. but as that capacity increases, so does the lodestone need 
            // again, so now the threshold has to bump up to like .7, repeat, then eventually all thresholds will be 1
            // @TODO: obviously tier 2 things should start taking higher priority as more tier 1 units fill up

            // Maybe ratio follows lodestone ratio? Like as a reflection of mana capacity. still needs more though of course

            // Lodestones Tier 1
            if (quotaItems.LodestoneTier1.ratio < 0.2f)
                needs.Add(quotaItems.LodestoneTier1);
            // Factories Tier 1
            if (quotaItems.FactoryTier1.ratio < 0.2f)
                needs.Add(quotaItems.FactoryTier1);
            // Factory Tier 2
            if (quotaItems.FactoryTier2.ratio < 0.2f)
                needs.Add(quotaItems.FactoryTier2);
            // Fort Tier 1
            if (quotaItems.FortTier1.ratio < 0.2f)
                needs.Add(quotaItems.FortTier1);
            // Fort Tier 2
            if (quotaItems.FortTier2.ratio < 0.2f)
                needs.Add(quotaItems.FortTier2);
            // Naval Tier 1
            if (quotaItems.NavalTier1.ratio < 0.2f)
                needs.Add(quotaItems.NavalTier1);
        }
        else if (quotaItems.FactoryTier1.count > 0 || quotaItems.FactoryTier2.count > 0)
        {
            // Obviously, if no builders exist, creating them takes top priority
            quotaItems.BuilderTier1.priority = 1;
            needs.Add(quotaItems.BuilderTier1);
        }

        // Needs that require FactoryTier1
        if (quotaItems.FactoryTier1.count > 0)
        {
            // Scout? 
            // InfantryTier1
            if (quotaItems.InfantryTier1.ratio < 0.7f)
                needs.Add(quotaItems.InfantryTier1);
            // StalwartTier1
            if (quotaItems.StalwartTier1.ratio < 0.2f)
                needs.Add(quotaItems.StalwartTier1);
            // SiegeTier1
            if (quotaItems.SiegeTier1.ratio < 0.2f)
                needs.Add(quotaItems.SiegeTier1);
            // BuilderTier1
            if (quotaItems.BuilderTier1.ratio < 0.2)
                needs.Add(quotaItems.BuilderTier1);
        }

        // Needs that require FactoryTier2
        if (quotaItems.FactoryTier2.count > 0)
        {
            // InfantryTier2
            // StalwartTier2
            // SiegeTier2
            // BuilderTier2
            // @Note that factoriesTier2 can also build BuildersTier1
        }

        // Needs that require Builders Tier 2 (e.g. Acolyte, Dark Priest, etc)
        if (quotaItems.BuilderTier2.count > 0)
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
        // currentNeedType = needs[0].label;
        // Debug.Log(playerNumber + " current need: " + currentNeedType);
        List<UnitCategories> flattenedNeeds = new List<UnitCategories>();
        foreach (MasterQuota.Item need in needs)
            flattenedNeeds.Add(need.label);
        return flattenedNeeds;
    }
}
