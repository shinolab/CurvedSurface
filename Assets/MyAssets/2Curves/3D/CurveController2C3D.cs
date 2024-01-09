using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class CurveController2C3D : MonoBehaviour
{
    private Manager2C3D _manager;

    Vector3 planeYZ;

    float halfCurveLength;
    float curveHeightDepth;
    float curveRadius;
    float curveDistance;

    //public GameObject Plane;
    float width = 0.1f; //z方向の奥行の半分

    //flag
    int flagLM = 0;   //0 -> Position(pink), 1 -> Strength(blue), 2 ->Position+Strength(purple)
    bool flagVideoConvex = true;

    //for discover change
    float previousCurveRadius = 0f;
    float previousCurveDistance = 0f;
    bool previousFlagVideoConvex = true;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2C3D>();

        planeYZ = _manager.PlaneYZ;

        halfCurveLength = _manager.HalfCurveLength;
        curveHeightDepth = _manager.CurveHeightDepth;
        curveRadius = _manager.CurveRadius;
        curveDistance = _manager.CurveDistance;

    }

    // Update is called once per frame
    void Update()
    {
        halfCurveLength = _manager.HalfCurveLength;
        curveHeightDepth = _manager.CurveHeightDepth;
        curveRadius = _manager.CurveRadius;
        curveDistance = _manager.CurveDistance;
        flagLM = _manager.FlagLM;
        flagVideoConvex = _manager.FlagVideoConvex;

        if ((previousCurveRadius != curveRadius) || (previousCurveDistance != curveDistance) || (previousFlagVideoConvex != flagVideoConvex))
        {
            if (flagVideoConvex)
            {
                CreateConvex();
            }
            else
            {
                CreateConvene();
            }

            previousCurveRadius = curveRadius;
            previousCurveDistance = curveDistance;
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

    int Angle1; //単円、始まり（時計回り）
    int Angle2; //２円が重なるときの角度

    int startAngleLeft = 0;  //スタート地点の角度（度）
    int startAngleRight = 0;
    int areaAngle = 0;  //作成する角度（度）

    int quality = 1000;     //180degのときのtriangle数

    Vector3 pos; //位置

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    private void CreateConvex()
    {
        //Get values
        halfCurveLength = _manager.HalfCurveLength;

        ///Detect
        //Make Parameters
        Angle1 = (int)(Mathf.Atan2(curveRadius - curveHeightDepth, halfCurveLength) * Mathf.Rad2Deg);
        Angle2 = (int)(Mathf.Acos((curveDistance / 2) / curveRadius) * Mathf.Rad2Deg);
        startAngleRight = Angle1;

        if (curveDistance / 2 < halfCurveLength) //When Stack
        {
            startAngleLeft = Angle2;
            areaAngle = 180 - Angle1 - Angle2;
        }
        else
        {
            startAngleLeft = startAngleRight;
            areaAngle = 180 - 2 * Angle1;
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
            vertList.Add(new Vector3(v2 * curveRadius - curveDistance / 2, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius - curveDistance / 2, v1 * curveRadius, -width));

            if (i <= max - 1)
            {
                triList.Add(2 * i); triList.Add(2 * i + 1); triList.Add(2 * i + 3);
                triList.Add(2 * i); triList.Add(2 * i + 3); triList.Add(2 * i + 2);
            }
        }

        //Right Cylinder
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngleRight;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curveRadius + curveDistance / 2, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius + curveDistance / 2, v1 * curveRadius, -width));

            if ((i <= max - 1) && (i > 0))
            {
                triList.Add(2 * max + 2 * i); triList.Add(2 * max + 2 * i + 1); triList.Add(2 * max + 2 * i + 3);
                triList.Add(2 * max + 2 * i); triList.Add(2 * max + 2 * i + 3); triList.Add(2 * max + 2 * i + 2);
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

        //Change Height(Depth) from planeYZ
        pos = this.transform.position;
        pos.y = planeYZ.y - curveRadius + curveHeightDepth;
        pos.z = planeYZ.z;
        this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }


    private void CreateConvene()
    {
        //Get values
        halfCurveLength = _manager.HalfCurveLength;

        ///Detect
        //Make Parameters
        Angle1 = (int)(Mathf.Atan2(curveRadius - curveHeightDepth, halfCurveLength) * Mathf.Rad2Deg);
        Angle2 = (int)(Mathf.Acos((curveDistance / 2) / curveRadius) * Mathf.Rad2Deg);
        startAngleLeft = 180 + Angle1;

        if (curveDistance / 2 < halfCurveLength) //When Stack
        {
            startAngleRight = 180 + Angle2;
            areaAngle = 180 - Angle1 - Angle2;
        }
        else
        {
            startAngleRight = startAngleLeft;
            areaAngle = 180 - 2 * Angle1;
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
            vertList.Add(new Vector3(v2 * curveRadius - curveDistance / 2, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius - curveDistance / 2, v1 * curveRadius, -width));

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
            vertList.Add(new Vector3(v2 * curveRadius + curveDistance / 2, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius + curveDistance / 2, v1 * curveRadius, -width));

            if ((i <= max - 1) && (i > 0))
            {
                triList.Add(2 * max + 2 * i); triList.Add(2 * max + 2 * i + 3); triList.Add(2 * max + 2 * i + 1);
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

        //Change Height(Depth) from planeYZ
        pos = this.transform.position;
        pos.y = planeYZ.y + curveRadius - curveHeightDepth;
        pos.z = planeYZ.z;
        this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312