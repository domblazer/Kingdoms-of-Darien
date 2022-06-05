using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

[RequireComponent(typeof(AudioSource))]
/// <summary>
/// Class <c>UnitAudioManager</c> handles the various sounds played by a unit.
/// </summary>
public class UnitAudioManager : MonoBehaviour
{
    protected AudioSource _AudioSource;
    protected RTSUnit baseUnit;

    // Sounds
    public AudioClip[] attackSounds;
    public AudioClip dieSound;
    public AudioClip moveSound;
    public AudioClip selectSound;

    // Specifically to set whether RTSUnit script will play the attack sounds (e.g. Archer plays through ProjectileLauncher)
    public bool unitPlaysAttackSounds = true;

    private void Awake()
    {
        _AudioSource = GetComponent<AudioSource>();
        _AudioSource.spatialBlend = 0.8f;
        _AudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        _AudioSource.minDistance = 12;
        baseUnit = GetComponent<RTSUnit>();
    }

    public void PlayAttackSound()
    {
        // @TODO: vary when to play a sound, to limit the amount of one shots playing at a time when lots of units
        // @TODO: some sounds need a delay to account for animation
        if (attackSounds.Length > 0)
            _AudioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)]);
    }

    public void PlayDieSound()
    {
        // Play the die sound 50% of the time
        if (Random.Range(0.0f, 1.0f) > 0.5f && dieSound != null)
            _AudioSource.PlayOneShot(dieSound);
    }

    // This function is only for Melee weapon hits
    public void PlayMeleeHitSound(RTSUnit.BodyTypes hitBodyType)
    {
        // Get the appropriate sounds based on this unit's weapon type and what type of body was hit
        AudioClip[] hitSounds = SoundHitClasses.GetHitSounds(baseUnit._AttackBehavior.activeWeapon.weaponSoundClass, hitBodyType);
        if (hitSounds.Length > 0)
            _AudioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length - 1)]);
    }

    // Clone the audio source temporarily to play a Projectile hit sound
    public void PlayHitAtPoint(Vector3 hitPos, AudioClip clip)
    {
        GameObject tempObj = new GameObject("(hit_clip_temp)", typeof(AudioSource));
        tempObj.transform.position = hitPos;
        AudioSource tempSource = tempObj.GetComponent<AudioSource>();
        // @TODO: ideally the temp audio source should be an exact copy of _AudioSource, instead of specific properties
        tempSource.rolloffMode = _AudioSource.rolloffMode;
        tempSource.spatialBlend = _AudioSource.spatialBlend;
        tempSource.PlayOneShot(clip);
        Destroy(tempObj, clip.length);
    }

    public void PlayMoveSound()
    {
        // Play the move sound 50% of the time and only when unit is alone
        if (Random.Range(0.0f, 1.0f) > 0.5f && moveSound != null)
            _AudioSource.PlayOneShot(moveSound);
    }

    public void PlaySelectSound()
    {
        // Play the select sound 50% of the time
        if (Random.Range(0.0f, 1.0f) > 0.5f && selectSound != null)
            _AudioSource.PlayOneShot(selectSound);
    }
}
