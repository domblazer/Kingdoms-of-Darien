using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;

public class GhostUnit : MonoBehaviour
{
    public GameObject intangibleUnit;
    public GameObject invalidIcon;

    public bool isSet { get; set; } = false;
    private Vector3 hitPos;
    public Vector3 offset { get; set; } = Vector3.zero;

    // This is the Builder that queued this ghost
    private Builder builder;
    // Just a reference to the virtualMenu item that instantiated this ghost
    protected PlayerConjurerArgs playerConjurerArgs;

    private List<Material> materials = new List<Material>();
    private List<Renderer> renderers = new List<Renderer>();

    private Directions facingDir = Directions.Forward;
    private bool placementValid = true;

    void Start()
    {
        invalidIcon.SetActive(false);
        // Compile all renderers and materials on this model
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>() && child != invalidIcon.transform)
            {
                renderers.Add(child.GetComponent<Renderer>());
                List<Material> temp = child.GetComponent<Renderer>().materials.ToList();
                materials = materials.Concat<Material>(temp).ToList();
            }
        }

        // Set all materials
        foreach (Material mat in materials)
            SetMaterialTransparency(mat);

        // Get object offset values
        CalculateOffset();
    }

    void Update()
    {
        // Active ghost follows raycast hit on terrain
        if (!isSet)
            FollowMousePoint();

        // Click to place
        if (Input.GetMouseButtonDown(0) && InputManager.HoldingShift() && !isSet && placementValid)
            PlaceAndCopy();
        else if (Input.GetMouseButtonDown(0) && !InputManager.HoldingShift() && !isSet && placementValid)
            PlaceSingle();

        // Rotate 90 deg clock-wise on mouse wheel click
        if (Input.GetMouseButtonDown(2) && !isSet)
            Rot90();

        // Handle if shift-released, right-mouse-click, etc.
        HandleInputChanges();

        // @TODO: when I have an active ghost, shouldn't be able to select other units, and mouse over doesn't focus them
        // @TODO: if placed over another ghost, the other ghost gets removed
        // @TODO: don't allow build when mouse is over an intangible or base unit
        // @TODO: when there's an active ghost, the build menu goes transparent and is disabled
    }

    // Instantiate the intangible unit and destroy this ghost
    public void StartBuild()
    {
        GameObject intangible = Instantiate(intangibleUnit, transform.position, intangibleUnit.transform.localRotation);
        intangible.GetComponent<IntangibleUnit>().Bind(builder, playerConjurerArgs, transform);
        Destroy(gameObject);
    }

    // Place this ghost and make a copy
    private void PlaceAndCopy()
    {
        isSet = true;
        invalidIcon.SetActive(false);
        // Increase the count of ghosts placed during this shift period
        builder.placedSinceLastShift++;
        // Add this newly placed self to the build queue
        playerConjurerArgs.prefab = builder.activeFloatingGhost;
        Debug.Log("Placed and copied: " + playerConjurerArgs.prefab);
        builder.masterBuildQueue.Enqueue(playerConjurerArgs);
        // Instantiate a copy of this self, which will now become the "active" (!isSet) ghost
        builder.InstantiateGhost(new PlayerConjurerArgs { prefab = playerConjurerArgs.prefab, menuButton = playerConjurerArgs.menuButton }, hitPos + offset);
        // Tell the builder this ghost has been placed and is ready to be reached and built
        builder.SetNextQueueReady(true);
    }

    // Place self and done
    private void PlaceSingle()
    {
        isSet = true;
        invalidIcon.SetActive(false);
        // Reset count of ghosts placed this shift period
        builder.placedSinceLastShift = 0;
        // Queue this self on single place
        playerConjurerArgs.prefab = gameObject;
        builder.masterBuildQueue.Enqueue(playerConjurerArgs);
        builder.SetNextQueueReady(true);

        // Hide immediately
        Toggle(false);
    }

    private void FollowMousePoint()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            hitPos = hit.point;
            // Round hitPos to nearest ints to snap
            Vector3 finalPos = new Vector3(Mathf.Round(hitPos.x), Mathf.Round(hitPos.y), Mathf.Round(hitPos.z));
            transform.position = finalPos;
        }
    }

    // Handle clockwise rotation by 90deg
    private void Rot90()
    {
        if (facingDir == Directions.Forward)
            facingDir = Directions.Right;
        else if (facingDir == Directions.Right)
            facingDir = Directions.Backwards;
        else if (facingDir == Directions.Backwards)
            facingDir = Directions.Left;
        else if (facingDir == Directions.Left)
            facingDir = Directions.Forward;
        SetFacingDir(facingDir);
    }

    // Show/hide this ghost
    private void Toggle(bool val)
    {
        foreach (Renderer r in renderers)
            r.enabled = val;
    }

    // Handle ghost behaviour response to input events
    private void HandleInputChanges()
    {
        // @TODO: if ghost isSet, at least one friendly builder is selected, and shift key was pressed, show the ghost
        if (isSet && InputManager.ShiftPressed())
            Toggle(true);
        // Hide again when shift is released
        if (isSet && InputManager.ShiftReleased())
        {
            Toggle(false);
        }
        // If not set (active ghost) and shift released, only remove when placed count since last shift release greater than 0
        else if (!isSet && InputManager.ShiftReleased() && builder.placedSinceLastShift > 0)
        {
            builder.placedSinceLastShift = 0;
            Destroy(gameObject);
        }
        // Destroy active ghost on right-click
        if (!isSet && Input.GetMouseButtonDown(1))
            Destroy(gameObject);
    }

    public void Bind(Builder bld, PlayerConjurerArgs args, Directions dir = Directions.Forward)
    {
        builder = bld;
        playerConjurerArgs = args;
        SetFacingDir(dir);
        Debug.Log("GhostUnit.Bind() then builder: " + builder);
    }

    // Set facing and rotation
    public void SetFacingDir(Directions dir)
    {
        facingDir = dir;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
    }

    private void CalculateOffset()
    {
        if (gameObject.GetComponent<BoxCollider>())
        {
            offset = gameObject.GetComponent<BoxCollider>().size;
        }
        else if (gameObject.GetComponent<CapsuleCollider>())
        {
            float r = gameObject.GetComponent<CapsuleCollider>().radius;
            offset = new Vector3(r, r, r);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (!col.isTrigger && !isSet)
        {
            placementValid = false;
            invalidIcon.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (!col.isTrigger && !isSet)
        {
            placementValid = true;
            invalidIcon.SetActive(false);
        }
    }

    public bool IsSet()
    {
        return isSet;
    }

    public Directions GetFacingDirection()
    {
        return facingDir;
    }

    // Set material blend mode to transparent
    private void SetMaterialTransparency(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        Color tempColor = mat.color;
        tempColor.a = 0.25f; // Set the material alpha value
        mat.color = tempColor;
    }

    public override string ToString()
    {
        string s1 = "Name: " + gameObject.name + "\n";
        s1 += "Builder Name: " + builder.gameObject.name + "\n";
        s1 += "Is Set: " + isSet + "\n";
        s1 += "Offset: " + offset + "\n";
        return s1;
    }
}
