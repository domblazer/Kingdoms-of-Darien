using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class FactoryAI : UnitBuilderAI
{
    public Transform spawnPoint;
    public Transform rallyPoint;
    protected bool parkingDirectionToggle = false;

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
        intangible.GetComponent<IntangibleUnitAI>().Bind(this, rallyPoint, RTSUnit.States.Patrolling, parkingDirectionToggle);
        intangible.GetComponent<IntangibleUnitAI>().Callback(NextIntangibleCompleted);
    }

    // Callback when intangible is complete
    private void NextIntangibleCompleted()
    {
        // Factory just needs to dequeue on intangible complete
        masterBuildQueue.Dequeue();
    }
}
