//------------------------------------------------------------------------------
// Unit.cs
// Copyright 2018 2018/11/3 
// Created by CYM on 2018/11/3
// Owner: CYM
// 策略游戏的基础对象
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseEntity<TConfig,TDBData,TOwner,TGlobal>: BaseUnit 
        where TConfig: class,new() 
        where TDBData :class, new() 
        where TOwner: BaseUnit
        where TGlobal: BaseGlobal
    {
        #region Callback
        public event Callback<TOwner> Callback_OnSetOwner;
        #endregion

        #region prop
        public TConfig Config { get; set; }
        public TDBData DBData { get; set; }
        public TOwner Owner { get; private set; }
        public TGlobal SelfGlobal { get; private set; }
        #endregion

        #region life
        public override void Awake()
        {
            SelfGlobal = SelfBaseGlobal as TGlobal;
            base.Awake();
        }
        #endregion

        #region set
        /// <summary>
        /// 设置父对象
        /// </summary>
        public virtual void SetOwner(TOwner unit)
        {
            Owner = unit;
            Callback_OnSetOwner?.Invoke(unit);
        }
        #endregion

        #region is
        public virtual bool IsOwner(BaseEntity<TConfig, TDBData, TOwner, TGlobal> other)
        {
            return this==other.Owner;
        }
        public virtual bool IsOOF(BaseEntity<TConfig, TDBData, TOwner, TGlobal> other)
        {
            return IsOwner(other) || IsFriend(other);
        }
        public override bool IsLocalPlayer()
        {
            return ScreenMgr.BaseLocalPlayer == Owner;
        }
        #endregion
    }
}