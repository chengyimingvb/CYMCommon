using UnityEngine;
using System.Collections.Generic;
//**********************************************
// Class Name	: CYMBaseScreenController
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class BaseScreenMgr<TUnit, TData> : BaseGlobalCoreMgr where TUnit : BaseUnit where TData : TDValue
    {
        #region Callback val
        /// <summary>
        /// 设置玩家的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public event Callback<TUnit, TUnit> Callback_OnSetPlayer;
        /// <summary>
        /// 本地玩家死亡
        /// </summary>
        public event Callback Callback_OnPlayerRealDeath;
        /// <summary>
        /// 可选择的对象
        /// </summary>
        public List<TData> SelectItems = new List<TData>();
        #endregion

        #region property
        /// <summary>
        /// 选择的ID
        /// </summary>
        public string Selected { get; protected set; }
        /// <summary>
        /// 当前的玩家
        /// </summary>
        public TUnit LocalPlayer { get; private set; }
        /// <summary>
        /// 老玩家
        /// </summary>
        public TUnit PrePlayer { get; private set; }
        public TData SelectData { get; private set; }
        #endregion

        #region methon
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        #endregion

        #region set
        /// <summary>
        /// 选择id
        /// </summary>
        /// <param name="id"></param>
        public virtual void Select(TData data)
        {
            SelectData = data;
            Selected = SelectData.TDID;
        }
        /// <summary>
        /// 选择id 通过index
        /// </summary>
        /// <param name="id"></param>
        public void Select(int index)
        {
            SelectData = SelectItems[index];
            Selected = SelectData.TDID;
        }
        /// <summary>
        /// 设置玩家
        /// </summary>
        /// <param name="unit"></param>
        public void SetPlayer(BaseUnit unit)
        {
            PrePlayer = LocalPlayer;
            LocalPlayer = unit as TUnit;
            Callback_OnSetPlayer?.Invoke(PrePlayer, LocalPlayer);

            if (PrePlayer != null)
                PrePlayer.Callback_OnRealDeath -= OnPlayerRealDeath;
            if (LocalPlayer != null)
                LocalPlayer.Callback_OnRealDeath += OnPlayerRealDeath;
        }
        /// <summary>
        /// 随机选择
        /// </summary>
        public void RandSelect()
        {
            Select(BaseMathUtils.RandArray(SelectItems));
        }
        #endregion

        #region Callback
        protected virtual void OnPlayerRealDeath()
        {
            Callback_OnPlayerRealDeath?.Invoke();
        }
        protected override void OnAllLoadEnd()
        {
            LoadSelectItems(ref SelectItems);
        }
        #endregion

        #region must override 
        protected virtual void LoadSelectItems(ref List<TData> items)
        {
            //foreach (var item in SelfGlobal.ConfigData.SelectCharas)
            //{
            //    var tempCharaData = SelfGlobal.DPMgr.TDChara.Find(item);
            //    if (tempCharaData != null)
            //        SelectItems.Add(tempCharaData);
            //}
            throw new System.NotImplementedException("此函数必须被实现");
        }
        #endregion
    }
}
