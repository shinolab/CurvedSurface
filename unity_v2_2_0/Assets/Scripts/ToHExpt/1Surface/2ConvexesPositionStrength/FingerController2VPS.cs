using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerController2VPS : MonoBehaviour
{
    private Manager2VPS _manager;
    private CurvesController2VPS _curvesController;
    private AUTDController2VPS _autdController;

    float halfExploreLength;

    Vector3 planeYZ;
    //float fingerRadius;

    float fingerPosX;
    float focusPosX;

    float curvesRadius;
    float circlesPosY;

    public GameObject FingerReal = null;
    public GameObject FingerCurve = null;
    Vector3 posFingerCurve;
    Vector3 posFingerReal;

    float circlePosX;
    float leftCirclePosX;
    float rightCirclePosX;
    float centorCirclesPosX = 0f;


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2VPS>();
        _curvesController = GameObject.Find("CurvesController").GetComponent<CurvesController2VPS>();
        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDController2VPS>();

        planeYZ = _manager.PlaneYZ;
        //fingerRadius = _manager.FingerRadius;

        centorCirclesPosX = (leftCirclePosX + rightCirclePosX) / 2; //Determine centor
    }

    // Update is called once per frame
    void Update()
    {
        halfExploreLength = _manager.HalfExploreLength;

        curvesRadius = _manager.CurvesRadius;
        circlesPosY = _curvesController.CirclesPosY;
        fingerPosX = _autdController.FingerPosX;
        focusPosX = _autdController.FocusPosX;

        DetermineSide();
        DisplayFingers();
    }


    private void DetermineSide()
    {
        leftCirclePosX = _curvesController.LeftCurvePosX;
        rightCirclePosX = _curvesController.RightCurvePosX;

        if (fingerPosX <= centorCirclesPosX)
        {
            circlePosX = leftCirclePosX;
        }
        else
        {
            circlePosX = rightCirclePosX;
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

        if ((Mathf.Abs(fingerPosX - circlePosX) <= halfExploreLength))   //When Curved Surface
        {
            if (Mathf.Pow(curvesRadius, 2) - Mathf.Pow(focusPosX - circlePosX, 2) <= 0)
            {
                posFingerCurve.y = circlesPosY;
            }
            else
            {
                posFingerCurve.y = Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(focusPosX - circlePosX, 2)) + circlesPosY;
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
