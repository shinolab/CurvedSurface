using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;


//using System.Collections.Generic;  // For List

public class Manager2SPS : MonoBehaviour
{
    Vector3 planeYZ = new Vector3(0f, 0.27f, -0.01f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }

    //private float[] delayArray = { 0.78125f, 0.83815f, 0.416f, 0.3f };
    //private List<float> delayList = new List<float>();
    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }
    }

    //private float[] halfExploreLengthArray = { 0.025f, 0.005f, 0.003f};
    //private List<float> halfExploreLengthList = new List<float>();

    float halfCurveLength;
    public float HalfCurveLength
    {
        get { return this.halfCurveLength; }
        private set { this.halfCurveLength = value; }
    }


    float fingerRadius = 0.007f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }


    //For Sphere Controller

    float sphereRadius = 0f;
    public float SphereRadius
    {
        get { return this.sphereRadius; }
        private set { this.sphereRadius = value; }
    }
    int sphereRadiusMM = 0;

    float curveHight = 0f;


    //For AUTD Controller
    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }

    bool flagLM = false;   //true -> Strength, false -> Position
    public bool FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }


    //For Coroutine
    bool flagExitCoroutine = false;

    bool flagInput = false;
    public bool FlagInput
    {
        get { return this.flagInput; }
        private set { this.flagInput = value; }
    }
    //public InputField inputField;
    //public Text text;

    float managerDis;
    public float ManagerDis
    {
        get { return this.managerDis; }
        private set { this.managerDis = value; }
    }


    //For Experiment
    public Text textRadius;

    //public Text textManage;

    float distanceArrow = 0.005f;   //Contact: 0.005f, Power: 0.01f
    //float previousDistanceArrow = 0.005f;

    float previousManagerDis = 0f;
    int count = 0;
    bool flagInvert = false;
    int invertCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        //delay = 0.78125f;
        //halfCurveLength = 0.025f;
        //CalcHalfExploreLength();

        curveHight = 0.005f;

        sphereRadius = 0.03f;
        managerDis = 0.005f;
        previousManagerDis = managerDis;

        CalcCurvedLength();
        CalcHalfExploreLength();

        //Use number
        //inputField = inputField.GetComponent<InputField>();
        //text = text.GetComponent<Text>();
        //InitInputField();

        //Experiment
        textRadius = textRadius.GetComponent<Text>();

        sphereRadiusMM = (int)(sphereRadius * 1000);

        textRadius.text = "Radius: " + sphereRadiusMM.ToString() + "mm";
        Debug.Log("Radius: " + sphereRadiusMM.ToString() + "mm");

        //StartCoroutine("WaitKeyRadius");
        //StartCoroutine("WaitKeyDis");
        //StartCoroutine("WaitArrowDis");

    }

    //private IEnumerator WaitArrowDis()
    //{
    //    while (!flagExitCoroutine)
    //    {
    //        yield return new WaitUntil(() => (UnityEngine.Input.GetKeyDown(KeyCode.D)));
    //        Debug.Log("Distance Arrow Inputing...");

    //        while (! UnityEngine.Input.GetKeyDown(KeyCode.Return))
    //        {
    //            if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
    //            {
    //                distanceArrow -= 0.001f;
    //                //textDistance.text = "Distance: " + (distanceArrow * 1000).ToString("F1") + "mm";
    //            }
    //            if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
    //            {
    //                distanceArrow += 0.001f;
    //                //textDistance.text = "Distance: " + (distanceArrow * 1000).ToString("F1") + "mm";
    //            }
    //            yield return null;
    //        }

    //        managerDis = distanceArrow;
    //        Debug.Log("Distances Inputed," + "managerDis: " + managerDis.ToString());
    //    }
    //}




    //private IEnumerator WaitKeyRadius()
    //{
    //    while (! flagExitCoroutine)
    //    {
    //        yield return new WaitUntil(() => (UnityEngine.Input.GetKeyDown(KeyCode.S)));
    //        InitInputField();
    //        flagInput = true;
    //        Debug.Log("Radius Inputing...");
    //        text.text = inputField.text;

    //        yield return new WaitUntil(() => (UnityEngine.Input.GetKeyDown(KeyCode.Return)));
    //        string inputValue = inputField.text;
    //        InitInputField();

    //        sphereRadius = float.Parse(inputValue);
    //        CalcCurvedLength();
    //        CalcHalfExploreLength();
    //        Debug.Log("Radius Inputed," + "shpereRadius: " + sphereRadius.ToString("F4") + "delay: " + delay.ToString("F4"));
    //        flagInput = false;
    //    }
    //}
    //private IEnumerator WaitKeyDis()
    //{
    //    while (!flagExitCoroutine)
    //    {
    //        yield return new WaitUntil(() => (UnityEngine.Input.GetKeyDown(KeyCode.D)));
    //        InitInputField();
    //        flagInput = true;
    //        Debug.Log("Distance Inputing...");
    //        text.text = inputField.text;

    //        yield return new WaitUntil(() => (UnityEngine.Input.GetKeyDown(KeyCode.Return)));
    //        string inputValue = inputField.text;
    //        InitInputField();

    //        managerDis = float.Parse(inputValue);

    //        Debug.Log("Distance Inputed," + "managerDis: " + managerDis.ToString());
    //        flagInput = false;
    //    }
    //}
    //void InitInputField()
    //{
    //    inputField.text = ""; //Reset
    //    inputField.ActivateInputField(); //Focus?
    //}




    // Update is called once per frame
    void Update()
    {

        if ((!flagInput) && (UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad0)))
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
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
        {
            sphereRadius += 0.002f;
            CalcCurvedLength();
            CalcHalfExploreLength();
            sphereRadiusMM = (int)(sphereRadius * 1000);
            textRadius.text = "Radius: " + sphereRadiusMM.ToString() + "mm";
            Debug.Log("Radius: " + sphereRadiusMM.ToString() + "mm");
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (sphereRadius > 0)
            {
                sphereRadius -= 0.002f;
                CalcCurvedLength();
                CalcHalfExploreLength();
                sphereRadiusMM = (int)(sphereRadius * 1000);
                textRadius.text = "Radius: " + sphereRadiusMM.ToString() + "mm";
                Debug.Log("Radius: " + sphereRadiusMM.ToString() + "mm");
            }
            else
            {
                Debug.Log("Already 0 mm");
            }

        }



    }

    private void CalcCurvedLength() //sphereRadious, curvedHight -> halfCurveLength
    {
        if (2 * sphereRadius * curveHight > Mathf.Pow(curveHight, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * sphereRadius * curveHight - Mathf.Pow(curveHight, 2));
        }
        else
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = sphereRadius / (sphereRadius + fingerRadius);

    }


    //private void CalcHightDelay()   //halfExploreLength, curvedHight -> sphereRadius -> delay
    //{
    //    sphereRadius = (Mathf.Pow(halfCurveLength, 2) + Mathf.Pow(curveHight, 2)) / (2 * curveHight);

    //    delay = sphereRadius / (sphereRadius + fingerRadius);

    //    //Debug.Log("shpereRadius: " + sphereRadius.ToString("F4") + "delay: " + delay.ToString("F4"));
    //}

    private void CalcHalfExploreLength()
    {
        halfExploreLength = halfCurveLength / delay;
    }

    private void OnApplicationQuit()
    {
        if (!flagExitCoroutine) flagExitCoroutine = true;

    }

}
