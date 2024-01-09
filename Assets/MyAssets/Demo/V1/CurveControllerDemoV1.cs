using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class CurveControllerDemoV1 : MonoBehaviour
{
    private ManagerDemoV1 _manager;

    Vector3 planeYZ;

    float halfCurveLength;
    float curveHeightDepth;
    float curveRadius;
    float curveDistance;

    //for creating curves;
    float width = 0.3f; //z方向の奥行の半分
    int numberPlacement = 0;
    Color color;


    //flag
    int flagHaptics = 0;   //0 -> Position(Convex), 1 -> Strength(Convex), 2 ->Position+Strength(Convex), 3 -> Position(Concave), 4 -> Strength(Concave), 5 ->Position+Strength(Concave)
    bool flagConvex = true; //false -> convene
    bool flagStack;

    //for discover change
    float previousCurveRadius = 0f;
    float previousCurveDistance = 0f;
    bool previousFlagConvex = true;

    // Start is called before the first frame update
    void Start()
    {
        _manager = GameObject.Find("Manager").GetComponent<ManagerDemoV1>();

        planeYZ = _manager.PlaneYZ;

        halfCurveLength = _manager.HalfCurveLength;
        curveHeightDepth = _manager.CurveHeightDepth;
        curveRadius = _manager.CurveRadius;
        curveDistance = _manager.CurveDistance;

        if (flagStack)
        {
            PlaceStackConvex();
        }
        else
        {
            PlaceNotStackConvex();
        }

    }

    // Update is called once per frame
    void Update()
    {
        halfCurveLength = _manager.HalfCurveLength;
        curveHeightDepth = _manager.CurveHeightDepth;
        curveRadius = _manager.CurveRadius;
        curveDistance = _manager.CurveDistance;
        flagHaptics = _manager.FlagHaptics;
        flagConvex = _manager.FlagConvex;
        flagStack = _manager.FlagStack;
        numberPlacement = _manager.NumberPlacement;

        if ((previousCurveRadius != curveRadius) || (previousCurveDistance != curveDistance) || (previousFlagConvex != flagConvex))
        {
            if (flagConvex)
            {
                if (flagStack)
                {
                    PlaceStackConvex();
                }
                else
                {
                    PlaceNotStackConvex();
                }
                
            }
            else
            {
                if (flagStack)
                {
                    PlaceStackConvene();
                }
                else
                {
                    PlaceNotStackConvene();
                }
            }

            previousCurveRadius = curveRadius;
            previousCurveDistance = curveDistance;
            previousFlagConvex = flagConvex;
        }

        ///Change Colors       
        ChangeColors();

    }

    private void ChangeColors()
    {
        switch (flagHaptics)
        {
            case 0: //Position
            case 3:
                color = new Color32(212, 117, 117, 255); //pink
                break;
            case 1: //Strength
            case 4:
                color = new Color32(100, 125, 191, 255); //blue
                break;
            case 2: //Position+Strength
            case 5:
                color = new Color32(151, 90, 193, 255); //purple
                break;
        }

        foreach (GameObject meshObject in GameObject.FindGameObjectsWithTag("CurveMeshTag"))
        {
            MeshRenderer renderer = meshObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material newMaterial = new Material(Shader.Find("Standard"));
                newMaterial.color = color;
                renderer.material = newMaterial;
            }
        }
    }

    int Angle; 
    int startAngle = 0;  //スタート地点の角度（度）
    int areaAngle = 0;  //作成する角度（度）

    int quality = 1000;     //180degのときのtriangle数

    Vector3 pos; //位置

    private Vector3[] vertices; //頂点
    private int[] triangles;    //index

    private void PlaceStackConvex()
    {

        //Destroy
        foreach (GameObject oldMesh in GameObject.FindGameObjectsWithTag("CurveMeshTag"))
        {
            Destroy(oldMesh);
        }

        //Make Parameters
        //Angle = (int)(Mathf.Atan2(curveRadius - curveHeightDepth, halfCurveLength) * Mathf.Rad2Deg);
        Angle = (int)(Mathf.Acos((curveDistance / 2) / curveRadius) * Mathf.Rad2Deg);
        startAngle = Angle;
        areaAngle = 180 - 2*Angle;

        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        float th, v1, v2;
        int max = quality * areaAngle / 180;

        //Create One Mesh
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngle;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2, v1 * curveRadius, -width));

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

        //Place
        for (int i = -numberPlacement; i <= numberPlacement; i++)
        {
            GameObject newMeshObject = new GameObject("CurveMesh_" + i);
            newMeshObject.tag = "CurveMeshTag";
            newMeshObject.transform.position = planeYZ + new Vector3(i * curveDistance, -curveRadius + curveHeightDepth, 0.2f);

            MeshFilter newMeshFilter = newMeshObject.AddComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newMeshObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = new Material(Shader.Find("Standard"));

            newMeshFilter.sharedMesh = mesh;
        }
    }
    private void PlaceNotStackConvex()
    {

        //Destroy
        foreach (GameObject oldMesh in GameObject.FindGameObjectsWithTag("CurveMeshTag"))
        {
            Destroy(oldMesh);
        }

        //Make Parameters
        Angle = (int)(Mathf.Atan2(curveRadius - curveHeightDepth, halfCurveLength) * Mathf.Rad2Deg);
        startAngle = Angle;
        areaAngle = 180 - 2 * Angle;

        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        float th, v1, v2;
        int max = quality * areaAngle / 180;

        //Create One Mesh
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngle;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, -width));

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

        //Place
        for (int i = -numberPlacement; i <= numberPlacement; i++)
        {
            GameObject newMeshObject = new GameObject("CurveMesh_" + i);
            newMeshObject.tag = "CurveMeshTag";
            newMeshObject.transform.position = planeYZ + new Vector3(i * curveDistance, -curveRadius + curveHeightDepth, 0.2f);

            MeshFilter newMeshFilter = newMeshObject.AddComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newMeshObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = new Material(Shader.Find("Standard"));

            newMeshFilter.sharedMesh = mesh;
        }
    }
    private void PlaceStackConvene()
    {

        //Destroy
        foreach (GameObject oldMesh in GameObject.FindGameObjectsWithTag("CurveMeshTag"))
        {
            Destroy(oldMesh);
        }

        //Make Parameters
        //Angle = (int)(Mathf.Atan2(curveRadius - curveHeightDepth, halfCurveLength) * Mathf.Rad2Deg);
        Angle = (int)(Mathf.Acos((curveDistance / 2) / curveRadius) * Mathf.Rad2Deg);
        startAngle = 180 + Angle;
        areaAngle = 180 - 2 * Angle;

        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        float th, v1, v2;
        int max = quality * areaAngle / 180;

        //Create One Mesh
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngle;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, -width));

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

        //Place
        for (int i = -numberPlacement; i <= numberPlacement; i++)
        {
            GameObject newMeshObject = new GameObject("CurveMesh_" + i);
            newMeshObject.tag = "CurveMeshTag";
            newMeshObject.transform.position = planeYZ + new Vector3(i * curveDistance, +curveRadius - curveHeightDepth, 0.2f);

            MeshFilter newMeshFilter = newMeshObject.AddComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newMeshObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = new Material(Shader.Find("Standard"));

            newMeshFilter.sharedMesh = mesh;
        }
    }

    private void PlaceNotStackConvene()
    {

        //Destroy
        foreach (GameObject oldMesh in GameObject.FindGameObjectsWithTag("CurveMeshTag"))
        {
            Destroy(oldMesh);
        }

        //Make Parameters
        Angle = (int)(Mathf.Atan2(curveRadius - curveHeightDepth, halfCurveLength) * Mathf.Rad2Deg);
        startAngle = 180 + Angle;
        areaAngle = 180 - 2 * Angle;

        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();

        float th, v1, v2;
        int max = quality * areaAngle / 180;

        //Create One Mesh
        for (int i = 0; i <= max; i++)
        {
            th = i * areaAngle / max + startAngle;
            v1 = Mathf.Sin(th * Mathf.Deg2Rad);
            v2 = Mathf.Cos(th * Mathf.Deg2Rad);
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, width));
            vertList.Add(new Vector3(v2 * curveRadius, v1 * curveRadius, -width));

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

        //Place
        for (int i = -numberPlacement; i <= numberPlacement; i++)
        {
            GameObject newMeshObject = new GameObject("CurveMesh_" + i);
            newMeshObject.tag = "CurveMeshTag";
            newMeshObject.transform.position = planeYZ + new Vector3(i * curveDistance, +curveRadius - curveHeightDepth, 0.2f);

            MeshFilter newMeshFilter = newMeshObject.AddComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = newMeshObject.AddComponent<MeshRenderer>();
            newMeshRenderer.material = new Material(Shader.Find("Standard"));

            newMeshFilter.sharedMesh = mesh;
        }
    }

}

//Cylinder //https://blog.narumium.net/2016/11/21/unity-c-%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%97%E3%83%88%E3%81%A7%E5%86%86%E5%BC%A7%E3%81%A8%E7%AD%92%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B/
//Plane(by Rectangular) //https://www.ame-name.com/archives/8312