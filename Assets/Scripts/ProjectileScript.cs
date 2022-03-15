using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float removeTime = 5.0f;
    private float damage = 1;
    private RTSUnit whoFired;

    // Start is called before the first frame update
    void Start()
    {
        // @TODO: set damage from referrer 

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

    void Update()
    {
        // Arrows slant towards the ground in the air
        // transform.Rotate(Time.deltaTime * -100, 0, 0);
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy" && !col.isTrigger)
        {
            Debug.Log("Got em!");

            // @TODO: friendly fire
            // Presumably "Enemy" should only ever be an AI
            RTSUnit hitUnit;
            if (hitUnit = col.gameObject.GetComponent<RTSUnit>())
            {
                // Projectile hit clip plays sound based on weapon of firer and at point
                AudioClip[] clips = SoundHitClasses.GetHitSounds(whoFired._AttackBehavior.activeWeapon.weaponSoundClass, hitUnit.bodyType);
                // @TODO: PlayClipAtPoint is very quiet, not working like other unit audio sources
                // AudioSource.PlayClipAtPoint(clips[Random.Range(0, clips.Length - 1)], transform.position, 0.5f);
                whoFired.AudioManager.PlayHitAtPoint(transform.position, clips[Random.Range(0, clips.Length - 1)]);
                hitUnit.ReceiveDamage(damage);
            }
            else
            {
                Debug.Log("problem sending damage");
            }
            Destroy(gameObject);
        }
        else if (col.gameObject.tag == "Terrain")
        {
            Debug.Log("Hit ground");
            // play hit ground sound
            AudioClip groundHitSound = SoundHitClasses.GetHitSounds(whoFired._AttackBehavior.activeWeapon.weaponSoundClass, RTSUnit.BodyTypes.Default)[0];
            // AudioSource.PlayClipAtPoint(groundHitSound, transform.position, 0.5f);
            whoFired.AudioManager.PlayHitAtPoint(transform.position, groundHitSound);
            Destroy(gameObject);
        }
    }
}
