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
   public class MicroSplatTextureClusters : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_TEXTURECLUSTERS__";
      static MicroSplatTextureClusters()
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
         return "Texture Clusters";
      }

      public enum DefineFeature
      {
         _TEXTURECLUSTER2,
         _TEXTURECLUSTER3,
         _PERTEXCLUSTERCONTRAST,
         _PERTEXCLUSTERBOOST,
         _TEXTURECLUSTERTRIPLANARNOISE,
         _TEXTURECLUSTERNOISE2,
         _STOCHASTIC,
         kNumFeatures,
      }
         
      public enum ClusterMode
      {
         None,
         TwoVariants,
         ThreeVariants,
         Stochastic
      }

      public enum ClusterNoiseUV
      {
         UV,
         Triplanar
      }

      public ClusterMode clusterMode = ClusterMode.None;
      public ClusterNoiseUV clusterNoiseUV = ClusterNoiseUV.UV;
      public bool perTexClusterContrast;
      public bool perTexClusterBoost;
      public bool secondNoise;

      public TextAsset properties;
      public TextAsset functions;
      public TextAsset stochasticFunctions;
      public TextAsset stochasticProperties;

      GUIContent CShaderClusters = new GUIContent("Texture Cluster Mode", "Number of parallel arrays to sample for clustering, or use Stochastic sampler");
      GUIContent CClusterNoiseUVs = new GUIContent("Cluster Noise UV Mode", "The noise for clusetrs can be triplanar, which can be useful when UVs are not continuous");
      GUIContent CClusterNoise2 = new GUIContent("Use secondary cluster noise", "On very large worlds, cluster noise tile may be visible. You can use a second noise, combined with the first, to prevent this");
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
         clusterMode = (ClusterMode)EditorGUILayout.EnumPopup(CShaderClusters, clusterMode);
         if (clusterMode != ClusterMode.None && clusterMode != ClusterMode.Stochastic)
         {
            EditorGUI.indentLevel++;
            clusterNoiseUV = (ClusterNoiseUV)EditorGUILayout.EnumPopup(CClusterNoiseUVs, clusterNoiseUV);
            secondNoise = EditorGUILayout.Toggle(CClusterNoise2, secondNoise);
            EditorGUI.indentLevel--;
         }
      }

      public override int CompileSortOrder()
      {
         return -100;   // first, so we can redefine sampling macros..
      }

      static GUIContent CAlbedoTex2 = new GUIContent("Albedo2", "Second Albedo array");
      static GUIContent CNormalTex2 = new GUIContent("Normal2", "Second Normal array");
      static GUIContent CAlbedoTex3 = new GUIContent("Albedo3", "Third Albedo array");
      static GUIContent CNormalTex3 = new GUIContent("Normal3", "Third Normal array");
      static GUIContent CNoiseTex = new GUIContent("Cluster Noise", "Cluster Noise texture, with weights for each texture in RGB");
      static GUIContent CInterpContrast = new GUIContent("Cluster Contrast", "Interpolation contrast for texture clusters");
      static GUIContent CClusterScale = new GUIContent("Scale Variation", "Variation in scale of cluster layers");
      static GUIContent CClusterBoost = new GUIContent("Noise Boost", "Increase or reduce the amount of blending between textures in the cluster");
      static GUIContent CEmis2 = new GUIContent("Emissive2", "Second Emissive Array");
      static GUIContent CEmis3 = new GUIContent("Emissive3", "Third Emissive Array");

      static GUIContent CSmoothAO2 = new GUIContent("Smoothness/AO2", "Second smoothness/ao array");
      static GUIContent CSmoothAO3 = new GUIContent("Smoothness/AO3", "Third smoothness/ao array");

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (clusterMode != ClusterMode.None)
         {
            if (MicroSplatUtilities.DrawRollup("Texture Clustering"))
            {
               if (clusterMode == ClusterMode.Stochastic)
               {
                  if (mat.HasProperty("_StochasticContrast"))
                  {
                     materialEditor.RangeProperty(shaderGUI.FindProp("_StochasticContrast", props), "Blend Contrast");
                  }
                  if (mat.HasProperty("_StochasticScale"))
                  {
                     materialEditor.RangeProperty(shaderGUI.FindProp("_StochasticScale", props), "Noise Scale");
                  }
               }
               else
               {
                  if (mat.HasProperty("_ClusterNoise"))
                  {
                     var noiseMap = shaderGUI.FindProp("_ClusterNoise", props);
                     var albedoMap = shaderGUI.FindProp("_ClusterDiffuse2", props);
                     var normalMap = shaderGUI.FindProp("_ClusterNormal2", props);
                     var noiseParams = shaderGUI.FindProp("_ClusterParams", props);


                     materialEditor.TexturePropertySingleLine(CAlbedoTex2, albedoMap);
                     materialEditor.TexturePropertySingleLine(CNormalTex2, normalMap);

                     if (mat.HasProperty("_ClusterSmoothAO2"))
                     {
                        var smoothAO = shaderGUI.FindProp("_ClusterSmoothAO2", props);
                        materialEditor.TexturePropertySingleLine(CSmoothAO2, smoothAO);
                     }

                     if (mat.HasProperty("_ClusterEmissiveMetal2"))
                     {
                        var emis2 = shaderGUI.FindProp("_ClusterEmissiveMetal2", props);
                        materialEditor.TexturePropertySingleLine(CEmis2, emis2);
                     }



                     if (clusterMode == ClusterMode.ThreeVariants)
                     {
                        var albedoMap3 = shaderGUI.FindProp("_ClusterDiffuse3", props);
                        var normalMap3 = shaderGUI.FindProp("_ClusterNormal3", props);

                        materialEditor.TexturePropertySingleLine(CAlbedoTex3, albedoMap3);
                        materialEditor.TexturePropertySingleLine(CNormalTex3, normalMap3);

                        if (mat.HasProperty("_ClusterSmoothAO3"))
                        {
                           var smoothAO = shaderGUI.FindProp("_ClusterSmoothAO3", props);
                           materialEditor.TexturePropertySingleLine(CSmoothAO3, smoothAO);
                        }


                        if (mat.HasProperty("_ClusterEmissiveMetal3"))
                        {
                           var emis3 = shaderGUI.FindProp("_ClusterEmissiveMetal3", props);
                           materialEditor.TexturePropertySingleLine(CEmis3, emis3);
                        }
                     }

                     materialEditor.TexturePropertySingleLine(CNoiseTex, noiseMap);
                     MicroSplatUtilities.EnforceDefaultTexture(noiseMap, "microsplat_def_clusternoise");
                     if (secondNoise && mat.HasProperty("_ClusterNoise2"))
                     {
                        var noiseMap2 = shaderGUI.FindProp("_ClusterNoise2", props);
                        materialEditor.TexturePropertySingleLine(CNoiseTex, noiseMap2);
                        MicroSplatUtilities.EnforceDefaultTexture(noiseMap, "microsplat_def_clusternoise");
                     }

                     bool enabled = GUI.enabled;
                     if (perTexClusterContrast)
                     {
                        GUI.enabled = false;
                     }
                     var contrastProp = shaderGUI.FindProp("_ClusterContrast", props);
                     contrastProp.floatValue = EditorGUILayout.Slider(CInterpContrast, contrastProp.floatValue, 1.0f, 0.0001f);
                     if (perTexClusterContrast)
                     {
                        GUI.enabled = enabled;
                     }

                     if (perTexClusterBoost)
                     {
                        GUI.enabled = false;
                     }
                     var boostProp = shaderGUI.FindProp("_ClusterBoost", props);
                     boostProp.floatValue = EditorGUILayout.Slider(CClusterBoost, boostProp.floatValue, 0.5f, 4.0f);
                     if (perTexClusterBoost)
                     {
                        GUI.enabled = enabled;
                     }


                     var skew = shaderGUI.FindProp("_ClusterScaleVar", props);
                     skew.floatValue = EditorGUILayout.Slider(CClusterScale, skew.floatValue, 0.0f, 0.2f);


                     {
                        Vector4 vec = noiseParams.vectorValue;
                        EditorGUI.BeginChangeCheck();
                        Vector2 scale = new Vector2(vec.x, vec.y);
                        Vector2 offset = new Vector2(vec.z, vec.w);

                        scale = EditorGUILayout.Vector2Field("Scale", scale);
                        offset = EditorGUILayout.Vector2Field("Offset", offset);

                        if (EditorGUI.EndChangeCheck())
                        {
                           vec.x = scale.x;
                           vec.y = scale.y;
                           vec.z = offset.x;
                           vec.w = offset.y;
                           noiseParams.vectorValue = vec;
                        }
                     }
                     if (mat.HasProperty("_ClusterParams2"))
                     {
                        EditorGUILayout.LabelField("Second Octave Noise");
                        var noiseParams2 = shaderGUI.FindProp("_ClusterParams2", props);
                        Vector4 vec = noiseParams2.vectorValue;
                        EditorGUI.BeginChangeCheck();
                        Vector2 scale = new Vector2(vec.x, vec.y);
                        Vector2 offset = new Vector2(vec.z, vec.w);

                        scale = EditorGUILayout.Vector2Field("Scale", scale);
                        offset = EditorGUILayout.Vector2Field("Offset", offset);

                        if (EditorGUI.EndChangeCheck())
                        {
                           vec.x = scale.x;
                           vec.y = scale.y;
                           vec.z = offset.x;
                           vec.w = offset.y;
                           noiseParams2.vectorValue = vec;
                        }
                     }
                  }
               }
            }
         }
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_clusters.txt"))
            {
               properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_clusters.txt"))
            {
               functions = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_stochastic.txt"))
            {
               stochasticFunctions = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_stochastic.txt"))
            {
               stochasticProperties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      } 

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (clusterMode != ClusterMode.None)
         {
            if (clusterMode != ClusterMode.Stochastic)
            {

               sb.AppendLine(properties.text);
               if (clusterMode == ClusterMode.ThreeVariants)
               {
                  sb.AppendLine("[NoScaleOffset]_ClusterDiffuse3 (\"Diffuse Array\", 2DArray) = \"white\" {}");
                  sb.AppendLine("[NoScaleOffset]_ClusterNormal3 (\"Normal Array\", 2DArray) = \"bump\" {}");
               }
               if (features.Contains("_USEEMISSIVEMETAL"))
               {
                  sb.AppendLine("[NoScaleOffset]_ClusterEmissiveMetal2 (\"Emissive Array\", 2DArray) = \"black\" {}");
                  if (clusterMode == ClusterMode.ThreeVariants)
                  {
                     sb.AppendLine("[NoScaleOffset]_ClusterEmissiveMetal3 (\"Emissive Array\", 2DArray) = \"black\" {}");
                  }
               }
               if (features.Contains("_PACKINGHQ"))
               {
                  sb.AppendLine("      [NoScaleOffset]_ClusterSmoothAO2 (\"Smooth AO Array\", 2DArray) = \"black\" {}");
                  if (clusterMode == ClusterMode.ThreeVariants)
                  {
                     sb.AppendLine("      [NoScaleOffset]_ClusterSmoothAO3 (\"Smooth AO Array\", 2DArray) = \"black\" {}");
                  }
               }
               if (secondNoise)
               {
                  sb.AppendLine("_ClusterNoise2(\"Cluster Noise2\", 2D) = \"white\" { }");
                  sb.AppendLine("_ClusterParams2(\"Cluster Params\", Vector) = (3, 3, 0, 0)");
               }
            }
            else
            {
               sb.AppendLine(stochasticProperties.text);
               if (features.Contains("_USEEMISSIVEMETAL"))
               {
                  sb.AppendLine("[NoScaleOffset]_EmissiveMetalInv (\"Emissive Array\", 2DArray) = \"black\" {}");
               }
               if (features.Contains("_PACKINGHQ"))
               {
                  sb.AppendLine("      [NoScaleOffset]_ClusterSmoothAOInv (\"Smooth AO Array\", 2DArray) = \"black\" {}");
               }
            }
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (clusterMode == ClusterMode.TwoVariants)
         {
            arraySampleCount *= 2;
         }
         else if (clusterMode == ClusterMode.ThreeVariants)
         {
            arraySampleCount *= 3;
         }
         else if (clusterMode == ClusterMode.Stochastic)
         {
            arraySampleCount *= 3;
         }
         if (clusterMode != ClusterMode.None)
         {
            textureSampleCount++;
            if (secondNoise)
            {
               textureSampleCount++;
            }
            if (clusterNoiseUV == ClusterNoiseUV.Triplanar)
            {
               textureSampleCount += 2;
               if (secondNoise)
                  textureSampleCount += 2;
            }
         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (clusterMode == ClusterMode.TwoVariants)
         {
            features.Add(GetFeatureName(DefineFeature._TEXTURECLUSTER2));
         }
         else if (clusterMode == ClusterMode.ThreeVariants)
         {
            features.Add(GetFeatureName(DefineFeature._TEXTURECLUSTER3));
         }
         else if (clusterMode == ClusterMode.Stochastic)
         {
            features.Add(GetFeatureName(DefineFeature._STOCHASTIC));
         }

         if (clusterNoiseUV == ClusterNoiseUV.Triplanar)
         {
            features.Add(GetFeatureName(DefineFeature._TEXTURECLUSTERTRIPLANARNOISE));
         }

         if (perTexClusterContrast)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXCLUSTERCONTRAST));
         }
         if (perTexClusterBoost)
         {
            features.Add(GetFeatureName(DefineFeature._PERTEXCLUSTERBOOST));
         }
         if (secondNoise)
         {
            features.Add(GetFeatureName(DefineFeature._TEXTURECLUSTERNOISE2));
         }
         return features.ToArray();
      }

      public override void WriteFunctions(System.Text.StringBuilder sb)
      {
         if (clusterMode != ClusterMode.None)
         {
            if (clusterMode == ClusterMode.Stochastic)
            {
               sb.AppendLine(stochasticFunctions.text);
            }
            else
            {
               sb.AppendLine(functions.text);
            }
         }
      }

      public override void Unpack(string[] keywords)
      {
         clusterMode = ClusterMode.None;
         if (HasFeature(keywords, DefineFeature._TEXTURECLUSTER2))
         {
            clusterMode = ClusterMode.TwoVariants;
         }
         else if (HasFeature(keywords, DefineFeature._TEXTURECLUSTER3))
         {
            clusterMode = ClusterMode.ThreeVariants;
         }
         else if (HasFeature(keywords, DefineFeature._STOCHASTIC))
         {
            clusterMode = ClusterMode.Stochastic;
         }

         if (clusterMode != ClusterMode.None)
         {
            clusterNoiseUV = HasFeature(keywords, DefineFeature._TEXTURECLUSTERTRIPLANARNOISE) ? ClusterNoiseUV.Triplanar : ClusterNoiseUV.UV;
            perTexClusterContrast = HasFeature(keywords, DefineFeature._PERTEXCLUSTERCONTRAST);
            perTexClusterBoost = HasFeature(keywords, DefineFeature._PERTEXCLUSTERBOOST);
            secondNoise = HasFeature(keywords, DefineFeature._TEXTURECLUSTERNOISE2);
         }
         

      }

      static GUIContent CPerTexClusterContrast = new GUIContent("Cluster Contrast", "Contrast for height blending of cluster data");
      static GUIContent CPerTextureClusterBoost = new GUIContent("Cluster Boost", "Contrast the noise texture used for choosing textures");

      public override void DrawPerTextureGUI(int index, Material mat, MicroSplatPropData propData)
      {
         if (clusterMode != ClusterMode.None && clusterMode != ClusterMode.Stochastic)
         {
            perTexClusterContrast = DrawPerTexFloatSlider(index, 10, GetFeatureName(DefineFeature._PERTEXCLUSTERCONTRAST), 
               mat, propData, Channel.R, CPerTexClusterContrast, 1.0f, 0.01f);

            perTexClusterBoost = DrawPerTexFloatSlider(index, 10, GetFeatureName(DefineFeature._PERTEXCLUSTERBOOST), 
               mat, propData, Channel.G, CPerTextureClusterBoost, 0.5f, 4.0f);
         }
         
         
      }
   }   
#endif

}