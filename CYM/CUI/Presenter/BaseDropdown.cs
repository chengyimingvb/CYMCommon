using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using System.Collections.Generic;

namespace CYM.UI
{
    public class BaseDropdownData : BaseButtonData
    {
        public Func<string[]> Opts;
        public Callback<int> OnValueChanged;
        public Func<int> Value;
    }

    public class BaseDropdown : Presenter<BaseDropdownData>
    {
        #region presenter
        [SerializeField]
        Dropdown Dropdown;
        [SerializeField]
        Text Name;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
        }
        public override void Init(BaseDropdownData data)//,bool isAutoRefresh=true
        {
            base.Init(data);//, isAutoRefresh
            if (Dropdown != null)
            {
                Dropdown.onValueChanged.AddListener(OnValueChanged);
            }
        }
        public override void Cleanup()
        {
            if (Dropdown != null)
            {
                Dropdown.ClearOptions();
                Dropdown.onValueChanged.RemoveAllListeners();
            }
            base.Cleanup();
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Dropdown != null)
            {
                if (Data.Opts != null)
                {
                    List<Dropdown.OptionData> listOp = new List<Dropdown.OptionData>();
                    foreach (var item in Data.Opts.Invoke())
                        listOp.Add(new Dropdown.OptionData(item));
                    Dropdown.ClearOptions();
                    Dropdown.AddOptions(listOp);
                }

                if (Data.Value!=null)
                    Dropdown.value = Data.Value.Invoke();
            }
            if(Name!=null)
                Name.text = Data.GetName();
        }
        #endregion

        #region callback
        void OnValueChanged(int index)
        {
            Data.OnValueChanged?.Invoke(index);
        }
        #endregion
    }

}