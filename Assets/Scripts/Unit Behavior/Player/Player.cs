using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.Clustering;

public class Player : MonoBehaviour
{
    // Human player is always Player1
    private PlayerNumbers playerNumber = PlayerNumbers.Player1;
    public TeamNumbers teamNumber;

    // To determine if we are clicking with left mouse or holding down left mouse
    public float clickHoldDelay = 0.2f;
    private float clickTime = 0f;
    private bool isHoldingDown = false;
    private bool isClicking = false;
    private bool goodHit = false;

    // The start and end coordinates of the square we are making
    private Vector3 squareStartPos;
    private Vector3 squareEndPos;
    private bool hasCreatedSquare;
    // The selection squares 4 corner positions
    private Vector3 TL, TR, BL, BR;

    private List<BaseUnit> selectedUnits = new List<BaseUnit>();

    // The selection square we draw when we drag the mouse to select units
    public RectTransform selectionSquareTrans;
    public AudioClip clickSound;

    // [HideInInspector]
    public UnitBuilderBase<PlayerConjurerArgs> currentActiveBuilder;
    public Inventory inventory;

    // Called at run-time in GameManager.Awake(), when Player component initialized
    public void Init(Inventory inv, RectTransform square, AudioClip sound)
    {
        inventory = inv;
        selectionSquareTrans = square;
        clickSound = sound;
        selectionSquareTrans.gameObject.SetActive(false);
    }

    void Start()
    {
        CursorManager.Instance.OnCursorChanged += Instance_OnCursorChanged;
    }

    private void Instance_OnCursorChanged(object sender, CursorManager.OnCursorChangedEventArgs e)
    {
        if (e.cursorType == CursorManager.CursorType.Normal)
            if (selectedUnits.Count > 0)
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Move);
    }

    // Update is called once per frame
    void Update()
    {
        SelectUnits();

        // Clear selection with right-click
        if (Input.GetMouseButtonDown(1))
            ClearAll();
    }

    private void SelectUnits()
    {
        // Are we clicking with left mouse or holding down left mouse
        isClicking = false;
        isHoldingDown = false;

        goodHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

        // Click the mouse button
        if (Input.GetMouseButtonDown(0) && !InputManager.IsMouseOverUI())
        {
            clickTime = Time.time;
            // We dont yet know if we are drawing a square, but we need the first coordinate in case we do draw a square
            if (goodHit)
                squareStartPos = hit.point; // The corner position of the square
        }

        // Release the mouse button
        if (Input.GetMouseButtonUp(0))
            HandleMouseRelease(hit);

        // Holding down the mouse button
        if (Input.GetMouseButton(0) && !InputManager.IsMouseOverUI())
            if (Time.time - clickTime > clickHoldDelay)
                isHoldingDown = true;

        // Select one unit with left mouse and deselect all units with left mouse by clicking on what's not a unit
        if (isClicking)
            HandleUnitClicked(hit);

        // If holding down and mouse has been dragged, select all units within the square
        // Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit movedPosition);
        if (isHoldingDown && goodHit && squareStartPos != hit.point)
        {
            // Display the selection UI image
            DisplaySquare();
            // Highlight the units within the selection square, but don't select the units
            if (hasCreatedSquare)
                HandleUnitsUnderSquare(true);
        }
    }

    private void HandleMouseRelease(RaycastHit hit)
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
        else if (!InputManager.IsMouseOverUI() && goodHit)
        {
            // Handle click-to-move command here
            HandleMoveCommand(hit);
        }

        if (selectedUnits.Count == 0 && !InputManager.IsMouseOverUI())
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
    }

    // Select units under square
    public void HandleUnitsUnderSquare(bool highlightOnly = false)
    {
        foreach (BaseUnit currentUnit in inventory.totalUnits)
        {
            // Is this unit within the square
            if (currentUnit.selectable && IsWithinPolygon(currentUnit.transform.position))
            {
                currentUnit.Select();
                // Add to the selection if not just highlighting
                if (!highlightOnly)
                    selectedUnits.Add(currentUnit);
            }
            else if (!InputManager.HoldingShift())
                currentUnit.DeSelect();
        }
        // Release any active builder if we just selected more than 1 unit
        if (!highlightOnly && selectedUnits.Count > 1)
            ReleaseActiveBuilder();
    }

    // Handle click-to-move command for selected, kinematic units
    private void HandleMoveCommand(RaycastHit hit)
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
                Clusters.MoveGroup(selectedUnits, hit.point, addToMoveQueue, doAttackMove);
            }
            else if (selectedUnits.Count == 1)
            {
                // Just move the single selected unit directly to click point
                BaseUnit unit = selectedUnits[0];
                unit.SetMove(hit.point, addToMoveQueue, doAttackMove);
                unit.AudioManager.PlayMoveSound();
            }
            // TODO: conflict with unit.PlayMoveSound()?
            GameManager.Instance.AudioSource.PlayOneShot(clickSound);
        }
    }

    // Handle if a single unit was clicked
    private void HandleUnitClicked(RaycastHit hit)
    {
        if (goodHit)
        {
            // Did we hit a friendly unit?
            // @TODO: intangibles are also on unit layer with friendly tag, but should not be clickable like this
            if (hit.collider.CompareTag("Friendly"))
            {
                // Deselect all units when clicking single other unit, unless holding shift
                if (!InputManager.HoldingShift())
                    ClearSelectedUnits();

                BaseUnit activeUnit = hit.collider.gameObject.GetComponent<BaseUnit>();
                if (activeUnit.selectable)
                {
                    // Play click sound
                    if (!InputManager.HoldingShift())
                        GameManager.Instance.AudioSource.PlayOneShot(clickSound);
                    // Select this unit alone
                    activeUnit.Select(true);
                    selectedUnits.Add(activeUnit);
                }
            }
            else if (hit.collider.CompareTag("Enemy"))
            {
                // If we clicked an Enemy unit while at least one canAttack unit is selected, tell those/that unit to attack
                foreach (BaseUnit unit in selectedUnits)
                    unit.TryAttack(hit.collider.gameObject);
            }
        }
    }

    public int SelectedUnitsCount()
    {
        return selectedUnits.Count;
    }

    public void ClearAll()
    {
        // Clear selected units and release current builder
        ClearSelectedUnits();
        ReleaseActiveBuilder();
        // Hide the action menus and reset cursor
        if (UIManager.Instance.actionMenuInstance.actionMenuActive)
            UIManager.Instance.actionMenuInstance.Toggle(false);
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
        UIManager.Instance.unitInfoInstance.Toggle(false);
    }

    public void SetActiveBuilder(UnitBuilderBase<PlayerConjurerArgs> builder)
    {
        // Release the current active builder before setting new one
        if (currentActiveBuilder != null)
            ReleaseActiveBuilder();
        // Set new current active builder
        currentActiveBuilder = builder;
        if (currentActiveBuilder.IsFactory())
            (currentActiveBuilder as Factory).SetCurrentActive();
        else
            (currentActiveBuilder as Builder).SetCurrentActive();
    }

    public void ReleaseActiveBuilder()
    {
        if (currentActiveBuilder != null)
        {
            if (currentActiveBuilder.IsFactory())
                (currentActiveBuilder as Factory).ReleaseCurrentActive();
            else
                (currentActiveBuilder as Builder).ReleaseCurrentActive();
            currentActiveBuilder = null;
        }
    }

    public void ClearSelectedUnits()
    {
        foreach (BaseUnit unit in selectedUnits)
            unit.DeSelect();
        selectedUnits.Clear();
    }

    public int SelectedAttackUnitsCount()
    {
        int count = 0;
        foreach (BaseUnit unit in selectedUnits)
            if (unit.canAttack)
                count++;
        return count;
    }

    public void RemoveUnitFromSelection(BaseUnit unit)
    {
        selectedUnits.Remove(unit);
    }

    // Is a unit within a polygon determined by 4 corners
    bool IsWithinPolygon(Vector3 unitPos)
    {
        bool isWithinPolygon = false;

        // The polygon forms 2 triangles, so we need to check if a point is within any of the triangles
        // Triangle 1: TL - BL - TR
        if (IsWithinTriangle(unitPos, TL, BL, TR))
            return true;
        // Triangle 2: TR - BL - BR
        if (IsWithinTriangle(unitPos, TR, BL, BR))
            return true;

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
            isWithinTriangle = true;
        return isWithinTriangle;
    }

    // Display the selection with a GUI square
    void DisplaySquare()
    {
        // Activate the square selection image
        if (!selectionSquareTrans.gameObject.activeInHierarchy)
            selectionSquareTrans.gameObject.SetActive(true);

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

}
