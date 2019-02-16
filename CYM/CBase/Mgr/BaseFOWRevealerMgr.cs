//------------------------------------------------------------------------------
// BaseFOWRevealerMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using FoW;

namespace CYM
{
    public class BaseFOWRevealerMgr : BaseCoreMgr
    {
        #region prop
        FogOfWarUnit FOWRevealer { get; set; }
        protected BaseUnit BaseLocalPlayer => SelfBaseGlobal.ScreenMgr.BaseLocalPlayer;
        protected BaseFOWMgr BaseFOWMgr => SelfBaseGlobal.FOWMgr;
        public bool IsVisible { get; protected set; } = true;
        public bool IsInFog { get; private set; }
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            BaseFOWMgr.FOWRevealerList.Add(this);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            BaseFOWMgr.FOWRevealerList.Remove(this);
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            FOWRevealer = Mono.SetupMonoBehaviour<FogOfWarUnit>();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        public override void Refresh()
        {
            base.Refresh();
            bool isEnableView = IsEnableView();
            FOWRevealer.team = isEnableView ? 0 : 1;
            FOWRevealer.circleRadius = isEnableView?Radius:0;
            //FOWRevealer.cellBased = false;
            IsInFog = IsInFogRange();
            IsVisible = !IsInFog;
        }
        protected virtual bool IsEnableView()
        {
            return true;
        }
        #endregion

        #region set
        protected virtual float Radius
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion

        #region is
        bool IsInFogRange()
        {
            if (SelfBaseGlobal.FOWMgr.IsInFog(SelfBaseUnit.Pos, 70))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}