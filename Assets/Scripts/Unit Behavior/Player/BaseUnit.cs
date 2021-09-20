using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using DarienEngine;

public class BaseUnit : RTSUnit
{
    private bool selected;
    public bool selectable = true;
    public GameObject selectRing;
    public GameObject fogOfWarMask;
    private Directions facingDir = Directions.Forward;
    private UnitBuilderBase<PlayerConjurerArgs> _Builder;

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
        // Update health
        if (!isDead && UpdateHealth() <= 0)
            StartCoroutine(Die());

        if (!isDead)
        {
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

                // Kinematic units can auto-pick targets as long as they are not busy with other commands
                if ((isKinematic && commandQueue.Count == 0) || attackMove)
                    AutoPickAttackTarget();
                // @TODO: non-kinematic always autopick target?
                else if (!isKinematic)
                    AutoPickAttackTarget();
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

            // if (isKinematic && _Agent.enabled)
            //    DebugNavPath();
        }
    }

    public void Select(bool alone = false)
    {
        if (selectable)
        {
            selected = true;
            selectRing.SetActive(true);

            if (alone)
            {
                // If the unit is a builder, show the build menu when selected
                if (isBuilder)
                    mainPlayer.SetActiveBuilder(_Builder);
                // Else if lone unit selected was not a builder, try clear current active builder
                else
                    mainPlayer.ReleaseActiveBuilder();
                // Show the unit action menu
                UIManager.Instance.actionMenuInstance.Set(isKinematic, canAttack, isBuilder, specialAttacks);
                AudioManager.PlaySelectSound();
            }
        }
    }

    public void DeSelect()
    {
        if (selectable)
        {
            selected = false;
            selectRing.SetActive(false);
        }
    }

    private IEnumerator Die()
    {
        HandleDie();

        DeSelect();
        selectable = false;
        mainPlayer.RemoveUnitFromSelection(this);

        // Units loose line-of-sight when dead
        // @TODO: fogOfWarMask should probably eventually be applied to AIs as well so allied AIs can reveal line-of-sight
        if (fogOfWarMask != null)
            fogOfWarMask.SetActive(false);

        yield return new WaitForSeconds(dieTime);
        Destroy(gameObject);
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

    // Continue to update UI with most recent health, mana, etc. values while mouseover
    private void OnMouseOver()
    {
        // First check if I am already selected
        if (!selected)
            UIManager.Instance.unitInfoInstance.Set(super, null);
    }

    public void SetFacingDir(Directions dir)
    {
        facingDir = dir;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
    }
}
