using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;


//using System.Collections.Generic;  // For List

public class ManagerDemoV1 : MonoBehaviour
{
    //General parameters
    Vector3 planeYZ = new Vector3(0f, 0.115f, 0.052f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }
    float gap = -0.007f;   //Gap between the real finger position and the supposed finger position 
    public float Gap
    {
        get { return this.gap; }
        private set { this.gap = value; }
    }
    float fingerRadius = 0.0075f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }
    float curveHeightDepth = 0.005f;
    public float CurveHeightDepth
    {
        get { return this.curveHeightDepth; }
        private set { this.curveHeightDepth = value; }
    }

    int flagHaptics = 0;   //0 -> Position(Convex), 1 -> Strength(Convex), 2 ->Position+Strength(Convex), 3 -> Position(Concave), 4 -> Strength(Concave), 5 ->Position+Strength(Concave)
    public int FlagHaptics
    {
        get { return this.flagHaptics; }
        private set { this.flagHaptics = value; }
    }
    string stringHaptics;
    bool flagConvex = true; //false -> convene
    public bool FlagConvex
    {
        get { return this.flagConvex; }
        private set { this.flagConvex = value; }
    }

    float curveRadius = 0f;
    public float CurveRadius
    {
        get { return this.curveRadius; }
        private set { this.curveRadius = value; }
    }
    float curveDistance = 0f;
    public float CurveDistance
    {
        get { return this.curveDistance; }
        private set { this.curveDistance = value; }
    }
    bool flagStack = true;
    public bool FlagStack
    {
        get { return this.flagStack; }
        private set { this.flagStack = value; }
    }

    //Calculated parameters
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
    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }
    int numberPlacement;
    public int NumberPlacement
    {
        get { return this.numberPlacement; }
        private set { this.numberPlacement = value; }
    }

    //Text
    public Text textMode;
    public Text textRadius;
    public Text textDistance;





    void Start()
    {
        curveRadius = 0.015f;
        curveDistance = 0.024f;
        CalcRadiusDelay();
        JudgeStack();

        textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
        textDistance.text = "Distance: " + (curveDistance * 1000).ToString("F0") + " mm";
    }

    // Update is called once per frame
    void Update()
    {
        ///Change Curve Presentation Method
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad0)))
        {
            switch (flagHaptics)
            {
                case 0: //Position(Convex) -> Strength(Convex)
                    flagHaptics = 1;
                    stringHaptics = "Strength(Cvx.)";
                    break;
                case 1: //Strength(Convex) -> Position+Strength(Convex)
                    flagHaptics = 2;
                    stringHaptics = "Mixed(Cvx.)";
                    break;
                case 2: //Position+Strength(Convex) -> Position(Concave)
                    flagHaptics = 3;
                    CalcRadiusDelay();
                    flagConvex = false;
                    stringHaptics = "Position(Ccv.)";
                    break;
                case 3: //Position(Concave) -> Strength(Concave)
                    flagHaptics = 4;
                    stringHaptics = "Strength(Ccv.)";
                    break;
                case 4: //Strength(Concave) -> Position+Strength(Concave)
                    flagHaptics = 5;
                    stringHaptics = "Mixed(Ccv.)";
                    break;
                case 5: //Position+Strength(Concave) -> Position(Convex)
                    flagHaptics = 0;
                    CalcRadiusDelay();
                    flagConvex = true;
                    stringHaptics = "Position(Cvx.)";
                    break;
            }
            textMode.text = "Method: " + stringHaptics;
        }        

        ///Change Radius
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad8))
        {
            curveRadius += 0.002f;
            CalcRadiusDelay();
            JudgeStack();

            textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5))
        {
            if (curveRadius > 0.005f)
            {
                curveRadius -= 0.002f;
                CalcRadiusDelay();
                JudgeStack();

                textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
            }
            else
            {
                textRadius.text = "Radius: 5 mm is Minimum!";
            }
        }

        ///Change Distance
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))
        {
            curveDistance += 0.002f;
            JudgeStack();
            textDistance.text = "Distance: " + (curveDistance * 1000).ToString("F0") + " mm";
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (curveDistance > 0)
            {
                curveDistance -= 0.002f;
                JudgeStack();
                textDistance.text = "Distance: " + (curveDistance * 1000).ToString("F0") + " mm";
            }
            else
            {
                textDistance.text = "Distance: 2 mm is Minimum!";
                Debug.Log("Distance: Minimum");
            }

        }

    }

    private void CalcRadiusDelay() //curveRadius, curveDepth -> halfCurveLength, delay, halfExploreLength, 
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

        switch (flagHaptics)
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
        halfExploreLength = halfCurveLength / delay;
    }

    private void JudgeStack()
    {
        if (curveDistance / 2 < halfCurveLength) //When Stack
        {
            FlagStack = true;
        }
        else
        {
            FlagStack = false;
        }

        numberPlacement = Mathf.RoundToInt(0.4f / curveDistance);
        numberPlacement = numberPlacement / 2;//the number of items to place on each side
    }


}