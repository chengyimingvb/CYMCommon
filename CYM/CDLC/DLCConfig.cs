//------------------------------------------------------------------------------
// AssetBundleConfig.cs
// Copyright 2018 2018/5/18 
// Created by CYM on 2018/5/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;
using System.IO;
namespace CYM.DLC
{
    [CreateAssetMenu]
    public class DLCConfig : ScriptableObjectConfig<DLCConfig>
    {
        #region inspector
        [SerializeField]
        public bool IsSimulationDLCEditor = true;
        [SerializeField]
        List<BuildRuleData> Data = new List<BuildRuleData>();
        [SerializeField]
        List<string> CopyDirectory = new List<string>();
        [SerializeField]
        List<DLCItem> DLC = new List<DLCItem>();
        #endregion

        #region prop
        //默认DLC
        public DLCItem Native { get; private set; } = new DLCItem();
        //扩展DLC 不包含 Native
        public Dictionary<string, DLCItem> DLCItems { get; private set; } = new Dictionary<string, DLCItem>();
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            CreateNative();
        }
        public override void OnInited()
        {
            if (Application.isEditor)
            {
                //初始化默认DLC
                string internalRootPath = BaseConstMgr.Path_InternalBundle;
                Native = new DLCItem();
                Native.Name = "Native";
                Native.AddBuildData("Audio", BuildRuleType.Directroy, internalRootPath);
                Native.AddBuildData("Icon", BuildRuleType.Directroy, internalRootPath);
                Native.AddBuildData("Material", BuildRuleType.Directroy, internalRootPath);
                Native.AddBuildData("Prefab", BuildRuleType.Directroy, internalRootPath);
                Native.AddBuildData("System", BuildRuleType.Directroy, internalRootPath);
                Native.AddBuildData("UI",BuildRuleType.Directroy, internalRootPath);
                Native.LoadConfig(Data, CopyDirectory);

                //初始化扩展DLC
                DLCItems = new Dictionary<string, DLCItem>();
                foreach (var item in DLC)
                {
                    DLCItems.Add(item.Name, item);
                    item.LoadConfig(Data, CopyDirectory);
                }
            }
        }
        #endregion

        #region set
        //初始化默认配置,方便编辑
        private void CreateNative()
        {
            AddBuildData("Animator");
            AddBuildData("Audio");
            AddBuildData("AudioMixer");
            AddBuildData("BG");
            AddBuildData("Icon");
            AddBuildData("Material");
            AddBuildData("Music");
            AddBuildData("Perform");
            AddBuildData("PhysicsMaterial");
            AddBuildData("Prefab");
            AddBuildData("Scene", BuildRuleType.File);
            AddBuildData("UI");
            AddBuildData("Video");
            AddBuildData("System");
            AddBuildData("Texture");
            AddCopyDirectory("Language");
            AddCopyDirectory("Lua");
            AddCopyDirectory("Config");
        }

        public void AddBuildData(string name, BuildRuleType type = BuildRuleType.Directroy)
        {
            var data = new BuildRuleData();
            data.BuildRuleType = type;
            data.SearchPath = name;
            data.CustomRootPath = null;
            Data.Add(data);
        }
        public void AddCopyDirectory(string path)
        {
            CopyDirectory.Add(path);
        }
        #endregion

        #region is
        //是否为编辑器模式
        public bool IsEditorMode
        {
            get
            {
                if (!Application.isEditor)
                    return false;
                if (Application.isEditor && IsSimulationDLCEditor)
                    return true;
                return false;
            }
        }
        #endregion

    }
}