using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float removeTime = 5.0f;
    private float damage = 1;
    private RTSUnit whoFired;

    void Start()
    {
        // Tidy the gameObject by removing after set amount of time
        Destroy(gameObject, removeTime);
    }

    public void SetDamage(float d)
    {
        damage = d;
    }

    public void SetWhoFired(RTSUnit baseUnit)
    {
        whoFired = baseUnit;
    }

    void OnTriggerEnter(Collider col)
    {
        // Hit enemy unit collider
        if (col.gameObject.tag == "Enemy" && !col.isTrigger)
        {
            // @TODO: handle intangibles
            RTSUnit hitUnit;
            if (hitUnit = col.gameObject.GetComponent<RTSUnit>())
            {
                // Projectile hit clip plays sound based on weapon of firer and at point
                AudioClip[] clips = SoundHitClasses.GetHitSounds(whoFired._AttackBehavior.activeWeapon.weaponSoundClass, hitUnit.bodyType);
                whoFired.AudioManager.PlayHitAtPoint(transform.position, clips[Random.Range(0, clips.Length - 1)]);
                hitUnit.ReceiveDamage(damage);
            }
            else
            {
                Debug.LogWarning("problem sending damage");
            }
            Destroy(gameObject);
        }
        // Hit ground or obstacle
        else if (col.gameObject.tag == "Terrain" || col.gameObject.layer == 7)
        {
            // Play default hit sound
            AudioClip groundHitSound = SoundHitClasses.GetHitSounds(whoFired._AttackBehavior.activeWeapon.weaponSoundClass, RTSUnit.BodyTypes.Default)[0];
            whoFired.AudioManager.PlayHitAtPoint(transform.position, groundHitSound);
            Destroy(gameObject);
        }
        // @TODO: friendly fire
    }
}
