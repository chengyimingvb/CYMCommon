using System.Collections;
using System.Collections.Generic;
using CYM.Pool;
using UnityEngine;

//**********************************************
// Class Name	: CYMPoolManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BasePoolMgr : BaseGFlowMgr
    {
        #region member variable
        public SpawnPool Common { get;private set;}
        public SpawnPool Unit { get; private set; }
        public SpawnPool Perform { get; private set; }
        public SpawnPool HUD{ get; private set; }
        #endregion

        #region methon
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);

        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        public virtual void CreateBasePool()
        {
            Common = PoolManager.Pools.Create("Common");
            Unit = PoolManager.Pools.Create("Units");
            Perform = PoolManager.Pools.Create("Perform");
            HUD = PoolManager.Pools.Create("HUD");
        }
        public void DestroyBasePool()
        {
            PoolManager.Pools.DestroyAll();
            Common = null;
            Unit = null;
            Perform = null;
            HUD = null;
        }
        public SpawnPool CreatePool(string name)
        {
            return PoolManager.Pools.Create(name);
        }

        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            CreateBasePool();
        }
        protected override void OnBattleUnLoad()
        {
            DestroyBasePool();
        }
        #endregion


    }

}

