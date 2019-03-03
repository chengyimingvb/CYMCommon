
using System;
using System.Collections.Generic;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

#if VEGETATION_STUDIO
using AwesomeTechnologies;
using AwesomeTechnologies.VegetationStudio;
#endif
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem.Biomes;
#endif

[RequireComponent(typeof(MeshFilter))]
public class LakePolygon : MonoBehaviour
{
    public LakePolygonProfile currentProfile;
    public LakePolygonProfile oldProfile;


    public List<Vector3> points = new List<Vector3>();
    public List<Vector3> splinePoints = new List<Vector3>();

    public AnimationCurve terrainCarve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(10, -2) });
    public float distSmooth = 5;
    public float uvScale = 1;
    public float distSmoothStart = 1;
    public AnimationCurve terrainPaintCarve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    public int currentSplatMap = 1;

    public float maximumTriangleSize = 50;
    public float traingleDensity = 0.2f;

    public float height = 0;
    public bool lockHeight = true;



    public float yOffset = 0;


    public float trianglesGenerated = 0;
    public Mesh currentMesh;


#if VEGETATION_STUDIO_PRO
    public float biomMaskResolution = 0.5f;
    public float vegetationBlendDistance = 3;
    public BiomeMaskArea biomeMaskArea;
    public bool refreshMask = false;
#endif
#if VEGETATION_STUDIO

    public float vegetationMaskResolution = 0.5f;
    public float vegetationMaskPerimeter = 5;
    public VegetationMaskArea vegetationMaskArea;

#endif



    /// <summary>
    /// Add point at end of spline
    /// </summary>
    /// <param name="position">New point position</param>
    public void AddPoint(Vector3 position)
    {
        points.Add(position);
    }

    /// <summary>
    /// Add point in the middle of the spline
    /// </summary>
    /// <param name="i">Point id</param>
    public void AddPointAfter(int i)
    {
        Vector3 position = points[i];
        if (i < points.Count - 1 && points.Count > i + 1)
        {
            Vector3 positionSecond = points[i + 1];
            if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                position = (position + positionSecond) * 0.5f;
            else
                position.x += 1;
        }
        else if (points.Count > 1 && i == points.Count - 1)
        {
            Vector3 positionSecond = points[i - 1];
            if (Vector3.Distance((Vector3)positionSecond, (Vector3)position) > 0)
                position = position + (position - positionSecond);
            else
                position.x += 1;
        }
        else
        {
            position.x += 1;
        }
        points.Insert(i + 1, position);

    }



    /// <summary>
    /// Changes point position, if new position doesn't have width old width will be taken
    /// </summary>
    /// <param name="i">Point id</param>
    /// <param name="position">New position</param>
    public void ChangePointPosition(int i, Vector3 position)
    {

        points[i] = position;
    }

    /// <summary>
    /// Removes point in spline
    /// </summary>
    /// <param name="i"></param>
    public void RemovePoint(int i)
    {
        if (i < points.Count)
        {
            points.RemoveAt(i);
        }
    }

    /// <summary>
    /// Removes points from point id forward
    /// </summary>
    /// <param name="fromID">Point id</param>
    public void RemovePoints(int fromID = -1)
    {
        int pointsCount = points.Count - 1;
        for (int i = pointsCount; i > fromID; i--)
        {
            RemovePoint(i);
        }

    }

    void CenterPivot()
    {
        Vector3 position = transform.position;
        Vector3 center = Vector3.zero;

        for (int i = 0; i < points.Count; i++)
        {
            center += points[i];
        }
        center /= points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 vec = points[i];
            vec.x -= center.x;
            vec.y -= center.y;
            vec.z -= center.z;
            points[i] = vec;
        }

        transform.position += center;
    }


    public void GeneratePolygon()
    {
        if (lockHeight)
        {
            for (int i = 1; i < points.Count; i++)
            {
                Vector3 vec = points[i];
                vec.y = points[0].y;
                points[i] = vec;
            }

        }

        if (points.Count < 3)
            return;

        CenterPivot();

        splinePoints.Clear();
        for (int i = 0; i < points.Count; i++)
        {

            //if ((i == 0 || i == points.Count - 2 || i == points.Count - 1) && !true)
            //{
            //    continue;
            //}

            DisplayCatmullRomSpline(i);
        }





        List<Vector3> verticesList = new List<Vector3>();
        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();
        verticesList.AddRange(splinePoints.ToArray());


        //Triangulator traingulator = new Triangulator(verts2d.ToArray());
        // indices.AddRange(traingulator.Triangulate());


        Polygon polygon = new Polygon();

        List<Vertex> vertexs = new List<Vertex>();

        for (int i = 0; i < verticesList.Count; i++)
        {
            Vertex vert = new Vertex(verticesList[i].x, verticesList[i].z);
            vert.z = verticesList[i].y;
            vertexs.Add(vert);
        }
        polygon.Add(new Contour(vertexs));

        var options = new ConstraintOptions() { ConformingDelaunay = true };
        var quality = new QualityOptions() { MinimumAngle = 25, MaximumArea = maximumTriangleSize };



        TriangleNet.Mesh mesh = (TriangleNet.Mesh)polygon.Triangulate(options, quality);

        indices.Clear();
        foreach (var triangle in mesh.triangles)
        {
            Vertex vertex = mesh.vertices[triangle.vertices[2].id];

            Vector3 v0 = new Vector3((float)vertex.x, (float)vertex.z, (float)vertex.y);

            vertex = mesh.vertices[triangle.vertices[1].id];
            Vector3 v1 = new Vector3((float)vertex.x, (float)vertex.z, (float)vertex.y);
            vertex = mesh.vertices[triangle.vertices[0].id];
            Vector3 v2 = new Vector3((float)vertex.x, (float)vertex.z, (float)vertex.y);

            indices.Add(verts.Count);
            indices.Add(verts.Count + 1);
            indices.Add(verts.Count + 2);

            verts.Add(v0);
            verts.Add(v1);
            verts.Add(v2);

        }


        Vector3[] vertices = verts.ToArray();
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y += yOffset;
        }

        currentMesh = new Mesh();

        currentMesh.vertices = vertices;
        currentMesh.subMeshCount = 1;
        currentMesh.SetTriangles(indices, 0);
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z) * 0.01f * uvScale;
        }


        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.black;
        }

        currentMesh.uv = uvs;
        currentMesh.normals = normals;
        currentMesh.colors = colors;
        currentMesh.RecalculateTangents();
        currentMesh.RecalculateBounds();


        currentMesh.RecalculateTangents();
        currentMesh.RecalculateBounds();
        trianglesGenerated = indices.Count / 3;

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.mesh = currentMesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = currentMesh;
        }
    }


    public static LakePolygon CreatePolygon(Material material, List<Vector3> positions = null)
    {
        GameObject gameobject = new GameObject("Lake Polygon");
        LakePolygon polygon = gameobject.AddComponent<LakePolygon>();
        MeshRenderer meshRenderer = gameobject.AddComponent<MeshRenderer>();
        meshRenderer.receiveShadows = false;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        if (material != null)
            meshRenderer.sharedMaterial = material;

        if (positions != null)
            for (int i = 0; i < positions.Count; i++)
            {
                polygon.AddPoint(positions[i]);
            }

        return polygon;
    }

    void DisplayCatmullRomSpline(int pos)
    {
        Vector3 p0 = points[ClampListPos(pos - 1)];
        Vector3 p1 = points[pos];
        Vector3 p2 = points[ClampListPos(pos + 1)];
        Vector3 p3 = points[ClampListPos(pos + 2)];


        int loops = Mathf.FloorToInt(1f / traingleDensity);

        for (int i = 1; i <= loops; i++)
        {
            float t = i * traingleDensity;

            splinePoints.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
        }
    }

    public int ClampListPos(int pos)
    {
        if (pos < 0)
        {
            pos = points.Count - 1;
        }

        if (pos > points.Count)
        {
            pos = 1;
        }
        else if (pos > points.Count - 1)
        {
            pos = 0;
        }

        return pos;
    }



    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }





}