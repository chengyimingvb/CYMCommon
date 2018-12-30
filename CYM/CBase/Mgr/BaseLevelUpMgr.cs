
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
//using System.Data;
//**********************************************
// Class Name	: CYMBaseLanguage
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BaseLevelupMgr : BaseCoreMgr
    {
        #region Callback
        public event Callback Callback_OnLevelUp;
        public event Callback Callback_OnAddExp;
        #endregion

        #region prop
        /// <summary>
        /// 当前经验
        /// </summary>
        public int CurExp { get; protected set; }
        /// <summary>
        /// 最大经验
        /// </summary>
        public int MaxExp { get; protected set; }
        #endregion

        #region set
        /// <summary>
        /// 获得最大经验值
        /// </summary>
        /// <returns></returns>
        protected virtual int GetMaxExp(int lv)
        {
            return Mathf.FloorToInt((Mathf.Pow((lv - 1), 2) + 60) / 5 * ((lv - 1) * 2 + 60));
        }
        /// <summary>
        /// 初始化经验
        /// </summary>
        public int SetMaxExp(int lv)
        {
            MaxExp = GetMaxExp(lv);
            return MaxExp;
        }
        public int SetMaxExp(float lv)
        {
            return SetMaxExp(Mathf.CeilToInt(lv));
        }
        /// <summary>
        /// 测试经验
        /// </summary>
        public void TestLvExp()
        {
            for (int i = 0; i < 100; ++i)
            {
                CLog.Info("Lv:{0},{1}", i, SetMaxExp(i));
            }
        }
        /// <summary>
        /// 增加经验
        /// </summary>
        public virtual void AddExp(int val)
        {
            CurExp += val;
            Callback_OnAddExp?.Invoke();
            if (CurExp >= MaxExp)
            {
                Callback_OnLevelUp?.Invoke();
                OnLevelUp();
            }
        }
        public virtual void SetExp(int val)
        {
            CurExp = val;
        }
        #endregion

        #region get
        /// <summary>
        /// 经验值百分比
        /// </summary>
        public float Percent
        {
            get
            {
                if (MaxExp <= 0)
                    return 0.0f;
                return ((float)CurExp / (float)MaxExp);
            }
        }
        /// <summary>
        /// 获得经验字符窜
        /// </summary>
        /// <returns></returns>
        public string GetExpStr()
        {
            return string.Format($"{CurExp}/{MaxExp}");
        }
        #endregion

        #region Callback
        protected virtual void OnLevelUp()
        {
            CurExp = 0;
        }
        #endregion

    }
}
