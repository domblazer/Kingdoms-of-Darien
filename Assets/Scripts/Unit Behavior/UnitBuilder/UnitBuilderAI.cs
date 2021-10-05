using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

public class UnitBuilderAI : UnitBuilderBase
{
    public BuildUnit[] buildUnitPrefabs;

    public void QueueBuild(GameObject intangiblePrefab)
    {
        // Debug.Log("UnitBuilderAI queued: " + intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName);
        if (!baseUnit.commandQueue.IsEmpty())
            nextQueueReady = true;
        // Enqueue master queue to keep track of build order and total queue
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            conjurerArgs = new ConjurerArgs { prefab = intangiblePrefab }
        });
    }
}
