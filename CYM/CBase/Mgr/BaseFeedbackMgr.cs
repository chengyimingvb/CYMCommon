//------------------------------------------------------------------------------
// BaseFeedbackMgr.cs
// Copyright 2018 2018/6/16 
// Created by CYM on 2018/6/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using CYM.Utile;
namespace CYM
{
    public class BaseFeedbackMgr : BaseCoreMgr  
    {
        #region Callback val
        #endregion

        #region prop
        public Trello Trello { get; private set; }
        private string SendTitle = "";
        private string SendDesc = "";
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            Trello = new Trello();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            Trello.Callback_OnStartScreenshot += OnStartScreenshot;
            Trello.Callback_OnEndScreenshot += OnEndScreenshot;
            Trello.Callback_OnStartSend += OnStartSend;
            Trello.Callback_OnEndSend += OnEndcSend;
        }
        public override void OnDisable()
        {
            Trello.Callback_OnStartSend -= OnStartSend;
            Trello.Callback_OnEndSend -= OnEndcSend;
            Trello.Callback_OnEndScreenshot -= OnEndScreenshot;
            Trello.Callback_OnStartScreenshot -= OnStartScreenshot;
            base.OnDisable();
        }
        #endregion

        #region set
        public void Send(string title, string desc)
        {
            if (title.IsInvStr())
                return;
            if (desc.IsInvStr())
                return;
            SendTitle = title;
            SendDesc = desc;
            Trello.Send(SendTitle, SendDesc);
        }
        public void ScreenShot()
        {
            Trello.ScreenShot();
        }
        #endregion

        #region Callback

        protected virtual void OnEndScreenshot()
        {

        }

        protected virtual void OnStartScreenshot()
        {

        }
        protected virtual void OnStartSend()
        {
        }

        protected virtual void OnEndcSend()
        {

        }
        #endregion


    }
}