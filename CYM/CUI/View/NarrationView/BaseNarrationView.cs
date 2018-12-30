//------------------------------------------------------------------------------
// BaseNarrationView.cs
// Copyright 2018 2018/3/26 
// Created by CYM on 2018/3/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using CYM.Pool;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

namespace CYM
{
    public class BaseNarrationView : BaseUIView
    {
        #region inspector
        [SerializeField]
        BaseRichText Desc;
        [SerializeField]
        CanvasGroup CanvasGroup;
        [SerializeField]
        BaseImage Bg;
        [SerializeField]
        BaseImage Image;
        [SerializeField]
        BaseText KeyTip;
        #endregion

        #region private
        Tween TweenText;
        Tween TweenAlpha;
        BaseRichText CurDesc;
        GOPool DescPool;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Title.CancleInit();
            DescPool= new GOPool(Desc.gameObject, CanvasGroup.transform);
            Bg.Init(new BaseImageData{OnClick = OnClickBg });
            KeyTip.Init(new BaseTextData {Name = GetKeyTip,IsTrans=false });
            Desc.Show(false);
        }
        public override void OnDestroy()
        {
            DescPool.Destroy();
            base.OnDestroy();
        }
        #endregion

        #region set
        public virtual void Show(NarrationFragment fragment)
        {
            Show(true);
            if (TweenText != null)
                TweenText.Complete();
            if (TweenAlpha != null)
                TweenAlpha.Kill();

            if (fragment.IsNewPage)
            {
                DescPool.DespawnAll();
            }

            GameObject tempGO = DescPool.Spawn();
            CurDesc = tempGO.GetComponent<BaseRichText>();
            CurDesc.RichText.text = "";
            CurDesc.Show(true);
            CurDesc.transform.SetAsLastSibling();

            Title.Text.text = fragment.GetName();
            if (fragment.IsNewPage)
            {
                CurDesc.RichText.Content = "";
                CanvasGroup.alpha = 0.0f;
                TweenAlpha = DOTween.To(() => CanvasGroup.alpha, (x) => CanvasGroup.alpha = x, 1.0f, 0.3f);
                TweenText = DOTween.To(() => CurDesc.RichText.Content, (x) => CurDesc.RichText.Content = x, fragment.GetDesc(), 1.0f).SetDelay(0.5f).OnComplete(OnTypeEnd);
            }
            else
            {
                string temp = fragment.GetDesc();
                TweenText = DOTween.To(() => CurDesc.RichText.Content, (x) => CurDesc.RichText.Content = x, temp, 1.0f).OnComplete(OnTypeEnd);
            }
            var tempSprite = fragment.GetIcon();
            if (tempSprite)
            {
                Image.Image.sprite = tempSprite;
                Image.Image.CrossFadeAlpha(0.0f, 0.0f, true);
                Image.Image.CrossFadeAlpha(1.0f, 0.5f, true);
            }

            Title.Show((fragment.CurPage==0));
            Image.Show(true);
        }
        #endregion

        #region Callback
        protected virtual void OnClickBg(BasePresenter presenter, PointerEventData arg3)
        {
            
        }
        protected virtual string GetKeyTip()
        {
            return "Enter";
        }

        protected void OnTypeEnd()
        {
            CurDesc.RichText.RefreshRichText();
        }
        #endregion
    }
}