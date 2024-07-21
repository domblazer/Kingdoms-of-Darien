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
        if (t < 1 && builder)
        {
            // @TODO: Ideally, the call to get the player context should be to an instance var in this class, but that may require creating a super class
            // for the MainPlayerContext and AIPlayerContext classes and returning that from the AddToPlayerContext function.

            // If currentMana is empty, all building slows down by half
            if (GameManager.Instance.PlayerMain.inventory.currentMana == 0)
                lagRate = 0.5f;
            else
                lagRate = 1;

            EvalColorGradient();
        }
        else if (t > 0 && t < 1 && builder == null)
        {
            // @TODO: State: Intangible has made progress but has lost its builder
            // @TODO: builders may be a list if this intangible is spawned by a kinematic builder - in that case, builders.Count == 0 
            // 1) turn off the sparkle particles
            // 2) reverse the change rate and add the drainRate to the rechargeRate
            Debug.Log("intangible has no builder - t: " + t);
            lagRate = -1.0f;
            EvalColorGradient();
        }
        else if (t <= 0 && builder == null)
        {
            // @TODO: destroy this intanbile since it has no builder and has gone back to 0 progress
            // 1) A builder that might have this unit in its buildQueue must handle that it no longer exists: "Failed to conjure..." or something
            // 2) remove the temp mana recharge

            CancelIntangible();
        }
        // Done
        else if (t >= 1 && builder)
        {
            FinishIntangible();
        }

        // @TODO: If builder is reassigned while t > 0 && t < 1, restart the sparkle particles, reset the mana drain
    }

    // Bind vars from referring builder/factory
    public void Bind(UnitBuilderBase bld, Directions dir = Directions.Forward)
    {
        builder = bld;
        // Restart particles if Builder binds to this intangible
        sparkleParticles?.Play();
        rallyPoint = transform.position;
        SetFacingDir(dir);
    }

    public void Bind(UnitBuilderBase bld, Transform rally, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        builder = bld;
        // Restart particles if Builder binds to this intangible
        sparkleParticles?.Play();
        builder.currentIntangible = this;
        parkToggle = parkDirToggle;
        rallyPoint = rally.position;
        SetFacingDir(dir);
    }
}
