using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class ConvexController1VTE : MonoBehaviour
{
    private Manager1VTE _manager;

    float delay;
    float previousDelay = 0f;

    float halfCurveLength;
    float previousCurveRadius;

    Vector3 planeYZ;

    float curveRadius;

    float circlePosY;
    public float CirclePosY
    {
        get { return this.circlePosY; }
        private set { this.circlePosY = value; }
    }

    Vector3 pos;

    //Change Colors
    int flagLM=0;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1VTE>();
        planeYZ = _manager.PlaneYZ;

    }

    // Update is called once per frame
    void Update()
    {
        ///Change Colors
        flagLM = _manager.FlagLM;
        ChangeColors();

        delay = _manager.Delay;        
        curveRadius = _manager.CurveRadius;
        if ((previousDelay != delay) || (previousCurveRadius != curveRadius))
        {
            halfCurveLength = _manager.HalfCurveLength;
            DetectConvex();

            previousDelay = delay;
            previousCurveRadius = curveRadius;
        }

    }


    private void ChangeColors()
    {
        switch (flagLM)
        {
            case 0: //Position
                GetComponent<Renderer>().material.color = new Color32(255, 162, 162, 255); //pink
                break;
            case 1: //Strength
                GetComponent<Renderer>().material.color = new Color32(162, 190, 255, 255); //blue
                break;
            case 2: //Position+Strength
                GetComponent<Renderer>().material.color = new Color32(205, 134, 255, 255); //purple
                break;
        }
    }
    
    private void DetectConvex()
    {
        if (Mathf.Pow(curveRadius, 2) < Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            circlePosY = planeYZ.y;
        }
        else
        {
            circlePosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2));
        }

        pos = this.transform.position;
        pos.y = circlePosY;
        this.transform.position = pos;

        this.transform.localScale = new Vector3(curveRadius * 2, 0.19f, curveRadius * 2);
    }




}
