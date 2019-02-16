//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.Linq;

namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatTessellation : FeatureDescriptor
   {
      const string sGlobalTess = "__MICROSPLAT_TESSELLATION__";
      static MicroSplatTessellation()
      {
         MicroSplatDefines.InitDefine(sGlobalTess);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sGlobalTess);
      }
      public override string ModuleName()
      {
         return "Tessellation & Parallax";
      }

      public enum DefineFeature
      {
         _TESSDISTANCE,
         _PERTEXTESSDISPLACE,
         _PERTEXTESSUPBIAS,
         _PERTEXTESSOFFSET,
         _PARALLAX,
         _PERTEXPARALLAX,
         kNumFeatures,
      }
         
      public bool isTessellated = false;
      public bool perTexDisplace;
      public bool perTexUpBias;
      public bool perTexOffset;
      public bool parallax;
      public bool perTexParallax;

      public TextAsset properties_tess;
      public TextAsset func_tess;
      public TextAsset func_parallax;

      GUIContent CShaderTessellation = new GUIContent("Tessellation", "Tessellate and Displace Geometry?");

      GUIContent CTessUpBias = new GUIContent("Up Bias", "How much to bias displacement along the normal, or up");
      GUIContent CTessDisplacement = new GUIContent("Displacement", "How far to displace the surface from it's original position");
      GUIContent CTessMipBias = new GUIContent("Mip Bias", "Allows you to use lower mip map levels for displacement, which often produces a better looking result and is slightly faster");
      GUIContent CTessShaping = new GUIContent("Shaping", "A seperate contrast blend for tessellation, lower values tend to look best");
      GUIContent CTessMinDistance = new GUIContent("Min Distance", "Distance in which distance based tessellation is at maximum. Also acts as the distance in which tessellation amount begins to fade when fade tessellation is on");
      GUIContent CTessMaxDistance = new GUIContent("Max Distance", "Distance in which distance based tessellation is at minimum. Also acts as the distance in which tessellation amount begins is completely faded when fade tessellation is on");
      GUIContent CTessTessellation = new GUIContent("Tessellation", "How much to tesselate the mesh at min distance, lower values are more performant");
      GUIContent CParallax = new GUIContent("Parallax", "Parallax mapping, which does an extra height map lookup to create a deeper looking texture effect");

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
         return "2.3";
      }

      public override bool RequiresShaderModel46()
      {
         return isTessellated;
      }

      public override int CompileSortOrder()
      {
         return 1; // after most stuff, so variables, etc, can all be declared..
      }

      public override void DrawFeatureGUI(Material mat)
      {
         isTessellated = EditorGUILayout.Toggle(CShaderTessellation, isTessellated);
         parallax = EditorGUILayout.Toggle(CParallax, parallax);
      }

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (isTessellated && mat.HasProperty("_TessData1") && MicroSplatUtilities.DrawRollup("Tessellation"))
         {
            var td1 = shaderGUI.FindProp("_TessData1", props);
            var td2 = shaderGUI.FindProp("_TessData2", props);
            EditorGUI.BeginChangeCheck();
            var td1v = td1.vectorValue;
            var td2v = td2.vectorValue;
            td1v.y = EditorGUILayout.Slider(CTessDisplacement, td1v.y, 0, 3);
            td1v.z = (float)EditorGUILayout.IntSlider(CTessMipBias, (int)td1v.z, 0, 6);
            td2v.z = 1.0f - (float)EditorGUILayout.Slider(CTessShaping, 1.0f - td2v.z, 0.0f, 0.999f);
            if (!perTexUpBias)
            {
               td2v.w = (float)EditorGUILayout.Slider(CTessUpBias, td2v.w, 0.0f, 1);
            }

            td1v.x = EditorGUILayout.Slider(CTessTessellation, td1v.x, 1, 32);
            td2v.x = EditorGUILayout.FloatField(CTessMinDistance, td2v.x);
            td2v.y = EditorGUILayout.FloatField(CTessMaxDistance, td2v.y);

            if (EditorGUI.EndChangeCheck())
            {
               td1.vectorValue = td1v;
               td2.vectorValue = td2v;
            }
         }
         if (parallax && MicroSplatUtilities.DrawRollup("Parallax") && mat.HasProperty("_ParallaxParams"))
         {
            EditorGUI.BeginChangeCheck();

            var parprop = shaderGUI.FindProp("_ParallaxParams", props);

            Vector4 vec = parprop.vectorValue;
            vec.x = EditorGUILayout.Slider("Parallax Height", vec.x, 0.0f, 0.12f); 
            vec.y = EditorGUILayout.FloatField("Parallax Fade Start", vec.y);
            vec.z = EditorGUILayout.FloatField("Parallax Fade Distance", vec.z);
            if (EditorGUI.EndChangeCheck())
            {
               parprop.vectorValue = vec;
            }
         }

      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_tess.txt"))
            {
               properties_tess = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_tess.txt"))
            {
               func_tess = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_parallax.txt"))
            {
               func_parallax = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      } 

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (isTessellated)
         {
            sb.Append(properties_tess.text);
         }
         if (parallax)
         {
            sb.AppendLine("      _ParallaxParams(\"Parallax Height\", Vector) = (0.08, 30, 30, 0)");
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (parallax)
         {
            arraySampleCount += 2;
            if (!features.Contains<string>(MicroSplatBaseFeatures.GetFeatureName(MicroSplatBaseFeatures.DefineFeature._MAX2LAYER)))
            {
               arraySampleCount += 1;
            }
            if (!features.Contains<string>(MicroSplatBaseFeatures.GetFeatureName(MicroSplatBaseFeatures.DefineFeature._MAX3LAYER)))
            {
               arraySampleCount += 1;
            }
         }
         if (isTessellated)
         {
            if (features.Contains<string>(MicroSplatBaseFeatures.GetFeatureName(MicroSplatBaseFeatures.DefineFeature._MAX2LAYER)))
            {
               tessellationSamples += 2;
            }
            else if (features.Contains<string>(MicroSplatBaseFeatures.GetFeatureName(MicroSplatBaseFeatures.DefineFeature._MAX3LAYER)))
            {
               tessellationSamples += 3;
            }
            else
            {
               tessellationSamples += 4;
            }

            textureSampleCount += 4; // control textures
         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (isTessellated)
         {
            features.Add(GetFeatureName(DefineFeature._TESSDISTANCE));
            if (perTexDisplace)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXTESSDISPLACE));
            }
            if (perTexUpBias)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXTESSUPBIAS));
            }
            if (perTexOffset)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXTESSOFFSET));
            }
         }
         if (parallax)
         {
            features.Add(GetFeatureName(DefineFeature._PARALLAX));
            if (perTexParallax)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXPARALLAX));
            }
         }

         return features.ToArray();
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (isTessellated)
         {
            sb.AppendLine(func_tess.text);
         }
         if (parallax)
         {
            sb.AppendLine(func_parallax.text);
         }

      }

      public override void Unpack(string[] keywords)
      {
         isTessellated = (HasFeature(keywords, DefineFeature._TESSDISTANCE));
         perTexDisplace = (HasFeature(keywords, DefineFeature._PERTEXTESSDISPLACE));
         perTexUpBias = (HasFeature(keywords, DefineFeature._PERTEXTESSUPBIAS));
         perTexOffset = (HasFeature(keywords, DefineFeature._PERTEXTESSOFFSET));
         parallax = HasFeature(keywords, DefineFeature._PARALLAX);
         if (parallax)
         {
            perTexParallax = HasFeature(keywords, DefineFeature._PERTEXPARALLAX);
         }
      }

      float IdealOffset(Texture2DArray ta, int index)
      {
         Material mat = new Material(Shader.Find("Hidden/MicroSplat/ExtractHeight"));
         mat.SetInt("index", index);
         RenderTexture rt = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32);
         Graphics.Blit(ta, rt, mat);
         RenderTexture.active = rt;
         Texture2D tex = new Texture2D(128, 128);
         tex.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
         tex.Apply();
         RenderTexture.active = null;
         rt.Release();
         GameObject.DestroyImmediate(rt);
         GameObject.DestroyImmediate(mat);

         var colors = tex.GetPixels();
         GameObject.DestroyImmediate(tex);
         float h = 0;
         for (int i = 0; i < colors.Length; ++i)
         {
            h += colors[i].a;
         }
         h /= (float)colors.Length;

         return -h;
      }

      static GUIContent CPerTexDisplace = new GUIContent("Displacement", "Scale for tessellation displacement");
      static GUIContent CPerTexUpBias = new GUIContent("Tess Up Bias", "Bias displacement from normal angle to up");
      static GUIContent CPerTexOffset = new GUIContent("Tess Offset", "Offset displacement center, to push this texture up and down along the displacement vector");
      static GUIContent CPerTexParallax = new GUIContent("Parallax Height", "Parallax height for given texture");
      static GUIContent CComputeIdeal = new GUIContent("Compute Ideal Offset", "Compute per-texture offsets based on the texture data - the idea is to get the majority of the terrain as close to the collider as possible");
      static GUIContent CComputeIdealAll = new GUIContent("Compute All", "Compute per-texture offsets for all textures based on the texture data - the idea is to get the majority of the terrain as close to the collider as possible");

      public override void DrawPerTextureGUI(int index, Material mat, MicroSplatPropData propData)
      {
         if (isTessellated)
         {
            InitPropData(6, propData, new Color(1.0f, 0.0f, 0.0f, 0.5f)); // displace, up, offset

            perTexDisplace = DrawPerTexFloatSlider(index, 6, GetFeatureName(DefineFeature._PERTEXTESSDISPLACE), 
               mat, propData, Channel.R, CPerTexDisplace, 0, 2);

            perTexUpBias = DrawPerTexFloatSlider(index, 6, GetFeatureName(DefineFeature._PERTEXTESSUPBIAS), 
               mat, propData, Channel.G, CPerTexUpBias, 0, 1);

            perTexOffset = DrawPerTexFloatSlider(index, 6, GetFeatureName(DefineFeature._PERTEXTESSOFFSET), 
               mat, propData, Channel.B, CPerTexOffset, -1, 1);

            if (perTexOffset)
            {
               EditorGUILayout.BeginHorizontal();
               if (GUILayout.Button(CComputeIdeal))
               {
                  float h = IdealOffset(mat.GetTexture("_Diffuse") as Texture2DArray, index);
                  propData.SetValue(index, 6, (int)Channel.B, h);
                  AssetDatabase.Refresh();
               }
               if (GUILayout.Button(CComputeIdealAll))
               {
                  var ta = mat.GetTexture("_Diffuse") as Texture2DArray;
                  for (int i = 0; i < 16; ++i)
                  {
                     float h = IdealOffset(ta, i);
                     propData.SetValue(i, 6, (int)Channel.B, h);
                  }
               }
               EditorGUILayout.EndHorizontal();
            }
         }

         if (parallax)
         {
            perTexParallax = DrawPerTexFloatSlider(index, 6, GetFeatureName(DefineFeature._PERTEXPARALLAX),
               mat, propData, Channel.A, CPerTexParallax, 0, 1);
         }
      }
   }   
   #endif

}