﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using DarienEngine;
using DarienEngine.AI;

/* 
    This class inherits RTSUnit to derive common behavior for all units, AI or otherwise, (e.g. pathfinding, attack routine, etc).
    BaseUnit must, however, implement the Unity game methods Awake(), Start(), and Update() since "super" is not a thing in C#,
    so RTSUnit class can't implement any common functionality that happens in those methods.
*/
public class BaseUnitAI : RTSUnit
{
    private bool enableFogOfWar;

    private List<Renderer> renderers = new List<Renderer>();
    private bool alreadyHidden = false;
    private bool alreadyShown = false;
    private float nextSightCheck = 5.0f;
    private float sightCheckRate = 5.0f;
    public float patrolRange = 15.0f;

    public enum StartStates
    {
        Standby, Patrolling, Parking
    }
    public StartStates startState;
    private States defaultState;

    private UnitBuilderAI _Builder;

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
    }

    private void Update()
    {
        // Update health
        if (!isDead && UpdateHealth() <= 0)
            StartCoroutine(Die());

        // Show/hide based on Fog of War
        if (enableFogOfWar)
            UpdateFogOfWar();

        if (!isDead)
        {
            if (!commandQueue.IsEmpty())
            {
                switch (currentCommand.commandType)
                {
                    case CommandTypes.Move:
                        // just handle move behaviour
                        HandleMovement();
                        // Auto-pick attack targets by default while moving
                        if (canAttack)
                            _AttackBehavior.AutoPickAttackTarget(true);
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
            else
            {
                state = States.Standby;
            }

            // @TODO: AI should autopick attack target under most circumstances
            if (canAttack && state.Value == States.Standby.Value)
                _AttackBehavior.AutoPickAttackTarget();
        }
    }

    public List<PatrolPoint> SetPatrolPoints(Vector3 origin, int pointCount = 3, float range = 25.0f)
    {
        List<PatrolPoint> patrolPoints = new List<PatrolPoint>();
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 radius = Random.insideUnitSphere;
            Vector3 randomPoint = GenerateValidRandomPoint(origin, range);
            patrolPoints.Add(new PatrolPoint { point = randomPoint });
        }
        return patrolPoints;
    }

    // Find a random world point for unit to move to
    public Vector3 GenerateValidRandomPoint(Vector3 origin, float range)
    {
        Vector3 radius = Random.insideUnitSphere;
        Vector3 validPoint = (origin + (radius * range));
        validPoint.y = origin.y;
        int testLimit = 5;
        int currentTest = 0;
        TestPointInfo info;
        // Keep trying points until TestPoint() returns true
        while (!(info = TestPoint(validPoint)).valid && currentTest < testLimit)
        {
            // Debug.Log(info.message);
            radius = Random.insideUnitSphere;
            validPoint = (origin + (radius * range));
            validPoint.y = origin.y;
            currentTest++;
        }
        validPoint.y = origin.y;
        return validPoint;
    }

    private class TestPointInfo
    {
        public bool valid = false;
        public string message = "N/A";
    }
    // Test a world point to see if it is a valid point to move to
    private TestPointInfo TestPoint(Vector3 point)
    {
        bool navMeshValid = false;
        bool pointCollisionValid = true;
        string errorReasons = "Test point results: ";

        // @TODO: PRIORITY: outside map bounds is also invalid

        NavMeshHit navMeshHit;
        // NavMesh check point is on "Built-in-0": "Walkable" area
        if (NavMesh.SamplePosition(point, out navMeshHit, 1f, NavMesh.AllAreas))
        {
            // @TODO: for builder, maybe test four corners around point based on unit offset values?

            // If nav distance is much larger than distance between points, not a good point
            bool distanceDisparity = navMeshHit.distance > (transform.position - point).sqrMagnitude * 5;
            // If point is outside the map boundary, bad point
            bool insideMapBounds = GameManager.Instance.mapInfo.PointInsideBounds(point.x, point.z);
            if (!distanceDisparity && insideMapBounds)
                navMeshValid = true;
        }
        else
            errorReasons += "(Error: NavMesh)";

        // Layer 9 is unit layer, so check this point is not occupied already by another unit
        int layerMask = 1 << 9;
        Collider[] hitColliders = Physics.OverlapSphere(point, 1.5f, layerMask);
        if (hitColliders.Length > 0)
        {
            errorReasons += "(Error: PointCollision)";
            // errorReasons += "(Error: " + gameObject.name + " PointCollision: " + string.Join<Collider>(", ", hitColliders) + ")";
            pointCollisionValid = false;
        }

        return new TestPointInfo
        {
            valid = navMeshValid && pointCollisionValid,
            message = errorReasons
        };
    }

    void UpdateFogOfWar()
    {
        if (whoCanSeeMe.Count == 0)
            TriggerHide();
        else
        {
            // Periodically clear dead or destroyed units from whoCanSeeMe
            if (Time.time > nextSightCheck)
            {
                nextSightCheck = Time.time + sightCheckRate;
                whoCanSeeMe = whoCanSeeMe.Where(item => item != null && !item.GetComponent<RTSUnit>().isDead).ToList();
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
