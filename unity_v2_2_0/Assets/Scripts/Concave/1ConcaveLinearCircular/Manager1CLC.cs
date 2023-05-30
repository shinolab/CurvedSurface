using UnityEngine;
using UnityEngine.UI;


//using System.Collections.Generic;  // For List

public class Manager1CLC : MonoBehaviour
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


    bool flagLM = true;   //true -> Linear, false -> Circular
    public bool FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }

    public Text textRadius;


    void Start()
    {
        curveRadius = 0.015f;
        curveDepth = 0.005f;
        CalcRadiusDelay();

    }

    // Update is called once per frame
    void Update()
    {

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (flagLM)
            {
                flagLM = false;
                Debug.Log("Circular");
            }
            else
            {
                flagLM = true;
                Debug.Log("Linear");
            }
        }


        ///Change depth
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            curveDepth += 0.002f;
            CalcRadiusDelay();

            Debug.Log("Height: " + (curveDepth * 1000).ToString("F0") + " mm");
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (curveRadius > 0)
            {
                curveRadius -= 0.002f;
                CalcRadiusDelay();

                Debug.Log("Height: " + (curveDepth * 1000).ToString("F0") + " mm");
            }
            else
            {
                Debug.Log("Height: 0 mm!!");
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

    private void CalcRadiusDelay() //curveRadius, curveHight -> halfCurveLength, circlesPosY, delay
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
