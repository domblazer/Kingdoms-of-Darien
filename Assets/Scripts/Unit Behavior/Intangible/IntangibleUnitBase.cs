using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;

public class IntangibleUnitBase<T> : MonoBehaviour
{
    // Material fade starts with a basic mana color
    public Color manaColor = new Color(255, 255, 0);
    public GameObject finalUnitPrefab;
    public RTSUnit finalUnit { get { return finalUnitPrefab.GetComponent<RTSUnit>(); } }
    public float buildCost { get { return finalUnit.buildCost; } }
    public float buildTime { get { return finalUnit.buildTime; } }
    public int drainRate { get { return Mathf.RoundToInt((buildCost / buildTime) * 10); } }

    // Keep track of model materials to create "Intangible Mass" effect
    private List<Material> materials = new List<Material>();
    private List<MaterialsMap> materialsMap = new List<MaterialsMap>();
    protected class MaterialsMap
    {
        public Material material;
        public Color originalColor;
        public Gradient gradient;

        public MaterialsMap(Material mat, Color col)
        {
            material = mat;
            originalColor = col;
            gradient = new Gradient();
        }

        public void CreateGradient(Color manaColor)
        {
            GradientColorKey[] colorKey = new GradientColorKey[3];
            colorKey[0].color = manaColor; // Begin with transparent mana color
            colorKey[0].time = 0.0f;
            colorKey[1].color = manaColor; // Full mana color at 50%
            colorKey[1].time = 0.5f;
            colorKey[2].color = originalColor; // Back to original color
            colorKey[2].time = 1.0f;
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
            alphaKey[0].alpha = 0.0f;
            alphaKey[0].time = 0.0f;
            alphaKey[1].alpha = 1.0f; // Transparency done at 50%
            alphaKey[1].time = 0.5f;
            alphaKey[2].alpha = 1.0f;
            alphaKey[2].time = 1.0f;
            gradient.SetKeys(colorKey, alphaKey);
        }
    }

    // Reference back to the Builder or Factory that spawned this intangible. Can be either an AI or player
    public UnitBuilderBase<T> builder { get; set; }

    // Control variables for transparency change
    [Range(0.1f, 1.0f)] private float rate = 1.0f;
    protected float t = 0;
    // Other state variables
    protected Directions facingDir = Directions.Forward;
    protected bool parkToggle;
    protected Vector3 rallyPoint;
    protected RTSUnit.States firstState = RTSUnit.States.Standby;

    void Start()
    {
        // Check if finalUnit is assigned
        if (!finalUnitPrefab)
            throw new System.Exception("Intangible mass needs a final prefab to instantiate on completion.");

        // Compile all materials from mesh renderers on this model
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>())
            {
                List<Material> temp = child.GetComponent<Renderer>().materials.ToList();
                materials = materials.Concat<Material>(temp).ToList();
            }
        }

        // Save the original material colors so they can be reset on destroy
        foreach (Material mat in materials)
        {
            SetMaterialTransparency(mat);
            MaterialsMap t = new MaterialsMap(mat, mat.color);
            t.CreateGradient(manaColor);
            materialsMap.Add(t);
        }

        // Add this intangible to the Player context (inventory and all)
        Functions.AddIntangibleToPlayerContext<T>(this);
    }

    // Finalize and destroy this intangible when done
    protected void FinishIntangible()
    {
        // Instantiate final new unit
        GameObject newUnit = Instantiate(finalUnitPrefab, transform.position, transform.rotation);
        // @TODO: determine appropriate next state
        newUnit.GetComponent<RTSUnit>().Begin(facingDir, rallyPoint, parkToggle, firstState);

        // Tell builder it can continue then destroy this intangible
        builder.SetNextQueueReady(true);

        // @TODO: remove intangible from inventory/player context as well
        // Functions.RemoveIntangibleFromPlayerContext<T>(this);

        Destroy(gameObject);
    }

    // Evaluate color gradient change over time
    protected void EvalColorGradient()
    {
        foreach (MaterialsMap map in materialsMap)
            map.material.color = map.gradient.Evaluate(t);
        t += (Time.deltaTime / (buildTime / 10)) * rate;
    }

    protected void SetFacingDir(Directions dir)
    {
        facingDir = dir;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
    }

    // Set a material to use transparency rendering
    private void SetMaterialTransparency(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    private void OnDestroy()
    {
        // Reset the original color values of the model when done
        foreach (MaterialsMap map in materialsMap)
            map.material.color = map.originalColor;
    }
}
