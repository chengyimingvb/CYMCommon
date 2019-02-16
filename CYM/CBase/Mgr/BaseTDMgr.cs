//------------------------------------------------------------------------------
// BaseTDMgr.cs
// Copyright 2018 2018/11/27 
// Created by CYM on 2018/11/27
// Owner: CYM
// TableData 数据管理器
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;

namespace CYM
{
    public class BaseTDMgr<TData> : BaseCoreMgr, ITableDataMgr<TData>, ISpawnMgr<TData> where TData : BaseConfig<TData>, new()
    {

        #region ISpawnMgr
        public TData Gold { get; set; }
        public DicList<TData> Data { get; set; } = new DicList<TData>();
        public event Callback<TData> Callback_OnAdd;
        public event Callback<TData> Callback_OnSpawnGold;
        public event Callback<TData> Callback_OnSpawn;
        public event Callback<TData> Callback_OnDespawn;
        #endregion

        #region life
        public override void GameLogicTurn()
        {
            base.GameLogicTurn();
            foreach (var item in Data)
            {
                item.GameLogicTurn();
            }
        }
        public override void GameFrameTurn(int gameFramesPerSecond)
        {
            base.GameFrameTurn(gameFramesPerSecond);
            foreach (var item in Data)
            {
                item.GameFrameTurn(gameFramesPerSecond);
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in Data)
            {
                item.OnUpdate();
            }
        }
        #endregion

        #region set
        public virtual TData Spawn(string id, params object[] ps)
        {
            if (id.IsInvStr())
                return null;
            TData prefab = Table.Find(id).Copy();
            OnSpawned(id, prefab);
            Data.Add(prefab);
            prefab.OnBeAdded(Mono);
            Callback_OnSpawn?.Invoke(prefab);
            return prefab;
        }
        /// <summary>
        /// despawn
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Despawn(TData data, float delay = 0.0f)
        {
            data.OnBeRemoved();
            Data.Remove(data);
            Callback_OnDespawn?.Invoke(data);
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public virtual void Clear()
        {
            Data.Clear();
        }
        public virtual void OnSpawned(string id, TData unit)
        {
        }
        #endregion

        #region must override
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public TData CurData{get;set;}
        #endregion
    }
}