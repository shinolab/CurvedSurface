using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//using System.Collections.Generic;  // For List

public class Manager1SLC : MonoBehaviour
{
    //private float[] delayArray = { 0.78125f, 0.83815f, 0.416f, 0.3f };
    //private List<float> delayList = new List<float>();

    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }   //private set { this.delay = value; }
    }

    //private float[] halfExploreLengthArray = { 0.025f, 0.005f, 0.003f};
    //private List<float> halfExploreLengthList = new List<float>();

    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }


    float fingerRadius = 0.007f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }

    Vector3 planeYZ = new Vector3(0f, 0.27f, -0.01f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }

    float sphereRadius = 0f;
    float curvedHight = 0f;


    bool flagLM = true;   //true -> Linear, false -> Circular
    public bool FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }




    // Start is called before the first frame update
    void Start()
    {
        delay = 0.78125f;
        halfExploreLength = 0.025f;

    }

    // Update is called once per frame
    void Update()
    {

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (flagLM)
            {
                flagLM = false;
                Debug.Log("Circular");
            }
            else
            {
                flagLM = true;
                Debug.Log("Linear");
            }
        }


        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad2))
        {
            //delay = 0.78125f;
            halfExploreLength = 0.025f;
            curvedHight = 0.025f;
            CalcHightDelay();
            Debug.Log("Manager, Pattern 1:, " + "shpereRadius: " + sphereRadius.ToString("F4") + "delay: " + delay.ToString("F4"));

        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad3))
        {
            halfExploreLength = 0.025f;
            curvedHight = 0.02f;
            CalcHightDelay();
            Debug.Log("Manager, Pattern 2:, " + "shpereRadius: " + sphereRadius.ToString("F4") + "delay: " + delay.ToString("F4"));
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad4))
        {
            halfExploreLength = 0.025f;
            curvedHight = 0.015f;
            CalcHightDelay();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha5) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad5))
        {
            halfExploreLength = 0.025f;
            curvedHight = 0.01f;
            CalcHightDelay();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha6) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad6))
        {
            halfExploreLength = 0.025f;
            curvedHight = 0.005f;
            CalcHightDelay();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha7) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad7))
        {
            halfExploreLength = 0.05f;
            curvedHight = 0.025f;
            CalcHightDelay();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha8) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad8))
        {
            halfExploreLength = 0.0125f;
            curvedHight = 0.00625f;
            curvedHight = 0.0125f;
            CalcHightDelay();
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha9) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad9))
        {

            halfExploreLength = 0.08f;
            curvedHight = 0.04f;
            CalcHightDelay();
        }


    }

    private void CalcHightDelay()   //halfExploreLength, curvedHight -> sphereRadius -> delay
    {
        sphereRadius = (Mathf.Pow(halfExploreLength, 2) + Mathf.Pow(curvedHight, 2)) / (2 * curvedHight);

        delay = sphereRadius / (sphereRadius + fingerRadius);

        //Debug.Log("shpereRadius: " + sphereRadius.ToString("F4") + "delay: " + delay.ToString("F4"));
    }


}
