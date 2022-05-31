using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class BumpsEuroDemo2 : MonoBehaviour
{
    private ManagerEuroDemo2 _manager;

    public GameObject LeftBump = null;
    public GameObject RightBump = null;


    float fingerRadius;
    Vector3 planeYZ;


    //For CalcBump
    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }
    }

    float halfCurveLength;
    public float HalfCurveLength
    {
        get { return this.halfCurveLength; }
        private set { this.halfCurveLength = value; }
    }
    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }

    float curveHight;

    float bumpRadius;

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

    float distance = 0;

    bool flagMode = true;   //true -> Adjusting, false -> Presenting

    bool flagLM = true;   //true -> Strength, false -> Position


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo2>();

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;
        curveHight = _manager.CurveHight;
        bumpRadius = _manager.BumpRadius;
        distance = _manager.Distance;
    }

    // Update is called once per frame
    void Update()
    {
        //delay = _manager.Delay;
        //halfCurveLength = _manager.HalfCurveLength;

        flagMode = _manager.FlagMode;
        flagLM = _manager.FlagLM;

        bumpRadius = _manager.BumpRadius;
        distance = _manager.Distance;

        //BumpPosX = Bump.transform.position.x;

        //If Adjusting and( Bigger or Smaller or Further or Closer)
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5) || UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2) || UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3) || UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1)))
        {
            //CalcCurvedLength();
            //CalcHalfExploreLength();
            CalcBump();
            DetectCenterBump();

            if ((distance > 0) && (bumpRadius > 0)) PlaceBumps();
        }



    }


    private void CalcBump() //Calc bumpPosY
    {
        //Calc halfCurveLength, halfExploreLength
        if (2 * bumpRadius * curveHight > Mathf.Pow(curveHight, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * bumpRadius * curveHight - Mathf.Pow(curveHight, 2));
        }
        else
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = bumpRadius / (bumpRadius + fingerRadius);
        halfExploreLength = halfCurveLength / delay;


        //Calc bumpPosY
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

        if (flagLM) //Strength && color =red
        {
            LeftBump.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
            RightBump.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
        }
        else //Position && color = blue
        {
            LeftBump.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red
            RightBump.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //red
        }

        LeftBump.transform.localScale = new Vector3(bumpRadius * 2, 0.14f, bumpRadius * 2);
        RightBump.transform.localScale = new Vector3(bumpRadius * 2, 0.14f, bumpRadius * 2);

        bumpPos = LeftBump.transform.position;
        bumpPos.y = bumpPosY;
        LeftBump.transform.position = bumpPos;

        bumpPos = RightBump.transform.position;
        bumpPos.y = bumpPosY;
        RightBump.transform.position = bumpPos;
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
        bumpNumber = (int)(0.173f / distance) + 1;

        for (i = 0; i < bumpNumber; i++)
        {
            eachBumpDitance = i * distance;

            GameObject rightBumps = Instantiate(RightBump, new Vector3(RightBump.transform.position.x + eachBumpDitance, bumpPosY, 0.06f), Quaternion.Euler(90f, 0.0f, 0f)); //Right
            GameObject leftBumps = Instantiate(LeftBump, new Vector3(LeftBump.transform.position.x - eachBumpDitance, bumpPosY, 0.06f), Quaternion.Euler(90f, 0.0f, 0f)); //Left

            rightBumps.name = "right" + i;
            leftBumps.name = "left" + i;
        }

        //Debug.Log("bumpNumber; " + bumpNumber + ", eachBumpDitance; " + eachBumpDitance.ToString("F4"));
    }



}
