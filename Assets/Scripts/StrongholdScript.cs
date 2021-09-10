using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongholdScript : MonoBehaviour
{
    public GameObject swivle;
    public GameObject cannon;

    public float precisionThreshold = 0.2f;
    public float cannonMaxAngle = 30.0f;
    public float swivleSpeed = 8;

    private Quaternion targetRotation;
    private Vector3 targetDirection;

    private BaseUnit _BaseUnit;

    private float step;

    private Vector3 velocity; 
    private float swivleLookDamp = 1;
    private float maxLookSpeed = 50;

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
        if (_BaseUnit.engagingTarget)
        {
            // @TODO: swivle -> LookRotation around y axis (side-to-side)
            /* Vector3 targetPos = _BaseUnit.attackTarget.transform.position;
            Vector3 swivlePos = swivle.transform.position;
            Vector3 direction = targetPos - swivlePos;
            direction.y = 0;
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);

            // Stop the SmoothDamp when angles are similar
            if (Quaternion.Angle(swivle.transform.rotation, targetRotation) > 0.01f)
            {
                swivle.transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(swivle.transform.rotation.eulerAngles, targetRotation.eulerAngles, ref velocity, cannonLookDamp, maxLookSpeed));
            } */
            
            // Determine which direction to rotate towards
            targetDirection = _BaseUnit.attackTarget.transform.position - transform.position;
            targetDirection.y = 0;
            targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            swivle.transform.rotation = Quaternion.RotateTowards(swivle.transform.rotation, targetRotation, swivleSpeed * Time.deltaTime);

            // @TODO: set animation speed -1 or 1 based on look direction

            // Cannon look: x axis (up and down) only
            Vector3 cannonFacingDirection = _BaseUnit.attackTarget.transform.position - cannon.transform.localPosition;
            cannonFacingDirection.x = 0;
            Quaternion cannonTargetRotation = Quaternion.LookRotation(cannonFacingDirection, Vector3.up);

            // Clamp cannon rotation angles
            if (cannon.transform.localRotation.eulerAngles.x >= -cannonMaxAngle
                && cannon.transform.localRotation.eulerAngles.x >= 0
                && cannon.transform.localRotation.eulerAngles.x <= cannonMaxAngle)
            {
                cannon.transform.localRotation = Quaternion.Euler(Vector3.SmoothDamp(cannon.transform.localRotation.eulerAngles, cannonTargetRotation.eulerAngles, ref velocity, swivleLookDamp, maxLookSpeed));
            }

            // Check if rotation is approx done
            if (Quaternion.Angle(swivle.transform.rotation, targetRotation) <= 0.01f)
            {
                Debug.Log("Facing done.");
                _BaseUnit.facing = false;
                _BaseUnit.isAttacking = true;
            }
            else
            {
                Debug.Log("Facing...");
                _BaseUnit.facing = true;
                _BaseUnit.isAttacking = false;
            }

        }

    }
}
