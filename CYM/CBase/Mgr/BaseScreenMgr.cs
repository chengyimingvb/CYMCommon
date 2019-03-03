using UnityEngine;
using System.Collections.Generic;
using System;
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
    public class BaseScreenMgr<TUnit, TData> : BaseGFlowMgr, IBaseScreenMgr where TUnit : BaseUnit where TData : TDValue
    {
        #region Callback val
        /// <summary>
        /// 设置玩家的时候
        /// T1:oldPlayer
        /// T2:newPlayer
        /// </summary>
        public event Callback<TUnit, TUnit> Callback_OnSetPlayer;
        public event Callback<BaseUnit, BaseUnit> Callback_OnSetPlayerBase;
        /// <summary>
        /// 本地玩家死亡
        /// </summary>
        public event Callback Callback_OnPlayerRealDeath;
        #endregion

        #region property
        /// <summary>
        /// 可选择的对象
        /// </summary>
        public List<TData> SelectItems = new List<TData>();
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
        public BaseUnit BaseLocalPlayer { get; set; }
        public BaseUnit BasePrePlayer { get; set; }
        public BaseUnit SelectedUnit { get; protected set; }
        protected virtual LayerMask SelectUnitLayerMask { get; }
        protected Collider LastHitCollider { get; set; }
        protected Vector3 LastMouseDownPos;
        #endregion

        #region methon
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateMouseControl();
        }
        protected void UpdateMouseControl()
        {
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    OnTouchDown(Input.mousePosition, i);
                }
                else if (Input.GetMouseButton(i))
                {
                    OnTouchMove(Input.mousePosition, i);
                }
                else if (Input.GetMouseButtonUp(i))
                {
                    OnTouchUp(Input.mousePosition, i);
                }
            }
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
        public void SetPlayer(TUnit unit)
        {
            PrePlayer = LocalPlayer;
            BasePrePlayer = LocalPlayer;
            LocalPlayer = unit as TUnit;
            BaseLocalPlayer = unit;
            Callback_OnSetPlayer?.Invoke(PrePlayer, LocalPlayer);
            Callback_OnSetPlayerBase?.Invoke(PrePlayer, LocalPlayer);

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
        public void SelectUnit(BaseUnit unit)
        {
            SelectedUnit?.OnUnBeSelected();
            if (unit)
            {
                unit?.OnBeSelected();
                SelectedUnit = unit;
            }
            else
            {
                SelectedUnit = null;
            }
        }
        public virtual void RightClickUnit(BaseUnit unit)
        {

        }

        public virtual void LeftClickUnit(BaseUnit unit)
        {
        }
        #endregion

        #region get
        public bool MouseRayCast(out RaycastHit hit, LayerMask layer)
        {
            return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 999, layer, QueryTriggerInteraction.Collide);
        }
        public Vector3 GetMouseHitPoint()
        {
            RaycastHit hit;
            MouseRayCast(out hit, (LayerMask)BaseConstMgr.Layer_Terrain);
            return hit.point;
        }
        protected virtual void LoadSelectItems(ref List<TData> items)
        {
            throw new System.NotImplementedException("此函数必须被实现");
        }
        #endregion

        #region is
        /// <summary>
        /// 鼠标按下的位置是否和弹起的时候处于同一个位置
        /// </summary>
        /// <returns></returns>
        public bool IsSameMousePtClick()
        {
            return LastMouseDownPos == Input.mousePosition;
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
        protected virtual void OnTouchMove(Vector3 mousePosition, int i)
        {

        }

        protected virtual void OnTouchDown(Vector3 mousePosition, int i)
        {
            LastMouseDownPos = Input.mousePosition;
            RaycastHit hit;
            MouseRayCast(out hit, SelectUnitLayerMask);
            {
                if (BaseUIUtils.CheckGuiObjects())
                    return;
                LastHitCollider = hit.collider;
                if (i == 1)//右键
                {
                    if (LastHitCollider != null)
                    {
                        BaseUnit tempUnit = LastHitCollider.GetComponent<BaseUnit>();
                        if (tempUnit != null)
                        {
                            RightClickUnit(tempUnit);             
                        }
                    }
                }
                else if (i == 0)//左键
                {
                    if (LastHitCollider != null)
                    {
                        BaseUnit tempUnit = LastHitCollider.GetComponent<BaseUnit>();
                        if (tempUnit != null)
                        {
                            SelectUnit(tempUnit);
                            LeftClickUnit(tempUnit);
                        }
                    }
                }
            }
        }
        protected virtual void OnTouchUp(Vector3 mousePosition, int i)
        {
            if (i == 1)
            {

            }
            else if (i == 0)
            {
                if (!BaseUIUtils.CheckGuiObjects() && LastHitCollider == null && IsSameMousePtClick())
                {
                    SelectUnit(null);
                }
            }
        }
        #endregion
    }
}
