//------------------------------------------------------------------------------
// BaseFOWMgr.cs
// Copyright 2018 2018/11/11 
// Created by CYM on 2018/11/11
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
    public class BaseFOWMgr : BaseGlobalCoreMgr
    {
        #region prop
        private bool isDirty = false;
        private FogOfWar FOW { get; set; }
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            FOW = Mono.GetComponentInChildren<FogOfWar>();
            EnableFOW(false);
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            EnableFOW(true);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            EnableFOW(false);
        }
        #endregion

        #region set
        /// <summary>
        /// 更新谜雾图
        /// </summary>
        public void UpdateTexture()
        {
            if(FOW!=null)
                FOW.ManualUpdate();
        }
        /// <summary>
        /// 设置所有
        /// </summary>
        /// <param name="val"></param>
        public void SetAll(byte val)
        {
            if (FOW != null)
                FOW.SetAll(val);
        }
        public void EnableFOW(bool b)
        {
            if (FOW != null)
                FOW.enabled = b;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否在迷雾内
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="minFog"></param>
        /// <returns></returns>
        public bool IsInFog(Vector3 pos,byte minFog)
        {
            if (FOW != null)
                return FOW.GetFogValue(pos)> minFog;
            return false;
        }
        #endregion
    }
}