using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

public class UnitBuilderAI : UnitBuilderBase
{
    public BuildUnit[] buildUnitPrefabs;
    public HashSet<UnitCategories> availableTypes { get; set; }

    private void Start()
    {
        availableTypes = new HashSet<UnitCategories>();
        // Build a unique hash set of need categories supported by this unit's build options. Used in AIPlayer
        foreach (BuildUnit cat in buildUnitPrefabs)
            availableTypes.Add(cat.unitCategory);
    }

    public void QueueBuild(BuildUnit buildUnit)
    {
        // Debug.Log("UnitBuilderAI queued: " + intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName);
        if (baseUnit.commandQueue.IsEmpty())
            nextQueueReady = true;
        // Enqueue master queue to keep track of build order and total queue
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            conjurerArgs = new ConjurerArgs { prefab = buildUnit.intangiblePrefab, unitCategory = buildUnit.unitCategory }
        });
    }
}
