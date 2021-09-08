using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;
using DarienEngine.AI;

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
    public MasterQuota profile;
    public AIProfileTypes profileType;
    public PlayerNumbers playerNumber;
    public TeamNumbers teamNumber;

    public void Init(BaseUnitScriptAI[] initialTotalUnits)
    {
        // @TODO: need to be able to start with any unit configuration, e.g. 3 cabals, 1 temple, 10 executioners
        // So, need to compile all units for this team player and group by type, e.g. builders, infantry, fort units, etc.
        foreach (BaseUnitScriptAI unit in initialTotalUnits)
            profile.AddUnit(unit);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Init profile
        profile = AIProfiles.NewProfile(profileType);
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

    public void AddToTotal(BaseUnitScriptAI unitAI)
    {
        // Add this unit to MasterQuota
        profile.AddUnit(unitAI);
    }
}
