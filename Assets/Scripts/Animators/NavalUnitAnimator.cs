using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavalUnitAnimator : MonoBehaviour
{
    private Animator _Animator;
    private RTSUnit _Unit;

    void Start()
    {
        _Animator = GetComponent<Animator>();
        _Unit = GetComponent<RTSUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        _Animator.SetBool("moving", _Unit.IsMoving());
        _Animator.SetBool("turning", _Unit.facing);

        if (_Unit.IsAttacking() && _Unit._AttackBehavior.nextAttackReady)
        {
            _Animator.SetTrigger("attack");
        }
    }
}
