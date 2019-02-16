//------------------------------------------------------------------------------
// BaseGlobalMgr.cs
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
    public class BaseGlobalMgr<TGlobal> : BaseGFlowMgr where TGlobal:BaseGlobal 
    {
        protected TGlobal SelfGlobal { get; private set; }

        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfGlobal = SelfBaseGlobal as TGlobal;
        }
    }
}