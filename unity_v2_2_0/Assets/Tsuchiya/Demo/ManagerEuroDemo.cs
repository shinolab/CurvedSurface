using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;


//using System.Collections.Generic;  // For List

public class ManagerEuroDemo : MonoBehaviour
{
    //General Information
    Vector3 planeYZ = new Vector3(0f, 0.22f, 0.11f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }
    float fingerRadius = 0.0075f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }
    float bumpsHeight = 0.005f;
    public float BumpsHeight
    {
        get { return this.bumpsHeight; }
        private set { this.bumpsHeight = value; }
    }

    //Changing Information
    bool flagMode = true;   //true -> Adjusting, false -> Presenting
    public bool FlagMode
    {
        get { return this.flagMode; }
        private set { this.flagMode = value; }
    }
    bool flagLM = true;   //true -> Position, false -> Strength
    public bool FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }
    float bumpsRadius = 0f;
    public float BumpsRadius
    {
        get { return this.bumpsRadius; }
        private set { this.bumpsRadius = value; }
    }
    float bumpsDistance = 0f;
    public float BumpsDistance
    {
        get { return this.bumpsDistance; }
        private set { this.bumpsDistance = value; }
    }


    //Calculated information
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
    float bumpsPosY;
    public float BumpsPosY
    {
        get { return this.bumpsPosY; }
        private set { this.bumpsPosY = value; }
    }

    //Text
    public Text textMode;   
    public Text textRadius;
    public Text textDistance;


    void Start()
    {
        bumpsRadius = 0.01f;
        CalcRadiusDelay();

        textMode = textMode.GetComponent<Text>();
        textRadius = textRadius.GetComponent<Text>();
        textDistance = textDistance.GetComponent<Text>();

        textRadius.text = "Radius: " + (bumpsRadius * 1000).ToString("F0") + " mm";
        textDistance.text = "Interval: " + (bumpsDistance * 1000).ToString("F0") + " mm";

    }

    void Update()
    {
        //Mode
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad9))) //Adjusting or Presenting //true ->Adjusting
        {
            if (flagMode)
            {
                flagMode = false;
                textMode.text = "Mode: Presenting";
            }
            else
            {
                flagMode = true;
                textMode.text = "Mode: Adjusting";
            }
        }
        //Method
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))) //Method Strength or Position true ->Position
        {
            if (flagLM)
            {
                flagLM = false;
                Debug.Log("Strength");
            }
            else
            {
                flagLM = true;
                Debug.Log("Position");
            }
        }
        //Radius
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5)))  //Bigger
        {
            bumpsRadius += 0.005f;
            CalcRadiusDelay();
            textRadius.text = "Radius: " + (bumpsRadius * 1000).ToString("F0") + " mm";
        }
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2)))  //Smaller
        {
            if (bumpsRadius > 0.005f)
            {
                bumpsRadius -= 0.005f;
                CalcRadiusDelay();
                textRadius.text = "Radius: " + (bumpsRadius * 1000).ToString("F0") + " mm";
            }
            else
            {
                textRadius.text = "Radius: Minimum!!";
            }
        }

        //Distance
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))) //Further
        {
            bumpsDistance += 0.004f;
            textDistance.text = "Interval: " + (bumpsDistance * 1000).ToString("F0") + " mm";
        }
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))) //Closer
        {
            if (bumpsDistance > 0.004f)
            {
                bumpsDistance -= 0.004f;
                textDistance.text = "Interval: " + (bumpsDistance * 1000).ToString("F0") + " mm";
            }
            else
            {
                textDistance.text = "Interval: Minimum!!";
            }
        }
    }

    private void CalcRadiusDelay() //bumpsRadius, bumpsHight -> halfCurveLength, delay, halfExploreLength,bumpsPosY
    {
        if (2 * bumpsRadius * bumpsHeight > Mathf.Pow(bumpsHeight, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * bumpsRadius * bumpsHeight - Mathf.Pow(bumpsHeight, 2));
        }
        else
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = bumpsRadius / (bumpsRadius + fingerRadius);
        halfExploreLength = halfCurveLength / delay;

        if (Mathf.Pow(bumpsRadius, 2) > Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            bumpsPosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(bumpsRadius, 2) - Mathf.Pow(halfCurveLength, 2));
            
        }
        else
        {
            bumpsPosY = planeYZ.y;
        }
    }

}