using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    public class BaseRailSDKMgr : BasePlatSDKMgr
    {
        //Timer timer = new Timer();
        //#region life
        //public override void OnBeAdded(IMono mono)
        //{
        //    base.OnBeAdded(mono);
        //    if (IsInited)
        //    {
        //        CLog.Error("rail is already initialized!");
        //        return;
        //    }
        //    RailGameID id = new RailGameID();
        //    id.id_ = GetAppId();
        //    //string[] argv = new string[1];
        //    bool need_restart = rail_api.RailNeedRestartAppForCheckingEnvironment(id, 0, null);
        //    if (need_restart)
        //    {
        //        CLog.Error("CheckingEnvironment failed, please run in TGP");
        //        return;
        //    }
        //    IsInited = rail_api.RailInitialize();
        //    if (IsInited)
        //    {
        //        CLog.Info("RailInitialize success");
        //    }
        //    else
        //    {
        //        CLog.Error("RailInitialize failed");
        //    }
        //}
        //public override void OnDisable()
        //{
        //    CLog.Info("UnInitRail...");
        //    if (IsInited)
        //    {
        //        rail_api.CSharpRailUnRegisterAllEvent();
        //        rail_api.RailFinalize();
        //    }
        //    else
        //    {
        //        CLog.Error("rail is not initialized, uninit fail!");
        //    }
        //    base.OnDisable();
        //}
        //public override void OnUpdate()
        //{
        //    base.OnUpdate();
        //    if (IsInited)
        //    {
        //        if (timer.Elapsed() >= 1)
        //        {
        //            timer.Restart();
        //            rail_api.RailFireEvents();
        //        }
        //    }
        //}
        //#endregion

        //#region is
        ///// <summary>
        ///// 是否为正版
        ///// </summary>
        //public override bool IsLegimit
        //{
        //    get
        //    {
        //        return IsInited;
        //    }
        //}
        ///// <summary>
        ///// 文件APPid不一致
        ///// </summary>
        //public override bool IsDifferentAppId
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}
        ///// <summary>
        ///// 是否支持云存档
        ///// </summary>
        ///// <returns></returns>
        //public override bool IsSuportCloudArchive()
        //{
        //    return false;
        //}
        ///// <summary>
        ///// 是否支持平台语言
        ///// </summary>
        ///// <returns></returns>
        //public override bool IsSuportPlatformLanguage()
        //{
        //    return false;
        //}
        //#endregion

        //#region set

        //#endregion

        //#region Get
        //protected override uint GetAppId()
        //{
        //    return 2000126;
        //}
        ///// <summary>
        ///// 得到错误信息
        ///// </summary>
        ///// <returns></returns>
        //public override string GetErrorInfo()
        //{
        //    if (IsDifferentAppId)
        //        return "The game is not activated, app found the different app id,do you changed any thing?";
        //    if (!IsInited)
        //        return "Can't init rail sdk";
        //    return "Error";
        //}
        ///// <summary>
        ///// 平台类型
        ///// </summary>
        //public override Distribution DistributionType
        //{
        //    get
        //    {
        //        return Distribution.Rail;
        //    }
        //}
        //#endregion

        //#region Callback

        //#endregion
    }

}