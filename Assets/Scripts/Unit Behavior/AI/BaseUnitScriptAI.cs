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

    private List<Renderer> renderers = new List<Renderer>();
    private bool alreadyHidden = false;
    private bool alreadyShown = false;
    private float nextSightCheck = 5.0f;
    private float sightCheckRate = 5.0f;

    public enum StartStates
    {
        Standby, Patrol
    }
    // @TODO: eventually this should evolve into a "scenario" driven framework, 
    // i.e. when some event occurs (possibly trigger enter on some objective trigger object), change state
    public StartStates startState = StartStates.Standby;

    private void Awake()
    {
        // @TODO: implement moveToPositionQueue
        moveToPosition = transform.position;
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

        for (int i = 0; i < patrolPointsCount; i++)
        {
            // @TODO: obviously need to check if these points are on the navmesh and not in restricted areas like water
            Vector3 radius = Random.insideUnitSphere;
            Vector3 randomPoint = (transform.position + (radius * patrolRange));
            randomPoint.y = transform.position.y;
            patrolPoints.Add(randomPoint);
        }
    }

    private void Update()
    {
        // Show/hide based on Fog of War
        if (whoCanSeeMe.Count == 0)
        {
            TriggerHide();
        }
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

        UpdateHealth();

        if (!isDead)
        {
            if (isKinematic)
            {
                // @TODO: start states obviously need to be able to change after start
                if (startState == StartStates.Patrol)
                {
                    Patrol();
                }
                else if (startState == StartStates.Standby)
                {
                    HandleMovement();
                }
            }

            // Handle attack states if can attack
            if (canAttack)
            {
                HandleAttackRoutine();
                AutoPickAttackTarget();
            }
        }
    }

    void Patrol()
    {
        // Add extra steering while moving
        if (IsMoving())
        {
            HandleFacing(_Agent.steeringTarget, 0.25f);
        }
        if (!IsInRangeOf(patrolPoints[patrolIndex], 2))
        {
            _Agent.SetDestination(patrolPoints[patrolIndex]);
        }
        else if (IsInRangeOf(patrolPoints[patrolIndex], 2))
        {
            patrolIndex = Random.Range(0, patrolPoints.Count);
        }
    }

    void TriggerHide()
    {
        if (!alreadyHidden)
        {
            Debug.Log("hide triggered");
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
            Debug.Log("show triggered");
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
