//**********************************************
// Discription	：CYMBaseCoreComponent
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;
using CYM.Utile;
namespace CYM
{
    public class BaseFPSMgr : BaseGlobalCoreMgr
    {
        bool isShowFPS = false;
        AFPSCounter FPSCounter;
        GameObject FPSCounterGO;
        public override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnAllLoadEnd()
        {
            base.OnAllLoadEnd();
            FPSCounterGO = SelfBaseGlobal.GRMgr.GetResources("BaseFPSCounter", true);
            FPSCounter = FPSCounterGO.GetComponentInChildren<AFPSCounter>();
            ShowFPS(isShowFPS);
        }

        #region set
        public virtual void ShowFPS(bool b)
        {
            if (FPSCounter == null)
            {

            }
            else
            {
                FPSCounter.OperationMode = b ? OperationMode.Normal : OperationMode.Background;
                SelfBaseGlobal.SettingsMgr.GetBaseSettings().ShowFPS = b;
            }
            isShowFPS = b;
        }
        #endregion

        #region get
        public virtual FPSCounterData GetFPSData()
        {
            return FPSCounter.fpsCounter;
        }
        #endregion
    }

}