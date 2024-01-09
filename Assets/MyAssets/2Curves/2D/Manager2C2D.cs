using UnityEngine;
using UnityEngine.UI;


//using System.Collections.Generic;  // For List

public class Manager2C2D : MonoBehaviour
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

    Vector3 planeYZ = new Vector3(0f, 0.115f, 0.052f);   //y=0.26f, z=0.013f
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
    float curveHeightDepth = 0f;
    public float CurveHeightDepth
    {
        get { return this.curveHeightDepth; }
        private set { this.curveHeightDepth = value; }
    }
    float curveDistance;
    public float CurveDistance
    {
        get { return this.curveDistance; }
        private set { this.curveDistance = value; }
    }

    int flagLM = 0;   //0 -> Position(Convex), 1 -> Strength(Convex), 2 ->Position+Strength(Convex), 3 -> Position(Concave), 4 -> Strength(Concave), 5 ->Position+Strength(Concave)
    public int FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }
    bool flagConvex = true; //false -> convene
    public bool FlagConvex
    {
        get { return this.flagConvex; }
        private set { this.flagConvex = value; }
    }
    bool flagVideoConvex = true; //For video, false -> Convene
    public bool FlagVideoConvex
    {
        get { return this.flagVideoConvex; }
        private set { this.flagVideoConvex = value; }
    }

    public Text textRadius;
    public Text textDistance;

    float gap = -0.008f;   //Gap between the real finger position and the supposed finger position 
    public float Gap
    {
        get { return this.gap; }
        private set { this.gap = value; }
    }


    void Start()
    {
        curveRadius = 0.025f;
        curveHeightDepth = 0.005f;
        curveDistance = 0.05f;
        CalcRadiusDelay();

        textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
        textDistance.text = "Distance: " + (curveDistance * 1000).ToString("F0") + " mm";
    }

    // Update is called once per frame
    void Update()
    {

        ///Change Curve Presentation Method
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad0)))
        {
            switch (flagLM)
            {
                case 0: //Position(Convex) -> Strength(Convex)
                    flagLM = 1;
                    //convex = true;
                    break;
                case 1: //Strength(Convex) -> Position+Strength(Convex)
                    flagLM = 2;
                    //convex = true;
                    break;
                case 2: //Position+Strength(Convex) -> Position(Concave)
                    flagLM = 3;
                    CalcRadiusDelay();
                    flagConvex = false;
                    VideoConvexReverse();
                    break;
                case 3: //Position(Concave) -> Strength(Concave)
                    flagLM = 4;
                    //convex = false;
                    break;
                case 4: //Strength(Concave) -> Position+Strength(Concave)
                    flagLM = 5;
                    //convex = false;
                    break;
                case 5: //Position+Strength(Concave) -> Position(Convex)
                    flagLM = 0;
                    CalcRadiusDelay();
                    flagConvex = true;
                    VideoConvexReverse();
                    break;
            }
        }

        ///Change Convex and Convene
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1)))
        {
            VideoConvexReverse();
        }

        ///Change Radius
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            curveRadius += 0.002f;
            CalcRadiusDelay();

            textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
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

        ///Change Distance
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
        {
            curveDistance += 0.002f;
            textDistance.text = "Distance: " + (curveDistance * 1000).ToString("F0") + " mm";
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (curveDistance > 0)
            {
                curveDistance -= 0.002f;
                textDistance.text = "Distance: " + (curveDistance * 1000).ToString("F0") + " mm";
            }
            else
            {
                textDistance.text = "Distance: 0 mm!!";
                Debug.Log("Distance: 0 mm!!");
            }

        }

    }

    private void CalcRadiusDelay() //curveRadius, curveDepth -> halfCurveLength, circlesPosY, delay
    {
        if (2 * curveRadius * curveHeightDepth > Mathf.Pow(curveHeightDepth, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * curveRadius * curveHeightDepth - Mathf.Pow(curveHeightDepth, 2));
        }
        else //Error
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        switch (flagLM)
        {
            case 0: //Convex
            case 1:
            case 2:
                delay = curveRadius / (curveRadius + fingerRadius);
                break;
            case 3: //Convene
            case 4:
            case 5:
                delay = curveRadius / Mathf.Abs(curveRadius - fingerRadius);
                break;
        }
    }

    private void VideoConvexReverse()
    {
        if (flagVideoConvex)
        {
            flagVideoConvex = false;
        }
        else
        {
            flagVideoConvex = true;
        }
    }


}