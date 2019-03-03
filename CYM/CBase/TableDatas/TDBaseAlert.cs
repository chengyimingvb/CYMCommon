//------------------------------------------------------------------------------
// TDBaseAlert.cs
// Copyright 2019 2019/3/1 
// Created by CYM on 2019/3/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using UnityEngine.EventSystems;

namespace CYM
{
    public class TDBaseAlertData : BaseConfig<TDBaseAlertData>
    {
        #region lua
        /// <summary>
        /// 通知类型
        /// </summary>
        public AlertType AlertType { get; set; } = AlertType.Normal;
        /// <summary>
        /// 背景
        /// </summary>
        public string Bg { get; set; }
        /// <summary>
        /// 出现音效
        /// </summary>
        public string StartSFX { get; set; }
        /// <summary>
        /// 显示条件
        /// </summary>
        public Func<bool> IsShow = () => true;
        /// <summary>
        /// 执行动作
        /// </summary>
        public Callback<BasePresenter, PointerEventData> OnClick = (x,y) => { };
        /// <summary>
        /// 存在天数
        /// </summary>
        public float TotalTurn { get; set; }
        /// <summary>
        /// 提示文字
        /// </summary>
        public string TipStr { get; set; }
        #endregion

        #region prop
        //当前天数
        public float CurTurn { get; set; }
        //是否时间快结束了
        public bool IsCommingTimeOutFalg { get; set; }
        public BaseUnit Cast { get; set; }
        #endregion

        public Sprite GetBg()
        {
            return GRMgr.GetIcon(Bg);
        }

        /// <summary>
        /// 是否结束
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOver()
        {
            if (TotalTurn <= 0)
                return false;
            if (CurTurn >= TotalTurn)
                return true;
            return false;
        }

        /// <summary>
        /// 判断时间是否快结束了,执行一次
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCommingTimeOut()
        {
            if (IsCommingTimeOutFalg)
                return false;
            if (TotalTurn <= 0)
                return false;
            if (CurTurn >= (int)(TotalTurn * 0.8f))
                return true;
            return false;
        }
        #region Callback
        /// <summary>
        /// 通知超时后未处理
        /// </summary>
        public virtual void OnTimeOut()
        {
        }
        /// <summary>
        /// 通知即将超时
        /// </summary>
        public virtual void OnCommingTimeOut()
        {
            IsCommingTimeOutFalg = true;
        }
        /// <summary>
        /// 通知合并
        /// </summary>
        public virtual void OnMerge()
        {
            CurTurn = 0;
            IsCommingTimeOutFalg = false;
        }
        /// <summary>
        /// 通知开始
        /// </summary>
        public virtual void OnStart()
        {
            CurTurn = 0;
        }
        /// <summary>
        /// 通知结束
        /// </summary>
        public virtual void OnEnd()
        {

        }
        #endregion


        public class TDBaseAlert<T> : LuaTDMgr<T> where T:TDBaseAlertData,new()
        {
            public TDBaseAlert():base()
            {
                Register(new T {
                    TDID="Alert_Common",
                });
            }
        }
    }
}