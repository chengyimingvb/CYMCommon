//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public enum InputBntType
    {
        Normal,
        Up,
        Down,
        DoublePressHold,
        DoublePressDown,
        DoublePressUp,
    }
    public enum InputAxisType
    {
        Normal,
        Raw,
        DoubleClick,
    }

    public class BaseInputMgr : BaseGlobalCoreMgr
    {
        #region Callback
        public event Callback Callback_OnInputMapChanged;
        #endregion

        #region prop
        public static readonly string UISubmit = "UISubmit";
        public static readonly string UICancel = "UICancel";
        public static readonly string UIPgUp = "UIPgUp";
        public static readonly string UIPgDn = "UIPgDn";

        protected BoolState IsDisablePlayerInput { get; set; } = new BoolState();
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsCanInput())
            {
                UpdateInput();
            }
        }
        protected virtual void UpdateInput()
        {

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
        }
        #endregion

        #region set
        public bool GetBnt(string key, InputBntType type = InputBntType.Down)
        {
            return false;
        }
        public bool IsAnyKey()
        {
            return Input.anyKey;
        }
        public virtual void EnablePlayerInput(bool b)
        {
            IsDisablePlayerInput.Push(!b);
        }
        public virtual void ResumePlayerInput()
        {
            IsDisablePlayerInput.Reset();
        }
        public bool GetMouseDown(int index)
        {
            return Input.GetMouseButtonDown(index);
        }
        public bool GetMouseUp(int index)
        {
            return Input.GetMouseButtonUp(index);
        }
        public bool GetMouse(int index)
        {
            return Input.GetMouseButton(index);
        }
        public bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }
        public bool GetKeyUp(KeyCode keyCode)
        {
            return Input.GetKeyUp(keyCode);
        }
        public bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }
        #endregion

        #region is
        public virtual bool IsCanInput()
        {
            if (SelfBaseGlobal == null)
                return false;
            if (SelfBaseGlobal.DevConsoleMgr.IsShow())
                return false;
            return true;
        }
        public virtual bool IsCanPlayerInput()
        {
            if (SelfBaseGlobal == null)
                return false;
            if (IsDisablePlayerInput.IsIn())
                return false;
            if (SelfBaseGlobal.DevConsoleMgr.IsShow())
                return false;
            if (SelfBaseGlobal.IsPause)
                return false;
            return true;
        }
        #endregion

        #region UI
        protected bool GetUICancle(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UICancel, type);
        }

        protected bool GetUISubmit(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UISubmit, type);
        }
        protected bool GetUIPgUp(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UIPgUp, type);
        }
        protected bool GetUIPgDn(InputBntType type = InputBntType.Down)
        {
            return GetBnt(UIPgDn, type);
        }
        #endregion

    }

}