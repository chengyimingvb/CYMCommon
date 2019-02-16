//------------------------------------------------------------------------------
// BaseCursorMgr.cs
// Copyright 2018 2018/11/1 
// Created by CYM on 2018/11/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;

namespace CYM
{
    public class BaseCursorMgr : BaseGFlowMgr
    {
        #region prop
        List<Texture2D> currentCursor;
        Texture2D curCursorTex;
        int curCursorIndex = 0;
        public bool IsStayUI { get; private set; }
        #endregion

        #region mgr
        BaseAudioMgr AudioMgr => SelfBaseGlobal.AudioMgr;
        CursorConfig CursorConfig => CursorConfig.Ins;
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            SetCursor(CursorConfig.Normal);
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Input.GetMouseButtonDown(0))
            {
                if (currentCursor == CursorConfig.Normal)
                {
                    SetPress();
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (currentCursor == CursorConfig.Press)
                {
                    SetNormal();
                }
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (currentCursor != null &&
                curCursorTex != currentCursor[curCursorIndex])
            {
                curCursorTex = currentCursor[curCursorIndex];
                Cursor.SetCursor(currentCursor[curCursorIndex], new Vector2(3, 3), CursorMode.Auto);
                curCursorIndex++;
                if (currentCursor.Count <= curCursorIndex)
                    curCursorIndex = 0;
            }
        }
        #endregion

        public void Hide(bool b)
        {
            Cursor.lockState = b ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !b;
        }
        protected void SetCursor(List<Texture2D> cursorList)
        {
            if (cursorList == null)
                return;
            if (cursorList.Count == 0)
                return;
            if (currentCursor == cursorList)
                return;
            currentCursor = cursorList;
            curCursorIndex = 0;
        }
        public void SetWait()
        {
            SetCursor(CursorConfig.Wait);
        }
        public void SetNormal()
        {
            SetCursor(CursorConfig.Normal);
        }
        public void SetPress()
        {
            SetCursor(CursorConfig.Press);
            if (CursorConfig.PressSound != null && SelfBaseGlobal != null)
                AudioMgr?.PlayUI(CursorConfig.PressSound);
        }
        public void PlayPressAudio()
        {
            if (CursorConfig.PressSound != null)
                AudioMgr?.PlayUI(CursorConfig.PressSound);
        }

        public void MouseEnterUnit()
        {
        }
        public void MouseExitUnit()
        {

        }
        void OnEnterTerrain(Vector3 point)
        {

        }
        void OnExitTerrain()
        {

        }
        void OnStayTerrin(Vector3 point)
        {

        }
        void OnEnterUI()
        {
            IsStayUI = true;
            SetCursor(CursorConfig.Ins.Normal);
        }
        void OnExitUI()
        {
            IsStayUI = false;
        }

        #region Callback
        protected override void OnBattleLoad()
        {
            SetWait();
        }
        protected override void OnBattleLoaded()
        {
            SetNormal();
        }
        protected override void OnBattleUnLoad()
        {
            SetWait();
        }
        protected override void OnBattleUnLoaded()
        {
            SetNormal();
        }
        #endregion
    }
}