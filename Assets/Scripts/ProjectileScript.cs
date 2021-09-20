using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public float removeTime = 5.0f;
    private float damage = 1;
    public AudioClip[] hitSounds;
    public AudioClip[] groundHitSounds;

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

            // play one shot sound
            col.gameObject.GetComponent<BaseUnitAI>().AudioManager.PlayHitSound();

            // @TODO: friendly fire
            // Presumably "Enemy" should only ever be an AI 
            if (col.gameObject.GetComponent<BaseUnitAI>())
            {
                col.gameObject.GetComponent<BaseUnitAI>().ReceiveDamage(damage);
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
            if (groundHitSounds.Length > 0)
            {
                // @TODO: GameManager.GetAudioSource()?
                // col.gameObject.GetComponent<BaseUnitAI>().GetAudioSource().PlayOneShot(groundHitSounds[Random.Range(0, groundHitSounds.Length)], 0.5f);
            }
            Destroy(gameObject);
        }
    }
}
