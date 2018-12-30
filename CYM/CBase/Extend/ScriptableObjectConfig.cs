//------------------------------------------------------------------------------
// BaseScriptableObjectConfig.cs
// Copyright 2018 2018/3/28 
// Created by CYM on 2018/3/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace CYM
{
    public interface IScriptableObjectConfig
    {
        void OnCreate();
        void OnCreated();
        void OnInited();
    }
    public class ScriptableObjectConfig<T> : ScriptableObject, ISerializationCallbackReceiver, IScriptableObjectConfig where T: ScriptableObject, IScriptableObjectConfig
    {
        public virtual void OnAfterDeserialize()
        {

        }

        public virtual void OnBeforeSerialize()
        {

        }

        static T _ins;
        public static T Ins
        {
            get
            {
                if (_ins == null)
                {

                    string fileName = typeof(T).Name;
                    _ins = Resources.Load<T>(BaseConstMgr.Dir_Config+"/"+fileName);
                    if (_ins == null)
                    {
                        _ins = CreateInstance<T>();
                        _ins.OnCreate();
#if UNITY_EDITOR
                        AssetDatabase.CreateAsset(_ins,string.Format(BaseConstMgr.Format_ConfigAssetPath, fileName));
#endif
                        _ins.OnCreated();
                    }
                    _ins.OnInited();
                }
                return _ins;
            }
        }
        public virtual void OnCreate()
        {

        }
        public virtual void OnCreated()
        {

        }
        public virtual void OnInited()
        {

        }
    }
}