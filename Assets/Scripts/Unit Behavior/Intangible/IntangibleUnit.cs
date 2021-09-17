using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;

public class IntangibleUnit : IntangibleUnitBase<PlayerConjurerArgs>
{
    // Note this is only used when T is PlayerConjurerArgs
    public PlayerConjurerArgs playerConjurerArgs;

    // Update "Intangible Mass" color gradient until done
    void Update()
    {
        if (t < 1)
            EvalColorGradient();
        else
        {
            // Only Factory needs to Dequeue here
            if (builder.IsFactory())
            {
                builder.masterBuildQueue.Dequeue();
                // Only Player Factory
                playerConjurerArgs.buildQueueCount--;
            }
            // Done
            FinishIntangible();
        }
    }

    // Binding GhostUnits requires fewer args
    public void Bind(UnitBuilderBase<PlayerConjurerArgs> bld, PlayerConjurerArgs args, Directions dir = Directions.Forward)
    {
        builder = bld;
        playerConjurerArgs = args;
        rallyPoint = transform.position;
        SetFacingDir(dir);
    }

    // Bind vars from referring builder/factory
    public void Bind(UnitBuilderBase<PlayerConjurerArgs> bld, PlayerConjurerArgs args, Transform rally, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        builder = bld;
        playerConjurerArgs = args;
        parkToggle = parkDirToggle;
        rallyPoint = rally.position;
        SetFacingDir(dir);
    }
}
