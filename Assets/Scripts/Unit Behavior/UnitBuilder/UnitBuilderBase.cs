using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilderBase<T> : MonoBehaviour
{
    public bool isBuilding { get; set; }
    public Queue<GameObject> masterBuildQueue = new Queue<GameObject>();
    public bool nextQueueReady { get; set; } = false;
    public RTSUnit baseUnit { get; set; }

    private void Awake()
    {
        baseUnit = GetComponent<RTSUnit>();
    }
}
