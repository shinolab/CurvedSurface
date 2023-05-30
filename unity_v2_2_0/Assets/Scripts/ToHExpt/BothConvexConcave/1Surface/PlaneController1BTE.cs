using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class PlaneController1BTE : MonoBehaviour
{
    private Manager1BTE _manager;

    float delay;
    float previousDelay = 0f;

    float halfCurveLength;
    float previousCurveLength;

    Vector3 planeYZ;



    //public GameObject Plane;
    float length = 0.4f; //x方向の長さの半分
    float width = 0.2f; //z方向の奥行の半分


    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<Manager1BTE>();

        planeYZ = _manager.PlaneYZ;

        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;


    }

    // Update is called once per frame
    void Update()
    {
        delay = _manager.Delay;
        halfCurveLength = _manager.HalfCurveLength;

        if ((previousDelay != delay) || (previousCurveLength != halfCurveLength))
        {
            DetectPlane();
        }

    }

    Vector3 pos; //位置

    Color colorPlane = new Color(0.3176471f, 1f, 0.3058824f, 1f); //RGBA
    private void DetectPlane()
    {
        // 頂点の配列
        var vertices = new Vector3[8];

        // 三角形の配列
        var tri = new int[12];

        // Y(+X)
        vertices[0] = new Vector3(halfCurveLength, 0, -width)+planeYZ;
        vertices[1] = new Vector3(halfCurveLength, 0, width) + planeYZ;
        vertices[2] = new Vector3(length, 0, width) + planeYZ;
        vertices[3] = new Vector3(length, 0, -width) + planeYZ;

        tri[0] = 0;
        tri[1] = 1;
        tri[2] = 2;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 0;

        // Y(-X)
        vertices[4] = new Vector3(-length, 0, -width) + planeYZ;
        vertices[5] = new Vector3(-length, 0, width) + planeYZ;
        vertices[6] = new Vector3(-halfCurveLength, 0, width) + planeYZ;
        vertices[7] = new Vector3(-halfCurveLength, 0, -width) + planeYZ;

        tri[6] = 4;
        tri[7] = 5;
        tri[8] = 6;

        tri[9] = 6;
        tri[10] = 7;
        tri[11] = 4;

        //Set Parameters
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = tri;

        // 法線とバウンディングの計算
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mesh.name = "PlaneMesh";
        //transform.localScale = scale;

        //pos = this.transform.position;
        //pos.y = planeYZ.y;
        //this.transform.position = pos;

        GetComponent<MeshFilter>().sharedMesh = mesh;
        //GetComponent<MeshCollider>().sharedMesh = mesh;

        // 色指定
        GetComponent<MeshRenderer>().material.color = colorPlane;

    }



}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312
