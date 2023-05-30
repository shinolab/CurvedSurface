using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class CurveController1BPS : MonoBehaviour
{
    private Manager1BPS _manager;

    Vector3 planeYZ;

    float delay;    
    float halfCurveLength;
    float curveHeightDepth;
    float curveRadius;

    //public GameObject Plane;
    float width = 0.2f; //z方向の奥行の半分

    //flag
    int flagLM = 0;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)
    bool flagVideoConvex = true;

    //for discover change
    float previousDelay = 0f;
    float previousCurveHeightDepth=0f;
    bool previousFlagVideoConvex = true;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1BPS>();

        planeYZ = _manager.PlaneYZ;

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveHeightDepth = _manager.CurveHeightDepth;
        curveRadius = _manager.CurveRadius;

    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;
        curveHeightDepth = _manager.CurveHeightDepth;
        curveRadius = _manager.CurveRadius;
        flagLM = _manager.FlagLM;
        flagVideoConvex = _manager.FlagVideoConvex;

        if ((previousDelay != delay) || (previousCurveHeightDepth != curveHeightDepth) || (previousFlagVideoConvex != flagVideoConvex))
        {
            if (flagVideoConvex)
            {
                CreateConvex();
            }
            else
            {
                CreateConvene();
            }

            previousDelay = delay;
            previousCurveHeightDepth = curveHeightDepth;
            previousFlagVideoConvex = flagVideoConvex;
        }

        ///Change Colors       
        ChangeColors();

    }

    private void ChangeColors()
    {
        switch (flagLM)
        {
            case 0: //Position
            case 3:
                GetComponent<MeshRenderer>().material.color = new Color32(255, 162, 162, 255); //pink
                //GetComponent<MeshRenderer>().material.color = new Color32(56, 56, 56, 255); //gray
                break;
            case 1: //Strength
            case 4:
                GetComponent<MeshRenderer>().material.color = new Color32(162, 190, 255, 255); //blue
                break;
            case 2: //Position+Strength
            case 5:
                GetComponent<MeshRenderer>().material.color = new Color32(205, 134, 255, 255); //purple
                break;
        }
    }

    int reverseAngle = 0; //反時計回りでみたときのスタート地点の角度（度）
    int startAngle = 0;  //スタート地点の角度（度）
    int areaAngle = 0;  //作成する角度（度）

    int quality = 100;     //180degのときのtriangle数
    //bool isOutward = true; //内向きか外向きか //Outward -> convex, Inward -> concave

    Vector3 pos; //位置

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    private void CreateConvex()
    {
        startAngle = (int)(Mathf.Atan2(Mathf.Abs(curveRadius - curveHeightDepth), halfCurveLength) * Mathf.Rad2Deg);
        areaAngle = 180 - 2 * startAngle;

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
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, width) + planeYZ);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, -width) + planeYZ);

            if (i <= max - 1)
            {
                triList.Add(2 * i); triList.Add(2 * i + 1); triList.Add(2 * i + 3);
                triList.Add(2 * i); triList.Add(2 * i + 3); triList.Add(2 * i + 2);
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

        mesh.name = "CurveMesh";
        //transform.localScale = scale;

        //Change Height(Depth) from planeYZ
        pos = this.transform.position;
        pos.y = curveHeightDepth - curveRadius;
        this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }


    private void CreateConvene()
    {
        reverseAngle = (int)(Mathf.Atan2(Mathf.Abs(curveRadius - curveHeightDepth), halfCurveLength) * Mathf.Rad2Deg);
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
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, width) + planeYZ);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, -width) + planeYZ);

            if (i <= max - 1)
            {
                triList.Add(2 * i); triList.Add(2 * i + 3); triList.Add(2 * i + 1);
                triList.Add(2 * i); triList.Add(2 * i + 2); triList.Add(2 * i + 3);
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

        mesh.name = "CurveMesh";
        //transform.localScale = scale;

        //Change Height(Depth) from planeYZ
        pos = this.transform.position;
        pos.y = curveRadius - curveHeightDepth;
        this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312
