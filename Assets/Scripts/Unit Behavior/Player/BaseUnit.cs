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
    private UnitBuilderPlayer _Builder;

    private LineRenderer lineRenderer;

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

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        lineRenderer.widthMultiplier = 0.2f;
        lineRenderer.startColor = lineRenderer.endColor = Color.black;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Tile;
        ToggleCommandPointsUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Update health
        if (!isDead && UpdateHealth() <= 0)
            StartCoroutine(Die());

        if (!isDead)
        {
            // @TODO: handle behaviour by what's in the commandQueue
            if (!commandQueue.IsEmpty())
            {
                switch (currentCommand.commandType)
                {
                    case CommandTypes.Move:
                        // just handle move behaviour
                        // @TODO: Sub-CommandTypes? E.g. Attack-Move, Guard-Move, etc.?
                        // @TODO: state = States.Parking
                        HandleMovement();
                        break;
                    case CommandTypes.Attack:
                        // handle engaging target (moveTo target) and attacking behaviour
                        HandleAttackRoutine();
                        break;
                    case CommandTypes.Patrol:
                        // handle patrol behavior
                        // @TODO: if patrol point not set?
                        Patrol();
                        break;
                    case CommandTypes.Conjure:
                        // @TODO: _Builder.handleConjuringRouting (moveTo buildSpot and conjure)

                        state = States.Conjuring;
                        break;
                    case CommandTypes.Guard:
                        // @TODO 
                        break;
                    default:
                        // Idle behavour?
                        state = States.Standby;
                        break;
                }
            }
            else
            {
                // @TODO: if commandQueue is empty, just be on standby
                state = States.Standby;
            }

            // @TODO: Determine when to Auto pick attack targets
            if (canAttack && state.Value == States.Standby.Value)
            {
                AutoPickAttackTarget();

                // If any units in sight to attack, continue picking closest and engage attack
                // @TODO: break off attack if player unit told to move while isAttacking
                // @TODO: player unit, autopick enables attack-move by default, but should ignore if in Passive mode
                // @TODO: also, if in Defensive mode, should not pursue enemies as aggressively, e.g. if chasing for more than 5 secs, break; 
                // defensive unit should also only engage after enemy gets very close (like hold-your-ground type behavior)
            }

            if (selectable && selected)
            {
                // Update select ring color based on health when selected
                selectRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, (health / 100));

                Debug.Log(gameObject.name + " commandQueue: " + string.Join<CommandQueueItem>(", ", commandQueue.ToArray()));

                // Update the unit info UI if no other unit has focus from hovering
                if (!GameManager.Instance.IsHoveringOther(gameObject))
                {
                    // @TODO: secondary could be a "Conjuring" secondary
                    RTSUnit secondary = attackTarget ? attackTarget.GetComponent<RTSUnit>() : null;
                    UIManager.Instance.unitInfoInstance.Set(super, secondary);
                }

                // Toggle line renderer on shift up/down for mobile units
                if (isKinematic)
                {
                    if (InputManager.ShiftPressed())
                        ToggleCommandPointsUI(true);
                    if (InputManager.HoldingShift())
                        UpdateCommandPointLines();
                    if (InputManager.ShiftReleased())
                        ToggleCommandPointsUI(false);
                }
            }

            // if (isKinematic && _Agent.enabled)
            //    DebugNavPath();
        }
    }

    // Draw lines between commandQueue points
    private void UpdateCommandPointLines()
    {
        if (commandQueue.Count > 0)
        {
            Vector3 adjustedTransformPosition = new Vector3(transform.position.x, 1.2f, transform.position.z);
            lineRenderer.positionCount = commandQueue.Count + 1;
            if (currentCommand.commandType != CommandTypes.Patrol)
                lineRenderer.SetPosition(0, adjustedTransformPosition);
            int patrolCount = 0;
            foreach (var (item, index) in commandQueue.WithIndex())
            {
                if (item.commandType == CommandTypes.Patrol)
                {
                    // We don't want to draw lines to patrol points, subtract from positionCount after
                    patrolCount++;
                }
                else
                {
                    Vector3 adjustedCommandPoint = new Vector3(item.commandPoint.x, 1.2f, item.commandPoint.z);
                    lineRenderer.SetPosition(index + 1, adjustedCommandPoint);
                }
            }
            lineRenderer.positionCount -= patrolCount;
        }
        else
            lineRenderer.positionCount = 0;
    }

    private void ToggleCommandPointsUI(bool val)
    {
        // @TODO: this works differently when multiple units are selected, only showing a sticker at a general average location
        // of all the group's movement. Obviously, this could get complicated, like when some units are moving to one area, but others
        // in the selection are moving to another area, and also if they all have different commandTypes for currentCommand, what icon to show?
        lineRenderer.enabled = val;
        foreach (CommandQueueItem item in commandQueue)
        {
            if (item.commandType == CommandTypes.Patrol)
            {
                item.commandSticker.SetActive(val);
                foreach (PatrolPoint pp in item.patrolRoute.patrolPoints)
                {
                    if (pp.sticker)
                        pp.sticker.SetActive(val);
                }
            }
            item.commandSticker.SetActive(val);
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
        // @TODO: if mainPlayer.nextCommandIsPrimed, this should still change to Select cursor, but when moving back, should
        // go back to the primed command mouse cursor
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
