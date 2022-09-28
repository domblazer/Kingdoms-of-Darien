using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

/// <summary>
/// Class <c>BuilderAI</c> represents an AI unit that can build and can move.
/// </summary>
public class BuilderAI : UnitBuilderAI
{
    private float searchBuildRange = 15.0f;
    public float roamIntervalTime = 30.0f;
    public bool isInRoamInterval { get; set; } = false;
    private float timeRemaining = 0;

    private Vector3 nextIntangibleOffset = Vector3.zero;

    private void Update()
    {
        // If isInRoamInterval, count down roamIntervalTime then set to false
        if (isInRoamInterval && timeRemaining >= 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0)
            {
                isInRoamInterval = false;
                // When roam interval is done, dequeue the patrol command, this will satisfy the next build condition in AIPlayer
                baseUnit.commandQueue.Dequeue();
            }
        }
    }

    public void HandleConjureRoutine()
    {
        if (nextQueueReady && !isInRoamInterval)
        {
            ConjurerArgs buildArgs = baseUnit.currentCommand.conjurerArgs;
            // Calculate the intangible's offset once every new queue. @Note. can't ref IntangibleUnitBase b/c it hasn't been instantiated
            if (nextIntangibleOffset == Vector3.zero)
                nextIntangibleOffset = CalculateIntangibleOffset(buildArgs.prefab);
            // If build unit is lodestone, FindBuildSpot will pick nearest sacred site
            bool isLodestone = buildArgs.unitCategory == UnitCategories.LodestoneTier1 || buildArgs.unitCategory == UnitCategories.LodestoneTier2;
            // Vector3.zero means buildSpot is "null"
            if (buildArgs.buildSpot == Vector3.zero)
                buildArgs.buildSpot = FindBuildSpot(transform.position, isLodestone);
            // @TODO: calculate mix of x and y offset
            if (!baseUnit.IsInRangeOf(buildArgs.buildSpot, nextIntangibleOffset.x))
                baseUnit.MoveToPosition(buildArgs.buildSpot);
            else
                StartNextIntangible(buildArgs);
        }
    }

    private Vector3 CalculateIntangibleOffset(GameObject gameObj)
    {
        // @TODO: need to separate the collider that determines build size vs the collider that picks up hits
        Vector3 offset = new Vector3();
        if (gameObj.GetComponent<BoxCollider>())
            offset = gameObj.GetComponent<BoxCollider>().size;
        else if (gameObj.GetComponent<CapsuleCollider>())
        {
            float r = gameObj.GetComponent<CapsuleCollider>().radius;
            offset = new Vector3(r, r, r);
        }
        return offset;
    }

    // Called once upon arrival at next build position
    private void StartNextIntangible(ConjurerArgs buildArgs)
    {
        baseUnit.MoveToPosition(transform.position);
        nextQueueReady = false;
        isBuilding = true;
        InstantiateNextIntangible(buildArgs.prefab, buildArgs.buildSpot);
    }

    // Return the closest SacredSite position
    public Vector3 FindClosestSacredSite()
    {
        // @TODO: all sacred sites should be found and compiled at beginning of game, maybe in gamemanager?
        GameObject[] sacredStones = GameObject.FindGameObjectsWithTag("SacredSite");
        float closestDistance = Mathf.Infinity;
        GameObject closestStone = null;
        foreach (GameObject stone in sacredStones)
        {
            float distance = (transform.position - stone.transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStone = stone;
            }
        }
        Vector3 p = closestStone.transform.position;
        Vector3 pos = new Vector3(p.x, p.y + 0.1f, p.z);
        return pos;
    }

    public Vector3 FindBuildSpot(Vector3 origin, bool forLodestone = false)
    {
        // @TODO: TOP PRIORITIES:
        // 1. builders cannot build outside the map bounds:: DONE
        // 2. @TODO: cannot place buildings that overlap other units or obstacles
        // 3. @TODO: builders must build factories in relatively central groupings
        //      Idea: use Physics.OverlapBox(origin, toBuild.offset * 2, Quaternion.identity, (1 << 9) | (1 << 11));
        //      @TODO: box cannot also cover more invalid surface area than not, i.e. if more than half the structure 
        //      is going to sit in an invalid area, the build spot is not valid
        // 4. @TODO: an improvement that could be made is also not testing points around the failed areas again
        return forLodestone ? FindClosestSacredSite() :
            (baseUnit as BaseUnitAI).GenerateValidBuildPoint(origin, searchBuildRange, nextIntangibleOffset);
    }

    private void InstantiateNextIntangible(GameObject itg, Vector3 spawnPoint)
    {
        GameObject intangible = Instantiate(itg, spawnPoint, new Quaternion(0, 180, 0, 1));
        intangible.GetComponent<IntangibleUnitAI>().Bind(this, null, new CommandQueueItem { commandType = CommandTypes.Patrol });
        intangible.GetComponent<IntangibleUnitAI>().Callback(IntangibleCompleted);
    }

    // Called back when intangible is complete
    private void IntangibleCompleted()
    {
        // Builder should go into roam routine after intangible is done
        isInRoamInterval = true;
        nextIntangibleOffset = Vector3.zero;
        // Reset timer for roaming interval
        timeRemaining = roamIntervalTime;
        // Clear queue and set current new patrol (roam) command. @Note: a new route will be set automatically in BaseUnitAI
        baseUnit.currentCommand = new CommandQueueItem { commandType = CommandTypes.Patrol };
    }
}
