//------------------------------------------------------------------------------
// UIConfig.cs
// Copyright 2018 2018/11/29 
// Created by CYM on 2018/11/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using CYM.Utile;

namespace CYM
{

    [Serializable]
    public class LogoData
    {
        public float WaitTime = 1.0f;
        public float InTime = 0.5f;
        public float OutTime = 0.5f;
        public Sprite Logo;
    }
    [Serializable]
    public class PresenterStateColor
    {
        public Color Normal = Color.white;
        public Color Enter = Color.grey;
        public Color Press = Color.white * 0.8f;
        public Color Disable = Color.grey;
        public Color Selected = Color.white;
    }
    [Serializable]
    public class UIFonts: SerializableDictionaryBase<LanguageType, Font>
    {

    }
    [Serializable]
    public class UIPresenterStateColors : SerializableDictionaryBase<string, PresenterStateColor>
    {

    }

    [CreateAssetMenu]
    public class UIConfig : ScriptableObjectConfig<UIConfig>
    {
        #region prop
        public bool IsEditorMode()
        {
            if (Application.isEditor)
                return IsShowLogo;
            return true;
        }
        public Dictionary<string, MethodInfo> DynStrFuncs = new Dictionary<string, MethodInfo>();
        #endregion

        #region inspector
        [SerializeField]
        public bool EnableSharpText = true;
        [SerializeField]
        public bool IsShowLogo;
        [SerializeField]
        public List<LogoData> Logos = new List<LogoData>();
        [SerializeField]
        public List<SpriteGroupConfig> SpriteGroupConfigs;
        [SerializeField]
        public UIPresenterStateColors PresenterStateColors = new UIPresenterStateColors();
        #endregion

        #region Fonts
        [FoldoutGroup("Fonts"), SerializeField]
        public Font DefaultFont;
        [FoldoutGroup("Fonts"), SerializeField]
        public UIFonts Fonts = new UIFonts();
        #endregion

        #region dyn
        [FoldoutGroup("Dyn"),ReadOnly,SerializeField]
        private List<string> DynStrName = new List<string>();
        [FoldoutGroup("Dyn"),ReadOnly,SerializeField]
        private List<MethodInfo> DynStrMethodInfo = new List<MethodInfo>();
        [FoldoutGroup("Dyn"),ReadOnly,SerializeField]
        public string MonoTypeName = "";
#if UNITY_EDITOR
        [FoldoutGroup("Dyn"), SerializeField]
        public MonoScript DynamicFuncScript;
#endif
        #endregion

        #region get
        public PresenterStateColor GetStateColor(string key)
        {
            if (PresenterStateColors.ContainsKey(key))
                return PresenterStateColors[key];
            CLog.Error("没有这个StateColor:{0}",key);
            return new PresenterStateColor();
        }
        #endregion

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (DynamicFuncScript != null)
            {
                MonoTypeName = DynamicFuncScript.GetClass().FullName;
            }
#endif
            DynStrName.Clear();
            DynStrMethodInfo.Clear();
            var type = Type.GetType(MonoTypeName);
            if (type == null)
                return;
            var array = type.GetMethods();
            foreach (var item in array)
            {
                var attrArray = item.GetCustomAttributes(true);
                foreach (var attr in attrArray)
                {
                    if (attr is DynStr)
                    {
                        DynStrName.Add(item.Name);
                        DynStrMethodInfo.Add(item);
                    }
                }
            }
            DynStrFuncs.Clear();
            for (int i = 0; i < DynStrName.Count; ++i)
            {
                DynStrFuncs.Add(DynStrName[i], DynStrMethodInfo[i]);
            }
        }
    }
}