using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryUnitAnimator : MonoBehaviour
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
        _Animator.SetBool("building", _Unit.state == RTSUnit.States.Conjuring);
    }
}
