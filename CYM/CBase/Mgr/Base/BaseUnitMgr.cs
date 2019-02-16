//------------------------------------------------------------------------------
// BaseUnitMgr.cs
// Copyright 2019 2019/1/20 
// Created by CYM on 2019/1/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseUnitMgr<TUnit,TGlobal> : BaseCoreMgr 
        where TUnit:BaseUnit
        where TGlobal:BaseGlobal
    {
        #region prop
        public TUnit SelfUnit { get; private set; }
        public TGlobal SelfGlobal { get; private set; }
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfUnit = SelfBaseUnit as TUnit;
            SelfGlobal = SelfBaseGlobal as TGlobal;
        }
        #endregion
    }
}