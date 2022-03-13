using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerController1SLC : MonoBehaviour
{
    private Manager1SLC _manager;
    private SphereController1SLC _sphereController;
    private AUTDController1SLC _autdController;

    float delay;
    float halfExploreLength;

    Vector3 planeYZ;

    float fingerPosX;

    float sphereRadius;
    float spherePosY;

    public GameObject FingerReal = null;


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1SLC>();

        _sphereController = GameObject.Find("Sphere").GetComponent<SphereController1SLC>();

        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDController1SLC>();

        delay = _manager.Delay;
        halfExploreLength = _manager.HalfExploreLength;

        planeYZ = _manager.PlaneYZ;
    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfExploreLength = _manager.HalfExploreLength;

        sphereRadius = _sphereController.SphereRadius;

        spherePosY = _sphereController.SpherePosY;

        fingerPosX = _autdController.FingerPosX;

        DetectFinger();
    }

    Vector3 posFingerSphere;
    Vector3 posFingerReal;

    private void DetectFinger()
    {
        posFingerSphere = this.transform.position;
        posFingerReal = FingerReal.transform.position;

        posFingerSphere.x = fingerPosX;
        posFingerReal.x = fingerPosX;

        if (Mathf.Abs(fingerPosX * delay) <= halfExploreLength)   //When Curved Surface
        {
            posFingerSphere.y = Mathf.Sqrt(Mathf.Pow(sphereRadius, 2) - Mathf.Pow(posFingerSphere.x * delay, 2)) + spherePosY;

            //Debug.Log("sphereRadius: " + sphereRadius.ToString("F4")+ "fingerPosX: " + fingerPosX.ToString("F4") + "spherePosY: " + spherePosY.ToString("F4") + "posFinger.y: " + posFinger.y.ToString("F4"));
            //Debug.Log("Curved, " + "posFinger.y: " + posFinger.y.ToString("F4"));
        }
        else //When Plane
        {
            posFingerSphere.y = planeYZ.y; //YZ.y + radius of the figner
            //Debug.Log("Plane, " +"posFinger.y: " + posFinger.y.ToString("F4"));
        }

        this.transform.position = posFingerSphere;
        FingerReal.transform.position = posFingerReal;

    }

}
