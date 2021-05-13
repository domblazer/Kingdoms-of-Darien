using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallGhostScript : MonoBehaviour
{
    public GameObject leftConnector;
    public GameObject rightConnector;
    public GameObject cornerConnector;

    // @TODO: states: Ghost, Intangible, or BaseUnit
    private GhostUnitScript _Ghost;
    private IntangibleUnitScript _Intangible;
    private BaseUnitScript _BaseUnit;

    private enum WallStates
    {
        Ghost, Intangible, BaseUnit
    }

    private WallStates state = WallStates.Ghost;

    public struct Orientation
    {
        public bool leftConnectorActive;
        public bool rightConnectorActive;
        public bool cornerConnectorActive;
        public Quaternion cornerRotation;

        public Orientation(bool left, bool right, bool corner, Quaternion cornerRot)
        {
            leftConnectorActive = left;
            rightConnectorActive = right;
            cornerConnectorActive = corner;
            cornerRotation = cornerRot;
        }
    }

    public Orientation frozenOrientation;

    // Start is called before the first frame update
    void Start()
    {
        leftConnector.SetActive(false);
        rightConnector.SetActive(false);
        cornerConnector.SetActive(false);

        if (state == WallStates.Ghost)
        {
            _Ghost = GetComponent<GhostUnitScript>();
        }

    }

    void OnTriggerExit(Collider other)
    {
        leftConnector.SetActive(false);
        rightConnector.SetActive(false);
        cornerConnector.SetActive(false);
    }

    // @TODO: need to merge this script with standard wall behavior, b/c wall ghost still needs to interact with 
    // intangibles and main wall to determine connectors

    private void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Friendly" && col.gameObject.layer == 12)
        {
            HandleConnectors(col.gameObject.transform);
        }
    }

    void HandleConnectors(Transform connectingWall)
    {
        Vector3 heading = connectingWall.position - transform.position;
        float dirNum = AngleDir(transform.forward, heading, transform.up);

        GhostUnitScript.Directions facingDir = _Ghost.GetFacingDirection();
        GhostUnitScript.Directions otherFacingDir = connectingWall.GetComponent<GhostUnitScript>().GetFacingDirection();

        switch (dirNum)
        {
            case 0:
                // I touched a wall either in front or behind me

                // if I am facing right or left and other is facing forward or back and other doesn't have appropriate 
                // right or left connector active, activate the corner
                Debug.Log("I touched a wall either in front or behind me");

                // @TODO: if walls have different rotations, you can't really compare same axis. Need to find a way to do a better axis comparison

                float cornerRot = -1.0f;
                if (facingDir == GhostUnitScript.Directions.Forward &&
                    otherFacingDir == GhostUnitScript.Directions.Right)
                {
                    Debug.Log("I am facing forward, other right");
                    cornerRot = (float)GhostUnitScript.Directions.Left;
                }
                else if (facingDir == GhostUnitScript.Directions.Forward &&
                  otherFacingDir == GhostUnitScript.Directions.Left)
                {
                    Debug.Log("I am facing forward, other left");
                    cornerRot = (float)GhostUnitScript.Directions.Backwards;
                }
                else if (facingDir == GhostUnitScript.Directions.Backwards &&
                    otherFacingDir == GhostUnitScript.Directions.Left)
                {
                    Debug.Log("I am facing Backwards, other left");
                    cornerRot = (float)GhostUnitScript.Directions.Left;
                }
                else if (facingDir == GhostUnitScript.Directions.Backwards &&
                    otherFacingDir == GhostUnitScript.Directions.Right)
                {
                    Debug.Log("I am facing Backwards, other right");
                    cornerRot = (float)GhostUnitScript.Directions.Backwards;
                }

                if (cornerRot != -1.0f)
                {
                    cornerConnector.transform.localRotation = Quaternion.Euler(cornerConnector.transform.localRotation.x,
                                                                                cornerRot,
                                                                                cornerConnector.transform.localRotation.z);
                    cornerConnector.SetActive(true);
                }

                break;
            case 1:
                // I touched a wall to my right
                Debug.Log("I touched a wall to my right");
                // check both are facing same direction, axes align, and corner is not active
                if (facingDir == otherFacingDir && CheckAxisAlignment(connectingWall) && !cornerConnector.activeInHierarchy)
                {
                    Debug.Log("activating left connector");
                    leftConnector.SetActive(true);
                }
                else if (facingDir == otherFacingDir && CheckAxisAlignment(connectingWall) &&
                    connectingWall.GetComponent<WallGhostScript>().cornerConnector.activeInHierarchy)
                {
                    Debug.Log("deactivating right connector");
                    rightConnector.SetActive(false);
                }
                // @TODO: if I am touching a wall whose corner is active, I need to deactivate one of my connectors

                // @TODO facing different dirs
                /* else if (facingDir == )
                {

                } */
                else
                {
                    leftConnector.SetActive(false);
                }

                break;
            case -1:
                // I touched a wall to my left
                Debug.Log("I touched a wall to my left");
                if (facingDir == otherFacingDir && CheckAxisAlignment(connectingWall) && !cornerConnector.activeInHierarchy)
                {
                    Debug.Log("activating right connector");
                    rightConnector.SetActive(true);
                }
                /* else if (facingDir == GhostUnitScript.Directions.Right &&
                    otherFacingDir == GhostUnitScript.Directions.Forward)
                {
                    // do nothing, corner will be activated by the wall in front of me
                    // leftConnector.SetActive(false);
                } */
                else
                {
                    rightConnector.SetActive(false);
                }

                break;
        }
    }

    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0f)
        {
            // right
            return 1f;
        }
        else if (dir < 0f)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    bool CheckAxisAlignment(Transform otherWall)
    {
        GhostUnitScript.Directions thisDir = _Ghost.GetFacingDirection();
        GhostUnitScript.Directions otherDir = otherWall.GetComponent<GhostUnitScript>().GetFacingDirection();

        // If both are side alignment, compare position.x
        if ((thisDir == GhostUnitScript.Directions.Left || thisDir == GhostUnitScript.Directions.Right) &&
            (otherDir == GhostUnitScript.Directions.Left || otherDir == GhostUnitScript.Directions.Right))
        {
            // Check x align and z distance is 2
            float distApart = Mathf.Abs(transform.position.z - otherWall.position.z);
            return transform.position.x == otherWall.position.x && distApart == 2;
        }
        // If both are forward/back alignment, compare position.z
        else if ((thisDir == GhostUnitScript.Directions.Forward || thisDir == GhostUnitScript.Directions.Backwards) &&
            (otherDir == GhostUnitScript.Directions.Forward || otherDir == GhostUnitScript.Directions.Backwards))
        {
            // Check z align and x distance is 2
            float distApart = Mathf.Abs(transform.position.x - otherWall.position.x);
            return transform.position.z == otherWall.position.z && distApart == 2;
        }
        return false;
    }

    public void FreezeOrientation()
    {
        frozenOrientation = new Orientation(leftConnector.activeInHierarchy,
                                            rightConnector.activeInHierarchy,
                                            cornerConnector.activeInHierarchy,
                                            cornerConnector.transform.localRotation);
        Debug.Log("My orientation has been frozen: " + " show left: " + leftConnector.activeInHierarchy + " " +
                                            " show right: " + rightConnector.activeInHierarchy + " " +
                                            " show corner: " + cornerConnector.activeInHierarchy);
    }
}
