using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
namespace CYM.UI
{
    public class BaseGOView : BaseView
    {
        #region life
        public override void Attach(BaseView parentView, BaseView rootView, Object mono)
        {
            base.Attach(parentView, rootView, mono);
            // 有利于找到UIprefab问题
            if (Trans == null)
            {
                CLog.Error("Trans没有，没有Awake？");
            }
            if (parentView != null)
                Trans.SetParent(parentView.Trans);
            Trans.localPosition = sourceLocalPos;
        }
        public override void Show(bool b=true, float? fadeTime = null, float delay = 0, bool useGroup = true, bool force = false)
        {
            if (IsShow == b && !force)
                return;
            float tempFade = 0.0f;
            if (fadeTime != null)
                tempFade = fadeTime.Value;
            else
                tempFade = b ? InTime : OutTime;
            IsShow = b;

            if (scaleTween != null)
                scaleTween.Kill();
            if (IsScale)
                scaleTween = Trans.DOScale(IsShow ? 1.0f : 0.001f, tempFade).SetEase(IsShow ? InEase : OutEase);
            else
                scaleTween = Trans.DOScale(IsShow ? 1.0f : 0.001f, 0).SetEase(InEase);
            if (IsShow)
            {
                OnOpen(this, useGroup);
                scaleTween.OnStart(OnFadeIn);
                scaleTween.SetDelay(delay);
            }
            else
            {
                OnClose();
                scaleTween.OnComplete(OnFadeOut);
                scaleTween.SetDelay(delay);
            }

            if (IsShow)
            {
                SetDirty();
                //UI互斥,相同UI组只能有一个UI被打开
                if (useGroup && Group > 0)
                {
                    for (int i = 0; i < SubViews.Count; ++i)
                    {
                        if (SubViews[i] != this && SubViews[i].Group == Group && SubViews[i].ViewLevel == ViewLevel)
                            SubViews[i].Show(false, null, 0, false);
                    }
                }
            }
            else
            {
                //关闭界面的时候自动刷新父级界面
                if (ParentView.IsShow && !ParentView.IsRootView)
                {
                    ParentView.SetDirty();
                }
                //关闭子界面
                foreach (var item in SubViews)
                    item.Show(false);
            }

            OnShow();
        }
        #endregion

        #region get
        public Camera GetCamera()
        {
            if(IsRootView)
                return Canvas.worldCamera;
            return RootView.Canvas.worldCamera;
        }
        #endregion

    }

}