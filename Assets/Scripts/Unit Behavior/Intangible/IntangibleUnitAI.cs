using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;
using DarienEngine.AI;

public class IntangibleUnitAI : IntangibleUnitBase
{
    // Update "Intangible Mass" color gradient until done
    void Update()
    {
        // TODO: if no builder is attatched to this intangible, the mana flow is reversed, draining from the intangible and adding back to main mana
        if (t < 1)
            EvalColorGradient();
        // Done
        else
            FinishIntangible();
    }

    // Bind vars for AI
    public void Bind(UnitBuilderBase bld, Transform rally, CommandQueueItem nextCmd, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        builder = bld;
        parkToggle = parkDirToggle;
        rallyPoint = rally ? rally.position : Vector3.zero;
        nextCommandAfterParking = nextCmd;
        SetFacingDir(dir);
    }
}
