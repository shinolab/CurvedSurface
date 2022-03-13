using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class SphereControllerEuroExpt : MonoBehaviour
{
    private ManagerEuroExpt _manager;

    public GameObject LeftSphere = null;
    public GameObject RightSphere = null;


    float fingerRadius;
    Vector3 planeYZ;


    //For CalcSphere

    float delay;
    float previousDelay = 0f;

    float halfCurveLength;
    float previousHalfCurveLength;

    float curveHight;

    float sphereRadius;
    public float SphereRadius
    {
        get { return this.sphereRadius; }
        private set { this.sphereRadius = value; }
    }
    float sphereDiameter;


    //For DetectSphere

    float spherePosY;
    public float SpherePosY
    {
        get { return this.spherePosY; }
        private set { this.spherePosY = value; }
    }

    Vector3 leftSpherePos;
    Vector3 rightSpherePos;

    //For Send Information to AUTD Controller
    float leftSpherePosX = 0f;
    public float LeftSpherePosX
    {
        get { return this.leftSpherePosX; }
        private set { this.leftSpherePosX = value; }
    }
    float rightSpherePosX = 0.01f;
    public float RightSpherePosX
    {
        get { return this.rightSpherePosX; }
        private set { this.rightSpherePosX = value; }
    }


    public Text textDistance;

    float distance = 0f;
    int distanceMM = 0; //mm
    public int DistanceMM
    {
        get { return this.distanceMM; }
        private set { this.distanceMM = value; }
    }


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroExpt>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;

        fingerRadius = _manager.FingerRadius;

        textDistance = textDistance.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        leftSpherePosX = LeftSphere.transform.position.x;
        rightSpherePosX = RightSphere.transform.position.x;

        if ((previousDelay != delay) || (previousHalfCurveLength != halfCurveLength))
        {
            CalcSphere();
            DetectSphre();

            //Debug.Log("Sphere, delay: " + delay.ToString("F4")+"halfExploreLength: " + halfExploreLength.ToString("F4"));
            previousDelay = delay;
            previousHalfCurveLength = halfCurveLength;
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))) //Leave
        {
            distance += 0.002f;
            distanceMM += 2;

            RightSphere.transform.position += new Vector3(0.001f, 0, 0);
            LeftSphere.transform.position -= new Vector3(0.001f, 0, 0);

            textDistance.text = "Distance: " + distanceMM + "mm";
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))) //Close
        {
            if (distance > 0)
            {
                distance -= 0.002f;
                distanceMM -= 2;

                RightSphere.transform.position -= new Vector3(0.001f, 0, 0);
                LeftSphere.transform.position += new Vector3(0.001f, 0, 0);

                textDistance.text = "Distance: " + distanceMM + "mm";
            }
            else
            {
                Debug.Log("Already 0 mm");
            }

        }

    }

    private void CalcSphere()
    {

        sphereRadius = delay / (1 - delay) * fingerRadius;
        sphereDiameter = sphereRadius * 2;

        if (Mathf.Pow(sphereRadius, 2) < Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            spherePosY = planeYZ.y;
        }
        else
        {
            spherePosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(halfCurveLength, 2));
        }
        curveHight = sphereRadius - planeYZ.y + spherePosY;

        Debug.Log("Sphere, delay: " + delay.ToString("F4") + "halfCurveLength" + halfCurveLength.ToString("F4") + "planeYZ.y" + planeYZ.y.ToString("F4") + "sphereRadius" + sphereRadius.ToString("F4") + ", spherePosY" + spherePosY.ToString("F4") + ", curvedHight" + curveHight.ToString("F8"));

    }


    private void DetectSphre()
    {
        LeftSphere.transform.localScale = new Vector3(sphereDiameter, 0.2f, sphereDiameter);
        RightSphere.transform.localScale = new Vector3(sphereDiameter, 0.2f, sphereDiameter);

        leftSpherePos = LeftSphere.transform.position;
        leftSpherePos.y = spherePosY;
        LeftSphere.transform.position = leftSpherePos;

        rightSpherePos = RightSphere.transform.position;
        rightSpherePos.y = spherePosY;
        RightSphere.transform.position = rightSpherePos;
    }



}
