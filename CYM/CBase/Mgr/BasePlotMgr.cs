using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

namespace CYM
{
    public class BasePlotMgr<TData> : BaseGlobalCoreMgr, IBasePlotMgr, ITableDataMgr<TData> where TData : TDBasePlotData, new()
    {
        #region Callback value
        public event Callback<bool,bool,int> Callback_OnPlotMode;
        public event Callback<int> Callback_OnAddPlotIndex;
        #endregion

        #region prop
        /// <summary>
        /// 是否禁用AI
        /// </summary>
        public bool IsEnableAI { get; protected set; } = true;
        /// <summary>
        /// 是否开启了剧情模式
        /// </summary>
        public BoolState IsEnablePlotMode { get; protected set; } = new BoolState();
        /// <summary>
        /// 当前的剧情节点
        /// </summary>
        public int CurPlotIndex { get; protected set; } = 0;
        protected BaseCoroutineMgr BattleCoroutine => SelfBaseGlobal.BattleCoroutine;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            CurData?.OnUpdate();
        }
        #endregion

        #region is
        public bool IsPlotMode
        {
            get
            {
                return IsEnablePlotMode.IsIn();
            }
        }
        public bool IsHavePlot()
        {
            return SelfBaseGlobal.DiffMgr.IsHavePlot() && !SelfBaseGlobal.SettingsMgr.GetBaseDevSettings().NoPlot;
        }
        #endregion

        #region set
        /// <summary>
        /// 禁用ai
        /// </summary>
        /// <param name="b"></param>
        public virtual void EnableAI(bool b)
        {
            IsEnableAI = b;
        }
        /// <summary>
        /// 启动剧情,自定义
        /// </summary>
        public void EnablePlotMode(bool b,int type=0)
        {
            IsEnablePlotMode.Push(b);
            OnEnablePlotMode(b,IsEnablePlotMode.IsIn(), type);
        }
        public void ResumePlotMode()
        {
            IsEnablePlotMode.Reset();
            OnEnablePlotMode(false,IsEnablePlotMode.IsIn(),0);
        }
        /// <summary>
        /// 开始一段剧情
        /// </summary>
        public virtual void Start(string id)
        {
            if (id.IsInvStr())
                return;
            var temp = Table.Find(id);
            if (temp == null)
            {
                CLog.Error("无法找到剧情:{0}", id);
                return;
            }
            else
            {
                CurData = temp.Copy() as TData;
            }
            CurPlotIndex = 0;
            CurData.OnBeAdded(SelfBaseGlobal);
            BattleCoroutine.Run(CurData.OnPlotStart());
        }
        /// <summary>
        /// 停止一段剧情
        /// </summary>
        public virtual void Stop()
        {
            if (CurData == null)
                return;
            CurData.OnBeRemoved();
            CurData = null;
        }
        /// <summary>
        /// 推进剧情
        /// </summary>
        public virtual int AddIndex()
        {
            CurPlotIndex++;
            Callback_OnAddPlotIndex?.Invoke(CurPlotIndex);
            return CurPlotIndex;
        }
        public CoroutineHandle CustomStartBattleCoroutine()
        {
            if (CurData == null)
                return BattleCoroutine.Run(CustomStartBattleFlow());
            return BattleCoroutine.Run(CurData.CustomStartBattleFlow());
        }
        protected virtual IEnumerator<float> CustomStartBattleFlow()
        {
            yield break;

        }
        #endregion

        #region Must overide
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                throw new NotImplementedException("这个函数必须被实现");
            }
        }

        public virtual TData CurData
        {
            get;
            set;
        }
        #endregion

        #region Event
        protected override void OnAllLoadEnd()
        {
        }
        protected virtual void OnEnablePlotMode(bool b,bool curState,int type)
        {
            Callback_OnPlotMode?.Invoke(b, curState, type);
        }
        #endregion
    }

}