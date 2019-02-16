//------------------------------------------------------------------------------
// BaseFOWMgr.cs
// Copyright 2018 2018/11/11 
// Created by CYM on 2018/11/11
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FoW;
using UnityEngine;

namespace CYM
{
    public class BaseFOWMgr : BaseGFlowMgr
    {
        #region prop
        private bool isDirty = false;
        private FogOfWar FOW { get; set; }
        TweenerCore<float, float, FloatOptions> fogAlphaTween;
        public bool IsFogShow { get; private set; } = true;
        public HashList<BaseFOWRevealerMgr> FOWRevealerList { get; private set; } = new HashList<BaseFOWRevealerMgr>();
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedUpdate = true;
            NeedLateUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            FOW = Mono.SetupMonoBehaviour<FogOfWar>();
            EnableFOW(false);
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            EnableFOW(true);
            FOW.SetAll(0);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            EnableFOW(false);
        }
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in FOWRevealerList)
            {
                item.Refresh();
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

        }
        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
            FOW.ManualUpdate();
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
            fogAlphaTween = DOTween.To(() => FOW.fogColor.a, x => FOW.fogColor.a = x, b ? 0.5f : 0.0f, 0.3f);
        }
        /// <summary>
        /// 设置所有
        /// </summary>
        /// <param name="val"></param>
        public void SetAll(byte val)
        {
            FOW.SetAll(val);
        }
        public void EnableFOW(bool b)
        {
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
        public bool IsInFog(Vector3 pos, byte minFog)
        {
            return FOW.GetFogValue(pos) > minFog;
        }
        public bool IsInFog(Vector3 pos)
        {
            return FOW.GetFogValue(pos) > 0;
        }
        #endregion
    }
}