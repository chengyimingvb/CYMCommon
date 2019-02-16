//------------------------------------------------------------------------------
// BaseVolumetricFogMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using VolumetricFogAndMist;

namespace CYM
{
    public class BaseVolumetricFogMgr : BaseGFlowMgr
    {
        #region prop
        VolumetricFog VolumetricFog;
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            VolumetricFog = Mono.GetComponentInChildren<VolumetricFog>();
            EnableFog(false);
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            EnableFog(true);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            EnableFog(false);
        }
        #endregion

        #region set
        public void EnableFog(bool b)
        {
            if (VolumetricFog != null)
                VolumetricFog.enabled = b;
        }
        #endregion
    }
}