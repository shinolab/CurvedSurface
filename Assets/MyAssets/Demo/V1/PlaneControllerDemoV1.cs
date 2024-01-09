using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEditor;
using UnityEngine.UIElements;
using System.Drawing;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class PlaneControllerDemoV1 : MonoBehaviour
{
    private ManagerDemoV1 _manager;

    float curveDistance;
    float previousCurveDistance = 0f;

    float halfCurveLength;
    float previousHalfCurveLength = 0f;

    Vector3 planeYZ;

    bool flagStack;
    int numberPlacement;

    //public GameObject Plane;
    UnityEngine.Color colorPlane = new UnityEngine.Color32(71, 183, 69, 255); //RGBA
    float halfPlaneLength;

    float width = 0.3f; //z方向の奥行の半分

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerDemoV1>();

        planeYZ = _manager.PlaneYZ;

        curveDistance = _manager.CurveDistance;
        halfCurveLength = _manager.HalfCurveLength;


    }

    // Update is called once per frame
    void Update()
    {
        flagStack = _manager.FlagStack;
        if (!flagStack)
        {
            curveDistance = _manager.CurveDistance;
            halfCurveLength = _manager.HalfCurveLength;
            numberPlacement = _manager.NumberPlacement;

            PlacePlane();
        }
        else
        {
            foreach (GameObject oldMesh in GameObject.FindGameObjectsWithTag("PlaneMeshTag"))
            {
                Destroy(oldMesh);
            }
        }

    }


    private void PlacePlane()
    {

        //Destroy
        foreach (GameObject oldMesh in GameObject.FindGameObjectsWithTag("PlaneMeshTag"))
        {
            Destroy(oldMesh);
        }

        // One plane
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        halfPlaneLength = (curveDistance - 2 * halfCurveLength) / 2;

        vertList.Add(new Vector3(-halfPlaneLength, 0, -width)); //0
        vertList.Add(new Vector3(-halfPlaneLength, 0, width));  //1
        vertList.Add(new Vector3(halfPlaneLength, 0, width));   //2
        vertList.Add(new Vector3(halfPlaneLength, 0, -width));  //3

        triList.Add(0);
        triList.Add(1);
        triList.Add(2);

        triList.Add(2);
        triList.Add(3);
        triList.Add(0);

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

        //Place
        for (int i = -numberPlacement; i <= numberPlacement; i++)
        {
            GameObject newMeshObject = new GameObject("PlaneMesh_" + i);
            newMeshObject.tag = "PlaneMeshTag";
            newMeshObject.transform.position = planeYZ + new Vector3(i * curveDistance + (halfCurveLength + halfPlaneLength), 0, 0.2f);
            //newMeshObject.transform.position = planeYZ + new Vector3(i * curveDistance, 0, 0.1f);

            MeshFilter newMeshFilter = newMeshObject.AddComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newMeshObject.AddComponent<MeshRenderer>();
            Material newMaterial = new Material(Shader.Find("Standard"));
            newMaterial.color = colorPlane;
            newMeshRenderer.material = newMaterial;

            newMeshFilter.sharedMesh = mesh;
        }
    }


}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312