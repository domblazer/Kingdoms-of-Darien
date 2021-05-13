using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BaseUnitScript))]
public class ProjectileLauncher : MonoBehaviour
{
    public Transform launchPoint;
    public Rigidbody projectilePrefab;
    public float projectileVelocity = 30.0f;
    // public AudioClip launchSound;

    private BaseUnitScript _BaseUnit;

    public float animDelay = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        _BaseUnit = gameObject.GetComponent<BaseUnitScript>();
    }

    private void Update()
    {
        if (_BaseUnit.IsAttacking() && _BaseUnit.nextAttackReady)
        {
            StartCoroutine(Shoot(_BaseUnit.attackTarget));
        }
    }

    IEnumerator Shoot(GameObject target)
    {
        yield return new WaitForSeconds(animDelay);

        // GetComponent<AudioSource>().PlayOneShot(launchSound);

        // Adjust aiming target since all models have position.y = 1, which is ground level; we want to aim for their center
        // @TODO: should adjust y by an offset value based on the target's (collider?) height
        Vector3 adjustedTargetPos = new Vector3(target.transform.position.x, target.transform.position.y + 2, target.transform.position.z);
        /* Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction); */

        Rigidbody projectile = Instantiate(projectilePrefab, launchPoint.position, launchPoint.rotation) as Rigidbody;
        projectile.GetComponent<ProjectileScript>().SetDamage(_BaseUnit.weaponDamage);

        // projectile.velocity = transform.forward * projectileVelocity;
        if (_BaseUnit.isKinematic)
        {
            // E.g. archer arrow doesn't track towards enemy, just point and shoot
            projectile.AddForce(transform.forward * projectileVelocity, ForceMode.Impulse);
        }
        else
        {
            // E.g. trebuchet needs to target exact enemy position in order to hit
            projectile.velocity = (adjustedTargetPos - launchPoint.position).normalized * projectileVelocity;
        }

        // If sounds should be played on launch, not at start of attack
        if (_BaseUnit.attackSounds.Length > 0 && !_BaseUnit.playAttackSounds)
        {
            _BaseUnit.GetAudioSource().PlayOneShot(_BaseUnit.attackSounds[Random.Range(0, _BaseUnit.attackSounds.Length)], 0.4f);
        }

        // ignore collisions between the projectile and parent object
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), projectile.GetComponent<Collider>(), true);
    }
}
