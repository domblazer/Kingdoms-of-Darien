using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private List<RTSUnit> totalUnits = new List<RTSUnit>();
    private Dictionary<string, List<RTSUnit>> groupedUnits = new Dictionary<string, List<RTSUnit>>()
    {
        // @TODO
        {"Builders", new List<RTSUnit>()},
        {"Mobile_Builders", new List<RTSUnit>()},
        {"Infantry", new List<RTSUnit>()},
        {"Cavalry", new List<RTSUnit>()},
        {"Artillery", new List<RTSUnit>()}
    };
    private int unitLimit = 500;

    // Start is called before the first frame update
    void Start()
    {
        // @TODO: need to be able to start with any unit configuration, e.g. 3 cabals, 1 temple, 10 executioners
        // So, need to compile all units for this team player and group by type, e.g. builders, infantry, fort units, etc.
        GameObject _Units = GameObject.Find("_Units_Team_2");
        foreach (Transform child in _Units.transform)
        {
            // @TODO: group unit by category
            totalUnits.Add(child.gameObject.GetComponent<RTSUnit>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (totalUnits.Count < unitLimit)
        {
            if (groupedUnits.TryGetValue("Builders", out List<RTSUnit> builderUnits))
            {
                // Tell builders to start building some units
                
                // Cases:
                // if (builderUnits.Count < builderUnitCountLimit) => tell mobile builders to queue up some Factories
                // if (mobileBuilderUnits.Count < mobileBuilderUnitCountLimit) => tell Factories to queue up some mobile builder
            }
        }
    }
}
