using AUTD3Sharp;
using UnityEngine;
using UnityEngine.UI;

using System.Threading;
using System.Threading.Tasks;

//using System.Collections;  //Coroutine

public class AUTDControllerEuroExpt : MonoBehaviour
{
    private ManagerEuroExpt _manager;
    private SphereControllerEuroExpt _sphereController;

    //For Get Value
    Vector3 planeYZ;
    float fingerRadius;

    float delay;
    float halfExploreLength;
    float halfCurveLength;
    float sphereRadius;

    bool flagLM = true;   //true -> Linear, false -> Circular

    float leftSpherePosX;
    float rightSpherePosX;

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
    float centorSpheresPosX;
    float spherePosX;
    float focusPosX;
    float focusPosY;
    Vector3 FocusPos;


    //For Power
    float focusPosXPower;

    float phiRad = 0f;
    float phiPlane = 0f;
    float phiNew = 0f;
    float phiNewNormalized = 0f;

    float power = 0f;



    //For Gain by Calc LM
    const int Freq = 15;    //freq = 5;
    const int Size = 50;   // freq*size   sould be < 1000 
    float LMLength = 0.006f;   //1mm==0.001f    //LMlength=0.003f

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
    public GameObject FingerSphere = null;

    //For Real
    Vector3 posFingerReal;

    //For Sphere
    Vector3 posFingerSphere;
    float spherePosY;

    private void ExecutePower()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Determine spherePosX
            centorSpheresPosX = (leftSpherePosX + rightSpherePosX) / 2;
            if (fingerPosX <= centorSpheresPosX)
            {
                spherePosX = leftSpherePosX;
            }
            else
            {
                spherePosX = rightSpherePosX;
            }

            //Calc FocusPos by focusPosX, focusPosY, and phiNewNormalized
            if (Mathf.Abs(fingerPosX - spherePosX) <= halfExploreLength)  //When Curved
            {
                focusPosXPower = fingerPosX;

                focusPosX = delay * (fingerPosX - spherePosX) + spherePosX;

                phiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(focusPosX - spherePosX, 2)), focusPosX - spherePosX);
                if (Mathf.Pow(sphereRadius, 2) > Mathf.Pow(halfCurveLength, 2))
                {
                    phiPlane = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(halfCurveLength, 2)), halfCurveLength);
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
            else if (fingerPosX - spherePosX > halfExploreLength)  //When Plane
            {
                focusPosXPower = spherePosX + halfExploreLength;
            }
            else
            {
                focusPosXPower = spherePosX - halfExploreLength;
            }

            FocusPos = new Vector3(focusPosXPower, 0, 0) + planeYZ;

            //Gain by Calc LMPos, and power
            if ((Mathf.Abs(fingerPosX - spherePosX) <= halfExploreLength)) //When Curved
            {
                Theta = 2 * Mathf.PI * i / Size;
                LMPos = new Vector3(0.0f, 0.0f, LMLength) * Mathf.Sin(Theta);

                power = Mathf.Pow(phiNewNormalized, 0.5f);
                Gain = Gain.FocalPoint((FocusPos + LMPos), power);
                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            else //When Plane
            {
                power = 0;
                Gain = Gain.FocalPoint(FocusPos);
                //Debug.Log("Plane, " + ", FocusPos" + FocusPos.ToString("F4"));
            }

            //Send AUTD
            _autd.Send(Gain, false);
            Thread.Sleep(Interval);
        }
    }

    private void ExecuteContact()
    {
        for (var i = 0; i < Size; i++)
        {
            //For exit form Loop
            if (flagQuit == false) break;

            //Determine spherePosX
            centorSpheresPosX = (leftSpherePosX + rightSpherePosX) / 2;
            if (fingerPosX <= centorSpheresPosX)
            {
                spherePosX = leftSpherePosX;
            }
            else
            {
                spherePosX = rightSpherePosX;
            }

            //Calc FocusPos by focusPosX, focusPosY, 
            if (Mathf.Abs(fingerPosX - spherePosX) <= halfExploreLength)  //When Curved
            {
                focusPosX = delay * (fingerPosX - spherePosX) + spherePosX;
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX - focusPosX, 2));

            }
            else if (fingerPosX - spherePosX > halfExploreLength)  //When Plane
            {
                focusPosX = spherePosX + halfCurveLength;
            }
            else
            {
                focusPosX = spherePosX - halfCurveLength;
            }

            FocusPos = new Vector3(focusPosX, focusPosY, 0) + planeYZ;


            //Gain by Calc LMPos
            if ((Mathf.Abs(fingerPosX - spherePosX) <= halfExploreLength)) //When Curved
            {
                Theta = 2 * Mathf.PI * i / Size;
                LMPos = new Vector3(0.0f, 0.0f, LMLength) * Mathf.Sin(Theta);

                Gain = Gain.FocalPoint(FocusPos + LMPos);
                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            else //When Plane
            {
                Gain = Gain.FocalPoint(FocusPos);
                //Debug.Log("Plane, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
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

        //Figner Sphere
        posFingerSphere = FingerSphere.transform.position;
        posFingerSphere.x = fingerPosX;

        if ((Mathf.Abs(fingerPosX - spherePosX) <= halfExploreLength))   //When Curved Surface
        {
            if (Mathf.Pow(sphereRadius, 2) - Mathf.Pow(focusPosX - spherePosX, 2) <= 0)
            {
                posFingerSphere.y = spherePosY;
            }
            else
            {
                posFingerSphere.y = Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(focusPosX - spherePosX, 2)) + spherePosY;
            }
            //Debug.Log("sphereRadius: " + sphereRadius.ToString("F4") + "spherePosX: " + spherePosX.ToString("F4") + "focusPosX: " + focusPosX.ToString("F4") + "spherePosY: " + spherePosY.ToString("F4") + "posFingerSphere.y: " + posFingerSphere.y.ToString("F4"));
            //Debug.Log("Curved, " + "posFingerSphere.y: " + posFingerSphere.y.ToString("F4"));
        }
        else //When Plane
        {
            posFingerSphere.y = planeYZ.y; //YZ.y + radius of the figner
            //Debug.Log("Plane, " + "posFingerSphere.y: " + posFingerSphere.y.ToString("F4"));
        }
        FingerSphere.transform.position = posFingerSphere;
        FingerReal.transform.position = posFingerReal;

    }


    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroExpt>();
        _sphereController = GameObject.Find("SphereController").GetComponent<SphereControllerEuroExpt>();

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
                if (flagLM) //Power
                {
                    ExecutePower();
                }
                else
                {
                    //_autd.Send(Modulation.Static());
                    ExecuteContact();
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

        sphereRadius = _sphereController.SphereRadius;
        spherePosY = _sphereController.SpherePosY;

        leftSpherePosX = _sphereController.LeftSpherePosX;
        rightSpherePosX = _sphereController.RightSpherePosX;

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

        fingerPosX = (touchPos.x - Centor) / Meter;   // 34/33=1.03...;

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
