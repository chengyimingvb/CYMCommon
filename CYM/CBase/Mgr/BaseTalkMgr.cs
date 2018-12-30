//**********************************************
// Class Name	: BaseSpawnMgr
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
    public class BaseTalkMgr<TData> : BaseGlobalCoreMgr, ITableDataMgr<TData> where TData : TDBaseTalkData, new()
    {
        #region Callback val
        /// <summary>
        /// 开始一段对话
        /// </summary>
        public Callback<TData, TalkFragment> Callback_OnStartTalk { get; set; }
        /// <summary>
        /// 下一段对话
        /// </summary>
        public Callback<TData, TalkFragment, int> Callback_OnNextTalk { get; set; }
        /// <summary>
        /// 显示对话
        /// </summary>
        public Callback<TData, TalkFragment, int> Callback_OnTalk { get; set; }
        /// <summary>
        /// 结束一段对话
        /// </summary>
        public Callback<TData,string,int> Callback_OnEndTalk { get; set; }
        /// <summary>
        /// 选择对话项目
        /// </summary>
        public Callback<TData, string,int> Callback_OnSelect { get; set; }
        #endregion

        #region prop
        /// <summary>
        /// 当前的对话索引
        /// </summary>
        public int CurTalkIndex { get; private set; }
        /// <summary>
        /// 有对话?
        /// </summary>
        public bool IsStartTalk { get; private set; } = false;
        /// <summary>
        /// 当前选的的对话选项
        /// </summary>
        public string CurSelectOption { get; private set; } = BaseConstMgr.STR_Inv;
        /// <summary>
        /// 选择对话选项的Index
        /// </summary>
        public int CurSelectOptionIndex { get; private set; } = 0;
        /// <summary>
        /// 暂停标记
        /// </summary>
        public bool PauseFlag { get; private set; } = false;
        #endregion

        #region set
        /// <summary>
        /// 开始一段对话
        /// isPause=暂停
        /// isUnPauseOnEndTalk=对话结束后取消暂停
        /// </summary>
        /// <param name="id"></param>
        public TalkFragment StartOption(string id)
        {
            CurData = Table.Find(id);
            if (CurData == null)
            {
                CLog.Error("没有找到对话:{0}", id);
                return null;
            }
            return Start(id,CurData.Fragments.Count - 1);
        }
        public virtual TalkFragment Start(string id,int index=0)
        {
            CurData = Table.Find(id);
            if (CurData == null)
            {
                CLog.Error("没有找到对话:{0}", id);
                return null;
            }
            CurTalkIndex = index;
            CurSelectOption = BaseConstMgr.STR_Inv;
            CurSelectOptionIndex = -1;
            if (IsHave())
            {
                var ret = CurData.Fragments[CurTalkIndex];
                Callback_OnStartTalk?.Invoke(CurData, ret);
                Callback_OnTalk?.Invoke(CurData, ret, CurTalkIndex);
                OnTalk(CurData, ret, CurTalkIndex);
                OnStartTalk(CurData, ret);
                IsStartTalk = true;
                if (!PauseFlag)
                    SelfBaseGlobal.PlotMgr.EnablePlotMode(true);
                PauseFlag = true;
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 下一段对话
        /// </summary>
        public virtual TalkFragment Next()
        {
            if (IsLockNextTalk)
                return null;
            if (!IsStartTalk)
                return null;
            if (IsInOption() && CurSelectOption.IsInvStr())
                return CurTalkFragment();
            CurTalkIndex++;
            if (IsHave())
            {
                var ret = CurTalkFragment();
                Callback_OnNextTalk?.Invoke(CurData, ret, CurTalkIndex);
                Callback_OnTalk?.Invoke(CurData, ret, CurTalkIndex);
                OnTalk(CurData, ret, CurTalkIndex);
                return ret;
            }
            else
            {
                Stop();
                return null;
            }
        }
        /// <summary>
        /// 点击选项,语法糖函数
        /// </summary>
        /// <param name="index"></param>
        public virtual void ClickOption(int index)
        {
            SelectOption(index);
            Next();
        }
        /// <summary>
        /// 点击对话,语法糖函数
        /// </summary>
        public virtual void ClickTalk()
        {
            if (!IsInOption())
                Next();
        }
        /// <summary>
        /// 选择选项
        /// </summary>
        /// <returns></returns>
        public virtual string SelectOption(int index)
        {
            if (!IsInOption())
                return BaseConstMgr.STR_Inv;
            CurSelectOptionIndex =Mathf.Clamp(index,0, CurData.Option.Count-1);
            CurSelectOption = CurData.GetOption(CurSelectOptionIndex);
            Callback_OnSelect?.Invoke(CurData, CurSelectOption, CurSelectOptionIndex);
            return CurSelectOption;
        }
        /// <summary>
        /// 选择上一个选项
        /// </summary>
        public void SelectPreOption()
        {
            CurSelectOptionIndex--;
            if (CurSelectOptionIndex < 0)
                CurSelectOptionIndex = CurData.Option.Count - 1;
            SelectOption(CurSelectOptionIndex);
        }
        /// <summary>
        /// 选择下一个选项
        /// </summary>
        public void SelectNextOption()
        {
            CurSelectOptionIndex++;
            if (CurSelectOptionIndex > CurData.Option.Count - 1)
                CurSelectOptionIndex = 0;
            SelectOption(CurSelectOptionIndex);
        }
        public virtual void Stop()
        {
            if (CurData != null)
            {
                IsStartTalk = false;
                //回调里面可能会重新开启对话
                Callback_OnEndTalk?.Invoke(CurData, CurSelectOption,CurSelectOptionIndex);
                OnEndTalk(CurData, CurSelectOption, CurSelectOptionIndex);
                if (IsStartTalk)
                    return;
                if (PauseFlag)
                {
                    SelfBaseGlobal.PlotMgr.EnablePlotMode(false);
                }
                CurData = null;
                PauseFlag = false;
            }
        }
        /// <summary>
        /// 是否拥有对话
        /// </summary>
        /// <returns></returns>
        public bool IsHave()
        {
            if (CurData == null) return false;
            if (CurData.Fragments == null) return false;
            if (CurTalkIndex >= CurData.Fragments.Count)
                return false;
            return true;
        }
        /// <summary>
        /// 是否在选项界面中
        /// </summary>
        /// <returns></returns>
        public bool IsInOption()
        {
            if (CurData.IsHaveOption() && CurTalkFragment().IsLasted)
                return true;
            return false;
        }
        /// <summary>
        /// 是否锁住对话
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLockNextTalk=>false;
        #endregion

        #region get
        public TalkFragment CurTalkFragment()
        {
            if (!IsHave())
                return new TalkFragment();
            return CurData.Fragments[CurTalkIndex];
        }
        #endregion

        #region must override
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public virtual TData CurData
        {
            get;
            set;
        }
        #endregion

        #region Callback
        protected virtual void OnStartTalk(TData talkData, TalkFragment fragment)
        {

        }
        protected virtual void OnTalk(TData talkData, TalkFragment fragment, int index)
        {

        }
        protected virtual void OnEndTalk(TData talkData, string op, int index)
        {

        }
        #endregion
    }

}