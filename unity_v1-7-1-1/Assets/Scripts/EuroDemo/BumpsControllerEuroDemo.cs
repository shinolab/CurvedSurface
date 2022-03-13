using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class BumpsControllerEuroDemo : MonoBehaviour
{
    private ManagerEuroDemo _manager;

    public GameObject Bump = null;


    float fingerRadius;
    Vector3 planeYZ;


    //For CalcBump

    float delay;
    float previousDelay = 0f;

    float halfCurveLength;
    float previousHalfCurveLength;

    float curveHight;

    float bumpRadius;
    public float BumpRadius
    {
        get { return this.bumpRadius; }
        private set { this.bumpRadius = value; }
    }
    float bumpDiameter;


    //For DetectBump

    float bumpPosY;
    public float BumpPosY
    {
        get { return this.bumpPosY; }
        private set { this.bumpPosY = value; }
    }

    Vector3 bumpPos;

    //For Send Information to AUTD Controller
    float bumpPosX = 0f;
    public float BumpPosX
    {
        get { return this.bumpPosX; }
        private set { this.bumpPosX = value; }
    }

    int bumpNumber = 0;
    public int BumpNumber
    {
        get { return this.bumpNumber; }
        private set { this.bumpNumber = value; }
    }
    float eachBumpDitance = 0f;
    
    public Text textDistance;

    float distance = 0f;
    public float Distance
    {
        get { return this.distance; }
        private set { this.distance = value; }
    }
    int distanceMM = 0; //mm
    

    bool flagLM = true;   //true -> Strength, false -> Position
    bool previousFlagLM = true;   //true -> Strength, red, false -> Position, blue

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo>();

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

        flagLM = _manager.FlagLM;

        //BumpPosX = Bump.transform.position.x;

        if ((previousDelay != delay) || (previousHalfCurveLength != halfCurveLength) || (previousFlagLM != flagLM))
        {
            CalcBump();
            DetectCenterBump();

            //Debug.Log("Bump, delay: " + delay.ToString("F4")+"halfExploreLength: " + halfExploreLength.ToString("F4"));
            previousDelay = delay;
            previousHalfCurveLength = halfCurveLength;
            previousFlagLM = flagLM;
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || (UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))) //Leave
        {
            distance += 0.004f;
            distanceMM += 4;

            PlaceBumps();

            textDistance.text = "Distance: " + distanceMM + "mm";
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || (UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))) //Close
        {
            if (distance > 0)
            {
                distance -= 0.004f;
                distanceMM -= 4;

                PlaceBumps();

                textDistance.text = "Distance: " + distanceMM + "mm";
            }
            else
            {
                Debug.Log("Already 0 mm");
            }

        }

    }

    private void CalcBump()
    {

        bumpRadius = delay / (1 - delay) * fingerRadius;
        bumpDiameter = bumpRadius * 2;

        if (Mathf.Pow(bumpRadius, 2) < Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            bumpPosY = planeYZ.y;
        }
        else
        {
            bumpPosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(bumpRadius, 2) - Mathf.Pow(halfCurveLength, 2));
        }
        curveHight = bumpRadius - planeYZ.y + bumpPosY;

        Debug.Log("Bump, delay: " + delay.ToString("F4") + "halfCurveLength" + halfCurveLength.ToString("F4") + "planeYZ.y" + planeYZ.y.ToString("F4") + "bumpRadius" + bumpRadius.ToString("F4") + ", bumpPosY" + bumpPosY.ToString("F4") + ", curvedHight" + curveHight.ToString("F8"));

    }


    private void DetectCenterBump()
    {

        if(previousFlagLM != flagLM)        //Change color
        {
            if (flagLM) //Strength && color =red
            {
                Bump.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
            }
            else //Position && color = blue
            {
                Bump.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red\
            }
        }

        Bump.transform.localScale = new Vector3(bumpDiameter, 0.2f, bumpDiameter);

        bumpPos = Bump.transform.position;
        bumpPos.y = bumpPosY;
        Bump.transform.position = bumpPos;
    }

    private void PlaceBumps()
    {
        int i;
        //Delete Objects
        if (bumpNumber > 0)
        {
            for (i = 0; i < bumpNumber; i++)
            {
                Destroy(GameObject.Find("right" + i));
                Destroy(GameObject.Find("left" + i));
            }          
        }

        //Place
        bumpNumber = (600 / distanceMM);

        for(i = 0; i < bumpNumber; i++)
        {
            eachBumpDitance = i * distance;

            GameObject rightBumps = Instantiate(Bump, new Vector3(eachBumpDitance, bumpPosY, 0.16f), Quaternion.Euler(90f, 0.0f, 0f)); //Right
            GameObject leftBumps = Instantiate(Bump, new Vector3(-eachBumpDitance, bumpPosY, 0.16f), Quaternion.Euler(90f, 0.0f, 0f)); //Left

            rightBumps.name = "right"+i;
            leftBumps.name = "left" + i;
        }

        Debug.Log("bumpNumber; " + bumpNumber + ", eachBumpDitance; " + eachBumpDitance.ToString("F4"));
    }



}
