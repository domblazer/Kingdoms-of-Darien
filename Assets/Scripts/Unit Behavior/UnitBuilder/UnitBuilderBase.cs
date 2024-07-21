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
    public bool IsBuilding { get; set; }
    public bool NextQueueReady { get; set; } = false;
    public RTSUnit BaseUnit { get; set; }
    public IntangibleUnitBase currentIntangible;

    private void Awake()
    {
        BaseUnit = GetComponent<RTSUnit>();
        BaseUnit.commandQueue.OnQueueChanged += Interrupt;
    }

    public void SetNextQueueReady(bool val)
    {
        NextQueueReady = val;
        // Reset isBuilding for kinematic builders since once one intangible is done, builder must re-enter move routine before starting another
        if (IsBuilder())
            IsBuilding = false;
    }

    public bool IsFactory()
    {
        return !BaseUnit.isKinematic;
    }

    public bool IsBuilder()
    {
        return BaseUnit.isKinematic;
    }

    public void Interrupt(object sender, CommandQueue.CommandQueueChangedEventArgs changeEvent)
    {
        Debug.Log("Builder commandQueue was changed.");
        if (changeEvent.changeType == "Clear")
        {
            Debug.Log("Builder queue was cleared.");
            // @TODO: Would probably like to detect also that the previous current command of the Builder was a Conjure task
            if (currentIntangible)
                CancelBuild();
        }
    }

    public void CancelBuild()
    {   
        // @TODO: reset builder conjuring animation
        Debug.Log("Cancel current build; disconnect intangible " + currentIntangible.gameObject.name);
        currentIntangible.DetachBuilder();
    }
}
