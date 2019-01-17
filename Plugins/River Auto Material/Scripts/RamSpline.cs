using UnityEngine;
using System.Collections;

//Interpolation between points with a Catmull-Rom spline
using System.Collections.Generic;

[RequireComponent (typeof(MeshFilter))]
public class RamSpline : MonoBehaviour
{
	public MeshFilter meshfilter;
	public List<Vector4> controlPoints = new List<Vector4> ();

	//Are we making a line or a loop?
	public bool isLooping = false;


	public List<Vector3> points = new List<Vector3> ();
	public List<float> widths = new List<float> ();
	public List<Quaternion> orientations = new List<Quaternion> ();
	public List<Vector3> tangents = new List<Vector3> ();
	public List<Vector3> normalsList = new List<Vector3> ();

	public float width = 4;

	public int vertsInShape = 3;
	public float traingleDensity = 0.2f;
	public float uvScale = 15f;

	public void Start ()
	{
		

		GenerateSpline ();

	}

	public void GenerateSpline ()
	{

		List<Vector4> pointsChecked = new List<Vector4> ();
		for (int i = 0; i < controlPoints.Count; i++) {
			if (i > 0) {
				if (Vector3.Distance ((Vector3)controlPoints [i], (Vector3)controlPoints [i - 1]) > 0)
					pointsChecked.Add (controlPoints [i]);

			} else
				pointsChecked.Add (controlPoints [i]);
		}

		Mesh mesh = new Mesh ();
		meshfilter = GetComponent<MeshFilter> ();
		if (pointsChecked.Count < 2) {
			mesh.Clear ();
		
			meshfilter.mesh = mesh;
			return;

		}
				

		points.Clear ();
		orientations.Clear ();
		tangents.Clear ();
		normalsList.Clear ();
		widths.Clear ();

			
		for (int i = 0; i < pointsChecked.Count; i++) {
			
			if (i > pointsChecked.Count - 2 && !isLooping) {
				continue;
			}

			CalculateCatmullRomSpline (pointsChecked, i);
		}

		GenerateMesh (ref mesh);



	}

	void GenerateMesh (ref Mesh mesh)
	{
		int segments = points.Count - 1;
		int edgeLoops = points.Count;

		int vertCount = vertsInShape * edgeLoops;

		List<int> triangleIndices = new List<int> ();
		Vector3[] vertices = new Vector3[vertCount];
		Vector3[] normals = new Vector3[vertCount];
		Vector2[] uvs = new Vector2[vertCount];
		float length = 0;


		for (int i = 0; i < points.Count; i++) {
			int offset = i * vertsInShape;
			if (i > 0)
				length += Vector3.Distance (points [i], points [i - 1]) / (float)uvScale;
			
			float u = 0;
			for (int j = 0; j < vertsInShape; j++) {
				int id = offset + j;
				float parameter = 1 / (float)(vertsInShape - 1);

				vertices [id] = points [i] + orientations [i] * ((j - (vertsInShape - 1) * 0.5f) * widths [i] * parameter * Vector3.right);
				if (j > 0) {
					u += Vector3.Distance (vertices [id], vertices [id - 1]) / (float)uvScale;
				}
				normals [id] = orientations [i] * Vector3.up;
				uvs [id] = new Vector2 (1 - length, u);
			}
		}

		for (int i = 0; i < segments; i++) {
			int offset = i * vertsInShape;
			for (int l = 0; l < vertsInShape - 1; l += 1) {
				int a = offset + l;
				int b = offset + l + vertsInShape;
				int c = offset + l + 1 + vertsInShape;
				int d = offset + l + 1;
				triangleIndices.Add (a);
				triangleIndices.Add (b);
				triangleIndices.Add (c);
				triangleIndices.Add (c);
				triangleIndices.Add (d);
				triangleIndices.Add (a);
			}
		}
		mesh = new Mesh ();
		mesh.Clear ();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uvs;
		mesh.triangles = triangleIndices.ToArray ();
		mesh.RecalculateTangents ();
		meshfilter.mesh = mesh;
	}

	//float leftDistance = 0;

	void CalculateCatmullRomSpline (List<Vector4> controlPoints, int pos)
	{
		
		Vector3 p0 = controlPoints [pos];

		Vector3 p1 = controlPoints [pos];

		Vector3 p2 = controlPoints [ClampListPos (pos + 1)];

		Vector3 p3 = controlPoints [ClampListPos (pos + 1)];

		if (pos > 0)
			p0 = controlPoints [ClampListPos (pos - 1)];
	
		if (pos < controlPoints.Count - 2)
			p3 = controlPoints [ClampListPos (pos + 2)];
	

		int loops = Mathf.FloorToInt (1f / traingleDensity);

		float i = 1;

		//float dist = Vector3.Distance (p1, p2);

//		Debug.Log (traingleDensity);
//		for (i = leftDistance; i <= dist; i += traingleDensity) {
//			float t = i / (float)dist;
//			CalculatePoint (controlPoints, pos, p0, p1, p2, p3, t);
//		}
//		leftDistance = traingleDensity - dist + i;
//
//		if (i < dist) {
//			float t = 1;
//			CalculatePoint (controlPoints, pos, p0, p1, p2, p3, t);
//		}
		float start = 0;
		if (pos > 0)
			start = 1;

		for (i = start; i <= loops; i++) {
			float t = i * traingleDensity;
			CalculatePoint (controlPoints, pos, p0, p1, p2, p3, t);
		}

		if (i < loops) {
			i = loops;
			float t = i * traingleDensity;
			CalculatePoint (controlPoints, pos, p0, p1, p2, p3, t);
		}

	}

	void CalculatePoint (List<Vector4> controlPoints, int pos, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		
		Vector3 newPos = GetCatmullRomPosition (t, p0, p1, p2, p3);
		widths.Add (Mathf.Lerp (controlPoints [pos].w, controlPoints [ClampListPos (pos + 1)].w, t));
		points.Add (newPos);
		Vector3 tangent = GetCatmullRomTangent (t, p0, p1, p2, p3).normalized;
		Vector3 normal = CalculateNormal (tangent, Vector3.up).normalized;
		Quaternion orientation;
		if (normal == tangent && normal == Vector3.zero)
			orientation = Quaternion.identity;
		else
			orientation = Quaternion.LookRotation (tangent, normal);
		orientations.Add (orientation);
		tangents.Add (tangent);
		normalsList.Add (normal);
	
	}

	int ClampListPos (int pos)
	{
		if (pos < 0) {
			pos = controlPoints.Count - 1;
		}

		if (pos > controlPoints.Count) {
			pos = 1;
		} else if (pos > controlPoints.Count - 1) {
			pos = 0;
		}

		return pos;
	}

	Vector3 GetCatmullRomPosition (float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}

	Vector3 GetCatmullRomTangent (float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{		
		return  0.5f * ((-p0 + p2) + 2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t + 3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t);
	}

	Vector3 CalculateNormal (Vector3 tangent, Vector3 up)
	{
		Vector3 binormal = Vector3.Cross (up, tangent);
		return Vector3.Cross (tangent, binormal);
	}
}