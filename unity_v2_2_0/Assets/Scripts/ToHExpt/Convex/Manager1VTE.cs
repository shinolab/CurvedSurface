using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Linq;



//using System.Collections.Generic;  // For List

public class Manager1VTE : MonoBehaviour
{
    //private float[] delayArray = { 0.78125f, 0.83815f, 0.416f, 0.3f };
    //private List<float> delayList = new List<float>();

    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }   //private set { this.delay = value; }
    }

    //private float[] halfExploreLengthArray = { 0.025f, 0.005f, 0.003f};
    //private List<float> halfExploreLengthList = new List<float>();

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

    Vector3 planeYZ = new Vector3(0f, 0.185f, 0.11f);
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
    float curveHeight = 0.005f;
    public float CurveHeight
    {
        get { return this.curveHeight; }
        private set { this.curveHeight = value; }
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

    // Start is called before the first frame update
    void Start()
    {
        //UI
        ui = ui.GetComponent<Image>();
        textUI = textUI.GetComponent<Text>();

        //Creating CSV File
        sw = new StreamWriter(Application.dataPath + "/" + "CSVData" + "/" + @"SaveData" + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + "_" + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + ".csv", true, Encoding.GetEncoding("Shift_JIS"));
        
        //Create and record Orders
        randomOrders = Enumerable.Range(0, 24).OrderBy(_ => Guid.NewGuid()).ToArray();
        string stringOrders = string.Join(",",randomOrders);
        sw.WriteLine(stringOrders);

        //Set First Condition
        SetRadiusMethod(randomOrders[count]);//count=0
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Keypad3) ||
        Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Keypad7))
        {
            //Save Data

            //Display next condition
            count++;
            SetRadiusMethod(randomOrders[count]);
            textUI.text = "どれくらい盛り上がりを感じますか？（1-7）\n試行回数："+(count+1)+"/24";
        }
    }

    private void SetRadiusMethod(int i)
    {
        switch (i / 6)
        {
            case 0: //0-5
                curveRadius = 0.005f;
                CalcRadiusDelay();
                textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
                break;
            case 1: //6-11
                curveRadius = 0.015f;
                CalcRadiusDelay();
                textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
                break ;
            case 2: //12-17
                curveRadius = 0.025f;
                CalcRadiusDelay();
                textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
                break;
            case 3: //18-23
                curveRadius = 0.035f;
                CalcRadiusDelay();
                textRadius.text = "Radius: " + (curveRadius * 1000).ToString("F0") + " mm";
                break;
        }
        switch (i % 6) //FlagLM, 0 -> Position, 1 -> Strength, 2 ->Position+Strength
        {
            case 0: //0,6,12,18 -> Position
                FlagLM = 0;
                break;
            case 1: //1,7,13,19 -> Position
                FlagLM = 0;
                break;
            case 2: //2,8,14,20 -> Strength
                FlagLM = 1;
                break;
            case 3: //3,9,15,21 -> Strength
                FlagLM = 1;
                break;
            case 4: //4,10,16,22 -> Position+Strength
                FlagLM = 2;
                break;
            case 5: //5,11,17,23 -> Position+Strength
                FlagLM = 2;
                break;
        }
    }


    private void CalcRadiusDelay() //curveRadius, curveHight -> halfCurveLength, delay, halfExploreLength
    {
        if (2 * curveRadius * curveHeight > Mathf.Pow(curveHeight, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * curveRadius * curveHeight - Mathf.Pow(curveHeight, 2));
        }
        else
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = curveRadius / (curveRadius + fingerRadius);
        halfExploreLength = halfCurveLength / delay;
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
