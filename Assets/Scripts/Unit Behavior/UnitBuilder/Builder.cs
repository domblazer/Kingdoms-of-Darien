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
        ToggleBuildMenu(false);
    }

    void Update()
    {
        // Keep traveling to ghosts in the queue until empty
        if (masterBuildQueue.Count > 0 && nextQueueReady)
        {
            // Builders always keep a queue of GhostUnits
            GameObject nextGhost = masterBuildQueue.Peek().prefab;
            GhostUnit nextGhostScript = nextGhost.GetComponent<GhostUnit>();

            // @TODO: offset depends on direction, e.g. if walking along x, use x, y, y, and diagonal use mix
            Vector3 offsetRange = nextGhostScript.offset;
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

    public void QueueBuild(PlayerConjurerArgs item, Vector2 clickPoint)
    {
        // First, protect double clicks with click delay
        ProtectDoubleClick();
        // Instantiate ghost prefab. @Note: ghost is only enqueued when set
        GameObject ghost = InstantiateGhost(item, clickPoint);
    }

    // Instantiate new ghost and set bindings with this builder and the menu item clicked
    private GameObject InstantiateGhost(PlayerConjurerArgs item, Vector2 clickPoint)
    {
        GameObject ghost = Instantiate(item.prefab, new Vector3(clickPoint.x, 1, clickPoint.y), item.prefab.transform.localRotation);
        ghost.GetComponent<GhostUnit>().Bind(this, item);
        // The ghost instantiated by any menu click will always become the activeFloatingGhost
        activeFloatingGhost = ghost;
        return ghost;
    }

    // Selected Builder gets the menu button click events
    public void TakeOverButtonListeners()
    {
        foreach (PlayerConjurerArgs item in virtualMenu)
            item.menuButton.onClick.AddListener(delegate { QueueBuild(item, Input.mousePosition); });
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
}
