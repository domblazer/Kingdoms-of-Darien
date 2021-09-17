using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class IntangibleUnitAI : IntangibleUnitBase<AIConjurerArgs>
{
    // Update "Intangible Mass" color gradient until done
    void Update()
    {
        if (t < 1)
            EvalColorGradient();
        else
        {
            // Only Factory needs to Dequeue here
            if (builder.IsFactory())
                builder.masterBuildQueue.Dequeue();
            // Done
            FinishIntangible();
        }
    }

    // Bind vars for AI, which does not use PlayerConjurerArgs
    public void Bind(UnitBuilderBase<AIConjurerArgs> bld, Transform rally, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        builder = bld;
        parkToggle = parkDirToggle;
        rallyPoint = rally.position;
        SetFacingDir(dir);
    }
}
