using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilderBase<T> : MonoBehaviour
{
    public Queue<T> masterBuildQueue = new Queue<T>();
    public bool isBuilding { get; set; }
    public bool nextQueueReady { get; set; } = false;
    public RTSUnit baseUnit { get; set; }

    private void Awake()
    {
        baseUnit = GetComponent<RTSUnit>();
    }

    public void SetNextQueueReady(bool val)
    {
        nextQueueReady = val;
        // Reset isBuilding for kinematic builders since once one intangible is done, builder must re-enter move routine before starting another
        if (IsBuilder())
            isBuilding = false;
    }

    public bool IsFactory()
    {
        return !baseUnit.isKinematic;
    }

    public bool IsBuilder()
    {
        return baseUnit.isKinematic;
    }

    public bool IsAI()
    {
        return typeof(T).IsAssignableFrom(typeof(GameObject));
    }
}
