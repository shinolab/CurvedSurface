using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;

public class CSVManagerEuroExpt : MonoBehaviour
{
    private ManagerEuroExpt _manager;
    private SphereControllerEuroExpt _sphereController;

    private StreamWriter sw;
    private DateTime now = DateTime.Now;
    bool flagSWClose = false;


    //Infomation
    bool flagLM = false; //false -> Contact
    int sphereRadiusMM = 0;
    int distanceMM = 0;

    int previousDistanceMM = 0;




    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerEuroExpt>();
        _sphereController = GameObject.Find("SphereController").GetComponent<SphereControllerEuroExpt>();

        sw = new StreamWriter(Application.dataPath + "/" + "CSVData" + "/" + @"SaveData" + now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + "_" + now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + ".csv", true, Encoding.GetEncoding("Shift_JIS"));
        string[] s1 = { "Stimulus", "Radius[mm]", "Distance[mm]", "1 or 2" };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
    }
    public void SaveData(string txt1, string txt2, string txt3, string txt4)
    {
        string[] s1 = { txt1, txt2, txt3, txt4 };
        string s2 = string.Join(",", s1);
        sw.WriteLine(s2);
    }

    // Update is called once per frame
    void Update()
    {
        flagLM = _manager.FlagLM;
        sphereRadiusMM = _manager.SphereRadiusMM;
        distanceMM = _sphereController.DistanceMM;
        flagLM = _manager.FlagLM;

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!flagLM)
            {
                SaveData("Contact", sphereRadiusMM.ToString(), previousDistanceMM.ToString(), "1");
                Debug.Log("DataSaved: " + "Contact, Radius: " + sphereRadiusMM.ToString() + "[mm], Dis: " + previousDistanceMM.ToString() + "[mm], 1");

                previousDistanceMM = distanceMM;
            }
            else
            {
                SaveData("Power", sphereRadiusMM.ToString(), previousDistanceMM.ToString(), "1");
                Debug.Log("DataSaved: " + "Power, Radius: " + sphereRadiusMM.ToString() + "[mm], Dis: " + previousDistanceMM.ToString() + "[mm], 1");

                previousDistanceMM = distanceMM;
            }
        }
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!flagLM)
            {
                SaveData("Contact", sphereRadiusMM.ToString(), previousDistanceMM.ToString(), "2");
                Debug.Log("DataSaved: " + "Contact, Radius: " + sphereRadiusMM.ToString() + "[mm], Dis: " + previousDistanceMM.ToString() + "[mm], 2");

                previousDistanceMM = distanceMM;
            }
            else
            {
                SaveData("Power", sphereRadiusMM.ToString(), previousDistanceMM.ToString(), "2");
                Debug.Log("DataSaved: " + "Power, Radius: " + sphereRadiusMM.ToString() + "[mm], Dis: " + previousDistanceMM.ToString() + "[mm], 2");

                previousDistanceMM = distanceMM;
            }
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.E))
        {
            SaveData("Erase", "", "", "");
            Debug.Log("Erase");
        }


        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    sw.Close();
        //    flagSWClose = true;
        //    Debug.Log("DataClosed");
        //}
    }
    private void OnApplicationQuit()
    {
        if (!flagSWClose)
        {
            sw.Close();
        }
    }

}
