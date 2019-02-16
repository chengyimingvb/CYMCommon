//------------------------------------------------------------------------------
// BaseVirtualMgr.cs
// Copyright 2018 2018/11/26 
// Created by CYM on 2018/11/26
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
    public class BaseVirtualMgr<T> : BaseGFlowMgr, ISpawnMgr<T> where T : BaseVirtual,new()
    {
        #region ISpawnMgr
        public T Gold { get; set; }
        public DicList<T> Data { get; set; } = new DicList<T>();
        public event Callback<T> Callback_OnAdd;
        public event Callback<T> Callback_OnSpawnGold;
        public event Callback<T> Callback_OnSpawn;
        public event Callback<T> Callback_OnDespawn;
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
        protected override void OnBattleUnLoaded()
        {
            Clear();
        }
        #endregion

        #region set
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Add(T chara)
        {
            Data.Add(chara);
            Callback_OnAdd?.Invoke(chara);
        }
        public virtual T Spawn(string id, params object[] ps)
        {
            if (id.IsInvStr())
                return null;
            T prefab = GetData(id, ps);
            OnSpawned(id, prefab);
            Data.Add(prefab);
            Callback_OnSpawn?.Invoke(prefab);
            return prefab;
        }
        /// <summary>
        /// despawn
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Despawn(T chara, float delay = 0.0f)
        {
            Data.Remove(chara);
            Callback_OnDespawn?.Invoke(chara);
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public virtual void Clear()
        {
            Data.Clear();
        }
        #endregion

        #region virtual
        protected virtual T GetData(string id, params object[] ps)
        {
            return new T();
        }
        public virtual void OnSpawned(string id, T unit)
        {
        }
        #endregion
    }
}