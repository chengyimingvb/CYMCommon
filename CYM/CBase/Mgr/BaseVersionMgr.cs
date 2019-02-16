using UnityEngine;
using System.Collections;
using CYM;
using System;
using System.Collections.Generic;

//**********************************************
// Class Name	: GlobalComponet
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BaseVersionMgr : BaseGFlowMgr
    {


        public string GameVersion { get { return Config.ToString(); } }
        public string FullGameVersion { get { return Config.FullVersionName; } }
        public BuildConfig Config { get; private set; }

        public bool IsTrial
        {
            get
            {
                return SelfBaseGlobal.PlatSDKMgr.DistributionType == Distribution.Trial;
            }
        }

        public bool IsDevBuild
        {
            get
            {
                return Config.IsDevBuild;
            }
        }
        public override void OnCreate()
        {
            base.OnCreate();
            Config = BuildConfig.Ins;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);

        }

        #region isCan
        /// <summary>
        /// 数据库版本是否兼容
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsInData(int data)
        {
            return Config.Data == data;
        }
        #endregion
    }
}