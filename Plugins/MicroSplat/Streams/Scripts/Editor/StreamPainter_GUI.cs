//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__
   public partial class StreamPainterWindow : EditorWindow 
   {

      enum Tab
      {
         Wetness,
         Puddles,
         Streams,
         Lava,
      }

      string[] tabNames =
      {
         "Wetness",
         "Puddles",
         "Streams",
         "Lava",
      };
      Tab tab = Tab.Wetness;



      Texture2D SaveTexture(string path, Texture2D tex, bool overwrite = false)
      {
         if (overwrite || !System.IO.File.Exists(path))
         {
            var bytes = tex.EncodeToPNG();

            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            AssetImporter ai = AssetImporter.GetAtPath(path);
            TextureImporter ti = ai as TextureImporter;
            ti.sRGBTexture = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            var ftm = ti.GetDefaultPlatformTextureSettings();
            ftm.format = TextureImporterFormat.ARGB32;
            ti.SetPlatformTextureSettings(ftm);

            ti.mipmapEnabled = true;
            ti.isReadable = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.SaveAndReimport();
         }
         return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
      }

      void CreateTexture(Terrain t, string texNamePrefix = "", Material customMat = null)
      {
         // find/create manager
         var mgr = t.GetComponent<MicroSplatTerrain>();
         if (mgr == null)
         {
            return;
         }
         Texture2D tex = mgr.streamTexture;

         // if we still don't have a texture, create one
         if (tex == null)
         {
            tex = new Texture2D(t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, TextureFormat.ARGB32, false, true);
            mgr.streamTexture = tex;
            Color c = new Color(0, 0, 0, 0);
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  tex.SetPixel(x, y, c);
               }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;
            var path = MicroSplatUtilities.RelativePathFromAsset(t.terrainData);
            path += "/" + t.name + "_stream_data.png";
            mgr.streamTexture = SaveTexture(path, tex);
         }

         mgr.Sync();
      }


      bool VerifyData()
      {
         if (rawTerrains == null || rawTerrains.Count == 0)
            return false;

         for (int i = 0; i < rawTerrains.Count; ++i)
         {
            Terrain t = rawTerrains[i];
            if (t.materialType != Terrain.MaterialType.Custom || t.materialTemplate == null || !t.materialTemplate.IsKeywordEnabled("_MICROSPLAT"))
            {
               EditorGUILayout.HelpBox("Terrain(s) are not setup for MicroSplat, please set them up", MessageType.Error);
               return false;
            }
         }

         for (int i = 0; i < rawTerrains.Count; ++i)
         {
            Terrain t = rawTerrains[i];
            MicroSplatTerrain mst = t.GetComponent<MicroSplatTerrain>();

            if (mst != null)
            {
               var tex = mst.streamTexture;
               if (tex != null)
               {
                  AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
                  TextureImporter ti = ai as TextureImporter;
                  if (ti == null || !ti.isReadable)
                  {
                     EditorGUILayout.HelpBox("Control texture is not read/write", MessageType.Error);
                     if (GUILayout.Button("Fix it!"))
                     {
                        ti.isReadable = true;
                        ti.SaveAndReimport();
                     }
                     return false;
                  }
               
                  bool isLinear = ti.sRGBTexture == false;
                  bool isRGB32 = ti.textureCompression == TextureImporterCompression.Uncompressed && ti.GetDefaultPlatformTextureSettings().format == TextureImporterFormat.ARGB32;

                  if (isRGB32 == false || isLinear == false || ti.wrapMode == TextureWrapMode.Repeat)
                  {
                     EditorGUILayout.HelpBox("Control texture is not in the correct format (Uncompressed, linear, clamp, ARGB)", MessageType.Error);
                     if (GUILayout.Button("Fix it!"))
                     {
                  
                        ti.sRGBTexture = false;
                        ti.textureCompression = TextureImporterCompression.Uncompressed;
                        var ftm = ti.GetDefaultPlatformTextureSettings();
                        ftm.format = TextureImporterFormat.ARGB32;
                        ti.SetPlatformTextureSettings(ftm);

                        ti.mipmapEnabled = true;
                        ti.wrapMode = TextureWrapMode.Clamp;
                        ti.SaveAndReimport();
                     }
                     return false;
                  }
               }
            }
         }
         return true;

      }

      void DrawFillGUI(int channel)
      {
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Fill"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillTerrain(terrains[i], channel, paintValue);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }
         if (GUILayout.Button("Clear"))
         {
            if (OnBeginStroke != null)
            {
               OnBeginStroke(terrains);
            }
            for (int i = 0; i < terrains.Length; ++i)
            {
               FillTerrain(terrains[i], channel, 0);
               if (OnStokeModified != null)
               {
                  OnStokeModified(terrains[i], true);
               }
            }
            if (OnEndStroke != null)
            {
               OnEndStroke();
            }
         }
         EditorGUILayout.EndHorizontal();
      }

      float paintValue = 1;

      void DrawWetnessGUI()
      {
         if (MicroSplatUtilities.DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         DrawFillGUI(0);
         if (MicroSplatUtilities.DrawRollup("Raycast Wetness", true, true))
         {
            EditorGUILayout.HelpBox("This utility will raycast against your terrain, generating a wetness map which will wet uncovered terrain. You can then use the maximum wetess value to remove the effect, raising it when it rains", MessageType.Info);
            if (GUILayout.Button("Calculate"))
            {
               DoWetnessRaycast();
            }
         }
      }

      Vector2 slopeRange = new Vector2(0, 1);
      static GUIContent CSlopeRange = new GUIContent("Slope Range", "Filter strokes to only affect certain angles");

      void DoWetnessRaycast()
      {
         for (int i = 0; i < terrains.Length; ++i)
         {
            var terrain = terrains[i];
            var tex = terrain.terrainTex;
            RaycastHit hit;
            for (int x = 0; x < tex.width; ++x)
            {
               for (int y = 0; y < tex.height; ++y)
               {
                  Vector3 tp = TerrainToWorld(terrain.terrain, x, y, tex);
                  tp += Vector3.up * 500;
                  Ray ray = new Ray(tp, Vector3.down);
                  bool val = false;
                  if  (Physics.Raycast(ray, out hit))
                  {
                     if (hit.collider == terrain.collider || hit.collider.GetComponent<Terrain>() != null)
                     {
                        val = true;
                     }
                  }
                  Color c = tex.GetPixel(x, y);
                  c.r = val ? 1 : 0;
                  tex.SetPixel(x, y, c);
               }
            }
            tex.Apply();
         }
      }


      void DrawPuddlesGUI()
      {
         if (MicroSplatUtilities.DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         DrawFillGUI(1);

      }
         
      void DrawStreamGUI()
      {
         if (MicroSplatUtilities.DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         DrawFillGUI(2);
      }
         
      void DrawLavaGUI()
      {
         if (MicroSplatUtilities.DrawRollup("Brush Settings"))
         {
            DrawBrushSettingsGUI();
         }
         DrawFillGUI(3);
      }
         

      Vector2 scroll;
      void OnGUI()
      {
         if (VerifyData() == false)
         {
            EditorGUILayout.HelpBox("Please select a terrain to begin", MessageType.Info);
            return;
         }

         DrawSettingsGUI();
         DrawSaveGUI();
         tab = (Tab)GUILayout.Toolbar((int)tab, tabNames);

         bool hasWetness = false;
         bool hasPuddles = false;
         bool hasStreams = false;
         bool hasLava = false;

         for (int i = 0; i < terrains.Length; ++i)
         {
            var t = terrains[i];
            if (t != null && t.terrain != null && t.terrain.terrainData != null && t.terrain.materialTemplate != null)
            {
               if (!hasWetness)
                  hasWetness = t.terrain.materialTemplate.IsKeywordEnabled("_WETNESS");
               if (!hasPuddles)
                  hasPuddles = t.terrain.materialTemplate.IsKeywordEnabled("_PUDDLES");
               if (!hasStreams)
                  hasStreams = t.terrain.materialTemplate.IsKeywordEnabled("_STREAMS");
               if (!hasLava)
                  hasLava = t.terrain.materialTemplate.IsKeywordEnabled("_LAVA");
            }
         }

         if (tab == Tab.Wetness)
         {
            if (hasWetness)
            {
               DrawWetnessGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Wetness is not enabled on your terrain, please enable in the shader options if you want to paint wetness", MessageType.Warning);
            }
         }

         else if (tab == Tab.Puddles)
         {
            if (hasPuddles)
            {
               DrawPuddlesGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Puddles is not enabled on your terrain, please enable in the shader options if you want to paint puddles", MessageType.Warning);
            }
         }
         else if (tab == Tab.Streams)
         {
            if (hasStreams)
            {
               DrawStreamGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Streams are not enabled on your terrain, please enable in the shader options if you want to paint streams", MessageType.Warning);
            }
         }
         else if (tab == Tab.Lava)
         {
            if (hasLava)
            {
               DrawLavaGUI();
            }
            else
            {
               EditorGUILayout.HelpBox("Lava is not enabled on your terrain, please enable in the shader options if you want to paint lava", MessageType.Warning);
            }
         }

      }

      void DrawSaveGUI()
      {
         EditorGUILayout.Space();
         EditorGUILayout.BeginHorizontal();
         if (GUILayout.Button("Save"))
         {
            for (int i = 0; i < terrains.Length; ++i)
            {
               string path = AssetDatabase.GetAssetPath(terrains[i].terrainTex);
               var bytes = terrains[i].terrainTex.EncodeToPNG();
               System.IO.File.WriteAllBytes(path, bytes);
            }
            AssetDatabase.Refresh();

         }

         EditorGUILayout.EndHorizontal();
         EditorGUILayout.Space();
      }

      void DrawSettingsGUI()
      {
         EditorGUILayout.Separator();
         GUI.skin.box.normal.textColor = Color.white;
         if (MicroSplatUtilities.DrawRollup("MicroSplat FX Painter"))
         {
            bool oldEnabled = enabled;
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp)
            {
               enabled = !enabled;
            }
            enabled = GUILayout.Toggle(enabled, "Active (ESC)");
            if (enabled != oldEnabled)
            {
               InitTerrains();
            }

            brushVisualization = (BrushVisualization)EditorGUILayout.EnumPopup("Brush Visualization", brushVisualization);
            EditorGUILayout.Separator();
            GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
            EditorGUILayout.Separator();
         }
      }

      void DrawBrushSettingsGUI()
      {
         brushSize      = EditorGUILayout.Slider("Brush Size", brushSize, 0.01f, 30.0f);
         brushFlow      = EditorGUILayout.Slider("Brush Flow", brushFlow, 0.1f, 128.0f);
         brushFalloff   = EditorGUILayout.Slider("Brush Falloff", brushFalloff, 0.1f, 3.5f);
         EditorGUILayout.MinMaxSlider(CSlopeRange, ref slopeRange.x, ref slopeRange.y, 0.0f, 1.0f);
         paintValue     = EditorGUILayout.Slider("Target Value", paintValue, 0.0f, 1.0f);
         EditorGUILayout.Separator();
         GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
         EditorGUILayout.Separator();

      }



      void FillTerrain(MicroSplatTerrainJob t, int channel, float val)
      {
         InitTerrains();
         t.RegisterUndo();
         Texture2D tex = t.terrainTex;
         int width = tex.width;
         int height = tex.height;
         for (int x = 0; x < width; ++x)
         {
            for (int y = 0; y < height; ++y)
            {
               var c = tex.GetPixel(x, y);

               Vector3 normal = t.terrain.terrainData.GetInterpolatedNormal((float)x / tex.width, (float)y / tex.height);
               float dt = Vector3.Dot(normal, Vector3.up);
               dt = 1 - Mathf.Clamp01(dt);
               bool filtered = dt < slopeRange.x || dt > slopeRange.y;
           
               if (!filtered)
               {
                  c[channel] = val;

                  tex.SetPixel(x, y, c);
               }
            }
         }
         tex.Apply();
      }

   }
   #endif
}
