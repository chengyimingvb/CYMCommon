using System;
using UnityEngine;
using CYM;
using System.Collections.Generic;
using MoonSharp.Interpreter;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class TDBaseBattleData : BaseConfig<TDBaseBattleData>
    {
        public string SceneName { get; set; } = BaseConstMgr.STR_Inv;
        public string GetSceneName()
        {
            if (SceneName.IsInvStr())
                return TDID.Replace(BaseConstMgr.Prefix_Scene, "");
            return SceneName;
        }


    }
}
