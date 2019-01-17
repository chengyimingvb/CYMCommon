//**********************************************
// Class Name	: HelpItem
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;
using CYM.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseTalkItemData : PresenterData
    {
        public BaseImageData Bg = new BaseImageData();
        public BaseTextData KeyTip = new BaseTextData();
        public BaseTextData SelectTip = new BaseTextData();
        public BaseButtonData Option = new BaseButtonData();

        public Func<int> CurSelectOptionIndex=() => 0;
    }
    public class BaseTalkItem : Presenter<BaseTalkItemData>
    {
        #region inspector
        [SerializeField]
        protected BaseImage Icon;
        [SerializeField]
        protected BaseText Name;
        [SerializeField]
        protected BaseRichText Desc;
        [SerializeField]
        protected BaseImage Bg;
        [SerializeField]
        protected BaseImage Next;
        [SerializeField]
        protected BaseText KeyTip;
        [SerializeField]
        protected BaseDupplicate DP_Select;
        [SerializeField]
        protected GameObject SelectTipObj;
        [SerializeField]
        protected BaseText SelectTip;
        [SerializeField]
        protected LayoutElement TextLayoutElement;
        #endregion

        #region prop
        Tween Tween;
        TDBaseTalkData CurTalkData;
        TalkFragment CurTalkFragment;
        AudioSource PreAudioSource;
        public bool IsTypeEnd { get; set; } = false;
        #endregion

        public override void Init(BaseTalkItemData data)
        {
            base.Init(data);
            Bg.Init(data.Bg);
            KeyTip.Init(data.KeyTip);
            SelectTip.Init(data.SelectTip);
            DP_Select.Init<BaseButton, BaseButtonData>(BaseConstMgr.MaxTalkOptionCount, (p, d) =>
            {
                var pressenter = p as BaseButton;
                if (CurTalkData.Option.Count > pressenter.Index)
                {
                    pressenter.Show(true);
                    pressenter.Text.text = BaseLanguageMgr.Get(CurTalkData.Option[pressenter.Index]);
                }
                else
                {
                    pressenter.Show(false);
                }

                if (data.CurSelectOptionIndex() == pressenter.Index)
                    pressenter.SetSelected(true);
                else
                    pressenter.SetSelected(false);
            },null,
            data.Option);
        }

        public override void Refresh()
        {
            base.Refresh();
        }

        #region set
        public void Show(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            CurTalkData = talkData;
            CurTalkFragment = talkFragment;
            Show(true,true);

            if (Icon != null) Icon.Image.overrideSprite = GetIcon(talkData,talkFragment);
            if (Name != null) Name.text = GetName(talkData, talkFragment);
            if (PreAudioSource != null)
            {
                PreAudioSource.Stop();
            }
            PreAudioSource= PlayClip(GetAudio(talkData, talkFragment));

            Desc.text = "";
            Desc.IsAnimation = false;
            if (Tween != null)
                Tween.Kill();
            Tween = DOTween.To(() => Desc.RichText.Content, (x) => Desc.RichText.Content = x, talkFragment.GetDesc(), 0.5f).SetDelay(0.5f).OnComplete(OnTypeEnd).OnStart(OnTweenStart);

            bool isHaveOpt = talkData.IsHaveOption() && talkFragment.IsLasted;
            DP_Select.Show(isHaveOpt);
            SelectTipObj.SetActive(isHaveOpt);
            if (isHaveOpt)
            {
                TextLayoutElement.minHeight = 50.0f;
            }
            else
            {
                TextLayoutElement.minHeight = 100.0f;
            }
        }
        public override void Show(bool b, bool isForce = false)
        {
            IsTypeEnd = !b;
            base.Show(b, isForce);
        }
        #endregion

        #region must override
        protected virtual Sprite GetIcon(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            return talkFragment.GetIcon();
        }
        protected virtual string GetName(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            return talkFragment.GetName();
        }
        protected virtual string GetAudio(TDBaseTalkData talkData, TalkFragment talkFragment)
        {
            return talkFragment.Audio;
        }
        #endregion

        #region Callback
        void OnTweenStart()
        {
            
        }
        void OnTypeEnd()
        {
            IsTypeEnd = true;
            Desc.RichText.RefreshRichText();
        }
        #endregion
    }
}
