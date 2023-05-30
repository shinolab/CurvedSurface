using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class PlaneController2CPS : MonoBehaviour
{
    private Manager2CPS _manager;

    float curvesDis;
    float previousCurvesDis = 0f;

    float halfCurveLength;
    float previousHalfCurveLength=0f;

    Vector3 planeYZ;



    //public GameObject Plane;
    float length = 0.4f; //x方向の長さの半分
    float width = 0.2f; //z方向の奥行の半分

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager2CPS>();

        planeYZ = _manager.PlaneYZ;

        curvesDis = _manager.CurvesDis;
        halfCurveLength = _manager.HalfCurveLength;


    }

    // Update is called once per frame
    void Update()
    {
        curvesDis = _manager.CurvesDis;
        halfCurveLength = _manager.HalfCurveLength;

        if ((previousCurvesDis != curvesDis) || (previousHalfCurveLength != halfCurveLength))
        {
            DetectPlane();
            previousCurvesDis = curvesDis;
            previousHalfCurveLength = halfCurveLength;
        }

    }

    Vector3 pos; //位置

    Color colorPlane = new Color(0.3176471f, 1f, 0.3058824f, 1f); //RGBA
    private void DetectPlane()
    {
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        // Y(+X)
        vertList.Add(new Vector3(curvesDis/2+halfCurveLength, 0, -width) + planeYZ);    //0
        vertList.Add(new Vector3(curvesDis / 2 + halfCurveLength, 0, width) + planeYZ); //1
        vertList.Add(new Vector3(length, 0, width) + planeYZ);                          //2
        vertList.Add(new Vector3(length, 0, -width) + planeYZ);                         //3

        triList.Add(0);
        triList.Add(1);
        triList.Add(2);

        triList.Add(2);
        triList.Add(3);
        triList.Add(0);

        // Y(-X)
        vertList.Add(new Vector3(-length, 0, -width) + planeYZ);                         //4
        vertList.Add(new Vector3(-length, 0, width) + planeYZ);                          //5
        vertList.Add(new Vector3(-curvesDis / 2  -halfCurveLength, 0, width) + planeYZ); //6
        vertList.Add(new Vector3(-curvesDis / 2 - halfCurveLength, 0, -width) + planeYZ);//7

        triList.Add(4);
        triList.Add(5);
        triList.Add(6);

        triList.Add(6);
        triList.Add(7);
        triList.Add(4);

        //Y(0)
        if (curvesDis / 2 > halfCurveLength) //When not stack
        {
            vertList.Add(new Vector3(-curvesDis / 2 + halfCurveLength, 0, -width) + planeYZ); //8
            vertList.Add(new Vector3(-curvesDis / 2 + halfCurveLength, 0, width) + planeYZ);  //9
            vertList.Add(new Vector3(curvesDis / 2 - halfCurveLength, 0, width) + planeYZ);   //10
            vertList.Add(new Vector3(curvesDis / 2 - halfCurveLength, 0, -width) + planeYZ);  //11

            triList.Add(8);
            triList.Add(9);
            triList.Add(10);

            triList.Add(10);
            triList.Add(11);
            triList.Add(8);
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
