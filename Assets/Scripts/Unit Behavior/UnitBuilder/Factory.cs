using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Factory : UnitBuilderPlayer<Factory>
{
    public Transform spawnPoint;
    public Transform rallyPoint;
    public GameObject[] intangibleUnits;
    private bool parkingDirectionToggle = false;

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
        // Keep track of master queue to know when building
        isBuilding = masterBuildQueue.Count > 0;

        // @TODO: only the single one builder selected should update button text
        // UpdateAllButtonsText();

        // While masterQueue is not empty, continue queueing up intangible prefabs
        if (masterBuildQueue.Count > 0 && nextQueueReady)
        {
            // @TODO: also need to check that the spawn point is clear before moving on to next unit
            baseUnit.state = RTSUnit.States.Conjuring;
            // @TODO
            // MenuItem next = masterBuildQueue.Peek();
            // InstantiateNextIntangible(next);
            // Toggle whether new unit parks towards the right or left
            parkingDirectionToggle = !parkingDirectionToggle;
            nextQueueReady = false;

            // @TODO: handle infinite. If one unit got the infinite command, all subsequent left-clicks on that unit need to be ignored
            // (right-click) to clear. Also, as long as the condition is true, this should just keep pumping out the same unit
            // maybe something like:
            // if (mode === Modes.Infinite) {masterBuildQueue.Enqueue(map); map.buildQueue.Enqueue(map.prefab);}
        }
    }

    public void QueueBuild(MenuItem item, Vector2 clickPoint)
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

    private void QueueUnit(MenuItem item)
    {
        // Instantiate first immediately
        if (masterBuildQueue.Count == 0)
            nextQueueReady = true;
        // Increment individual unit queue count
        item.buildQueueCount++;
        // Enqueue master queue to keep track of build order and total queue
        masterBuildQueue.Enqueue(item.prefab);
        // @TODO: iff (this builder is the only one selected) UpdateButtonText(map);
    }

    private void InstantiateNextIntangible(MenuItem item)
    {
        GameObject intangible = Instantiate(item.prefab, spawnPoint.position, spawnPoint.rotation);
        // @TODO: intangible.GetComponent<IntangibleUnit<Factory>>().Bind(this, item, parkingDirectionToggle);
    }

    public void ToggleRallyPoint(bool value)
    {
        rallyPoint.gameObject.SetActive(value);
    }
}
