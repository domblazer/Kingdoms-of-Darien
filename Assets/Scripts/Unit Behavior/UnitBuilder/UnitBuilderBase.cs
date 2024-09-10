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
    [HideInInspector] public bool isMovingToNextConjure = false;

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
        {
            // @NOTE: Important that the build is cleared before queue is cleared so any ghosts can get cleaned up, intangibles detached, etc.
            baseUnit.commandQueue.TriggerCancelBuild();
            baseUnit.commandQueue.Clear();
        }
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            commandPoint = intangible.transform.position,
            conjurerArgs = new() { prefab = intangible.gameObject }
        });
        // Register to the intangible's OnDie function to cancel it if the intangible dies or completes before builder gets there
        intangible.OnDie += CancelMovingToIntangibleRoutine;
        //SetNextQueueReady(true);
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

    private void CancelMovingToIntangibleRoutine(object sender, System.EventArgs e)
    {
        if (sender != null && isMovingToNextConjure)
        {
            // If a registered Intangible died or was completed while builder was moving to it, builder should remove it from their routine
            baseUnit.commandQueue.Dequeue();
            SetNextQueueReady(true);
        }
    }

    /**
    * This function is registered to the OnCommandQueueChanged event handler, so fires when the Builder's parent Unit commandQueue is changed.
    * The idea is to fire a "CancelBuild" event in the currentCommand setter which clears all commands and inserts a priority command at the top of the queue.
    */
    public void Interrupt(object sender, CommandQueue.CommandQueueChangedEventArgs changeEvent)
    {
        // Debug.Log("Builder interrupted.");
        if (changeEvent.changeType == "CancelBuild")
        {
            Debug.Log("CancelBuild event fired.");
            CancelBuild();
        }
    }

    public void CancelBuild(bool deferArray = false)
    {
        IsBuilding = false;
        if (currentIntangible)
        {
            Debug.Log("Cancel current build; disconnect intangible " + currentIntangible?.gameObject?.name);
            currentIntangible.DetachBuilder(this, deferArray);
        }
        currentIntangible = null;

        // Destroy all ghosts for this builder and remove all it's other conjure commands
        if (IsBuilder())
        {
            foreach (CommandQueueItem cmd in baseUnit.commandQueue)
            {
                if (cmd.commandType == CommandTypes.Conjure && cmd.conjurerArgs.prefab != null && cmd.conjurerArgs.prefab.GetComponent<GhostUnit>())
                    Destroy(cmd.conjurerArgs.prefab);
            }
            baseUnit.commandQueue.RemoveAll(cmd => cmd.commandType == CommandTypes.Conjure);
        }

        if (IsBuilding)
        {
            CommandQueueItem lastCommand = baseUnit.commandQueue.Dequeue();
            lastCommand.conjurerArgs.buildQueueCount--;
        }
        // Ensure builder is ready with next command if current build is canceled
        SetNextQueueReady(true);
    }
}
