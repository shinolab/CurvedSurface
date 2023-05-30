using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Test : MonoBehaviour
{
	 int areaAngle = 160;  //作成する角度
	int startAngle = 200;  //スタート地点の角度
	float height = 0.1f;  //高さ
	int quality = 100;     //360degのときのtriangle数/2
	bool isOutward = true; //内向きか外向きか
	Color color = new Color(0, 1, 0, 0); //RGBA
	Vector3 scale = new Vector3(0.2f, 0.2f, 0.1f);  //大きさ

	private Vector3[] vertices; //頂点
	private int[] triangles;    //index

	private void makeParams()
	{
		List<Vector3> vertList = new List<Vector3>();
		List<int> triList = new List<int>();

		float th, v1, v2;
		int max = (int)quality * areaAngle / 360;
		for (int i = 0; i <= max; i++)
		{
			th = i * areaAngle / max + startAngle;
			v1 = Mathf.Sin(th * Mathf.Deg2Rad);
			v2 = Mathf.Cos(th * Mathf.Deg2Rad);
			vertList.Add(new Vector3(v2*0.2f, v1 * 0.2f, height));
			vertList.Add(new Vector3(v2 * 0.2f, v1 * 0.2f, -height));
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
	}

	private void setParams()
	{
		Mesh mesh = new Mesh();

		mesh.vertices = vertices;
		mesh.triangles = triangles;

		// 法線とバウンディングの計算
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		mesh.name = "tubeMesh";
		//transform.localScale = scale;

		GetComponent<MeshFilter>().sharedMesh = mesh;
		GetComponent<MeshCollider>().sharedMesh = mesh;

		// 色指定
		GetComponent<MeshRenderer>().material.color = color;
	}

	void Start()
	{
		makeParams();
		setParams();
	}
}