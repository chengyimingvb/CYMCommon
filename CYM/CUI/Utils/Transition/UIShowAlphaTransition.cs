using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace CYM.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIShowAlphaTransition : UIShowTransition
    {
        #region prop
        public float From = 0.5f;
        public float To = 1.0f;
        CanvasGroup CanvasGroup;
        #endregion

        //private TweenerCore<float,> colorTween;

        protected override void Awake()
        {
            base.Awake();
            if (RectTrans != null)
            {
                CanvasGroup = RectTrans.GetComponent<CanvasGroup>();
                //if (CanvasGroup != null)
                //    CanvasGroup.alpha = From;
            }
        }

        public override void OnShow(bool b, bool isActiveByShow)
        {
            base.OnShow(b, isActiveByShow);
            if (RectTrans != null)
            {
                if (CanvasGroup == null)
                    CanvasGroup = RectTrans.GetComponent<CanvasGroup>();

                if (IsReset)
                {
                    CanvasGroup.alpha =b?From:To;
                }

                tweener=DOTween.To(() => CanvasGroup.alpha, x => CanvasGroup.alpha = x, b ? To : From, Duration).SetEase(GetEase(b)).OnComplete(OnTweenComplete).SetDelay(Delay);
            }
        }

    }

}