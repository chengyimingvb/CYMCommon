using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using Sirenix.OdinInspector;

namespace CYM.UI
{
    public class UIColorTransition : UITransition
    {
        #region Inspector
        public string StateColorPreset;
        [HideIf("Inspector_IsHideStateColor")]
        public PresenterStateColor StateColor=new PresenterStateColor();
        #endregion

        #region prop
        private TweenerCore<Color, Color, DG.Tweening.Plugins.Options.ColorOptions> colorTween;
        #endregion

        #region LIFE
        protected override void Awake()
        {
            base.Awake();
            if(!StateColorPreset.IsInvStr())
                StateColor = UIConfig.Ins.GetStateColor(StateColorPreset);
            if (Text != null)
            {
                Text.color = StateColor.Normal;
            }
            else if (Image != null)
            {
                Image.color = StateColor.Normal;
            }
            else if (Graphic != null)
                Graphic.color = Color.white ;
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {

                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Enter, Duration).SetDelay(Delay);
            }
            else if (Image != null)
            {
                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Enter, Duration).SetDelay(Delay);
            }
            else
            {
                //Graphic.CrossFadeColor(Enter, Duration, true, true);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {

                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            if (Image != null)
            {

                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
                //Graphic.CrossFadeColor(Normal, Duration, true, true);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {


                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Press, Duration).SetDelay(Delay);
            }
            if (Image != null)
            {

                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Press, Duration).SetDelay(Delay);
            }
            else
            {
                //Graphic.CrossFadeColor(Press, Duration, true, true);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {

                colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            if (Image != null)
            {

                colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
            }
            else
            {
                //Graphic.CrossFadeColor(Normal, Duration, true, true);
            }
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Disable, Duration).SetDelay(Delay);
                }
            }
            else if (Image != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Disable, Duration).SetDelay(Delay);
                }
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Disable, Duration, true, true);
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (Graphic == null) return;
            if (colorTween != null)
                colorTween.Kill();
            if (Text != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Selected, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Text.color, x => Text.color = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
            }
            else if (Image != null)
            {

                if (b)
                {
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Selected, Duration).SetDelay(Delay);
                }
                else
                {
                    colorTween = DOTween.To(() => Image.color, x => Image.color = x, StateColor.Normal, Duration).SetDelay(Delay);
                }
            }
            else
            {
                if (b)
                    Graphic.CrossFadeColor(StateColor.Selected, Duration, true, true);
                else
                    Graphic.CrossFadeColor(StateColor.Normal, Duration, true, true);
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
