﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseUnit))]
public class WatchTowerScript : MonoBehaviour
{
    public GameObject archerOne;
    public GameObject archerTwo;
    public Transform launchPointOne;
    public Transform launchPointTwo;
    private Animator archerOneAnimator;
    private Animator archerTwoAnimator;
    public float animDelay = 1.5f;
    public Rigidbody projectilePrefab;
    public float projectileVelocity = 1;

    private BaseUnit _BaseUnit;

    // Start is called before the first frame update
    void Start()
    {
        archerOneAnimator = archerOne.GetComponent<Animator>();
        archerTwoAnimator = archerTwo.GetComponent<Animator>();
        _BaseUnit = GetComponent<BaseUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_BaseUnit.IsAttacking())
        {
            ArcherLookAt(archerOne, _BaseUnit._AttackBehavior.attackTarget.target);
            ArcherLookAt(archerTwo, _BaseUnit._AttackBehavior.attackTarget.target);
        }
        if (_BaseUnit.IsAttacking() && _BaseUnit._AttackBehavior.nextAttackReady)
        {
            ArcherLookAt(archerOne, _BaseUnit._AttackBehavior.attackTarget.target);
            ArcherLookAt(archerTwo, _BaseUnit._AttackBehavior.attackTarget.target);
            archerOneAnimator.SetTrigger("attack_1");
            Debug.Log("Watch Tower archer 1 triggered");
            StartCoroutine(Shoot(_BaseUnit._AttackBehavior.attackTarget.target, launchPointOne));
            StartCoroutine(TriggerSecond());
        }
        // @TODO: if baseUnit.nextAttackReady, archerOne.animator.setTrigger("attack_01") & [set delay, then] archerTwo.animator.setTrigger(...)
        // @TODO: rotate the archers in place to face their target
        // @TODO: make this work with projectileLauncherScript
    }

    IEnumerator TriggerSecond()
    {
        yield return new WaitForSeconds(0.25f);
        archerTwoAnimator.SetTrigger("attack_1");
        Debug.Log("Watch Tower archer 2 triggered");
        StartCoroutine(Shoot(_BaseUnit._AttackBehavior.attackTarget.target, launchPointTwo));
    }

    IEnumerator Shoot(GameObject target, Transform launchPoint)
    {
        yield return new WaitForSeconds(animDelay);

        // @TODO: if target is null, should clear attack so this doesn't get called
        // Adjust aiming target since all models have position.y = 1, which is ground level; we want to aim for their center
        Vector3 adjustedTargetPos = new Vector3(target.transform.position.x, target.transform.position.y + 2, target.transform.position.z);

        Rigidbody projectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation) as Rigidbody;
        projectile.GetComponent<ProjectileScript>().SetWhoFired(_BaseUnit);
        projectile.GetComponent<ProjectileScript>().SetDamage(_BaseUnit._AttackBehavior.activeWeapon.weaponDamage);
        projectile.velocity = (adjustedTargetPos - launchPoint.position).normalized * projectileVelocity;

        // If sounds should be played on launch, not at start of attack
        if (!_BaseUnit.AudioManager.unitPlaysAttackSounds)
            _BaseUnit.AudioManager.PlayAttackSound();

        // ignore collisions between the projectile and parent object
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), projectile.GetComponent<Collider>(), true);
    }

    void ArcherLookAt(GameObject archer, GameObject target)
    {
        Vector3 targetDirection = target.transform.position - archer.transform.position;
        targetDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        archer.transform.rotation = Quaternion.RotateTowards(archer.transform.rotation, targetRotation, 100 * Time.deltaTime);
    }
}
