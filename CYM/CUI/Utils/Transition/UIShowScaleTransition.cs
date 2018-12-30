using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace CYM.UI
{
    public class UIShowScaleTransition : UIShowTransition
    {
        #region prop
        public Vector3 FromScale = Vector3.one * 0.5f;
        public Vector3 ToScale = Vector3.one;
        //public Ease Ease= Ease.OutBounce;
        //public bool State = true;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            //RectTrans.localScale = FromScale;
        }

        public override void OnShow(bool b, bool isActiveByShow)
        {
            base.OnShow(b, isActiveByShow);
            if (RectTrans != null)
            {
                if (IsReset)
                {
                    RectTrans.localScale = b ? FromScale : ToScale;
                }
                tweener =RectTrans.DOScale(b?ToScale:FromScale, Duration).SetEase(GetEase(b)).OnComplete(OnTweenComplete).SetDelay(Delay);
            }
        }
    }

}