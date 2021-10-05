using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

public class BuilderAI : UnitBuilderAI
{
    private float searchBuildRange = 15.0f;
    public float roamIntervalTime = 30.0f;
    public bool isInRoamInterval { get; set; } = false;
    private float timeRemaining = 0;

    private void Update()
    {
        // If isInRoamInterval, count down roamIntervalTime then set to false
        if (isInRoamInterval && timeRemaining >= 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0)
                isInRoamInterval = false;
        }

        // @TODO: builder should start in patrol state after parking before starting building

        if (!baseUnit.commandQueue.IsEmpty() && nextQueueReady && !isInRoamInterval)
        {
            ConjurerArgs buildArgs = baseUnit.currentCommand.conjurerArgs;

            // Vector3.zero means buildSpot is "null"
            if (buildArgs.buildSpot == Vector3.zero)
                buildArgs.buildSpot = FindBuildSpot(transform.position);

            // @TODO: intangible unit offset
            if (!baseUnit.IsInRangeOf(buildArgs.buildSpot, 2))
                baseUnit.SetMove(buildArgs.buildSpot);
            else
                StartNextIntangible(buildArgs);
        }
        else if (!isBuilding && isInRoamInterval)
        {
            // If builderAI is not in a build routine, just have it roam around
            baseUnit.state = RTSUnit.States.Patrolling;
        }
    }

    // Called once upon arrival at next build position
    private void StartNextIntangible(ConjurerArgs buildArgs)
    {
        baseUnit.SetMove(transform.position);
        baseUnit.state = RTSUnit.States.Conjuring;
        nextQueueReady = false;
        isBuilding = true;
        baseUnit.commandQueue.Dequeue();
        InstantiateNextIntangible(buildArgs.prefab, buildArgs.buildSpot);
    }

    public Vector3 FindBuildSpot(Vector3 origin)
    {
        // @TODO: GenerateValidRandomPoint will handle most validation, but still need more specific point
        // picking for e.g. Lodestones which can only go on sacred sites. Also, can't go outside a certain bounds 
        // from the player start position (or base position?). Also, factories should be loosely clustered and defenses
        // built around them and near Lodestones. etc. etc.
        return (baseUnit as BaseUnitAI).GenerateValidRandomPoint(origin, searchBuildRange);
    }

    private void InstantiateNextIntangible(GameObject itg, Vector3 spawnPoint)
    {
        GameObject intangible = Instantiate(itg, spawnPoint, new Quaternion(0, 180, 0, 1));
        intangible.GetComponent<IntangibleUnitAI>().Bind(this, null, new CommandQueueItem
        {
            commandType = CommandTypes.Patrol,
            patrolRoute = null // @Note: AI will set random patrol (roam) points automatically when this is null
        });
        intangible.GetComponent<IntangibleUnitAI>().Callback(NextIntangibleCompleted);
    }

    // Called back when intangible is complete
    private void NextIntangibleCompleted()
    {
        // Builder should go into roam routine after intangible is done
        isInRoamInterval = true;
        // Reset timer for roaming interval
        timeRemaining = roamIntervalTime;
        // Set new patrol points for roam
        // @TODO: use CommandQueue to reset patrol points. New patrol route?
        (baseUnit as BaseUnitAI).SetPatrolPoints(transform.position, 3, searchBuildRange);
    }
}
