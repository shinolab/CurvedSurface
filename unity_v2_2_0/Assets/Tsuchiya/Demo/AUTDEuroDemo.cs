using AUTD3Sharp;
using UnityEngine;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;

//using System.Collections;  //Coroutine

public class AUTDEuroDemo : MonoBehaviour
{
    private ManagerEuroDemo _manager;
    private BumpsEuroDemo _bumpsController;

    //General Information
    Vector3 planeYZ;
    float fingerRadius;

    //Changing Information
    float delay;
    float halfCurveLength;
    float halfExploreLength;

    float bumpsRadius;

    //For found target bump;
    int bumpsNumber = 0;
    float bumpsDistance = 0;
    List<float> eachBumpPosX = new List<float>();

    //Mode
    bool flagMode = true;   //true -> Adjusting, false -> Presenting
    bool flagPause = false; //For pause and resume

    bool flagLM;   //true -> Position(pink), false -> Strength(blue)

    //For Screen
    float Left = 0f;
    float Right = 1919f;
    float Centor;
    float Length;
    float Meter;
    Vector3 touchPos;

    float fingerPosX;
    public float FingerPosX
    {
        get { return this.fingerPosX; }
        private set { this.fingerPosX = value; }
    }

    //LM Loop
    bool flagQuit = true;     //For Exit from Loop 
    bool flagStop = false; //For _autd.Stop();

    //Calc FocusPos
    float bumpPosX;
    public float BumpPosX
    {
        get { return this.bumpPosX; }
        private set { this.bumpPosX = value; }
    }
    float focusPosX;
    public float FocusPosX
    {
        get { return this.focusPosX; }
        private set { this.focusPosX = value; }
    }
    float focusPosY;
    Vector3 FocusPos;


    //For Strength
    float focusPosXStrength;

    float phiRad = 0f;
    float phiPlane = 0f;
    float phiNew = 0f;
    float phiNewNormalized = 0f;

    float strength = 0f;



    //For Gain by Calc LM
    const int Freq = 5;    //freq = 5;
    const int Size = 80;   // freq*size   sould be < 1000 
    float LMLength = 0.004f;   //1mm==0.001f    //LMlength=0.003f

    Gain gain;
    int Interval = (int)(1000 / (Freq * Size));  //((1/frea)/size)*1000, unit[mm]

    //Linear
    float[] arrayTheta = new float[Size];
    float[] arrayLinearLMPosZ = new float[Size];
    Vector3[] arrayLinearLMPos = new Vector3[Size];


    //Display LM
    public GameObject TextLM = null;
    Text text;


    private void CalcLinearLMPos()
    {
        for (var i = 0; i < Size; i++)
        {
            arrayTheta[i] = 2 * Mathf.PI * i / Size;
            arrayLinearLMPosZ[i] = LMLength * Mathf.Sin(arrayTheta[i]);
            arrayLinearLMPos[i] = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
        }
    }

    //https://ni4muraano.hatenablog.com/entry/2018/03/08/215029
    private float FindTargetBumpPosX(List<float> list, float value)
    {
        list.Clear();

        for (int i = 0; i <= bumpsNumber; i++)
        {
            list.Add((i * bumpsDistance)); //0.01f<=RightBump.transform.position.x
            list.Add(- (i * bumpsDistance)); //-0.01f<=LeftBump.transform.position.x
        }

        //float closest = list.Aggregate((x, y) => Math.Abs(x - value) < Math.Abs(y - value) ? x : y);
        //return list.IndexOf(closest);

        return list.Aggregate((x, y) => Math.Abs(x - value) < Math.Abs(y - value) ? x : y);
    }
    private void ExecuteStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            if (flagMode == true) 
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }
            }
            else
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                }

                //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
                if (Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength)  //When Curved
                {
                    focusPosXStrength = fingerPosX;

                    focusPosX = delay * (fingerPosX - bumpPosX) + bumpPosX;

                    phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(bumpsRadius, 2) - Mathf.Pow(focusPosX - bumpPosX, 2)), focusPosX - bumpPosX);

                    //Calculate PhiNew and PhiNewNormalized
                    if (Mathf.Pow(bumpsRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                    {
                        phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(bumpsRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                    }
                    else //Prevent error
                    {
                        phiPlane = 0f;
                    }
                    phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                    if (phiNew <= (Mathf.PI / 2))
                    {
                        phiNewNormalized = phiNew / (Mathf.PI / 2);
                    }
                    else if (phiNewNormalized <= (Mathf.PI))
                    {
                        phiNewNormalized = (Mathf.PI - phiNew) / (Mathf.PI / 2);
                    }
                    else
                    {
                        phiNewNormalized = 0f;
                    }
                    strength = Mathf.Pow(phiNewNormalized, 0.5f);

                    //SendAUTD
                    FocusPos = new Vector3(focusPosXStrength, 0, 0) + planeYZ;
                    gain = new Focus((FocusPos + arrayLinearLMPos[i]), strength);
                    _autd.Send(gain);
                    Thread.Sleep(Interval);
                }
                else //When Plane
                {
                    strength = 0;
                }

            }

        }
    }

    private void ExecutePosition()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            if (flagMode == true)
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }
            }
            else
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                }

                //Calc FocusPos by focusPosX, focusPosY, 
                if (Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength)  //When Curved
                {
                    focusPosX = delay * (fingerPosX - bumpPosX) + bumpPosX;
                    focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX - focusPosX, 2));

                    FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                    gain = new Focus(FocusPos + arrayLinearLMPos[i]);
                    _autd.Send(gain);
                    Thread.Sleep(Interval);
                }
            }
        }
    }


    private void DisplayText()
    {
        if (flagLM) //Position
        {
            text.text = "Position";
        }
        else //Strength
        {
            text.text = "Strength: " + (strength * 100).ToString("F0") + " %";
        }
    }

    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo>();
        _bumpsController = GameObject.Find("BumpsController").GetComponent<BumpsEuroDemo>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        halfExploreLength = _manager.HalfExploreLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;

        StartAUTD();
        CalcLinearLMPos();
        ProofTS();

        text = TextLM.GetComponent<Text>();

        Task nowait = AUTDWork();  //Prepare Thread

    }
    async Task AUTDWork()
    {

        await Task.Run(() =>
        {

            while (flagQuit)
            {
                if (flagLM) //Position
                {
                    ExecutePosition();                   
                }
                else
                {
                    ExecuteStrength();
                }
            }
        });
    }


    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        halfExploreLength = _manager.HalfExploreLength;

        flagMode = _manager.FlagMode;
        flagLM = _manager.FlagLM;
        bumpsRadius = _manager.BumpsRadius;
        bumpsDistance = _manager.BumpsDistance;

        bumpsNumber = _bumpsController.BumpsNumber;
        //Debug.Log("Left: " + leftCirclePosX.ToString("F0")+ ", Right: " + rightCirclePosX.ToString("F0"));

        //if (Input.touchCount > 0)
        //{
        //    touchPosX = Input.GetTouch(0).position.x;
        //    ProofRL();
        //}

        touchPos = Input.mousePosition;
        touchPos.z = 1;
        Camera.main.ScreenToWorldPoint(touchPos);

        ProofRL();
        //Debug.Log("Input.touchCount: " + Input.touchCount + "touchPosX: " + touchPosX.ToString("F4"));
        //Debug.Log("Input.mousePosition.x: " + Input.mousePosition.x.ToString("F4") + "touchPosX: " + touchPosX.ToString("F4"));

        fingerPosX = (touchPos.x - Centor) / Meter - 0.004f;   // 34/33=1.03...;
        //Debug.Log("fingerPosX" + fingerPosX.ToString("F4"));

        bumpPosX = FindTargetBumpPosX(eachBumpPosX, fingerPosX);   //Find TargetBumpPosX for CSC/CPC

        DisplayText();
    }

    Controller _autd = new Controller();
    Link _link = null;
    SilencerConfig config = SilencerConfig.None();

    public Transform[] transforms = null;
    private void StartAUTD()
    {
        foreach (var transform in transforms)
        {
            _autd.AddDevice(transform.position, transform.rotation);
        }

        string ifname = @"\\Device\\NPF_{425C764E-7B4C-44B7-A24D-60A34C403894}";
        _link = new SOEM(ifname, _autd.NumDevices);
        _autd.Open(_link);

        _autd.CheckAck = false;

        _autd.Clear();

        _autd.Synchronize();

        _autd.Send(config);
        _autd.Send(new Static()); //Maxamp=255
    }

    private void ProofTS()
    {
        Centor = (Left + Right) / 2;
        Length = Right - Left;
        Meter = Length * 1000 / 346.5f;  //The length in the screen in relation to one meter in real
    }
    private void ProofRL()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.L))
        {
            Left = touchPos.x;
            Debug.Log("LeftProofed: " + touchPos.x.ToString("F4"));
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            Right = touchPos.x;
            Debug.Log("RightProofed" + touchPos.x.ToString("F4"));

            ProofTS();
        }
    }


    private void OnApplicationQuit()
    {

        flagQuit = false;

        _autd.Stop();
        _autd.Clear();
        _autd.Close();
        _autd.Dispose();
    }

}