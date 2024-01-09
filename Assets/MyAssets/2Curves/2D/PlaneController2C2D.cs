using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class PlaneController2C2D : MonoBehaviour
{
    private Manager2C2D _manager;

    float curveDistance;
    float previousCurveDistance = 0f;

    float halfCurveLength;
    float previousHalfCurveLength = 0f;

    Vector3 planeYZ;

    //public GameObject Plane;
    float length = 0.25f; //+x, -xの長さ
    float width = 0.004f; //y方向の幅

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2C2D>();

        planeYZ = _manager.PlaneYZ;

        curveDistance = _manager.CurveDistance;
        halfCurveLength = _manager.HalfCurveLength;


    }

    // Update is called once per frame
    void Update()
    {
        curveDistance = _manager.CurveDistance;
        halfCurveLength = _manager.HalfCurveLength;

        if ((previousCurveDistance != curveDistance) || (previousHalfCurveLength != halfCurveLength))
        {
            DetectPlane();
            previousCurveDistance = curveDistance;
            previousHalfCurveLength = halfCurveLength;
        }

    }

    Color colorPlane = new Color(0.3176471f, 1f, 0.3058824f, 1f); //RGBA
    private void DetectPlane()
    {
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        // +X
        vertList.Add(new Vector3(curveDistance / 2 + halfCurveLength, 0, -0.001f) + planeYZ);               //0
        vertList.Add(new Vector3(curveDistance / 2 + halfCurveLength + length, 0, -0.001f) + planeYZ);      //1
        vertList.Add(new Vector3(curveDistance / 2 + halfCurveLength, -width, -0.001f) + planeYZ);          //2
        vertList.Add(new Vector3(curveDistance / 2 + halfCurveLength + length, -width, -0.001f) + planeYZ); //3

        triList.Add(0);
        triList.Add(1);
        triList.Add(2);

        triList.Add(2);
        triList.Add(1);
        triList.Add(3);

        // Y(-X)
        vertList.Add(new Vector3(-curveDistance / 2 - halfCurveLength - length, 0, -0.001f) + planeYZ);      //4
        vertList.Add(new Vector3(-curveDistance / 2 - halfCurveLength, 0, -0.001f) + planeYZ);               //5
        vertList.Add(new Vector3(-curveDistance / 2 - halfCurveLength - length, -width, -0.001f) + planeYZ); //6
        vertList.Add(new Vector3(-curveDistance / 2 - halfCurveLength, -width, -0.001f) + planeYZ);          //7

        triList.Add(6);
        triList.Add(4);
        triList.Add(5);

        triList.Add(6);
        triList.Add(5);
        triList.Add(7);

        //Y(0)
        if (curveDistance / 2 > halfCurveLength) //When not stack
        {
            vertList.Add(new Vector3(-curveDistance / 2 + halfCurveLength, 0, -0.001f) + planeYZ);      //8
            vertList.Add(new Vector3(curveDistance / 2 - halfCurveLength, 0, -0.001f) + planeYZ);       //9
            vertList.Add(new Vector3(-curveDistance / 2 + halfCurveLength, -width, -0.001f) + planeYZ); //10
            vertList.Add(new Vector3(curveDistance / 2 - halfCurveLength, -width, -0.001f) + planeYZ);  //11

            triList.Add(10);
            triList.Add(8);
            triList.Add(9);

            triList.Add(10);
            triList.Add(9);
            triList.Add(11);
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

        mesh.name = "PlaneMesh";

        GetComponent<MeshFilter>().sharedMesh = mesh;

        // 色指定
        GetComponent<MeshRenderer>().material.color = colorPlane;
    }
}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312