using AUTD3Sharp;
using UnityEngine;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;


//using System.Collections;  //Coroutine
public class AUTDEuroDemo220518 : MonoBehaviour
{
    private ManagerEuroDemo _manager;
    private BumpsControllerEuroDemo _bumpsController;

    //For Get Value
    Vector3 planeYZ;
    float fingerRadius;

    float delay;
    float halfExploreLength;
    float halfCurveLength;
    float bumpRadius;

    bool flagLM = true;   //true -> Linear, false -> Circular

    //For Screen
    float Left = 0f;
    float Right = 1919f;
    float Centor;
    float Length;
    float Meter;
    Vector3 touchPos;

    float fingerPosX;

    //For LM
    bool flagQuit = true;     //For Exit from Loop 

    //Calc FocusPos
    float centorBumpPosX;
    float bumpPosX;
    float focusPosX;
    float focusPosY;
    Vector3 FocusPos;

    //For found target bump;
    int bumpNumber = 0;
    float distance = 0;
    List<float> eachBumpPosX = new List<float>();

    //For Power
    float focusPosXPower;

    float phiRad = 0f;
    float phiPlane = 0f;
    float phiNew = 0f;
    float phiNewNormalized = 0f;

    float power = 0f;



    //For Gain by Calc LM
    const int Freq = 15;    //freq = 5;
    const int Size = 65;   // freq*size   sould be < 1000 
    float LMLength = 0.01f;   //1mm==0.001f    //LMlength=0.003f

    float Theta;

    Vector3 LMPos;
    float circularPhiRad = 0f;

    float circularLMPosX;
    float circularLMPosY;
    float circularLMPosZ;

    Gain Gain;
    int Interval = (int)(1000 / (Freq * Size));  //((1/frea)/size)*1000, unit[mm]


    //Display LM
    public GameObject TextLM = null;
    Text text;

    //For Display Focus

    public GameObject FocusLinear = null;
    //public GameObject FocusCircular = null;
    float circularPhiDegree;


    ////For Display Finger
    public GameObject FingerReal = null;
    public GameObject FingerBump = null;

    //For Real
    Vector3 posFingerReal;

    //For Bump
    Vector3 posFingerBump;
    float bumpPosY;

    //https://ni4muraano.hatenablog.com/entry/2018/03/08/215029
    private float FindTargetBumpPosX(List<float> list, float value)
    {
        list.Clear();

        for (int i = 0; i <= bumpNumber; i++)
        {
            list.Add(i * distance);
            list.Add(-i * distance);
        }

        return list.Aggregate((x, y) => Math.Abs(x - value) < Math.Abs(y - value) ? x : y);
        //return list.IndexOf(closest);

    }

    private void ExecuteCSC()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            bumpPosX = FindTargetBumpPosX(eachBumpPosX, fingerPosX);
            //Debug.Log("bumpPosX: " + bumpPosX.ToString("F4"));

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength)  //When Curved
            {
                focusPosXPower = fingerPosX;

                focusPosX = delay * (fingerPosX - bumpPosX) + bumpPosX;

                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(bumpRadius, 2) - Mathf.Pow(focusPosX - bumpPosX, 2)), focusPosX - bumpPosX);
                if (Mathf.Pow(bumpRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(bumpRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
                }
                else
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
            }
            //else if (fingerPosX - bumpPosX > halfExploreLength)  //When Plane
            //{
            //    focusPosXPower = bumpPosX + halfExploreLength;
            //}
            //else
            //{
            //    focusPosXPower = bumpPosX - halfExploreLength;
            //}

            FocusPos = new Vector3(focusPosXPower, 0, 0) + planeYZ;

            //Gain by Calc LMPos, and power
            if ((Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength)) //When Curved
            {
                Theta = 2 * Mathf.PI * i / Size;
                LMPos = new Vector3(0.0f, 0.0f, LMLength) * Mathf.Sin(Theta);

                power = Mathf.Pow(phiNewNormalized, 0.5f);
                Gain = Gain.FocalPoint((FocusPos + LMPos), power);
                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            //else //When Plane
            //{
            //    power = 0;
            //    Gain = Gain.FocalPoint(FocusPos);
            //    //Debug.Log("Plane, " + ", FocusPos" + FocusPos.ToString("F4"));
            //}

            //Send AUTD
            _autd.Send(Gain, false);
            Thread.Sleep(Interval);
        }
    }

    private void ExecuteCPC()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            bumpPosX = FindTargetBumpPosX(eachBumpPosX, fingerPosX);
            //Debug.Log("bumpPosX: " + bumpPosX.ToString("F4"));

            //Calc FocusPos by focusPosX, focusPosY, 
            if (Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength)  //When Curved
            {
                focusPosX = delay * (fingerPosX - bumpPosX) + bumpPosX;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX - focusPosX, 2));

            }
            //else if (fingerPosX - bumpPosX > halfExploreLength)  //When Plane
            //{
            //    focusPosX = bumpPosX + halfCurveLength;
            //}
            //else
            //{
            //    focusPosX = bumpPosX - halfCurveLength;
            //}

            FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;


            //Gain by Calc LMPos
            if ((Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength)) //When Curved
            {
                Theta = 2 * Mathf.PI * i / Size;
                LMPos = new Vector3(0.0f, 0.0f, LMLength) * Mathf.Sin(Theta);

                Gain = Gain.FocalPoint(FocusPos + LMPos);
                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            //else //When Plane
            //{
            //    Gain = Gain.FocalPoint(FocusPos);
            //    //Debug.Log("Plane, " + ", FocusPos" + FocusPos.ToString("F4"));
            //}
            //Send AUTD;
            _autd.Send(Gain, false);   //false->don't wait for the data
            Thread.Sleep(Interval);
        }

    }

    private void DisplayFocus()
    {
        FocusLinear.transform.position = FocusPos;
        //Debug.Log("phiRad: " + phiRad.ToString()+ "phiPlane: " + phiPlane.ToString()+ "phiNew: " + phiNew.ToString()+ "power: " + power.ToString());
    }

    private void DisplayText()
    {
        if (flagLM) //Power
        {
            text.text = "Strength: " + (power * 100).ToString("F3") + "%";
        }
        else //Contact
        {
            text.text = "Position";
        }
    }


    private void DisplayFinger()
    {
        //Finger Real
        posFingerReal = FingerReal.transform.position;
        posFingerReal.x = fingerPosX;
        FingerReal.transform.position = posFingerReal;

        //Figner Bump
        posFingerBump = FingerBump.transform.position;
        posFingerBump.x = fingerPosX;

        if ((Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength))   //When Curved Surface
        {
            if (Mathf.Pow(bumpRadius, 2) - Mathf.Pow(focusPosX - bumpPosX, 2) <= 0)
            {
                posFingerBump.y = bumpPosY;
            }
            else
            {
                posFingerBump.y = Mathf.Sqrt(Mathf.Pow(bumpRadius, 2) - Mathf.Pow(focusPosX - bumpPosX, 2)) + bumpPosY;
            }
            //Debug.Log("bumpRadius: " + bumpRadius.ToString("F4") + "bumpPosX: " + bumpPosX.ToString("F4") + "focusPosX: " + focusPosX.ToString("F4") + "spherePosY: " + spherePosY.ToString("F4") + "posFingerSphere.y: " + posFingerSphere.y.ToString("F4"));
            //Debug.Log("Curved, " + "posFingerBump.y: " + posFingerBump.y.ToString("F4"));
        }
        //else //When Plane
        //{
        //    posFingerBump.y = planeYZ.y; //YZ.y + radius of the figner
        //    //Debug.Log("Plane, " + "posFingerBump.y: " + posFingerBump.y.ToString("F4"));
        //}
        FingerBump.transform.position = posFingerBump;
        FingerReal.transform.position = posFingerReal;

    }


    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo>();
        _bumpsController = GameObject.Find("BumpsController").GetComponent<BumpsControllerEuroDemo>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        halfExploreLength = _manager.HalfExploreLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;

        StartAUTD();
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
                if (flagLM) //Strength
                {
                    ExecuteCSC();
                }
                else
                {
                    //_autd.Send(Modulation.Static());
                    ExecuteCPC();
                }


            }
        });
    }


    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        halfExploreLength = _manager.HalfExploreLength;

        flagLM = _manager.FlagLM;

        bumpRadius = _bumpsController.BumpRadius;
        bumpPosY = _bumpsController.BumpPosY;

        bumpNumber = _bumpsController.BumpNumber;
        distance = _bumpsController.Distance;

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

        fingerPosX = (touchPos.x - Centor) / Meter + 0.001f;   // 34/33=1.03...;

        DisplayText();
        DisplayFocus();
        DisplayFinger();


    }

    AUTD _autd = new AUTD();
    Link _link = null;

    public Transform[] transforms = null;
    private void StartAUTD()
    {
        foreach (var transform in transforms)
        {
            _autd.AddDevice(transform.position, transform.rotation);
        }

        string ifname = @"\\Device\\NPF_{425C764E-7B4C-44B7-A24D-60A34C403894}";
        _link = Link.SOEM(ifname, _autd.NumDevices);
        _autd.Open(_link);

        _autd.SilentMode = false;

        _autd.Send(Modulation.Static());    //MaxAmp=255

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
