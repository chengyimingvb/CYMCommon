//**********************************************
// Class Name	: BattleCameraMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;
namespace CYM
{
    public class BaseProfilterMgr : BaseGFlowMgr
    {
        public void StartProfilter(string name,GameObject obj=null)
        {
            //if (!SelfGlobal.SettingsMgr.DevSettings.IsProfilter)
            //    return;
            if(obj!=null)
                UnityEngine.Profiling.Profiler.BeginSample(name,obj);
            else
                UnityEngine.Profiling.Profiler.BeginSample(name);
        }
        public void EndProfilter()
        {
            //if (!SelfGlobal.SettingsMgr.DevSettings.IsProfilter)
            //    return;
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

}