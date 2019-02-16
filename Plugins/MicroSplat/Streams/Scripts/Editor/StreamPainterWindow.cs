//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__
   public partial class StreamPainterWindow : EditorWindow 
   {
      [MenuItem("Window/MicroSplat/FX Painter")]
      public static void ShowWindow()
      {
         var window = GetWindow<JBooth.MicroSplat.StreamPainterWindow>();
         window.InitTerrains();
         window.Show();
      }

      bool enabled = true;


      MicroSplatTerrainJob[] terrains;
      bool[] jobEdits;

      MicroSplatTerrainJob FindJob(Terrain t)
      {
         if (terrains == null || t == null)
            return null;

         for (int i = 0; i < terrains.Length; ++i)
         {
            if (terrains[i] != null && terrains[i].terrain == t)
               return terrains[i];
         }
         return null;
      }

      List<Terrain> rawTerrains = new List<Terrain>();

      void InitTerrains()
      {
         Object[] objs = Selection.GetFiltered(typeof(Terrain), SelectionMode.Editable | SelectionMode.OnlyUserModifiable | SelectionMode.Deep);
         List<MicroSplatTerrainJob> ts = new List<MicroSplatTerrainJob>();
         rawTerrains.Clear();
         for (int i = 0; i < objs.Length; ++i)
         {
            Terrain t = objs[i] as Terrain;
            MicroSplatTerrain mst = t.GetComponent<MicroSplatTerrain>();
            if (mst == null)
               continue;
            rawTerrains.Add(t);
            if (t.materialType == Terrain.MaterialType.Custom && t.materialTemplate != null)
            {
               if (!t.materialTemplate.HasProperty("_StreamControl"))
                  continue;
               if (mst.streamTexture == null)
               {
                  CreateTexture(t);
               }

               var tj = FindJob(t);
               if (tj != null)
               {
                  tj.collider = t.GetComponent<Collider>();
                  tj.terrainTex = mst.streamTexture;
                  ts.Add(tj);
               }
               else
               {
                  tj = MicroSplatTerrainJob.CreateInstance<MicroSplatTerrainJob>();
                  tj.terrain = t;
                  tj.collider = t.GetComponent<Collider>();
                  tj.terrainTex = mst.streamTexture;
                  ts.Add(tj);
               }
            }
         }
         if (terrains != null)
         {
            // clear out old terrains
            for (int i = 0; i < terrains.Length; ++i)
            {
               if (!ts.Contains(terrains[i]))
               {
                  DestroyImmediate(terrains[i]);
               }
            }
         }

         terrains = ts.ToArray();
         jobEdits = new bool[ts.Count];
      }

      void OnSelectionChange()
      {
         InitTerrains();
         this.Repaint();
      }

      void OnFocus() 
      {
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         SceneView.onSceneGUIDelegate += this.OnSceneGUI;

         Undo.undoRedoPerformed -= this.OnUndo;
         Undo.undoRedoPerformed += this.OnUndo;

         this.titleContent = new GUIContent("MicroSplat FX Painter");
         InitTerrains();
         Repaint();
      }

      void OnUndo()
      {
         if (terrains == null)
            return;
         for (int i = 0; i < terrains.Length; ++i)
         {
            if (terrains[i] != null)
            {
               terrains[i].RestoreUndo();
            }
         }
         Repaint();
      }

      void OnInspectorUpdate()
      {
         // unfortunate...
         Repaint ();
      }

      void OnDestroy() 
      {
         SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
         terrains = null;
      }


   }
   #endif
}

