using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class CurvesController2CPS : MonoBehaviour
{
    private Manager2CPS _manager;

    Vector3 planeYZ;
    float curvesDepth;

    //For CalcCurves
    float halfCurveLength;
    float circlesPosY;

    float curvesRadius;
    float previousCurvesRadius;

    float curvesDis;
    float previousCurvesDis;

    //Change Colors
    int flagLM;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)

    Color pink = new Color32(255, 162, 162, 255); 
    Color blue = new Color32(162, 190, 255, 255);
    Color purple = new Color32(205, 134, 255, 255); 

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2CPS>();

        planeYZ = _manager.PlaneYZ;
        curvesDepth = _manager.CurvesDepth;
        curvesRadius = _manager.CurvesRadius;
        curvesDis = _manager.CurvesDis;
        DetectCurve();
    }

    // Update is called once per frame
    void Update()
    {
        ///Change Colors
        flagLM = _manager.FlagLM;
        switch (flagLM)
        {
            case 0: //Position
                GetComponent<Renderer>().material.color = pink;
                break;
            case 1: //Strength
                GetComponent<Renderer>().material.color = blue;
                break;
            case 2: //Position+Strength
                GetComponent<Renderer>().material.color = purple;
                break;
        }

        ///DetectCurves (when Radius or Distance changes)
        curvesRadius = _manager.CurvesRadius;
        curvesDis = _manager.CurvesDis;
        if ((previousCurvesRadius != curvesRadius) || (previousCurvesDis != curvesDis))
        {
            DetectCurve();
            previousCurvesRadius = curvesRadius;
            previousCurvesDis = curvesDis;
        }

    }

    int Angle1; //単円、始まり（時計回り）
    int Angle2; //２円が重なるときの角度

    int startAngleLeft = 0;  //スタート地点の角度（度）
    int startAngleRight = 0;
    int areaAngle = 0;  //作成する角度（度）

    int quality = 1000;     //180degのときのtriangle数

    Vector3 pos; //位置

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    private void DetectCurve()
    {
        //Get values
        halfCurveLength = _manager.HalfCurveLength;
        circlesPosY = _manager.CirclesPosY;

        ///Detect
        //Make Parameters
        Angle1 = (int)(Mathf.Atan2(curvesRadius - curvesDepth, halfCurveLength) * Mathf.Rad2Deg);
        Angle2 = (int)(Mathf.Acos((curvesDis / 2) / curvesRadius) * Mathf.Rad2Deg);
        startAngleLeft = 180 + Angle1;

        if (curvesDis / 2 < halfCurveLength) //When Stack
        {
            startAngleRight = 180 + Angle2;
            areaAngle = 180 - Angle1 - Angle2;
        }
        else
        {
            startAngleRight = startAngleLeft;
            areaAngle = 180 - 2*Angle1;
        }

        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        float th, v1, v2;
        int max = quality * areaAngle / 180;

        //Left Cylinder
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngleLeft;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curvesRadius - curvesDis / 2, v1 * curvesRadius, 0.2f));
            vertList.Add(new Vector3(v2 * curvesRadius - curvesDis / 2, v1 * curvesRadius, -0.2f));

            if (i <= max - 1)
            {
                triList.Add(2 * i); triList.Add(2 * i + 3); triList.Add(2 * i + 1);
                triList.Add(2 * i); triList.Add(2 * i + 2); triList.Add(2 * i + 3);
            }
        }
        //Right Cylinder
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngleRight;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curvesRadius + curvesDis / 2, v1 * curvesRadius, 0.2f));
            vertList.Add(new Vector3(v2 * curvesRadius + curvesDis / 2, v1 * curvesRadius, -0.2f));

            if ((i <= max - 1)&&(i>0))
            {
                triList.Add(2*max+2 * i); triList.Add(2 * max + 2 * i + 3); triList.Add(2 * max + 2 * i + 1);
                triList.Add(2 * max + 2 * i); triList.Add(2 * max + 2 * i + 2); triList.Add(2 * max + 2 * i + 3);
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
        pos.y = circlesPosY;
        pos.z = planeYZ.z;
        this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }


}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/