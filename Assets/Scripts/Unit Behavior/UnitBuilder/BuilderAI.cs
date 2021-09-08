using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class BuilderAI : BuilderBase<AIConjurerArgs>, IUnitBuilderAI
{
    public BuildUnit[] buildUnitPrefabs;

    public void QueueBuild(GameObject intangiblePrefab)
    {
        Debug.Log("BuilderAI queued: " + intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName);
        if (masterBuildQueue.Count == 0)
            nextQueueReady = true;
        // Enqueue master queue to keep track of build order and total queue
        masterBuildQueue.Enqueue(new AIConjurerArgs { nextIntangible = intangiblePrefab });
    }
}
