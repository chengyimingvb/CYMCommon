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
   public class MicroSplatAlphaHole : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_ALPHAHOLE__";
      static MicroSplatAlphaHole()
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
         return "Alpha Hole";
      }

      public enum DefineFeature
      {
         _ALPHAHOLE,
         _ALPHABELOWHEIGHT,
         _ALPHAHOLETEXTURE,
         kNumFeatures,
      }

      static TextAsset properties;
      static TextAsset funcs;

      public enum AlphaHoleMode
      {
         None = 0,
         SplatIndex, 
         ClipMap
      }

      public AlphaHoleMode alphaHole;
      public bool alphaBelowHeight;
      public int textureIndex;


      GUIContent CAlphaHole = new GUIContent("Alpha Hole", "Paint areas which are transparent or use a clip (r channel) to clip sections of terrain out");
      GUIContent CAlphaBelowHeight = new GUIContent("Alpha Water Level", "Clip any area below this level");

      GUIContent CAlphaTexture = new GUIContent("Clip Texture", "Clip map for terrain, reads R channel - black is clipped");
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

      static GUIContent CTextureIndex = new GUIContent("Texture Index", "Texture Index which is considered 'transparent'");
      static GUIContent CWaterLevel = new GUIContent("Water Level", "Height at which to clip terrain below");

      public override string GetVersion()
      {
         return "2.3";
      }

      public override void DrawFeatureGUI(Material mat)
      {
         alphaHole = (AlphaHoleMode)EditorGUILayout.EnumPopup(CAlphaHole, alphaHole);
         alphaBelowHeight = EditorGUILayout.Toggle(CAlphaBelowHeight, alphaBelowHeight);
      }

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (alphaHole != AlphaHoleMode.None || alphaBelowHeight)
         {
            if (MicroSplatUtilities.DrawRollup("Alpha"))
            {
                  if (mat.HasProperty("_AlphaData") && alphaHole == AlphaHoleMode.SplatIndex || alphaBelowHeight)
                  {
                     Vector4 vals = shaderGUI.FindProp("_AlphaData", props).vectorValue;
                     Vector4 newVals = vals;
                     if (alphaHole == AlphaHoleMode.SplatIndex)
                     {
                        newVals.x = (int)EditorGUILayout.IntSlider(CTextureIndex, (int)vals.x, 0, 16);
                     }
                     if (alphaBelowHeight)
                     {
                        newVals.y = EditorGUILayout.FloatField(CWaterLevel, vals.y);
                     }
                     if (newVals != vals)
                     {
                        shaderGUI.FindProp("_AlphaData", props).vectorValue = newVals;
                     }
                  
               }
               else if (mat.HasProperty("_AlphaHoleTexture"))
               {
                  materialEditor.TexturePropertySingleLine(CAlphaTexture, shaderGUI.FindProp("_AlphaHoleTexture", props));
               }
            }
         }

      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (alphaHole == AlphaHoleMode.SplatIndex)
         {
            features.Add(GetFeatureName(DefineFeature._ALPHAHOLE));
         }
         else if (alphaHole == AlphaHoleMode.ClipMap)
         {
            features.Add(GetFeatureName(DefineFeature._ALPHAHOLETEXTURE));
         }
         if (alphaBelowHeight)
         {
            features.Add(GetFeatureName(DefineFeature._ALPHABELOWHEIGHT));
         }
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         alphaHole = AlphaHoleMode.None;
         if (HasFeature(keywords, DefineFeature._ALPHAHOLE))
         {
            alphaHole = AlphaHoleMode.SplatIndex;
         }
         else if (HasFeature(keywords, DefineFeature._ALPHAHOLETEXTURE))
         {
            alphaHole = AlphaHoleMode.ClipMap;
         }
         alphaBelowHeight = HasFeature(keywords, DefineFeature._ALPHABELOWHEIGHT);
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_alphahole.txt"))
            {
               properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_alphahole.txt"))
            {
               funcs = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (alphaHole != AlphaHoleMode.None || alphaBelowHeight)
         {
            sb.Append(properties.text);
            if (alphaHole == AlphaHoleMode.ClipMap)
            {
               sb.AppendLine("      _AlphaHoleTexture(\"ClipMap\", 2D) = \"white\" {}");
            }
         }
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (alphaHole != AlphaHoleMode.None || alphaBelowHeight)
         {
            sb.Append(funcs.text);
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (alphaHole == AlphaHoleMode.ClipMap)
         {
            textureSampleCount++;
         }

      }

   }   

   #endif
}