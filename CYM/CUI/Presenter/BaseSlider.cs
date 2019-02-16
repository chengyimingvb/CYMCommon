using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using System.Collections.Generic;

namespace CYM.UI
{
    public class BaseSliderData : BaseButtonData
    {
        public Callback<float> OnValueChanged;
        public Func<float,string> ValueText=(x)=> { return BaseUIUtils.Percent(x); };
        public Func<float> Value = () => 0;
        public float MaxVal = 1.0f;
        public float MinVal = 0.0f;
    }
    public class BaseSlider : Presenter<BaseSliderData>
    {
        #region presenter
        [SerializeField]
        Slider Slider;
        [SerializeField]
        Text Name;
        [SerializeField]
        Text Value;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
        }
        public override void Init(BaseSliderData data)
        {
            base.Init(data);
            if (Slider != null)
            {

                Slider.onValueChanged.AddListener(OnValueChanged);
                Slider.maxValue = data.MaxVal;
                Slider.minValue = data.MinVal;
                Slider.value = data.Value.Invoke();
            }
        }
        public override void Cleanup()
        {
            if (Slider != null)
            {
                Slider.onValueChanged.RemoveAllListeners();
            }
            base.Cleanup();
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Slider != null)
            {
                Slider.value = Data.Value.Invoke();
            }
            if (Name != null)
            {
                Name.text = Data.GetName();
            }
            RefreshValueChange();
        }
        #endregion

        #region set
        void RefreshValueChange()
        {
            if (Value != null)
            {
                Value.text = Data.ValueText.Invoke(Slider.value);
            }
        }
        #endregion

        #region callback
        void OnValueChanged(float value)
        {
            Data.OnValueChanged?.Invoke(value);
            RefreshValueChange();
        }
        #endregion
    }

}