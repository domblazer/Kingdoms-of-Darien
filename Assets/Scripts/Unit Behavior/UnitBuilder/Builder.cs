using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Builder is defined as canMove && canBuild && !isAI
public class Builder : BuilderBase<MenuItem>, IUnitBuilderPlayer
{
    // @Note: cannot include menuRoot in interface since it must be a public var visible in inspector
    public RectTransform menuRoot;
    public List<MenuItem> virtualMenu { get; set; } = new List<MenuItem>();
    public int placedSinceLastShift { get; set; } = 0;
    public GameObject activeFloatingGhost;

    public float lastClickTime { get; set; }
    public float clickDelay { get; set; } = 0.25f;

    void Update()
    {
        // Keep traveling to ghosts in the queue until empty
        if (masterBuildQueue.Count > 0 && nextQueueReady)
        {
            // Builders always keep a queue of GhostUnits
            GameObject nextGhost = masterBuildQueue.Peek().prefab;
            GhostUnitScript nextGhostScript = nextGhost.GetComponent<GhostUnitScript>();

            // @TODO: offset depends on direction, e.g. if walking along x, use x, y, y, and diagonal use mix
            Vector3 offsetRange = nextGhostScript.sizeOffset;
            // Move to next ghost in the queue
            if (nextGhostScript.IsSet() && !baseUnit.IsInRangeOf(nextGhost.transform.position, offsetRange.x))
            {
                baseUnit.SetMove(nextGhost.transform.position);
                Debug.Log("Builder moving to ghost");
            }
            // When arrived at ghost, start building intangible
            else
            {
                Debug.Log("Arrived at ghost");
                baseUnit.SetMove(transform.position);
                baseUnit.state = RTSUnit.States.Conjuring;
                nextQueueReady = false;
                isBuilding = true;
                masterBuildQueue.Dequeue();
                nextGhostScript.StartBuild();
            }
        }
    }

    // Selected Builder gets the menu button click events
    public void TakeOverButtonListeners()
    {
        foreach (MenuItem item in virtualMenu)
            item.menuButton.onClick.AddListener(delegate { QueueBuild(item, Input.mousePosition); });
    }

    public void QueueBuild(MenuItem item, Vector2 clickPoint)
    {
        // First, protect double clicks with click delay
        ProtectDoubleClick();
        // Instantiate ghost prefab. @Note: ghost is only enqueued when set
        GameObject ghost = InstantiateGhost(item, clickPoint);
    }

    // Instantiate new ghost and set bindings with this builder and the menu item clicked
    private GameObject InstantiateGhost(MenuItem item, Vector2 clickPoint)
    {
        GameObject ghost = Instantiate(item.prefab, new Vector3(clickPoint.x, 1, clickPoint.y), item.prefab.transform.localRotation);
        ghost.GetComponent<GhostUnit<Builder>>().Bind(this, item);
        // The ghost instantiated by any menu click will always become the activeFloatingGhost
        activeFloatingGhost = ghost;
        return ghost;
    }

    // Construct a "virtual" menu to represent behavior of menu
    public void InitVirtualMenu(GameObject[] prefabs)
    {
        Button[] menuChildren = menuRoot.GetComponentsInChildren<Button>();
        foreach (var (button, index) in menuChildren.WithIndex())
            virtualMenu.Add(new MenuItem { menuButton = button, prefab = prefabs[index] });
    }

    // Handle small click delay to prevent double clicks on menu
    public void ProtectDoubleClick()
    {
        if (lastClickTime + clickDelay > Time.unscaledTime)
            return;
        lastClickTime = Time.unscaledTime;
    }

    // Clear listeners for next selected builder
    public void ReleaseButtonListeners()
    {
        foreach (MenuItem virtualMenuItem in virtualMenu)
            virtualMenuItem.menuButton.onClick.RemoveAllListeners();
    }
}
