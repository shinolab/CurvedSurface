using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;


//using System.Collections.Generic;  // For List

public class ManagerEuroDemo4 : MonoBehaviour
{
    Vector3 planeYZ = new Vector3(0f, 0.27f, -0.005f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }

    float fingerRadius = 0.007f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }


    //For Bump Controller
    int[] bumpRadiusMM = { 0, 0};

    float[] bumpRadius = { 0f, 0f };
    public float[] BumpRadius
    {
        get { return this.bumpRadius; }
        private set { this.bumpRadius = value; }
    }
    public Text textLeftRadius;
    public Text textRightRadius;



    float[] distance = { 0f, 0f };
    public float[] Distance
    {
        get { return this.distance; }
        private set { this.distance = value; }
    }
    int[] distanceMM = { 0, 0 }; //mm

    public Text textLeftDistance;
    public Text textRightDistance;


    float curveHight = 0.005f;
    public float CurveHight
    {
        get { return this.curveHight; }
        private set { this.curveHight = value; }
    }

    //For AUTD Controller
    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }

    bool flagLM = true;   //true -> Power, false -> Contact, LinearLM
    public bool FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }




    //For Mode
    public Text textMode;

    bool flagMode = true;   //true -> Adjusting, false -> Presenting
    public bool FlagMode
    {
        get { return this.flagMode; }
        private set { this.flagMode = value; }
    }
    public bool HasChangedValue
    {
        get; set;
    }

    //For Change Left/Right
    int commandLeftRight = 0;
    public int CommandLeftRight
    {
        get { return this.CommandLeftRight; }
        private set { this.CommandLeftRight = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        //curveHight = 0.005f;

        bumpRadiusMM[0] = 10;
        bumpRadius[0] = 0.01f;

        bumpRadius[1] = 10;
        bumpRadius[1] = 0.01f;

        HasChangedValue = false;

        //CalcCurvedLength();
        //CalcHalfExploreLength();

        //Demo

        textMode = textMode.GetComponent<Text>();

        textLeftRadius = textLeftRadius.GetComponent<Text>();
        textRightRadius = textRightRadius.GetComponent<Text>();

        textLeftDistance = textLeftDistance.GetComponent<Text>();
        textRightDistance = textRightDistance.GetComponent<Text>();

        textLeftRadius.text = "Radius: " + bumpRadiusMM[0].ToString() + "mm";
        textRightRadius.text = "Radius: " + bumpRadiusMM[1].ToString() + "mm";
    }




    // Update is called once per frame
    void Update()
    {
        /*
         * int command = 0 or 1
         * if(Input.GetKeyDown(Keycode.‰½‚©)){command=0}
         * else{command =1}
         * Œã‚ÍbumpRadius[command] = ’l‚Å‚¢‚¯‚é.
         */
        HasChangedValue = false;
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
            HasChangedValue = true;
        }
        //Change Left/Right
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad8))) //Method Strength or Position true ->Strength
        {
            if (commandLeftRight==1)
            {
                commandLeftRight = 0;
                Debug.Log("Left");
            }
            else
            {
                commandLeftRight = 1;
                Debug.Log("Right");
            }
            //HasChangedValue = true;
        }


        //Method
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))) //Method Strength or Position true ->Strength
        {
            if (flagLM)
            {
                flagLM = false;
                Debug.Log("Position");
            }
            else
            {
                flagLM = true;
                Debug.Log("Strength");
            }
            HasChangedValue = true;
        }

        //Radius
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5)))  //Bigger
        {
            bumpRadiusMM[commandLeftRight] += 5;
            bumpRadius[commandLeftRight] += 0.005f;

            textLeftRadius.text = "Radius: " + bumpRadiusMM[0] + "mm";

            if (commandLeftRight == 0)
            {
                textLeftRadius.text = "Radius: " + bumpRadiusMM[0] + "mm";
            }
            else
            {
                textRightRadius.text = "Radius: " + bumpRadiusMM[1] + "mm";
            }
            HasChangedValue = true;
        }
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2)))  //Smaller
        {
            if (bumpRadiusMM[commandLeftRight] > 5)
            {
                bumpRadiusMM[commandLeftRight] -= 5;
                bumpRadius[commandLeftRight] -= 0.005f;

                if (commandLeftRight == 0)
                {
                    textLeftRadius.text = "Radius: " + bumpRadiusMM[0] + "mm";
                }
                else
                {
                    textRightRadius.text = "Radius: " + bumpRadiusMM[1] + "mm";
                }
            }
            else
            {
                bumpRadiusMM[commandLeftRight] = 5;
                
                if (commandLeftRight == 0)
                {
                    textLeftRadius.text = "Radius 5 mm is Minimum!!";
                }
                else
                {
                    textRightRadius.text = "Radius 5 mm is Minimum!!";
                }
            }
            HasChangedValue = true;
        }

        //Distance
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))) //Further
        {
            distance[commandLeftRight] += 0.004f;
            distanceMM[commandLeftRight] += 4;
            
            if (commandLeftRight == 0)
            {
                textLeftDistance.text = "Distance: " + distanceMM + "mm";
            }
            else
            {
                textRightDistance.text = "Distance: " + distanceMM + "mm";
            }

            HasChangedValue = true;
        }
        if ((flagMode == true) && (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))) //Closer
        {
            if (distance[commandLeftRight] > 0)
            {
                distance[commandLeftRight] -= 0.004f;
                distanceMM[commandLeftRight] -= 4;
                if (commandLeftRight == 0)
                {
                    textLeftDistance.text = "Distance: " + distanceMM + "mm";
                }
                else
                {
                    textRightDistance.text = "Distance: " + distanceMM + "mm";
                }
            }
            else
            {
                
                if (commandLeftRight == 0)
                {
                    textLeftDistance.text = "Distance 0 mm!!";
                }
                else
                {
                    textRightDistance.text = "Distance 0 mm!!";
                }
            }
            HasChangedValue = true;
        }

    }

}
