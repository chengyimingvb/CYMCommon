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
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;

namespace CYM
{
    public class BaseFOWMgr : BaseGlobalCoreMgr
    {
        #region prop
        private bool isDirty = false;
        private FogOfWar FOW { get; set; }
        TweenerCore<float, float, FloatOptions> fogAlphaTween;
        public bool IsFogShow { get; private set; } = true;
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
        public void Show(bool b)
        {
            if (IsFogShow == b)
                return;
            IsFogShow = b;
            if (fogAlphaTween != null)
                fogAlphaTween.Kill();
            //tween地图颜色
            fogAlphaTween = DOTween.To(() => FOW.fogColor.a, x => FOW.fogColor.a = x, b ? 0.4f : 0.0f, 0.3f);
        }
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
            {
                if (FOW.enabled == b)
                    return;
                FOW.enabled = b;
            }
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