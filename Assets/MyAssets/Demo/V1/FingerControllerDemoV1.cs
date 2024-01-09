using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerControllerDemoV1 : MonoBehaviour
{
    private ManagerDemoV1 _manager;
    private AUTDControllerDemoV1 _autdController;

    //Manager
    Vector3 planeYZ;
    float fingerRadius;

    float delay;
    float halfCurveLength;
    float curveRadius;
    float curveHeightDepth;
    bool flagVideoConvex;

    //AUTDController
    float fingerPosX;
    float circlePosX;

    //Finger Controller
    Vector3 posFingerCurve;
    Vector3 posFingerReal;

    public GameObject FingerSupposed = null;
    public GameObject FingerReal = null;


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerDemoV1>();
        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDControllerDemoV1>();

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveRadius = _manager.CurveRadius;
        curveHeightDepth = _manager.CurveHeightDepth;
        flagVideoConvex = _manager.FlagConvex;

        fingerPosX = _autdController.FingerPosX;
        circlePosX = _autdController.CirclePosX;

        if (flagVideoConvex)
        {
            DisplayConvexFingers();
        }
        else
        {
            DisplayConveneFingers();
        }

    }

    void DisplayConvexFingers()
    {
        posFingerCurve = FingerSupposed.transform.position;
        posFingerReal = FingerReal.transform.position;

        posFingerCurve.x = fingerPosX;
        posFingerReal.x = fingerPosX;

        if (Mathf.Abs(fingerPosX - circlePosX) <= halfCurveLength / delay)   //When Curved Surface
        {
            posFingerCurve.y = planeYZ.y - curveRadius + curveHeightDepth + Mathf.Sqrt(Mathf.Pow(curveRadius + fingerRadius, 2) - Mathf.Pow(fingerPosX - circlePosX, 2));
        }
        else //When Plane
        {
            posFingerCurve.y = planeYZ.y + fingerRadius; //YZ.y + radius of the figner
        }

        FingerSupposed.transform.position = posFingerCurve;
        FingerReal.transform.position = posFingerReal;
    }

    void DisplayConveneFingers()
    {
        posFingerCurve = FingerSupposed.transform.position;
        posFingerReal = FingerReal.transform.position;

        posFingerCurve.x = fingerPosX;
        posFingerReal.x = fingerPosX;

        if (Mathf.Abs(fingerPosX - circlePosX) <= halfCurveLength / delay)   //When Curved Surface
        {
            if (curveRadius >= fingerRadius)
            {
                if (Mathf.Pow(curveRadius - fingerRadius, 2) > Mathf.Pow(fingerPosX - circlePosX, 2))//prevent error
                {
                    posFingerCurve.y = planeYZ.y + curveRadius - curveHeightDepth - Mathf.Sqrt(Mathf.Pow(curveRadius - fingerRadius, 2) - Mathf.Pow(fingerPosX - circlePosX, 2));
                }
                else
                {
                    posFingerCurve.y = planeYZ.y + curveRadius - curveHeightDepth;
                }

            }
            else
            {
                posFingerCurve.y = planeYZ.y + curveRadius - curveHeightDepth + Mathf.Sqrt(Mathf.Pow(fingerRadius - curveRadius, 2) - Mathf.Pow(fingerPosX - circlePosX, 2));
            }

        }
        else //When Plane
        {
            posFingerCurve.y = planeYZ.y + fingerRadius; //YZ.y + radius of the figner
        }

        FingerSupposed.transform.position = posFingerCurve;
        FingerReal.transform.position = posFingerReal;
    }
}