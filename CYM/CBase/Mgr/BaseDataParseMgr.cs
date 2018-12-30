//**********************************************
// Class Name	: DataParseMgr
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
using System;

namespace CYM
{
    public class BaseDataParseMgr : BaseGlobalCoreMgr
    {
        /// <summary>
        /// lua管理器列表类
        /// </summary>
        static List<ITDLuaMgr> LuaMgrList { get; set; } = new List<ITDLuaMgr>();

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            foreach (var item in LuaMgrList)
            {
                SelfBaseGlobal.LuaMgr.Callback_OnLuaParseStart += item.OnLuaParseStart;
                SelfBaseGlobal.LuaMgr.Callback_OnLuaParseEnd += item.OnLuaParseEnd;
            }
        }
        public override void OnDestroy()
        {
            foreach (var item in LuaMgrList)
            {
                SelfBaseGlobal.LuaMgr.Callback_OnLuaParseStart -= item.OnLuaParseStart;
                SelfBaseGlobal.LuaMgr.Callback_OnLuaParseEnd -= item.OnLuaParseEnd;
            }
            LuaMgrList.Clear();
            base.OnDestroy();
        }
        #endregion

        public static void AddTDLuaMgr(ITDLuaMgr TDLuaMgr)
        {
            LuaMgrList.Add(TDLuaMgr);
        }
    }

}