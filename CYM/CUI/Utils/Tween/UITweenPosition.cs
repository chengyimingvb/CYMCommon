using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

namespace CYM.UI
{
    public class UITweenPosition : UITween
    {
        #region inspector
        [SerializeField]
        protected Vector3 Target;
        #endregion

        public override void Tween()
        {
            if (tween != null)
                tween.Kill();
            tween = DOTween.To(()=> RectTrans.anchoredPosition3D,(x)=> RectTrans.anchoredPosition3D = x,Target,Duration)
                .SetLoops(LoopCount, LoopType)
                .SetEase(Ease);
        }
    }
}
