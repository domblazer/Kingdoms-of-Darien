using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class FactoryAI : UnitBuilderAI
{
    public Transform spawnPoint;
    public Transform rallyPoint;
    protected bool parkingDirectionToggle = false;

    public void HandleConjureRoutine()
    {
        // Keep track of master queue to know when building
        isBuilding = !baseUnit.commandQueue.IsEmpty();

        Debug.Log("FactoryAI: isBuilding: " + isBuilding + " nextQueueReady: " + nextQueueReady);

        // While masterQueue is not empty, continue queueing up intangible prefabs
        if (nextQueueReady)
        {
            // @TODO: also need to check that the spawn point is clear before moving on to next unit
            GameObject nextItg = baseUnit.currentCommand.conjurerArgs.prefab;
            InstantiateNextIntangible(nextItg);
            nextQueueReady = false;
        }
    }

    private void InstantiateNextIntangible(GameObject itg)
    {
        GameObject intangible = Instantiate(itg, spawnPoint.position, new Quaternion(0, 180, 0, 1));
        intangible.GetComponent<IntangibleUnitAI>().Bind(
            this,
            rallyPoint,
            new CommandQueueItem { commandType = CommandTypes.Patrol },
            parkingDirectionToggle
        );
        intangible.GetComponent<IntangibleUnitAI>().Callback(IntangibleCompleted);
    }

    // Callback when intangible is complete
    private void IntangibleCompleted()
    {
        // Factory just needs to dequeue on intangible complete
        baseUnit.commandQueue.Dequeue();
    }
}
