using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

/* 
    This class inherits RTSUnit to derive common behavior for all units, AI or otherwise, (e.g. pathfinding, attack routine, etc).
    BaseUnit must, however, implement the Unity game methods Awake(), Start(), and Update() since "super" is not a thing in C#,
    so RTSUnit class can't implement any common functionality that happens in those methods.
*/
public class BaseUnitScriptAI : RTSUnit
{
    public float patrolRange = 25.0f;
    public int patrolPointsCount = 3;
    private List<Vector3> patrolPoints = new List<Vector3>();
    private int patrolIndex = 0;

    public bool enableFogOfWar = true;

    private List<Renderer> renderers = new List<Renderer>();
    private bool alreadyHidden = false;
    private bool alreadyShown = false;
    private float nextSightCheck = 5.0f;
    private float sightCheckRate = 5.0f;

    public enum StartStates
    {
        Standby, Patrolling
    }
    public StartStates startState;
    private States defaultState;

    private void Awake()
    {
        // @TODO: implement moveToPositionQueue
        moveToPosition = transform.position;

        GameObject _Units = GameObject.Find("_Units_" + playerNumber);
        if (!_Units)
            _Units = new GameObject("_Units_" + playerNumber);
        transform.parent = _Units.transform;
    }

    private void Start()
    {
        // Grab all of this unit's mesh renderers
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>())
            {
                renderers.Add(child.GetComponent<Renderer>());
            }
        }

        Init();

        // @TODO: if AI is on the same team as player, assign "Friendly-AI" tag
        gameObject.tag = "Enemy";

        SetPatrolPoints(transform.position);

        // Initialize the starting state
        if (startState == StartStates.Patrolling)
            state = defaultState = States.Patrolling;
        else if (startState == StartStates.Standby)
            state = defaultState = States.Standby;

        // @TODO: handle parking when built by a unitBuilderAI then switch to patrolling around park point
    }

    private void Update()
    {
        // Update health
        UpdateHealth();

        // Show/hide based on Fog of War
        if (enableFogOfWar)
            UpdateFogOfWar();

        if (!isDead)
        {
            if (!IsAttacking() && !engagingTarget)
                state = defaultState;

            if (isKinematic)
            {
                if (state.Value == States.Patrolling.Value)
                    Patrol();
                else
                    HandleMovement();
            }

            // Handle attack states if can attack
            if (canAttack)
            {
                HandleAttackRoutine();
                AutoPickAttackTarget();
            }
        }
    }

    void SetPatrolPoints(Vector3 origin)
    {
        patrolPoints.Clear();
        for (int i = 0; i < patrolPointsCount; i++)
        {
            // @TODO: obviously need to check if these points are on the navmesh and not in restricted areas like water
            Vector3 radius = Random.insideUnitSphere;
            Vector3 randomPoint = (origin + (radius * patrolRange));
            randomPoint.y = origin.y;
            patrolPoints.Add(randomPoint);
        }
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

    void Patrol()
    {
        // Add extra steering while moving
        if (IsMoving())
            HandleFacing(_Agent.steeringTarget, 0.25f);
        // Cycle random patrol points
        if (!IsInRangeOf(patrolPoints[patrolIndex], 2))
            _Agent.SetDestination(patrolPoints[patrolIndex]);
        else if (IsInRangeOf(patrolPoints[patrolIndex], 2))
            patrolIndex = Random.Range(0, patrolPoints.Count);
    }

    void TriggerHide()
    {
        if (!alreadyHidden)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
        }
        alreadyShown = false;
        alreadyHidden = true;
    }

    void TriggerShow()
    {
        if (!alreadyShown)
        {
            foreach (Renderer r in renderers)
            {
                r.enabled = true;
            }
        }
        alreadyHidden = false;
        alreadyShown = true;
    }

    void OnMouseEnter()
    {
        // Change to Attack cursor if hovering enemy AND at least one friendly unit who canAttack is selected
        if (gameObject.tag == "Enemy" && _UnitSelection.SelectedAttackUnitsCount() > 0)
        {
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Attack);
        }
        GameManagerScript.Instance.SetHovering(gameObject);
    }

    void OnMouseExit()
    {
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
        UIManager.Instance.unitInfoInstance.Toggle(false);
        GameManagerScript.Instance.ClearHovering();
    }

    private void OnMouseOver()
    {
        UIManager.Instance.unitInfoInstance.Set(super, null, true);
    }
}
