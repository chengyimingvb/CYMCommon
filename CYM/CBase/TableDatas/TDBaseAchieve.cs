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
    public class TDBaseAchieveData : BaseConfig<TDBaseAchieveData>
    {
        #region 属性
        public List<BaseTarget> Targets { get; set; } = new List<BaseTarget>();
        public bool State { get; set; } = false;
        public DateTime UnlockTime { get; set; }
        public string SourceName { get; set; }
        public string SourceDesc { get; set; }
        #endregion

        #region get
        /// <summary>
        /// 获得总数
        /// </summary>
        /// <returns></returns>
        public virtual int GetTotalCount()
        {
            return 0;
        }
        /// <summary>
        /// 当前百分比
        /// </summary>
        /// <returns></returns>
        public virtual float GetPercent()
        {
            if (State)
                return 1.0f;
            return 0.0f;
        }
        #endregion

    }
}
