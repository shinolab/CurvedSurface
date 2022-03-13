using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using DG.Tweening;


//using System.Collections.Generic;  // For List

public class ManagerEuroExpt : MonoBehaviour
{
    Vector3 planeYZ = new Vector3(0f, 0.27f, -0.01f);   //y=0.26f, z=0.013f
    public Vector3 PlaneYZ
    {
        get { return this.planeYZ; }
        private set { this.planeYZ = value; }
    }

    float delay;
    public float Delay
    {
        get { return this.delay; }
        private set { this.delay = value; }
    }

    float halfCurveLength;
    public float HalfCurveLength
    {
        get { return this.halfCurveLength; }
        private set { this.halfCurveLength = value; }
    }


    float fingerRadius = 0.007f;
    public float FingerRadius
    {
        get { return this.fingerRadius; }
        private set { this.fingerRadius = value; }
    }


    //For Sphere Controller
    int sphereRadiusMM = 0;
    public int SphereRadiusMM
    {
        get { return this.sphereRadiusMM; }
        private set { this.sphereRadiusMM = value; }
    }
    float sphereRadius = 0f;
    public float SphereRadius
    {
        get { return this.sphereRadius; }
        private set { this.sphereRadius = value; }
    }


    float curveHight = 0.005f;

    //For AUTD Controller
    float halfExploreLength;
    public float HalfExploreLength
    {
        get { return this.halfExploreLength; }
        private set { this.halfExploreLength = value; }
    }

    bool flagLM = true;   //true -> Power, false -> Contact, LinearLM
    public bool FlagLM
    {
        get { return this.flagLM; }
        private set { this.flagLM = value; }
    }




    //For Experiment
    public Text textRadius;



    // Start is called before the first frame update
    void Start()
    {
        //curveHight = 0.005f;

        sphereRadiusMM = 5;
        sphereRadius = 0.005f;

        CalcCurvedLength();
        CalcHalfExploreLength();

        //Experiment
        textRadius = textRadius.GetComponent<Text>();

        textRadius.text = "Radius: " + sphereRadiusMM.ToString() + "mm";

    }




    // Update is called once per frame
    void Update()
    {

        if ((UnityEngine.Input.GetKeyDown(KeyCode.Alpha0) || UnityEngine.Input.GetKeyDown(KeyCode.Keypad0)))
        {
            if (flagLM)
            {
                flagLM = false;
                Debug.Log("Position");
            }
            else
            {
                flagLM = true;
                Debug.Log("Strength");
            }
        }


    }

    private void CalcCurvedLength() //sphereRadious, curvedHight -> halfCurveLength
    {
        if (2 * sphereRadius * curveHight > Mathf.Pow(curveHight, 2))
        {
            halfCurveLength = Mathf.Sqrt(2 * sphereRadius * curveHight - Mathf.Pow(curveHight, 2));
        }
        else
        {
            halfCurveLength = 0;
            Debug.Log("halfCurveLength=0");
        }

        delay = sphereRadius / (sphereRadius + fingerRadius);

    }

    private void CalcHalfExploreLength()
    {
        halfExploreLength = halfCurveLength / delay;
    }

}
