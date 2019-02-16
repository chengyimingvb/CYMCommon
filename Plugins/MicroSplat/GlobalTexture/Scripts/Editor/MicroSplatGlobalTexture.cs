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
   public class MicroSplatGlobalTexture : FeatureDescriptor
   {
      const string sGlobalTextureDefine = "__MICROSPLAT_GLOBALTEXTURE__";
      static MicroSplatGlobalTexture()
      {
         MicroSplatDefines.InitDefine(sGlobalTextureDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sGlobalTextureDefine);
      }

      public override string ModuleName()
      {
         return "Global Texturing";
      }

      public enum DefineFeature
      {
         _GEOMAP,
         _GEORANGE,
         _GEOCURVE,
         _PERTEXGEO,
         _GLOBALTINT,
         _GLOBALNORMALS,
         _GLOBALTINTMULT2X,
         _GLOBALTINTOVERLAY,
         _GLOBALTINTCROSSFADE,
         _GLOBALNORMALCROSSFADE,
         _PERTEXGLOBALTINTSTRENGTH,
         _PERTEXGLOBALNORMALSTRENGTH,

         kNumFeatures,
      }

      public enum BlendMode
      {
         Off,
         Multiply2X,
         Overlay,
         CrossFade
      }

      public enum NormalBlendMode
      {
         Off,
         NormalBlend,
         CrossFade
      }


      public bool geoTexture;
      public bool geoRange;
      public bool perTexGeoStr;
      public bool perTexTintStr;
      public bool perTexNormalStr;
      public bool geoCurve;


      public BlendMode tintBlendMode = BlendMode.Off;
      public NormalBlendMode normalBlendMode = NormalBlendMode.Off;
     
      public TextAsset properties_geomap;
      public TextAsset properties_tint;
      public TextAsset properties_normal;
      public TextAsset properties_params;
      public TextAsset function_geomap;

      GUIContent CShaderGeoTexture = new GUIContent("Geo Height Texture", "Enabled Geo Height Texture, which is mapped virtically to colorize the terrain");
      GUIContent CShaderGeoRange = new GUIContent("Geo Range", "Fade geo effect so it only affects a certain height range of the terrain");
      GUIContent CShaderGeoCurve = new GUIContent("Geo Curve", "Use a Curve to distort the height of the geotexture on the terrain");
      GUIContent CShaderTint = new GUIContent("Global Tint", "Enable a Tint map, which is blended with the albedo of the terrain in one of several ways");
      GUIContent CShaderGlobalNormal = new GUIContent("Global Normal", "Enabled a global normal map which is blended with the terrain in one of several ways");

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

      public override void DrawFeatureGUI(Material mat)
      {
         geoTexture = EditorGUILayout.Toggle(CShaderGeoTexture, geoTexture);
         if (geoTexture)
         {
            EditorGUI.indentLevel++;
            geoRange = EditorGUILayout.Toggle(CShaderGeoRange, geoRange);
            geoCurve = EditorGUILayout.Toggle(CShaderGeoCurve, geoCurve);
            EditorGUI.indentLevel--;
         }
         tintBlendMode = (BlendMode)EditorGUILayout.EnumPopup(CShaderTint, tintBlendMode);
         normalBlendMode = (NormalBlendMode)EditorGUILayout.EnumPopup(CShaderGlobalNormal, normalBlendMode);
      }

      static GUIContent CGeoTex = new GUIContent("Geo Texture", "Virtical striping texture for terrain");
      static GUIContent CGeoRange = new GUIContent("Geo Range", "World height at which geo texture begins to fade in, is faded in completely, begins to fade out, and is faded out completely");
      static GUIContent CGeoCurve = new GUIContent("Height Curve", "Allows you to bend the height lookup on the GeoTexture");
      static GUIContent CUVScale = new GUIContent("UV Scale", "Scale for UV tiling");
      static GUIContent CUVOffset = new GUIContent("UV Offset", "Offset for UV tiling");

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (geoTexture && MicroSplatUtilities.DrawRollup("Geo Texture"))
         {
            if (mat.HasProperty("_GeoTex"))
            {
               var texProp = shaderGUI.FindProp("_GeoTex", props);
               materialEditor.TexturePropertySingleLine(CGeoTex, texProp);
               MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_geomap_01");

               Vector4 parms = mat.GetVector("_GeoParams");
               EditorGUI.BeginChangeCheck();
               parms.x = EditorGUILayout.Slider("Blend", parms.x, 0, 1);
               parms.y = 1.0f / Mathf.Max(parms.y, 0.00001f);
               parms.y = EditorGUILayout.FloatField("World Scale", parms.y);
               parms.y = 1.0f / Mathf.Max(parms.y, 0.00001f);
               parms.z = EditorGUILayout.FloatField("World Offset", parms.z);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetVector("_GeoParams", parms);
                  EditorUtility.SetDirty(mat);
               }
               if (geoRange && mat.HasProperty("_GeoRange"))
               {
                  Vector4 rangeParams = mat.GetVector("_GeoRange");
                  EditorGUI.BeginChangeCheck();
                  rangeParams = EditorGUILayout.Vector4Field(CGeoRange, rangeParams);
                  if (EditorGUI.EndChangeCheck())
                  {
                     if (rangeParams.z < rangeParams.x || rangeParams.z < rangeParams.y)
                     {
                        rangeParams.z = rangeParams.y;
                     }
                     if (rangeParams.y < rangeParams.x)
                     {
                        rangeParams.y = rangeParams.x;
                     }
                     if (rangeParams.w < rangeParams.z)
                     {
                        rangeParams.z = rangeParams.w;
                     }

                     mat.SetVector("_GeoRange", rangeParams);
                     EditorUtility.SetDirty(mat);
                  }
               }
               if (geoCurve && mat.HasProperty("_GeoCurveParams"))
               {
                  var propData = MicroSplatShaderGUI.FindOrCreatePropTex(mat);
                  EditorGUI.BeginChangeCheck();
                  if (propData != null)
                  {
                     propData.geoCurve = EditorGUILayout.CurveField(CGeoCurve, propData.geoCurve);
                  }
                  Vector4 curveParams = mat.GetVector("_GeoCurveParams");
                  curveParams.x = EditorGUILayout.FloatField("Scale", curveParams.x);
                  curveParams.y = EditorGUILayout.FloatField("Offset", curveParams.y);
                  curveParams.z = EditorGUILayout.FloatField("Rotation", curveParams.z);

                  if (EditorGUI.EndChangeCheck())
                  {
                     AnimationCurve c = propData.geoCurve;
                     for (int i = 0; i < c.length; ++i)
                     {
                        c.keys[i].time = Mathf.Clamp01(c.keys[i].time);
                     }
                     mat.SetVector("_GeoCurveParams", curveParams);
                     EditorUtility.SetDirty(mat);
                     EditorUtility.SetDirty(propData);
                     MicroSplatTerrain.SyncAll();
                  }


               }

            }
         }
         if ((tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off) && MicroSplatUtilities.DrawRollup("Global Texture"))
         {
            if (tintBlendMode != BlendMode.Off && mat.HasProperty("_GlobalTintTex"))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent("Tint Texture", "Albedo Tint Texture"), shaderGUI.FindProp("_GlobalTintTex", props));
               Vector4 parms = mat.GetVector("_GlobalTextureParams");
               EditorGUI.BeginChangeCheck();
               parms.x = EditorGUILayout.Slider("Blend", parms.x, 0, 1);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetVector("_GlobalTextureParams", parms);
                  EditorUtility.SetDirty(mat);
               }
               if (mat.HasProperty("_GlobalTintFade"))
               {
                  Vector4 fade = mat.GetVector("_GlobalTintFade");
                  EditorGUI.BeginChangeCheck();

                  fade.x = EditorGUILayout.FloatField("Begin Fade", fade.x);
                  fade.z = EditorGUILayout.Slider("Opacity At Begin", fade.z, 0, 1);
                  fade.y = EditorGUILayout.FloatField("Fade Range", fade.y);
                  fade.w = EditorGUILayout.Slider("Opacity At End", fade.w, 0, 1);

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_GlobalTintFade", fade);
                     EditorUtility.SetDirty(mat);
                  }
               }
               if (mat.HasProperty("_GlobalTintUVScale"))
               {
                  Vector4 uv = mat.GetVector("_GlobalTintUVScale");
                  Vector2 scale = new Vector2(uv.x, uv.y);
                  Vector2 offset = new Vector2(uv.z, uv.w);

                  EditorGUI.BeginChangeCheck();
                  scale = EditorGUILayout.Vector2Field(CUVScale, scale);
                  offset = EditorGUILayout.Vector2Field(CUVOffset, offset);

                  if (EditorGUI.EndChangeCheck())
                  {
                     uv = new Vector4(scale.x, scale.y, offset.x, offset.y);
                     mat.SetVector("_GlobalTintUVScale", uv);
                     EditorUtility.SetDirty(mat);
                  }
               }
            }
            if (normalBlendMode != NormalBlendMode.Off && mat.HasProperty("_GlobalNormalTex"))
            {
               materialEditor.TexturePropertySingleLine(new GUIContent("Normal Texture", "Global Normal Texture"), shaderGUI.FindProp("_GlobalNormalTex", props));
               Vector4 parms = mat.GetVector("_GlobalTextureParams");
               EditorGUI.BeginChangeCheck();
               parms.y = EditorGUILayout.Slider("Blend", parms.y, 0, 3);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetVector("_GlobalTextureParams", parms);
                  EditorUtility.SetDirty(mat);
               }

               if (mat.HasProperty("_GlobalNormalFade"))
               {
                  Vector4 fade = mat.GetVector("_GlobalNormalFade");
                  EditorGUI.BeginChangeCheck();

                  fade.x = EditorGUILayout.FloatField("Begin Fade", fade.x);
                  fade.z = EditorGUILayout.Slider("Opacity At Begin", fade.z, 0, 1);
                  fade.y = EditorGUILayout.FloatField("Fade Range", fade.y);
                  fade.w = EditorGUILayout.Slider("Opacity At End", fade.w, 0, 1);

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetVector("_GlobalNormalFade", fade);
                     EditorUtility.SetDirty(mat);
                  }
               }
               if (mat.HasProperty("_GlobalNormalUVScale"))
               {
                  Vector4 uv = mat.GetVector("_GlobalNormalUVScale");
                  Vector2 scale = new Vector2(uv.x, uv.y);
                  Vector2 offset = new Vector2(uv.z, uv.w);

                  EditorGUI.BeginChangeCheck();
                  scale = EditorGUILayout.Vector2Field(CUVScale, scale);
                  offset = EditorGUILayout.Vector2Field(CUVOffset, offset);

                  if (EditorGUI.EndChangeCheck())
                  {
                     uv = new Vector4(scale.x, scale.y, offset.x, offset.y);
                     mat.SetVector("_GlobalNormalUVScale", uv);
                     EditorUtility.SetDirty(mat);
                  }
               }
            }
         }
      }

      static GUIContent CPerTexGeo = new GUIContent("Geo Strength", "How much the geo texture should show on this texture");
      static GUIContent CPerTexTint = new GUIContent("Global Tint Strength", "How much the global tint texture should show on this texture");
      static GUIContent CPerTexNormal = new GUIContent("Global Normal Strength", "How much the global normal texture should show on this texture");


      public override void DrawPerTextureGUI(int index, Material mat, MicroSplatPropData propData)
      {
         InitPropData(5, propData, new Color(1.0f, 1.0f, 1.0f, 1.0f)); //geoTexture, global tint, global normal
         if (geoTexture)
         {
            perTexGeoStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGEO),
               mat, propData, Channel.R,  CPerTexGeo, 0, 1);
         }
         if (tintBlendMode != BlendMode.Off)
         {
            perTexTintStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGLOBALTINTSTRENGTH),
               mat, propData, Channel.G,  CPerTexTint, 0, 1);
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            perTexNormalStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGLOBALNORMALSTRENGTH),
               mat, propData, Channel.B,  CPerTexNormal, 0, 2);
         }
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_geomap.txt"))
            {
               properties_geomap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_geomap.txt"))
            {
               function_geomap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_globalnormal.txt"))
            {
               properties_normal = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_globaltint.txt"))
            {
               properties_tint = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_globalparams.txt"))
            {
               properties_params = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
             
         }
      } 

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (geoTexture)
         {
            sb.Append(properties_geomap.text);
         }
         if (tintBlendMode != BlendMode.Off)
         {
            sb.Append(properties_tint.text);
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            sb.Append(properties_normal.text);
         }

         if (tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off)
         {
            sb.Append(properties_params.text);
         }

      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (geoTexture)
         {
            textureSampleCount++;
            if (geoCurve)
            {
               textureSampleCount++;
            }
         }
         if (tintBlendMode != BlendMode.Off)
         {
            textureSampleCount++;
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            textureSampleCount++;
         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (geoTexture)
         {
            features.Add(GetFeatureName(DefineFeature._GEOMAP));
            if (perTexGeoStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGEO));
            }
            if (geoRange)
            {
               features.Add(GetFeatureName(DefineFeature._GEORANGE));
            }
            if (geoCurve)
            {
               features.Add(GetFeatureName(DefineFeature._GEOCURVE));
            }
         }

         if (tintBlendMode != BlendMode.Off)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALTINT));
            if (perTexTintStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGLOBALTINTSTRENGTH));
            }
            if (tintBlendMode == BlendMode.Multiply2X)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALTINTMULT2X));
            }
            else if (tintBlendMode == BlendMode.Overlay)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALTINTOVERLAY));
            }
            else
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALTINTCROSSFADE));
            }
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALNORMALS));
            if (perTexNormalStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGLOBALNORMALSTRENGTH));
            }
            if (normalBlendMode == NormalBlendMode.CrossFade)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALNORMALCROSSFADE));
            }
         }

         return features.ToArray();
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (geoTexture || tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off)
         {
            sb.AppendLine(function_geomap.text);
         }

      }

      public override void Unpack(string[] keywords)
      {
         geoTexture = HasFeature(keywords, DefineFeature._GEOMAP);
         if (geoTexture)
         {
            perTexGeoStr = HasFeature(keywords, DefineFeature._PERTEXGEO);
            geoRange = HasFeature(keywords, DefineFeature._GEORANGE);
            geoCurve = HasFeature(keywords, DefineFeature._GEOCURVE);
         }
            
         perTexTintStr = HasFeature(keywords, DefineFeature._PERTEXGLOBALTINTSTRENGTH);

         tintBlendMode = BlendMode.Off;
         if (HasFeature(keywords, DefineFeature._GLOBALTINT))
         {
            tintBlendMode = BlendMode.Multiply2X;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALTINTOVERLAY))
         {
            tintBlendMode = BlendMode.Overlay;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALTINTCROSSFADE))
         {
            tintBlendMode = BlendMode.CrossFade;
         }

         normalBlendMode = NormalBlendMode.Off;
         if (HasFeature(keywords, DefineFeature._GLOBALNORMALS))
         {
            normalBlendMode = NormalBlendMode.NormalBlend;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALNORMALCROSSFADE))
         {
            normalBlendMode = NormalBlendMode.CrossFade;
         }

         perTexNormalStr = HasFeature(keywords, DefineFeature._PERTEXGLOBALNORMALSTRENGTH);
      }

   }   
   #endif


}