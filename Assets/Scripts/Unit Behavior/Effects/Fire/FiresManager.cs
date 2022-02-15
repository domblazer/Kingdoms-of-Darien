using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiresManager : MonoBehaviour
{
    private Fire[] fireObjects;
    private bool[] fireToggleTriggers;
    private RTSUnit _Unit;

    // Start is called before the first frame update
    void Start()
    {
        _Unit = gameObject.GetComponentInParent<RTSUnit>();
        fireObjects = gameObject.GetComponentsInChildren<Fire>();
        fireToggleTriggers = new bool[fireObjects.Length];
        TurnOffAllFires();
    }

    // Update is called once per frame
    void Update()
    {
        int partSize = 100 / fireObjects.Length;
        int o = 100 - partSize;

        // Start checking to turn on fires just before the first threshold, giving us time to turn off the final fire when repairing
        if (_Unit.health <= o + 1)
        {
            for (int i = 0; i < fireObjects.Length - 1; i++)
            {
                if (_Unit.health >= o - partSize && _Unit.health < o)
                {
                    // Turn on the fire at this index and set it's trigger to true, so .Play() isn't called repeatedly
                    TurnOnFire(fireObjects[i], fireToggleTriggers[i]);
                    fireToggleTriggers[i] = true;
                }
                else if (_Unit.health >= o)
                {
                    // Turn off fire and set trigger if not in range to show
                    TurnOffFire(fireObjects[i], fireToggleTriggers[i]);
                    fireToggleTriggers[i] = false;
                }
                // Slide the health range to check down by partSize
                o -= partSize;
            }
        }
        // @TODO: need to unparent the fires from building when it dies and stop the particle systems so they don't just disappear
    }

    private void TurnOffAllFires()
    {
        foreach (Fire fireObj in fireObjects)
            TurnOffFire(fireObj);
    }

    private void TurnOnAllFires()
    {
        foreach (Fire fireObj in fireObjects)
            TurnOnFire(fireObj);
    }

    private void TurnOnFire(Fire fireObj, bool alreadyDone = false)
    {
        if (!alreadyDone)
        {
            StartParticles(fireObj);
            if (fireObj.firePointLight)
                fireObj.firePointLight.gameObject.SetActive(true);
        }
    }

    private void TurnOffFire(Fire fireObj, bool alreadyDone = false)
    {
        if (!alreadyDone)
        {
            if (fireObj.firePointLight)
                fireObj.firePointLight.gameObject.SetActive(false);
            StopParticles(fireObj);
        }
    }

    private void StartParticles(Fire fireObj)
    {
        ParticleSystem[] particles = fireObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particles)
            p.Play();
    }

    private void StopParticles(Fire fireObj)
    {
        ParticleSystem[] particles = fireObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particles)
            p.Stop();
    }
}
