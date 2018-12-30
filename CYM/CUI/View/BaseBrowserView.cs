//------------------------------------------------------------------------------
// BaseBrowserView.cs
// Copyright 2018 2018/3/22 
// Created by CYM on 2018/3/22
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
//using ZenFulcrum.EmbeddedBrowser;
using System;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseBrowserView : BaseUIView 
    {
        [SerializeField]
        BaseButton BntBack;
        [SerializeField]
        BaseButton BntForward;
        [SerializeField]
        BaseButton BntRefresh;
        [SerializeField]
        BaseInputField Input;
        //[SerializeField]
        //Browser BrowserCtrl;
        //[SerializeField]
        //PointerUIGUI PointerUIGUI;

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            //Input.Init(new BaseInputFieldData { OnEndEdit= OnEndEdit, OnValueChange = OnChange });
            BntBack.Init(new BaseButtonData {OnClick= OnBntBack });
            BntForward.Init(new BaseButtonData { OnClick = OnBntForward });
            BntRefresh.Init(new BaseButtonData { OnClick = OnBntRefresh});
        }
        public void Show(string URL)
        {
            if (URL.IsInvStr())
                return;
            Show(true);
            //BrowserCtrl.Url = URL;
            Input.InputText = URL;
        }
        #endregion

        #region Callback
        private void OnChange(string arg1)
        {
        }
        private void OnEndEdit(string arg1)
        {
            //BrowserCtrl.Url = arg1;
        }
        private void OnBntBack(BasePresenter presenter, PointerEventData arg3)
        {
            //BrowserCtrl.GoBack();
        }
        private void OnBntForward(BasePresenter presenter, PointerEventData arg3)
        {
            //BrowserCtrl.GoForward();
        }
        private void OnBntRefresh(BasePresenter presenter, PointerEventData arg3)
        {
            //BrowserCtrl.LoadURL(BrowserCtrl.Url, true);
        }
        private void OnBntClose(BasePresenter presenter, PointerEventData arg3)
        {
            Show(false);
        }
        #endregion
    }
}