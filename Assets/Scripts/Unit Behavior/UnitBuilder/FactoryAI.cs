using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class FactoryAI : FactoryBase<AIConjurerArgs>, IUnitBuilderAI
{
    public BuildUnit[] buildUnitPrefabs;

    private void Update()
    {
        // Keep track of master queue to know when building
        isBuilding = masterBuildQueue.Count > 0;

        // While masterQueue is not empty, continue queueing up intangible prefabs
        if (masterBuildQueue.Count > 0 && nextQueueReady)
        {
            // @TODO: also need to check that the spawn point is clear before moving on to next unit
            baseUnit.state = RTSUnit.States.Conjuring;
            GameObject nextItg = masterBuildQueue.Peek().nextIntangible;
            InstantiateNextIntangible(nextItg);
            nextQueueReady = false;
        }

    }

    private void InstantiateNextIntangible(GameObject itg)
    {
        GameObject intangible = Instantiate(itg, spawnPoint.position, new Quaternion(0, 180, 0, 1));
        // @TODO: this itg needs to tell it's final prefab to park then just start roaming around the park point
        intangible.GetComponent<IntangibleUnitAI>().Bind(this, rallyPoint, parkingDirectionToggle);
    }

    public void QueueBuild(GameObject intangiblePrefab)
    {
        Debug.Log("FactoryAI queued: " + intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName);
        if (masterBuildQueue.Count == 0)
            nextQueueReady = true;
        // Enqueue master queue to keep track of build order and total queue
        masterBuildQueue.Enqueue(new AIConjurerArgs { nextIntangible = intangiblePrefab });
    }
}
