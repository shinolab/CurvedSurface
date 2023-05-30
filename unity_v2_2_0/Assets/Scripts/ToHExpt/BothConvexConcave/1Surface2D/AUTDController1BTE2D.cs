using AUTD3Sharp;
using UnityEngine;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;

//using System.Collections;  //Coroutine

public class AUTDController1BTE2D : MonoBehaviour
{
    private Manager1BTE2D _manager;

    //For Get Value
    float delay;
    float halfCurveLength;
    float fingerRadius;
    float curveRadius;
    Vector3 planeYZ;
    float gap;

    //For Screen
    float Left = 0f;
    float Right = 1919f;
    float Centor;
    float Length;
    float Meter;
    Vector3 touchPos;



    float fingerPosX; //For Sending Finger Controller
    public float FingerPosX
    {
        get { return this.fingerPosX; }
        private set { this.fingerPosX = value; }
    }

    //Flag
    bool flagQuit = true;
    bool flagPause = false; //For pause and resume
    int flagLM;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)
    bool flagConvex = true; //false -> convene

    //Calc FocusPos
    float focusPosX;
    float focusPosY;
    Vector3 FocusPos;

    float fingerPosXInLoop;

    float phiRad = 0f;
    float phiPlane = 0f;
    float phiNew = 0f;
    float phiNewNormalized = 0f;

    float strength = 0f;


    ////For Gain by Calc LM
    const int Freq = 5;    //freq = 5;
    const int Size = 100;   // freq*size   sould be < 1000 
    float LMLength = 0.005f;   //1mm==0.001f    //LMlength=0.003f
    Gain gain;
    int Interval = (int)(1000 / (Freq * Size));  //((1/frea)/size)*1000, unit[mm]
    //Linear
    float[] arrayTheta = new float[Size];
    float[] arrayLinearLMPosZ = new float[Size];
    Vector3[] arrayLinearLMPos = new Vector3[Size];

    //Display LM
    //public Text textLM;
    //Display Focus
    public GameObject focusLinear = null;


    private void DisplayFocus()
    {
        focusLinear.transform.position = FocusPos;
    }

    private void DisplayText()
    {
        if (flagConvex)
        {
            switch (flagLM)
            {
                case 0:
                    //textLM.text = "Position(“Ê)";
                    break;
                case 1:
                    //textLM.text = "Strength(“Ê): " + (strength * 100).ToString("F0") + " %";
                    break;
                case 2:
                    //textLM.text = "Pos + Str(“Ê): " + (strength * 100).ToString("F0") + " %";
                    break;
            }
        }
        else
        {
            switch (flagLM)
            {
                case 0:
                    //textLM.text = "Position(‰š)";
                    break;
                case 1:
                    //textLM.text = "Strength(‰š): " + (strength * 100).ToString("F0") + " %";
                    break;
                case 2:
                    //textLM.text = "Pos + Str(‰š): " + (strength * 100).ToString("F0") + " %";
                    break;
            }
        }
    }


    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1BTE2D>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;
        gap = _manager.Gap;

        StartAUTD();
        CalcLinearLMPos();
        ProofTS();

        DisplayText();

        Task nowait = AUTDWork();  //Prepare Thread


    }


    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        flagLM = _manager.FlagLM;
        flagConvex = _manager.FlagConvex;

        curveRadius = _manager.CurveRadius;

        //if (Input.touchCount > 0)
        //{
        //    touchPosX = Input.GetTouch(0).position.x;
        //    ProofRL();
        //}

        touchPos = Input.mousePosition;
        touchPos.z = 1;
        Camera.main.ScreenToWorldPoint(touchPos);

        ProofRL();

        fingerPosX = (touchPos.x - Centor) / Meter + gap;   // 34/33=1.03...;

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

        //_autd.Send(config);
        _autd.Send(new Static()); //Maxamp=255
    }

    async Task AUTDWork()
    {

        await Task.Run(() =>
        {

            while (flagQuit)
            {
                if (flagConvex)
                {
                    switch (flagLM)
                    {
                        case 0:
                            ConvexPosition();
                            break;
                        case 1:
                            ConvexStrength();
                            break;
                        case 2:
                            ConvexPositionStrength();
                            break;
                    }
                }
                else
                {
                    switch (flagLM)
                    {
                        case 0:
                            ConcavePosition();
                            break;
                        case 1:
                            ConcaveStrength();
                            break;
                        case 2:
                            ConcavePositionStrength();
                            break;
                    }
                }

            }
        });
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



    private void CalcLinearLMPos()
    {
        for (var i = 0; i < Size; i++)
        {
            arrayTheta[i] = 2 * Mathf.PI * i / Size;
            arrayLinearLMPosZ[i] = LMLength * Mathf.Sin(arrayTheta[i]);
            arrayLinearLMPos[i] = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
        }
    }

    private void ConvexPosition()
    {
        for (var i = 0; i < Size; i++)
        {
            if (flagQuit == false) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    Thread.Sleep(Interval);
                    continue;
                }

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosXInLoop;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosXInLoop * Mathf.Abs(delay - 1), 2));
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                gain = new Focus(FocusPos + arrayLinearLMPos[i]);

                _autd.Send(gain);
                Thread.Sleep(Interval);
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }
            }
        }
    }

    private void ConcavePosition()
    {
        for (var i = 0; i < Size; i++)
        {
            if (flagQuit == false) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    Thread.Sleep(Interval);
                    continue;
                }

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosXInLoop;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosXInLoop * Mathf.Abs(delay - 1), 2));
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                gain = new Focus(FocusPos + arrayLinearLMPos[i]);

                _autd.Send(gain);
                Thread.Sleep(Interval);
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }
            }
        }
    }

    private void ConvexStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    Thread.Sleep(Interval);
                    continue;
                }

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosXInLoop;
                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(focusPosX, 2)), focusPosX);

                //Calculate PhiNew
                if (Mathf.Pow(curveRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else //Prevent error
                {
                    phiPlane = 0f;
                }
                phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                //Calc PhiNewNormalized
                if (phiNew <= (Mathf.PI / 2))
                {
                    phiNewNormalized = 2 * phiNew / Mathf.PI;
                }
                else
                {
                    phiNewNormalized = -(2 * phiNew / Mathf.PI) + 2;
                }

                strength = Mathf.Pow(phiNewNormalized, 0.5f);

                //SendAUTD
                FocusPos = new Vector3(fingerPosXInLoop, 0, 0) + planeYZ;
                gain = new Focus((FocusPos + arrayLinearLMPos[i]), strength);
                _autd.Send(gain);
                Thread.Sleep(Interval);
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }

                if (strength != 0) strength = 0;

            }

        }
    }

    private void ConcaveStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    Thread.Sleep(Interval);
                    continue;
                }

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosXInLoop;
                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(focusPosX, 2)), focusPosX);

                //Calculate PhiNew
                if (Mathf.Pow(curveRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else //Prevent error
                {
                    phiPlane = 0f;
                }
                phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                //Calc PhiNewNormalized
                if (phiNew <= (Mathf.PI / 2))
                {
                    phiNewNormalized = -(2 * phiNew / Mathf.PI) + 1;
                }
                else
                {
                    phiNewNormalized = 2 * phiNew / Mathf.PI - 1;
                }
                strength = Mathf.Pow(phiNewNormalized, 0.5f);

                //SendAUTD
                FocusPos = new Vector3(fingerPosXInLoop, 0, 0) + planeYZ;
                gain = new Focus((FocusPos + arrayLinearLMPos[i]), strength);
                _autd.Send(gain);
                Thread.Sleep(Interval);
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }

                if (strength != 0) strength = 0;

            }

        }
    }
    private void ConvexPositionStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    Thread.Sleep(Interval);
                    continue;
                }

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosX;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosXInLoop * Mathf.Abs(delay - 1), 2));

                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(focusPosX, 2)), focusPosX);

                //Calculate PhiNew
                if (Mathf.Pow(curveRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else //Prevent error
                {
                    phiPlane = 0f;
                }
                phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                //Calc PhiNewNormalized
                if (phiNew <= (Mathf.PI / 2))
                {
                    phiNewNormalized = 2 * phiNew / Mathf.PI;
                }
                else
                {
                    phiNewNormalized = -(2 * phiNew / Mathf.PI) + 2;
                }

                strength = Mathf.Pow(phiNewNormalized, 0.5f);
                //SendAUTD
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                gain = new Focus((FocusPos + arrayLinearLMPos[i]), strength);
                _autd.Send(gain);
                Thread.Sleep(Interval);
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }

                if (strength != 0) strength = 0;
            }

        }
    }

    private void ConcavePositionStrength()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    Thread.Sleep(Interval);
                    continue;
                }

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosX;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosXInLoop * Mathf.Abs(delay - 1), 2));

                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(focusPosX, 2)), focusPosX);

                //Calculate PhiNew
                if (Mathf.Pow(curveRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else //Prevent error
                {
                    phiPlane = 0f;
                }
                phiNew = (phiRad - phiPlane) * (Mathf.PI / 2) / ((Mathf.PI / 2) - phiPlane);

                //Calc PhiNewNormalized
                if (phiNew <= (Mathf.PI / 2))
                {
                    phiNewNormalized = -(2 * phiNew / Mathf.PI) + 1;
                }
                else
                {
                    phiNewNormalized = 2 * phiNew / Mathf.PI - 1;
                }
                strength = Mathf.Pow(phiNewNormalized, 0.5f);
                //SendAUTD
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                gain = new Focus((FocusPos + arrayLinearLMPos[i]), strength);
                _autd.Send(gain);
                Thread.Sleep(Interval);
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }

                if (strength != 0) strength = 0;
            }

        }
    }
}

