using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    [ExecuteInEditMode]
    public class BaseSceneObject : BaseMono
    {
        #region inspector
        [SerializeField]
        protected Transform RootPoints;
        #endregion

        #region prop
        public static BaseSceneObject Ins;
        public static Terrain ActiveTerrain => Terrain.activeTerrain;
        public List<Transform> Points { get; private set; } = new List<Transform>();
        protected Dictionary<string, Transform> PointsDic { get; private set; } = new Dictionary<string, Transform>();
        #endregion

        #region life
        public override void Awake()
        {
            Ins = (BaseSceneObject)this;
            base.Awake();
            Parse();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Parse();
        }
        #endregion

        #region set
        [Button("Parse")]
        protected virtual void Parse()
        {
            if (RootPoints != null)
            {
                Points.Clear();
                PointsDic.Clear();
                Points.AddRange( RootPoints.GetComponentsInChildren<Transform>());
                if(Points.Count>0)
                    Points.RemoveAt(0);
                foreach (var item in Points)
                {
                    if (PointsDic.ContainsKey(item.name))
                        continue;
                    PointsDic.Add(item.name, item);
                }
            }
        }
        #endregion

        #region get
        public Vector3 GetInterpolatedNormal(int x,int z)
        {
            if (ActiveTerrain == null)
                throw new Exception("ActiveTerrain == null");
            return ActiveTerrain.terrainData.GetInterpolatedNormal(x,z);
        }
        /// <summary>
        /// 获得出身点
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Transform GetPoint(int index=0)
        {
            if (Points.Count <= 0)
                return null;
            if (Points.Count <= index)
                return Points[0];
            return Points[index];
        }
        /// <summary>
        /// 获得位置点
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Transform GetPoint(string name)
        {
            if (!PointsDic.ContainsKey(name))
                return null;
            return PointsDic[name];
        }
        /// <summary>
        /// 获得高度
        /// </summary>
        /// <returns></returns>
        public float GetHeight(float x,float z)
        {
            if (ActiveTerrain == null)
                throw new Exception("ActiveTerrain == null");
            return ActiveTerrain.terrainData.GetHeight((int)x, (int)z) + ActiveTerrain.transform.position.y;
        }
        #endregion

    }

}