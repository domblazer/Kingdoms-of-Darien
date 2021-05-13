using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortUnitAnimatorScript : MonoBehaviour
{
    private Animator m_Animator;
    private BaseUnitScript baseUnit;
    private BaseUnitScriptAI baseUnitAI;

    public bool isAI = false;

    void Start()
    {
        m_Animator = GetComponent<Animator>();
        if (isAI)
        {
            baseUnitAI = GetComponent<BaseUnitScriptAI>();
        }
        else
        {
            baseUnit = GetComponent<BaseUnitScript>();
        }
    }

    void Update()
    {
        if (isAI)
        {
            m_Animator.SetBool("facing", baseUnitAI.facing);
            // m_Animator.SetBool("attacking", baseUnitAI.IsAttacking());
        }
        else
        {
            m_Animator.SetBool("facing", baseUnit.facing);
            // m_Animator.SetBool("attacking", baseUnit.IsAttacking());

            // Run attack anim on interval
            if (baseUnit.IsAttacking() && baseUnit.nextAttackReady)
            {
               //  Debug.Log("Attack anim triggered");
                m_Animator.SetTrigger("attack");
            }
        }
    }
}
