using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

namespace CYM.UI
{
    public class UITweenScale : UITween
    {
        #region inspector
        [SerializeField]
        protected Vector3 Target;
        #endregion

        public override void Tween()
        {
            if (tween != null)
                tween.Kill();
            tween = DOTween.To(() => Trans.localScale, (x) => Trans.localScale = x, Target, Duration)
                .SetLoops(LoopCount, LoopType)
                .SetEase(Ease);
        }

    }
}
