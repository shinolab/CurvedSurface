using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerController1VTE : MonoBehaviour
{
    private Manager1VTE _manager;
    private ConvexController1VTE _convexController;
    private AUTDController1VTE _autdController;

    float delay;
    float halfCurveLength;

    Vector3 planeYZ;

    float fingerPosX;

    float curveRadius;
    float circlePosY;

    public GameObject FingerReal = null;
    public GameObject FingerCircle = null;


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1VTE>();
        _convexController = GameObject.Find("Convex").GetComponent<ConvexController1VTE>();
        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDController1VTE>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        curveRadius = _manager.CurveRadius;

        circlePosY = _convexController.CirclePosY;

        fingerPosX = _autdController.FingerPosX;

        DetectFingers();
    }

    Vector3 posFingerCircle;
    Vector3 posFingerReal;

    private void DetectFingers()
    {
        posFingerCircle = FingerCircle.transform.position;
        posFingerReal = FingerReal.transform.position;

        posFingerCircle.x = fingerPosX;
        posFingerReal.x = fingerPosX;

        if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength)   //When Curved Surface
        {
            posFingerCircle.y = circlePosY + Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(fingerPosX * delay, 2));
            //posFingerCircle.y = circlePosY+Mathf.Sqrt(Mathf.Pow(curveRadius + fingerRadius, 2) - Mathf.Pow(fingerPosX, 2));

            //Debug.Log("circleRadius: " + circleRadius.ToString("F4")+ "fingerPosX: " + fingerPosX.ToString("F4") + "circlePosY: " + circlePosY.ToString("F4") + "posFinger.y: " + posFinger.y.ToString("F4"));
            //Debug.Log("Curved, " + "posFinger.y: " + posFinger.y.ToString("F4"));
        }
        else //When Plane
        {
            posFingerCircle.y = planeYZ.y;
            //posFingerCircle.y = planeYZ.y+fingerRadius; //YZ.y + radius of the figner
            //Debug.Log("Plane, " +"posFinger.y: " + posFinger.y.ToString("F4"));
        }

        FingerCircle.transform.position = posFingerCircle;
        FingerReal.transform.position = posFingerReal;

    }

}
