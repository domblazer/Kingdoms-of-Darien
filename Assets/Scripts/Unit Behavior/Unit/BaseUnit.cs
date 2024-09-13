using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DarienEngine;

/// <summary>
/// Class <c>BaseUnit</c> derives from class <c>RTSUnit</c> and implements unit behavior specific to playable units.
/// </summary>
public class BaseUnit : RTSUnit
{
    private bool selected;
    public bool selectable = true;
    public GameObject selectRing;
    public GameObject fogOfWarMask;
    public AttackModes attackMode = AttackModes.Offensive;
    private Directions facingDir = Directions.Forward;
    [HideInInspector] public UnitBuilderPlayer _Builder;
    [HideInInspector] public GameObject secondary;
    private LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        if (selectable)
        {
            selectRing.SetActive(false);
            // Set an initial green color on select-ring
            selectRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, health / 100);
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
                HandleCurrentCommand();
            }
            else
            {
                // No commands, idle
                state = States.Standby;
                TryToggleToObstacle();
                // If idle and in offensive mode, autopick attack target
                if (canAttack && attackMode == AttackModes.Offensive)
                    _AttackBehavior.AutoPickAttackTarget();
            }

            UpdateSecondaryUnit();
            // if (secondary)
            //     Debug.Log(name + " secondary " + secondary);

            if (selectable && selected)
            {
                // Update select ring color based on health when selected
                selectRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, Color.green, health / 100);
                // Debug.Log(gameObject.name + " commandQueue: " + string.Join<CommandQueueItem>(", ", commandQueue.ToArray()));

                // Update the unit info UI if no other unit has focus from hovering
                if (!GameManager.Instance.IsHoveringOther(gameObject))
                {
                    // @TODO: this should only be set if this unit is the singular unit selected, not a group selection
                    UIManager.UnitInfoInstance.Set(super, secondary);
                }

                // Toggle line renderer on shift up/down for mobile units
                // @TODO: if selectedUnits.Count > 0, don't show lines and only show one common point with sticker
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

    private void HandleCurrentCommand()
    {
        switch (currentCommand.commandType)
        {
            case CommandTypes.Move:
                if (canAttack && currentCommand.isAttackMove)
                    _AttackBehavior.AutoPickAttackTarget();
                HandleMovement();
                break;
            case CommandTypes.Attack:
                // handle engaging target (moveTo target) and attacking behaviour
                _AttackBehavior.HandleAttackRoutine();
                state = States.Attacking;
                break;
            case CommandTypes.Patrol:
                // handle patrol behavior
                // @TODO: if patrol point not set?
                // @TODO: auto attack if mode is offensive
                Patrol();
                break;
            case CommandTypes.Conjure:
                TryToggleToAgent();
                if (isKinematic)
                    (_Builder as Builder).HandleConjureRoutine();
                else
                    (_Builder as Factory).HandleConjureRoutine();
                state = States.Conjuring;
                break;
            case CommandTypes.Guard:
                // @TODO: HandleGuardRoutine()
                // @TODO: attack anyone that attacks the guarding unit
                break;
        }
    }

    // Update the secondaryUnit var to set an attacking unit or conjuring unit
    private void UpdateSecondaryUnit()
    {
        string secondaryType = "rtsunit";
        if (_AttackBehavior && _AttackBehavior.attackTarget != null)
        {
            secondary = _AttackBehavior.attackTarget.target;
        }
        else if (_Builder && _Builder.currentIntangible)
        {
            secondary = _Builder.currentIntangible.gameObject;
            secondaryType = "intangibleunit";
        }
        else
        {
            secondary = null;
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
            if (item.commandSticker)
                item.commandSticker.SetActive(val);
        }
    }

    public void Select(bool alone = false)
    {
        if (selectable)
        {
            selected = true;
            selectRing.SetActive(true);

            Debug.Log("select unit");
            Debug.Log("selected count " + mainPlayer.SelectedUnitsCount());
            if (alone)
            {
                // If the unit is a builder, show the build menu when selected
                if (isBuilder)
                {
                    Debug.Log("selected is builder");
                    mainPlayer.SetActiveBuilder(_Builder);
                }
                // Else if lone unit selected was not a builder, try clear current active builder
                else
                {
                    mainPlayer.ReleaseActiveBuilder();
                }
                // Show the unit action menu
                UIManager.BattleMenuInstance.Set(isKinematic, canAttack, isBuilder, _AttackBehavior?.weapons);
                // @TODO: This shouldn't be here. Select can be called many times when selecting a number of units under the square
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
            ToggleCommandPointsUI(false);
        }
    }

    private IEnumerator Die()
    {
        HandleDie();

        // If builder, clear any current build
        if (isBuilder && _Builder.IsBuilding)
            _Builder.CancelBuild();

        // @TODO: If this unit was selected, clear the UI 
        // @TODO: Also, need to update UI when hovering over corpse to show corpse name and handle resurrection cursor and behavior
        UIManager.UnitInfoInstance.Toggle(false);
        // @TODO: should only reset cursor if no other selected units
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);

        DeSelect();
        selectable = false;
        mainPlayer.RemoveUnitFromSelection(this);

        // Units loose line-of-sight when dead
        // @TODO: fogOfWarMask should probably eventually be applied to AIs as well so allied AIs can reveal line-of-sight
        fogOfWarMask?.SetActive(false);

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
        if (!InputManager.IsMouseOverUI() && selectable)
        {
            if (!isDead)
            {
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Select);
                UIManager.UnitInfoInstance.Set(super, null);
            }
            else
            {
                // @TODO: Set UI to display "Corpse"
            }
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
                UIManager.UnitInfoInstance.Toggle(false);
        }
        GameManager.Instance.ClearHovering();
    }

    // Continue to update UI with most recent health, mana, etc. values while mouseover
    private void OnMouseOver()
    {
        // First check if I am already selected
        if (!selected && !isDead)
            UIManager.UnitInfoInstance.Set(super, secondary);

        // @TODO: Set UI to display "Corpse"
    }

    public void SetFacingDir(Directions dir)
    {
        facingDir = dir;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
    }
}
