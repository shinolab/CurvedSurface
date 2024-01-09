using AUTD3Sharp;
using UnityEngine;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;
using AUTD3Sharp.Gain;
using AUTD3Sharp.Link;
using AUTD3Sharp.Modulation;
using System.Linq;
using System;
using System.Diagnostics;

//using System.Collections;  //Coroutine

public class AUTDController1C3D : MonoBehaviour
{
    private Manager1C3D _manager;

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
    const int Freq = 8;    //freq = 5;
    const int Size = 25;   // freq*size   sould be < 1000 
    float LMLength = 0.004f;   //1mm==0.001f    //LMlength=0.003f
    int hectoMicroInterval = (int)(10000 / (Freq * Size));  //((1/freq)/size)*1000*10, unit[hecto micro seconds]
    //Linear
    float[] arrayTheta = new float[Size];
    float[] arrayLinearLMPosZ = new float[Size];
    Vector3[] arrayLinearLMPos = new Vector3[Size];

    //Display LM
    public Text textLM;
    //Display Focus
    public GameObject focusLinear = null;

    //stopwatch
    Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

    private void DisplayFocus()
    {
        focusLinear.transform.position = FocusPos;
    }

    private void DisplayText()
    {
        switch (flagLM)
        {
            case 0:
                textLM.text = "Position(“Ê)";
                break;
            case 1:
                textLM.text = "Strength(“Ê): " + (strength * 100).ToString("F0") + " %";
                break;
            case 2:
                textLM.text = "Pos + Str(“Ê): " + (strength * 100).ToString("F0") + " %";
                break;
            case 3:
                textLM.text = "Position(‰š)";
                break;
            case 4:
                textLM.text = "Strength(‰š): " + (strength * 100).ToString("F0") + " %";
                break;
            case 5:
                textLM.text = "Pos + Str(‰š): " + (strength * 100).ToString("F0") + " %";
                break;
        }
    }
    private Controller? _autd = null;
    private static bool _isPlaying = true;

    private static void OnLost(string msg)
    {
        UnityEngine.Debug.LogError(msg);
#if UNITY_EDITOR
        _isPlaying = false;  // UnityEditor.EditorApplication.isPlaying can be set only from the main thread
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }

    private static void LogOutput(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    private static void LogFlush()
    {
    }

    private readonly AUTD3Sharp.Link.SOEM.OnLostCallbackDelegate _onLost = new(OnLost);
    private readonly AUTD3Sharp.Link.OnLogOutputCallback _output = new(LogOutput);
    private readonly AUTD3Sharp.Link.OnLogFlushCallback _flush = new(LogFlush);


    void Awake()
    {
        var builder = Controller.Builder();
        foreach (var obj in FindObjectsOfType<AUTD3Device>(false).OrderBy(obj => obj.ID))
        {
            builder.AddDevice(new AUTD3(obj.transform.position, obj.transform.rotation));
        }
        try
        {
            _autd = builder.OpenWith(new AUTD3Sharp.Link.SOEM()
                .WithOnLost(_onLost)
                .WithLogFunc(_output, _flush));

            if (_autd == null)
            {
                UnityEngine.Debug.LogError("_autd is still null.");
                return;
            }
        }
        catch (Exception)
        {
            UnityEngine.Debug.LogError("Failed to open AUTD3 controller!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            UnityEngine.Application.Quit();
#endif
        }


        _autd!.Send(new Clear());
        _autd!.Send(new Synchronize());

        _autd!.Send(new Static()); //Maxamp=255
    }

    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1C3D>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;
        gap = _manager.Gap;

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

        touchPos = Input.mousePosition;
        touchPos.z = 1;
        Camera.main.ScreenToWorldPoint(touchPos);

        ProofRL();

        fingerPosX = (touchPos.x - Centor) / Meter + gap;   // 34/33=1.03...;

        DisplayText();
        DisplayFocus();
    }

    async Task AUTDWork()
    {

        await Task.Run(() =>
        {

            while (flagQuit)
            {
                switch (flagLM)
                {
                    case 0: //Position(Convex)
                        ConvexPosition();
                        break;
                    case 1:
                        ConvexStrength();
                        break;
                    case 2:
                        ConvexPositionStrength();
                        break;
                    case 3: //Position(Concave)
                        ConcavePosition();
                        break;
                    case 4: //Strength(Concave)
                        ConcaveStrength();
                        break;
                    case 5: //Position+Strength(Concave)
                        ConcavePositionStrength();
                        break;
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
            UnityEngine.Debug.Log("LeftProofed: " + touchPos.x.ToString("F4"));
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            Right = touchPos.x;
            UnityEngine.Debug.Log("RightProofed" + touchPos.x.ToString("F4"));

            ProofTS();
        }
    }


    private void OnApplicationQuit()
    {
        flagQuit = false;

        _autd.Dispose();
    }
    private void SleepHectoMicroseconds(int hMicroseconds)
    {
        Stopwatch sw = Stopwatch.StartNew();

        double targetMilliseconds = hMicroseconds / 10.0;  // Convert to milliseconds

        while (true)
        {
            if (sw.Elapsed.TotalMilliseconds > targetMilliseconds)
                break;
        }
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
            if (!flagQuit) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    //Thread.Sleep(Interval);
                    SleepHectoMicroseconds(hectoMicroInterval);
                    continue;
                }

                stopwatch.Restart();

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosXInLoop;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosXInLoop * Mathf.Abs(delay - 1), 2));
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + arrayLinearLMPos[i]));

                var elapsedHectoMicroseconds = stopwatch.ElapsedMilliseconds * 10
                             + (stopwatch.ElapsedTicks * (1000 * 10) / System.Diagnostics.Stopwatch.Frequency);//Calculate the elapsed time for the current step
                var remainingTimeHectoMicroseconds = hectoMicroInterval - elapsedHectoMicroseconds;//Calculate the remaining wait time
                if (remainingTimeHectoMicroseconds > 0) SleepHectoMicroseconds((int)remainingTimeHectoMicroseconds);//If there is still waiting time left, wait for that duration
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static().WithAmp(0));
                    flagPause = true;
                }
            }
        }
    }
    private void ConcavePosition()
    {
        for (var i = 0; i < Size; i++)
        {
            if (!flagQuit) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    SleepHectoMicroseconds(hectoMicroInterval);
                    continue;
                }

                stopwatch.Restart();

                fingerPosXInLoop = fingerPosX;
                focusPosX = delay * fingerPosXInLoop;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosXInLoop * Mathf.Abs(delay - 1), 2));
                FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;
                _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + arrayLinearLMPos[i]));

                var elapsedHectoMicroseconds = stopwatch.ElapsedMilliseconds * 10
                             + (stopwatch.ElapsedTicks * (1000 * 10) / System.Diagnostics.Stopwatch.Frequency);//Calculate the elapsed time for the current step
                var remainingTimeHectoMicroseconds = hectoMicroInterval - elapsedHectoMicroseconds;//Calculate the remaining wait time
                if (remainingTimeHectoMicroseconds > 0) SleepHectoMicroseconds((int)remainingTimeHectoMicroseconds);//If there is still waiting time left, wait for that duration

            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static().WithAmp(0));
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
            if (!flagQuit) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    SleepHectoMicroseconds(hectoMicroInterval);
                    continue;
                }

                stopwatch.Restart();

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
                _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + arrayLinearLMPos[i]).WithAmp(strength));

                var elapsedHectoMicroseconds = stopwatch.ElapsedMilliseconds * 10
                             + (stopwatch.ElapsedTicks * (1000 * 10) / System.Diagnostics.Stopwatch.Frequency);//Calculate the elapsed time for the current step
                var remainingTimeHectoMicroseconds = hectoMicroInterval - elapsedHectoMicroseconds;//Calculate the remaining wait time
                if (remainingTimeHectoMicroseconds > 0) SleepHectoMicroseconds((int)remainingTimeHectoMicroseconds);//If there is still waiting time left, wait for that duration
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static().WithAmp(0));
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
            if (!flagQuit) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    SleepHectoMicroseconds(hectoMicroInterval);
                    continue;
                }

                stopwatch.Restart();

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
                _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + arrayLinearLMPos[i]).WithAmp(strength));

                var elapsedHectoMicroseconds = stopwatch.ElapsedMilliseconds * 10
                             + (stopwatch.ElapsedTicks * (1000 * 10) / System.Diagnostics.Stopwatch.Frequency);//Calculate the elapsed time for the current step
                var remainingTimeHectoMicroseconds = hectoMicroInterval - elapsedHectoMicroseconds;//Calculate the remaining wait time
                if (remainingTimeHectoMicroseconds > 0) SleepHectoMicroseconds((int)remainingTimeHectoMicroseconds);//If there is still waiting time left, wait for that duration
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static().WithAmp(0));
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
            if (!flagQuit) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    SleepHectoMicroseconds(hectoMicroInterval);
                    continue;
                }

                stopwatch.Restart();

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
                _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + arrayLinearLMPos[i]).WithAmp(strength));

                var elapsedHectoMicroseconds = stopwatch.ElapsedMilliseconds * 10
                             + (stopwatch.ElapsedTicks * (1000 * 10) / System.Diagnostics.Stopwatch.Frequency);//Calculate the elapsed time for the current step
                var remainingTimeHectoMicroseconds = hectoMicroInterval - elapsedHectoMicroseconds;//Calculate the remaining wait time
                if (remainingTimeHectoMicroseconds > 0) SleepHectoMicroseconds((int)remainingTimeHectoMicroseconds);//If there is still waiting time left, wait for that duration
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static().WithAmp(0));
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
            if (!flagQuit) break;

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                if (flagPause == true)
                {
                    _autd.Send(new Static());
                    flagPause = false;
                    SleepHectoMicroseconds(hectoMicroInterval);
                    continue;
                }

                stopwatch.Restart();

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
                _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + arrayLinearLMPos[i]).WithAmp(strength));

                var elapsedHectoMicroseconds = stopwatch.ElapsedMilliseconds * 10
                             + (stopwatch.ElapsedTicks * (1000 * 10) / System.Diagnostics.Stopwatch.Frequency);//Calculate the elapsed time for the current step
                var remainingTimeHectoMicroseconds = hectoMicroInterval - elapsedHectoMicroseconds;//Calculate the remaining wait time
                if (remainingTimeHectoMicroseconds > 0) SleepHectoMicroseconds((int)remainingTimeHectoMicroseconds);//If there is still waiting time left, wait for that duration
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static().WithAmp(0));
                    flagPause = true;
                }

                if (strength != 0) strength = 0;
            }

        }
    }
}

