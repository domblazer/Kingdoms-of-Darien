using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RTSCamera : MonoBehaviour
{
    public int Boundary = 50;
    private int screenWidth;
    private int screenHeight;

    public float zoomDistanceMax = 200f;
    public float zoomDistanceMin = 5f;
    public float zoomDistance = 100f;

    public float ScrollSpeed = 20.0f;
    public float PanSpeed = 50.0f;
    public float ZoomSpeed = 40.0f;

    public float CurrentZoom = 0.0f;

    private float x = 0f;
    private float y = 0f;
    public float xOrbitSpeed = 120f;
    public float yOrbitSpeed = 120f;
    private Vector3 resetPos;
    private Quaternion resetRot;

    public Transform[] views;
    private int currentView = 0;
    private bool lerpCam = false;
    public float lerpSpeed = 10;
    private float startTime;
    private float camJourneyLength;

    private bool doCameraBounding = true;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        screenWidth = Screen.width;
        screenHeight = Screen.height;

        startTime = Time.time;
        // camJourneyLength = Vector3.Distance(transform.position, views[currentView].position);
    }

    // Update is called once per frame
    void Update()
    {
        // ORBIT CAM
        if (Input.GetKey("mouse 2"))
        {
            /* x += Input.GetAxis("Mouse X") * xOrbitSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * yOrbitSpeed * 0.02f;

            Quaternion rotation = Quaternion.Euler(y, x, 0); */

            // TODO: orbit cam should "orbit" around the mouse click position like it does 
            // in the scene view. Also, clamp the y angles so you can't rotate under the plane of
            // empty space

            // transform.rotation = rotation;

            // PAN
            // transform.Translate(Vector3.right * Time.deltaTime * PanSpeed * (Input.mousePosition.x - screenWidth * 0.5f) / (screenWidth * 0.5f), Space.World);
            // transform.Translate(Vector3.forward * Time.deltaTime * PanSpeed * (Input.mousePosition.y - screenHeight * 0.5f) / (screenHeight * 0.5f), Space.World);
        }
        else
        {
            if (doCameraBounding)
            {
                // RIGHT
                if (Input.mousePosition.x > screenWidth - Boundary || Input.GetKey(KeyCode.RightArrow))
                {
                    transform.Translate(new Vector3(ScrollSpeed * Time.deltaTime, 0, 0));
                }
                // LEFT
                if (Input.mousePosition.x < 0 + Boundary || Input.GetKey(KeyCode.LeftArrow))
                {
                    transform.Translate(new Vector3(-ScrollSpeed * Time.deltaTime, 0, 0));
                }
                // FORWARDS
                if (Input.mousePosition.y > screenHeight - Boundary || Input.GetKey(KeyCode.UpArrow))
                {
                    //transform.Translate(new Vector3(ScrollSpeed * Time.deltaTime, 0, 0));

                    //transform.position += transform.forward * ScrollSpeed * Time.deltaTime;

                    transform.Translate(Vector3.forward * Time.deltaTime * ScrollSpeed, Space.World);
                }
                // BACKWARDS
                if (Input.mousePosition.y < 0 + Boundary || Input.GetKey(KeyCode.DownArrow))
                {
                    transform.Translate(-Vector3.forward * Time.deltaTime * ScrollSpeed, Space.World);
                }
            }
            // TODO: add movements to WASD and Arrow keys
        }

        // Lerp camera to new position and rotation
        if (lerpCam)
        {
            float distCovered = (Time.time - startTime) * lerpSpeed;
            float fracJourney = distCovered / camJourneyLength;
            transform.position = Vector3.Lerp(transform.position, views[currentView].position, fracJourney);
            transform.rotation = Quaternion.Lerp(transform.rotation, views[currentView].rotation, fracJourney);

            Vector3 offset = transform.position - views[currentView].position;
            //Debug.Log(offset);
            if (offset.magnitude < 20)
            {
                lerpCam = false;
                transform.rotation = views[currentView].rotation;
            }

            // TODO: if camera is moved or scrolled during movement, break the lerp
        }


        // TODO: Lift and rise with RB/LB


        // ZOOM for orthographic cam
        /*if(Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            this.GetComponent<Camera>().orthographicSize -= ZoomSpeed;
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            this.GetComponent<Camera>().orthographicSize += ZoomSpeed;
        }
        */

        // Zoom bounding
        if (transform.position.y > zoomDistanceMax || transform.position.y < zoomDistanceMin)
        {
            Debug.Log("zoom out of bounds");
            resetPos = new Vector3(transform.position.x, zoomDistanceMax, transform.position.z);
            resetRot = Quaternion.Euler(50f, 0, 0);
            transform.position = Vector3.Lerp(transform.position, resetPos, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, resetRot, Time.deltaTime);
        }

        // TODO: reclamp min and max distances

        // Zoom - perspective
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            // TODO should zoom in facing direction
            //CurrentZoom -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
            //CurrentZoom = Mathf.Clamp(CurrentZoom, cameraDistanceMin, cameraDistanceMax);
            //transform.position = new Vector3(transform.position.x, CurrentZoom, transform.position.z);
            //transform.eulerAngles.x -= (transform.eulerAngles.x - (InitRotation.x + CurrentZoom * ZoomRotation)) * 0.1;
            //CurrentZoom = Mathf.Clamp(CurrentZoom, cameraDistanceMin, cameraDistanceMax);


            transform.position += transform.forward * ZoomSpeed;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            transform.position -= transform.forward * ZoomSpeed;
        }

    }

    public void incView()
    {
        if (currentView < views.Length)
        {
            currentView++;
            lerpCam = true;
            startTime = Time.time;
            camJourneyLength = Vector3.Distance(transform.position, views[currentView].position);
        }

    }

    public void decView()
    {
        if (currentView > 0)
        {
            currentView--;
            lerpCam = true;
            startTime = Time.time;
            camJourneyLength = Vector3.Distance(transform.position, views[currentView].position);
        }
    }

    public void ToggleCameraBounding()
    {
        doCameraBounding = !doCameraBounding;
    }
}
