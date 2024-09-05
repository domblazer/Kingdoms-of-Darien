using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

/// <summary>
/// Class <c>UnitBuilderBase</c> models the most common and basic functionality needed for builders.
/// </summary>
public class UnitBuilderBase : MonoBehaviour
{
    public bool IsBuilding { get; set; }
    public bool NextQueueReady { get; set; } = false;
    public RTSUnit baseUnit { get; set; }
    [HideInInspector] public IntangibleUnitBase currentIntangible;

    private void Awake()
    {
        baseUnit = GetComponent<RTSUnit>();
        baseUnit.commandQueue.OnQueueChanged += Interrupt;
    }

    public void QueueBuildOnIntangible(IntangibleUnit intangible, bool addToQueue = false)
    {
        Debug.Log("QueueBuildOnIntangible");
        // Add to queue 
        if (!addToQueue)
            baseUnit.commandQueue.Clear();
        // @TODO
        ConjurerArgs conjurerArgs = new()
        {
            prefab = intangible.gameObject
        };
        // @TODO
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            commandPoint = intangible.transform.position,
            conjurerArgs = conjurerArgs
        });
        SetNextQueueReady(true);
    }

    public void SetNextQueueReady(bool val)
    {
        NextQueueReady = val;
        // Reset isBuilding for kinematic builders since once one intangible is done, builder must re-enter move routine before starting another
        if (IsBuilder())
            IsBuilding = false;
    }

    public bool IsFactory()
    {
        return !baseUnit.isKinematic;
    }

    public bool IsBuilder()
    {
        return baseUnit.isKinematic;
    }

    public void Interrupt(object sender, CommandQueue.CommandQueueChangedEventArgs changeEvent)
    {
        Debug.Log("Builder interrupted.");
        // @TODO: This is a really dirty solution and is not going to work long term. 
        //
        // The reason for this in the first place is so that if a Builder gets another command
        // while conjuring an intangible, it cancels that action so it can go do the command just queued. 
        // 
        // The problem is that commandQueue.Clear() is called on currentCommand set, which does not always mean a conjure routine should be cancelled.
        // The AI in particular uses the currentCommand setter in the intangible completed callback to queue a patrol routine after finishing an intangible. 
        if (changeEvent.changeType == "CancelBuild")
        {
            Debug.Log("CancelBuild event fired.");
            // @TODO: Would probably like to detect also that the previous current command of the Builder was a Conjure task
            // if (IsBuilding)
            CancelBuild();
        }
    }

    public void CancelBuild(bool deferArray = false)
    {
        Debug.Log("Cancel current build; disconnect intangible " + currentIntangible?.gameObject?.name);
        IsBuilding = false;
        if (currentIntangible)
            currentIntangible.DetachBuilder(this, deferArray);
        currentIntangible = null;

        Debug.Log("IsBuilding " + IsBuilding);

        // Ensure builder is ready with next command if current build is canceled
        if (IsBuilding)
        {
            CommandQueueItem lastCommand = baseUnit.commandQueue.Dequeue();
            lastCommand.conjurerArgs.buildQueueCount--;
        }
        SetNextQueueReady(true);
    }
}
