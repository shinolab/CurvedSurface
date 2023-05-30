using UnityEngine;
using UnityEngine.UI;


//using System.Collections.Generic;  // For List

public class Manager1CTE : MonoBehaviour
{
    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }   //private set { this.delay = value; }
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

    float fingerRadius = 0.0075f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }

    Vector3 planeYZ = new Vector3(0f, 0.185f, 0.11f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }

    float curveRadius = 0f;
    public float CurveRadius
    {
        get { return this.curveRadius; }
        private set { this.curveRadius = value; }
    }
    float curveDepth = 0f;
    public float CurveDepth
    {
        get { return this.curveDepth; }
        private set { this.curveDepth = value; }
    }

    int flagLM = 0;   //0 -> Position, 1 -> Strength, 2 ->Position+Strength
    public int FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }

    public Text textRadius;

    float gap = -0.008f;   //Gap between the real finger position and the supposed finger position 
    public float Gap
    {
        get { return this.gap; }
        private set { this.gap = value; }
    }


    void Start()
    {
        curveRadius = 0.035f;
        curveDepth = 0.005f;
        CalcRadiusDelay();

        textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
    }

    // Update is called once per frame
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


        ///Change Depth
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            curveDepth += 0.002f;
            CalcRadiusDelay();

            Debug.Log("Depth: " + (curveDepth * 1000).ToString("F0") + " mm");
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (curveRadius > 0)
            {
                curveRadius -= 0.002f;
                CalcRadiusDelay();

                Debug.Log("Depth: " + (curveDepth * 1000).ToString("F0") + " mm");
            }
            else
            {
                Debug.Log("Depth: 0 mm!!");
            }

        }

        ///Change Radius
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
        {
            curveRadius += 0.002f;
            CalcRadiusDelay();

            textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (curveRadius > 0)
            {
                curveRadius -= 0.002f;
                CalcRadiusDelay();

                textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
            }
            else
            {
                textRadius.text = "Radius: 0 mm!!";
            }
        }

    }

    private void CalcRadiusDelay() //curveRadius, curveDepth -> halfCurveLength, circlesPosY, delay
    {
        if (2 * curveRadius * curveDepth > Mathf.Pow(curveDepth, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * curveRadius * curveDepth - Mathf.Pow(curveDepth, 2));
        }
        else //Error
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = curveRadius / Mathf.Abs(curveRadius - fingerRadius);
    }

    //private void CalcDepthDelay()   //curveLength, curveDepth -> curveRadius -> delay
    //{
    //    curveRadius = (Mathf.Pow(halfCurveLength, 2) + Mathf.Pow(curveDepth, 2)) / (2 * curveDepth);

    //    delay = curveRadius / (curveRadius - fingerRadius);

    //    //Debug.Log("shpereRadius: " + circleRadius.ToString("F4") + "delay: " + delay.ToString("F4"));
    //}


}
