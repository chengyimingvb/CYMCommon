using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
/// <summary>
/// UI动画
/// 位置变化
/// 大小变化
/// 颜色变化等
/// </summary>
namespace CYM.UI
{
    public class UITween : BaseMono
    {
        #region inspector
        [SerializeField]
        protected Ease Ease= Ease.Linear;
        [SerializeField]
        protected LoopType LoopType= LoopType.Yoyo;
        [SerializeField]
        protected int LoopCount=-1;
        [SerializeField]
        protected float Duration=1.0f;
        [SerializeField]
        protected bool AutoTween=true;
        #endregion

        #region prop
        protected Tween tween;
        protected RectTransform RectTrans;
        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            RectTrans = Trans as RectTransform;
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void Start()
        {
            base.Start();
            if (AutoTween)
            {
                Tween();
            }
        }
        #endregion

        public virtual void Tween()
        {
            
        }
        public virtual void Stop()
        {
            if (tween != null)
                tween.Kill();
        }
    }

}