using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

/// <summary>
/// Class <c>Builder</c> represents a playable unit that can build and can move. Extends class <c>UnitBuilderPlayer</c> to inherit
/// common functionality shared with class <c>Factory</c>.
/// </summary>
public class Builder : UnitBuilderPlayer
{
    public GameObject[] ghostUnits;
    public int placedSinceLastShift { get; set; } = 0;
    [HideInInspector]
    public GameObject activeFloatingGhost;

    [HideInInspector] public bool isClickInProgress = false;

    void Start()
    {
        InitVirtualMenu(ghostUnits);
    }

    public void QueueBuild(ConjurerArgs item, Vector2 clickPoint)
    {
        // First, protect double clicks with click delay
        ProtectDoubleClick();
        // Instantiate new active ghost
        InstantiateGhost(item, new Vector3(clickPoint.x, 1, clickPoint.y));
    }

    // Instantiate new ghost and set bindings with this builder and the menu item clicked
    public GameObject InstantiateGhost(ConjurerArgs item, Vector3 clickPoint)
    {
        GameObject ghost = Instantiate(item.prefab, clickPoint, item.prefab.transform.localRotation);
        // @TODO: builder needs to keep track of all these ghosts so they can be deleted later if a single placement happens
        ghost.GetComponent<GhostUnit>().BindBuilder(this, item);
        // The ghost instantiated by any menu click will always become the activeFloatingGhost
        activeFloatingGhost = ghost;
        return ghost;
    }

    // Selected Builder gets the menu button click events
    public void TakeOverButtonListeners()
    {
        foreach (ConjurerArgs item in virtualMenu)
            item.clickHandler.OnLeftClick(delegate { QueueBuild(item, Input.mousePosition); });
    }

    public void SetCurrentActive()
    {
        isCurrentActive = true;
        ToggleBuildMenu(true); // Show build menu
        TakeOverButtonListeners();
    }

    public void ReleaseCurrentActive()
    {
        isCurrentActive = false;
        ToggleBuildMenu(false); // Hide build menu
        ReleaseButtonListeners();
    }

    public void HandleConjureRoutine()
    {
        if (NextQueueReady)
        {
            Debug.Log("baseUnit.commandQueue: " + baseUnit.commandQueue);
            if (baseUnit.currentCommand.conjurerArgs.prefab && baseUnit.currentCommand.conjurerArgs.prefab.GetComponent<GhostUnit>())
            {
                // Conjure command is for GhostUnit
                GhostUnit nextGhost = baseUnit.currentCommand.conjurerArgs.prefab.GetComponent<GhostUnit>();
                // @TODO: offset depends on direction, e.g. if walking along x, use x, y, y, and diagonal use mix
                Vector3 offsetRange = nextGhost.offset;
                // Move to next ghost in the queue
                if (nextGhost.IsSet() && !baseUnit.IsInRangeOf(nextGhost.transform.position, offsetRange.x))
                {
                    isMovingToNextConjure = true;
                    baseUnit.MoveToPosition(nextGhost.transform.position);
                }
                // When arrived at ghost, start building intangible
                else
                {
                    isMovingToNextConjure = false;
                    StartNextIntangible(nextGhost);
                }
            }
            else if (baseUnit.currentCommand.conjurerArgs.prefab && baseUnit.currentCommand?.conjurerArgs?.prefab.GetComponent<IntangibleUnit>())
            {
                // Conjure command is for IntangibleUnit
                IntangibleUnit intangible = baseUnit.currentCommand.conjurerArgs.prefab.GetComponent<IntangibleUnit>();
                // Move to the Intangible queued
                if (!baseUnit.IsInRangeOf(intangible.transform.position, intangible.offset.x))
                {
                    isMovingToNextConjure = true;
                    baseUnit.MoveToPosition(intangible.transform.position);
                }
                // When arrived at Intangible, start Conjuring it
                else
                {
                    isMovingToNextConjure = false;
                    AppendToIntangible(intangible);
                }
            }
            else
            {
                Debug.LogWarning("Prefab is missing. This builder may have a command in its queue that should have been removed.\n"
                    + "Command: " + baseUnit.currentCommand);
                Debug.LogWarning("Attempting to prune loose command in Builder script now.");
                baseUnit.commandQueue.Dequeue();
            }
        }
    }

    public void StartNextIntangible(GhostUnit ghost)
    {
        baseUnit.MoveToPosition(transform.position);
        NextQueueReady = false;
        IsBuilding = true;
        ghost.StartBuild();
    }

    public void AppendToIntangible(IntangibleUnit intangible)
    {
        currentIntangible = intangible;
        baseUnit.MoveToPosition(transform.position);
        NextQueueReady = false;
        IsBuilding = true;
        intangible.BindBuilder(this);
    }
}
