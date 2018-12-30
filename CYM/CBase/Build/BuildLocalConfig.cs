//------------------------------------------------------------------------------
// BuildLocalConfig.cs
// Copyright 2018 2018/5/3 
// Created by CYM on 2018/5/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    [CreateAssetMenu]
    public class BuildLocalConfig : ScriptableObjectConfig<BuildLocalConfig>
    {
        #region editor setting
        public bool IsScrollBuildWindow = false;
        public bool IsAutoRefresh = true;
        public bool IsEnableDevSetting = false;

        public bool Fold_Present_Main = false;
        public bool Fold_Present_Setting = false;
        public bool Fold_Present_DLC = false;
        public bool Fold_Present_Explorer = false;
        public bool Fold_Present_SceneList = false;
        public bool Fold_Present_ScriptTemplate = false;
        public bool Fold_Present_ExpressSetup = false;
        public bool Fold_Present_Other = false;
        public bool Fold_Present_SubWindow = false;
        public bool Fold_Present_Mod = false;
        #endregion
    }
}