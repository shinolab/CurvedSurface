using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;


public class FixedLM : MonoBehaviour
{
    Controller _autd = new Controller();
    Link _link = null;

    //Test Silence mode
    SilencerConfig config = SilencerConfig.None();


    public Transform[] transforms = null;


    Vector3 FocusPos;

    Vector3 YZ = new Vector3(0f, 0.22f, 0.11f); //y=0.30f, z=0f

    bool flag = true;


    //For Gain by Calc Linear LM / Circular LM
    const int Freq = 5;    //freq = 5;
    const int Size = 80;   // freq*size   sould be < 500 
    float LMLength = 0.003f;   //1mm==0.001f    //LMlength=0.003f

    int Interval = (int)(1000 / (Freq * Size));  //Unit; Millisecondes

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

    //float circularLMLength = 0.006f;

    public GameObject LMPlane = null;

    //public GameObject Focus = null;


    void Start()
    {
        StartAUTD();

        CalcLinearLMPos();
        CalcCircularLMPos();

        Task nowait = AUTDWork();  //Prepare Thread

        Debug.Log("Linear");

    }


    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
        {
            phi_angle = phi_angle + 5;
            phi_rad = (float)phi_angle / 180 * Mathf.PI;

            if (phi_angle > 180)
            {
                phi_angle = 0;
            }
            Debug.Log("phi_angle: " + phi_angle + "phi_rad: " + phi_rad.ToString("F4"));

        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))
        {
            phi_angle = phi_angle - 5;
            phi_rad = (float)phi_angle / 180 * Mathf.PI;

            if (phi_angle < 0)
            {
                phi_angle = 180;
            }
            Debug.Log("phi_angle: " + phi_angle + "phi_rad: " + phi_rad.ToString("F4"));
        }

        LMPlane.transform.rotation = Quaternion.Euler(0.0f, 0.0f, phi_angle - 90);
        LMPlane.transform.localScale = new Vector3(LMLength * 2, 0.001f, LMLength * 2);

        //Focus.transform.position = YZ+circularLMPos;


        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
            flagLinear = true;
            flagCircular = false;
            Debug.Log("Linear");
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
        {
            flagLinear = false;
            flagCircular = true;
            Debug.Log("Circular");
        }
    }

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
            //Debug.Log("Thread ID:" + Thread.CurrentThread.ManagedThreadId);

            while (flag)
            {
                Debug.Log("flag:" + flag);

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

    private void ExecuteLinearLM()     //private async void ExecuteSelfLM()
    {

        FocusPos = YZ;

        for (var i = 0; i < Size; i++)
        {
            if (flag == false) break;

            var LMPos = new Vector3(0.0f, 0.0f, arrayLinearLMPosZ[i]);
            var gain = new Focus(FocusPos + LMPos);

            //_autd.Send(new Static(),gain);
            _autd.Send(gain);

            Thread.Sleep(Interval);
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


        FocusPos = YZ;

        for (var i = 0; i < Size; i++)
        {
            if (flag == false) break;

            circularLMPos = new Vector3(arrayCircularLMPosX[i], arrayCircularLMPosY[i], arrayCircularLMPosZ[i]);
            var gain = new Focus(FocusPos + circularLMPos);

            _autd.Send(gain);

            Thread.Sleep(Interval);

        }

    }


    private void OnApplicationQuit()
    {

        flag = false;

        _autd.Stop();
        _autd.Clear();
        _autd.Close();
        _autd.Dispose();
    }


}