using System;
using System.Collections.Generic;
using System.Reflection;
using CYM;
using UnityEngine;
//using WhatA2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine.Rendering;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BaseMono : MonoBehaviour
    {
        private Transform _cacheTransform;
        public Transform Trans { get { return _cacheTransform ?? (_cacheTransform = transform); } }
        private GameObject _cacheGameObject;
        public GameObject GO { get { return _cacheGameObject ?? (_cacheGameObject = gameObject); } }
        public string Tag { get { return GO.tag; } set { GO.tag = value; } }
        public int Layer { get { return GO.layer; } set { GO.layer = value; } }
        private Rigidbody _cacheRigidbody;
        public Rigidbody Rigidbody { get { return _cacheRigidbody ?? (_cacheRigidbody = GO.GetComponent<Rigidbody>()); } }
        private Collider _cacheCollider;
        public Collider Collider { get { return _cacheCollider ?? (_cacheCollider = GO.GetComponent<Collider>()); } }
        private Collider[] _cacheColliders;
        public Collider[] Colliders { get { return _cacheColliders ?? (_cacheColliders = GO.GetComponentsInChildren<Collider>()); } }
        private Renderer[] _cacheRenderers;
        public Renderer[] Renderers { get { return _cacheRenderers ?? (_cacheRenderers = GO.GetComponentsInChildren<Renderer>()); } }
        private Transform[] _cacheTransforms;
        public Transform[] Transforms { get { return _cacheTransforms ?? (_cacheTransforms = GO.GetComponentsInChildren<Transform>()); } }

        #region
        public Vector3 Forward
        {
            get
            {
                if (Trans == null)
                    return Vector3.zero;
                return Trans.forward;
            }
        }
        public Vector3 Pos
        {
            get
            {
                if (Trans == null)
                    return Vector3.zero;
                return Trans.position;
            }
            set
            {
                if (Trans != null)
                    Trans.position = value;
            }
        }
        public Vector3 LocalPos
        {
            get
            {
                if (Trans == null)
                    return Vector3.zero;
                return Trans.localPosition;
            }
            set
            {
                if (Trans == null)
                    return;
                Trans.localPosition = value;
            }
        }
        public Quaternion Rot
        {
            get
            {
                if (Trans == null)
                    return Quaternion.identity;
                return Trans.rotation;
            }
            set
            {
                if (Trans == null)
                    return;
                Trans.rotation = value;
            }
        }
        public Quaternion LocalRot
        {
            get
            {
                if (Trans == null)
                    return Quaternion.identity;
                return Trans.localRotation;
            }
            set
            {
                if (Trans == null)
                    return;
                Trans.localRotation = value;
            }
        }
        public Vector3 LocalScale
        {
            get
            {
                if (Trans == null)
                    return Vector3.one;
                return Trans.localScale;
            }
            set
            {
                if (Trans == null)
                    return;
                Trans.localScale = value;
            }
        }
        #endregion

        #region methon
        public virtual void OnBeSetup()
        {

        }
        public virtual void OnEnable()
        {
        }

        public virtual void Awake()
        {
        }
        public virtual void Start()
        {
        }
        public virtual void OnDisable()
        {

        }
        public virtual void OnDestroy()
        {

        }

        public virtual T SetupMonoComponet<T>() where T : BaseMono
        {
            var temp = GO.GetComponentInChildren<T>();
            if(temp==null)
            {
                temp=EnsureComponet<T>();
            }
            temp.OnBeSetup();
            return temp;
        }
        public virtual T SetupUnityComponet<T>() where T : MonoBehaviour
        {
            var temp = GO.GetComponentInChildren<T>();
            if (temp == null)
            {
                temp = EnsureComponet<T>();
            }
            return temp;
        }
        /// <summary>
        /// 获得unity 组建 宝行子对象,返回List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetComponets<T>(bool includeInactive=true) where T : Component
        {
            List<T> ret = new List<T>();
            T[] component = GetComponentsInChildren<T>(includeInactive);
            if (component == null)
                return ret;
            ret.AddRange(component);
            return ret;
        }
        /// <summary>
        /// 获得组建,包含子对象,没有的话则自动添加一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T EnsureComponet<T>() where T : Component
        {
            T component = GetComponentInChildren<T>(true);
            if (component == null)
                component = gameObject.AddComponent<T>();
            return component;
        }
        /// <summary>
        /// 获得组建,包含子对象,没有的话则自动添加一个
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetUnityComponet<T>(GameObject GO) where T : Component
        {
            if (GO == null) return null;
            T component = GO.GetComponentInChildren<T>(true);
            if (component == null)
                component = GO.AddComponent<T>();
            return component;
        }
        /// <summary>
        /// 获得组建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mono"></param>
        /// <returns></returns>
        public static T GetUnityComponet<T>(BaseMono mono) where T : Component
        {
            return GetUnityComponet<T>(mono.GO);
        }

        public bool CompareTags(string tag)
        {
            return GO.CompareTag(tag);
        }

        public bool CompareLayer(int layer)
        {
            return GO.layer.Equals(layer);
        }
        public virtual void SetCollidersActive(bool enable)
        {
            if (Colliders == null)
                return;
            for (int i = 0; i < Colliders.Length; ++i)
                Colliders[i].enabled = enable;
        }
        public void SetCollidersTrigger(bool enable)
        {
            if (Colliders == null)
                return;
            for (int i = 0; i < Colliders.Length; ++i)
                Colliders[i].isTrigger = enable;
        }
        public GameObject FindSubGO(string name)
        {
            for (int i = 0; i < Trans.childCount; ++i)
            {
                if (Trans.GetChild(i).name == name)
                    return Trans.GetChild(i).gameObject;
            }
            return null;
        }
        public virtual void SetShadowMode(ShadowCastingMode mode)
        {
            if (Renderers != null)
            {
                foreach (var item in Renderers)
                {
                    item.shadowCastingMode = mode;
                }
            }
        }
        private void SetLayer(int layer,bool allChild=false)
        {
            if (Transforms == null)
                return;
            Trans.gameObject.layer = layer;
            if (allChild)
            {
                for (int i = 0; i < Transforms.Length; ++i)
                    Transforms[i].gameObject.layer = layer;
            }
        }

        public void SetLayer(LayerData layer, bool allChild = false)
        {
            SetLayer((int)layer,allChild);
        }

        /// <summary>
        /// 名称
        /// </summary>
        public virtual string Name => name;
        #endregion



    }

}