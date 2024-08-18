using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryUnitScript : MonoBehaviour
{
    public GameObject swivle;
    public float precisionThreshold = 0.2f;
    public float swivleSpeed = 8;
    private Vector3 targetDirection;
    private Quaternion targetRotation;

    private BaseUnit _BaseUnit;

    private void Awake()
    {
        _BaseUnit = gameObject.GetComponent<BaseUnit>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // While unit is still priming for attack but not attacking yet, continue facing to enemy
        if (_BaseUnit._AttackBehavior.engagingTarget && !_BaseUnit.IsAttacking())
        {
            // Determine which direction to rotate towards
            targetDirection = _BaseUnit._AttackBehavior.attackTarget.target.transform.position - transform.position;
            targetDirection.y = 0;
            targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            swivle.transform.rotation = Quaternion.RotateTowards(swivle.transform.rotation, targetRotation, swivleSpeed * Time.deltaTime);

            // Check if rotation is approx done
            if (Quaternion.Angle(swivle.transform.rotation, targetRotation) <= precisionThreshold)
            {
                Debug.Log("Facing done.");
                _BaseUnit.facing = false;
            }
            else
            {
                Debug.Log("Facing...");
                _BaseUnit.facing = true;
            }
        }
    }
}
