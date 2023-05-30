using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class ConcaveController1CTE : MonoBehaviour
{
    private Manager1CTE _manager;

    float delay;
    float previousDelay = 0f;

    float halfCurveLength;
    float previousCurveRadius;

    Vector3 planeYZ;


    float curveDepth;

    float curveRadius;


    float deltaCirclePosY; //planeYZ.y-circlePosY
    public float DeltaCirclePosY
    {
        get { return this.deltaCirclePosY; }
        private set { this.deltaCirclePosY = value; }
    }

    //public GameObject Plane;
    float width = 0.2f; //z方向の奥行の半分

    //Change Colors
    int flagLM = 0;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1CTE>();

        planeYZ = _manager.PlaneYZ;

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveDepth = _manager.CurveDepth;
        curveRadius = _manager.CurveRadius;

    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveDepth = _manager.CurveDepth;
        curveRadius = _manager.CurveRadius;

        if ((previousDelay != delay) || (previousCurveRadius != curveRadius))
        {
            CalcCircle();
            DetectConvene();

            //Debug.Log("Circle, delay: " + delay.ToString("F4")+"halfExploreLength: " + halfExploreLength.ToString("F4"));
            previousDelay = delay;
            previousCurveRadius = curveRadius;
        }

        ///Change Colors
        flagLM = _manager.FlagLM;
        ChangeColors();

    }

    private void ChangeColors()
    {
        switch (flagLM)
        {
            case 0: //Position
                GetComponent<MeshRenderer>().material.color = new Color32(255, 162, 162, 255); //pink
                break;
            case 1: //Strength
                GetComponent<MeshRenderer>().material.color = new Color32(162, 190, 255, 255); //blue
                break;
            case 2: //Position+Strength
                GetComponent<MeshRenderer>().material.color = new Color32(205, 134, 255, 255); //purple
                break;
        }
    }

    private void CalcCircle()
    {

        if (Mathf.Pow(curveRadius, 2) < Mathf.Pow(halfCurveLength, 2)) // Inside of SQRT < 0  ->  Errpor
        {
            //circlePosY = planeYZ.y;
            deltaCirclePosY = 0f;
        }
        else
        {
            //circlePosY = planeYZ.y + Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(curveLength, 2));
            deltaCirclePosY = Mathf.Sqrt(Mathf.Pow(curveRadius, 2) - Mathf.Pow(halfCurveLength, 2));
        }

        //Debug.Log("deltaCirclePosY" + deltaCirclePosY.ToString("F4"));
    }

    int reverseAngle = 0; //反時計回りでみたときのスタート地点の角度（度）
    int startAngle = 0;  //スタート地点の角度（度）
    int areaAngle = 0;  //作成する角度（度）

    int quality = 100;     //180degのときのtriangle数
    bool isOutward = true; //内向きか外向きか
    Color colorConvene = new Color(0.6352941f, 0.7450981f, 1f, 1f); //RGBA

    Vector3 pos; //位置

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    private void DetectConvene()
    {
        reverseAngle = (int)(Mathf.Atan2(curveRadius - curveDepth, halfCurveLength) * Mathf.Rad2Deg);
        startAngle = 180 + reverseAngle;
        areaAngle = 180 - 2 * reverseAngle;

        //Make Parameters
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        //Cylinder
        float th, v1, v2;
        int max = quality * areaAngle / 180;
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngle;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, width)+planeYZ);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, -width) + planeYZ);


            if (i <= max - 1)
            {
                if (isOutward)
                {
                    triList.Add(2 * i); triList.Add(2 * i + 3); triList.Add(2 * i + 1);
                    triList.Add(2 * i); triList.Add(2 * i + 2); triList.Add(2 * i + 3);
                }
                else
                {
                    triList.Add(2 * i); triList.Add(2 * i + 1); triList.Add(2 * i + 3);
                    triList.Add(2 * i); triList.Add(2 * i + 3); triList.Add(2 * i + 2);
                }
            }

        }

        vertices = vertList.ToArray();
        triangles = triList.ToArray();

        //Set Parameters
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // 法線とバウンディングの計算
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mesh.name = "ConcaveMesh";
        //transform.localScale = scale;

        pos = this.transform.position;
        pos.y = deltaCirclePosY;
        this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
        //GetComponent<MeshCollider>().sharedMesh = mesh;

        // 色指定
        //GetComponent<MeshRenderer>().material.color = colorConvene;
    }



}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312
