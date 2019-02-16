//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MicroSplatBlendableObject : MonoBehaviour 
{
   #if __MICROSPLAT__
   [HideInInspector]
   public MicroSplatObject msObject;

   public float blendDistance = 1;
   public float normalBlendDistance = 1;
   [Range(0.0001f, 1.0f)]
   public float blendContrast = 0.0f;
   [Range(0.25f, 4.0f)]
   public float blendCurve = 1.0f;

   [Range(0.0f, 1.0f)]
   public float slopeFilter = 1;

   [Range(1.0f, 40.0f)]
   public float slopeContrast = 20;

   [Range(0.0f, 1.0f)]
   public float slopeNoise = 0.35f;

   static MaterialPropertyBlock props;

   [Range(0.0f, 1.0f)]
   public float snowDampening = 0;

   [Range(0.0f, 1.0f)]
   public float snowWidth = 0;

   public bool doSnow = true;
   public bool doTerrainBlend = true;

   public Texture2D normalFromObject;

   void OnEnable()
   {
      #if UNITY_EDITOR
      MicroSplatTerrain.OnMaterialSyncAll += Sync;
      #endif
      Sync();
   }

   void Start()
   {
      Sync();
   }

   #if UNITY_EDITOR
   void OnDisable()
   {
      MicroSplatTerrain.OnMaterialSyncAll -= Sync;
   }
   #endif

   public Bounds TransformBounds( Bounds localBounds )
   {
      var center = transform.TransformPoint(localBounds.center);

      var extents = localBounds.extents;
      var axisX = transform.TransformVector(extents.x, 0, 0);
      var axisY = transform.TransformVector(0, extents.y, 0);
      var axisZ = transform.TransformVector(0, 0, extents.z);

      // sum their absolute value to get the world extents
      extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
      extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
      extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

      return new Bounds { center = center, extents = extents };
   }

   public void Sync()
   {
      #if UNITY_EDITOR
      // If we have a terrain, we might be moved over another terrain. So lets see if we're in the bounds of our current terrain
      // and if not, clear it and try to get a new one..
      bool testForNewTerrain = false;
      if (msObject != null)
      {
         var tbounds = TransformBounds(msObject.GetBounds());

         Renderer rend = GetComponent<Renderer>();
         if (!tbounds.Intersects(rend.bounds))
         {
            testForNewTerrain = true;
         }

      }
      if (msObject == null || testForNewTerrain)
      {
         RaycastHit[] hits = Physics.RaycastAll(this.transform.position + Vector3.up * 100, Vector3.down, 500);
         for (int i = 0; i < hits.Length; ++i)
         {
            var h = hits[i];
            var t = h.collider.GetComponent<Terrain>();
            if (t != null)
            {
               var nt = t.GetComponent<MicroSplatTerrain>();
               if (nt != null)
               {
                  msObject = nt;
                  break;
               }
            }
#if __MICROSPLAT_MESH__
            var m = h.collider.GetComponent<MicroSplatMesh>();
            if (m != null)
            {
               msObject = m;
               break;
            }
#endif
         }

      }
      #endif

      Material bmInstance = msObject.GetBlendMatInstance();
      if (msObject == null && bmInstance)
         return;
      
      Renderer r = GetComponent<Renderer>();
      var materials = r.sharedMaterials;
      bool hasBlendMat = false;
      for (int i = 0; i < materials.Length; ++i)
      {
         if (materials[i] == bmInstance && bmInstance != null)
         {
            hasBlendMat = true;
         }
         else if (materials[i] == null || materials[i].IsKeywordEnabled("_TERRAINBLENDING"))
         {
            hasBlendMat = true;
            materials[i] = bmInstance;
            r.sharedMaterials = materials;
         }
      }
      if (!hasBlendMat)
      {
         System.Array.Resize<Material>(ref materials, materials.Length + 1);
         materials[materials.Length - 1] = bmInstance;
         r.sharedMaterials = materials;
      }

      if (props == null)
      {
         props = new MaterialPropertyBlock();
      }


      props.Clear();


      props.SetVector("_TerrainBlendParams", new Vector4(blendDistance, blendContrast, msObject.transform.position.y, blendCurve));
      props.SetVector("_SlopeBlendParams", new Vector4(slopeFilter, slopeContrast, slopeNoise, normalBlendDistance));
      props.SetVector("_SnowBlendParams", new Vector4(snowWidth, 0, 0, 0));
      props.SetVector("_FeatureFilters", new Vector4(doTerrainBlend ? 0.0f : 1.0f, doSnow ? 0.0f : 1.0f, 0, 0));

      if (normalFromObject != null)
      {
         props.SetTexture("_NormalOriginal", normalFromObject);
      }

      r.SetPropertyBlock(props);
   }
   #endif
}
