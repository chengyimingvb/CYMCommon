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
   public class MicroSplatWindsAndGlitter : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_WINDGLITTER__";
      static MicroSplatWindsAndGlitter()
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
         return "Winds and Glitter";
      }

      public enum DefineFeature
      {
         _WINDPARTICULATE,
         _PERTEXWINDPARTICULATE,
         _SNOWPARTICULATE,
         _GLITTER,
         _SNOWGLITTER,
         _WINDSHADOWS,
         _SNOWSHADOWS,
         _GLOBALSNOWPARTICULATESTRENGTH,
         _GLOBALWINDPARTICULATESTRENGTH,
         _GLOBALPARTICULATEROTATION,
         _WINDPARTICULATEUPFILTER,
         _SNOWPARTICULATEUPFILTER,
         kNumFeatures,
      }

      public enum ParticulateMode
      {
         None,
         Particulate,
         ParticulateWithShadows
      };
         
      static TextAsset part_properties;
      static TextAsset part_funcs;
      static TextAsset glitter_properties;
      static TextAsset glitter_funcs;
      static TextAsset snowglitter_properties;
      static TextAsset snowglitter_funcs;


      public ParticulateMode windParticulate = ParticulateMode.None;
      public bool perTexParticulate;
      public ParticulateMode snowParticulate = ParticulateMode.None;
      public bool glitter;
      public bool snowGlitter;
      public bool globalWindRotation;
      public bool globalWindStrength;
      public bool globalSnowStrength;
      public bool windUpFilter;
      public bool snowUpFilter;


      GUIContent CWindParticulate = new GUIContent("Wind Particulate", "Turn on wind particulate, with or without shadows");
      GUIContent CSnowParticulate = new GUIContent("Snow Particulate", "Turn on show particulate, with or without shadows");
      GUIContent CGlitter = new GUIContent("Glitter Specular", "Glittery specular effect with shifting grains");
      GUIContent CSnowGlitter = new GUIContent("Snow Glitter Specular", "Glittery specular effect with shifting grains");
      GUIContent CUpFilter = new GUIContent("Slope Filter", "Filter particulate based on going up or down");
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
         windParticulate = (ParticulateMode)EditorGUILayout.EnumPopup(CWindParticulate, windParticulate);
         if (windParticulate != ParticulateMode.None)
         {
            EditorGUI.indentLevel++;
            windUpFilter = EditorGUILayout.Toggle(CUpFilter, windUpFilter);
            EditorGUI.indentLevel--;
         }
         glitter = EditorGUILayout.Toggle(CGlitter, glitter);
         if (mat.IsKeywordEnabled("_SNOW"))
         {
            snowParticulate = (ParticulateMode)EditorGUILayout.EnumPopup(CSnowParticulate, snowParticulate);
            if (snowParticulate != ParticulateMode.None)
            {
               EditorGUI.indentLevel++;
               snowUpFilter = EditorGUILayout.Toggle(CUpFilter, windUpFilter);
               EditorGUI.indentLevel--;
            }
            snowGlitter = EditorGUILayout.Toggle(CSnowGlitter, snowGlitter);
         }
         else
         {
            snowParticulate = ParticulateMode.None;
            snowGlitter = false;
         }
      }

      static GUIContent CWindStretch = new GUIContent("Stretch", "Stretching of UV in wind direction");
      static GUIContent CWindScale = new GUIContent("Scale", "Scale of wind particulate streams");
      static GUIContent CWindRotation = new GUIContent("Rotation", "Rotation of wind texture");
      static GUIContent CWindSpeed = new GUIContent("Speed", "Speed of motion");
      static GUIContent CWindContrast = new GUIContent("Contrast", "Contrast of particulate effect");
      static GUIContent CWindColor = new GUIContent("Color", "Color for particulate matter");
      static GUIContent CWindStrength = new GUIContent("Strength", "Master strength of the effect");
      static GUIContent CWindShadowColor = new GUIContent("Shadow Color", "Color for shadowed areas");
      static GUIContent CWindShadowOffset = new GUIContent("Shadow Offset", "Offset for shadow depth");
      static GUIContent CWindShadowStrength = new GUIContent("Shadow Strength", "Strength of shadow effect");
      static GUIContent CWindHeightMask = new GUIContent("Height Mask", "X=Begin Height, Y=Height at full strength, Z=End Full Strength, W=Fully Faded");
      static GUIContent CWindAngleMask = new GUIContent("Angle Mask", "X=Begin Angle, Y=Angle at full strength, Z=End Full Strength, W=Fully Faded");
      static GUIContent CWindEmissive = new GUIContent("Emissive Strength", "Apply color to emissive channel");
      static GUIContent CUpFilterRange = new GUIContent("Slope Range", "-1 = down, 1 = up; X = Begin, Y = end fade in, Z = begin fade out, W = faded out. Example: -1, -1, -0.9, 0.1 = only on downhill");

      static GUIContent CGlitterWind = new GUIContent("Effect Texture", "Glitter Texture in RGB, Wind in A");
      static GUIContent CGlitterGraininess = new GUIContent("Graininess", "How subtle are the grains");
      static GUIContent CGlitterShininess = new GUIContent("Shininess", "Width of the specular hightlight");
      static GUIContent CGlitterViewDep = new GUIContent("View Dependency", "How much grains are based on view direction");
      static GUIContent CGlitterUVScale = new GUIContent("UV Scale", "UV Scale for the grain texture");
      static GUIContent CGlitterStrength = new GUIContent("Strength", "Master strength of the effect");
      static GUIContent CGlitterThreshold = new GUIContent("Threshold", "Intensity of Glitter");

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (windParticulate != ParticulateMode.None || snowParticulate != ParticulateMode.None || glitter || snowGlitter)
         {
            if (MicroSplatUtilities.DrawRollup("Wind Particulate and Glitter") && mat.HasProperty("_GlitterWind"))
            {
               var texProp = shaderGUI.FindProp("_GlitterWind", props);
               MicroSplatUtilities.WarnLinear(texProp.textureValue as Texture2D);
               materialEditor.TexturePropertySingleLine(CGlitterWind, texProp);
               MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_windglitter");

               if ((windParticulate != ParticulateMode.None|| snowParticulate != ParticulateMode.None) && mat.HasProperty("_WindParticulateRotation"))
               {
                  EditorGUILayout.BeginHorizontal();
                  var oldEnabled = GUI.enabled;
                  if (globalWindRotation)
                     GUI.enabled = false;
                  materialEditor.ShaderProperty(shaderGUI.FindProp("_WindParticulateRotation", props), CWindRotation);
                  GUI.enabled = oldEnabled;
                  globalWindRotation = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALPARTICULATEROTATION), mat);
                  EditorGUILayout.EndHorizontal();


                  if (windParticulate != ParticulateMode.None && MicroSplatUtilities.DrawRollup("Wind", true, true) && mat.HasProperty("_WindParticulateColor"))
                  {

                     materialEditor.ShaderProperty(shaderGUI.FindProp("_WindParticulateColor", props), CWindColor);

                     EditorGUILayout.BeginHorizontal();
                     oldEnabled = GUI.enabled;
                     if (globalWindStrength)
                        GUI.enabled = false;

                     materialEditor.ShaderProperty(shaderGUI.FindProp("_WindParticulateStrength", props), CWindStrength);
                     GUI.enabled = oldEnabled;
                     globalWindStrength = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALWINDPARTICULATESTRENGTH), mat);
                     EditorGUILayout.EndHorizontal();


                     EditorGUI.BeginChangeCheck();
                     Vector4 speedPow = shaderGUI.FindProp("_WindParticulateParams", props).vectorValue;
                     speedPow.w = EditorGUILayout.FloatField(CWindScale, speedPow.w);
                     speedPow.x = EditorGUILayout.FloatField(CWindSpeed, speedPow.x);
                     speedPow.y = EditorGUILayout.Slider(CWindContrast, speedPow.y, 0.4f, 4.0f);
                     speedPow.z = EditorGUILayout.FloatField(CWindStretch, speedPow.z); 

                     if (EditorGUI.EndChangeCheck())
                     {
                        shaderGUI.FindProp("_WindParticulateParams", props).vectorValue = speedPow;
                     }

                     if (windParticulate == ParticulateMode.ParticulateWithShadows)
                     {
                        EditorGUI.BeginChangeCheck();
                        Vector4 shadow = shaderGUI.FindProp("_WindParticulateShadow", props).vectorValue;
                        Color shadowColor = shaderGUI.FindProp("_WindParticulateShadowColor", props).colorValue;
                        shadowColor = EditorGUILayout.ColorField(CWindShadowColor, shadowColor);
                        shadow.x = EditorGUILayout.Slider(CWindShadowOffset, shadow.x, 0, 0.1f);
                        shadow.y = EditorGUILayout.Slider(CWindShadowStrength, shadow.y, 0, 2.0f);

                        if (EditorGUI.EndChangeCheck())
                        {
                           shaderGUI.FindProp("_WindParticulateShadow", props).vectorValue = shadow;
                           shaderGUI.FindProp("_WindParticulateShadowColor", props).colorValue = shadowColor;
                        }
                     }
                     if (mat.HasProperty("_WindParticulateHeightMask"))
                     {
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_WindParticulateHeightMask", props), CWindHeightMask);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_WindParticulateAngleMask", props), CWindAngleMask);
                        if (mat.HasProperty("_WindParticulateUpMask"))
                        {
                           materialEditor.ShaderProperty(shaderGUI.FindProp("_WindParticulateUpMask", props), CUpFilterRange);
                        }
                     }

                     if (mat.HasProperty("_WindEmissive"))
                     {
                        Vector4 windEmis = shaderGUI.FindProp("_WindEmissive", props).vectorValue;
                        EditorGUI.BeginChangeCheck();
                        windEmis.x = EditorGUILayout.Slider(CWindEmissive, windEmis.x, 0, 1);
                        if (EditorGUI.EndChangeCheck())
                        {
                           shaderGUI.FindProp("_WindEmissive", props).vectorValue = windEmis;
                        }
                     }


                  }

                  if (snowParticulate != ParticulateMode.None && MicroSplatUtilities.DrawRollup("Snow Particulate Settings", true, true) && mat.HasProperty("_SnowParticulateColor"))
                  {
                     materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowParticulateColor", props), CWindColor);

                     EditorGUILayout.BeginHorizontal();
                     oldEnabled = GUI.enabled;
                     if (globalSnowStrength)
                        GUI.enabled = false;
                     
                     materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowParticulateStrength", props), CWindStrength);
                     GUI.enabled = oldEnabled;
                     globalSnowStrength = DrawGlobalToggle(GetFeatureName(DefineFeature._GLOBALSNOWPARTICULATESTRENGTH), mat);
                     EditorGUILayout.EndHorizontal();

                     EditorGUI.BeginChangeCheck();
                     Vector4 speedPow = shaderGUI.FindProp("_SnowParticulateParams", props).vectorValue;
                     speedPow.w = EditorGUILayout.FloatField(CWindScale, speedPow.w);
                     speedPow.x = EditorGUILayout.FloatField(CWindSpeed, speedPow.x);
                     speedPow.y = EditorGUILayout.Slider(CWindContrast, speedPow.y, 0.4f, 4.0f);
                     speedPow.z = EditorGUILayout.FloatField(CWindStretch, speedPow.z); 

                     if (EditorGUI.EndChangeCheck())
                     {
                        shaderGUI.FindProp("_SnowParticulateParams", props).vectorValue = speedPow;
                     }

                     if (snowParticulate == ParticulateMode.ParticulateWithShadows)
                     {
                        EditorGUI.BeginChangeCheck();
                        Vector4 shadow = shaderGUI.FindProp("_SnowParticulateShadow", props).vectorValue;
                        Color shadowColor = shaderGUI.FindProp("_SnowParticulateShadowColor", props).colorValue;
                        shadowColor = EditorGUILayout.ColorField(CWindShadowColor, shadowColor);
                        shadow.x = EditorGUILayout.Slider(CWindShadowOffset, shadow.x, 0, 0.1f);
                        shadow.y = EditorGUILayout.Slider(CWindShadowStrength, shadow.y, 0, 2.0f);

                        if (EditorGUI.EndChangeCheck())
                        {
                           shaderGUI.FindProp("_SnowParticulateShadow", props).vectorValue = shadow;
                           shaderGUI.FindProp("_SnowParticulateShadowColor", props).colorValue = shadowColor;
                        }

                     }

                     if (mat.HasProperty("_SnowParticulateHeightMask"))
                     {
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowParticulateHeightMask", props), CWindHeightMask);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowParticulateAngleMask", props), CWindAngleMask);
                        if (mat.HasProperty("_SnowParticulateUpMask"))
                        {
                           materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowParticulateUpMask", props), CUpFilterRange);
                        }
                     }

                     if (mat.HasProperty("_WindEmissive"))
                     {
                        Vector4 windEmis = shaderGUI.FindProp("_WindEmissive", props).vectorValue;
                        EditorGUI.BeginChangeCheck();
                        windEmis.y = EditorGUILayout.Slider(CWindEmissive, windEmis.y, 0, 1);
                        if (EditorGUI.EndChangeCheck())
                        {
                           shaderGUI.FindProp("_WindEmissive", props).vectorValue = windEmis;
                        }
                     }

                  }
               }
               if ((glitter || snowGlitter) )
               {
                  if (glitter && mat.HasProperty("_GlitterUVScale"))
                  {
                     if (MicroSplatUtilities.DrawRollup("Glitter", true, true))
                     {
                        var scale = shaderGUI.FindProp("_GlitterUVScale", props);
                        Vector2 scl = scale.vectorValue;
                        EditorGUI.BeginChangeCheck();
                        scl = EditorGUILayout.Vector2Field(CGlitterUVScale, scl);
                        if (EditorGUI.EndChangeCheck())
                        {
                           scale.vectorValue = scl;
                        }
                           
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_GlitterGraininess", props), CGlitterGraininess);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_GlitterShininess", props), CGlitterShininess);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_GlitterViewDep", props), CGlitterViewDep);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_GlitterStrength", props), CGlitterStrength);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_GlitterThreshold", props), CGlitterThreshold);

                        if (mat.HasProperty("_GlitterDistFade"))
                        {
                           Vector4 fade = mat.GetVector("_GlitterDistFade");
                           EditorGUI.BeginChangeCheck();

                           fade.x = EditorGUILayout.FloatField("Begin Fade", fade.x);
                           fade.z = EditorGUILayout.Slider("Opacity At Begin", fade.z, 0, 1);
                           fade.y = EditorGUILayout.FloatField("Fade Range", fade.y);
                           fade.w = EditorGUILayout.Slider("Opacity At End", fade.w, 0, 1);

                           if (EditorGUI.EndChangeCheck())
                           {
                              mat.SetVector("_GlitterDistFade", fade);
                              EditorUtility.SetDirty(mat);
                           }
                        }
                     }
                  }
                  if (snowGlitter && mat.HasProperty("_SnowGlitterUVScale"))
                  {
                     if (MicroSplatUtilities.DrawRollup("Snow Glitter", true, true))
                     {
                        var scale = shaderGUI.FindProp("_SnowGlitterUVScale", props);
                        Vector2 scl = scale.vectorValue;
                        EditorGUI.BeginChangeCheck();
                        scl = EditorGUILayout.Vector2Field(CGlitterUVScale, scl);
                        if (EditorGUI.EndChangeCheck())
                        {
                           scale.vectorValue = scl;
                        }

                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowGlitterGraininess", props), CGlitterGraininess);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowGlitterShininess", props), CGlitterShininess);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowGlitterViewDep", props), CGlitterViewDep);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowGlitterStrength", props), CGlitterStrength);
                        materialEditor.ShaderProperty(shaderGUI.FindProp("_SnowGlitterThreshold", props), CGlitterThreshold);

                        if (mat.HasProperty("_SnowGlitterDistFade"))
                        {
                           Vector4 fade = mat.GetVector("_SnowGlitterDistFade");
                           EditorGUI.BeginChangeCheck();

                           fade.x = EditorGUILayout.FloatField("Begin Fade", fade.x);
                           fade.z = EditorGUILayout.Slider("Opacity At Begin", fade.z, 0, 1);
                           fade.y = EditorGUILayout.FloatField("Fade Range", fade.y);
                           fade.w = EditorGUILayout.Slider("Opacity At End", fade.w, 0, 1);

                           if (EditorGUI.EndChangeCheck())
                           {
                              mat.SetVector("_SnowGlitterDistFade", fade);
                              EditorUtility.SetDirty(mat);
                           }
                        }
                     }
                  }

               }

            }
         }
        
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (windParticulate != ParticulateMode.None)
         {
            features.Add(GetFeatureName(DefineFeature._WINDPARTICULATE));
            if (windParticulate == ParticulateMode.ParticulateWithShadows)
            {
               features.Add(GetFeatureName(DefineFeature._WINDSHADOWS));
            }
            if (windParticulate != ParticulateMode.None && windUpFilter)
            {
               features.Add(GetFeatureName(DefineFeature._WINDPARTICULATEUPFILTER));
            }
            if (perTexParticulate)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXWINDPARTICULATE));
            }
         }

         if (glitter)
         {
            features.Add(GetFeatureName(DefineFeature._GLITTER));
         }


         if (snowParticulate != ParticulateMode.None)
         {
            features.Add(GetFeatureName(DefineFeature._SNOWPARTICULATE));
            if (snowParticulate == ParticulateMode.ParticulateWithShadows)
            {
               features.Add(GetFeatureName(DefineFeature._SNOWSHADOWS));
            }
            if (snowParticulate != ParticulateMode.None && snowUpFilter)
            {
               features.Add(GetFeatureName(DefineFeature._SNOWPARTICULATEUPFILTER));
            }
         }
         if (snowGlitter)
         {
            features.Add(GetFeatureName(DefineFeature._SNOWGLITTER));
         }

         if (globalWindRotation)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALPARTICULATEROTATION));
         }
         if (globalWindStrength)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALWINDPARTICULATESTRENGTH));
         }
         if (globalSnowStrength)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALSNOWPARTICULATESTRENGTH));
         }
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         windParticulate = ParticulateMode.None;
         snowParticulate = ParticulateMode.None;
         if (HasFeature(keywords, DefineFeature._WINDPARTICULATE))
         {
            windParticulate = HasFeature(keywords, DefineFeature._WINDSHADOWS) ? ParticulateMode.ParticulateWithShadows : ParticulateMode.Particulate;
         }

         if (HasFeature(keywords, DefineFeature._SNOWPARTICULATE))
         {
            snowParticulate = HasFeature(keywords, DefineFeature._SNOWSHADOWS) ? ParticulateMode.ParticulateWithShadows : ParticulateMode.Particulate;
         }

         snowUpFilter = snowParticulate != ParticulateMode.None && HasFeature(keywords, DefineFeature._SNOWPARTICULATEUPFILTER);
         windUpFilter = windParticulate != ParticulateMode.None && HasFeature(keywords, DefineFeature._WINDPARTICULATEUPFILTER);

         perTexParticulate = HasFeature(keywords, DefineFeature._PERTEXWINDPARTICULATE);
         glitter = HasFeature(keywords, DefineFeature._GLITTER);
         snowGlitter = HasFeature(keywords, DefineFeature._SNOWGLITTER);

         globalWindRotation = HasFeature(keywords, DefineFeature._GLOBALPARTICULATEROTATION);
         globalWindStrength = HasFeature(keywords, DefineFeature._GLOBALWINDPARTICULATESTRENGTH);
         globalSnowStrength = HasFeature(keywords, DefineFeature._GLOBALSNOWPARTICULATESTRENGTH);

      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_windparticulate.txt"))
            {
               part_properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_windparticulate.txt"))
            {
               part_funcs = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_glitter.txt"))
            {
               glitter_properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_glitter.txt"))
            {
               glitter_funcs = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_snowglitter.txt"))
            {
               snowglitter_properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_snowglitter.txt"))
            {
               snowglitter_funcs = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (windParticulate != ParticulateMode.None || snowParticulate != ParticulateMode.None || glitter || snowGlitter)
         {
            sb.AppendLine("      _GlitterWind (\"Glitter Wind Map\", 2D) = \"black\" {}");
         }
            
         if (windParticulate != ParticulateMode.None || snowParticulate != ParticulateMode.None)
         {
            sb.AppendLine(part_properties.text);
         }

         if (windParticulate != ParticulateMode.None && windUpFilter)
         {
            sb.AppendLine("      _WindParticulateUpMask(\"Up Mask\", Vector) = (-1, -1, 1, 1)");
         }
         if (snowParticulate != ParticulateMode.None && snowUpFilter)
         {
            sb.AppendLine("      _SnowParticulateUpMask(\"Up Mask\", Vector) = (-1, -1, 1, 1)");
         }

         if (glitter || snowGlitter)
         {

            if (glitter)
            {
               sb.AppendLine(glitter_properties.text);
            }
            if (snowGlitter)
            {
               sb.AppendLine(snowglitter_properties.text);
            }
         }
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (windParticulate != ParticulateMode.None || snowParticulate != ParticulateMode.None || glitter || snowGlitter)
         {
            sb.AppendLine("      UNITY_DECLARE_TEX2D_NOSAMPLER(_GlitterWind);");
         }

         if (windParticulate != ParticulateMode.None || snowParticulate != ParticulateMode.None)
         {
            sb.Append(part_funcs.text);
         }
         if (glitter || snowGlitter)
         {
            
            if (glitter)
            {
               sb.AppendLine(glitter_funcs.text);
            }
            if (snowGlitter)
            {
               sb.AppendLine(snowglitter_funcs.text);
            }
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (windParticulate == ParticulateMode.Particulate)
         {
            textureSampleCount += 2;
         }
         else if (windParticulate == ParticulateMode.ParticulateWithShadows)
         {
            textureSampleCount += 4;
         }
         if (snowParticulate == ParticulateMode.Particulate)
         {
            textureSampleCount += 2;
         }
         else if (snowParticulate == ParticulateMode.ParticulateWithShadows)
         {
            textureSampleCount += 4;
         }
         if (glitter)
         {
            textureSampleCount += 2;
         }
         if (snowGlitter)
         {
            textureSampleCount += 2;
         }

      }


      static GUIContent CPTWindParticulateStrength = new GUIContent("Wind Particulat Strength", "How strong is the wind particulate on this surface");
      static GUIContent CPTGlitterStrength = new GUIContent("Glitter Strength", "How string is the glitter on this texture");

      public override void DrawPerTextureGUI(int index, Material mat, MicroSplatPropData propData)
      {
         if (windParticulate != ParticulateMode.None)
         {
            perTexParticulate = DrawPerTexFloatSlider(index, 7, GetFeatureName(DefineFeature._PERTEXWINDPARTICULATE), mat, propData, Channel.A, CPTWindParticulateStrength, 0, 1.0f);
         }
         if (glitter)
         {
            DrawPerTexFloatSliderNoToggle(index, 8, GetFeatureName(DefineFeature._GLITTER), mat, propData, Channel.G, CPTGlitterStrength, 0, 1.0f);
         }
      }
   }   

   #endif
}