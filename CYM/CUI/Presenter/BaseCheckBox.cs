using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using System.Collections.Generic;

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
        Toggle ToggleObj;
        [SerializeField]
        Text Text;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
            if(ToggleObj!=null)
            {
                ToggleObj.onValueChanged.AddListener(OnValueChanged);
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Text != null)
                Text.text = Data.GetName();
            if (ToggleObj != null)
            {
                ToggleObj.interactable = Data.IsInteractable.Invoke(Index);
                ToggleObj.isOn = Data.IsOn.Invoke();
            }
        }
        public override void Cleanup()
        {
            if (ToggleObj != null)
            {
                ToggleObj.onValueChanged.RemoveAllListeners();
            }
            base.Cleanup();
        }
        #endregion

        #region Callback
        void OnValueChanged(bool b)
        {
            Data.OnValueChanged?.Invoke(b);
        }
        #endregion
    }
}