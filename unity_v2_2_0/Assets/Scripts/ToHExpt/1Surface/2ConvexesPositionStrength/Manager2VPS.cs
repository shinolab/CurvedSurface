using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;


//using System.Collections.Generic;  // For List

public class Manager2VPS : MonoBehaviour
{
    Vector3 planeYZ = new Vector3(0f, 0.185f, 0.11f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }

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


    float fingerRadius = 0.0075f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }


    //For Curves Controller

    float curvesRadius = 0f;
    public float CurvesRadius
    {
        get { return this.curvesRadius; }
        private set { this.curvesRadius = value; }
    }

    float curvesHeight = 0.005f;
    public float CurvesHeight
    {
        get { return this.curvesHeight; }
        private set { this.curvesHeight = value; }
    }


    //For AUTD Controller
    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }

    int flagLM = 0;   //0 -> Position, 1 -> Strength, 2 ->Position+Strength
    public int FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }

    float curvesDis;
    public float CurvesDis
    {
        get { return this.curvesDis; }
        private set { this.curvesDis = value; }
    }


    //For Experiment
    public Text textRadius;
    public Text textDistance;


    void Start()
    {
        curvesRadius = 0.015f;
        curvesDis = 0.025f;

        CalcRadiusDelay();

        //Experiment
        textRadius = textRadius.GetComponent<Text>();
        textDistance = textDistance.GetComponent<Text>();

        textRadius.text = "Radius: " + (curvesRadius * 1000).ToString() + " mm";
        textDistance.text = "Distance: " + (curvesDis * 1000).ToString() + " mm";

    }

    void Update()
    {
        ///Change Curve Presentation Method
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad0)))
        {
            switch (flagLM)
            {
                case 0: //Position -> Strength
                    flagLM = 1;
                    break;
                case 1: //Strength -> Position+Strength
                    flagLM = 2;
                    break;
                case 2: //Position+Strength -> Position
                    flagLM = 0;
                    break;
            }
        }

        ///Change Radius
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            curvesRadius += 0.002f;
            CalcRadiusDelay();
            
            textRadius.text = "Radius: " + (curvesRadius * 1000).ToString("F0") + " mm";
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (curvesRadius > 0)
            {
                curvesRadius -= 0.002f;
                CalcRadiusDelay();

                textRadius.text = "Radius: " + (curvesRadius * 1000).ToString("F0") + " mm";
            }
            else
            {
                textRadius.text = "Radius: 0 mm!!";
                //Debug.Log("Already 0 mm");
            }

        }

        ///Change Distance
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
        {
            curvesDis += 0.002f;
            
            textDistance.text = "Distance: " + (curvesDis * 1000).ToString("F0") + " mm";
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (curvesDis > 0)
            {
                curvesDis -= 0.002f;

                textDistance.text = "Distance: " + (curvesDis * 1000).ToString("F0") + " mm";
            }
            else
            {
                textDistance.text = "Distance: 0 mm!!";
            }
        }
    }

    private void CalcRadiusDelay() //curveRadius, curveHight -> halfCurveLength, delay, halfExploreLength
    {
        if (2 * curvesRadius * curvesHeight > Mathf.Pow(curvesHeight, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * curvesRadius * curvesHeight - Mathf.Pow(curvesHeight, 2));
        }
        else
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = curvesRadius / (curvesRadius + fingerRadius);
        halfExploreLength = halfCurveLength / delay;
    }

}