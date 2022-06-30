using AUTD3Sharp;
using UnityEngine;

using System.Threading;
using System.Threading.Tasks;


public class Impact : MonoBehaviour
{
    //AUTD
    Controller _autd = new Controller();
    Link _link = null;
    public Transform[] transforms = null;

    SilencerConfig config = SilencerConfig.None();

    //For Thread
    bool flag = true;

    //For Get Value from Object
    private Object _object;
    bool onCollision = false; //true -> Collision

    //public GameObject? Target = null;
    Vector3 YZ = new Vector3(0f, 0.30f, 0f); //y=0.27f, z=-0.01f


    //Freqs of Materials
    static int freqMetal=2000;   //Original->2000
    static int freqWood = 250;   //Original->250
    static int freqRubber = 100; //Original->100
    int[] freqMaterial = {freqMetal, freqWood, freqRubber};

    int freqNow = freqMetal;

    //Damping rates of Materials
    static float dampMetal = 2.5f;  //Original->30
    static float dampWood = 3f;   //Original->40
    static float dampRubber = 2f; //Original->40
    float[] dampMaterial = { dampMetal, dampWood, dampRubber };

    float dampNow = dampMetal;

    float amplitude;

    //Time Management
    float timeCollision;


    void Start()
    {
        StartAUTD();

        _object = GameObject.Find("Object").GetComponent<Object>();

        //Task nowait = AUTDWork();  //Prepare Thread

        Debug.Log("Material: Metal");

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

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) )
        {
            freqNow = freqMaterial[0];
            dampNow = dampMaterial[0];
            Debug.Log("Material: Metal");
            return;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {
            freqNow = freqMaterial[1];
            dampNow = dampMaterial[1];
            Debug.Log("Material: Wood");
            return;
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
        {
            freqNow = freqMaterial[2];
            dampNow = dampMaterial[2];
            Debug.Log("Material: Rubber");
            return;
        }


        timeCollision += Time.deltaTime;

        //Introduce Damping       
        if (dampNow == dampMaterial[1])
        {
            amplitude = 0.5f * Mathf.Exp(-dampNow * timeCollision);
        }
        else
        {
            amplitude = Mathf.Exp(-dampNow * timeCollision);
        }

        _autd.Send(new Focus(YZ, amplitude));

        if (onCollision == true)
        {
            timeCollision = 0f;
            _autd.Send(new Sine(freqNow, 0.5f)); // 150 Hz
            _autd.Send(new Focus(YZ, 1));

        }


        //radiate for 0.3s
        //if (onCollision == true)
        //{
        //    timeCollision = 0f;
        //    _autd.Send(new Sine(freqNow, 0.5f)); // 150 Hz
        //    _autd.Send(new Focus(YZ, 1));

        //}
        //if (timeCollision >= 0.3f)
        //{
        //    _autd.Send(new Static(0.0));
        //}



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