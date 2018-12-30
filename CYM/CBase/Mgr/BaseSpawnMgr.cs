//**********************************************
// Class Name	: BaseSpawnMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM.Pool;
using System;
namespace CYM
{
	public class BaseSpawnMgr<T> : BaseGlobalCoreMgr , ISpawnMgr<T> where T:BaseUnit
	{
        #region prop
        Dictionary<string, T> dynamicDic = new Dictionary<string, T>();
        GameObject TempSpawnTrans = new GameObject("TempSpawnTrans");
        #endregion

        #region ISpawnMgr
        public T Gold { get; set; }
        public List<T> Data { get; set; } = new List<T>();
        public event Callback<T> Callback_OnAdd;
        public event Callback<T> Callback_OnSpawnGold;
        public event Callback<T> Callback_OnSpawn;
        public event Callback<T> Callback_OnDespawn;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            TempSpawnTrans.hideFlags = HideFlags.HideInHierarchy;
        }
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
        #endregion

        #region set
        /// <summary>
        /// 产生Player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Spawn(string id, Transform spwanPoint, int team = 0, params object[] ps)
        {
            return Spawn(id, spwanPoint.position, spwanPoint.rotation,team,ps);
        }
        public virtual T SpawGold(Vector3 spwanPoint)
        {
            TempSpawnTrans.transform.position = spwanPoint;
            Gold = Spawn(GoldID, TempSpawnTrans.transform, int.MaxValue);
            Callback_OnSpawnGold?.Invoke(Gold);
            return Gold;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Add(T chara)
        {
            Data.Add(chara);
            Callback_OnAdd?.Invoke(chara);
        }
        /// <summary>
        /// despawn
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Despawn(T chara, float delay = 0.0f)
        {
            Pool().Despawn(chara, delay);
            Data.Remove(chara);
            Callback_OnDespawn?.Invoke(chara);
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public virtual void Clear()
        {
            Data.Clear();
            dynamicDic.Clear();
        }
        #endregion

        #region virtual
        protected virtual string GoldID => "";
        protected virtual SpawnPool Pool()
        {
            throw new NotImplementedException("此函数必须被实现");
        }
        protected virtual GameObject GetPrefab(string id, params object[] ps)
        {
            throw new System.NotImplementedException("此函数必须被实现");
        }

        public virtual T Spawn(string id, Vector3 spwanPoint, Quaternion? quaternion = null, int team = 0, params object[] ps)
        {
            if (id.IsInvStr())
                return null ;
            GameObject prefab = GetPrefab(id, ps);
            GameObject charaGO = Pool().Spawn(prefab, spwanPoint, quaternion);
            T unitChara = BaseCoreMono.GetUnityComponet<T>(charaGO);
            OnSpawned(id, unitChara);
            unitChara.AutoInit = false;
            unitChara.OnSpawnInit(id,team);            
            Data.Add(unitChara);
            Callback_OnSpawn?.Invoke(unitChara);
            return unitChara;
        }
        public virtual void OnSpawned(string id,T unit)
        {
        }
        /// <summary>
        /// 添加动态对象
        /// </summary>
        protected virtual void OnAddDynamicDic()
        {

        }
        public virtual void AddDynamic(string key,T unit)
        {
            string key1 = BaseConstMgr.Prefix_DynamicTrans + key;
            if (unit == null)
                return;
            if (dynamicDic.ContainsKey(key1))
            {
                dynamicDic[key1] = unit;
                return;
            }
            dynamicDic.Add(key1, unit);
        }
        public void AddDynamic(T unit)
        {
            AddDynamic(unit.GetTDID(),unit);
        }
        /// <summary>
        /// 传入的key需要加上$符号
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T GetDynamic(string key)
        {
            if (!dynamicDic.ContainsKey(key))
            {
                return null;
            }
            var chara = dynamicDic[key];
            if (!chara.IsLive)
                return null;
            return chara;
        }
        /// <summary>
        /// 忽略$符号,强制获取动态对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T GetDynamicSafe(string key)
        {
            if (!key.StartsWith(BaseConstMgr.Prefix_DynamicTrans))
                key = BaseConstMgr.Prefix_DynamicTrans + key;
            return GetDynamic(key);
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoaded()
        {
            Clear();
        }
        protected override void OnGameStart()
        {
            base.OnGameStart();
            OnAddDynamicDic();
        }
        #endregion

        #region DB
        public override void Read1<TDBData>(TDBData data)
        {
            base.Read1(data);
            foreach (var item in Data)
                item.Read1(data);
        }

        public override void Read2<TDBData>(TDBData data) 
        {
            base.Read2(data);
            foreach (var item in Data)
                item.Read2(data);
        }

        public override void Read3<TDBData>(TDBData data)
        {
            base.Read3(data);
            foreach (var item in Data)
                item.Read3(data);
        }
        public override void ReadEnd<TDBData>(TDBData data)
        {
            base.ReadEnd(data);
            foreach (var item in Data)
                item.ReadEnd(data);
        }

        public override void Write<TDBData>(TDBData data)
        {
            base.Write(data);
            foreach (var item in Data)
                item.Write(data);
        }
        #endregion
    }
}