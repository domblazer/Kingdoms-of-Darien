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
