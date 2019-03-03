using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
#endif

[CustomEditor(typeof(LakePolygonProfile)), CanEditMultipleObjects]
public class LakePolygonProfileEditor : Editor
{


    public override void OnInspectorGUI()
    {
        LakePolygonProfile lakePolygon = (LakePolygonProfile)target;

        lakePolygon.lakeMaterial = (Material)EditorGUILayout.ObjectField("Material", lakePolygon.lakeMaterial, typeof(Material), false);

        GUILayout.Label("Mesh settings:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        lakePolygon.maximumTriangleSize = EditorGUILayout.FloatField("Maximum triangle size", lakePolygon.maximumTriangleSize);
        lakePolygon.traingleDensity = 1 / (float)EditorGUILayout.IntSlider("Spline density", (int)(1 / (float)lakePolygon.traingleDensity), 1, 100);
        lakePolygon.uvScale = EditorGUILayout.FloatField("UV scale", lakePolygon.uvScale);


        GUILayout.Label("Terrain carve:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        lakePolygon.terrainCarve = EditorGUILayout.CurveField("Terrain carve", lakePolygon.terrainCarve);
        lakePolygon.distSmooth = EditorGUILayout.FloatField("Smooth distance", lakePolygon.distSmooth);
        lakePolygon.distSmoothStart = EditorGUILayout.FloatField("Smooth start distance", lakePolygon.distSmoothStart);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        lakePolygon.terrainPaintCarve = EditorGUILayout.CurveField("Terrain paint", lakePolygon.terrainPaintCarve);
        EditorGUI.indentLevel--;

#if VEGETATION_STUDIO_PRO
        lakePolygon.biomeType = System.Convert.ToInt32(EditorGUILayout.EnumPopup("Select biome", (BiomeType)lakePolygon.biomeType));
#else
        lakePolygon.biomeType = EditorGUILayout.IntField("Select biome", lakePolygon.biomeType);
#endif
    }
}
