using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

// @TODO: rename to KinematicUnitAnimator
/// <summary>
/// Class <c>KinematicUnitAnimator</c> controls the Animator component for units that can move. Includes naval and flying units.
/// </summary>
public class HumanoidUnitAnimator : MonoBehaviour
{
    private Animator _Animator;
    private RTSUnit _Unit;
    private UnitBuilderBase _UnitBuilderScript;
    public int activeAttackIndex { get; set; }

    void Start()
    {
        _Animator = GetComponent<Animator>();
        _Unit = GetComponent<RTSUnit>();

        if (_Unit.isBuilder)
            _UnitBuilderScript = GetComponent<UnitBuilderBase>();
    }

    void Update()
    {
        if (!_Unit.isDead)
        {
            // @TODO: flyers should maybe use their own animator?
            //_Animator.SetBool("flying", (_Unit.canFly && _Unit._FlyingUnit.changingPlanes));

            _Animator.SetBool("moving", _Unit.IsMoving());
            _Animator.SetFloat("speed", _Unit.GetVelocity().sqrMagnitude / (_Unit.maxSpeed * 2));
            _Animator.SetBool("attacking", _Unit.IsAttacking());

            // @TODO: if has been idle for random between 10 - 20 seconds, play idle variant

            // Run attack anim on interval
            if (_Unit.IsAttacking() && _Unit._AttackBehavior.nextAttackReady)
            {
                // Random int between 1 and 3
                activeAttackIndex = Random.Range(1, 4);
                _Animator.SetTrigger("attack_" + activeAttackIndex);
            }

            if (_Unit.isBuilder)
            {
                _Animator.SetBool("conjuring", _UnitBuilderScript.isBuilding);
            }
        }
    }
}
