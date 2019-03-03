//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Input;
using static UnityEngine.Experimental.Input.InputAction;

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

    public class BaseInputMgr : BaseGFlowMgr
    {
        #region Callback
        public event Callback Callback_OnInputMapChanged;
        #endregion

        #region prop
        protected BoolState IsDisablePlayerInput { get; set; } = new BoolState();
        protected InputActionAssetReference InputAssetReference => SelfBaseGlobal.InputAssetReference;
        protected InputActionAsset InputAsset => InputAssetReference.asset;
        public InputActionMap GamePlayMap { get; protected set; }
        public InputActionMap MenuMap { get; protected set; }
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            if (InputAssetReference == null || InputAsset == null)
            {
                CLog.Error("没有配置InputConfig");
            }
            else
            {
                GamePlayMap = TryGetActionMap("GamePlay");
                MenuMap = TryGetActionMap("Menu");
            }
        }
        public override void OnStart()
        {
            base.OnStart();
            Load();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateMapEnable(GamePlayMap, IsCanGamePlayInput);
        }
        void UpdateMapEnable(InputActionMap map , Func<bool> DoIsEnable)
        {
            if (map != null && DoIsEnable != null)
            {
                bool temp = DoIsEnable();
                if (map.enabled != temp)
                {
                    if (temp)
                        map.Enable();
                    else
                        map.Disable();
                }
            }
        }
        #endregion

        #region get
        protected InputActionMap TryGetActionMap(string id)
        {
            return InputAsset.TryGetActionMap(id);
        }
        protected InputAction GetGamePlayAction(string id)
        {
            return GamePlayMap.TryGetAction(id);
        }
        protected InputAction GetMenuAction(string id)
        {
            return MenuMap.TryGetAction(id);
        }
        #endregion

        #region set
        protected InputAction RegisterGameplay(string id, Action<CallbackContext> perform, Action<CallbackContext> start = null, Action<CallbackContext> cancel=null)
        {
            var item = GetGamePlayAction(id);
            if (item == null)
                return null;
            item.performed += perform;
            if (start != null) item.started += start;
            if (cancel!= null) item.cancelled += cancel;
            return item;
        }
        public virtual void EnablePlayerInput(bool b)
        {
            IsDisablePlayerInput.Push(!b);
        }
        public virtual void ResumePlayerInput()
        {
            IsDisablePlayerInput.Reset();
        }
        public void Save()
        {
            BaseFileUtils.SaveJson(BaseConstMgr.Path_Shortcuts,InputAsset.ToJson());
        }
        public void Load()
        {
            string data = BaseFileUtils.LoadFile(BaseConstMgr.Path_Shortcuts);
            if (data == null)
                return;
            InputAsset.LoadFromJson(data);
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
        public virtual bool IsCanGamePlayInput()
        {
            if (SelfBaseGlobal == null)
                return false;
            if (BattleMgr == null)
                return false;
            if (!BattleMgr.IsInBattle)
                return false;
            if (!IsCanInput())
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

        #region OldInput
        public bool GetBnt(string key, InputBntType type = InputBntType.Down)
        {
            return false;
        }
        public bool IsAnyKey()
        {
            return Input.anyKey;
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

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
        }
        #endregion



    }

}