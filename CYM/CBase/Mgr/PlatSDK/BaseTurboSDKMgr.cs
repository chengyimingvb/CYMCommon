using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using CommunitySDK;
using UnityEngine;

namespace CYM
{
    public class BaseTurboSDKMgr : BasePlatSDKMgr
    {
        //bool isInFatch = true;
        //Timer timer = new Timer();
        //#region life
        //public override void OnBeAdded(CYM.IMono mono)
        //{
        //    base.OnBeAdded(mono);
        //    CallBackResult_OwnershipVerify ownershipVerify = new CallBackResult_OwnershipVerify();
        //    ownershipVerify.ownershipVerifyCallback(OwnershipVerify, "eventID1", (int)GetAppId());
        //    IsInited = true;
        //    timer.Restart();
        //    //timer.Restart();
        //    //while (isInFatch&& timer.Elapsed()<=10.0f)
        //    //{
        //    //    Thread.Sleep(5);
        //    //}


        //}
        //public override void OnEnable()
        //{
        //    base.OnEnable();

        //}
        //public override void OnStart()
        //{
        //    base.OnStart();

        //}
        //public override void OnDisable()
        //{

        //    base.OnDisable();
        //}
        //public override void OnUpdate()
        //{
        //    base.OnUpdate();
        //    //if (timer.Elapsed()>=1.0f)
        //    //{
        //    //NativeEntryPoints.runCallback();
        //    //    timer.Restart();
        //    //}

        //}
        //public override void OnFixedUpdate()
        //{
        //    base.OnFixedUpdate();
        //}
        //public override void OnDestroy()
        //{

        //    base.OnDestroy();
        //}
        //#endregion

        //#region get
        //protected override uint GetAppId()
        //{
        //    return 1305;
        //}
        ///// <summary>
        ///// 平台类型
        ///// </summary>
        //public override Distribution DistributionType
        //{
        //    get
        //    {
        //        return Distribution.Turbo;
        //    }
        //}
        //public override string GetErrorInfo()
        //{
        //    return "The Game is Not Active";
        //}
        //#endregion

        //#region is
        //public override bool IsLegimit
        //{
        //    get
        //    {
        //        return IsInited;
        //    }
        //}
        //public override bool IsSuportCloudArchive()
        //{
        //    return false;
        //}
        //public override bool IsSuportPlatformLanguage()
        //{
        //    return false;
        //}
        //#endregion

        //#region Set

        //#endregion

        //#region Callback
        //void OwnershipVerify(bool result)
        //{

        //}
        //#endregion
    }

}