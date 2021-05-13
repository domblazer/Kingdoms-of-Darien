using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryUnitAnimator : MonoBehaviour
{
    private Animator m_Animator;
    private UnitBuilder unitBuilder;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        unitBuilder = GetComponent<UnitBuilder>();
    }

    void Update()
    {
        m_Animator.SetBool("building", unitBuilder.IsBuilding());
    }
}
