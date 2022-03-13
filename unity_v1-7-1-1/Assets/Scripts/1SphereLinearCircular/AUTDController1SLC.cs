using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;

//using System.Collections;  //Coroutine

public class AUTDController1SLC : MonoBehaviour
{
    private Manager1SLC _manager;
    private SphereController1SLC _sphereController;

    float delay;
    float halfExploreLength;


    float fingerRadius;

    float sphereRadius;


    Vector3 planeYZ;



    Vector3 FocusPos;

    float fingerPosX;
    public float FingerPosX
    {
        get { return this.fingerPosX; }
        private set { this.fingerPosX = value; }
    }

    float focusPosY;

    public GameObject LinearFocus = null;
    public GameObject CircularFocus = null;

    bool flagQuit = true;

    float Left = 0f;
    float Right = 1919f;
    float Centor;
    float Length;
    float Meter;
    Vector3 touchPos;


    bool flagLM = true;   //true -> Linear, false -> Circular
    //Linear
    const int linearFreq = 15;    //freq = 5;
    const int linearSize = 50;   // freq*size   sould be < 1000 
    float linearLMLength = 0.006f;   //1mm==0.001f    //LMlength=0.003f

    float linearTheta;
    Vector3 linearLMPos;
    Gain linearGain;

    int linearInterval = (int)(1000 / (linearFreq * linearSize));  //((1/frea)/size)*1000, unit[mm]

    //Circular
    const int circularFreq = 15;    //freq = 5;
    const int circularSize = 50;   // freq*size   sould be < 1000 
    float circularLMLength = 0.006f;   //1mm==0.001f    //LMlength=0.003f

    float circularTheta;
    Vector3 circularLMPos;
    Gain circularGain;

    int circularInterval = (int)(1000 / (circularFreq * circularSize));  //((1/frea)/size)*1000, unit[mm]

    float circularPhiRad = 0f;
    float circularPhiDegree = 0f;



    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1SLC>();
        _sphereController = GameObject.Find("Sphere").GetComponent<SphereController1SLC>();

        delay = _manager.Delay;
        halfExploreLength = _manager.HalfExploreLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;

        StartAUTD();

        ProofTS();

        Task nowait = AUTDWork();  //Prepare Thread



    }


    void Update()
    {
        delay = _manager.Delay;
        halfExploreLength = _manager.HalfExploreLength;

        flagLM = _manager.FlagLM;

        sphereRadius = _sphereController.SphereRadius;

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

        fingerPosX = (touchPos.x - Centor) / Meter - 0.002f;   // 34/33=1.03...;



        if (flagLM) //Linear
        {
            CircularFocus.transform.position = new Vector3(0.5f, 0.3f, 0f);

            LinearFocus.transform.position = FocusPos;
        }
        else //Circular
        {
            LinearFocus.transform.position = new Vector3(0.5f, 0.3f, 0f);

            CircularFocus.transform.position = FocusPos;
            CircularFocus.transform.rotation = Quaternion.Euler(0.0f, 0.0f, circularPhiDegree - 90);
            CircularFocus.transform.localScale = new Vector3(circularLMLength * 2, 0.001f, circularLMLength * 2);
        }


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

    async Task AUTDWork()
    {

        await Task.Run(() =>
        {

            while (flagQuit)
            {
                if (flagLM)
                {
                    ExecuteLinearLM();
                }
                else
                {
                    ExecuteCircularLM();
                }
            }
        });
    }


    private void ExecuteLinearLM()
    {
        for (var i = 0; i < linearSize; i++)
        {
            if (flagQuit == false) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfExploreLength) //When Curved
            {
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX * (1 - delay), 2));

                FocusPos = new Vector3(fingerPosX * delay, focusPosY, 0) + planeYZ;
                linearTheta = 2 * Mathf.PI * i / linearSize;
                linearLMPos = Mathf.Sin(linearTheta) * new Vector3(0.0f, 0.0f, linearLMLength);
                linearGain = Gain.FocalPoint(FocusPos + linearLMPos);
                _autd.Send(linearGain, false);   //false->don't wait for the data

                Thread.Sleep(linearInterval);

                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            else //When Plane
            {

                if (fingerPosX >= 0)
                {
                    FocusPos = new Vector3(halfExploreLength, focusPosY, 0) + planeYZ;
                }
                else
                {
                    FocusPos = new Vector3(-halfExploreLength, focusPosY, 0) + planeYZ;
                }

                //linearTheta = 2 * Mathf.PI * i / linearSize;
                //linearLMPos = Mathf.Sin(linearTheta) * new Vector3(0.0f, 0.0f, linearLMLength);
                //linearGain = Gain.FocalPoint(FocusPos + linearLMPos);

                linearGain = Gain.FocalPoint(FocusPos);
                _autd.Send(linearGain, false);   //false->don't wait for the data

                Thread.Sleep(linearInterval);

                //Debug.Log("Plane, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
        }

        //Debug.Log("Delay: 1" + ", touchPos" + touchPosX.ToString("F4") + ", Finger: " + fingerPosX.ToString("F4") + ", Focus: " + FocusPos.x.ToString("F4"));
    }


    private void ExecuteCircularLM()     //private async void ExecuteSelfLM()
    {
        for (var i = 0; i < circularSize; i++)
        {
            if (flagQuit == false) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfExploreLength) //When Curved
            {
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX * (1 - delay), 2));
                FocusPos = new Vector3(fingerPosX * delay, focusPosY, 0) + planeYZ;
                circularPhiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(fingerPosX * delay, 2)), fingerPosX * delay);
                circularPhiDegree = circularPhiRad / Mathf.PI * 180;

                circularTheta = 2 * Mathf.PI * i / circularSize;
                circularLMPos = new Vector3(circularLMLength, 0.0f, 0.0f) * Mathf.Cos(circularTheta) * Mathf.Sin(circularPhiRad) + new Vector3(0.0f, -circularLMLength, 0.0f) * Mathf.Cos(circularTheta) * Mathf.Cos(circularPhiRad) + new Vector3(0.0f, 0.0f, circularLMLength) * Mathf.Sin(circularTheta);
                circularGain = Gain.FocalPoint(FocusPos + circularLMPos);

                _autd.Send(circularGain, false);   //false->don't wait for the data

                Thread.Sleep(circularInterval);

                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            else //When Plane
            {

                if (fingerPosX >= 0)
                {
                    FocusPos = new Vector3(halfExploreLength, focusPosY, 0) + planeYZ;
                }
                else
                {
                    FocusPos = new Vector3(-halfExploreLength, focusPosY, 0) + planeYZ;
                }

                //circularTheta = 2 * Mathf.PI * i / circularSize;
                //circularLMPos = new Vector3(circularLMLength, 0.0f, 0.0f) * Mathf.Cos(circularTheta) * Mathf.Sin(circularPhiRad) + new Vector3(0.0f, -circularLMLength, 0.0f) * Mathf.Cos(circularTheta) * Mathf.Cos(circularPhiRad) + new Vector3(0.0f, 0.0f, circularLMLength) * Mathf.Sin(circularTheta);
                //circularGain = Gain.FocalPoint(FocusPos + circularLMPos);

                circularGain = Gain.FocalPoint(FocusPos);
                _autd.Send(circularGain, false);   //false->don't wait for the data

                Thread.Sleep(circularInterval);

                //Debug.Log("Plane, " + ", FocusPos" + FocusPos.ToString("F4"));
            }


        }

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

