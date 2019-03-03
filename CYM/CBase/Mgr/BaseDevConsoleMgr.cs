//**********************************************
// Class Name	: BaseDevConsoleMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickDev.CommandSystem;
namespace CYM
{
	public class BaseDevConsoleMgr : BaseGFlowMgr
    {
        #region prop
        BaseExcelMgr ExcelMgr => SelfBaseGlobal.ExcelMgr;
        #endregion

        static BaseDevConsoleMgr Ins;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            Ins = this;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Application.isEditor)
            {
                OnTestUpdate();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();

        }
        public override void OnDisable()
        {
            base.OnDisable();
        }

        public void Toggle()
        {
            if (DevConsole.singleton != null)
            {
                if (DevConsole.singleton.isOpen)
                    DevConsole.singleton.Close();
                else
                {
                    DevConsole.singleton.Open();
                }
            }
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
            if (DevConsole.singleton != null)
            {
                DevConsole.singleton.enabled = b;
            }
        }
        public bool IsEnableConsole
        {
            get {
                if (DevConsole.singleton == null)
                    return false;
                return DevConsole.singleton.enabled;
            }
        }

        protected virtual void OnTestUpdate()
        {

        }

        #region is
        public bool IsShow()
        {
            return DevConsole.singleton.isOpen;
        }
        protected bool IsGMMode
        {
            get
            {
                return SelfBaseGlobal.DiffMgr.IsGMMode();
            }
        }
        #endregion

        #region Command

        #endregion
    }
}