//////////////////////////////////////////////////////
// MegaSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
   #if __MICROSPLAT__
   public class MicroSplatTerrainJob : ScriptableObject
   {
      public Terrain terrain;
      public Texture2D terrainTex;
      public Collider collider;

      public byte[] undoBuffer;

      public void RegisterUndo()
      {
         if (terrainTex != null)
         {
            undoBuffer = terrainTex.GetRawTextureData();
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Terrain Edit");
         }
      }

      public void RestoreUndo()
      {
         if (undoBuffer != null && undoBuffer.Length > 0)
         {
            terrainTex.LoadRawTextureData(undoBuffer);
            terrainTex.Apply();
         }
      }
   }
   #endif
}
