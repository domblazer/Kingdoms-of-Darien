using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class <c>MeleeWeapon</c> represents behavior of the melee weapon a unit carries. AnimationEvents are required on every melee animation
/// to set _BaseUnit.isStriking, in order to accurately determine when a unit attack should register.
/// </summary>
public class MeleeWeapon : MonoBehaviour
{
    private float damage = 100;
    private RTSUnit _BaseUnit;
    private int activeAttackIndex = 0;

    public void SetLinkage(RTSUnit baseUnit, float d)
    {
        _BaseUnit = baseUnit;
        damage = d;
    }

    private void OnTriggerEnter(Collider col)
    {
        // Melee weapon must be attached to an RTSUnit with a HumanoidUnitAnimator
        if (_BaseUnit != null && _BaseUnit._HumanoidUnitAnimator)
        {
            // activeAttackIndex numbers 1-3, so minus 1 for array index
            activeAttackIndex = _BaseUnit._HumanoidUnitAnimator.activeAttackIndex - 1;

            // Only send damage when the collider makes contact while the unit is mid-strike
            if (_BaseUnit.IsAttacking() && _BaseUnit.isStriking)
            {
                string compareTag = gameObject.tag == "Enemy" ? "Friendly" : "Enemy";
                if (col.gameObject.tag == compareTag && !col.isTrigger)
                {
                    // Debug.Log("_BaseUnit.animStateTime " + _BaseUnit.animStateTime);
                    RTSUnit hitUnit = col.gameObject.GetComponent<RTSUnit>();
                    // Play one shot sound
                    _BaseUnit.AudioManager.PlayMeleeHitSound(hitUnit ? hitUnit.bodyType : RTSUnit.BodyTypes.Default);

                    // @TODO: friendly fire
                    if (col.gameObject.GetComponent<RTSUnit>())
                    {
                        // Debug.Log("I, " + _BaseUnit.unitName + ", meleed " + col.gameObject.name);
                        col.gameObject.GetComponent<RTSUnit>().ReceiveDamage(damage);
                    }
                    else
                    {
                        Debug.Log("problem sending damage");
                    }
                }
            }
        }
    }

    /* private void OnTriggerExit(Collider col)
    {
        if (gameObject.tag == "Friendly")
        {
            Debug.Log("weapon collider exit");
        }
    } */
}
