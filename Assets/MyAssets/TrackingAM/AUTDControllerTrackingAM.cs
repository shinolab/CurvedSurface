using System;
using System.Linq;
using AUTD3Sharp;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
#nullable enable
#endif

public class AUTDControllerTrackingAM : MonoBehaviour
{
    private Controller? _autd = null;
    private static bool _isPlaying = true;

    private static void OnLost(string msg)
    {
        Debug.LogError(msg);
#if UNITY_EDITOR
        _isPlaying = false;  // UnityEditor.EditorApplication.isPlaying can be set only from the main thread
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }

    private static void LogOutput(string msg)
    {
        Debug.Log(msg);
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
                Debug.LogError("_autd is still null.");
                return; 
            }
        }
        catch (Exception)
        {
            Debug.LogError("Failed to open AUTD3 controller!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
            UnityEngine.Application.Quit();
#endif
        }


        _autd!.Send(new Clear());
        _autd!.Send(new Synchronize());
        _autd!.Send(new AUTD3Sharp.Modulation.Sine(150)); // 150 Hz
    }

    public GameObject? Finger = null;

    float touchPosX;
    float fingerPosX;
    Vector3 focusPos;

    float gap = -0.008f;

    Vector3 planeYZ = new Vector3(0f, 0.12f, 0.052f);  //y=0.223f, z=0.02f

    float Left = 0f;
    float Right = 1919f;
    float Centor;
    float Length;
    float Meter;

    private void Start()
    {
        ProofTS();
    }


    private void Update()
    {
        touchPosX = Input.mousePosition.x;
        Debug.Log("touchPosX: " + touchPosX.ToString("F4"));

        fingerPosX = (touchPosX - Centor) / Meter + gap;   // 34/33=1.03...;


        focusPos = new Vector3(fingerPosX, planeYZ.y, planeYZ.z);
        if (Finger != null) Finger.transform.position = focusPos;
        _autd?.Send(new AUTD3Sharp.Gain.Focus(focusPos));
    }

    private void ProofTS()
    {
        Centor = (Left + Right) / 2;
        Length = Right - Left;
        Meter = Length / 0.3465f;  //The length in the screen in relation to one meter in real
    }
    private void ProofRL()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.L))
        {
            Left = touchPosX;
            Debug.Log("LeftProofed: " + touchPosX.ToString("F4"));
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
        {
            Right = touchPosX;
            Debug.Log("RightProofed" + touchPosX.ToString("F4"));

            ProofTS();
        }
    }

    private void OnApplicationQuit()
    {
        _autd?.Dispose();
    }
}

#if UNITY_2020_2_OR_NEWER
#nullable disable
#endif
