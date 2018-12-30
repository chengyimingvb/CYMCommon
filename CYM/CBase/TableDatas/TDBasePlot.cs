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
    public enum PlotModeType : int
    {
        Normal = 0,//剧情模式
        Manual = 1,//玩家手动暂停
    }
    public class TDBasePlotData : BaseConfig<TDBasePlotData>
    {
        protected int CurPlotIndex => SelfBaseGlobal.PlotMgr.CurPlotIndex;

        protected int AddPlotIndex()
        {
            return SelfBaseGlobal.PlotMgr.AddIndex();
        }

        public virtual IEnumerator<float> OnPlotStart()
        {
            yield break;
        }

        public virtual IEnumerator<float> CustomStartBattleFlow()
        {
            yield break;

        }
    }

}