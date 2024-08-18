using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BaseUnit))]
public class ProjectileLauncher : MonoBehaviour
{
    public Transform launchPoint;
    public Rigidbody projectilePrefab;
    public float projectileVelocity = 30.0f;
    // public AudioClip launchSound;

    private BaseUnit _BaseUnit;

    public float animDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        _BaseUnit = gameObject.GetComponent<BaseUnit>();
    }

    private void Update()
    {
        if (_BaseUnit.IsAttacking() && _BaseUnit._AttackBehavior.nextAttackReady)
        {
            StartCoroutine(Shoot(_BaseUnit._AttackBehavior.attackTarget.target));
        }
    }

    IEnumerator Shoot(GameObject target)
    {
        yield return new WaitForSeconds(animDelay);
        
        // @TODO: may not be efficient to get RTSUnit component every shoot
        // @TODO: handle intangibles

        // Adjust aiming target since all models have position.y = 1, which is ground level; we want to aim for their center
        RTSUnit targetUnit = target.GetComponent<RTSUnit>();
        float yAdjust = targetUnit.offset.y;

        Vector3 adjustedTargetPos = new Vector3(
            target.transform.position.x,
            target.transform.position.y + yAdjust,
            target.transform.position.z
        );

        Rigidbody projectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation) as Rigidbody;
        projectile.GetComponent<ProjectileScript>().SetWhoFired(_BaseUnit);
        projectile.GetComponent<ProjectileScript>().SetDamage(_BaseUnit._AttackBehavior.activeWeapon.weaponDamage);

        projectile.velocity = (adjustedTargetPos - launchPoint.position).normalized * projectileVelocity;
        // @DEP: point-and-shoot way: // projectile.AddForce(transform.forward * projectileVelocity, ForceMode.Impulse);

        // If sounds should be played on launch, not at start of attack
        if (!_BaseUnit.AudioManager.unitPlaysAttackSounds)
            _BaseUnit.AudioManager.PlayAttackSound();

        // ignore collisions between the projectile and parent object
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), projectile.GetComponent<Collider>(), true);
    }
}
