using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponScript : MonoBehaviour
{
    private float damage = 100;
    private RTSUnit _BaseUnit;

    [System.Serializable]
    public class AnimationClipRange
    {
        public float start = 0;
        public float end = 1.0f;
    }

    public AnimationClipRange[] animationClipRange = new AnimationClipRange[3];
    private int activeAttackIndex = 0;

    public void SetLinkage(RTSUnit baseUnit, float d)
    {
        _BaseUnit = baseUnit;
        damage = d;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (_BaseUnit._HumanoidUnitAnimator)
            activeAttackIndex = _BaseUnit._HumanoidUnitAnimator.activeAttackIndex;
        // Only send damage when the collider makes contact while the unit is in attack mode
        if (_BaseUnit != null && _BaseUnit.IsAttacking() && _BaseUnit.animStateTime > animationClipRange[activeAttackIndex].start && _BaseUnit.animStateTime < animationClipRange[activeAttackIndex].end)
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

    /* private void OnTriggerExit(Collider col)
    {
        if (gameObject.tag == "Friendly")
        {
            Debug.Log("weapon collider exit");
        }
    } */
}
