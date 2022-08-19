using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DarienEngine;

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
        public MeleeWeaponScript[] meleeWeapons;
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
    [HideInInspector] public GameObject attackTarget;
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
                foreach (MeleeWeaponScript mw in wp.meleeWeapons)
                    mw.SetLinkage(baseUnit, wp.weaponDamage);
        }
    }

    public void HandleAttackRoutine(bool autoAttackInterrupt = false)
    {
        // Check first if attack target has died
        if (attackTarget && attackTarget.GetComponent<RTSUnit>() && attackTarget.GetComponent<RTSUnit>().isDead)
        {
            // If attack target has died at any point during attack routine, clear attack and stop attack routine
            ClearAttack();
            baseUnit.commandQueue.Dequeue();
            return;
        }

        // State isAttacking when engagingTarget and not still orienting to attack position
        if (engagingTarget && (baseUnit.isKinematic ? !isMovingToAttack : !baseUnit.facing))
            isAttacking = true;

        float rangeOffset = activeWeapon.weaponRange;
        // Melee attackers use a portion of the attackTarget's collider (offset) size
        if (attackTarget)
            rangeOffset = activeWeapon.weaponType == Weapon.WeaponTypes.Melee ?
                attackTarget.GetComponent<RTSUnit>().offset.x * 0.85f : activeWeapon.weaponRange;

        // While locked on target but not in range, keep moving to attack position
        bool inRange = false;
        if (attackTarget)
            inRange = baseUnit.IsInRangeOf(attackTarget.transform.position, rangeOffset);
        if (attackTarget && !inRange)
        {
            isMovingToAttack = true;
            isAttacking = false;
            baseUnit.TryToggleToAgent();
            // Move to attack target position 
            baseUnit.MoveToPosition(attackTarget.transform.position);
            // If unit should change attack target to closest in range during move
            if (autoAttackInterrupt)
            {
                GameObject target = FindClosestEnemy();
                // Find the closest enemy that is not already the target
                if (target && target != attackTarget)
                {
                    ClearAttack();
                    TryAttack(target);
                }
                return;
            }
        }
        // Once unit is in range, can stop moving
        else if (isMovingToAttack && attackTarget && inRange)
        {
            baseUnit.MoveToPosition(transform.position);
            // baseUnit.TryToggleToObstacle();
            isMovingToAttack = false;
        }

        // @TODO: if attackTarget becomes null, move back to original location unless there's another unit around to attack
        // @TODO: if isAttacking/taking damage and health is low, AI start a retreat, player units can be set to have an auto retreat or retreat button

        // Handle attacking behavior
        if (isAttacking && attackTarget)
        {
            // Kinematic units do facing; @Note: stationary attack units do their own facing routine (e.g. Stronghold)
            if (baseUnit.isKinematic && !baseUnit.IsMoving())
                baseUnit.facing = baseUnit.HandleFacing(attackTarget.transform.position, 0.5f); // Continue facing attackTarget while isAttacking

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

    public GameObject AutoPickAttackTarget(bool insertFirst = false)
    {
        // Find closest target
        GameObject target = FindClosestEnemy();
        // If a valid target exists but attackTarget has not yet been assigned, lock onto this target
        if (target && target.GetComponent<RTSUnit>() && !attackTarget)
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
            commandPoint = attackTarget.transform.position,
            attackInfo = new AttackInfo { attackTarget = attackTarget }
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
            commandPoint = attackTarget.transform.position,
            attackInfo = new AttackInfo { attackTarget = attackTarget }
        });
    }

    private void SetAttackVars(GameObject target)
    {
        // Debug.Log(gameObject.name + " trying attack on " + target.name);
        attackTarget = target;
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
