using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LodestoneScript : MonoBehaviour
{
    public Light pulseLight;
    public float glowSpeedDamp = 0.8f;
    public MeshRenderer outerStoneRenderer;
    public int outerStoneMaterialIndex = 0;
    private float alpha = 0.5f;

    private RTSUnit _Unit;

    private InventoryScript inventory;
    private float manaIncome;
    private float manaStorage;

    // Start is called before the first frame update
    void Start()
    {
        _Unit = GetComponent<RTSUnit>();
        manaIncome = _Unit.manaIncome;
        manaStorage = _Unit.manaStorage;
        // @TODO: get appropriate team inventory
        if (GameManagerScript.Instance.Inventories.TryGetValue("Player", out InventoryScript inv))
        {
            inventory = inv;
        }
        inventory.AddLodestone(this);

        pulseLight.intensity = 1.0f;
        pulseLight.range = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Create a pulsating effect with light range and intensity
        float prevRange = pulseLight.range;
        pulseLight.range = Mathf.PingPong((Time.time * glowSpeedDamp), 2) + 1;

        alpha = Mathf.PingPong((Time.time * glowSpeedDamp * 0.25f), 0.5f);
        Color temp = outerStoneRenderer.materials[outerStoneMaterialIndex].color;
        temp.a = alpha;
        outerStoneRenderer.materials[outerStoneMaterialIndex].color = temp;

        // If range is increasing, bounce the intensity
        if (pulseLight.range - prevRange > 0)
        {
            pulseLight.intensity = Mathf.PingPong(Time.time * glowSpeedDamp, 1);
        }
    }

    public float GetManaIncome()
    {
        return manaIncome;
    }

    public float GetManaStorage()
    {
        return manaStorage;
    }
}
