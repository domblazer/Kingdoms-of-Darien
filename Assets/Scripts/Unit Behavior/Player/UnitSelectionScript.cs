using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitSelectionScript : MonoBehaviour
{
    // To determine if we are clicking with left mouse or holding down left mouse
    public float clickHoldDelay = 0.15f;
    float clickTime = 0f;
    private bool isHoldingDown = false;
    private bool isClicking = false;
    public AudioClip clickSound;

    // The start and end coordinates of the square we are making
    Vector3 squareStartPos;
    Vector3 squareEndPos;
    public RectTransform selectionSquareTrans; // The selection square we draw when we drag the mouse to select units
    bool hasCreatedSquare;
    Vector3 TL, TR, BL, BR; // The selection squares 4 corner positions

    private GameObject _Units;
    private List<GameObject> allUnits = new List<GameObject>();
    private List<GameObject> selectedUnits = new List<GameObject>();
    private GameObject highlightThisUnit;

    private Dictionary<string, int> unitOrdersByName = new Dictionary<string, int>() {
        {"Horseman", 0},
        {"Swordsman", 1},
        {"Archer", 2},
        {"Catapult", 3},
    };

    private struct UnitGroup
    {
        public int order { get; set; }
        public string name { get; set; }
        public List<GameObject> units;

        public UnitGroup(int o, string n)
        {
            order = o;
            name = n;
            units = new List<GameObject>();
        }

        public override string ToString()
        {
            return "group (name: " + name + ", order: " + order + ", count: " + units.Count + ")";
        }
    }

    private List<UnitGroup> unitGroups = new List<UnitGroup>();

    // Start is called before the first frame update
    void Start()
    {
        CursorManager.Instance.OnCursorChanged += Instance_OnCursorChanged;

        // @Note: player in skirmish is always Player1
        _Units = GameObject.Find("_Units_Player1"); // This needs to remain in Start() since _Units_1 is created on Awake() in BaseUnitScript
        RefreshAllUnits();
    }

    // Update is called once per frame
    void Update()
    {
        // Select one or several units by clicking or draging the mouse
        SelectUnits();

        // Clear selection with right-click
        if (Input.GetMouseButtonDown(1))
        {
            ClearSelectedUnits();
        }
    }

    private void Instance_OnCursorChanged(object sender, CursorManager.OnCursorChangedEventArgs e)
    {
        if (e.cursorType == CursorManager.CursorType.Normal)
        {
            if (selectedUnits.Count > 0)
            {
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Move);
            }
        }
    }

    public void RefreshAllUnits()
    {
        if (_Units)
        {
            allUnits.Clear();
            foreach (Transform child in _Units.transform)
            {
                allUnits.Add(child.gameObject);
            }
        }
    }

    void SelectUnits()
    {
        // Are we clicking with left mouse or holding down left mouse
        isClicking = false;
        isHoldingDown = false;

        // Click the mouse button
        if (Input.GetMouseButtonDown(0) && !InputManager.IsMouseOverUI())
        {
            clickTime = Time.time;
            // We dont yet know if we are drawing a square, but we need the first coordinate in case we do draw a square
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                squareStartPos = hit.point; // The corner position of the square
        }

        // @TODO: double click on a unit will select all units of same type within camera view

        // Release the mouse button
        if (Input.GetMouseButtonUp(0))
        {
            if (Time.time - clickTime <= clickHoldDelay)
                isClicking = true;

            // Select all units within the square if we have created a square
            if (hasCreatedSquare)
            {
                hasCreatedSquare = false;
                selectionSquareTrans.gameObject.SetActive(false); // Deactivate the square selection image

                // If holding shift, don't clear so current selection will add to selected
                // @TODO: need to subtract already selected units within the current square
                if (!InputManager.HoldingShift())
                    selectedUnits.Clear(); // Clear the list with selected unit

                // Select the units
                HandleUnitsUnderSquare();
            }
            else if (!InputManager.IsMouseOverUI() && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                // Here is where units should be told to move
                // @TODO: should compare against Unit layer
                if (!hit.collider.CompareTag("Friendly") && !hit.collider.CompareTag("Enemy"))
                {
                    bool doAttackMove = InputManager.HoldingCtrl();
                    bool addToMoveQueue = InputManager.HoldingShift();

                    // @TODO: if InputManager.HoldingShift() need to add this point to an array of positions for the unit to travel to sequentially
                    // @TODO: at this click point, need to instantiate sprite object that will show/hide depending on who is selected and holding shift
                    // so, need to queue the sprite object with the transform as well

                    // Handle group movement
                    if (selectedUnits.Count > 1)
                    {
                        MoveGroup(hit.point, addToMoveQueue, doAttackMove);
                    }
                    else if (selectedUnits.Count == 1)
                    {
                        // Just move the single selected unit directly to click point
                        BaseUnitScript unit = selectedUnits[0].GetComponent<BaseUnitScript>();
                        unit.SetMove(hit.point, addToMoveQueue, doAttackMove);
                        unit.PlayMoveSound();
                    }
                    GetComponent<AudioSource>().PlayOneShot(clickSound);
                }
            }
            if (selectedUnits.Count == 0 && !InputManager.IsMouseOverUI())
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
            // GroupSelectedByName();
        }

        // Holding down the mouse button
        if (Input.GetMouseButton(0) && !InputManager.IsMouseOverUI())
            if (Time.time - clickTime > clickHoldDelay)
                isHoldingDown = true;

        // Select one unit with left mouse and deselect all units with left mouse by clicking on what's not a unit
        if (isClicking)
        {
            // Try to select a new unit
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                // Did we hit a friendly unit?
                // @TODO: intangibles are also on unit layer with friendly tag, but should not be clickable like this
                if (hit.collider.CompareTag("Friendly"))
                {
                    // Deselect all units when clicking single other unit, unless holding shift
                    if (!InputManager.HoldingShift())
                    {
                        foreach (GameObject unit in selectedUnits)
                            unit.GetComponent<BaseUnitScript>().DeSelect();

                        selectedUnits.Clear();
                    }

                    GameObject activeUnit = hit.collider.gameObject;
                    if (activeUnit.GetComponent<BaseUnitScript>().selectable)
                    {
                        // Play click sound
                        if (!InputManager.HoldingShift())
                            GetComponent<AudioSource>().PlayOneShot(clickSound);

                        activeUnit.GetComponent<BaseUnitScript>().Select(true); // Set this unit to selected with param alone=true
                        selectedUnits.Add(activeUnit); // Add it to the list of selected units, which is now just 1 unit
                    }
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    // If we clicked an Enemy unit while at least one canAttack unit is selected, tell those/that unit to attack
                    foreach (GameObject unit in selectedUnits)
                        unit.GetComponent<BaseUnitScript>().TryAttack(hit.collider.gameObject);
                }
            }
        }

        // If holding down and mouse has been dragged, select all units within the square
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit movedPosition);
        if (isHoldingDown && squareStartPos != movedPosition.point)
        {
            // Display the selection UI image
            DisplaySquare();
            // Highlight the units within the selection square, but don't select the units
            if (hasCreatedSquare)
                HandleUnitsUnderSquare(true);
        }
    }

    public void HandleUnitsUnderSquare(bool highlightOnly = false)
    {
        foreach (GameObject currentUnit in allUnits)
        {
            // Is this unit within the square
            if (IsWithinPolygon(currentUnit.transform.position) && currentUnit.GetComponent<BaseUnitScript>().selectable)
            {
                currentUnit.GetComponent<BaseUnitScript>().Select();
                // Add to the selection if not just highlighting
                if (!highlightOnly)
                    selectedUnits.Add(currentUnit);
            }
            else if (!InputManager.HoldingShift())
                currentUnit.GetComponent<BaseUnitScript>().DeSelect();
        }
    }

    private void MoveGroup(Vector3 hitPoint, bool addToMoveQueue = false, bool attackMove = false)
    {
        UnitClusterMoveInfo clusterMoveInfo = CalculateSmartCenter(selectedUnits);
        foreach (GameObject unit in selectedUnits)
        {
            Vector3 offset = (unit.transform.position - clusterMoveInfo.smartCenter);
            Vector3 moveTo = hitPoint + offset;

            // If unit is outside the normal distribution, consider it outside primary cluster and must adjust move to collapse in
            if (offset.sqrMagnitude > clusterMoveInfo.standardDeviation)
            {
                // @TODO: need to use offset direction with stdDev magnitude
                moveTo = hitPoint + (offset.normalized * 2); // new Vector3(hitPoint.x + clusterMoveInfo.standardDeviation, hitPoint.y, hitPoint.z + clusterMoveInfo.standardDeviation);
                Debug.Log("I " + unit.name + " am outside the primary cluster, moving to " + moveTo);
            }
            // @TODO: also need to make sure moveTo points don't overlap or get too close to eachother
            // @TODO // if (noPrimaryCluster) // everyone collapse in around click point naively?

            BaseUnitScript unitScript = unit.GetComponent<BaseUnitScript>();
            if (unitScript && unitScript.isKinematic)
                unitScript.SetMove(moveTo, addToMoveQueue, attackMove);
        }
    }

    public UnitClusterMoveInfo CalculateSmartCenter(List<GameObject> group)
    {
        // Calculate "SmartCenter" of the selected units
        //   1. Calculate average position of selectedUnits
        //   2. Remove any units that aren’t within 1 standard deviation of that average
        //   3. Recalculate the average position from that subset of the group
        List<DescriptUnit> descriptUnits = new List<DescriptUnit>();
        Vector3 mean = Vector3.zero;
        List<Vector3> positions = new List<Vector3>();
        foreach (GameObject unit in group)
        {
            positions.Add(unit.transform.position);
            mean += unit.transform.position;
            // @TODO see if I can use median
        }
        mean = mean / group.Count;

        // Take the sum of the squared lengths of differences with the average
        float sumOfSquares = positions.Sum(d => ((d - mean).sqrMagnitude));
        // @Note: normally you take the square root of (sum/count), but that's expensive and unnecessary for this task
        float stdd = (sumOfSquares) / (positions.Count() - 1);

        // Check if there is a significant cluster of units by summing their radii and comparing against standard deviation
        float radiiSum = group.Sum(d => d.GetComponent<RTSUnit>().offset.x);
        bool primaryClusterExists = false;
        if (radiiSum > stdd)
        {
            Debug.Log("We have a primary cluster.");
            primaryClusterExists = true;
        }

        // Now calculate the adjusted mean
        Vector3 adjustedMean = Vector3.zero;
        int adjustedCount = 0;
        foreach (GameObject unit in group)
        {
            // If unit is within one standard deviation of the initial average, include it in the adjusted set
            if ((unit.transform.position - mean).sqrMagnitude <= stdd)
            {
                adjustedCount++;
                adjustedMean += unit.transform.position;
            }
            else
            {
                Debug.Log("This unit " + unit.name + " is not within 1 standard deviation of average position.");
                // @TODO: this unit needs to be told to go to hitPoint + offset(with magnitude = stdd)
            }
        }
        adjustedMean = adjustedMean / adjustedCount;
        Debug.Log("Adjusted average: " + adjustedMean);

        // If a primary cluster exists but at least 1 unit is more than 1 standard deviation away from mean
        // if (primaryClusterExists && adjustedCount < group.Count)

        return new UnitClusterMoveInfo
        {
            descriptUnits = descriptUnits,
            smartCenter = adjustedMean,
            standardDeviation = stdd,
            primaryClusterExists = primaryClusterExists,
            primaryClusterCount = adjustedCount,
            outlierCount = group.Count - adjustedCount
        };
    }

    public class UnitClusterMoveInfo
    {
        public List<DescriptUnit> descriptUnits = new List<DescriptUnit>();
        public bool primaryClusterExists = false;
        public float standardDeviation = 0.0f;
        public Vector3 smartCenter;
        public int primaryClusterCount;
        public int outlierCount;
    }

    public class DescriptUnit
    {
        GameObject unit;
        public bool isOutsidePrimaryCluster = false;
    }

    // Is a unit within a polygon determined by 4 corners
    bool IsWithinPolygon(Vector3 unitPos)
    {
        bool isWithinPolygon = false;

        // The polygon forms 2 triangles, so we need to check if a point is within any of the triangles
        // Triangle 1: TL - BL - TR
        if (IsWithinTriangle(unitPos, TL, BL, TR))
        {
            return true;
        }

        // Triangle 2: TR - BL - BR
        if (IsWithinTriangle(unitPos, TR, BL, BR))
        {
            return true;
        }

        return isWithinPolygon;
    }

    // Is a point within a triangle
    // From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    bool IsWithinTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        bool isWithinTriangle = false;

        // Need to set z -> y because of other coordinate system
        float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));

        float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
        float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
        float c = 1 - a - b;

        // The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }

    // Display the selection with a GUI square
    void DisplaySquare()
    {
        // Activate the square selection image
        if (!selectionSquareTrans.gameObject.activeInHierarchy)
        {
            selectionSquareTrans.gameObject.SetActive(true);
        }
        // @TODO: for some reason this isn't allowing the square to shrink
        squareEndPos = Input.mousePosition; // Get the latest coordinate of the square

        // The start position of the square is in 3d space, or the first coordinate will move as we move the camera which is not what we want
        Vector3 squareStartScreen = Camera.main.WorldToScreenPoint(squareStartPos);
        squareStartScreen.z = 0f;
        Vector3 middle = (squareStartScreen + squareEndPos) / 2f; // Get the middle position of the square
        selectionSquareTrans.position = middle; // Set the middle position of the GUI square

        // Change the size of the square
        float sizeX = Mathf.Abs(squareStartScreen.x - squareEndPos.x);
        float sizeY = Mathf.Abs(squareStartScreen.y - squareEndPos.y);
        selectionSquareTrans.sizeDelta = new Vector2(sizeX, sizeY); // Set the size of the square

        // The problem is that the corners in the 2d square is not the same as in 3d space
        // To get corners, we have to fire a ray from the screen
        // We have 2 of the corner positions, but we don't know which, so we can figure it out or fire 4 raycasts
        TL = new Vector3(middle.x - sizeX / 2f, middle.y + sizeY / 2f, 0f);
        TR = new Vector3(middle.x + sizeX / 2f, middle.y + sizeY / 2f, 0f);
        BL = new Vector3(middle.x - sizeX / 2f, middle.y - sizeY / 2f, 0f);
        BR = new Vector3(middle.x + sizeX / 2f, middle.y - sizeY / 2f, 0f);

        RaycastHit hit;
        int i = 0;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(TL), out hit))
        {
            TL = hit.point;
            i++;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(TR), out hit))
        {
            TR = hit.point;
            i++;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(BL), out hit))
        {
            BL = hit.point;
            i++;
        }
        if (Physics.Raycast(Camera.main.ScreenPointToRay(BR), out hit))
        {
            BR = hit.point;
            i++;
        }

        hasCreatedSquare = i == 4; // If we could find 4 points
    }

    public int SelectedUnitsCount()
    {
        return selectedUnits.Count;
    }

    public void ClearSelectedUnits()
    {
        selectedUnits.Clear();
    }

    public int SelectedAttackUnitsCount()
    {
        int count = 0;
        foreach (GameObject unit in selectedUnits)
        {
            if (unit.GetComponent<BaseUnitScript>().canAttack)
            {
                count++;
            }
        }
        return count;
    }

    public void RemoveUnitFromSelection(GameObject go)
    {
        selectedUnits.Remove(go);
    }

    public void RemoveUnitFromTotal(GameObject go)
    {
        allUnits.Remove(go);
    }
}
