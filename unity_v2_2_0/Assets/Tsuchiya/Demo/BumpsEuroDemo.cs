using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;

public class BumpsEuroDemo : MonoBehaviour
{
    private ManagerEuroDemo _manager;

    public GameObject CenterBump = null;

    //General Information
    Vector3 planeYZ;
    float fingerRadius;
    float bumpsHeight;

    bool flagMode = true;   //true -> Adjusting, false -> Presenting
    //Center Bump
    bool flagLM;//true -> Position(pink), false -> Strength(blue)
    Color32 colorPosition= new Color32(255, 162, 162, 255);
    Color32 colorStrength= new Color32(162, 190, 255, 255);

    float bumpsRadius;
    float preBumpsRadius;

    float bumpsPosY;
    Vector3 centerBumpPos;

    //Place Bumps
    float bumpsDistance;
    float preBumpsDistance;

    int bumpsNumber;
    public int BumpsNumber
    {
        get { return this.bumpsNumber; }
        private set { this.bumpsNumber = value; }
    }
    float eachBumpDitance = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroDemo>();

        planeYZ = _manager.PlaneYZ;
        fingerRadius = _manager.FingerRadius;
        bumpsHeight = _manager.BumpsHeight;
        bumpsRadius = _manager.BumpsRadius;
        bumpsDistance = _manager.BumpsDistance;

    }

    // Update is called once per frame
    void Update()
    {
        flagMode = _manager.FlagMode;
        flagLM = _manager.FlagLM;
        bumpsRadius = _manager.BumpsRadius;
        bumpsDistance = _manager.BumpsDistance;

        if ((flagMode == true) && ((preBumpsRadius != bumpsRadius)||(preBumpsDistance != bumpsDistance)))
        {
            DetectCenterBump();
            PlaceBumps();

            preBumpsRadius = bumpsRadius;
            preBumpsDistance = bumpsDistance;
        }





        //if (flagMode == true)//Adjusting
        //{
        //    if(preBumpsRadius != bumpsRadius)//radius changed
        //    {
        //        DetectCenterBump();
        //        preBumpsRadius = bumpsRadius;
        //    }
        //    if (preBumpsDistance != bumpsDistance)//distance changed
        //    {
        //        PlaceBumps();
        //        preBumpsDistance = bumpsDistance;
        //    }
        //}

    }

    private void DetectCenterBump()
    {
        if (flagLM) //Position && color =red
        {
            CenterBump.GetComponent<Renderer>().material.color = colorPosition;
        }
        else //Strength && color = blue
        {
            CenterBump.GetComponent<Renderer>().material.color = colorStrength;
        }

        CenterBump.transform.localScale = new Vector3(bumpsRadius * 2, 0.19f, bumpsRadius * 2);

        bumpsPosY = _manager.BumpsPosY;
        centerBumpPos = CenterBump.transform.position;
        centerBumpPos.y = bumpsPosY;
        CenterBump.transform.position = centerBumpPos;
    }

    private void PlaceBumps()
    {
        int i;
        //Delete Objects
        if (bumpsNumber > 0)
        {
            for (i = 0; i < bumpsNumber; i++)
            {
                Destroy(GameObject.Find("right" + i));
                Destroy(GameObject.Find("left" + i));
            }
        }

        //Place (again)
        bumpsNumber = (int)(0.2f / bumpsDistance) + 1;//original -> 0.173f

        for (i = 0; i < bumpsNumber; i++)
        {
            eachBumpDitance = i * bumpsDistance;

            GameObject rightBumps = Instantiate(CenterBump, new Vector3(centerBumpPos.x + eachBumpDitance, centerBumpPos.y, centerBumpPos.z), Quaternion.Euler(90f, 0.0f, 0f)); //Right
            GameObject leftBumps = Instantiate(CenterBump, new Vector3(centerBumpPos.x - eachBumpDitance, centerBumpPos.y, centerBumpPos.z), Quaternion.Euler(90f, 0.0f, 0f)); //Left

            rightBumps.name = "right" + i;
            leftBumps.name = "left" + i;
        }
    }
}