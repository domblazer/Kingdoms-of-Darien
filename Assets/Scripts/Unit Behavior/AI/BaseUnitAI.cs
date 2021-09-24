using System.Collections;
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
    public float patrolRange = 25.0f;
    public int patrolPointsCount = 3;
    private List<Vector3> patrolPoints = new List<Vector3>();
    private int patrolIndex = 0;
    private bool enableFogOfWar;

    private List<Renderer> renderers = new List<Renderer>();
    private bool alreadyHidden = false;
    private bool alreadyShown = false;
    private float nextSightCheck = 5.0f;
    private float sightCheckRate = 5.0f;

    public enum StartStates
    {
        Standby, Patrolling, Parking
    }
    public StartStates startState;
    private States defaultState;

    private UnitBuilderAI _Builder;

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

        SetPatrolPoints(transform.position, patrolRange);

        // Initialize the starting state
        if (startState == StartStates.Patrolling)
            state = defaultState = States.Patrolling;
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
            // Set the state properly
            // DetermineCurrentState<AIConjurerArgs>(_Builder);

            if (isKinematic)
            {
                // @TODO: Need different way to check if this unit should patrol
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

    public void SetPatrolPoints(Vector3 origin, float range = 25.0f)
    {
        patrolPoints.Clear();
        for (int i = 0; i < patrolPointsCount; i++)
        {
            Vector3 radius = Random.insideUnitSphere;
            Vector3 randomPoint = GenerateValidRandomPoint(origin, range);
            patrolPoints.Add(randomPoint);
        }
    }

    public Vector3 GenerateValidRandomPoint(Vector3 origin, float range)
    {
        Vector3 radius = Random.insideUnitSphere;
        Vector3 validPoint = (origin + (radius * range));
        validPoint.y = origin.y;
        int testLimit = 5;
        int currentTest = 0;
        // @TODO: seems possible units could get stuck and a bad point could get added, but
        // we can't just run this loop forever; maybe need a routine to get out of the loop if it's run too many times
        // and the point is still bad. Like maybe at that point, increase range? Or just find some default location and
        // just go there without testing?
        TestPointInfo info;
        while (!(info = TestPoint(validPoint)).valid && currentTest < testLimit)
        {
            Debug.Log(info.message);
            // Keep trying points until TestPoint() returns true
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
    private TestPointInfo TestPoint(Vector3 point)
    {
        bool navMeshValid = false;
        bool pointCollisionValid = true;
        string errorReasons = "Test point results: ";

        NavMeshHit navMeshHit;
        // NavMesh check point is on "Built-in-0": "Walkable" area
        if (NavMesh.SamplePosition(point, out navMeshHit, 1f, NavMesh.AllAreas))
            navMeshValid = true;
        else
            errorReasons += "(Error: NavMesh)";

        // Layer 9 is unit layer
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

    // @TODO: need to move patrol behavior into commandQueue
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
        if (gameObject.tag == "Enemy" && mainPlayer.SelectedAttackUnitsCount() > 0)
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Attack);
        GameManager.Instance.SetHovering(gameObject);
    }

    void OnMouseExit()
    {
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
        UIManager.Instance.unitInfoInstance.Toggle(false);
        GameManager.Instance.ClearHovering();
    }

    private void OnMouseOver()
    {
        UIManager.Instance.unitInfoInstance.Set(super, null, true);
    }
}
