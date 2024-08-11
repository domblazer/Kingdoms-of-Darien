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
        if (health < 1 && builders.Count > 0)
        {
            // Debug.Log("Building. health " + health);
            // If currentMana is empty, all building slows down by half
            if (GameManager.Instance.PlayerMain.inventory.currentMana == 0)
                lagRate = 0.5f;
            else
                lagRate = 1;

            EvalColorGradient();
        }
        else if (health > 0 && health < 1 && builders.Count == 0)
        {
            Debug.Log("Intangible has lost its builder.");
            // State: Intangible has made progress but has lost its builder
            // @TODO: builders may be a list if this intangible is spawned by a kinematic builder - in that case, builders.Count == 0 
            // 1) turn off the sparkle particles
            // 2) reverse the change rate and add the drainRate to the rechargeRate
            // Debug.Log("intangible has no builder - health: " + health);
            lagRate = -1.0f;
            EvalColorGradient();
        }
        else if (health <= 0 && builders.Count == 0)
        {
            // @TODO: A builder that might have this unit in its buildQueue must handle that it no longer exists: "Failed to conjure..." or something
            Debug.Log("Cancel intangible.");
            CancelIntangible();
        }
        // Done
        else if (health >= 1 && builders.Count > 0)
        {
            Debug.Log("Intangible finished");
            FinishIntangible();
        }
    }

    // Bind vars from referring builder/factory
    public void BindBuilder(UnitBuilderBase bld, Directions dir = Directions.Forward)
    {
        bld.currentIntangible = this;
        // Add this builder to the list and adjust mana drain
        builders.Add(bld);
        // Restart particles if Builder binds to this intangible
        if (sparkleParticles && sparkleParticles.isStopped)
            sparkleParticles?.Play();
        rallyPoint = transform.position;
        SetFacingDir(dir);
    }

    public void BindBuilder(UnitBuilderBase bld, Transform rally, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        bld.currentIntangible = this;
        // Add this builder to the list and adjust mana drain
        builders.Add(bld);
        // Restart particles if Builder binds to this intangible
        if (sparkleParticles && sparkleParticles.isStopped)
            sparkleParticles?.Play();
        // Set position vars
        parkToggle = parkDirToggle;
        rallyPoint = rally.position;
        SetFacingDir(dir);
    }

    void OnMouseEnter()
    {
        // @TODO: if mainPlayer.nextCommandIsPrimed, this should still change to Select cursor, but when moving back, should go back to the primed command mouse cursor
        // @TODO: must also check if any selected builders are VALID builders for attaching to conjure this intangible
        if (!InputManager.IsMouseOverUI())
        {
            if (GameManager.Instance.PlayerMain.player.SelectedBuilderUnitsCount() > 0)
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Repair);
        }
        // Debug.Log("Hovered over intangible...");
        GameManager.Instance.SetHovering(gameObject);
    }

    // Update UI with intangible mass details in unit UI
    private void OnMouseOver()
    {
        // Note: O.g. TAK seems to have an inconsistency when hovering over an intangible... The builder name it shows is always the builder who instantiated it, even if that
        // builder has moved off and is no longer conjuring it. Not sure what would happen if that builder died, but for our purposes, it may just serve to set Builders[0]

        // First check if I am already selected
        if (!InputManager.IsMouseOverUI())
            UIManager.UnitInfoInstance.Set(this, builders[0]);
    }

    // @TODO: if mouse is over this unit when the unit dies, still need to reset cursor, clear unit ui
    void OnMouseExit()
    {
        if (GameManager.Instance.PlayerMain.player.SelectedBuilderUnitsCount() > 0)
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);

        GameManager.Instance.ClearHovering();
    }
}
