using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour
{
    public GameObject leftConnector;
    public GameObject rightConnector;
    public GameObject cornerConnector;

    private GhostUnitScript.Directions facingDir;

    public void SetOrientation(GhostUnitScript.Directions facing, bool showLeft, bool showRight, bool showCorner, Quaternion cornerRot)
    {
        facingDir = facing;
        transform.rotation = Quaternion.Euler(transform.rotation.x, (float)facing, transform.rotation.z);

        leftConnector.SetActive(showLeft);
        rightConnector.SetActive(showRight);

        if (showCorner)
        {
            cornerConnector.transform.localRotation = cornerRot;
            cornerConnector.SetActive(true);
        }

    }

    public GhostUnitScript.Directions GetFacingDirection()
    {
        return facingDir;
    }
}
