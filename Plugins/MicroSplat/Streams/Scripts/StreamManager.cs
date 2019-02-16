//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if __MICROSPLAT__
public class StreamManager : MonoBehaviour 
{
   MicroSplatTerrain msTerrain;
   RenderTexture buffer0;
   RenderTexture buffer1;
   [HideInInspector]
   public RenderTexture currentBuffer;
   bool onBuffer0 = true;
   Material updateMat;

   [HideInInspector]
   public Shader updateShader;

   Vector4[] spawnBuffer = new Vector4[64];
   Vector4[] colliderBuffer = new Vector4[64];

   Texture2D terrainDesc;


   // props
   public Vector2 evaporation = new Vector2(0.01f, 0.01f);
   public Vector2 strength = new Vector2(1.0f, 1.0f);
   public Vector2 speed = new Vector2(1, 1);
   public Vector2 resistance = new Vector2(0.1f, 0.1f);
   public float wetnessEvaporation = 0.01f;
   public float burnEvaporation = 0.01f;

   List<StreamEmitter> emitters = new List<StreamEmitter>(16);
   List<StreamCollider> colliders = new List<StreamCollider>(16);

   static Vector2 WorldToTerrain(Terrain ter, Vector3 point, Texture splatControl)
   {
      point = ter.transform.worldToLocalMatrix.MultiplyPoint(point);
      float x = (point.x / ter.terrainData.size.x) * splatControl.width;
      float z = (point.z / ter.terrainData.size.z) * splatControl.height;
      return new Vector2(x,z);
   }

   public void Register(StreamEmitter e)
   {
      emitters.Add(e);
   }

   public void Unregister(StreamEmitter e)
   {
      emitters.Remove(e);
   }

   public void Register(StreamCollider e)
   {
      colliders.Add(e);
   }

   public void Unregister(StreamCollider e)
   {
      colliders.Remove(e);
   }

   void Awake()
   {
      msTerrain = GetComponent<MicroSplatTerrain>();
   }

	void OnEnable () 
   {
      terrainDesc = msTerrain.terrainDesc;
      int w = terrainDesc.width;
      int h = terrainDesc.height;
      buffer0 = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
      buffer1 = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

      Graphics.Blit(Texture2D.blackTexture, buffer0);
      Graphics.Blit(Texture2D.blackTexture, buffer1);

      updateMat = new Material(updateShader);
	}
	
   void OnDisable()
   {

      buffer0.Release();
      buffer1.Release();
      DestroyImmediate(buffer0);
      DestroyImmediate(buffer1);
      onBuffer0 = false;
      DestroyImmediate(updateMat);
      buffer0 = null;
      buffer1 = null;
      updateMat = null;
   }


   double timeSinceWetnessEvap = 0;
   double timeSinceBurnEvap = 0;
   double timeSinceEvapX = 0;
   double timeSinceEvapY = 0;

   Vector2 evapAmount = new Vector2(0, 0);

	void Update () 
   {
      int emitterCount = emitters.Count; 
      if (emitterCount > 64)
      {
         emitterCount = 64;
      }

      int colliderCount = colliders.Count;
      if (colliderCount > 64)
      {
         colliderCount = 64;
      }

      int usedEmitters = 0;
      for (int i = 0; i < emitters.Count; ++i)
      {
         var e = emitters[i];
         Vector2 ter = WorldToTerrain(msTerrain.terrain, e.transform.position, buffer0);
         if (ter.x >= 0 && ter.x < buffer0.width && ter.y >= 0 && ter.y < buffer0.width)
         {
            Vector3 pos = e.transform.position + Vector3.left * e.transform.lossyScale.x;
            Vector2 endPoint = WorldToTerrain(msTerrain.terrain, pos, buffer0);
            float d = Vector2.Distance(ter, endPoint);
            if (d < 1)
               d = 1;
            d *= e.strength;

            Vector4 data = new Vector4(ter.x, ter.y, 0, 0);
            if (e.emitterType == StreamEmitter.EmitterType.Water)
            {
               data.z = d;
            }
            else
            {
               data.w = d;
            }
            spawnBuffer[usedEmitters] = data;
            usedEmitters++;
         }
      }

      int usedColliders = 0;
      for (int i = 0; i < colliders.Count; ++i)
      {
         var c = colliders[i];
         Vector2 ter = WorldToTerrain(msTerrain.terrain, c.transform.position, buffer0);

         if (ter.x >= 0 && ter.x < buffer0.width && ter.y >= 0 && ter.y < buffer0.width)
         {
            Vector3 pos = c.transform.position + Vector3.left * c.transform.lossyScale.x;
            Vector2 endPoint = WorldToTerrain(msTerrain.terrain, pos, buffer0);
            float d = Vector2.Distance(ter, endPoint);

            Vector4 data = new Vector4(ter.x, ter.y, 0, 0);
            if (c.colliderType != StreamCollider.ColliderType.Lava)
            {
               data.z = d;
            }
            if (c.colliderType != StreamCollider.ColliderType.Water)
            {
               data.w = d;
            }


            colliderBuffer[usedColliders] = data;
            usedColliders++;
         }
      }
         
      updateMat.SetVectorArray("_Positions", spawnBuffer);
      updateMat.SetVectorArray("_Colliders", colliderBuffer);
      updateMat.SetInt("_PositionsCount", usedEmitters);
      updateMat.SetInt("_CollidersCount", usedColliders);
      updateMat.SetVector("_SpawnStrength", strength);

      updateMat.SetTexture("_TerrainDesc", terrainDesc);
      updateMat.SetFloat("_DeltaTime", Time.smoothDeltaTime);
      updateMat.SetVector("_Speed", speed);
      updateMat.SetVector("_Resistance", resistance);


      if (onBuffer0)
      {
         if (evaporation.x > 0)
         {
            float evapDelay = (1.0f / evaporation.x) / 255.0f;
            if (timeSinceEvapX > evapDelay)
            {
               timeSinceEvapX = 0;
               evapAmount.x = 0.004f;
            }
            else
            {
               evapAmount.x = 0;
            }
         }
         if (evaporation.y > 0)
         {
            float evapDelay = (1.0f / evaporation.y) / 255.0f;
            if (timeSinceEvapY > evapDelay)
            {
               timeSinceEvapY = 0;
               evapAmount.y = 0.004f;
            }
            else
            {
               evapAmount.y = 0;
            }
         }
         updateMat.SetVector("_Evaporation", evapAmount);

         if (wetnessEvaporation > 0)
         {
            float wetnessDelay = (1.0f / wetnessEvaporation) / 255.0f;
            if (timeSinceWetnessEvap > wetnessDelay)
            {
               updateMat.SetFloat("_WetnessEvaporation", 0.004f);
               timeSinceWetnessEvap = 0;
            }
            else
            {
               updateMat.SetFloat("_WetnessEvaporation", 0);
            }
         }

         if (burnEvaporation > 0)
         {
            float burnDelay = (1.0f * burnEvaporation) / 255.0f;
            if (timeSinceBurnEvap > burnDelay)
            {
               updateMat.SetFloat("_BurnEvaporation", 0.004f);
               timeSinceBurnEvap = 0;
            }
            else
            {
               updateMat.SetFloat("_BurnEvaporation", 0);
            }
         }

         Graphics.Blit(buffer0, buffer1, updateMat);
         currentBuffer = buffer1;
      }
      else
      {
         // only spawn, evaporate on first pass
         updateMat.SetInt("_PositionsCount", 0);
         updateMat.SetVector("_Evaporation", Vector2.zero);
         updateMat.SetFloat("_WetnessEvaporation", 0);
         updateMat.SetFloat("_BurnEvaporation", 0);
         Graphics.Blit(buffer1, buffer0, updateMat);
         currentBuffer = buffer0;
      }
      onBuffer0 = !onBuffer0;

      float dt = Time.deltaTime;
      timeSinceEvapX += dt;
      timeSinceEvapY += dt;
      timeSinceWetnessEvap += dt;
      timeSinceBurnEvap += dt;

      msTerrain.matInstance.SetTexture("_DynamicStreamControl", currentBuffer);

	}
}

#endif