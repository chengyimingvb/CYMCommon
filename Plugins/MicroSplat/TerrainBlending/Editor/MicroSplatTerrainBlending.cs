//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatTerrainBlending : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_TERRAINBLEND__";
      static MicroSplatTerrainBlending()
      {
         MicroSplatDefines.InitDefine(sDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sDefine);
      }

      public override string ModuleName()
      {
         return "Terrain Blending";
      }

      public enum DefineFeature
      {
         _TERRAINBLENDING,
         _TBDISABLE_DETAILNOISE,
         _TBDISABLE_DISTANCENOISE,
         _TBDISABLE_DISTANCERESAMPLE,
         _TBOBJECTNORMALBLEND,
         _TBDISABLE_ALPHACONTROL,
         kNumFeatures,
      }
         
      static TextAsset props;

      public bool terrainBlend;
      public bool alphaShader;
      public bool disableDetailNoise;
      public bool disableDistanceNoise;
      public bool disableDistanceResampling;
      public bool objectNormalBlend;
      public bool disableAlphaControl;

      GUIContent CTerrainBlend = new GUIContent("Terrain Blending", "Generate shader and enable system for mesh:terrain blending");
      GUIContent CDisableDetailNoise = new GUIContent("Disable Detail Noise", "Disabled detail noise in blendable object shader");
      GUIContent CDisableDistanceNoise = new GUIContent("Disable Distance Noise", "Disabled distance noise in blendable object shader");
      GUIContent CDisableDistanceResample = new GUIContent("Disable Distance Resample", "Disabled distance resample in blendable object shader");
      GUIContent CObjectNormalBlend = new GUIContent("Object Normal Blending", "When enabled, allows you to blend with the objects original normal map if provided");
      GUIContent CDisableAlphaControl = new GUIContent("Disable Alpha", "By default, the vertex color's alpha channel can be used to control the blend manually. However, if the shader being blended with already uses the vertex color alpha for something it may be necissary to disable it");
      // Can we template these somehow?
      static Dictionary<DefineFeature, string> sFeatureNames = new Dictionary<DefineFeature, string>();
      public static string GetFeatureName(DefineFeature feature)
      {
         string ret;
         if (sFeatureNames.TryGetValue(feature, out ret))
         {
            return ret;
         }
         string fn = System.Enum.GetName(typeof(DefineFeature), feature);
         sFeatureNames[feature] = fn;
         return fn;
      }

      public static bool HasFeature(string[] keywords, DefineFeature feature)
      {
         string f = GetFeatureName(feature);
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords[i] == f)
               return true;
         }
         return false;
      }
         
      public override string GetVersion()
      {
         return "2.4";
      }

      public override void DrawFeatureGUI(Material mat)
      {
         bool old = terrainBlend;
         terrainBlend = EditorGUILayout.Toggle(CTerrainBlend, terrainBlend);
         if (old)
         {
            EditorGUI.indentLevel++;

            objectNormalBlend = EditorGUILayout.Toggle(CObjectNormalBlend, objectNormalBlend);
            disableAlphaControl = EditorGUILayout.Toggle(CDisableAlphaControl, disableAlphaControl);

            if (mat.IsKeywordEnabled("_DETAILNOISE") || mat.IsKeywordEnabled("_ANTITILEARRAYDETAIL"))
            {
               disableDetailNoise = EditorGUILayout.Toggle(CDisableDetailNoise, disableDetailNoise);
            }
            if (mat.IsKeywordEnabled("_DISTANCENOISE") || mat.IsKeywordEnabled("_ANTITILEARRAYDISTANCE"))
            {
               disableDistanceNoise = EditorGUILayout.Toggle(CDisableDistanceNoise, disableDistanceNoise);
            }
            if (mat.IsKeywordEnabled("_DISTANCERESAMPLE"))
            {
               disableDistanceResampling = EditorGUILayout.Toggle(CDisableDistanceResample, disableDistanceResampling);
            }
            EditorGUI.indentLevel--;
         }
      }

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {

      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (terrainBlend)
         {
            features.Add(GetFeatureName(DefineFeature._TERRAINBLENDING));

            if (disableAlphaControl)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_ALPHACONTROL));
            }
            if (objectNormalBlend)
            {
               features.Add(GetFeatureName(DefineFeature._TBOBJECTNORMALBLEND));
            }

            if (disableDetailNoise)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_DETAILNOISE));
            }
            if (disableDistanceNoise)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_DISTANCENOISE));
            }
            if (disableDistanceResampling)
            {
               features.Add(GetFeatureName(DefineFeature._TBDISABLE_DISTANCERESAMPLE));
            }
         }
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         terrainBlend = HasFeature(keywords, DefineFeature._TERRAINBLENDING);
         if (terrainBlend)
         {
            disableAlphaControl = HasFeature(keywords, DefineFeature._TBDISABLE_ALPHACONTROL);
            disableDetailNoise = HasFeature(keywords, DefineFeature._TBDISABLE_DETAILNOISE);
            disableDistanceNoise = HasFeature(keywords, DefineFeature._TBDISABLE_DISTANCENOISE);
            disableDistanceResampling = HasFeature(keywords, DefineFeature._TBDISABLE_DISTANCERESAMPLE);
            objectNormalBlend = HasFeature(keywords, DefineFeature._TBOBJECTNORMALBLEND);
         }
         
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_terrainblend.txt"))
            {
               props = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (terrainBlend)
         {
            sb.AppendLine(props.text);
            if (objectNormalBlend)
            {
               sb.AppendLine("      [NoScaleOffset]_NormalOriginal (\"Normal(from original)\", 2D) = \"bump\" {}");
            }
         }
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {

      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
  
      }

   }   

   #endif
}