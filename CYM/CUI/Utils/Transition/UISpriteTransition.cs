using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;

namespace CYM.UI
{
    [ExecuteInEditMode]
    public class UISpriteTransition : UITransition
    {
        #region Inspector
        public Sprite Normal = null;
        public Sprite Enter = null;
        public Sprite Press = null;
        public Sprite Disable = null;
        public Sprite Selected = null;
        #endregion

        #region LIFE
        protected override void Awake()
        {
            base.Awake();
            if (Image != null)
            {
                Image.overrideSprite = Normal;
            }
        }
        #endregion

        #region callback
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Enter;
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
            if (Image != null)
            {
                Image.overrideSprite = Normal;
            }
            else
            {
                //Graphic.CrossFadeColor(Enter, Duration, true, true);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Press;
            }
            else
            {
                //Graphic.CrossFadeColor(Enter, Duration, true, true);
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsSelected) return;
            if (!IsInteractable) return;
            if (Graphic == null) return;
            if (Image != null)
            {
                Image.overrideSprite = Normal;
            }
            else
            {
                //Graphic.CrossFadeColor(Enter, Duration, true, true);
            }
        }

        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            if (Graphic == null) return;
            if (Image != null)
            {
                if(b)
                    Image.overrideSprite = Normal;
                else
                    Image.overrideSprite =Disable;
            }
            else
            {
                //Graphic.CrossFadeColor(Enter, Duration, true, true);
            }
        }
        public override void OnSelected(bool b)
        {
            if (!IsInteractable) return;
            base.OnSelected(b);
            if (Graphic == null) return;
            if (Image != null)
            {
                if (b)
                    Image.overrideSprite = Selected;
                else
                    Image.overrideSprite = Normal;
            }
            else
            {
                //Graphic.CrossFadeColor(Enter, Duration, true, true);
            }
        }
        #endregion
    }

}