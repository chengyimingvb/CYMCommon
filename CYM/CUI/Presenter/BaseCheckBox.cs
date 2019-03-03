using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BaseCheckBoxData : BaseButtonData
    {
        /// <summary>
        /// 连接的Presenter
        /// </summary>
        public BasePresenter LinkPresenter=null;
        /// <summary>
        /// 连接的View
        /// </summary>
        public BaseUIView LinkView=null;
        public Func<bool> IsOn = null;
        public Callback<bool> OnValueChanged=null;
    }
    public class BaseCheckBox : Presenter<BaseCheckBoxData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField]
        public Text Text;
        [FoldoutGroup("Inspector"), SerializeField]
        public Image Icon;
        [FoldoutGroup("Inspector"), SerializeField]
        public Image ActiveIcon;
        #endregion

        #region data
        [SerializeField, FoldoutGroup("Data")]
        float FadeDuration = 0.1f;
        #endregion

        #region prop
        bool IsOn = false;
        public bool IsToggleGroup { get; set; } = false;
        public bool IsHaveLink => Data.LinkPresenter != null || Data.LinkView != null;
        #endregion

        #region life
        public override void Init(BaseCheckBoxData data)
        {
            base.Init(data);

            //自动获得激活图片,后缀有_N自动替换成_On 否则自动增加_On
            if (!data.IconStr.IsInvStr() && data.ActiveIconStr.IsInvStr())
            {
                if (data.IconStr.EndsWith(BaseConstMgr.Suffix_Checkbox_N))
                    data.ActiveIconStr = data.IconStr.Replace(BaseConstMgr.Suffix_Checkbox_N, BaseConstMgr.Suffix_Checkbox_On);
                else
                    data.ActiveIconStr = data.IconStr + BaseConstMgr.Suffix_Checkbox_On;
            }

            if (Data != null)
            {
                if (Data.LinkView != null)
                    Data.LinkView.Callback_OnShow += OnLinkShow;
                if (Data.LinkPresenter != null)
                    Data.LinkPresenter.Callback_OnShow += OnLinkShow;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Data != null)
            {
                if (Data.LinkView != null)
                    Data.LinkView.Callback_OnShow -= OnLinkShow;
                if (Data.LinkPresenter != null)
                    Data.LinkPresenter.Callback_OnShow -= OnLinkShow;
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            if (Text != null)
                Text.text = Data.GetName();
            if (Icon != null)
                Icon.overrideSprite = Data.GetIcon();
            if (ActiveIcon != null)
                ActiveIcon.overrideSprite = Data.GetActiveIcon();

            if (!IsToggleGroup)
                RefreshState();
            else
                RefreshStateByDupplicate(BaseDupplicate? BaseDupplicate.CurSelectIndex:0);

            RefreshActiveIcon();
            RefreshLink();
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            Selected(!IsOn);
        }
        public virtual void Selected(bool b)
        {
            if (!IsToggleGroup)
            {
                RefreshStateByInput(b);
                RefreshActiveIcon();
                RefreshLink();
            }
            else
            {
                BaseDupplicate?.OnTabClick(this, null);
            }
        }
        #endregion

        #region Callback
        private void OnLinkShow(bool arg1)
        {
            RefreshStateByLink();
            RefreshActiveIcon();
        }
        #endregion

        #region utile
        void RefreshState()
        {
            if (Data.IsOn != null)
                IsOn = Data.IsOn.Invoke();
            else if (Data.LinkView != null)
                IsOn = Data.LinkView.IsShow;
            else if (Data.LinkPresenter != null)
                IsOn = Data.LinkPresenter.IsShow;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshStateByLink()
        {
            if (Data?.LinkView != null)
                IsOn = Data.LinkView.IsShow;
            else if (Data?.LinkPresenter != null)
                IsOn = Data.LinkPresenter.IsShow;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshStateByDupplicate(int index)
        {
            if (index != -1)
                IsOn = index == Index;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshStateByInput(bool inputState)
        {                
            IsOn = inputState;
            Data?.OnValueChanged?.Invoke(IsOn);
        }
        void RefreshLink()
        {
            if (Data?.LinkPresenter != null)
            {
                Data?.LinkPresenter.Show(IsOn);
            }
            if (Data?.LinkView != null)
            {
                Data?.LinkView.Show(IsOn);
            }
        }
        void RefreshActiveIcon()
        {
            if (ActiveIcon != null)
            {
                if (IsOn)
                {
                    ActiveIcon.CrossFadeAlpha(1.0f, FadeDuration, true);
                }
                else
                {
                    ActiveIcon.CrossFadeAlpha(0.0f, FadeDuration, true);
                }
            }
        }
        #endregion
    }
}