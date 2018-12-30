using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace CYM.UI
{
    [ExecuteInEditMode]
    public class UIAnimTransition : UITransition
    {
        #region inspector
        public string Normal = "Normal";
        public string Pressed = "Pressed";
        #endregion

        #region prop
        Animator animator;
        #endregion

        #region LIFE
        protected override void Awake()
        {
            base.Awake();
            if (RectTrans == null) return;
            animator = RectTrans.GetComponent<Animator>();
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (animator == null) return;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (animator == null) return;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            if (animator == null) return;
            animator.SetTrigger(Pressed);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable) return;
            if (animator == null) return;
            animator.SetTrigger(Normal);
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
        }
        #endregion

    }

}