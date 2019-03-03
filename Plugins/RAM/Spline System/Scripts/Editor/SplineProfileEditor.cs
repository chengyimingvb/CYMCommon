using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.VegetationSystem;
#endif

[CustomEditor(typeof(SplineProfile)), CanEditMultipleObjects]
public class SplineProfileEditor : Editor
{


    public override void OnInspectorGUI()
    {
        SplineProfile spline = (SplineProfile)target;

        spline.splineMaterial = (Material)EditorGUILayout.ObjectField("Material", spline.splineMaterial, typeof(Material), false);

        spline.meshCurve = EditorGUILayout.CurveField("Mesh curve", spline.meshCurve);

        EditorGUILayout.LabelField("Vertice distribution: " + spline.minVal.ToString() + " " + spline.maxVal.ToString());
        EditorGUILayout.MinMaxSlider(ref spline.minVal, ref spline.maxVal, 0, 1);
        spline.minVal = (int)(spline.minVal * 100) * 0.01f;
        spline.maxVal = (int)(spline.maxVal * 100) * 0.01f;
        if (spline.minVal > 0.5f)
            spline.minVal = 0.5f;
        if (spline.minVal < 0.01f)
            spline.minVal = 0.01f;
        if (spline.maxVal < 0.5f)
            spline.maxVal = 0.5f;
        if (spline.maxVal > 0.99f)
            spline.maxVal = 0.99f;

        spline.traingleDensity = 1 / (float)EditorGUILayout.IntSlider("U", (int)(1 / (float)spline.traingleDensity), 1, 100);
        spline.vertsInShape = EditorGUILayout.IntSlider("V", spline.vertsInShape - 1, 1, 20) + 1;

        spline.uvScale = EditorGUILayout.FloatField("UV scale (texture tiling)", spline.uvScale);
        spline.uvRotation = EditorGUILayout.Toggle("Rotate UV", spline.uvRotation);

        spline.flowFlat = EditorGUILayout.CurveField("Flow curve flat speed", spline.flowFlat);
        spline.flowWaterfall = EditorGUILayout.CurveField("Flow curve waterfall speed", spline.flowWaterfall);


        GUILayout.Label("Terrain carve:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        spline.terrainCarve = EditorGUILayout.CurveField("Terrain carve", spline.terrainCarve);
        spline.distSmooth = EditorGUILayout.FloatField("Smooth distance", spline.distSmooth);
       // spline.distSmoothStart = EditorGUILayout.FloatField("Smooth start distance", spline.distSmoothStart);
        EditorGUI.indentLevel--;



        EditorGUILayout.Space();
        EditorGUI.indentLevel++;
        spline.terrainPaintCarve = EditorGUILayout.CurveField("Terrain paint", spline.terrainPaintCarve);
        EditorGUI.indentLevel--;

        GUILayout.Label("Lightning settings:", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        spline.receiveShadows = EditorGUILayout.Toggle("Receive Shadows", spline.receiveShadows);

        spline.shadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup("Shadow Casting Mode", spline.shadowCastingMode);
        EditorGUI.indentLevel--;

        EditorUtility.SetDirty(target);
#if VEGETATION_STUDIO_PRO
        spline.biomeType = System.Convert.ToInt32(EditorGUILayout.EnumPopup("Select biome", (BiomeType)spline.biomeType));
#else
        spline.biomeType = EditorGUILayout.IntField("Select biome", spline.biomeType);
#endif

    }
}
