using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class ConvexController1VLC : MonoBehaviour
{
    private Manager1VLC _manager;

    float delay;
    float previousDelay = 0f;

    float halfCurveLength;
    float previousCurveRadius;

    Vector3 planeYZ;



    float curveHeight;
    float curveRadius;

    float circlePosY;
    public float CirclePosY
    {
        get { return this.circlePosY; }
        private set { this.circlePosY = value; }
    }



    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1VLC>();

        planeYZ = _manager.PlaneYZ;

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveHeight = _manager.CurveHeight;
        curveRadius = _manager.CurveRadius;
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveHeight = _manager.CurveHeight;
        curveRadius = _manager.CurveRadius;

        if ((previousDelay != delay) || (previousCurveRadius != curveRadius))
        {
            CalcCircle();
            DetectConvex();

            previousDelay = delay;
            previousCurveRadius = curveRadius;
        }

    }

    private void CalcCircle()
    {

        if (Mathf.Pow(curveRadius, 2) < Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            circlePosY = planeYZ.y;
        }
        else
        {
            circlePosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2));
        }

        //Debug.Log("Circle, delay: " + delay.ToString("F4") + "halfCurveLength" + halfCurveLength.ToString("F4") + "planeYZ.y" + planeYZ.y.ToString("F4") + "curveRadius" + curveRadius.ToString("F4") + ", circlePosY" + circlePosY.ToString("F4") + ", curveHight" + curveHeight.ToString("F8"));

    }

    Vector3 pos;
    private void DetectConvex()
    {
        pos = this.transform.position;
        pos.y = circlePosY;
        this.transform.position = pos;

        this.transform.localScale = new Vector3(curveRadius * 2, 0.19f, curveRadius * 2);

        //Debug.Log("circlePosY: " +  circlePosY.ToString("F4"));

    }




}
