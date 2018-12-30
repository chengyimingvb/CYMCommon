//------------------------------------------------------------------------------
// UIScaleTransition.cs
// Copyright 2018 2018/6/12 
// Created by CYM on 2018/6/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.AI;
using DG.Tweening.Core;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace CYM.UI
{
    public class UIPosTransition : UITransition
    {
        #region Inspector
        public Vector2 Normal = Vector2.zero;
        public Vector2 Enter = Vector2.one;
        public Vector2 Press = Vector2.zero;
        public Vector2 Disable = Vector2.one;
        public Vector2 Selected = Vector2.one;
        #endregion

        #region prop
        private TweenerCore<Vector2, Vector2, DG.Tweening.Plugins.Options.VectorOptions> vectorTween;
        #endregion

        #region LIFE
        protected override void Awake()
        {
            base.Awake();
            if (RectTrans != null)
            {
                RectTrans.anchoredPosition = Normal;
            }
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (RectTrans == null) return;
            if (vectorTween != null)
                vectorTween.Kill();
            if (RectTrans != null)
            {
                vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Enter, Duration).SetDelay(Delay);
            }

        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (RectTrans == null) return;
            if (vectorTween != null)
                vectorTween.Kill();
            if (RectTrans != null)
            {
                vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Normal, Duration).SetDelay(Delay);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (RectTrans == null) return;
            if (vectorTween != null)
                vectorTween.Kill();
            if (RectTrans != null)
            {
                vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Press, Duration).SetDelay(Delay);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (RectTrans == null) return;
            if (vectorTween != null)
                vectorTween.Kill();
            if (RectTrans != null)
            {
                vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Normal, Duration).SetDelay(Delay);
            }
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (RectTrans == null) return;
            if (vectorTween != null)
                vectorTween.Kill();
            if (RectTrans != null)
            {
                if (b)
                {
                    vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Normal, Duration).SetDelay(Delay);
                }
                else
                {
                    vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Disable, Duration).SetDelay(Delay);
                }
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (vectorTween != null)
                vectorTween.Kill();
            if (RectTrans != null)
            {

                if (b)
                {
                    vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Selected, Duration).SetDelay(Delay);
                }
                else
                {
                    vectorTween = DOTween.To(() => RectTrans.anchoredPosition, x => RectTrans.anchoredPosition = x, Normal, Duration).SetDelay(Delay);
                }
            }
        }
        #endregion
    }

}