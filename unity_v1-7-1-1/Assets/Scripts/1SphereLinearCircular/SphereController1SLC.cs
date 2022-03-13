using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class SphereController1SLC : MonoBehaviour
{
    private Manager1SLC _manager;

    float delay;
    float previousDelay = 0f;

    float halfExploreLength;
    float previousHalfExploreLength;


    //bool delayChanged=false;

    float fingerRadius;

    Vector3 planeYZ;



    float curvedHight;
    public float CurvedHight
    {
        get { return this.curvedHight; }
        private set { this.curvedHight = value; }
    }

    //float switchX; //for CalcSphere_Rejected1();

    float sphereRadius;
    public float SphereRadius
    {
        get { return this.sphereRadius; }
        private set { this.sphereRadius = value; }
    }

    float spherePosY;
    public float SpherePosY
    {
        get { return this.spherePosY; }
        private set { this.spherePosY = value; }
    }



    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1SLC>();

        delay = _manager.Delay;
        halfExploreLength = _manager.HalfExploreLength;

        planeYZ = _manager.PlaneYZ;

        fingerRadius = _manager.FingerRadius;
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfExploreLength = _manager.HalfExploreLength;

        if ((previousDelay != delay) || (previousHalfExploreLength != halfExploreLength))
        {
            CalcSphere();
            DetectSphre();

            //Debug.Log("Sphere, delay: " + delay.ToString("F4")+"halfExploreLength: " + halfExploreLength.ToString("F4"));
            previousDelay = delay;
            previousHalfExploreLength = halfExploreLength;
        }

    }

    private void CalcSphere()
    {

        sphereRadius = delay / (1 - delay) * fingerRadius;

        if (Mathf.Pow(sphereRadius, 2) < Mathf.Pow(halfExploreLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            spherePosY = planeYZ.y;
        }
        else
        {
            spherePosY = planeYZ.y - Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(halfExploreLength, 2));
        }
        curvedHight = sphereRadius - planeYZ.y + spherePosY;

        Debug.Log("Sphere, delay: " + delay.ToString("F4") + "halfExploreLength" + halfExploreLength.ToString("F4") + "planeYZ.y" + planeYZ.y.ToString("F4") + "sphereRadius" + sphereRadius.ToString("F4") + ", spherePosY" + spherePosY.ToString("F4") + ", curvedHight" + curvedHight.ToString("F8"));

    }

    Vector3 pos;
    private void DetectSphre()
    {
        pos = this.transform.position;
        pos.y = spherePosY;
        this.transform.position = pos;

        this.transform.localScale = new Vector3(sphereRadius * 2, sphereRadius * 2, sphereRadius * 2);

        //Debug.Log("shperePosY: " +  spherePosY.ToString("F4"));

    }


    //private void CalcSphere_Rejected2()
    //{

    //    sphereRadius = fingerRadius / (1 - delay);

    //    diameter = 2 * sphereRadius;

    //    spherePosY = planeYZ.y - sphereRadius + curvedHight;

    //}


    //private void CalcSphere_Rejected1()
    //{
    //    switchX = (float)0.007 / (1 - delay);

    //    sphereRadius = (Mathf.Pow(switchX, 2) + Mathf.Pow(curvedHight, 2)) / (2.0f * curvedHight);
    //    diameter = 2 * sphereRadius;

    //    spherePosY = 0.223f - sphereRadius + curvedHight;
    //}




}
