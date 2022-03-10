using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

[RequireComponent(typeof(AudioSource))]
public class UnitAudioManager : MonoBehaviour
{
    protected AudioSource _AudioSource;
    protected RTSUnit baseUnit;
    public SoundHitClasses.WeaponSoundHitClasses[] weaponSoundHitClasses;

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
        baseUnit = GetComponent<RTSUnit>();
    }

    public void PlayAttackSound()
    {
        // @TODO: vary when to play a sound, to limit the amount of one shots playing at a time when lots of units
        // @TODO: some sounds need a delay to account for animation
        if (attackSounds.Length > 0)
            _AudioSource.PlayOneShot(attackSounds[Random.Range(0, attackSounds.Length)], 0.4f);
    }

    public void PlayDieSound()
    {
        // Play the die sound 50% of the time
        if (Random.Range(0.0f, 1.0f) > 0.5f && dieSound != null)
            _AudioSource.PlayOneShot(dieSound, 0.5f);
    }

    public void PlayHitSound(RTSUnit.BodyTypes hitBodyType)
    {
        // Get the appropriate sounds based on weapon type and what type of body was hit
        // @TODO: if weaponSoundHitClasses is null, do nothing, show warning
        AudioClip[] hitSounds = SoundHitClasses.GetHitSounds(weaponSoundHitClasses[baseUnit.activeWeaponIndex], hitBodyType);
        if (hitSounds.Length > 0)
            _AudioSource.PlayOneShot(hitSounds[Random.Range(0, hitSounds.Length - 1)], 0.5f);
    }

    public void PlayMoveSound()
    {
        // Play the move sound 50% of the time and only when unit is alone
        if (Random.Range(0.0f, 1.0f) > 0.5f && moveSound != null)
            _AudioSource.PlayOneShot(moveSound, 1);
    }

    public void PlaySelectSound()
    {
        // Play the select sound 50% of the time
        if (Random.Range(0.0f, 1.0f) > 0.5f && selectSound != null)
            _AudioSource.PlayOneShot(selectSound, 1);
    }
}
