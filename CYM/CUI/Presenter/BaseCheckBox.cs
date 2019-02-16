using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseCheckBoxData : BaseButtonData
    {
        public Func<bool> IsOn = () => { return true; };
        public Callback<bool> OnValueChanged;
    }
    public class BaseCheckBox : Presenter<BaseCheckBoxData>
    {
        #region inspector
        [SerializeField]
        Text Text;
        [SerializeField]
        Image ActiveImage;
        #endregion

        #region prop
        bool IsOn = false;
        bool IsPreOn = false;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Text != null)
                Text.text = Data.GetName();
            IsPreOn=IsOn = Data.IsOn.Invoke();
            RefreshActive();
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            IsOn = !Data.IsOn.Invoke();
            if (IsOn != IsPreOn)
            {
                IsPreOn = IsOn;
                Data.OnValueChanged?.Invoke(IsOn);
            }
            RefreshActive();
        }
        void RefreshActive()
        {
            if (ActiveImage != null)
            {
                if (IsOn)
                {
                    ActiveImage.CrossFadeAlpha(1.0f, 0.1f, true);
                }
                else
                {
                    ActiveImage.CrossFadeAlpha(0.0f, 0.1f, true);
                }
            }
        }
        #endregion
    }
}