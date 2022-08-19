using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarienEngine;

public class FlyingUnit : MonoBehaviour
{
    private BaseUnit _BaseUnit;
    public bool changingPlanes { get; set; } = false;
    public bool isAirborne { get; set; } = false;
    private float transitionSpeed = 7f;
    public Vector3 lastCorrespondingGroundPoint { get; set; }
    private Vector3 adjustedPoint;
    private bool isAdjusting = false;
    // Certain flying units turn slowly and should not do extra facing rotation
    public bool doQuickFacing = false;

    private void Awake()
    {
        _BaseUnit = gameObject.GetComponent<BaseUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        // If unit has a command but is not airborne, takeoff, or if no commands and is airborne, seek to land
        if (((!_BaseUnit.commandQueue.IsEmpty() && !isAirborne) || (_BaseUnit.commandQueue.IsEmpty() && isAirborne)) && !changingPlanes)
        {
            _BaseUnit._Agent.enabled = false;
            _BaseUnit._Obstacle.enabled = false;
            changingPlanes = true;
            if (isAirborne)
            {
                // Adjust landing point to the ground hit point
                adjustedPoint = new Vector3(lastCorrespondingGroundPoint.x, transform.position.y, lastCorrespondingGroundPoint.z);
                if (!_BaseUnit.IsInRangeOf(adjustedPoint))
                {
                    _BaseUnit.SetMove(adjustedPoint);
                    isAdjusting = true;
                }
            }
            // @TODO: if you get another command during landing, need to interrupt and takeoff again
        }

        // @TODO: spyhawk not revealing ground units b/c radar trigger shape/size? Should make radar triggers capsules?

        // @TODO: landing not working on group move
        if (changingPlanes)
        {
            if (isAdjusting && _BaseUnit.IsInRangeOf(adjustedPoint))
                isAdjusting = false;
            // Translate up or down
            if (isAirborne && !isAdjusting)
            {
                // Land by descending down y axis
                transform.Translate(Vector3.down * Time.deltaTime * transitionSpeed, Space.World);

                if (transform.position.y <= 1)
                {
                    _BaseUnit._Agent.enabled = true;
                    changingPlanes = false;
                    transform.position = new Vector3(transform.position.x, 1, transform.position.z);
                    isAirborne = false;
                }
            }
            else
            {
                // Taking off
                // @TODO: there is some movement towards the point that happens during takeoff
                transform.Translate(Vector3.up * Time.deltaTime * transitionSpeed, Space.World);
                if (transform.position.y >= 10)
                {
                    _BaseUnit._Agent.enabled = true;
                    changingPlanes = false;
                    transform.position = new Vector3(transform.position.x, 10, transform.position.z);
                    isAirborne = true;
                }
            }

        }
    }
}
