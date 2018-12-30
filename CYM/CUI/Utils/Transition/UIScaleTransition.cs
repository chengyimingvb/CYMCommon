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
    public class UIScaleTransition : UITransition
    {
        #region Inspector
        public Vector3 Normal = Vector3.one;
        public Vector3 Enter = Vector3.one * 1.2f;
        public Vector3 Press = Vector3.one;
        public Vector3 Disable = Vector3.one;
        public Vector3 Selected = Vector3.one * 1.2f;
        #endregion

        private TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions> vectorTween;


        #region LIFE
        protected override void Awake()
        {
            base.Awake();
            if (RectTrans != null)
            {
                RectTrans.localScale = Normal;
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
                vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Enter, Duration).SetDelay(Delay);
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
                vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Normal, Duration).SetDelay(Delay);
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
                vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Press, Duration).SetDelay(Delay);
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
                vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Normal, Duration).SetDelay(Delay);
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
                    vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Normal, Duration).SetDelay(Delay);
                }
                else
                {
                    vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Disable, Duration).SetDelay(Delay);
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
                    vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Selected, Duration).SetDelay(Delay);
                }
                else
                {
                    vectorTween = DOTween.To(() => RectTrans.localScale, x => RectTrans.localScale = x, Normal, Duration).SetDelay(Delay);
                }
            }
        }
        #endregion
    }
}