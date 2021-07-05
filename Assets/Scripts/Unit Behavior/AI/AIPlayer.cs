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
        {RTSUnit.Categories.Lodestone, new List<BaseUnitScriptAI>()},
        {RTSUnit.Categories.Scout, new List<BaseUnitScriptAI>()},
        {RTSUnit.Categories.Factory, new List<BaseUnitScriptAI>()},
        // {RTSUnit.Categories., new List<RTSUnit>()}, // "Mobile_Builders"
        {RTSUnit.Categories.Tier1, new List<BaseUnitScriptAI>()}, // "Tier_01" e.g. Swordsmen, Crossbowmen, Zombies, Hunters, etc.
        {RTSUnit.Categories.Stalwart, new List<BaseUnitScriptAI>()}, // "Infantry_Tier_02" e.g. Titans, Barbarians, Crusaders,
        {RTSUnit.Categories.Vanguard, new List<BaseUnitScriptAI>()}, // "Infantry_Tier_03" e.g. Taros: Blade Demon, Knights, Amazon Knights
        // {RTSUnit.Categories., new List<RTSUnit>()}, // "Cavalry_Tier_01" e.g. Black Knight, Horseman, 
        {RTSUnit.Categories.Siege, new List<BaseUnitScriptAI>()}, // i.e. long-range shooters; e.g. Fire Demons, Cannons, Balistas, etc.
        {RTSUnit.Categories.Naval, new List<BaseUnitScriptAI>()}
        // Naval -> Naval Builders, Naval Vanguard, Naval Siege, etc.
        // Flying 
    };
    private int unitLimit = 500;
    public RTSUnit.Categories currentNeedType = RTSUnit.Categories.Tier1;

    // Start is called before the first frame update
    void Start()
    {
        // @TODO: need to be able to start with any unit configuration, e.g. 3 cabals, 1 temple, 10 executioners
        // So, need to compile all units for this team player and group by type, e.g. builders, infantry, fort units, etc.
        GameObject _Units = GameObject.Find("_Units_Team_2");
        foreach (Transform child in _Units.transform)
        {
            // @TODO: group unit by category
            totalUnits.Add(child.gameObject.GetComponent<BaseUnitScriptAI>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (totalUnits.Count < unitLimit)
        {
            if (groupedUnits.TryGetValue(RTSUnit.Categories.Factory, out List<BaseUnitScriptAI> builderUnits))
            {
                // Tell builders to start building some units
                foreach (BaseUnitScriptAI bld in builderUnits)
                {
                    // @TODO: mobile builders vs factories
                    UnitBuilderAI builderAI = bld.gameObject.GetComponent<UnitBuilderAI>();
                    UnitBuilderAI.BuildUnit[] buildOptions = builderAI.buildUnitPrefabs;
                    UnitBuilderAI.BuildUnit[] opts = buildOptions.Where(x => x.unitCategory == currentNeedType).ToArray();
                    GameObject toBld = opts[Random.Range(0, opts.Length)].intangiblePrefab;
                    builderAI.QueueBuild(toBld);
                }

                // Cases:
                // if (builderUnits.Count < builderUnitCountLimit) => tell mobile builders to queue up some Factories
                // if (mobileBuilderUnits.Count < mobileBuilderUnitCountLimit) => tell Factories to queue up some mobile builder
            }
        }
    }
}
