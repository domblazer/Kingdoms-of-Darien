using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

// Builder is defined as canMove && canBuild && !isAI
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
        ghost.GetComponent<GhostUnit>().Bind(this, item);
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
        if (nextQueueReady)
        {
            // Builders always keep a queue of GhostUnits
            // Debug.Log("Builder " + gameObject.name + " queue: " + baseUnit.commandQueue.ToString());
            GhostUnit nextGhost = baseUnit.currentCommand.conjurerArgs.prefab.GetComponent<GhostUnit>();
            // @TODO: offset depends on direction, e.g. if walking along x, use x, y, y, and diagonal use mix
            Vector3 offsetRange = nextGhost.offset;
            // Move to next ghost in the queue
            if (nextGhost.IsSet() && !baseUnit.IsInRangeOf(nextGhost.transform.position, offsetRange.x))
                baseUnit.MoveToPosition(nextGhost.transform.position);
            // When arrived at ghost, start building intangible
            else
                StartNextIntangible(nextGhost);
        }
    }

    public void StartNextIntangible(GhostUnit ghost)
    {
        baseUnit.MoveToPosition(transform.position);
        nextQueueReady = false;
        isBuilding = true;
        ghost.StartBuild(IntangibleCompleted);
    }

    public void IntangibleCompleted()
    {
        baseUnit.commandQueue.Dequeue();
    }
}
