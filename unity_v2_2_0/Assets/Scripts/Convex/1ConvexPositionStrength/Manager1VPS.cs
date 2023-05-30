using UnityEngine;
using UnityEngine.UI;



//using System.Collections.Generic;  // For List

public class Manager1VPS : MonoBehaviour
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
    float curveHeight = 0f;
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


    // Start is called before the first frame update
    void Start()
    {
        curveRadius = 0.015f;
        curveHeight = 0.005f;
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

        ///Change Height
        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            curveHeight += 0.002f;
            CalcRadiusDelay();

            Debug.Log("Height: " + (curveHeight * 1000).ToString("F0") + " mm");
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (curveRadius > 0)
            {
                curveRadius -= 0.002f;
                CalcRadiusDelay();

                Debug.Log("Height: " + (curveHeight * 1000).ToString("F0") + " mm");
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

    //private void CalcHightDelay()   //curveLength, curvedHight -> circleRadius -> delay
    //{
    //    curveRadius = (Mathf.Pow(halfCurveLength, 2) + Mathf.Pow(curveHeight, 2)) / (2 * curveHeight);

    //    delay = curveRadius / (curveRadius + fingerRadius);

    //    //Debug.Log("shpereRadius: " + circleRadius.ToString("F4") + "delay: " + delay.ToString("F4"));
    //}
}
