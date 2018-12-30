﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseGaoppSDKMgr : BasePlatSDKMgr
    {
        Timer timer = new Timer();
        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            IsInited = true;
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否为正版
        /// </summary>
        public override bool IsLegimit
        {
            get
            {
                return IsInited;
            }
        }
        /// <summary>
        /// 文件APPid不一致
        /// </summary>
        public override bool IsDifferentAppId
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 是否支持云存档
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportCloudArchive()
        {
            return false;
        }
        /// <summary>
        /// 是否支持平台语言
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformLanguage()
        {
            return false;
        }
        /// <summary>
        /// 是否支持menu prop UI
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportMenuPropUI()
        {
            return false;
        }
        #endregion

        #region set

        #endregion

        #region Get
        public override string GetMainMenuTitle()
        {
            return "";
        }
        protected override uint GetAppId()
        {
            return 2000126;
        }
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <returns></returns>
        public override string GetErrorInfo()
        {
            if (IsDifferentAppId)
                return "The game is not activated, app found the different app id,do you changed any thing?";
            if (!IsInited)
                return "Can't init rail sdk";
            return "Error";
        }
        /// <summary>
        /// 平台类型
        /// </summary>
        public override Distribution DistributionType
        {
            get
            {
                return Distribution.Gaopp;
            }
        }
        #endregion

        #region Callback
        #endregion
    }

}