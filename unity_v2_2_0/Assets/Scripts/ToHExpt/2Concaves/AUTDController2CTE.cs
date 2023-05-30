using AUTD3Sharp;
using UnityEngine;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;

//using System.Collections;  //Coroutine

public class AUTDController2CTE : MonoBehaviour
{
    private Manager2CTE _manager;

    //For Get Value
    Vector3 planeYZ;
    float fingerRadius;

    float delay;
    float halfCurveLength;
    float curvesRadius;
    float curvesDis;

    int flagLM;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)

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

    //For LM
    bool flagQuit = true;     //For Exit from Loop 

    //Calc FocusPos
    float centorCirclesPosX = 0f;
    float circlePosX;
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

    //Display Focus
    public GameObject FocusLinear = null;


    private void CalcLinearLMPos()
    {
        for (var i = 0; i < Size; i++)
        {
            arrayTheta[i] = 2 * Mathf.PI * i / Size;
            arrayLinearLMPosZ[i] = LMLength * Mathf.Sin(arrayTheta[i]);
            arrayLinearLMPos[i] = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
        }
    }
    private void ExecuteStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX - circlePosX) <= halfCurveLength / delay)  //When Curved
            {
                focusPosXStrength = fingerPosX;

                focusPosX = delay * (fingerPosX - circlePosX) + circlePosX;

                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(focusPosX - circlePosX, 2)), focusPosX - circlePosX);

                //Calculate PhiNew and PhiNewNormalized
                if (Mathf.Pow(curvesRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else //Prevent error
                {
                    phiPlane = 0f;
                }
                phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                if (phiNew <= (Mathf.PI / 2))
                {
                    phiNewNormalized = -2 * phiNew / Mathf.PI + 1;
                }
                else if (phiNewNormalized <= (Mathf.PI))
                {
                    phiNewNormalized = 2 * phiNew / Mathf.PI - 1;
                }
                else
                {
                    phiNewNormalized = 0f;
                }
                strength = Mathf.Pow(phiNewNormalized, 0.5f);
                FocusPos = new Vector3(focusPosXStrength, 0, 0) + planeYZ;

                //SendAUTD
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

    private void ExecutePosition()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

           //Debug.Log("circlePosX: "+circlePosX.ToString("F4")+"halfCurveLength: "+halfCurveLength.ToString("F4")+"delay: "+delay.ToString("F4"));
            //Calc FocusPos by focusPosX, focusPosY, 
            if (Mathf.Abs(fingerPosX - circlePosX) < halfCurveLength / delay)  //When Curved
            {
                focusPosX = delay * (fingerPosX - circlePosX) + circlePosX;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX - focusPosX, 2));
                //Debug.Log("FocusPosCalculated");

            }

            FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;



            //Send AUTD
            gain = new Focus(FocusPos + arrayLinearLMPos[i]);
            _autd.Send(gain);
            Thread.Sleep(Interval);
        }
    }

    private void ExecutePositionStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX - circlePosX) <= halfCurveLength / delay)  //When Curved
            {
                focusPosX = delay * (fingerPosX - circlePosX) + circlePosX;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX - focusPosX, 2));

                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(focusPosX - circlePosX, 2)), focusPosX - circlePosX);

                //Calculate PhiNew and PhiNewNormalized
                if (Mathf.Pow(curvesRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curvesRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else //Prevent error
                {
                    phiPlane = 0f;
                }
                phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                if (phiNew <= (Mathf.PI / 2))
                {
                    phiNewNormalized = -2 * phiNew / Mathf.PI + 1;
                }
                else if (phiNewNormalized <= (Mathf.PI))
                {
                    phiNewNormalized = 2 * phiNew / Mathf.PI - 1;
                }
                else
                {
                    phiNewNormalized = 0f;
                }
                strength = Mathf.Pow(phiNewNormalized, 0.5f);
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;

                //SendAUTD
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

    private void DisplayFocus()
    {
        FocusLinear.transform.position = FocusPos;
        //Debug.Log("FocusPos, X:" + focusPosX.ToString("F4"));
    }

    private void DisplayText()
    {
        switch (flagLM)
        {
            case 0: //Position -> Strength
                text.text = "Position";
                break;
            case 1: //Strength -> Position+Strength
                text.text = "Strength: " + (strength * 100).ToString("F0") + " %";
                break;
            case 2: //Position+Strength -> Position
                text.text = "Pos + Str: " + (strength * 100).ToString("F0") + " %";
                break;
        }
    }

    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2CTE>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;

        StartAUTD();
        CalcLinearLMPos();
        ProofTS();

        //Determine centor
        centorCirclesPosX = 0f;

        text = TextLM.GetComponent<Text>();

        Task nowait = AUTDWork();  //Prepare Thread

    }
    async Task AUTDWork()
    {

        await Task.Run(() =>
        {

            while (flagQuit)
            {
                switch (flagLM)
                {
                    case 0: //Position
                        ExecutePosition();
                        break;
                    case 1: //Strength
                        ExecuteStrength();
                        break;
                    case 2: //Position+Strength
                        ExecutePositionStrength();
                        break;
                }
            }
        });
    }


    void Update()
    {
        flagLM = _manager.FlagLM;
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curvesRadius = _manager.CurvesRadius;
        curvesDis = _manager.CurvesDis;

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

        fingerPosX = (touchPos.x - Centor) / Meter - 0.006f;   // 34/33=1.03...;

        //Determine left or right
        if (fingerPosX <= centorCirclesPosX)
        {
            circlePosX = -curvesDis / 2;//Left
        }
        else
        {
            circlePosX = curvesDis / 2;//Right
        }

        DisplayText();
        DisplayFocus();
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