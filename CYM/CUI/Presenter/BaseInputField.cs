using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using Sirenix.OdinInspector;
//using static UnityEngine.UI.InputField;

namespace CYM.UI
{
    public class BaseInputFieldData : PresenterData
    {
        public Callback<string> OnValueChange;
        public Callback<string> OnEndEdit;
    }

    public class BaseInputField : Presenter<BaseInputFieldData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField]
        InputField InputField;
        [FoldoutGroup("Inspector"), SerializeField]
        GameObject Placeholder;
        [FoldoutGroup("Inspector"), SerializeField]
        BaseImage Focus;
        #endregion

        #region life
        public override void Init(BaseInputFieldData data)
        {
            base.Init(data);
            InputField.onValueChanged.AddListener(OnValueChanged);
            InputField.onEndEdit.AddListener(OnEndEdit);
        }
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void OnDestroy()
        {
            InputField.onValueChanged.RemoveAllListeners();
            base.OnDestroy();
        }
        #endregion

        #region get
        /// <summary>
        /// 输入的字符窜
        /// </summary>
        public string InputText
        {
            get
            {
                return InputField.text;
            }
            set
            {
                InputField.text = value;
            }
        }
        #endregion

        #region is
        public bool IsHaveText()
        {
            return !InputText.IsInvStr();
        }
        #endregion

        #region Callback
        public override void OnInteractable(bool b)
        {
            base.OnInteractable(b);
            InputField.readOnly = !b;
            InputField.interactable = b;
        }
        void OnValueChanged(string text)
        {
            Data?.OnValueChange?.Invoke(text);
        }
        void OnEndEdit(string text)
        {
            Data?.OnEndEdit?.Invoke(text);
        }
        #endregion
    }

}