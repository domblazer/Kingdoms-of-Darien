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
    public RTSUnit BaseUnit { get; set; }
    [HideInInspector] public IntangibleUnitBase currentIntangible;

    private void Awake()
    {
        BaseUnit = GetComponent<RTSUnit>();
        BaseUnit.commandQueue.OnQueueChanged += Interrupt;
    }

    public void QueueBuildOnIntangible(IntangibleUnit intangible, bool addToQueue = false)
    {
        Debug.Log("QueueBuildOnIntangible");
        // Add to queue 
        if (!addToQueue)
            BaseUnit.commandQueue.Clear();
        // @TODO
        ConjurerArgs conjurerArgs = new()
        {
            prefab = intangible.gameObject
        };
        // @TODO
        BaseUnit.commandQueue.Enqueue(new CommandQueueItem
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
        return !BaseUnit.isKinematic;
    }

    public bool IsBuilder()
    {
        return BaseUnit.isKinematic;
    }

    public void Interrupt(object sender, CommandQueue.CommandQueueChangedEventArgs changeEvent)
    {
        Debug.Log("Builder commandQueue was changed.");
        if (changeEvent.changeType == "Clear")
        {
            Debug.Log("Builder queue was cleared.");
            // @TODO: Would probably like to detect also that the previous current command of the Builder was a Conjure task
            if (currentIntangible)
                CancelBuild();
        }
    }

    public void CancelBuild(bool deferArray = false)
    {
        Debug.Log("Cancel current build; disconnect intangible " + currentIntangible.gameObject.name);
        IsBuilding = false;
        currentIntangible.DetachBuilder(this, deferArray);
        currentIntangible = null;

        // Ensure builder is ready with next command if current build is canceled
        CommandQueueItem lastCommand = BaseUnit.commandQueue.Dequeue();
        lastCommand.conjurerArgs.buildQueueCount--;
        SetNextQueueReady(true);

    }
}
