using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu (fileName = "LakePolygonProfile", menuName = "LakePolygonProfile", order = 1)]
public class LakePolygonProfile : ScriptableObject
{
	public Material lakeMaterial;


    public AnimationCurve terrainCarve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(10, -2) });
    public float distSmooth = 5;
    public float uvScale = 1;
    public float distSmoothStart = 1;
    public AnimationCurve terrainPaintCarve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });
    public int currentSplatMap = 1;

    public float maximumTriangleSize = 50;
    public float traingleDensity = 0.2f;
    public int biomeType;
}
