using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

/// <summary>
/// Class <c>UnitBuilderBase</c> models the most common and basic functionality needed for builders.
/// </summary>
public class UnitBuilderBase : MonoBehaviour
{
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
}
