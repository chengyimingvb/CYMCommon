//------------------------------------------------------------------------------
// BaseAlertBarView.cs
// Copyright 2019 2019/3/2 
// Created by CYM on 2019/3/2
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using CYM.Pool;
using System;

namespace CYM
{
    public class BaseAlertBarView<TData> : BaseUIView where TData : TDBaseAlertData, new()
    {
        [SerializeField]
        Transform AlertBar;
        [SerializeField]
        BaseDupplicate DP_AlertPoints;
        [SerializeField]
        GameObject Prefab;
        [SerializeField]
        RectTransform StartPos;

        #region prop
        BaseGRMgr GRMgr => SelfBaseGlobal.GRMgr;
        HashList<BaseAlertItem> ActiveItems = new HashList<BaseAlertItem>();
        GOPool GOPool;
        private CoroutineHandle delayCallTask;
        #endregion

        #region life
        private BaseAlertMgr<TData> AlertMgr { get; set; }
        protected virtual BaseAlertMgr<TData> NewAlertMgr => throw new NotImplementedException();
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            GOPool = new GOPool(Prefab, AlertBar);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateAlertPoint();
        }
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in ActiveItems)
            {
                item.Refresh();
            }
        }
        #endregion

        #region Alert
        void UpdateAlertPoint()
        {
            int IndexPoint = 0;
            for (int i = 0; i < ActiveItems.Count; ++i)
            {
                var item = ActiveItems[i];
                if (!item.IsShow)
                    continue;
                if (IndexPoint < DP_AlertPoints.GOCount)
                {
                    item.RectTrans.position = Vector3.LerpUnclamped(item.RectTrans.position, DP_AlertPoints.GOs[IndexPoint].transform.position, 0.1f);
                    IndexPoint++;
                }
            }
        }
        void AddAlert(TDBaseAlertData alert)
        {
            GameObject go = GOPool.Spawn();
            BaseAlertItem alertItem = go.GetComponent<BaseAlertItem>();
            alertItem.Init(new BaseButtonData {
                IconStr = alert.Icon,
                BgStr = alert.Bg,
                OnClick = alert.OnClick,
                OnShow = OnAlertShow,
            });
            alertItem.Show(true);
            alertItem.RectTrans.position = StartPos.position;
            ActiveItems.Add(alertItem);
        }

        void RemoveAlert(TDBaseAlertData alert)
        {
            BaseAlertItem tempAlert = ActiveItems.Find((x) => { return x.DataIndex == alert.ID; });
            if (tempAlert == null) return;
            tempAlert.Show(false);
            ActiveItems.Remove(tempAlert);
        }
        void RecreateAlerts()
        {
            foreach (var alert in ActiveItems)
            {
                GOPool.Despawn(alert.gameObject);
            }
            ActiveItems.Clear();
            if (delayCallTask != null)
                BattleCoroutine.Kill(delayCallTask);
            delayCallTask = BattleCoroutine.Run(AddInitAlerts());
        }

        IEnumerator<float> AddInitAlerts()
        {
            if (AlertMgr != null)
            {
                for (int i = 0; i < AlertMgr.Data.Count; ++i)
                {
                    yield return Timing.WaitForSeconds(0.1f);
                    AddAlert(AlertMgr.Data[i]);
                }
            }
            yield break;
        }
        #endregion

        #region Callback
        protected override void OnSetPlayerBase(BaseUnit oldPlayer, BaseUnit newPlayer)
        {
            base.OnSetPlayerBase(oldPlayer, newPlayer);
            if (AlertMgr != null)
            {
                AlertMgr.Callback_OnAdded -= OnAlertAdded;
                AlertMgr.Callback_OnRemoved -= OnAlertRemoved;
                AlertMgr.Callback_OnMerge -= OnAlertMerge;
                AlertMgr.Callback_OnCommingTimeOut -= OnAlertCommingTimeOut;
            }
            AlertMgr = NewAlertMgr;
            AlertMgr.Callback_OnAdded += OnAlertAdded;
            AlertMgr.Callback_OnRemoved += OnAlertRemoved;
            AlertMgr.Callback_OnMerge += OnAlertMerge;
            AlertMgr.Callback_OnCommingTimeOut += OnAlertCommingTimeOut;
            RecreateAlerts();
        }

        private void OnAlertCommingTimeOut(TDBaseAlertData arg1)
        {
            throw new NotImplementedException();
        }

        private void OnAlertMerge(TDBaseAlertData arg1)
        {

        }

        private void OnAlertRemoved(TDBaseAlertData arg1)
        {
            RemoveAlert(arg1);
        }

        private void OnAlertAdded(TDBaseAlertData arg1)
        {
            AddAlert(arg1);
        }

        private void OnAlertShow(BasePresenter p,bool arg1)
        {
            if (arg1 == false)
            {
                GO.transform.position = StartPos.position;
                GOPool.Despawn(p.GO);
            }
        }
        #endregion
    }
}