//------------------------------------------------------------------------------
// BaseEntitySpawnMgr.cs
// Copyright 2019 2019/1/17 
// Created by CYM on 2019/1/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseEntitySpawnMgr<TUnit, TConfig, TDBData,TOwner,TGlobal> : BaseSpawnMgr<TUnit> 
        where TUnit : BaseEntity<TConfig, TDBData, TOwner, TGlobal> 
        where TConfig : class, new() 
        where TDBData : class, new()
        where TOwner : BaseUnit
        where TGlobal : BaseGlobal
    {
        #region prop
        protected TGlobal SelfGlobal { get; private set; }
        #endregion

        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfGlobal = SelfBaseGlobal as TGlobal;
        }

        public override void OnSpawned(string id, TUnit unit)
        {
            base.OnSpawned(id, unit);
            unit.Config = GetConfig(id);
            unit.DBData = GetDBData(id);
        }
        protected virtual TConfig GetConfig(string id)
        {
            return default;
        }
        protected virtual TDBData GetDBData(string id)
        {
            return default;
        }

    }
}