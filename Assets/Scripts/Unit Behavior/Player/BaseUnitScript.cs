using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class BaseUnitScript : RTSUnit
{
    private bool selected;
    public bool selectable = true;
    public GameObject selectRing;

    public AudioClip moveSound;
    public AudioClip selectSound;

    private GameObject _Units;
    private GhostUnitScript.Directions facingDir = GhostUnitScript.Directions.Forward;
    private bool hasAlreadyDied = false;

    private void Awake()
    {
        // @TODO: units parent needs to be by team number, e.g. _Units_Player, _Units_02, etc.
        _Units = GameObject.Find("_Units");
        if (!_Units)
        {
            _Units = new GameObject("_Units"); // If master unit holder doesn't exist yet, create it
        }
        transform.parent = _Units.transform; // child this game object to master unit holder        

        // Default move position, for units not instantiated with a parking location
        // moveToPosition = transform.position;
        moveToPositionQueue.Enqueue(transform.position);
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();

        if (selectable)
        {
            selectRing.SetActive(false);
            // Set an initial green color on select-ring
            selectRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, (health / 100));
        }

        gameObject.tag = "Friendly"; // @TODO: assign tags for different teams

        // @TODO: set the minimap icon sprite color to faction-color

        // Need to tell unit selection script to refresh allUnits. I think this needs to be in Start()
        _UnitSelection.RefreshAllUnits();
    }

    // Update is called once per frame
    void Update()
    {
        // @TODO: ability to reposition builderRallyPoint
        UpdateHealth();

        if (!isDead)
        {
            // De-select on right click
            if (Input.GetMouseButtonDown(1))
            {
                // @Note: this de-selects ALL instances of BaseUnitScript
                DeSelect();
            }

            // Handle movement and the various states of movement possible, e.g. "Parking"
            if (isKinematic)
            {
                HandleMovement();
            }

            // Handle attack states if can attack
            if (canAttack)
            {
                HandleAttackRoutine();

                // If any units in sight to attack, continue picking closest and engage attack
                // @TODO: break off attack if player unit told to move while isAttacking
                // @TODO: player unit, autopick enables attack-move by default, but should ignore if in Passive mode
                // @TODO: also, if in Defensive mode, should not pursue enemies as aggressively, e.g. if chasing for more than 5 secs, break; 
                // defensive unit should also only engage after enemy gets very close (like hold-your-ground type behavior)
                if ((isKinematic && !IsMoving()) || attackMove)
                {
                    // Player units should be expected to finish moving before autopicking, unless doing an attack-move
                    AutoPickAttackTarget();
                }
                else if (!isKinematic)
                {
                    // @TODO: non-kinematic always autopick target?
                    AutoPickAttackTarget();
                }

            }

            // @TODO: and !isBuilding
            if (!isParking && !IsAttacking() && !engagingTarget && !IsMoving())
            {
                state = States.Standby;
            }

            // If this unit is selected and no other unit has focus
            if (selectable && selected && !GameManagerScript.Instance.IsHovering())
            {
                // Color select-ring based on health value
                selectRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, (health / 100));

                // @TODO: secondary could be a "Conjuring" secondary
                RTSUnit secondary = attackTarget ? attackTarget.GetComponent<RTSUnit>() : null;
                UIManager.Instance.unitInfoInstance.Set(super, secondary);
            }

            if (isKinematic && _Agent.enabled)
                DebugNavPath();
        }
        else if (!hasAlreadyDied)
        {
            // Called once to set this unit to dead state
            Debug.Log("Has already died, make unselectable.");
            DeSelect();
            _UnitSelection.RemoveUnitFromSelection(gameObject);
            transform.parent = null; // Clear this unit from the "_Units" gameObject so as to remove it from totalUnits
            _UnitSelection.RemoveUnitFromTotal(gameObject);
            selectable = false;
            hasAlreadyDied = true;
        }
    }

    public void Select(bool alone = false)
    {
        if (selectable)
        {
            selected = true;
            selectRing.SetActive(true);

            // If the unit is a builder, show the build menu when selected
            if (isBuilder && alone)
            {
                _UnitBuilderScript.ToggleBuildMenu(true); // Show build menu
                _UnitBuilderScript.ToggleRallyPoint(true);

                // This instance must now seize the UI button listeners, since the UI is shared
                _UnitBuilderScript.TakeOverButtonListeners();

            }
            if (alone)
            {
                UIManager.Instance.actionMenuInstance.Set(isKinematic, canAttack, isBuilder, specialAttacks);

                // Play the select sound 50% of the time
                if (Random.Range(0.0f, 1.0f) > 0.5f && selectSound != null)
                {
                    _AudioSource.PlayOneShot(selectSound, 1);
                }
            }
        }
    }

    public void DeSelect()
    {
        if (selectable)
        {
            selected = false;
            selectRing.SetActive(false);

            if (isBuilder)
            {
                _UnitBuilderScript.ToggleBuildMenu(false); // Hide build menu
                _UnitBuilderScript.ToggleRallyPoint(false);

                // Remove this instance's button listeners for the next selected builder
                _UnitBuilderScript.ReleaseButtonListeners();
            }

            // @TODO: shouldn't have to call this for every unit getting Deselected, but doesn't ssem to work properly in UnitSelectionScript
            if (UIManager.Instance.actionMenuInstance.actionMenuActive)
            {
                UIManager.Instance.actionMenuInstance.Toggle(false);
            }
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
            UIManager.Instance.unitInfoInstance.Toggle(false);
        }
    }

    void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && selectable)
        {
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Select);
            UIManager.Instance.unitInfoInstance.Set(super, null);
        }
        GameManagerScript.Instance.SetHovering(gameObject);
    }

    // @TODO: if mouse is over this unit when the unit dies, still need to reset cursor, clear unit ui
    void OnMouseExit()
    {
        if (selectable)
        {
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
            if (!selected)
            {
                UIManager.Instance.unitInfoInstance.Toggle(false);
            }
        }
        GameManagerScript.Instance.ClearHovering();
    }

    private void OnMouseOver()
    {
        // First check if I am already selected
        if (!selected)
        {
            // Only update the UI while mouseover if not is already selected, to avoid conflicts
            UIManager.Instance.unitInfoInstance.Set(super, null);
        }
    }

    public void SetFacingDir(GhostUnitScript.Directions dir)
    {
        facingDir = dir;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
    }

    public void PlayMoveSound()
    {
        // Play the move sound 50% of the time and only when unit is alone
        if (Random.Range(0.0f, 1.0f) > 0.5f && moveSound != null)
        {
            _AudioSource.PlayOneShot(moveSound, 1);
        }
    }
}
