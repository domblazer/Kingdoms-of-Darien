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
    [System.Serializable]
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
    [HideInInspector] public List<GameObject> enemiesInSight = new List<GameObject>();

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

    public void HandleAttackRoutine(bool autoAttackInterrupt = false)
    {
        // If attack target has died at any point during attack routine, clear attack and stop attack routine
        if (TargetHasDied())
        {
            ClearAttack();
            baseUnit.commandQueue.Dequeue();
            return;
        }

        // @TODO: if attackTarget becomes null, move back to original location unless there's another unit around to attack
        // @TODO: if isAttacking/taking damage and health is low, AI start a retreat, player units can be set to have an auto retreat or retreat button

        // Handle attacking regular units
        if (attackTarget.targetType == AttackTargetTypes.Unit)
        {
            // State isAttacking when engagingTarget and not still orienting to attack position
            isAttacking = engagingTarget && (baseUnit.isKinematic ? !isMovingToAttack : !baseUnit.facing);

            // Move to attack target
            MoveToAttack(autoAttackInterrupt);

            // Handle attacking behavior once in range
            Attack();
        }
        // @TODO: handle intangibles
        else if (attackTarget.targetType == AttackTargetTypes.Intangible)
        {

        }

    }

    private bool TargetHasDied()
    {
        bool hasDied = false;
        switch (attackTarget.targetType)
        {
            case AttackTargetTypes.Unit:
                hasDied = attackTarget != null && attackTarget.targetType == AttackTargetTypes.Unit && attackTarget.target.GetComponent<RTSUnit>().isDead;
                break;
            case AttackTargetTypes.Intangible:
                // TODO: how to determine if Intangible has gone away or been completed? Also, if the intangible has been completed, does attackTarget switch to the final unit?
                break;
        }
        return hasDied;
    }

    private void Attack()
    {
        // Handle attacking behavior
        if (isAttacking && attackTarget != null)
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
        // Melee attackers use a portion of the attackTarget's collider (offset) size
        if (attackTarget != null)
            rangeOffset = activeWeapon.weaponType == Weapon.WeaponTypes.Melee ? attackTarget.target.GetComponent<RTSUnit>().offset.x * 0.85f : activeWeapon.weaponRange;
        return rangeOffset;
    }

    private void MoveToAttack(bool autoAttackInterrupt)
    {
        // Range offset is the distance to the target where attacker can strike from
        float rangeOffset = GetRangeOffset();

        // While locked on target but not in range, keep moving to attack position
        bool inRange = false;
        if (attackTarget != null)
            inRange = baseUnit.IsInRangeOf(attackTarget.target.transform.position, rangeOffset);

        if (attackTarget != null && !inRange)
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
        // Once unit is in range, can stop moving
        else if (isMovingToAttack && attackTarget != null && inRange)
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
        // If a valid target exists but attackTarget has not yet been assigned, lock onto this target
        // @TODO: handle Intangible
        if (target && target.GetComponent<RTSUnit>() && attackTarget == null)
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
        // @TODO: enemies can include Intangibles
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        if (enemiesInSight.Count > 0)
        {
            // @TODO: need to handle intangibles too, i.e. item.GetCompontent<IngangibleUnitBase<T??>() ? isIntangible
            // @TODO: this is also not efficient to be calling all the time. Need better way to remove nulls/dead
            enemiesInSight = enemiesInSight.Where(item => item != null && !item.GetComponent<RTSUnit>().isDead).ToList();
        }
        foreach (GameObject go in enemiesInSight)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    public void TryAttack(GameObject target, bool addToQueue = false)
    {
        SetAttackVars(target);
        // Add to queue 
        if (!addToQueue)
            baseUnit.commandQueue.Clear();
        baseUnit.commandQueue.Enqueue(new CommandQueueItem
        {
            commandType = CommandTypes.Attack,
            commandPoint = attackTarget.target.transform.position,
            attackInfo = new AttackInfo { attackTarget = attackTarget.target }
        });
    }

    public void TryInterruptAttack(GameObject target)
    {
        // Debug.Log(gameObject.name + " trying attack interrupt on " + target.name);
        SetAttackVars(target);
        // Push priority command, shifting existing to the right
        baseUnit.commandQueue.InsertFirst(new CommandQueueItem
        {
            commandType = CommandTypes.Attack,
            commandPoint = attackTarget.target.transform.position,
            attackInfo = new AttackInfo { attackTarget = attackTarget.target }
        });
    }

    private void SetAttackVars(GameObject target)
    {
        // Debug.Log(gameObject.name + " trying attack on " + target.name);

        attackTarget = new AttackTarget
        {
            // @TODO: determine proper targetType
            targetType = AttackTargetTypes.Unit,
            target = target
        };
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
