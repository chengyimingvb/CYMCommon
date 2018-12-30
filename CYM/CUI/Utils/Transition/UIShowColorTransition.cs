using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace CYM.UI
{
    public class UIShowColorTransition : UIShowTransition
    {
        #region prop
        public Color From = Color.gray;
        public Color To = Color.white;
        #endregion

        protected override void Awake()
        {
            base.Awake();
        }

        public override void OnShow(bool b, bool isActiveByShow)
        {
            base.OnShow(b, isActiveByShow);
            if (RectTrans != null)
            {
                if (IsReset)
                {
                    Graphic.color = b ? From : To;
                }

                tweener = DOTween.To(() => Graphic.color, x => Graphic.color = x, b ? To : From, Duration).SetEase(GetEase(b)).OnComplete(OnTweenComplete).SetDelay(Delay);
            }
        }


    }

}