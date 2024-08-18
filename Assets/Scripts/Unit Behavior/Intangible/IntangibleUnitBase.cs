using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;

public delegate void IntangibleCompletedCallback();
public class IntangibleUnitBase : MonoBehaviour
{
    // Material fade starts with a basic mana color
    public Color manaColor = new(255, 255, 0);
    public GameObject finalUnitPrefab;
    public RTSUnit finalUnit { get { return finalUnitPrefab.GetComponent<RTSUnit>(); } }
    public float buildCost { get { return finalUnit.buildCost; } }
    public float buildTime { get { return finalUnit.buildTime; } }
    // drainRate is build cost over time multiplied by the number of builders conjuring this intangible simultaneously, or -1 to reverse drainRate if no builders are attached
    public float drainRate { get { return buildCost / buildTime * 10; } }

    // Keep track of model materials to create "Intangible Mass" effect
    private List<Material> materials = new();
    private List<MaterialsMap> materialsMap = new();
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
    public List<UnitBuilderBase> builders { get; set; } = new List<UnitBuilderBase>();

    // Control variables for transparency change
    [Range(0.1f, 1.0f)] public float lagRate = 1.0f;
    public float health = 0;
    // Other state variables
    protected Directions facingDir = Directions.Forward;
    protected bool parkToggle;
    protected Vector3 rallyPoint;
    protected CommandQueueItem nextCommandAfterParking;
    public Vector3 offset { get; set; } = Vector3.zero;

    protected IntangibleCompletedCallback intangibleCompletedCallback;

    protected GameObject sparkleParticlesObj;
    protected ParticleSystem sparkleParticles;
    public PlayerNumbers playerNumber;

    void Start()
    {
        // Check if finalUnit is assigned
        if (!finalUnitPrefab)
            throw new Exception("Intangible mass needs a final prefab to instantiate on completion.");

        // Compile all materials from mesh renderers on this model
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>())
            {
                List<Material> temp = child.GetComponent<Renderer>().materials.ToList();
                materials = materials.Concat(temp).ToList();
            }
        }

        // Save the original material colors so they can be reset on destroy
        foreach (Material mat in materials)
        {
            SetMaterialTransparency(mat);
            MaterialsMap matMap = new(mat, mat.color);
            matMap.CreateGradient(manaColor);
            materialsMap.Add(matMap);
        }

        // Every intangible should have a "sparkle-particles" child object that holds the sparkles particle system
        Transform particles = transform.Find("sparkle-particles");
        if (particles && particles.gameObject.GetComponent<ParticleSystem>())
        {
            sparkleParticlesObj = particles.gameObject;
            sparkleParticles = sparkleParticlesObj.GetComponent<ParticleSystem>();
        }
        else
        {
            Debug.LogWarning("IntangibleUnitBase Error: No 'sparkle-particles' gameObject with ParticleSystem component found for " + name + ".");
        }

        // The player number of this intangible comes from the builder who instantiated it
        playerNumber = builders[0].BaseUnit.playerNumber;

        // Calculate the collider size of this intantible to gets its size "offset"
        CalculateOffset();

        // Add this intangible to the player inventory
        Functions.AddIntangibleToPlayerContext(this);
    }

    // Finalize and destroy this intangible when done
    protected void FinishIntangible()
    {
        // intangibleCompletedCallback?.Invoke();

        // Every builder assigned to this intangible now must queue the conjure command and get ready for the next
        builders.ForEach(builder =>
        {
            CommandQueueItem lastCommand = builder.BaseUnit.commandQueue.Dequeue();
            lastCommand.conjurerArgs.buildQueueCount--;
            builder.SetNextQueueReady(true);
        });

        // Instantiate final new unit
        GameObject newUnit = Instantiate(finalUnitPrefab, transform.position, transform.rotation);

        // Move the sparkle particles child object over to the RTSUnit for fade out and disposal
        sparkleParticlesObj.transform.parent = newUnit.transform;
        newUnit.GetComponent<RTSUnit>().Begin(facingDir, rallyPoint, parkToggle, nextCommandAfterParking, sparkleParticlesObj);

        // Remove intangible from inventory/player context as well
        Functions.RemoveIntangibleFromPlayerContext(this, playerNumber);
        Destroy(gameObject);
    }

    public void DetachBuilder(UnitBuilderBase builder)
    {
        builder.IsBuilding = false;
        builders.Remove(builder);

        // If there are no builders on this intangible at this point, reverse the progress
        if (builders.Count == 0)
        {
            sparkleParticles.Stop();
            lagRate = -1.0f;
        }
    }

    protected void CancelIntangible()
    {
        // @TODO: Where do particles go to finish if an intangible is cancelled?

        // Remove intangible from inventory/player context as well
        Functions.RemoveIntangibleFromPlayerContext(this, playerNumber);
        Destroy(gameObject);
    }

    // Set the completed event callback function
    public void Callback(IntangibleCompletedCallback completedCallback)
    {
        intangibleCompletedCallback = completedCallback;
    }

    // Evaluate color gradient change over time
    protected void EvalColorGradient()
    {
        foreach (MaterialsMap map in materialsMap)
            map.material.color = map.gradient.Evaluate(health);
        // lagRate is also influenced by the number of builders, i.e., more builders mean conjuring goes faster
        float lagRateMultiplier = builders.Count > 0 ? builders.Count : 1;
        health += Time.deltaTime / (buildTime / 10) * lagRateMultiplier;
    }

    protected void SetFacingDir(Directions dir)
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
