using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;




public class DampingLM : MonoBehaviour
{
    //AUTD
    Controller _autd = new Controller();
    Link _link = null;
    public Transform[] transforms = null;

    //Send fuctions
    SilencerConfig config = SilencerConfig.None();

    //For Thread
    bool flag = true;

    //For Get Value from Object
    private Object _object;
    bool onCollision = false; //true -> Collision

    //public GameObject? Target = null;
    Vector3 YZ = new Vector3(0f, 0.30f, 0f); //y=0.27f, z=-0.01f


    //Freqs of Materials
    static int freqMetal = 1000;   //Original->2000
    static int freqWood = 250;   //Original->250
    static int freqRubber = 100; //Original->100
    int[] freqMaterial = { freqMetal, freqWood, freqRubber };

    int freqNow = freqMetal;

    //Damping rates of Materials
    static float dampMetal = 25f;  //Original->30
    static float dampWood = 15f;   //Original->40
    static float dampRubber = 10f; //Original->40
    float[] dampMaterial = { dampMetal, dampWood, dampRubber };

    float dampNow = dampMetal;

    float amplitude = 0f;

    //STM
    PointSTM _stmMetal = new PointSTM();
    PointSTM _stmWood = new PointSTM();
    PointSTM _stmRubber=new PointSTM();
    
    float radius = 0.003f; //3 mm


    //Time Management
    float timeCollision;

    //Swich AM <-> LM
    bool flagAMLM = true; //true->AM, false->LM


    void Start()
    {
        StartAUTD();

        _object = GameObject.Find("Object").GetComponent<Object>();

        //Task nowait = AUTDWork();  //Prepare Thread

        _autd.Send(new Sine(freqNow, 0f));
        _autd.Send(new Focus(YZ,0));
        Debug.Log("Material: Metal");

        CalcSTM();

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

        _autd.CheckAck = true;

        _autd.Clear();

        _autd.Synchronize();

        _autd.Send(config);

    }

    async Task AUTDWork()
    {
        await Task.Run(() =>
        {
            //Debug.Log("Thread ID:" + Thread.CurrentThread.ManagedThreadId);
            while (flag)
            {
                //if (onCollision == true)
                //{
                //    _autd.Send(new Sine(freqNow, 0.5f)); // 150 Hz
                //    _autd.Send(new Focus(YZ, 1));

                //}

            }

        });
    }

    void Update()
    {
        onCollision = _object.OnCollision;
        //Debug.Log("AUTD, onCollision: "+onCollision);

        ChangeMateials();

        //Introduce Damping       
        amplitude = Mathf.Exp(-dampNow * timeCollision);

        if (flagAMLM) //AM
        {
            _autd.Send(new Sine(freqNow, amplitude));
        }
        else //LM
        {
            _autd.Send(new Static(amplitude));
        }

        //Time Management
        timeCollision += Time.deltaTime;
        if (onCollision == true)
        {
            timeCollision = 0f;
            if (flagAMLM) //AM
            {
                _autd.Send(new Sine(freqNow, 1));
            }
            else //LM
            {
                _autd.Send(new Static(1));
            }
        }

        //radiate for 0.3s
        //if (onCollision == true)
        //{
        //    timeCollision = 0f;
        //    //_autd.Send(new Sine(freqNow, 0.5f)); // 150 Hz
        //    //_autd.Send(new Focus(YZ, 1));

        //    _autd.Send(new Static(1));
        //   // _autd.Send(_wavRubber);

            
        //}
        //if (timeCollision >= 0.5f)
        //{
        //    _autd.Send(_static0);
        //}



    }

    private void ChangeMateials()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1)) //Metal
        {
            freqNow = freqMaterial[0];
            dampNow = dampMaterial[0];
            _autd.Send(new Focus(YZ, amplitude));
            flagAMLM = true;
            Debug.Log("Material: Metal");
            return;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2)) //Wood
        {
            freqNow = freqMaterial[1];
            dampNow = dampMaterial[1];
            _autd.Send(new Focus(YZ, amplitude));
            flagAMLM = true;
            Debug.Log("Material: Wood");
            return;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3)) //Rubber
        {
            freqNow = freqMaterial[2];
            dampNow = dampMaterial[2];
            _autd.Send(new Focus(YZ, amplitude));
            flagAMLM = true;
            Debug.Log("Material: Rubber");
            return;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4)) //Metal STM
        {
            _autd.Send(_stmMetal);
            dampNow = dampMaterial[0];
            flagAMLM = false;
            Debug.Log("Material: Metal, STM");
            return;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5)) //Wood STM
        {
            _autd.Send(_stmWood);
            dampNow = dampMaterial[1];
            flagAMLM = false;
            Debug.Log("Material: Wood, STM");
            return;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6)) //Rubber STM
        {
            _autd.Send(_stmRubber);
            dampNow = dampMaterial[2];
            flagAMLM = false;
            Debug.Log("Material: Rubber, STM");
            return;
        }
    }

    private void CalcSTM() //100Hz
    {
        const int pointNum = 10;
        for (var i = 0; i < pointNum; i++)
        {
            var theta = 2 * Mathf.PI * i / pointNum;
            var p = radius * new Vector3(0, 0, Mathf.Sin(theta));
            _stmMetal.Add(YZ + p);
            _stmWood.Add(YZ + p);
            _stmRubber.Add(YZ + p);
            
        }
        _stmMetal.Frequency = freqMetal;
        _stmWood.Frequency = freqWood;
        _stmRubber.Frequency = freqRubber;        
        //Debug.Log($"Actual frequency is {stmRubber.Frequency}");
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