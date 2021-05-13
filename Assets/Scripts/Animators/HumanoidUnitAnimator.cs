using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidUnitAnimator : MonoBehaviour
{
    private Animator _Animator;
    private RTSUnit _Unit;

    void Start()
    {
        _Animator = GetComponent<Animator>();
        _Unit = GetComponent<RTSUnit>();
    }

    void Update()
    {
        if (!_Unit.isDead)
        {
            // @TODO
            if (_Unit.IsMoving())
            {
                //  _Animator.speed = _Unit.GetVelocity() ;
            }
            _Animator.SetBool("walking", _Unit.IsMoving());
            // @TODO: running
            _Animator.SetBool("attacking", _Unit.IsAttacking());

            // @TODO: if has been idle for random between 10 - 20 seconds, play idle variant

            // Run attack anim on interval
            if (_Unit.IsAttacking() && _Unit.nextAttackReady)
            {
                // Random int between 1 and 3
                int r = Random.Range(1, 4);
                _Animator.SetTrigger("attack_" + r);
            }

            if (_Unit.isBuilder)
            {
                _Animator.SetBool("conjuring", _Unit._UnitBuilderScript.IsBuilding());
            }

            if (_Unit.IsAttacking())
            {
                _Unit.animStateTime = _Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            }
        }
    }
}
