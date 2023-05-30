using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;


//using System.Collections.Generic;  // For List

public class Manager2CTE : MonoBehaviour
{
    //Fixed values
    Vector3 planeYZ = new Vector3(0f, 0.215f, 0.11f);   //y=0.26f, z=0.013f
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

    //Changing values
    float curvesRadius = 0f;
    public float CurvesRadius
    {
        get { return this.curvesRadius; }
        private set { this.curvesRadius = value; }
    }

    float curvesDepth = 0.005f;
    public float CurvesDepth
    {
        get { return this.curvesDepth; }
        private set { this.curvesDepth = value; }
    }
    float halfCurveLength;
    public float HalfCurveLength
    {
        get { return this.halfCurveLength; }
        private set { this.halfCurveLength = value; }
    }
    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }
    }
    float circlesPosY;
    public float CirclesPosY
    {
        get { return this.circlesPosY; }
        private set { this.circlesPosY = value; }
    }
    float curvesDis;
    public float CurvesDis
    {
        get { return this.curvesDis; }
        private set { this.curvesDis = value; }
    }

    int flagLM = 0;   //0 -> Position, 1 -> Strength, 2 ->Position+Strength
    public int FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }
    //For Display text
    public Text textRadius;
    public Text textDistance;

    //For UI
    public Image ui;
    public Text textUI;
    float colorUI = 0.4f; //for color in UI, color32(0-255)

    bool inProcessWhite = false; //true -> in the process of White in/out, false -> not in the process
    bool whiteIn = true; //true -> white in, false -> white out    

    bool inProcessYellow = false; //true -> in the process of Yellow in/out, false -> not in the process
    bool yellowIn = true;

    //For Creating CSV File
    private StreamWriter sw;
    private DateTime now = DateTime.Now;
    bool flagSWClose = false;

    string method = "C:Position";

    //Count turn
    bool currentNumber = true; //true -> 1, fase -> 2
    bool previousNumber = true; //true -> 1, fase -> 2
    int countTurn = 0;
    int countInTurn = 0;

    void Start()
    {
        curvesRadius = 0.005f;
        curvesDis = 0f;
        CalcRadiusDelay();

        flagLM = 1; //0 -> Position, 1 -> Strength, 2 ->Position+Strength
        //method = "C:Position"; //Position, Strength, Pos + Str
        method = "C:Strength";
        //method = "C:Pos+Str";

        

        //Experiment
        textRadius = textRadius.GetComponent<Text>();
        textDistance = textDistance.GetComponent<Text>();

        textRadius.text = "Radius: " + (curvesRadius * 1000).ToString() + " mm";
        textDistance.text = "Distance: " + (curvesDis * 1000).ToString() + " mm";

        //Creating CSV File
        sw = new StreamWriter(Application.dataPath + "/" + "CSVData" + "/" + @"SaveData" + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + "_" + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + ".csv", true, Encoding.GetEncoding("Shift_JIS"));
        string[] s1 = { "Method", "Radius[mm]", "Distance[mm]", "1 or 2", "CountTurn" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);

        //UI
        ui = ui.GetComponent<Image>();
        textUI = textUI.GetComponent<Text>();
    }

    void Update()
    {
        ///Change Curve Presentation Method
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) ))
        {
            switch (flagLM)
            {
                case 0: //Position -> Strength
                    flagLM = 1;
                    method = "C:Position";
                    break;
                case 1: //Strength -> Position+Strength
                    flagLM = 2;
                    method = "C:Strength: ";
                    break;
                case 2: //Position+Strength -> Position
                    flagLM = 0;
                    method = "C:Pos + Str: ";
                    break;
            }
        }

        ////For CSV File and change distance
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
            currentNumber = true; //true -> 1
            countInTurn++;
            SaveData(method, (curvesRadius * 1000).ToString("F0") + " mm", (curvesDis * 1000).ToString("F0"), "1", countTurn.ToString());

            inProcessWhite = true;
            curvesDis += 0.002f;
            textDistance.text = "Distance: " + (curvesDis * 1000).ToString("F0") + " mm";

            textUI.text = "凹部をいくつ感じますか？（1または２）\n\nHow many concavity(ies) do you feel? (1 or 2)";
            //textUI.text = "凹部をいくつ感じますか？（1または２）\n折り返し回数: " + countTurn + "/6";
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
        {
            currentNumber = false; //false -> 2
            countInTurn++;
            SaveData(method, (curvesRadius * 1000).ToString("F0") + " mm", (curvesDis * 1000).ToString("F0"), "2", countTurn.ToString());

            textUI.text = "凹部をいくつ感じますか？（1または２）\n\nHow many concavity(ies) do you feel? (1 or 2)";
            //textUI.text = "凹部をいくつ感じますか？（1または２）\n折り返し回数: " + countTurn + "/6";

            if (curvesDis > 0)
            {
                inProcessWhite = true;
                curvesDis -= 0.002f;
                textDistance.text = "Distance: " + (curvesDis * 1000).ToString("F0") + " mm";
            }
            else
            {
                textUI.text = "この実験は終了です。\n\nYou finished this part.";
                textDistance.text = "Distance: 0 mm!!";
                SaveData("Done", "", "", "", "");
            }
        }
        //Error process
        if (UnityEngine.Input.GetKeyDown(KeyCode.E) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad8))
        {
            SaveData("Erase", "", "", "", "");
            inProcessYellow = true;

            if (currentNumber == previousNumber)
            {
                if (currentNumber)
                {
                    curvesDis -= 0.002f;
                }
                else
                {
                    curvesDis += 0.002f;
                }
            }
            if (countInTurn == 0)
            {
                countTurn--;
                currentNumber = !currentNumber;
                previousNumber = !previousNumber;
            }

            textUI.text = "処理を１つ戻しました。\n凹部をいくつ感じますか？（1または２）\n\nThe previous process was back. \nHow many concavity(ies) do you feel? (1 or 2)";
            //textUI.text = "処理を１つ戻しました。凹部をいくつ感じますか？（1または２）\n折り返し回数: " + countTurn + "/6";
        }

        ////White In and Out
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
        ////Yellow In and Out
        if (inProcessYellow)
        {
            if (yellowIn) //Yellow In
            {
                if (colorUI <= 1f)
                {
                    colorUI = colorUI + 0.003f;
                    ui.color = new Color(colorUI, colorUI, 0.4f, 1);
                }
                else
                {
                    yellowIn = false;
                }
            }
            else //Yellow Out
            {
                if (colorUI >= 0.4f)
                {
                    colorUI = colorUI - 0.003f;
                    ui.color = new Color(colorUI, colorUI, 0.4f, 1);
                }
                else
                {
                    yellowIn = true;
                    inProcessYellow = false;
                }
            }
        }

        ////Count Turn
        if (currentNumber != previousNumber)
        {
            countInTurn = 0;
            countTurn++;
            previousNumber = currentNumber;
            //textUI.text = "凹部をいくつ感じますか？（1または２）\n折り返し回数: " + countTurn + "/6";
        }
        if (countTurn == 6)
        {
            textUI.text = "この実験は終了です。\n\nYou finished this part.";
            SaveData("Finished", "", "", "", "");
            countTurn++;
        }

    }

    private void CalcRadiusDelay() //curveRadius, curveHight -> halfCurveLength, circlesPosY, delay
    {
        if (2 * curvesRadius * curvesDepth > Mathf.Pow(curvesDepth, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * curvesRadius * curvesDepth - Mathf.Pow(curvesDepth, 2));
        }
        else //Error
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        circlesPosY = planeYZ.y + curvesRadius - curvesDepth;

        delay = curvesRadius / Mathf.Abs(curvesRadius - fingerRadius);
        
    }

    public void SaveData(string txt1, string txt2, string txt3, string txt4, string txt5)
    {
        string[] s1 = { txt1, txt2, txt3, txt4, txt5 };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
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