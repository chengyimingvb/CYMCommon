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
	public class BaseSpawnMgr<TUnit> : BaseGFlowMgr , ISpawnMgr<TUnit> where TUnit:BaseUnit  
	{
        #region prop
        Dictionary<string, TUnit> dynamicDic = new Dictionary<string, TUnit>();
        GameObject TempSpawnTrans = new GameObject("TempSpawnTrans");
        #endregion

        #region ISpawnMgr
        public TUnit Gold { get; set; }
        public DicList<TUnit> Data { get; set; } = new DicList<TUnit>();
        public event Callback<TUnit> Callback_OnAdd;
        public event Callback<TUnit> Callback_OnSpawnGold;
        public event Callback<TUnit> Callback_OnSpawn;
        public event Callback<TUnit> Callback_OnDespawn;
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
        public TUnit Spawn(string id, Transform spwanPoint, int team = 0, params object[] ps)
        {
            return Spawn(id, spwanPoint.position, spwanPoint.rotation,team,ps);
        }
        public virtual TUnit SpawGold()
        {
            TempSpawnTrans.transform.position = BaseConstMgr.FarawayPos;
            Gold = Spawn(GoldID, TempSpawnTrans.transform, int.MaxValue);
            Callback_OnSpawnGold?.Invoke(Gold);
            return Gold;
        }
        /// <summary>
        /// 执行Add操作,但是也会触发Spawn流程,适用于对已经存在的对象使用
        /// </summary>
        public virtual void SpawnAdd(TUnit chara,string id,int team=0)
        {
            OnSpawned(id, chara);
            chara.OnSpawnInit(id, team);
            Add(chara);
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Add(TUnit chara)
        {
            Data.Add(chara);
            Callback_OnAdd?.Invoke(chara);
        }
        /// <summary>
        /// despawn
        /// </summary>
        /// <param name="chara"></param>
        public virtual void Despawn(TUnit chara, float delay = 0.0f)
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
        public virtual TUnit Spawn(string id, Vector3 spwanPoint, Quaternion? quaternion = null, int team = 0, params object[] ps)
        {
            if (id.IsInvStr())
                return null;
            GameObject prefab = GetPrefab(id, ps);
            GameObject charaGO = Pool().Spawn(prefab, spwanPoint, quaternion);
            TUnit unitChara = BaseCoreMono.GetUnityComponet<TUnit>(charaGO);
            OnSpawned(id, unitChara);
            unitChara.OnSpawnInit(id, team);
            Data.Add(unitChara);
            Callback_OnSpawn?.Invoke(unitChara);
            return unitChara;
        }
        public virtual void AddDynamic(string key, TUnit unit)
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
        public void AddDynamic(TUnit unit)
        {
            AddDynamic(unit.GetTDID(), unit);
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
        /// <summary>
        /// 添加动态对象
        /// </summary>
        protected virtual void OnAddDynamicDic()
        {

        }
        public virtual void OnSpawned(string id, TUnit unit)
        {
        }

        #endregion

        #region get
        /// <summary>
        /// 传入的key需要加上$符号
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual TUnit GetDynamic(string key)
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
        public virtual TUnit GetDynamicSafe(string key)
        {
            if (!key.StartsWith(BaseConstMgr.Prefix_DynamicTrans))
                key = BaseConstMgr.Prefix_DynamicTrans + key;
            return GetDynamic(key);
        }
        public TUnit GetUnit(string id)
        {
            return Data.Get(id);
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