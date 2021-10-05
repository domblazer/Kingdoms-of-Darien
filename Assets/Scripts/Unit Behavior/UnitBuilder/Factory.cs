﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

public class Factory : UnitBuilderPlayer
{
    public Transform spawnPoint;
    public Transform rallyPoint;
    protected bool parkingDirectionToggle = false;
    public GameObject[] intangibleUnits;

    void Start()
    {
        if (!spawnPoint)
            throw new System.Exception("Factory requires Spawn Point. Create empty child, 'spawn-point', and assign it in inspector.");
        if (!rallyPoint)
            throw new System.Exception("Factory requires Rally Point. Create empty child, 'rally-point', and assign it in inspector.");

        InitVirtualMenu(intangibleUnits);
        ToggleRallyPoint(false);
    }

    void Update()
    {
        // @TODO: ability to reposition builderRallyPoint

        // Keep track of master queue to know when building
        isBuilding = !baseUnit.commandQueue.IsEmpty();

        // Only currently selected builder updates menu text
        if (isCurrentActive)
            UpdateAllButtonsText();

        // While masterQueue is not empty, continue queueing up intangible prefabs
        if (!baseUnit.commandQueue.IsEmpty() && nextQueueReady)
        {
            // @TODO: also need to check that the spawn point is clear before moving on to next unit
            baseUnit.state = RTSUnit.States.Conjuring;
            ConjurerArgs next = baseUnit.currentCommand.conjurerArgs;
            InstantiateNextIntangible(next);
            // Toggle whether new unit parks towards the right or left
            parkingDirectionToggle = !parkingDirectionToggle;
            nextQueueReady = false;

            // @TODO: handle infinite. If one unit got the infinite command, all subsequent left-clicks on that unit need to be ignored
            // (right-click) to clear. Also, as long as the condition is true, this should just keep pumping out the same unit
            // maybe something like:
            // if (mode === Modes.Infinite) {masterBuildQueue.Enqueue(map); map.buildQueue.Enqueue(map.prefab);}
        }
    }

    public void QueueBuild(ConjurerArgs item, Vector2 clickPoint)
    {
        // First, protect double clicks with click delay
        ProtectDoubleClick();

        // Capture different queue modes
        bool plusFive = false;
        if (InputManager.HoldingShift())
            plusFive = true;

        // @TODO: infinite mode
        // else if (InputManager.HoldingCtrl())
        //     mode = "infinite";

        if (plusFive)
        {
            for (int i = 0; i < 5; i++)
                QueueUnit(item);
        }
        // @TODO: else if mode is infinite, UpdateButtonText("+++");
        else
        {
            QueueUnit(item);
        }
    }

    private void QueueUnit(ConjurerArgs item)
    {
        // Instantiate first immediately
        if (baseUnit.commandQueue.IsEmpty())
            nextQueueReady = true;
        // Increment individual unit queue count
        item.buildQueueCount++;
        // New conjure command
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            // commandPoint = item // @TODO: commandPoint not needed here?
            conjurerArgs = item
        });
    }

    private void InstantiateNextIntangible(ConjurerArgs item)
    {
        GameObject intangible = Instantiate(item.prefab, spawnPoint.position, spawnPoint.rotation);
        intangible.GetComponent<IntangibleUnit>().Bind(this, rallyPoint, parkingDirectionToggle);
        intangible.GetComponent<IntangibleUnit>().Callback(NextIntangibleCompleted);
    }

    // Callback when intangible is complete
    private void NextIntangibleCompleted()
    {
        // Factory dequeue commandQueue and decrement menu item buildQueueCount
        CommandQueueItem lastCommand = baseUnit.commandQueue.Dequeue();
        lastCommand.conjurerArgs.buildQueueCount--;
    }

    public void ToggleRallyPoint(bool value)
    {
        rallyPoint.gameObject.SetActive(value);
    }

    // Selected Builder gets the menu button click events
    public void TakeOverButtonListeners()
    {
        foreach (ConjurerArgs item in virtualMenu)
            item.menuButton.onClick.AddListener(delegate { QueueBuild(item, Input.mousePosition); });
    }

    public void SetCurrentActive()
    {
        isCurrentActive = true;
        ToggleRallyPoint(true);
        ToggleBuildMenu(true); // Show build menu
        TakeOverButtonListeners();
    }

    public void ReleaseCurrentActive()
    {
        isCurrentActive = false;
        ToggleRallyPoint(false);
        ToggleBuildMenu(false); // Hide build menu
        ReleaseButtonListeners();
    }
}
