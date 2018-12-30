using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace CYM.UI
{
    public class UIShowPositionTransition : UIShowTransition
    {
        #region prop
        public Vector2 From = Vector2.one * 0.5f;
        public Vector2 To = Vector2.one;
        #endregion

        public override void OnShow(bool b, bool isActiveByShow)
        {
            base.OnShow(b, isActiveByShow);
            if (RectTrans != null)
            {
                if (IsReset)
                {
                    RectTrans.anchoredPosition = b ? From : To;
                }

                tweener=DOTween.To(() => RectTrans.anchoredPosition, (x) => RectTrans.anchoredPosition = x,b?To:From, Duration).SetEase(GetEase(b)).OnComplete(OnTweenComplete).SetDelay(Delay);
            }
        }

    }

}