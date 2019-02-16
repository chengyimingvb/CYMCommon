//------------------------------------------------------------------------------
// BaseLogoMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace CYM
{
    public class BaseLogoMgr : BaseGFlowMgr, ILoader
    {
        Tweener tweener;
        UIConfig LogoConfig => UIConfig.Ins;
        List<LogoData> Logos => UIConfig.Ins.Logos;
        Image LogoImage => BaseLogoPlayer.Logo;

        GameObject BaseLogoViewGO=null;
        BaseLogoPlayer BaseLogoPlayer;
        CanvasGroup CanvasGroup;

        #region life
        protected override void OnStartLoad()
        {
            base.OnStartLoad();
            if (BaseLogoViewGO != null)
                return;
            BaseLogoViewGO = SelfBaseGlobal.GRMgr.GetResources("BaseLogoPlayer", true);
            BaseLogoPlayer = BaseLogoViewGO.GetComponentInChildren<BaseLogoPlayer>();
            CanvasGroup = BaseLogoViewGO.GetComponent<CanvasGroup>();
        }
        protected override void OnAllLoadEnd()
        {
            base.OnAllLoadEnd();
            DOTween.To(()=>CanvasGroup.alpha,x=> CanvasGroup.alpha=x,0.0f,1.0f).OnComplete(OnTweenEnd);
        }

        private void OnTweenEnd()
        {
            GameObject.Destroy(BaseLogoViewGO);
            BaseLogoViewGO = null;
        }
        #endregion

        #region loader
        public string GetLoadInfo()
        {
            return "Show Logo";
        }

        public IEnumerator Load()
        {
            if (LogoConfig.IsEditorMode())
            {
                if (Logos == null || Logos.Count == 0)
                    yield break;
                yield return new WaitForSeconds(0.1f);
                for (int i = 0; i < Logos.Count; ++i)
                {
                    if (tweener != null)
                        tweener.Kill();
                    LogoImage.color = new Color(1, 1, 1, 0);
                    tweener = DOTween.ToAlpha(() => LogoImage.color, x => LogoImage.color = x, 1.0f, Logos[i].InTime);
                    LogoImage.sprite = Logos[i].Logo;
                    LogoImage.SetNativeSize();
                    yield return new WaitForSeconds(Logos[i].WaitTime);
                    if (tweener != null)
                        tweener.Kill();
                    tweener = DOTween.ToAlpha(() => LogoImage.color, x => LogoImage.color = x, 0.0f, Logos[i].OutTime);
                    if (i < Logos.Count - 1)
                        yield return new WaitForSeconds(Logos[i].OutTime);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        #endregion
    }
}