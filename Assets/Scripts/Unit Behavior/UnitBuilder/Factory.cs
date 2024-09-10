using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DarienEngine;

/// <summary>
/// Class <c>Factory</c> represents a playable unit that can build but cannot move.
/// </summary>
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

        // Only currently selected builder updates menu text
        if (isCurrentActive)
            UpdateAllButtonsText();
    }

    public void HandleConjureRoutine()
    {
        IsBuilding = !baseUnit.commandQueue.IsEmpty();
        // While masterQueue is not empty, continue queueing up intangible prefabs
        if (NextQueueReady)
        {
            // @TODO: also need to check that the spawn point is clear before moving on to next units
            ConjurerArgs next = baseUnit.currentCommand.conjurerArgs;
            InstantiateNextIntangible(next);
            // Toggle whether new unit parks towards the right or left
            parkingDirectionToggle = !parkingDirectionToggle;
            NextQueueReady = false;

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

        // @TODO: infinite mode
        // else if (InputManager.HoldingCtrl())
        //     mode = "infinite";

        if (InputManager.HoldingShift())
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
            NextQueueReady = true;
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
    public class ListToRemove
    {
        public CommandQueueItem commandQueueItem;
        public int queueIndex;
    }
    private void RemoveBuild(ConjurerArgs itemToRemove)
    {
        // If holding shift, remove last 5 units of same type from the queue
        if (InputManager.HoldingShift())
        {
            int count = 5;
            List<ListToRemove> listToRemove = new();
            // Compile up to the last five commands of the queue
            for (int i = baseUnit.commandQueue.Count - 1; i >= 0 && count > 0; i--)
            {
                // Find the last 5 commands in the queue of the same unit type
                CommandQueueItem command = baseUnit.commandQueue[i];
                string commandUnitName = command?.conjurerArgs?.prefab.GetComponent<IntangibleUnitBase>().finalUnit.unitName;
                string itemUnitName = itemToRemove?.prefab.GetComponent<IntangibleUnitBase>().finalUnit.unitName;
                // If the command in the queue is the same type of unit, remove it up to 5
                if (command.commandType == CommandTypes.Conjure && commandUnitName == itemUnitName)
                {
                    command.conjurerArgs.buildQueueCount--;
                    listToRemove.Add(new ListToRemove { commandQueueItem = command, queueIndex = i });
                    count--;
                }
            }
            // Remove the found commands from the queue
            listToRemove.ForEach(toRemove =>
            {
                // Basically, if you zero out a unit type with -5 and the last one is the one being conjured at the moment, cancel it
                if (toRemove.commandQueueItem.conjurerArgs.buildQueueCount == 0 && toRemove.commandQueueItem.conjurerArgs.instantiatedPrefab)
                {
                    toRemove.commandQueueItem.conjurerArgs.instantiatedPrefab.GetComponent<IntangibleUnitBase>().CancelIntangible();
                    SetNextQueueReady(true);
                }
                // Splice the intangible from this Factory's queue
                baseUnit.commandQueue.RemoveAt(toRemove.queueIndex);
            });
        }
        else
        {
            itemToRemove.buildQueueCount--;
            int itemIndex = baseUnit.commandQueue.FindIndex(x => x.conjurerArgs == itemToRemove);
            // Basically, if you zero out a unit type with -5 and the last one is the one being conjured at the moment, cancel it
            if (itemToRemove.buildQueueCount == 0 && itemToRemove.instantiatedPrefab)
            {
                baseUnit.commandQueue[itemIndex].conjurerArgs.instantiatedPrefab.GetComponent<IntangibleUnitBase>().CancelIntangible();
                SetNextQueueReady(true);
            }
            // Splice the intangible from this Factory's queue
            baseUnit.commandQueue.RemoveAt(itemIndex);
        }
    }

    private void InstantiateNextIntangible(ConjurerArgs item)
    {
        GameObject intangible = Instantiate(item.prefab, spawnPoint.position, spawnPoint.rotation);
        item.instantiatedPrefab = intangible;
        intangible.GetComponent<IntangibleUnit>().BindBuilder(this, rallyPoint, parkingDirectionToggle);
        // @TODO: deprecated: intangible.GetComponent<IntangibleUnit>().Callback(IntangibleCompleted);
    }

    // @TODO: callbacks are deprecated. Factory and Builder should handle the same way
    // Callback when intangible is complete
    /* private void IntangibleCompleted()
    {
        // Factory dequeue commandQueue and decrement menu item buildQueueCount
        CommandQueueItem lastCommand = baseUnit.commandQueue.Dequeue();
        lastCommand.conjurerArgs.buildQueueCount--;
    } */

    public void ToggleRallyPoint(bool value)
    {
        rallyPoint.gameObject.SetActive(value);
    }

    // Selected Builder gets the menu button click events
    public void TakeOverButtonListeners()
    {
        foreach (ConjurerArgs item in virtualMenu)
        {
            item.clickHandler.OnLeftClick(delegate { QueueBuild(item, Input.mousePosition); });
            item.clickHandler.OnRightClick(delegate { RemoveBuild(item); });
        }
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
