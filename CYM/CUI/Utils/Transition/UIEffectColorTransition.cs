using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using Sirenix.OdinInspector;

namespace CYM.UI
{
    [ExecuteInEditMode]
    public class UIEffectColorTransition : UITransition
    {
        #region Inspector
        public string StateColorPreset;
        [HideIf("Inspector_IsHideStateColor")]
        public PresenterStateColor StateColor = new PresenterStateColor();
        #endregion

        #region prop
        private TweenerCore<Color, Color, DG.Tweening.Plugins.Options.ColorOptions> colorTween;
        #endregion

        #region LIFE
        protected override void Awake()
        {
            base.Awake();
            
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Shadow != null)
            {
                colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Enter, Duration).SetDelay(Delay);
            }
            else if (Outline != null)
            {
                colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Enter, Duration).SetDelay(Delay);
            }
            else
            {
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Shadow != null)
            {

                colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            if (Outline != null)
            {

                colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Shadow != null)
            {


                colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Press, Duration).SetDelay(Delay);
            }
            if (Outline != null)
            {

                colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Press, Duration).SetDelay(Delay);
            }
            else
            {
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Effect == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Shadow != null)
            {

                colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            if (Outline != null)
            {

                colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
            }
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (Effect == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Shadow != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Disable, Duration).SetDelay(Delay);
                }
            }
            else if (Outline != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Disable, Duration).SetDelay(Delay);
                }
            }
            else
            {
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (Effect == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Shadow != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Selected, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Shadow.effectColor, x => Shadow.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
            }
            else if (Outline != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Selected, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Outline.effectColor, x => Outline.effectColor = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
            }
            else
            {
            }
        }
        #endregion

        #region inspector
        bool Inspector_IsHideStateColor()
        {
            return !StateColorPreset.IsInvStr();
        }
        #endregion
    }

}