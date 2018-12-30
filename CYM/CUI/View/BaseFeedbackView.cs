//------------------------------------------------------------------------------
// BaseFeedbackView.cs
// Copyright 2018 2018/6/16 
// Created by CYM on 2018/6/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using UnityEngine.EventSystems;

namespace CYM
{
    public class BaseFeedbackView : BaseUIView
    {
        #region inspector
        [SerializeField]
        BaseInputField InputTitle;
        [SerializeField]
        BaseInputField InputDesc;
        [SerializeField]
        BaseButton BntSubmit;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntClose.Data.IsInteractable = IsSubmitInteractable;
            BntSubmit.Init(new BaseButtonData {OnClick = OnSubmit ,IsInteractable = IsSubmitInteractable ,Name=()=>"Submit",IsTrans=false});
            InputTitle.Init(new BaseInputFieldData { OnEndEdit = OnInputTitleEndEdit, IsInteractable = IsSubmitInteractable });
            InputDesc.Init(new BaseInputFieldData { OnEndEdit = OnInputDescEndEdit, IsInteractable = IsSubmitInteractable });

            FeedbackMgr.Trello.Callback_OnStartSend += OnStartSend;
            FeedbackMgr.Trello.Callback_OnEndSend += OnEndSend;
        }

        public override void OnDestroy()
        {
            FeedbackMgr.Trello.Callback_OnEndSend -= OnEndSend;
            FeedbackMgr.Trello.Callback_OnStartSend -= OnStartSend;
            base.OnDestroy();
        }
        #endregion

        #region get
        protected override string GetTitle()
        {
            return "Feedback";
        }
        protected virtual BaseFeedbackMgr FeedbackMgr => SelfBaseGlobal.FeedbackMgr;
        #endregion

        #region Callback
        protected virtual void OnSubmit(BasePresenter presenter, PointerEventData arg3)
        {
            FeedbackMgr.Send(InputTitle.InputText, InputDesc.InputText);
        }
        protected virtual void OnInputTitleEndEdit(string arg1)
        {
            SetDirty();
        }
        protected virtual void OnInputDescEndEdit(string arg1)
        {
            SetDirty();
        }
        private bool IsSubmitInteractable(int arg)
        {
            return !FeedbackMgr.Trello.IsSubmitting;
        }
        private void OnEndSend()
        {           
            SetDirty();
        }

        private void OnStartSend()
        {
            SetDirty();
        }
        #endregion
    }
}