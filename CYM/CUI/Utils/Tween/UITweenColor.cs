using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

namespace CYM.UI
{
    public class UITweenColor : UITween
    {
        #region inspector
        [SerializeField]
        protected Color Target;
        #endregion

        #region prop
        protected Graphic Graphic;
        #endregion

        public override void Awake()
        {
            base.Awake();
            Graphic = GetComponent<Graphic>();
        }
        public override void Tween()
        {
            if (tween != null)
                tween.Kill();
            tween = DOTween.To(() => Graphic.color, (x) => Graphic.color = x, Target, Duration)
                .SetLoops(LoopCount, LoopType)
                .SetEase(Ease) ;
        }

    }

}