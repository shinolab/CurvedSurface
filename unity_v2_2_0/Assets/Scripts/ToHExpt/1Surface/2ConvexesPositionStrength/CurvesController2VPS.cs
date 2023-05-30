using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class CurvesController2VPS : MonoBehaviour
{
    private Manager2VPS _manager;

    public GameObject LeftCurve = null;
    public GameObject RightCurve = null;


    Vector3 planeYZ;


    //For CalcCurves
    float halfCurveLength;

    float curvesRadius;
    float previousCurvesRadius;

    float curvesDis;
    float previousCurvesDis;

    //For DetectEachCurve

    float circlesPosY;
    public float CirclesPosY
    {
        get { return this.circlesPosY; }
        private set { this.circlesPosY = value; }
    }

    Vector3 leftCurvePos;
    Vector3 rightCurvePos;


    //For Tell the CurvePosX of left and right
    public float LeftCurvePosX
    {
        get { return this.leftCurvePos.x; }
        private set { this.leftCurvePos.x = value; }
    }
    public float RightCurvePosX
    {
        get { return this.rightCurvePos.x; }
        private set { this.rightCurvePos.x = value; }
    }

    //Change Colors
    int flagLM;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2VPS>();

        planeYZ = _manager.PlaneYZ;

        halfCurveLength = _manager.HalfCurveLength;
        curvesRadius = _manager.CurvesRadius;
        curvesDis =_manager.CurvesDis;

        DetectEachCurve();
        DetectDistance();
    }

    // Update is called once per frame
    void Update()
    {

        ///Change Colors
        flagLM = _manager.FlagLM;
        switch (flagLM)
        {
            case 0: //Position
                LeftCurve.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //pink
                RightCurve.GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //pink
                break;
            case 1: //Strength
                LeftCurve.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
                RightCurve.GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
                break;
            case 2: //Position+Strength
                LeftCurve.GetComponent<Renderer>().material.color = new Color32(205, 134, 255, 255); //purple
                RightCurve.GetComponent<Renderer>().material.color = new Color32(205, 134, 255, 255); //purple
                break;
        }


        ///DetectEachCurve by Radius
        curvesRadius = _manager.CurvesRadius;      
        if (previousCurvesRadius != curvesRadius)
        {
            halfCurveLength = _manager.HalfCurveLength;
            DetectEachCurve();
            previousCurvesRadius = curvesRadius;
        }

        ///DetectCurvesDis
        curvesDis =_manager.CurvesDis;
        if (previousCurvesDis != curvesDis)
        {
            DetectDistance();
            previousCurvesDis = curvesDis;
        }

    }

    private void DetectEachCurve()
    {
        ///Calculation
        if (Mathf.Pow(curvesRadius, 2) < Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Error
        {
            circlesPosY = planeYZ.y;
        }
        else
        {
            circlesPosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(halfCurveLength, 2));
        }

        ///Detenction
        LeftCurve.transform.localScale = new Vector3(curvesRadius * 2, 0.19f, curvesRadius * 2);
        RightCurve.transform.localScale = new Vector3(curvesRadius * 2, 0.19f, curvesRadius * 2);

        leftCurvePos = LeftCurve.transform.position;
        leftCurvePos.y = circlesPosY;
        LeftCurve.transform.position = leftCurvePos;

        rightCurvePos = RightCurve.transform.position;
        rightCurvePos.y = circlesPosY;
        RightCurve.transform.position = rightCurvePos;
    }

    private void DetectDistance()
    {
        leftCurvePos = LeftCurve.transform.position;
        leftCurvePos.x = -curvesDis/2;
        LeftCurve.transform.position = leftCurvePos;

        rightCurvePos = RightCurve.transform.position;
        rightCurvePos.x = curvesDis / 2;
        RightCurve.transform.position = rightCurvePos;
    }
}