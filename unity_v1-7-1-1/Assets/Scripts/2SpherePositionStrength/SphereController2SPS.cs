using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class SphereController2SPS : MonoBehaviour
{
    private Manager2SPS _manager;

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


    //For Move Sphere

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

    float closeAcceleration = 0.05f;
    float leaveAcceleration = 0.01f;
    Vector3 closeVelocity = Vector3.zero;
    Vector3 leaveVelocity = Vector3.zero;


    float managerDis;
    float previousManagerDis = 0f;
    bool flagInput = false;

    public Text textDistance;
    int distanceMM = 0; //mm

    bool flagLM = true;   //true -> Strength, false -> Position
    bool colorSphere = true;   //true -> Strength, red, false -> Position, blue

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2SPS>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;

        fingerRadius = _manager.FingerRadius;

        leftSpherePosX = LeftSphere.transform.position.x;
        rightSpherePosX = RightSphere.transform.position.x;

        textDistance = textDistance.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        flagLM = _manager.FlagLM;

        //moveSpherePosX = _manager.MoveSpherePosX;

        //distanceMM = (int)(rightSpherePosX *1000); //mm
        distanceMM = (int)((rightSpherePosX - leftSpherePosX) * 1000); //mm
        textDistance.text = "Distance: " + distanceMM.ToString() + "mm";

        if (flagLM && !(colorSphere)) //Strength && color =red
        {
            LeftSphere.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
            RightSphere.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
             
            colorSphere = true;  //blue
        }
        else if(!(flagLM) && colorSphere)  //Position && color = blue
        {
            LeftSphere.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red
            RightSphere.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red

            colorSphere = false;  //red
        }

        if ((previousDelay != delay) || (previousHalfCurveLength != halfCurveLength))
        {
            CalcSphere();
            DetectSphre();

            //Debug.Log("Sphere, delay: " + delay.ToString("F4")+"halfExploreLength: " + halfExploreLength.ToString("F4"));
            previousDelay = delay;
            previousHalfCurveLength = halfCurveLength;
        }

        flagInput = _manager.FlagInput;
        //managerDis = _manager.ManagerDis;
        //if ((previousManagerDis != managerDis))
        //{
        //    rightSpherePosX = leftSpherePosX+managerDis;
        //    RightSphere.transform.position = new Vector3(rightSpherePosX, SpherePosY, planeYZ.z);

        //    Debug.Log("Distance: " + distance.ToString());
        //    previousManagerDis = managerDis;
        //}

        if ((!flagInput) && (UnityEngine.Input.GetKey(KeyCode.Alpha4) || UnityEngine.Input.GetKey(KeyCode.Keypad4) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)))
        {
            if (leaveVelocity.x != 0f)
            {
                leaveVelocity = Vector3.zero;
                //Debug.Log("RightSpherePosX: " + rightSpherePosX.ToString());
            }

            closeVelocity.x += closeAcceleration * Time.deltaTime;
            RightSphere.transform.position -= closeVelocity * Time.deltaTime;
            LeftSphere.transform.position += closeVelocity * Time.deltaTime;

            if (rightSpherePosX >= LeftSpherePosX)
            {
                rightSpherePosX = RightSphere.transform.position.x;
                leftSpherePosX = LeftSphere.transform.position.x;
                distanceMM = (int)((rightSpherePosX - leftSpherePosX) * 1000); //mm
                textDistance.text = "Distance: " + distanceMM.ToString() + "mm";
                Debug.Log("Distance: " + distanceMM.ToString() + "mm");
            }
            else
            {
                closeVelocity = Vector3.zero;
                Debug.Log("Maxclosed, Distance: " + (rightSpherePosX - leftSpherePosX).ToString());
                distanceMM = (int)((rightSpherePosX - leftSpherePosX) * 1000); //mm
                //distanceMM = (int)(rightSpherePosX - leftSpherePosX) * 1000;
                textDistance.text = "Distance: " + distanceMM.ToString() + "mm";
                Debug.Log("Distance: " + distanceMM.ToString() + "mm");
            }


        }

        if ((!flagInput) && (UnityEngine.Input.GetKey(KeyCode.Alpha6) || UnityEngine.Input.GetKey(KeyCode.Keypad6) || UnityEngine.Input.GetKey(KeyCode.RightArrow)))
        {
            if (closeVelocity.x != 0f)
            {
                closeVelocity = Vector3.zero;
                Debug.Log("Distance: " + (rightSpherePosX - leftSpherePosX).ToString());
                distanceMM = (int)((rightSpherePosX - leftSpherePosX) * 1000); //mm
                textDistance.text = "Distance: " + distanceMM.ToString() + "mm";
                Debug.Log("Distance: " + distanceMM.ToString() + "mm");
            }

            leaveVelocity.x += leaveAcceleration * Time.deltaTime;
            RightSphere.transform.position += leaveVelocity * Time.deltaTime;
            LeftSphere.transform.position -= leaveVelocity * Time.deltaTime;

            rightSpherePosX = RightSphere.transform.position.x;
            leftSpherePosX = LeftSphere.transform.position.x;
            distanceMM = (int)((rightSpherePosX - leftSpherePosX) * 1000); //mm
            textDistance.text = "Distance: " + distanceMM.ToString() + "mm";
            Debug.Log("Distance: " + distanceMM.ToString() + "mm");
        }



        if ((!flagInput) && (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5)))
        {
            distanceMM = (int)((rightSpherePosX - leftSpherePosX) * 1000); //mm
            Debug.Log("Distance: " + (rightSpherePosX - leftSpherePosX).ToString() + "distance: " + distanceMM.ToString());

            textDistance.text = "Distance: " + distanceMM.ToString() + "mm";
            Debug.Log("Distance: " + distanceMM.ToString() + "mm");

            if (leaveVelocity.x != 0f)
            {
                leaveVelocity = Vector3.zero;
            }
            if (closeVelocity.x != 0f)
            {
                closeVelocity = Vector3.zero;
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

        //LeftSphere.transform.localScale = new Vector3(sphereDiameter, sphereDiameter, sphereDiameter);
        //RightSphere.transform.localScale = new Vector3(sphereDiameter, sphereDiameter, sphereDiameter);

        leftSpherePos = LeftSphere.transform.position;
        leftSpherePos.y = spherePosY;
        LeftSphere.transform.position = leftSpherePos;

        rightSpherePos = RightSphere.transform.position;
        rightSpherePos.y = spherePosY;
        RightSphere.transform.position = rightSpherePos;
    }



}
