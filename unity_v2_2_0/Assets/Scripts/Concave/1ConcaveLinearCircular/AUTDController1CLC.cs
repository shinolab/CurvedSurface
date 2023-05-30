using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;

//using System.Collections;  //Coroutine

public class AUTDController1CLC : MonoBehaviour
{
    private Manager1CLC _manager;
    private ConcaveController1CLC _concaveController;

    float delay;
    float halfCurveLength;


    float fingerRadius;

    float curveRadius;


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

    bool flagPause = false; //For pause and resume
    bool flagLM = true;   //true -> Linear, false -> Circular


    //For Gain by Calc Linear LM / Circular LM
    const int Freq = 5;    //freq = 5;
    const int Size = 80;   // freq*size   sould be < 500 
    float LMLength = 0.004f;   //1mm==0.001f    //LMlength=0.003f

    int Interval = (int)(1000 / (Freq * Size));  //Unit; Millisecondes

    //Linear
    float[] arrayTheta = new float[Size];
    float[] arrayLinearLMPosZ = new float[Size];
    Vector3[] arrayLinearLMPos = new Vector3[Size];

    //Circular
    float circularLMPosX;
    float circularLMPosY;
    float circularLMPosZ;
    Vector3 circularLMPos;

    float circularPhiRad = 0f;
    float circularPhiDegree = 0f;

    float circularTheta;




    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1CLC>();
        _concaveController = GameObject.Find("ConcaveBox").GetComponent<ConcaveController1CLC>();

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;

        StartAUTD();

        CalcLinearLMPos();

        ProofTS();

        Task nowait = AUTDWork();  //Prepare Thread


    }


    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        flagLM = _manager.FlagLM;

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
        //Debug.Log("Input.touchCount: " + Input.touchCount + "touchPosX: " + touchPosX.ToString("F4"));
        //Debug.Log("Input.mousePosition.x: " + Input.mousePosition.x.ToString("F4") + "touchPosX: " + touchPosX.ToString("F4"));

        fingerPosX = (touchPos.x - Centor) / Meter - 0.006f;   // 34/33=1.03...;



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
            CircularFocus.transform.localScale = new Vector3(LMLength * 2, 0.001f, LMLength * 2);
        }


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

    private void CalcLinearLMPos()
    {
        for (var i = 0; i < Size; i++)
        {
            arrayTheta[i] = 2 * Mathf.PI * i / Size;
            arrayLinearLMPosZ[i] = LMLength * Mathf.Sin(arrayTheta[i]);
            arrayLinearLMPos[i] = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
        }
    }

    private void ExecuteLinearLM()
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
                }


                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX * (delay-1), 2));

                FocusPos = new Vector3(fingerPosX * delay, focusPosY, 0) + planeYZ;

                var gain = new Focus(FocusPos + arrayLinearLMPos[i]);
                _autd.Send(gain);

                Thread.Sleep(Interval);

                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            else //When Plane
            {
                if (flagPause == false)
                {
                    _autd.Send(new Static(0));
                    flagPause = true;
                }

                //if (fingerPosX >= 0)
                //{
                //    FocusPos = new Vector3(halfCurveLength, focusPosY, 0) + planeYZ;
                //}
                //else
                //{
                //    FocusPos = new Vector3(-halfCurveLength, focusPosY, 0) + planeYZ;
                //}

                //var gain = new Focus(FocusPos);
                //_autd.Send(gain);

                //Thread.Sleep(Interval);
            }
        }

        //Debug.Log("Delay: 1" + ", touchPos" + touchPosX.ToString("F4") + ", Finger: " + fingerPosX.ToString("F4") + ", Focus: " + FocusPos.x.ToString("F4"));
    }



    private void ExecuteCircularLM()     //private async void ExecuteSelfLM()
    {
        for (var i = 0; i < Size; i++)
        {
            if (flagQuit == false) break;

            if (Mathf.Abs(fingerPosX * delay) <= halfCurveLength) //When Curved
            {
                focusPosY = fingerRadius - Mathf.Sqrt(Mathf.Pow(fingerRadius, 2) - Mathf.Pow(fingerPosX * (delay - 1), 2));
                FocusPos = new Vector3(fingerPosX * delay, focusPosY, 0) + planeYZ;
                circularPhiRad = Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(fingerPosX * delay, 2)), Mathf.Abs(fingerPosX * delay));
                circularPhiDegree = circularPhiRad / Mathf.PI * 180;

                circularTheta = 2 * Mathf.PI * i / Size;

                circularLMPosX = LMLength * Mathf.Cos(circularTheta) * Mathf.Sin(circularPhiRad);
                circularLMPosY = -LMLength * Mathf.Cos(circularTheta) * Mathf.Cos(circularPhiRad);
                circularLMPosZ = LMLength * Mathf.Sin(circularTheta);
                circularLMPos = new Vector3(circularLMPosX, circularLMPosY, circularLMPosZ);


                var gain = new Focus(FocusPos + circularLMPos);
                _autd.Send(gain);


                Thread.Sleep(Interval);

                //Debug.Log("Curved, " + ", FocusPos" + FocusPos.ToString("F4"));
            }
            else //When Plane
            {

                if (fingerPosX >= 0)
                {
                    FocusPos = new Vector3(halfCurveLength, focusPosY, 0) + planeYZ;
                }
                else
                {
                    FocusPos = new Vector3(-halfCurveLength, focusPosY, 0) + planeYZ;
                }


                var gain = new Focus(FocusPos);
                _autd.Send(gain);

                Thread.Sleep(Interval);

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

