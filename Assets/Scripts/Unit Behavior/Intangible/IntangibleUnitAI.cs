﻿using System.Collections;
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
        if (health < 1)
        {
            // If currentMana is empty, all building slows down by half
            if (GameManager.Instance.AIPlayers[playerNumber].inventory.currentMana == 0)
                lagRate = 0.5f;
            else
                lagRate = 1;

            EvalColorGradient();
        }
        // Done
        else
            FinishIntangible();
    }

    // Bind vars for AI
    public void BindBuilder(UnitBuilderBase bld, Transform rally, CommandQueueItem nextCmd, bool parkDirToggle = false, Directions dir = Directions.Forward)
    {
        bld.currentIntangible = this;
        // Add this builder to the list and adjust mana drain
        builders.Add(bld);
        // Restart particles if Builder binds to this intangible
        if (sparkleParticles && sparkleParticles.isStopped)
            sparkleParticles?.Play();
        // Set position vars
        parkToggle = parkDirToggle;
        rallyPoint = rally ? rally.position : Vector3.zero;
        nextCommandAfterParking = nextCmd;
        SetFacingDir(dir);
    }

    void OnMouseEnter()
    {
        if (!InputManager.IsMouseOverUI())
        {
            if (gameObject.tag == "Enemy" && GameManager.Instance.PlayerMain.player.SelectedAttackUnitsCount() > 0 && !GameManager.Instance.PlayerMain.player.nextCommandIsPrimed)
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Attack);

            GameManager.Instance.SetHovering(gameObject);
        }
    }

    // Update UI with intangible mass details in unit UI
    private void OnMouseOver()
    {
        if (!InputManager.IsMouseOverUI())
            UIManager.UnitInfoInstance.Set(this, null);
    }

    // @TODO: if mouse is over this unit when the unit dies, still need to reset cursor, clear unit ui
    void OnMouseExit()
    {
        if (GameManager.Instance.PlayerMain.player.SelectedBuilderUnitsCount() > 0)
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);

        GameManager.Instance.ClearHovering();
    }
}
