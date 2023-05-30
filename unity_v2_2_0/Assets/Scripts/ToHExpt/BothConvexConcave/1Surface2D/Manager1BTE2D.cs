using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Linq;

//using System.Collections.Generic;  // For List

public class Manager1BTE2D : MonoBehaviour
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
    float curveHeightDepth = 0.005f;
    public float CurveHeightDepth
    {
        get { return this.curveHeightDepth; }
        private set { this.curveHeightDepth = value; }
    }

    int flagLM = 0;   //0 -> Position, 1 -> Strength, 2 ->Position+Strength
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

    //public Text textRadius;

    float gap = -0.008f;   //Gap between the real finger position and the supposed finger position 
    public float Gap
    {
        get { return this.gap; }
        private set { this.gap = value; }
    }

    //For Creating CSV File
    private StreamWriter sw;
    private DateTime now = DateTime.Now;
    bool flagSWClose = false;

    //For set radius and method randomly
    int[] randomOrders;
    int count = 0;

    //For UI
    public Image ui;
    public Text textUI;

    float colorUI = 0.4f; //for color in UI, color32(0-255)
    bool inProcessWhite = false; //true -> in the process of White in/out, false -> not in the process
    bool whiteIn = true; //true -> white in, false -> white out

    //For get number 
    int keyNumber=0;

    //Change the information to string
    string stringMethod;
    string stringConvex;
    string stringVideoConvex;


    void Start()
    {
        //UI
        ui = ui.GetComponent<Image>();
        //textUI = textUI.GetComponent<Text>();

        //Creating CSV File
        sw = new StreamWriter(Application.dataPath + "/" + "CSVData" + "/" + @"SaveData" + now.Year.ToString() + now.Month.ToString() 
            + now.Day.ToString() + "_" + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + ".csv", true, Encoding.GetEncoding("Shift_JIS"));

        //Create and record Orders
        randomOrders = Enumerable.Range(0, 32).OrderBy(_ => Guid.NewGuid()).ToArray();
        string stringOrders = string.Join(",", randomOrders);
        sw.WriteLine(stringOrders);

        //Set First Condition
        string[] s1 = { "current Number(0-31)", "condition Number(0-31)", "Selected Number(1-7)" , "radius[mm]",
                        "Method", "Haptic Convex/Convene", "Video Convex/Convene"};
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);

        SetCondition(randomOrders[count]);//count=0
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
            keyNumber = 1;
            SaveAndDisplay();
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
        {
            keyNumber = 2;
            SaveAndDisplay();
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
        {
            keyNumber = 3;
            SaveAndDisplay();
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
        {
            keyNumber = 4;
            SaveAndDisplay();
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad5))
        {
            keyNumber = 5;
            SaveAndDisplay();
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))
        {
            keyNumber = 6;
            SaveAndDisplay();
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Keypad7))
        {
            keyNumber = 7;
            SaveAndDisplay();
        }

        //White In and Out
        if (inProcessWhite)
        {
            if (whiteIn) //White In
            {
                if (colorUI <= 1f)
                {
                    colorUI = colorUI + 0.003f;
                    ui.color = new Color(colorUI, colorUI, colorUI, 1);
                }
                else
                {
                    whiteIn = false;
                }
            }
            else //White Out
            {
                if (colorUI >= 0.4f)
                {
                    colorUI = colorUI - 0.003f;
                    ui.color = new Color(colorUI, colorUI, colorUI, 1);
                }
                else
                {
                    whiteIn = true;
                    inProcessWhite = false;
                }
            }
        }
        
    }

    private void SetCondition(int i)//Set Radius, Haptic Convx / Convene, Presentation Method, Video Convex / Convene
    {
        switch (i % 16) //Change Radius
        {
            case 0: 
            case 1:
            case 2: 
            case 3:
            case 4:
            case 5:
            case 12:
            case 13:
            case 14:
            case 15:
                curveRadius = 0.015f;
                //textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
                break;
            case 6: 
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
                curveRadius = 0.035f;
                //textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
                break;
        }
        if (i >= 28) curveRadius = 0.035f;

        switch (i / 16) //Change Haptic Convx / Convene
        {
            case 0: //0-15
                flagConvex = true;
                break;
            case 1: //16-31
                flagConvex = false;
                break;
        }

        switch (i % 16) //Change Presentation Method, 0 -> Position, 1 -> Strength, 2 ->Position+Strength
        {
            case 0: 
            case 1: 
            case 6:
            case 7:
            case 12:
            case 13:
                FlagLM = 0;
                break;
            case 2: 
            case 3:
            case 8:
            case 9:
            case 14:
            case 15:
                FlagLM = 1;
                break;
            case 4: 
            case 5:
            case 10:
            case 11:
                FlagLM = 2;
                break;
        }

        switch (i % 16) //Change Video Convx / Convene
        {
            case 0: 
            case 1: 
            case 2: 
            case 3:
            case 4: 
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
                flagVideoConvex = true;
                break;
            case 12: 
            case 13:
            case 14:
            case 15:
                flagVideoConvex = false;
                break;
        }
        if (i >= 16) flagVideoConvex = !flagVideoConvex;

        ///Calc
        CalcRadiusDelay();
    }

    private void SaveAndDisplay()
    {
        //Change the conditins to strings
        switch (flagLM)
        {
            case 0:
                stringMethod = "Position";
                break;
            case 1:
                stringMethod = "Strength";
                break;
            case 2:
                stringMethod = "Both";
                break;
        }
        switch (flagConvex)
        {
            case true:
                stringConvex = "Convex";
                break;
            case false:
                stringConvex = "Convene";
                break;
        }
        switch (flagVideoConvex)
        {
            case true:
                stringVideoConvex = "Video: Convex";
                break;
            case false:
                stringVideoConvex = "Video: Convene";
                break;
        }

        //Save Data
        string[] s1 = { count.ToString(), randomOrders[count].ToString(), keyNumber.ToString(),  (curveRadius * 1000).ToString("F0"), 
                        stringMethod, stringConvex, stringVideoConvex};
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);

        //Change UI, WhiteIn
        inProcessWhite = true;

        //Display next condition
        if (count < 31)
        {
            count++;
            SetCondition(randomOrders[count]);
            CalcRadiusDelay();
            textUI.text = "映像とどの程度違和感ないですか？\nHow comfortable is the haptic sensation with the video?\n(1-7)\n\nCurrent count：" + (count + 1).ToString() + "/32";
        }
        else
        {
            textUI.text = "実験終了です。お疲れさまでした！";
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

        switch (flagConvex)
        {
            case true: //Convex
                delay = curveRadius / (curveRadius + fingerRadius);
                break;
            case false: //Convene
                delay = curveRadius / Mathf.Abs(curveRadius - fingerRadius);
                break;
        }       
    }

    private void OnApplicationQuit()
    {
        if (!flagSWClose)
        {
            sw.Close();
            flagSWClose = true;
        }
    }
}
