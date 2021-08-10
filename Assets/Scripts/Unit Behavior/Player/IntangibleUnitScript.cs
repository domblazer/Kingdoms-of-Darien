using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class IntangibleUnitScript : MonoBehaviour
{
    public Color manaColor; // Material fade starts with a basic mana color
    public GameObject finalUnitPrefab;
    public RTSUnit finalUnit { get { return finalUnitPrefab.GetComponent<RTSUnit>(); } }

    public float buildCost { get; set; } = 1000; // Cost in mana
    public float buildTime { get; set; } = 10; // duration in seconds

    private List<Material> materials = new List<Material>();
    private List<MatMapping> mapping = new List<MatMapping>();

    [Range(0.1f, 1.0f)]
    private float rate = 1.0f; // multiplier to slow down build time based on mana
    private float t = 0; // lerp control variable

    private UnitBuilder referrer; // This is the UnitBuilder (base unit) instance that instantiated this IntangibleUnitScript instance
    private UnitBuilder.BuildMapping buildEvent;

    private bool parkToggle;

    private GhostUnitScript.Directions facingDir = GhostUnitScript.Directions.Forward;
    private InventoryScript inventory;

    private float nextManaDrain = 0;
    private float manaDrainRate = 0.1f;

    private struct MatMapping
    {
        public Material material { get; set; }
        public Color originalColor { get; set; }
        public Gradient gradient { get; }

        public MatMapping(Material mat, Color col)
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

    void Start()
    {
        buildCost = finalUnitPrefab.GetComponent<RTSUnit>().buildCost;
        buildTime = finalUnitPrefab.GetComponent<RTSUnit>().buildTime;

        // @TODO: get appropriate team inventory
        if (GameManagerScript.Instance.Inventories.TryGetValue("Player", out InventoryScript inv))
        {
            inventory = inv;
            inventory.AddIntangible(this);
        }
        else
        {
            throw new System.Exception("Intangible could not find inventory.");
        }

        // Check if finalUnit is assigned
        if (!finalUnitPrefab)
        {
            throw new System.Exception("Intangible mass needs a final prefab to instantiate on completion.");
        }

        foreach (Transform child in transform)
        {
            if (child.GetComponent<Renderer>())
            {
                List<Material> temp = child.GetComponent<Renderer>().materials.ToList();
                materials = materials.Concat<Material>(temp).ToList();
            }
        }
        // Debug.Log("all mats len: " + materials.Count);

        // Save the original material color so it can be reset on game stop or construction completion
        foreach (Material mat in materials)
        {
            SetMaterialTransparency(mat);
            MatMapping t = new MatMapping(mat, mat.color);
            t.CreateGradient(manaColor);
            mapping.Add(t);
        }
    }

    void Update()
    {
        if (t < 1)
        {
            // Conjuring
            // decrement currentMana every 1/10th of a second
            // @TODO: mana recharge and drain rates should be set as global vars somewhere, maybe GameManager
            /* if (Time.time > nextManaDrain)
            {
                nextManaDrain = Time.time + manaDrainRate;
                int dec = (int)((manaCost / duration) * manaDrainRate);
                Debug.Log("dec " + dec);
                inventory.MinusCurrentMana(dec);
            } */

            foreach (MatMapping map in mapping)
            {
                map.material.color = map.gradient.Evaluate(t);
            }
            t += (Time.deltaTime / (buildTime / 10)) * rate;
        }
        else
        {
            // Done
            inventory.RemoveIntangible(this);

            if (referrer._BaseUnit.isKinematic)
            {
                // @NOTE: dequeue for kinematic builder handled in UnitBuilder script b/c unit builder tracks ghosts, not intangibles

                // Instantiate final new unit
                GameObject newUnit = Instantiate(finalUnitPrefab, transform.position, transform.rotation);
                newUnit.GetComponent<BaseUnitScript>().SetFacingDir(facingDir);

                referrer.SetNextQueueReady(true); // Tell builder it can move on to building the next unit

                Destroy(gameObject); // Destroy this intangible when done
            }
            else
            {
                // Dequeue the build queues
                referrer.masterBuildQueue.Dequeue();
                buildEvent.buildQueue.Dequeue();

                // Instantiate final new unit
                GameObject newUnit = Instantiate(finalUnitPrefab, transform.position, transform.rotation);

                // Tell builder it can move on to building the next unit
                referrer.SetNextQueueReady(true);

                // Tell newUnit to move to park at builder's rally point
                newUnit.GetComponent<BaseUnitScript>().SetParking(referrer.builderRallyPoint.position, parkToggle);

                // Destroy this intangible when done
                Destroy(gameObject);
            }

        }
    }

    void SetMaterialTransparency(Material mat)
    {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    public void SetReferences(UnitBuilder builderScript, UnitBuilder.BuildMapping buildButtonEventMap)
    {
        referrer = builderScript;
        buildEvent = buildButtonEventMap;
    }

    public void SetReferences(UnitBuilder builderScript, UnitBuilder.BuildMapping buildButtonEventMap, bool parkingDirectionToggle)
    {
        referrer = builderScript;
        buildEvent = buildButtonEventMap;
        parkToggle = parkingDirectionToggle;
    }

    public void SetFacingDir(GhostUnitScript.Directions dir)
    {
        facingDir = dir;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facingDir, transform.rotation.z);
    }

    private void OnDestroy()
    {
        foreach (MatMapping map in mapping)
        {
            map.material.color = map.originalColor;
        }
    }
}
