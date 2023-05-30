using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerController2CPS : MonoBehaviour
{
    private Manager2CPS _manager;
    private CurvesController2CPS _curvesController;
    private AUTDController2CPS _autdController;

    Vector3 planeYZ;
    //float fingerRadius;

    float fingerPosX;
    float focusPosX;

    float delay;
    float curvesRadius;
    float curvesDis;
    float circlesPosY;
    float halfCurveLength;

    public GameObject FingerReal = null;
    public GameObject FingerCurve = null;
    Vector3 posFingerCurve;
    Vector3 posFingerReal;

    float centorCirclesPosX = 0f;
    float circlePosX;


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2CPS>();
        _curvesController = GameObject.Find("CurvesController").GetComponent<CurvesController2CPS>();
        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDController2CPS>();

        planeYZ = _manager.PlaneYZ;
        //fingerRadius = _manager.FingerRadius;

        centorCirclesPosX = 0f; //Determine centor
    }

    // Update is called once per frame
    void Update()
    {
        //halfExploreLength = _manager.HalfExploreLength;

        delay = _manager.Delay;
        curvesRadius = _manager.CurvesRadius;
        curvesDis = _manager.CurvesDis;
        halfCurveLength = _manager.HalfCurveLength;
        circlesPosY = _manager.CirclesPosY;
        fingerPosX = _autdController.FingerPosX;
        focusPosX = _autdController.FocusPosX;

        DetermineSide();
        DisplayFingers();
    }


    private void DetermineSide()
    {
        //leftCirclePosX = _curvesController.LeftCurvePosX;
        //rightCirclePosX = _curvesController.RightCurvePosX;

        if (fingerPosX <= centorCirclesPosX)
        {
            circlePosX = -curvesDis / 2;
        }
        else
        {
            circlePosX = curvesDis / 2;
        }
    }


    private void DisplayFingers()
    {
        //Finger Real
        posFingerReal = FingerReal.transform.position;
        posFingerReal.x = fingerPosX;
        FingerReal.transform.position = posFingerReal;

        //Figner Curved Surface
        posFingerCurve = FingerCurve.transform.position;
        posFingerCurve.x = fingerPosX;

        if ((Mathf.Abs(fingerPosX - circlePosX) <= halfCurveLength/delay))   //When Curved Surface
        {
            if (Mathf.Pow(curvesRadius, 2) - Mathf.Pow(focusPosX - circlePosX, 2) <= 0)
            {
                posFingerCurve.y = circlesPosY;
            }
            else
            {
                posFingerCurve.y = circlesPosY-Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(focusPosX - circlePosX, 2));
            }
            //Debug.Log("Curved, " + "posFingerCurve.y: " + posFingerCurve.y.ToString("F4"));
        }
        else //When Plane
        {
            posFingerCurve.y = planeYZ.y; //YZ.y + radius of the figner
            //Debug.Log("Plane, " + "posFingerCurve.y: " + posFingerCurve.y.ToString("F4"));
        }
        FingerCurve.transform.position = posFingerCurve;

    }

}
