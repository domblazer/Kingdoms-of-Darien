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
    protected ConjurerArgs conjurerArgs;

    private List<Material> materials = new();
    private List<Renderer> renderers = new();

    private Directions facingDir = Directions.Forward;
    private bool placementValid = true;
    public bool isLodestone = false;

    void Start()
    {
        // All ghosts except lodestones should start off with valid placement
        SetPlacementValid(!isLodestone);

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

        CalculateOffset();
    }

    private void Update()
    {
        // Active ghost follows raycast hit on terrain
        if (!isSet)
            FollowMousePoint();

        // Click to place
        if (Input.GetMouseButtonDown(0) && InputManager.HoldingShift() && !isSet && placementValid)
        {
            builder.isClickInProgress = true;
            PlaceAndCopy();
        }
        else if (Input.GetMouseButtonDown(0) && !InputManager.HoldingShift() && !isSet && placementValid)
        {
            builder.isClickInProgress = true;
            PlaceSingle();
        }

        if (Input.GetMouseButtonUp(0) && builder.isClickInProgress)
        {
            builder.isClickInProgress = false;
        }

        // Rotate 90 deg clock-wise on mouse wheel click
        if (Input.GetMouseButtonDown(2) && !isSet)
            Rot90();

        // Handle if shift-released, right-mouse-click, etc.
        HandleInputChanges();

        // @TODO: if a builder gets interrupted, all the ghosts attached to it at that time need to get destroyed

        // @TODO: when I have an active ghost, shouldn't be able to select other units, and mouse over doesn't focus them
        // @TODO: if placed over another ghost, the other ghost gets removed
        // @TODO: when there's an active ghost, the build menu goes transparent and is disabled
    }

    private void OnTriggerStay(Collider col)
    {
        bool valid = false;
        if (isLodestone && col.tag == "SacredSite" && transform.position == col.transform.position)
            valid = true;
        else if (!isLodestone)
            valid = false;
        SetPlacementValid(valid);
    }

    private void OnTriggerExit()
    {
        if (!isLodestone)
            SetPlacementValid(true);
    }

    // Instantiate the intangible unit and destroy this ghost
    public void StartBuild()
    {
        GameObject intangible = Instantiate(intangibleUnit, transform.position, intangibleUnit.transform.localRotation);
        intangible.GetComponent<IntangibleUnit>().BindBuilder(builder, transform);
        Destroy(gameObject);
    }

    // Place this ghost and make a copy
    private void PlaceAndCopy()
    {
        Debug.Log("Placed and copied.");
        isSet = true;
        invalidIcon.SetActive(false);
        // Increase the count of ghosts placed during this shift period
        builder.placedSinceLastShift++; // @TODO: replace with conjurerArgs.buildQueueCount
        // Add the activeFloatingGhost (this) to the player args and enqueue it to the commandQueue
        // Debug.Log("builder.activeFloatingGhost " + builder.activeFloatingGhost);
        conjurerArgs.prefab = builder.activeFloatingGhost;
        bool wasEmpty = builder.baseUnit.commandQueue.IsEmpty();
        // conjurerArgs
        builder.baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            commandPoint = hitPos,
            conjurerArgs = conjurerArgs
        });
        // Pass a copy of the player args initially passed to this ghost for the next active. (Pass-by-value will overwrite prefab)
        ConjurerArgs nextItem = new ConjurerArgs
        {
            menuButton = conjurerArgs.menuButton,
            prefab = conjurerArgs.prefab,
            buildQueueCount = conjurerArgs.buildQueueCount
        };
        // Instantiate new active floating ghost
        builder.InstantiateGhost(nextItem, hitPos + offset);
        // Only set next ready when masterBuildQueue was empty before the enqueue
        if (wasEmpty)
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
        conjurerArgs.prefab = gameObject;

        // Destroy all ghosts for this builder and remove all it's other conjure commands
        foreach (CommandQueueItem cmd in builder.baseUnit.commandQueue)
        {
            if (cmd.commandType == CommandTypes.Conjure && cmd.conjurerArgs.prefab != null && cmd.conjurerArgs.prefab.GetComponent<GhostUnit>())
                Destroy(cmd.conjurerArgs.prefab);
        }
        builder.baseUnit.commandQueue.RemoveAll(cmd => cmd.commandType == CommandTypes.Conjure);

        // Set the current command to building this ghost
        builder.baseUnit.currentCommand = new CommandQueueItem
        {
            commandType = CommandTypes.Conjure,
            commandPoint = hitPos,
            conjurerArgs = conjurerArgs
        };
        builder.SetNextQueueReady(true);
        builder.activeFloatingGhost = null;

        // Hide immediately
        Toggle(false);
    }

    private void FollowMousePoint()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Ignore the Sky layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(1 << LayerMask.NameToLayer("Sky"))))
        {
            hitPos = hit.point;
            // Debug.Log("hit.collider.tag: " + hit.collider.tag);
            // Round hitPos to nearest ints to snap
            Vector3 finalPos = new(Mathf.Round(hitPos.x), Mathf.Round(hitPos.y), Mathf.Round(hitPos.z));
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
            Toggle(false);
        // If not set (active ghost) and shift released, only remove when placed count since last shift release greater than 0
        else if (!isSet && InputManager.ShiftReleased() && builder.placedSinceLastShift > 0)
        {
            builder.placedSinceLastShift = 0;
            Destroy(gameObject);
        }
        // Destroy active ghost on right-click
        if (!isSet && Input.GetMouseButtonDown(1))
        {
            Destroy(gameObject);
        }
    }

    public void BindBuilder(Builder bld, ConjurerArgs args, Directions dir = Directions.Forward)
    {
        builder = bld;
        // Ghosts need to instantiate new args; they are more independent than intangibles
        conjurerArgs = new ConjurerArgs
        {
            menuButton = args.menuButton,
            prefab = args.prefab,
            buildQueueCount = args.buildQueueCount
        };
        SetFacingDir(dir);
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

    private void SetPlacementValid(bool val)
    {
        placementValid = val;
        invalidIcon.SetActive(!val);
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
