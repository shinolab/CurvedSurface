using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;


public class FixedLM : MonoBehaviour
{
    AUTD _autd = new AUTD();
    Link _link = null;

    public Transform[] transforms = null;


    Vector3 FocusPos;

    Vector3 YZ = new Vector3(0f, 0.27f,0.05f); //y=0.27f, z=-0.01f

    bool flag = true;


    bool flagLinear = true;
    bool flagCircular = false;

    float circularLMLength = 0.006f;

    float phi_rad = 0f;
    int phi_angle = 0;

    public GameObject LMPlane = null;

    //public GameObject Focus = null;
    Vector3 circularLMPos;

    void Start()
    {
        StartAUTD();

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
        LMPlane.transform.localScale = new Vector3(circularLMLength * 2, 0.001f, circularLMLength * 2);

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
        _link = Link.SOEM(ifname, _autd.NumDevices);
        _autd.Open(_link);

        _autd.SilentMode = false;   //_autd.SilentMode = false;

        _autd.Send(Modulation.Static());  //Maxamp=255
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


    private void ExecuteLinearLM()     //private async void ExecuteSelfLM()
    {
        int freq = 15;    //freq = 5;
        const int size = 50;   // freq*size   sould be < 1000 
        float LMLength = 0.006f;   //1mm==0.001f    //LMlength=0.003f

        int milliseconds = (int)(1000 / (freq * size));    //((1/frea)/size)*1000

        FocusPos = YZ;

        for (var i = 0; i < size; i++)
        {
            if (flag == false) break;

            float theta = 2 * Mathf.PI * i / size;
            var LMPos = Mathf.Sin(theta) * new Vector3(0.0f, 0.0f, LMLength);
            var gain = Gain.FocalPoint(FocusPos + LMPos);

            _autd.Send(gain, false);   //false->don't wait for the data

            Thread.Sleep(milliseconds);

        }

    }

    private void ExecuteCircularLM()     //private async void ExecuteSelfLM()
    {
        int freq = 15;    //freq = 5;
        const int size = 50;   // freq*size   sould be < 1000                  
        int milliseconds = (int)(1000 / (freq * size));    //((1/frea)/size)*1000

        FocusPos = YZ;

        for (var i = 0; i < size; i++)
        {
            if (flag == false) break;

            float theta = 2 * Mathf.PI * i / size;
            circularLMPos = new Vector3(circularLMLength, 0.0f, 0.0f) * Mathf.Cos(theta) * Mathf.Sin(phi_rad) + new Vector3(0.0f, -circularLMLength, 0.0f) * Mathf.Cos(theta) * Mathf.Cos(phi_rad) + new Vector3(0.0f, 0.0f, circularLMLength) * Mathf.Sin(theta);
            var gain = Gain.FocalPoint(FocusPos + circularLMPos);

            _autd.Send(gain, false);   //false->don't wait for the data

            Thread.Sleep(milliseconds);

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
