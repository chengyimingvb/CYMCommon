//------------------------------------------------------------------------------
// BaseAlertMgr.cs
// Copyright 2019 2019/3/1 
// Created by CYM on 2019/3/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;

namespace CYM
{
    public enum AlertType
    {
        Normal,         //持续
        Diplomacy,      //交互
        Reply,          //一次性
    }
    public class BaseAlertMgr<TData> : BaseCoreMgr, ITableDataMgr<TData> where TData : TDBaseAlertData, new()
    {
        #region Callback
        public Callback<TDBaseAlertData> Callback_OnAdded { get; set; }
        public Callback<TDBaseAlertData> Callback_OnRemoved { get; set; }
        public Callback<TDBaseAlertData> Callback_OnMerge { get; set; }
        public Callback<TDBaseAlertData> Callback_OnCommingTimeOut { get; set; }
        #endregion

        #region prop
        BaseUnit LocalPlayer => SelfBaseGlobal.ScreenMgr.BaseLocalPlayer;
        private IDMgr IDMgr = new IDMgr();
        private List<TDBaseAlertData> ClearData = new List<TDBaseAlertData>();
        private Dictionary<string, List<TDBaseAlertData>> CachesAlert = new Dictionary<string, List<TDBaseAlertData>>();
        public List<TDBaseAlertData> Data { get; private set; } = new List<TDBaseAlertData>();
        public virtual LuaTDMgr<TData> Table => throw new System.NotImplementedException();
        public TData CurData { get; set; }
        protected virtual string CommonAlert => "Alert_Common";
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGameLogicTurn = true;
        }
        public override void GameLogicTurn()
        {
            base.GameLogicTurn();
            foreach (var item in Data)
            {
                item.GameLogicTurn();
                if (item.IsOver())
                {
                    ClearData.Add(item);
                }
                if (item.IsCommingTimeOut())
                {
                    item.OnCommingTimeOut();
                    Callback_OnCommingTimeOut?.Invoke(item);
                }
            }
            for (int i = 0; i < ClearData.Count; ++i)
            {
                ClearData[i].OnTimeOut();
                RemoveAlert(ClearData[i]);
            }
            ClearData.Clear();
        }
        #endregion

        #region set
        public TDBaseAlertData AddPlCOM(BaseUnit cast = null)
        {
            if (SelfBaseUnit == null)
                return null;
            if (!SelfBaseUnit.IsLocalPlayer())
                return null;
            return AddCOM(cast);
        }
        public TDBaseAlertData AddCOM(BaseUnit cast = null)
        {
            return Add(CommonAlert, cast);
        }
        public TDBaseAlertData AddPl(string alertName, BaseUnit cast = null)
        {
            if (SelfBaseUnit == null)
                return null;
            if (!SelfBaseUnit.IsLocalPlayer())
                return null;
            return Add(alertName, cast);
        }
        public TDBaseAlertData Add(string alertName, BaseUnit cast = null)
        {
            if (!Table.Contains(alertName))
            {
                if(CommonAlert==alertName)
                    CLog.Error("请手动添加 CommonAlert");
                return null;
            }
            TDBaseAlertData tempAlert = null;
            if (IsHaveCache(alertName))
            {
                tempAlert = GetCache(alertName);
                CachesAlert.Remove(alertName);
            }
            else
            {
                tempAlert = Table.Find(alertName).Copy();
            }

            tempAlert.Cast = cast?cast:LocalPlayer;

            if (tempAlert == null)
            {
                CLog.Error("未找到alert errorId=" + alertName);
                return null;
            }
            //判断通知是否可以被合并
            var mergeAlert = CanMerge(tempAlert);
            if (mergeAlert != null)
            {
                mergeAlert.OnMerge();
                Callback_OnMerge?.Invoke(mergeAlert);
            }
            else
            {
                tempAlert.ID = IDMgr.GetNextId();
                Data.Add(tempAlert);
                tempAlert.OnBeAdded(SelfBaseUnit);
                Callback_OnAdded?.Invoke(tempAlert);
                tempAlert.OnStart();
            }
            return tempAlert;
        }
        public void RemoveAlert(TDBaseAlertData alert)
        {
            if (alert == null) return;
            Data.Remove(alert);
            Callback_OnRemoved?.Invoke(alert);
            alert.OnBeRemoved();
            alert.OnEnd();
            AddToCache(alert);
            IDMgr.Remove(alert.ID);
        }
        public void RemoveAlertByID(string id)
        {
            RemoveAlert(Data.Find((x) => { return id == x.TDID; }));
        }
        public TDBaseAlertData GetAlert(int id)
        {
            return Data.Find((x) => { return id == x.ID; });
        }
        /// <summary>
        /// 是否可以被合并，相同的Alert将会被合并
        /// </summary>
        /// <param name="alert"></param>
        /// <returns></returns>
        private TDBaseAlertData CanMerge(TDBaseAlertData alert)
        {
            for (int i = 0; i < Data.Count; ++i)
            {
                //普通的通知id相同就合并
                if (alert.AlertType == AlertType.Normal)
                {
                    if (alert.TDID == Data[i].TDID)
                        return Data[i];
                }
                //外交alert要国家相同才行
                else if (alert.AlertType == AlertType.Diplomacy)
                {
                    if (alert.TDID == Data[i].TDID &&
                        alert.Cast == Data[i].Cast &&
                        alert.SelfBaseUnit == Data[i].SelfBaseUnit)
                        return Data[i];
                }
                //回信Alert不做合并
                else if (alert.AlertType == AlertType.Reply)
                {

                }
            }
            return null;
        }
        #endregion

        #region Cache
        void AddToCache(TDBaseAlertData alert)
        {
            if (!CachesAlert.ContainsKey(alert.TDID))
            {
                CachesAlert.Add(alert.TDID, new List<TDBaseAlertData>());
            }
            CachesAlert[alert.TDID].Add(alert);
        }
        bool IsHaveCache(string id)
        {
            if (!CachesAlert.ContainsKey(id))
            {
                return false;
            }
            if (CachesAlert[id].Count > 0)
                return true;
            return false;
        }
        TDBaseAlertData GetCache(string id)
        {
            if (!IsHaveCache(id))
                return null;
            var ret = CachesAlert[id][0];
            CachesAlert[id].RemoveAt(0);
            return ret;
        }
        #endregion
    }
}