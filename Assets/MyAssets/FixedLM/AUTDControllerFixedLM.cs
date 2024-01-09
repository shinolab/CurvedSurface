using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;

using System;
using System.Linq;
using AUTD3Sharp.Modulation;
using System.Diagnostics;

#if UNITY_2020_2_OR_NEWER
#nullable enable
#endif

public class AUTDControllerFiedLM : MonoBehaviour
{
    private Controller? _autd = null;
    private static bool _isPlaying = true;

    SilencerConfig config = SilencerConfig.None();

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

        //_autd!.Send(config);
        _autd!.Send(new Static()); //Maxamp=255
    }

    Vector3 FocusPos;
    Vector3 planeYZ = new Vector3(0f, 0.12f, 0.052f); //y=0.30f, z=0f
    bool flag = true;


    //For Gain by Calc Linear LM / Circular LM
    const int Freq = 8;    //freq = 5;
    const int Size = 25;   // freq*size   sould be < 500 
    float LMLength = 0.004f;  //1mm==0.001f

    int miliInterval = (int)(1000 / (Freq * Size));  //((1/freq)/size)*1000, unit[m]
    int microInterval = (int)(1000000 / (Freq * Size));  //((1/freq)/size)*1000*1000, unit[u]

    float[] arrayTheta = new float[Size];
    float[] arrayLinearLMPosZ = new float[Size];

    float[] arrayCircularLMPosX = new float[Size];
    float[] arrayCircularLMPosY = new float[Size];
    float[] arrayCircularLMPosZ = new float[Size];
    Vector3 circularLMPos;

    float phi_rad = 0f;
    int phi_angle = 0;

    bool flagLinear = true;
    bool flagCircular = false;

    public GameObject? LMPlane = null;


    private void Start()
    {
        CalcLinearLMPos();
        CalcCircularLMPos();

        Task nowait = AUTDWork();  //Prepare Thread
        UnityEngine.Debug.Log("Linear");
    }


    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
        {
            phi_angle = phi_angle + 5;
            phi_rad = (float)phi_angle / 180 * Mathf.PI;

            if (phi_angle > 180)
            {
                phi_angle = 0;
            }
            UnityEngine.Debug.Log("phi_angle: " + phi_angle + "phi_rad: " + phi_rad.ToString("F4"));

        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))
        {
            phi_angle = phi_angle - 5;
            phi_rad = (float)phi_angle / 180 * Mathf.PI;

            if (phi_angle < 0)
            {
                phi_angle = 180;
            }
            UnityEngine.Debug.Log("phi_angle: " + phi_angle + "phi_rad: " + phi_rad.ToString("F4"));
        }

        if (LMPlane != null) LMPlane.transform.rotation = Quaternion.Euler(0.0f, 0.0f, phi_angle - 90);
        if (LMPlane != null) LMPlane.transform.localScale = new Vector3(LMLength * 2, 0.001f, LMLength * 2);

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
            flagLinear = true;
            flagCircular = false;
            UnityEngine.Debug.Log("Linear");
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
        {
            flagLinear = false;
            flagCircular = true;
            UnityEngine.Debug.Log("Circular");
        }
    }

    async Task AUTDWork()
    {

        await Task.Run(() =>
        {
            //Debug.Log("Thread ID:" + Thread.CurrentThread.ManagedThreadId);

            while (flag)
            {
                if (flagLinear)
                {
                    flagCircular = false;
                    ExecuteLinearLM();
                }

                if (flagCircular)
                {
                    flagLinear = false;
                    ExecuteCircularLM();
                }

            }
        });
    }
    private void CalcLinearLMPos()
    {
        for (var i = 0; i < Size; i++)
        {
            arrayTheta[i] = 2 * Mathf.PI * i / Size;
            arrayLinearLMPosZ[i] = LMLength * Mathf.Sin(arrayTheta[i]);
        }
    }

    //private void ExecuteLinearLM()     //private async void ExecuteSelfLM()
    //{

    //    FocusPos = YZ;

    //    for (var i = 0; i < Size; i++)
    //    {
    //        if (flag == false) break;

    //        var LMPos = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
    //        _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + LMPos));
    //        SleepMicroseconds(Interval);
    //    }

    //}

    private void ExecuteLinearLM()
    {
        FocusPos = planeYZ;

        var stopwatch = new System.Diagnostics.Stopwatch();
        for (var i = 0; i < Size; i++)
        {
            if (!flag) break;

            stopwatch.Restart();  // ストップウォッチをリセットして再スタートします。

            var LMPos = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
            _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + LMPos));

            // これまでのステップでの経過時間を計算します。
            var elapsed = stopwatch.ElapsedMilliseconds * 1000 + stopwatch.ElapsedTicks / (System.Diagnostics.Stopwatch.Frequency / (1000 * 1000)) % 1000;

            // 残りの待機時間を計算します。
            var remainingTime = microInterval - (int)elapsed;

            // まだ待機時間が残っていれば、その時間だけ待機します。
            if (remainingTime > 0)
            {
                //SleepMicroseconds(remainingTime);
                Thread.Sleep(miliInterval);
            }
        }
    }

    private void CalcCircularLMPos()
    {
        for (var i = 0; i < Size; i++)
        {
            //arrayTheta[i] = 2 * Mathf.PI * i / Size;  //Same as Linear

            arrayCircularLMPosX[i] = LMLength * Mathf.Cos(arrayTheta[i]) * Mathf.Sin(phi_rad);
            arrayCircularLMPosY[i] = -LMLength * Mathf.Cos(arrayTheta[i]) * Mathf.Cos(phi_rad);
            arrayCircularLMPosZ[i] = LMLength * Mathf.Sin(arrayTheta[i]);
        }
    }
    private void ExecuteCircularLM()     //private async void ExecuteSelfLM()
    {


        FocusPos = planeYZ;

        for (var i = 0; i < Size; i++)
        {
            if (flag == false) break;

            circularLMPos = new Vector3(arrayCircularLMPosX[i], arrayCircularLMPosY[i], arrayCircularLMPosZ[i]);
            _autd?.Send(new AUTD3Sharp.Gain.Focus(FocusPos + circularLMPos));

            SleepMicroseconds(microInterval);

        }

    }

    private void SleepMicroseconds(int microseconds)
    {
        Stopwatch sw = Stopwatch.StartNew();

        while (true)
        {
            if (sw.Elapsed.TotalMilliseconds > microseconds / 1000.0)
                break;
        }
    }

    private void OnApplicationQuit()
    {
        flag = false;

        _autd?.Dispose();
    }
}

#if UNITY_2020_2_OR_NEWER
#nullable disable
#endif
