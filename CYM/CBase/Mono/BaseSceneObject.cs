using System;
using System.Collections.Generic;
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
        bool ReParse=true;
        [SerializeField]
        protected Transform RootPoints;
        #endregion

        #region prop
        public static BaseSceneObject Ins;
        #endregion

        #region prop
        public List<Transform> Points { get; private set; } = new List<Transform>();
        protected Dictionary<string, Transform> PointsDic { get; private set; } = new Dictionary<string, Transform>();
        #endregion

        public override void Awake()
        {
            Ins = (BaseSceneObject)this;
            base.Awake();
            Parse();
        }

        void Update()
        {
            if(ReParse)
            {
                Parse();
                ReParse = false;
            }
        }

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

        #region get
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
        #endregion

    }

}