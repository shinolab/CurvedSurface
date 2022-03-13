using AUTD3Sharp;
using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using System.Linq;


public class TrackAM : MonoBehaviour
{
    AUTD _autd = new AUTD();

    Link _link = null;

    public Transform[] transforms = null;

    //public GameObject Focus = null;
    public GameObject Finger = null;

    float touchPosX;
    //float focusPosX;
    float fingerPosX;

    Vector3 focusPos;

    Vector3 yz = new Vector3(0f, 0.27f, -0.01f);  //y=0.223f, z=0.02f

    float Left = 0f;
    float Right = 1919f;
    float Centor;
    float Length;
    float Meter;

    bool flagPause;

    void Start()
    {
        foreach (var transform in transforms)
        {
            //_autd.AddDevice(gameObject.transform.position, gameObject.transform.rotation);
            _autd.AddDevice(transform.position, transform.rotation);
        }

        string ifname = @"\\Device\\NPF_{425C764E-7B4C-44B7-A24D-60A34C403894}";
        _link = Link.SOEM(ifname, _autd.NumDevices);
        _autd.Open(_link);

        _autd.SilentMode = false;
        _autd.Send(Modulation.Sine(150)); // 150 Hz

        ProofTS();
    }

    void Update()
    {

        //if (Input.touchCount > 0)
        //{
        //    touchPosX = Input.GetTouch(0).position.x;
        //    ProofRL();

        //    Debug.Log("touchPosX: " + touchPosX.ToString("F4"));
        //}


        touchPosX = Input.mousePosition.x;
        Debug.Log("touchPosX: " + touchPosX.ToString("F4"));

        fingerPosX = (touchPosX - Centor) / Meter - 0.002f;   // 34/33=1.03...;


        focusPos = new Vector3(fingerPosX, yz.y, yz.z);
        Finger.transform.position = focusPos;


        //if (Mathf.Abs(fingerPosX) < 0.020f)
        //{
        //    if (!flagPause)
        //    {
        //        _autd.Resume();
        //        flagPause = true;
        //    }

        //    _autd.Send(Gain.FocalPoint(focusPos));
        //}
        //else
        //{
        //    if (flagPause)
        //    {
        //        _autd.Pause();
        //        flagPause = false;
        //    }

        //}


        _autd.Send(Gain.FocalPoint(focusPos));

        //Debug.Log("touchPosX: " + touchPosX.ToString("F4") + "fingerPosX: " + fingerPosX.ToString("F4") + ", FocusPosX: " + focusPos.x.ToString("F4"));
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
        _autd.Stop();
        _autd.Clear();
        _autd.Dispose();
    }
}
