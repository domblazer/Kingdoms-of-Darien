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
        if (health > 0 && health < 1 && builders.Count > 0)
        {
            // If currentMana is empty, all building slows down by half
            if (GameManager.Instance.AIPlayers[playerNumber].inventory.currentMana == 0)
                lagRate = 0.5f;
            else
                lagRate = 1;

            EvalColorGradient();
        }
        else if (health > 0 && health < 1 && builders.Count == 0)
        {
            lagRate = -1.0f;
            EvalColorGradient();
        }
        else if (health <= 0 && builders.Count == 0)
        {
            // @TODO: A builder that might have this unit in its buildQueue must handle that it no longer exists: "Failed to conjure..." or something
            CancelIntangible();
        }
        // At any point, the intangible could dip down to 0 health and "die"; then cancel any builds
        else if (health <= 0 && builders.Count > 0)
        {
            Debug.Log("IntangibleAI health dipped below 0, should die");
            // @Note: CancelBuild removes builder from builders which is not allowed in a loop on that list
            builders.ForEach(bld => bld.CancelBuild(true));
            builders.Clear();

            CancelIntangible();
        }
        // Done
        else if (health >= 1 && builders.Count > 0)
        {
            FinishIntangible();
        }
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
        // @TODO: Case: If single friendly unit is selected and hovering over enemy intangible, 
        // shouldn't that say like "Swordsman -- Attacking -- Cabal"?
        if (!InputManager.IsMouseOverUI())
            UIManager.UnitInfoInstance.Set(this, null);
    }

    // @TODO: if mouse is over this unit when the unit dies, still need to reset cursor, clear unit ui
    void OnMouseExit()
    {
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Normal);
        UIManager.UnitInfoInstance.Toggle(false);
        GameManager.Instance.ClearHovering();
    }
}
