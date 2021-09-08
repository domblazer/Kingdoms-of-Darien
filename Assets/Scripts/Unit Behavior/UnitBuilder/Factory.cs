using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Factory : FactoryBase<PlayerConjurerArgs>, IUnitBuilderPlayer
{
    public RectTransform menuRoot;
    public List<PlayerConjurerArgs> virtualMenu { get; set; } = new List<PlayerConjurerArgs>();

    public GameObject[] intangibleUnits;

    public float lastClickTime { get; set; }
    public float clickDelay { get; set; } = 0.25f;

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
            PlayerConjurerArgs next = masterBuildQueue.Peek();
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

    public void QueueBuild(PlayerConjurerArgs item, Vector2 clickPoint)
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

    private void QueueUnit(PlayerConjurerArgs item)
    {
        // Instantiate first immediately
        if (masterBuildQueue.Count == 0)
            nextQueueReady = true;
        // Increment individual unit queue count
        item.buildQueueCount++;
        // Enqueue master queue to keep track of build order and total queue
        masterBuildQueue.Enqueue(item);
    }

    private void InstantiateNextIntangible(PlayerConjurerArgs item)
    {
        GameObject intangible = Instantiate(item.prefab, spawnPoint.position, spawnPoint.rotation);
        intangible.GetComponent<IntangibleUnit>().Bind(this, item, rallyPoint, parkingDirectionToggle);
    }

    public void ToggleRallyPoint(bool value)
    {
        rallyPoint.gameObject.SetActive(value);
    }

    public void ToggleBuildMenu(bool value)
    {
        menuRoot.gameObject.SetActive(value);
    }

    // Construct a "virtual" menu to represent behavior of menu
    public void InitVirtualMenu(GameObject[] prefabs)
    {
        Button[] menuChildren = menuRoot.GetComponentsInChildren<Button>();
        foreach (var (button, index) in menuChildren.WithIndex())
            virtualMenu.Add(new PlayerConjurerArgs { menuButton = button, prefab = prefabs[index] });
    }

    // Handle small click delay to prevent double clicks on menu
    public void ProtectDoubleClick()
    {
        if (lastClickTime + clickDelay > Time.unscaledTime)
            return;
        lastClickTime = Time.unscaledTime;
    }

    // Selected Builder gets the menu button click events
    public void TakeOverButtonListeners()
    {
        foreach (PlayerConjurerArgs item in virtualMenu)
            item.menuButton.onClick.AddListener(delegate { QueueBuild(item, Input.mousePosition); });
    }

    // Clear listeners for next selected builder
    public void ReleaseButtonListeners()
    {
        foreach (PlayerConjurerArgs virtualMenuItem in virtualMenu)
            virtualMenuItem.menuButton.onClick.RemoveAllListeners();
    }
}
