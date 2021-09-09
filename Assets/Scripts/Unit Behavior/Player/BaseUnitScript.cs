using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using DarienEngine;

public class BaseUnitScript : RTSUnit
{
    private bool selected;
    public bool selectable = true;
    public GameObject selectRing;

    public AudioClip moveSound;
    public AudioClip selectSound;

    private GameObject _Units;
    private Directions facingDir = Directions.Forward;
    private bool hasAlreadyDied = false;

    private UnitBuilderBase<PlayerConjurerArgs> _Builder;

    private void Awake()
    {
        // Default move position, for units not instantiated with a parking location
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

        // Get unit builder manager which handles btn listeners, build queues, etc
        if (isBuilder)
        {
            if (isKinematic)
                _Builder = GetComponent<Builder>();
            else
                _Builder = GetComponent<Factory>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // @TODO: ability to reposition builderRallyPoint
        UpdateHealth();

        if (!isDead)
        {
            // De-select (all) on right click
            if (Input.GetMouseButtonDown(1))
                DeSelect();

            // Handle movement and the various states of movement possible, e.g. "Parking"
            if (isKinematic)
                HandleMovement();

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
                state = States.Standby;

            // If this unit is selected and no other unit has focus
            if (selectable && selected && !GameManager.Instance.IsHovering())
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
            // @Note: removal from player context handled in RTSUnit.Die()
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
                if (!isKinematic)
                {
                    (_Builder as Factory).ToggleRallyPoint(true);
                    (_Builder as Factory).ToggleBuildMenu(true); // Show build menu
                    (_Builder as Factory).TakeOverButtonListeners();
                }
                else
                {
                    (_Builder as Builder).ToggleBuildMenu(true); // Show build menu
                    (_Builder as Builder).TakeOverButtonListeners();
                }
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
                // @TODO: this should only be applied to the current active builder
                if (!isKinematic)
                {
                    (_Builder as Factory).ToggleRallyPoint(false);
                    (_Builder as Factory).ToggleBuildMenu(false); // Hide build menu
                    (_Builder as Factory).ReleaseButtonListeners();
                }
                else
                {
                    (_Builder as Builder).ToggleBuildMenu(false); // Hide build menu
                    (_Builder as Builder).ReleaseButtonListeners();
                }
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

    public bool IsSelected()
    {
        return selected;
    }

    void OnMouseEnter()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && selectable)
        {
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Select);
            UIManager.Instance.unitInfoInstance.Set(super, null);
        }
        GameManager.Instance.SetHovering(gameObject);
    }

    // @TODO: if mouse is over this unit when the unit dies, still need to reset cursor, clear unit ui
    void OnMouseExit()
    {
        if (selectable)
        {
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
            if (!selected)
                UIManager.Instance.unitInfoInstance.Toggle(false);
        }
        GameManager.Instance.ClearHovering();
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

    public void SetFacingDir(Directions dir)
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
