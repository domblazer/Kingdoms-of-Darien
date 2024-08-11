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
            // Builders always keep a queue of GhostUnits
            // Debug.Log("Builder " + gameObject.name + " queue: " + baseUnit.commandQueue.ToString());

            // @TODO: Need to add the case of intangibles being the targets of Conjure commands
            // @TODO: Should not be calling GetComponent in an Update loop
            Debug.Log(name +  " BaseUnit.currentCommand " + BaseUnit.currentCommand);
            Debug.Log(name + " BaseUnit.commandQueue.Count " + BaseUnit.commandQueue.Count);

            if (BaseUnit.currentCommand.conjurerArgs.prefab.GetComponent<GhostUnit>())
            {
                Debug.Log("current command is ghost");
                // Conjure command is for GhostUnit
                GhostUnit nextGhost = BaseUnit.currentCommand.conjurerArgs.prefab.GetComponent<GhostUnit>();
                // @TODO: offset depends on direction, e.g. if walking along x, use x, y, y, and diagonal use mix
                Vector3 offsetRange = nextGhost.offset;
                // Move to next ghost in the queue
                if (nextGhost.IsSet() && !BaseUnit.IsInRangeOf(nextGhost.transform.position, offsetRange.x))
                    BaseUnit.MoveToPosition(nextGhost.transform.position);
                // When arrived at ghost, start building intangible
                else
                    StartNextIntangible(nextGhost);
            }
            else if (BaseUnit.currentCommand.conjurerArgs.prefab.GetComponent<IntangibleUnit>())
            {
                Debug.Log("current command is intangible");
                // Conjure command is for IntangibleUnit
                IntangibleUnit intangible = BaseUnit.currentCommand.conjurerArgs.prefab.GetComponent<IntangibleUnit>();
                // Move to the Intangible queued
                // @TODO: if !intangible.isDone else CancelCommand()
                // @TODO: if intangible dies during this routine, stop build
                if (!BaseUnit.IsInRangeOf(intangible.transform.position, intangible.offset.x))
                    BaseUnit.MoveToPosition(intangible.transform.position);
                // When arrived at Intangible, start Conjuring it
                else
                {
                    AppendToIntangible(intangible);
                }
            }

        }
    }

    public void StartNextIntangible(GhostUnit ghost)
    {
        BaseUnit.MoveToPosition(transform.position);
        NextQueueReady = false;
        IsBuilding = true;
        ghost.StartBuild();
    }

    public void AppendToIntangible(IntangibleUnit intangible)
    {
        currentIntangible = intangible;
        BaseUnit.MoveToPosition(transform.position);
        NextQueueReady = false;
        IsBuilding = true;
        intangible.BindBuilder(this);
    }
}
