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
    /// <summary>
    /// mono 的类型
    /// </summary>
    public enum MonoType
    {
        None=0,
        Unit=1,
        Global=2,
        View=4,
        Normal=8,
    }

    /// <summary>
    /// Mono生命周期:
    /// Awake
    /// OnEnable
    /// Init
    /// Birth,Rebirth
    /// Start
    /// OnUpdate
    /// OnFixedUpdate
    /// OnLateUpdate
    /// Death
    /// RealDeath
    /// OnDisable
    /// OnDestroy
    /// </summary>
    public class BaseCoreMono : BaseMono, ICYMBase, IMono, IDBDataConvert, IUnit,IOnAnimTrigger
    {
        #region callback Val
        /// <summary>
        /// OnInit:会在Disable的时候自动注销
        /// </summary>
        public event Callback Callback_OnInit;
        /// <summary>
        /// OnBirth:会在Disable的时候自动注销
        /// </summary>
        public event Callback Callback_OnBirth;
        /// <summary>
        /// OnRebirth:会在Disable的时候自动注销
        /// </summary>
        public event Callback Callback_OnRebirth;
        /// <summary>
        /// OnDeath:会在Disable的时候自动注销
        /// </summary>
        public event Callback<BaseUnit> Callback_OnDeath;
        /// <summary>
        /// OnRealDeath:会在Disable的时候自动注销
        /// </summary>
        public event Callback Callback_OnRealDeath;
        #endregion

        #region member variable
        protected List<IOnAnimTrigger> triggersComponets = new List<IOnAnimTrigger>();
        protected List<BaseCoreMgr> componets = new List<BaseCoreMgr>();
        protected List<BaseCoreMgr> updateComponets = new List<BaseCoreMgr>();
        protected List<BaseCoreMgr> fixedUpdateComponets = new List<BaseCoreMgr>();
        protected List<BaseCoreMgr> lateUpdateComponets = new List<BaseCoreMgr>();
        protected List<BaseCoreMgr> guiComponets = new List<BaseCoreMgr>();
        protected List<BaseCoreMgr> needGameLogicTurnComponets = new List<BaseCoreMgr>();
        public BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        public bool NeedUpdate { get; protected set; }
        public bool NeedGUI { get; protected set; }
        public bool NeedFixedUpdate { get; protected set; }
        public bool NeedLateUpdate { get; protected set; }
        public bool NeedGameLogicTurn { get; protected set; }
        public bool IsEnable { get; set; }
        public MonoType MonoType { get; protected set; }=MonoType.Normal;
        #endregion

        #region prop
        public int ID{get;set;}
        public string TDID { get; set; }
        public virtual LayerData LayerData => BaseConstMgr.Layer_Default;
        #endregion

        #region unit life        
        public virtual void Init()
        {
            foreach(var item in componets)
            {
                if(item.IsEnable)
                    item.Init();
            }
            Callback_OnInit?.Invoke();
            Birth();
            Birth2();
            Birth3();
        }

        public virtual void Birth()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.Birth();
            }
            Callback_OnBirth?.Invoke();
        }

        public virtual void Birth2()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.Birth2();
            }
        }

        public virtual void Birth3()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.Birth3();
            }
        }

        public virtual void ReBirth()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.ReBirth();
            }
            Birth();
            Callback_OnRebirth?.Invoke();
        }

        public virtual void Death(BaseUnit caster)
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.Death(caster);
            }
            Callback_OnDeath?.Invoke(caster);
        }

        public virtual void RealDeath()
        {
            foreach (var item in componets)
            {
                if (item.IsEnable)
                    item.RealDeath();
            }
            Callback_OnRealDeath?.Invoke();
        }
        #endregion

        #region methon
        public override void OnEnable()
        {
            //如果LayerData和GameObject的Layer不相等,才会被设置
            if ((int)LayerData != GO.layer)
                SetLayer(LayerData,false);
            foreach (var value in componets)
            {
                value.OnEnable();
            }
            BaseGlobalMonoMgr.ActiveMono(this);
        }
        public virtual void OnSetNeedFlag()
        {
        }
        public override void Awake()
        {
            //如果LayerData没有被重载,则不会被设置
            if (LayerData != BaseConstMgr.Layer_Default)
                SetLayer(LayerData, true);
            OnSetNeedFlag();
            BaseGlobalMonoMgr.AddMono(this);
            AttachComponet();
        }

        public override void Start()
        {
            foreach (var value in componets)
            {
                value.OnStart();
            }
            OnAffterStart();
        }
        /// <summary>
        /// 增加组建
        /// </summary>
        protected virtual void AttachComponet()
        {

        }

        public virtual void OnAffterStart()
        {
            foreach (var value in componets)
            {
                value.OnAffterStart();
            }
        }
        public virtual void OnUpdate()
        {
            foreach (var value in updateComponets)
            {
                if (value.IsEnable)
                    value.OnUpdate();
            }
        }
        public virtual void ManualUpdate()
        {
        }
        public virtual void OnFixedUpdate()
        {
            foreach (var value in fixedUpdateComponets)
            {
                if (value.IsEnable)
                    value.OnFixedUpdate();
            }
        }

        public virtual void OnLateUpdate()
        {
            foreach (var value in lateUpdateComponets)
            {
                if (value.IsEnable)
                    value.OnLateUpdate();
            }
        }
        /// <summary>
        /// 手动调用
        /// </summary>
        public virtual void GameLogicTurn()
        {
            foreach (var value in needGameLogicTurnComponets)
            {
                if (value.IsEnable)
                    value.GameLogicTurn();
            }
        }
        /// <summary>
        /// 手动调用
        /// </summary>
        /// <param name="gameFramesPerSecond"></param>
        public virtual void GameFrameTurn(int gameFramesPerSecond)
        {
            foreach (var value in componets)
            {
                if (value.IsEnable)
                    value.GameFrameTurn(gameFramesPerSecond);
            }
        }
        /// <summary>
        /// 手动调用
        /// </summary>
        public virtual void OnAnimTrigger(int param)
        {
            foreach (var item in triggersComponets)
                item.OnAnimTrigger(param);
        }

        public virtual void OnGUIPaint()
        {
            foreach (var value in guiComponets)
            {
                if (value.IsEnable)//&& value.NeedGUI
                    value.OnGUIPaint();
            }
        }
        public override void OnDisable()
        {
            foreach (var value in componets)
            {
                value.OnDisable();
            }
            BaseGlobalMonoMgr.DeactiveMono(this);
            Callback_OnInit=null;
            Callback_OnBirth = null;
            Callback_OnRebirth = null;
            Callback_OnDeath = null;
            Callback_OnRealDeath = null;
            if (Rigidbody != null)
                Rigidbody.Sleep();
        }
        public override void OnDestroy()
        {
            foreach (var value in componets)
            {
                value.OnDestroy();
            }
            BaseGlobalMonoMgr.RemoveMono(this);
        }

        public virtual T AddComponent<T>() where T : BaseCoreMgr,new()
        {
            var component = BaseCoreMgr.Create<T>();
            componets.Add(component);
            if (component is IOnAnimTrigger)
                triggersComponets.Add(component as IOnAnimTrigger);
            if (component.NeedUpdate)
                updateComponets.Add(component);
            if (component.NeedLateUpdate)
                lateUpdateComponets.Add(component);
            if (component.NeedFixedUpdate)
                fixedUpdateComponets.Add(component);
            if (component.NeedGUI)
                guiComponets.Add(component);
            if (component.NeedGameLogicTurn)
                needGameLogicTurnComponets.Add(component);
            component.OnBeAdded(this);
            return (T)component;
        }

        public virtual void RemoveComponent(BaseCoreMgr component)
        {
            if (component != null)
            {
                component.OnBeRemoved();
                if (component is IOnAnimTrigger)
                    triggersComponets.Remove(component as IOnAnimTrigger);
                if (component.NeedUpdate)
                    updateComponets.Remove(component);
                if (component.NeedLateUpdate)
                    lateUpdateComponets.Remove(component);
                if (component.NeedFixedUpdate)
                    fixedUpdateComponets.Remove(component);
                if (component.NeedGUI)
                    guiComponets.Remove(component);
                componets.Remove(component);
            }
        }
        #endregion

        #region static methon
        //深层拷贝工具
        public static object GetDeepCopy(object obj)
        {

            System.Object DeepCopyObj = null;
            if (obj == null)
                return null;
            if (obj.GetType().IsValueType == true)//值类型
            {
                DeepCopyObj = obj;
            }
            else//引用类型
            {
                DeepCopyObj = System.Activator.CreateInstance(obj.GetType()); //创建引用对象
                System.Reflection.MemberInfo[] memberCollection = obj.GetType().GetMembers();

                foreach (System.Reflection.MemberInfo member in memberCollection)
                {
                    if (member.MemberType == System.Reflection.MemberTypes.Field)
                    {
                        System.Reflection.FieldInfo field = (System.Reflection.FieldInfo)member;
                        System.Object fieldValue = field.GetValue(obj);

                        if (fieldValue is ICloneable)
                        {
                            field.SetValue(DeepCopyObj, (fieldValue as ICloneable).Clone());
                        }
                        else
                        {
                            field.SetValue(DeepCopyObj, GetDeepCopy(fieldValue));
                        }
                    }

                }
            }

            return DeepCopyObj;

        }



        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            Transform[] trans = obj.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; ++i)
            {
                trans[i].gameObject.layer = layer;
            }
        }
        //设置tag
        public static void SetTagRecursively(GameObject obj, string name)
        {
            Transform[] trans = obj.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; ++i)
            {
                trans[i].gameObject.tag = name;
            }
        }


        #endregion

        #region DB
        public virtual void Read1<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in componets)
                item.Read1(data);
        }

        public virtual void Read2<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in componets)
                item.Read2(data);
        }

        public virtual void Read3<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in componets)
                item.Read3(data);
        }

        public virtual void ReadEnd<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in componets)
                item.ReadEnd(data);
        }

        public virtual void Write<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in componets)
                item.Write(data);
        }
        #endregion

        bool firstDrawGizmos = true;
        protected virtual void OnDrawGizmos()
        {
            if (firstDrawGizmos)
            {
                OnFirstDrawGizmos();
                firstDrawGizmos = false;
            }
        }
        protected virtual void OnFirstDrawGizmos()
        {

        }
    }
}
