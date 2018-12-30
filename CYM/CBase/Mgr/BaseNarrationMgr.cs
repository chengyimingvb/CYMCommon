using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class BaseNarrationMgr<TData> : BaseGlobalCoreMgr, ITableDataMgr<TData>  where TData : TDBaseNarrationData, new()
    {
        #region Callback val
        /// <summary>
        /// 开始一段旁白
        /// </summary>
        public Callback<TData, NarrationFragment> Callback_OnStartNarration { get; set; }
        /// <summary>
        /// 下一段旁白
        /// </summary>
        public Callback<TData, NarrationFragment, int> Callback_OnNextNarration { get; set; }
        /// <summary>
        /// 结束一段旁白
        /// </summary>
        public Callback<TData, NarrationFragment> Callback_OnEndNarration { get; set; }
        #endregion

        #region val
        /// <summary>
        /// 当前的旁白索引
        /// </summary>
        public int CurNarrationIndex { get; private set; }
        /// <summary>
        /// 有旁白?
        /// </summary>
        public bool IsStartNarration { get; private set; } = false;
        /// <summary>
        /// 暂停标记
        /// </summary>
        public bool PauseFlag { get; private set; } = false;
        #endregion

        #region set
        /// <summary>
        /// 开始一段旁白
        /// isPause=暂停
        /// isUnPauseOnEndTalk=对话结束后取消暂停
        /// </summary>
        /// <param name="id"></param>
        public virtual NarrationFragment Start(string id)
        {
            CurData = Table.Find(id);
            if (CurData == null)
            {
                CLog.Error($"没有找到这个Plot:{id}");
                return null;
            }
            CurNarrationIndex = 0;
            if (IsHave())
            {
                var ret = CurData.Fragments[CurNarrationIndex];
                Callback_OnStartNarration?.Invoke(CurData, ret);
                IsStartNarration = true;
                if (!PauseFlag)
                {
                    SelfBaseGlobal.BattleMgr.LockGameStartFlow(true);
                    SelfBaseGlobal.PlotMgr.EnablePlotMode(true);
                }
                PauseFlag = true;
                return ret;
            }
            return null;
        }
        /// <summary>
        /// 下一段旁白
        /// </summary>
        public virtual NarrationFragment Next()
        {
            if (!IsStartNarration)
                return null;
            CurNarrationIndex++;
            if (IsHave())
            {
                var ret = CurNarrationFragment();
                Callback_OnNextNarration?.Invoke(CurData, ret, CurNarrationIndex);
                return ret;
            }
            else
            {
                Stop();
                return null;
            }
        }
        public virtual void Stop()
        {
            var ret = CurNarrationFragment();
            IsStartNarration = false;
            Callback_OnEndNarration?.Invoke(CurData, ret);
            if (IsStartNarration)
                return;
            if (PauseFlag)
            {
                SelfBaseGlobal.BattleMgr.LockGameStartFlow(false);
                SelfBaseGlobal.PlotMgr.EnablePlotMode(false);
            }
            //重置状态
            PauseFlag = false;
        }
        /// <summary>
        /// 是否拥有对话
        /// </summary>
        /// <returns></returns>
        public bool IsHave()
        {
            if (CurData == null) return false;
            if (CurData.Fragments == null) return false;
            if (CurNarrationIndex >= CurData.Fragments.Count)
                return false;
            return true;
        }
        #endregion

        #region get
        public NarrationFragment CurNarrationFragment()
        {
            if (!IsHave())
                return new NarrationFragment();
            return CurData.Fragments[CurNarrationIndex];
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

    }
}