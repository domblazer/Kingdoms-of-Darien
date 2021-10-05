using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DarienEngine;

public class IntangibleUnit : IntangibleUnitBase
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

    // Bind vars from referring builder/factory
    public void Bind(UnitBuilderBase bld, Directions dir = Directions.Forward)
    {
        builder = bld;
        rallyPoint = transform.position;
        SetFacingDir(dir);
    }

    public void Bind(UnitBuilderBase bld, Transform rally, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        builder = bld;
        parkToggle = parkDirToggle;
        rallyPoint = rally.position;
        SetFacingDir(dir);
    }
}
