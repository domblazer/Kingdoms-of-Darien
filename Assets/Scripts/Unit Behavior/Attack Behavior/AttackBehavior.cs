using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DarienEngine;
using System;

public class AttackBehavior : MonoBehaviour
{
    public RTSUnit baseUnit { get; set; }
    [Serializable]
    public class Weapon
    {
        // @Note: o.g. TA:K calls Projectile, "Ballistic"
        public enum WeaponTypes
        {
            Melee, Projectile, Guided, Wandering, RemoteEffect, LineOfSight
        }
        public WeaponTypes weaponType;
        public float weaponDamage = 100;
        public float weaponRange = 1.0f;
        public float weaponReloadRate = 2.5f;
        public SoundHitClasses.WeaponSoundHitClasses weaponSoundClass;
        public bool specialAttack = false;
        public Image specialAttackIcon;
        public string specialAttackName;
        public MeleeWeapon[] meleeWeapons;
    }
    public Weapon[] weapons;
    public int activeWeaponIndex = 0;
    public Weapon activeWeapon { get { return weapons[activeWeaponIndex]; } }

    private float nextAttack = 0.0f;
    public bool nextAttackReady { get; set; } = false;
    public bool engagingTarget { get; set; } = false;
    protected bool isMovingToAttack = false;
    [HideInInspector] public bool isAttacking = false;

    // @TODO: attackTarget can be an intangible enemy, in which case RTSUnit won't be present
    public class AttackTarget
    {
        public GameObject target;
        public AttackTargetTypes targetType;
    }
    [HideInInspector] public AttackTarget attackTarget;
    [HideInInspector] public List<GameObject> enemiesInSight = new();

    private void Awake()
    {
        baseUnit = GetComponent<RTSUnit>();
    }

    void Start()
    {
        foreach (Weapon wp in weapons)
        {
            // @TODO: presumably if this is a melee attacker, weapons.Length will be 1
            if (wp.weaponType == Weapon.WeaponTypes.Melee && wp.meleeWeapons.Length > 0)
                foreach (MeleeWeapon mw in wp.meleeWeapons)
                    mw.SetLinkage(baseUnit, wp.weaponDamage);
        }
    }

    /**
    *  Since this function is called in Update when this Unit has current command type of Attack, we assume the attackTarget is assigned to the current command
    */
    public void HandleAttackRoutine(bool autoAttackInterrupt = false)
    {
        attackTarget = baseUnit.currentCommand.attackTarget;
        // If attack target has died at any point during attack routine, clear attack and stop attack routine
        if (TargetHasDied())
        {
            ClearAttack();
            baseUnit.commandQueue.Dequeue();
            return;
        }

        // @TODO: if attackTarget becomes null, move back to original location unless there's another unit around to attack
        // @TODO: if isAttacking/taking damage and health is low, AI start a retreat, player units can be set to have an auto retreat or retreat button

        // State isAttacking when engagingTarget and not still orienting to attack position
        isAttacking = engagingTarget && (baseUnit.isKinematic ? !isMovingToAttack : !baseUnit.facing);

        // Move to attack target
        MoveToAttack(autoAttackInterrupt);

        // Handle attacking behavior once in range
        Attack();
    }

    private bool TargetHasDied()
    {
        bool hasDied = false;
        switch (attackTarget?.targetType)
        {
            case AttackTargetTypes.Unit:
                hasDied = attackTarget != null && attackTarget.target && attackTarget.target.GetComponent<RTSUnit>().isDead;
                break;
            case AttackTargetTypes.Intangible:
                // @Note: attackTarget object is only nullified in ClearAttack; if target becomes null while attacking, target has been destroyed in another script
                // @TODO handle intangibles
                hasDied = (isAttacking || isMovingToAttack) && attackTarget?.target == null;
                // TODO: how to determine if Intangible has gone away or been completed? Also, if the intangible has been completed, does attackTarget switch to the final unit?
                break;
        }
        return hasDied;
    }

    private void Attack()
    {
        // Handle attacking behavior
        if (isAttacking && attackTarget != null && attackTarget.target != null)
        {
            // Kinematic units do facing; @Note: stationary attack units do their own facing routine (e.g. Stronghold)
            if (baseUnit.isKinematic && !baseUnit.IsMoving())
                baseUnit.facing = baseUnit.HandleFacing(attackTarget.target.transform.position, 0.5f); // Continue facing attackTarget while isAttacking

            // Attack interval
            if (Time.time > nextAttack)
            {
                nextAttack = Time.time + activeWeapon.weaponReloadRate;
                nextAttackReady = true;
                // @TODO: should this just be default?
                if (baseUnit.AudioManager.unitPlaysAttackSounds)
                    baseUnit.AudioManager.PlayAttackSound();
            }
            else
                nextAttackReady = false;
        }
    }

    private float GetRangeOffset()
    {
        float rangeOffset = activeWeapon.weaponRange;
        // Melee attackers use a portion of the attackTarget's collider (offset) size to determine range
        if (attackTarget?.target != null && activeWeapon.weaponType == Weapon.WeaponTypes.Melee)
        {
            switch (attackTarget.targetType)
            {
                case AttackTargetTypes.Unit:
                    // @TODO: dirty fix: enemies have trouble attacking stationary units like the Barracks
                    rangeOffset = attackTarget.target.GetComponent<RTSUnit>().offset.x * (attackTarget.target.GetComponent<RTSUnit>().isKinematic ? 0.9f : 0.6f);
                    break;
                case AttackTargetTypes.Intangible:
                    rangeOffset = attackTarget.target.GetComponent<IntangibleUnitBase>().offset.x * (attackTarget.target.GetComponent<IntangibleUnitBase>().finalUnit.isKinematic ? 0.9f : 0.6f);
                    break;
            }
        }
        return rangeOffset;
    }

    private void MoveToAttack(bool autoAttackInterrupt)
    {
        // Range offset is the distance to the target where attacker can strike from
        float rangeOffset = GetRangeOffset();

        // While locked on target but not in range, keep moving to attack position
        bool inRange = false;
        if (attackTarget?.target != null)
            inRange = baseUnit.IsInRangeOf(attackTarget.target.transform.position, rangeOffset);

        if (attackTarget?.target != null && !inRange)
        {
            isMovingToAttack = true;
            // Melee attackers can swing and move at the same time, if fairly close enough to target
            isAttacking = activeWeapon.weaponType == Weapon.WeaponTypes.Melee && baseUnit.IsMoving() && baseUnit.IsInRangeOf(attackTarget.target.transform.position, rangeOffset * 2);
            baseUnit.TryToggleToAgent();
            // Move to attack target position 
            baseUnit.MoveToPosition(attackTarget.target.transform.position);
            // If unit should change attack target to closest in range during move
            if (autoAttackInterrupt)
            {
                GameObject target = FindClosestEnemy();
                // Find the closest enemy that is not already the target
                if (target && target != attackTarget.target)
                {
                    ClearAttack();
                    TryAttack(target);
                }
                return;
            }
        }
        // Once unit is in range, stop moving
        else if (isMovingToAttack && attackTarget?.target != null && inRange)
        {
            baseUnit.MoveToPosition(transform.position);
            baseUnit.TryToggleToObstacle();
            isMovingToAttack = false;
        }
    }

    public GameObject AutoPickAttackTarget(bool insertFirst = false)
    {
        // Find closest target
        GameObject target = FindClosestEnemy();

        /* string debug = "";
        enemiesInSight.ForEach(x => debug += x.gameObject.name + "\n");
        Debug.Log(gameObject.name + " is trying to auto pick an attack target. Enemies in sight: " + debug); */

        // If a valid target exists but attackTarget has not yet been assigned, lock onto this target
        if (target && (target.GetComponent<RTSUnit>() || target.GetComponent<IntangibleUnitBase>()) && attackTarget == null)
        {
            if (insertFirst)
                TryInterruptAttack(target);
            else
                TryAttack(target);
        }
        return target;
    }

    protected GameObject FindClosestEnemy()
    {
        GameObject closest = null;
        if (enemiesInSight.Count > 0)
        {
            float distance = Mathf.Infinity;
            Vector3 position = transform.position;
            List<GameObject> removeList = new();
            // Find the enemy in sight closest to me
            foreach (GameObject go in enemiesInSight)
            {
                if (go == null || go.GetComponent<RTSUnit>() && go.GetComponent<RTSUnit>().isDead)
                {
                    removeList.Add(go);
                }
                else
                {
                    Vector3 diff = go.transform.position - position;
                    float curDistance = diff.sqrMagnitude;
                    if (curDistance < distance)
                    {
                        closest = go;
                        distance = curDistance;
                    }
                }
            }
            // Prune any straggling nulls or dead units in the enemiesInSight list
            enemiesInSight = enemiesInSight.Except(removeList).ToList();
        }
        // Return closest object or null is none found
        return closest;
    }

    public void TryAttack(GameObject target, bool addToQueue = false)
    {
        SetAttackVars();

        // Add to queue 
        if (!addToQueue)
            baseUnit.commandQueue.Clear();
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Attack,
            commandPoint = target.transform.position,
            attackTarget = new AttackTarget { target = target, targetType = GetTargetType(target) }
        });
    }

    public void TryInterruptAttack(GameObject target)
    {
        SetAttackVars();

        // Push priority command, shifting existing to the right
        baseUnit.commandQueue.InsertFirst(new CommandQueueItem
        {
            commandType = CommandTypes.Attack,
            commandPoint = target.transform.position, // @TODO is this unused?
            attackTarget = new AttackTarget { target = target, targetType = GetTargetType(target) }
        });
    }

    private AttackTargetTypes GetTargetType(GameObject target)
    {
        AttackTargetTypes type = AttackTargetTypes.Unit;
        if (target.GetComponent<IntangibleUnitBase>())
            type = AttackTargetTypes.Intangible;
        return type;
    }

    private void SetAttackVars()
    {
        // Set relevant vars
        engagingTarget = true;
        isMovingToAttack = true;
        baseUnit.TryToggleToAgent();
    }

    public void ClearAttack()
    {
        nextAttackReady = false;
        isAttacking = false;
        engagingTarget = false;
        attackTarget = null;
    }
}
