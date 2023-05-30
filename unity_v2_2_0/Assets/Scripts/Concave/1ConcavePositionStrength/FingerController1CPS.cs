using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerController1CPS : MonoBehaviour
{
    private Manager1CPS _manager;
    private ConcaveController1CPS _concaveController;
    private AUTDController1CPS _autdController;

    float delay;
    float curveLength;

    Vector3 planeYZ;
    float fingerRadius;

    float fingerPosX;

    float curveRadius;
    float deltaCirclePosY; //planeYZ.y-circlePosY

    public GameObject FingerCircle = null;
    public GameObject FingerReal = null;


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1CPS>();

        _concaveController = GameObject.Find("ConcaveBox").GetComponent<ConcaveController1CPS>();

        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDController1CPS>();

        delay = _manager.Delay;
        curveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        curveLength = _manager.HalfCurveLength;

        curveRadius = _manager.CurveRadius;

        deltaCirclePosY = _concaveController.DeltaCirclePosY;

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

        if (Mathf.Abs(fingerPosX * delay) <= curveLength)   //When Curved Surface
        {
            //posFingerCircle.y = deltaCirclePosY+planeYZ.y - Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(posFingerCircle.x * delay, 2));
            posFingerCircle.y = deltaCirclePosY + planeYZ.y - Mathf.Sqrt(Mathf.Pow(curveRadius - fingerRadius, 2) - Mathf.Pow(fingerPosX, 2));

            //Debug.Log("Curved, " + "posFinger.y: " + posFinger.y.ToString("F4"));
        }
        else //When Plane
        {
            posFingerCircle.y = planeYZ.y+fingerRadius; //YZ.y + radius of the figner
            //Debug.Log("Plane, " +"posFinger.y: " + posFinger.y.ToString("F4"));
        }

        FingerCircle.transform.position = posFingerCircle;
        FingerReal.transform.position = posFingerReal;

    }

}
