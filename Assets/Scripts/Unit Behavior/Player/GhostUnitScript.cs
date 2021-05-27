using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GhostUnitScript : MonoBehaviour
{
    public GameObject intangibleUnit;
    public Vector3 offset = Vector3.zero;
    public Vector3 sizeOffset;
    public GameObject invalidIcon;
    private bool isSet = false;
    private Vector3 hitPos;

    private UnitBuilder referrer; // This is the UnitBuilder (base unit) instance that instantiated this IntangibleUnitScript instance
    private UnitBuilder.BuildMapping buildEvent;

    private List<Material> materials = new List<Material>();
    private List<Renderer> renderers = new List<Renderer>();

    private bool shiftReleased;

    public enum Directions : int
    {
        Forward = 180, Right = 90, Backwards = 0, Left = -90
    }

    private Directions facingDir = Directions.Forward;

    private bool placementValid = true;

    // Start is called before the first frame update
    void Start()
    {
        invalidIcon.SetActive(false);
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>() && child != invalidIcon.transform)
            {
                renderers.Add(child.GetComponent<Renderer>());
                List<Material> temp = child.GetComponent<Renderer>().materials.ToList();
                materials = materials.Concat<Material>(temp).ToList();
            }
        }

        foreach (Material mat in materials)
        {
            SetMaterialTransparency(mat);
        }

        // Get object offset values based on collider
        if (gameObject.GetComponent<BoxCollider>())
        {
            sizeOffset = gameObject.GetComponent<BoxCollider>().size;
        }
        else if (gameObject.GetComponent<CapsuleCollider>())
        {
            float r = gameObject.GetComponent<CapsuleCollider>().radius;
            sizeOffset = new Vector3(r, r, r);
        }
    }

    private void Update()
    {
        // Follow the mouse position based on terrain as well
        if (!isSet)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                hitPos = hit.point;
                Vector3 finalPos = new Vector3(Mathf.Round(hitPos.x), Mathf.Round(hitPos.y), Mathf.Round(hitPos.z));
                // Debug.Log("snap to pos: " + finalPos);
                transform.position = finalPos;
            }
        }

        // @TODO: when I have an active ghost, shouldn't be able to select other units, and mouse over doesn't focus them
        // @TODO: if placed over another ghost, the other ghost gets removed
        // @TODO: don't allow build when mouse is over an intangible or base unit
        // @TODO: when there's an active ghost, the build menu goes transparent and is disabled

        // If click to set while holding shift
        if (Input.GetMouseButtonDown(0) && HoldingShift() && !isSet && placementValid)
        {
            // Place this ghost and make a copy
            Debug.Log("placed and copied");
            isSet = true;
            invalidIcon.SetActive(false);
            // Increase the count of ghosts placed during this shift period
            referrer.PlusPlaced();
            // Add this newly placed ghost to the build queue
            referrer.masterBuildQueue.Enqueue(buildEvent);

            // Instantiate a copy of this self ghost, which will now become the "active" (!isSet) ghost
            GameObject ghost = Instantiate(gameObject, hitPos + offset, gameObject.transform.localRotation);
            // IMPORTANT: this new active ghost needs to set it's buildEvent.ghost to itself so UnitBuilder will know to travel to it next
            buildEvent.ghost = ghost;
            ghost.GetComponent<GhostUnitScript>().SetReferences(referrer, buildEvent);
            // Make sure the copy maintains this ghost's rotation
            ghost.GetComponent<GhostUnitScript>().SetFacingDir(facingDir);

            // Tell the builder this ghost has been placed and is ready to be reached and built
            referrer.SetNextQueueReady(true);
        }
        else if (Input.GetMouseButtonDown(0) && !HoldingShift() && !isSet && placementValid)
        {
            // Place single
            Debug.Log("placed single");
            isSet = true;
            invalidIcon.SetActive(false);
            // Reset count of ghosts placed this shift period
            referrer.ResetPlaced();
            // Queue this self on single place
            referrer.masterBuildQueue.Enqueue(buildEvent);
            referrer.SetNextQueueReady(true);

            Toggle(false); // Hide immediately
        }

        // Rotate 90 deg clock-wise on mouse wheel click
        if (Input.GetMouseButtonDown(2) && !isSet)
        {
            if (facingDir == Directions.Forward)
            {
                facingDir = Directions.Right;
            }
            else if (facingDir == Directions.Right)
            {
                facingDir = Directions.Backwards;
            }
            else if (facingDir == Directions.Backwards)
            {
                facingDir = Directions.Left;
            }
            else if (facingDir == Directions.Left)
            {
                facingDir = Directions.Forward;
            }
            transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
            // Debug.Log("facing dir: " + facingDir);
        }

        // @TODO: if ghost isSet, at least one friendly builder is selected, and shift key was pressed, show the ghost
        if (isSet && ShiftPressed())
        {
            Toggle(true);
        }

        // Hide again when shift is released
        if (isSet && ShiftReleased())
        {
            Toggle(false);
        }
        // If not set (active ghost) and shift released, only remove when placed count since last shift release greater than 0
        else if (!isSet && ShiftReleased() && referrer.GetPlaced() > 0)
        {
            referrer.ResetPlaced();
            Destroy(gameObject);
        }

        // Destroy active ghost on right-click
        if (!isSet && Input.GetMouseButtonDown(1))
        {
            Destroy(gameObject);
        }
    }

    void SetMaterialTransparency(Material mat)
    {
        // Set material blend mode to transparent
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

    public void SetReferences(UnitBuilder builderScript, UnitBuilder.BuildMapping buildButtonEventMap)
    {
        referrer = builderScript;
        buildEvent = buildButtonEventMap;
    }

    public void SetFacingDir(Directions dir)
    {
        facingDir = dir;
    }

    private void Toggle(bool val)
    {
        foreach (Renderer r in renderers)
        {
            r.enabled = val;
        }
        // invalidIcon.SetActive(false);
    }

    public void StartBuild()
    {
        // Instantiate the intangible unit and destroy this ghost
        GameObject intangible = Instantiate(intangibleUnit, transform.position, intangibleUnit.transform.localRotation);
        intangible.GetComponent<IntangibleUnitScript>().SetReferences(referrer, buildEvent);
        intangible.GetComponent<IntangibleUnitScript>().SetFacingDir(facingDir);
        Destroy(gameObject);
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

    private bool HoldingShift()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private bool ShiftPressed()
    {
        return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
    }

    private bool ShiftReleased()
    {
        return Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift);
    }

    public bool IsSet()
    {
        return isSet;
    }

    public Directions GetFacingDirection()
    {
        return facingDir;
    }
}
