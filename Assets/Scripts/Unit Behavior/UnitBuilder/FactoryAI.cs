using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

/// <summary>
/// Class <c>FactoryAI</c> represents an AI unit that can build but cannot move.
/// </summary>
public class FactoryAI : UnitBuilderAI
{
    public Transform spawnPoint;
    public Transform rallyPoint;
    protected bool parkingDirectionToggle = false;

    public void HandleConjureRoutine()
    {
        // Keep track of master queue to know when building
        IsBuilding = !BaseUnit.commandQueue.IsEmpty();
        // Debug.Log("FactoryAI: isBuilding: " + isBuilding + " nextQueueReady: " + nextQueueReady);
        // While masterQueue is not empty, continue queueing up intangible prefabs
        if (NextQueueReady)
        {
            // @TODO: also need to check that the spawn point is clear before moving on to next unit
            GameObject nextItg = BaseUnit.currentCommand.conjurerArgs.prefab;
            InstantiateNextIntangible(nextItg);
            NextQueueReady = false;
        }
    }

    private void InstantiateNextIntangible(GameObject itg)
    {
        GameObject intangible = Instantiate(itg, spawnPoint.position, new Quaternion(0, 180, 0, 1));
        intangible.GetComponent<IntangibleUnitAI>().BindBuilder(
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
        BaseUnit.commandQueue.Dequeue();
        IsBuilding = false;
    }
}
