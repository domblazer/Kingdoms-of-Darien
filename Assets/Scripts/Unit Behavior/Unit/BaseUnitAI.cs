using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using DarienEngine;
using DarienEngine.AI;

/// <summary>
/// Class <c>BaseUnitAI</c> inherits RTSUnit to derive common behavior for all units, AI or otherwise, (e.g. pathfinding, attack routine, etc).
/// BaseUnit must, however, implement the Unity game methods Awake(), Start(), and Update() since "super" is not a thing in C#,
/// so RTSUnit class can't implement any common functionality that happens in those methods.
/// </summary>
public class BaseUnitAI : RTSUnit
{
    private bool enableFogOfWar;

    private readonly List<Renderer> renderers = new();
    private bool alreadyHidden = false;
    private bool alreadyShown = false;
    private float nextSightCheck = 5.0f;
    private readonly float sightCheckRate = 5.0f;
    public float patrolRange = 15.0f;

    public enum StartStates
    {
        Standby, Patrolling, Parking
    }
    public StartStates startState;
    private States defaultState;

    private UnitBuilderAI _Builder;
    public TooltipManager _TooltipManager { get; set; }
    public Army _Army { get; set; }

    private void Awake()
    {
        // @IMPORTANT: CommandQueue needs to know this is for AI so it won't place stickers on commandPoints
        commandQueue.isAI = true;
    }

    private void Start()
    {
        // General set-up in RTSUnit
        Init();

        // Set fog-of-war based on game manager
        enableFogOfWar = GameManager.Instance.enableFogOfWar;

        // Grab all of this unit's mesh renderers
        foreach (Transform child in transform)
            if (child.GetComponent<Renderer>())
                renderers.Add(child.GetComponent<Renderer>());

        // @TODO: if AI is on the same team as player, assign "Friendly-AI" tag
        gameObject.tag = "Enemy";

        // Get builder component, if applicable
        if (isBuilder)
        {
            if (isKinematic)
                _Builder = GetComponent<BuilderAI>();
            else
                _Builder = GetComponent<FactoryAI>();
        }

        // SetPatrolPoints(transform.position, patrolRange);

        // Initialize the starting state
        if (startState == StartStates.Patrolling)
            currentCommand = new CommandQueueItem { commandType = CommandTypes.Patrol, patrolRoute = null };
        // @TODO: map other states to commands 
        else if (startState == StartStates.Standby)
            state = defaultState = States.Standby;

        // Create a tooltip for this unit
        _TooltipManager = GetComponent<TooltipManager>();
        if (_TooltipManager != null)
        {
            _TooltipManager.CreateNewTooltip();
            _TooltipManager.SetTooltipActive(UIManager.Instance.tooltipActive);
        }
        else
            Debug.LogWarning("Note: No TooltipManager found for " + gameObject);
    }

    private void Update()
    {
        // @TODO: Tilda key is usually used to toggle the health bars, for now toggling the army debug panels
        // @TODO: disabled for demo
        /* if (Input.GetKeyDown(KeyCode.BackQuote) && _TooltipManager != null)
            _TooltipManager.ToggleTooltip(); */

        // @TODO: F6 is arbitrary key for this command; toggles FogOfWar in-game
        // @TODO: Implement this later. Toggle for Fog of War may be useful for debugging/general game feature.
        /* if (Input.GetKeyDown(KeyCode.F6))
        {
            enableFogOfWar = !enableFogOfWar;
            if (enableFogOfWar)
            {
                Debug.Log("fogofwar hide triggered");
                TriggerHide();
            }
            else
                TriggerShow();
        } */

        // Update health
        if (!isDead && UpdateHealth() <= 0)
            StartCoroutine(Die());

        // Show/hide based on Fog of War
        if (enableFogOfWar)
            UpdateFogOfWar();

        if (!isDead)
        {
            if (!commandQueue.IsEmpty())
                HandleCurrentCommand();
            else
                state = States.Standby;

            // @TODO: AI should autopick attack target under most circumstances
            if (canAttack && state.Value == States.Standby.Value)
                _AttackBehavior.AutoPickAttackTarget();
        }

        if (_TooltipManager != null && _TooltipManager.IsActive())
        {
            string tooltipText = state.Value + "\n";
            if (_Army != null)
            {
                tooltipText += "In army. \n";
                if (canAttack && _AttackBehavior.attackTarget != null)
                    tooltipText += "Attack target? " + _AttackBehavior.attackTarget.target.name;
            }
            tooltipText += "cluster: " + clusterNum;
            if (_TooltipManager)
                _TooltipManager.SetTooltipText(tooltipText);
        }
    }

    /// <summary> 
    /// This method determines which behavior routines to run given the current command.
    /// </summary>
    private void HandleCurrentCommand()
    {
        switch (currentCommand.commandType)
        {
            case CommandTypes.Move:
                // Auto-pick attack targets by default while moving
                if (canAttack && currentCommand.isAttackMove)
                    _AttackBehavior.AutoPickAttackTarget(true);
                HandleMovement();
                break;
            case CommandTypes.Attack:
                // handle engaging target (moveTo target) and attacking behaviour
                _AttackBehavior.HandleAttackRoutine(true);
                state = States.Attacking;
                break;
            case CommandTypes.Patrol:
                // handle patrol behavior
                // If currentCommand has not yet set a patrol route, set it now
                if (currentCommand.patrolRoute == null)
                    currentCommand.patrolRoute = new PatrolRoute { patrolPoints = SetPatrolPoints(transform.position, 3, patrolRange) };
                // Roam and auto-pick attack targets
                Patrol(true);
                if (canAttack)
                    _AttackBehavior.AutoPickAttackTarget(true);
                break;
            case CommandTypes.Conjure:
                // Builder and Factory conjuring behavior
                if (isKinematic)
                    (_Builder as BuilderAI).HandleConjureRoutine();
                else
                    (_Builder as FactoryAI).HandleConjureRoutine();
                state = States.Conjuring;
                break;
            case CommandTypes.Guard:
                // @TODO 
                break;
        }
    }

    public List<PatrolPoint> SetPatrolPoints(Vector3 origin, int pointCount = 3, float range = 25.0f)
    {
        List<PatrolPoint> patrolPoints = new();
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 randomPoint = GenerateValidRandomPoint(origin, range);
            patrolPoints.Add(new PatrolPoint { point = randomPoint });
        }
        return patrolPoints;
    }

    // Find a random, valid world point for unit to move to
    public Vector3 GenerateValidRandomPoint(Vector3 origin, float range)
    {
        Vector3 validPoint = GenerateRandomPoint(origin, range);
        // @TODO: testLimit may result in a bad point being used, 
        // @TODO: if unit gets stuck, maybe should cancel current routine and try again later?
        int testLimit = 25;
        int currentTest = 0;
        // Keep trying points until TestPoint() returns true
        while (!TestPoint(validPoint) && currentTest < testLimit)
        {
            validPoint = GenerateRandomPoint(origin, range);
            currentTest++;
        }
        validPoint.y = origin.y; // @TODO: terrain height issue
        return validPoint;
    }

    // @TODO: Build points should probably do better than picking randomly, maybe introduce some math
    public Vector3 GenerateValidBuildPoint(Vector3 origin, float range, Vector3 boxExtends)
    {
        Vector3 validPoint = GenerateRandomPoint(origin, range);
        // @TODO: testLimit may result in a bad point being used, 
        // @TODO: if unit gets stuck, maybe should cancel current routine and try again later?
        int testLimit = 50;
        int currentTest = 0;
        // Keep trying points until TestPoint() returns true
        while (!TestBuildPoint(validPoint, boxExtends) && currentTest < testLimit)
        {
            validPoint = GenerateRandomPoint(origin, range);
            currentTest++;
        }
        validPoint.y = origin.y; // @TODO: terrain height issue
        return validPoint;
    }

    // Randomly generate a world point within range of origin
    public Vector3 GenerateRandomPoint(Vector3 origin, float range)
    {
        Vector3 radius = Random.insideUnitSphere;
        Vector3 point = origin + (radius * range);
        // @TODO: introducing different terrain height levels will complicate this. Will need to raycast to world to get exact y value
        point.y = origin.y;
        return point;
    }

    // Test a world point to see if it is a valid point to move to
    private bool TestPoint(Vector3 point)
    {
        return TestNavMeshPoint(point)
            && TestPointCollision(point)
            && GameManager.Instance.mapInfo.PointInsideBounds(point.x, point.z);
    }

    // Test a world point to see if it is valid for a factory
    private bool TestBuildPoint(Vector3 point, Vector3 boxExtends)
    {
        bool one = TestNavMeshPoint(point);
        bool two = TestPointCollision(point, boxExtends);
        // @TODO: buildings can't be lain like half way out of map bounds, need to adjust padding
        bool three = GameManager.Instance.mapInfo.PointInsideBounds(point.x, point.z);
        // Debug.Log("TestBuildPoint results: (" + point + "):\nNavMeshPointValid: " + one + "\nPointCollisionValid: " + two + "\nPointInsideMapBounds: " + three);
        return one && two && three;
    }

    // Check if a point is on the NavMesh and has reasonable distance disparity
    public bool TestNavMeshPoint(Vector3 point)
    {
        bool navMeshValid = false;
        // NavMesh check point is on "Built-in-0": "Walkable" area
        if (NavMesh.SamplePosition(point, out NavMeshHit navMeshHit, 1f, NavMesh.AllAreas))
        {
            NavMeshPath path = new NavMeshPath();
            _Agent.CalculatePath(navMeshHit.position, path);
            if (path.status == NavMeshPathStatus.PathComplete)
                navMeshValid = true;
            // @TODO: test distanceDisparity: if path distance is much larger than distance between points
            // bool distanceDisparity = (navMeshHit.distance * navMeshHit.distance) > (transform.position - point).sqrMagnitude;
        }
        return navMeshValid;
    }

    // Test a small radius around point for collisions with Unit layer
    public bool TestPointCollision(Vector3 point)
    {
        // @TODO: leaving out Obstacle layer check b/c units should be able to move onto berms, for example
        return Physics.OverlapSphere(point, 1.5f, 1 << 9).Length == 0;
    }

    // Test a box area around point for collisions with Unit or Obstacle layers
    public bool TestPointCollision(Vector3 point, Vector3 boxExtends)
    {
        Collider[] cols = Physics.OverlapBox(point, boxExtends / 2, Quaternion.identity, (1 << 7) | (1 << 9));
        // Debug.Log("TestPointCollision: cols: " + string.Join<Collider>(", ", cols.ToArray()));
        return cols.Length == 0;
    }

    void UpdateFogOfWar()
    {
        // Debug.Log("whoCanSeeMe count " + whoCanSeeMe.Count);
        if (whoCanSeeMe.Count == 0)
            TriggerHide();
        else
        {
            // Periodically clear dead or destroyed units from whoCanSeeMe
            // @TODO: this isn't exactly efficient, maybe can register to the RTSUnit.OnDie to remove from whoCanSeeMe
            if (Time.time > nextSightCheck)
            {
                nextSightCheck = Time.time + sightCheckRate;
                whoCanSeeMe = whoCanSeeMe.Where(item => item && item.GetComponent<RTSUnit>() && !item.GetComponent<RTSUnit>().isDead).ToList();
            }
            TriggerShow();
        }
    }

    void TriggerHide()
    {
        if (!alreadyHidden)
            foreach (Renderer r in renderers)
                r.enabled = false;
        alreadyShown = false;
        alreadyHidden = true;
    }

    void TriggerShow()
    {
        if (!alreadyShown)
            foreach (Renderer r in renderers)
                r.enabled = true;
        alreadyHidden = false;
        alreadyShown = true;
    }

    private IEnumerator Die()
    {
        HandleDie();

        // If builder, clear any current build
        if (isBuilder && _Builder.IsBuilding)
            _Builder.CancelBuild();

        if (_TooltipManager)
        {
            _TooltipManager.RemoveTooltip();
            _TooltipManager = null;
        }

        // @TODO: if this AI unit is part of an Army, it must be removed from the Army as well
        yield return new WaitForSeconds(dieTime);
        Destroy(gameObject);
    }

    void OnMouseEnter()
    {
        // Change to Attack cursor if hovering enemy AND at least one friendly unit who canAttack is selected
        if (!InputManager.IsMouseOverUI())
        {
            if (gameObject.tag == "Enemy" && mainPlayer.SelectedAttackUnitsCount() > 0 && !mainPlayer.nextCommandIsPrimed)
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Attack);

            GameManager.Instance.SetHovering(gameObject);
        }
    }

    void OnMouseExit()
    {
        if (!InputManager.IsMouseOverUI())
        {
            if (!mainPlayer.nextCommandIsPrimed)
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
            UIManager.UnitInfoInstance.Toggle(false);
            GameManager.Instance.ClearHovering();
        }
    }

    private void OnMouseOver()
    {
        if (!InputManager.IsMouseOverUI())
            UIManager.UnitInfoInstance.Set(super, null, true);
    }
}
