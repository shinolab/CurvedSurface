using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FingerEuroDemo : MonoBehaviour
{
    private ManagerEuroDemo _manager;
    //private BumpsEuroDemo _bumpsController;
    private AUTDEuroDemo _autdController;

    //General Information
    Vector3 planeYZ;

    //Manager
    bool flagMode;
    float halfExploreLength;
    float bumpsRadius;
    float bumpsPosY;


    //AUTD Controller
    float fingerPosX;
    float focusPosX;
    float bumpPosX;

    public GameObject FingerBumps = null;
    Vector3 posFingerBumps;




    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo>();
        //_bumpsController = GameObject.Find("BumpsController").GetComponent<BumpsEuroDemo>();
        _autdController = GameObject.Find("AUTDController").GetComponent<AUTDEuroDemo>();

        planeYZ = _manager.PlaneYZ;

    }

    // Update is called once per frame
    void Update()
    {
        flagMode = _manager.FlagMode;
        halfExploreLength = _manager.HalfExploreLength;
        bumpsRadius = _manager.BumpsRadius;
        bumpsPosY = _manager.BumpsPosY;

        fingerPosX = _autdController.FingerPosX;
        focusPosX = _autdController.FocusPosX;
        bumpPosX = _autdController.BumpPosX;

        DisplayFinger();
    }


    private void DisplayFinger()//Figner Bump
    {         
        posFingerBumps = FingerBumps.transform.position;

        if (flagMode == true) //When Adjusting
        {
            posFingerBumps.x = 1;
        }
        else   ////When Presenting
        {
            posFingerBumps.x = fingerPosX;

            if ((Mathf.Abs(fingerPosX - bumpPosX) <= halfExploreLength))   //When Curved Surface
            {
                if (Mathf.Pow(bumpsRadius, 2) - Mathf.Pow(focusPosX - bumpPosX, 2) <= 0)
                {
                    posFingerBumps.y = bumpsPosY;
                }
                else
                {
                    posFingerBumps.y = Mathf.Sqrt(Mathf.Pow(bumpsRadius, 2) - Mathf.Pow(focusPosX - bumpPosX, 2)) + bumpsPosY;
                }
            }
            else //When Plane
            {
                posFingerBumps.y = planeYZ.y; //YZ.y + radius of the figner
            }
        }
        FingerBumps.transform.position = posFingerBumps;
        //Debug.Log("posFinger_X" + posFingerBumps.x.ToString("F4") + ", posFinger_Y" + posFingerBumps.y.ToString("F4"));

    }

}
