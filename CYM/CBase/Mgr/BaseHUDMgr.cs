//**********************************************
// Class Name	: BaseHUDMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;
using CYM.UI;
using System;
namespace CYM
{ 
	public class BaseHUDMgr : BaseCoreMgr
	{
        #region text
        protected virtual string ChatBubble => "BaseTextBubble";
        protected virtual string DamageText => "BaseDamageJumpFont";
        protected virtual string TreatmentText => "BaseTreatmentJumpFont";
        protected virtual string StateJumpText => "BaseStateJumpText";
        protected virtual string BarItemPrefab => BaseConstMgr.STR_Inv;
        #endregion

        #region prop
        protected float NextInterval = 0.0f;
        public bool IsCanJumpText { get; set; } = true;
        public BaseHUDItem BaseHUDBarItem { get; protected set; }
        #endregion

        #region Life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (!BarItemPrefab.IsInvStr())
            {
                BaseHUDBarItem = Spawn(BarItemPrefab);
            }
        }
        public override void OnDisable()
        {
            base.OnDisable();
            if (BaseHUDBarItem != null)
            {
                BaseHUDBarItem.DoDestroy();
            }
        }
        public override void Init()
        {
            base.Init();
            jumpFontTimer.Restart();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateJumpFontList();
        }
        #endregion

        #region JumpFont
        protected struct JumpFontData
        {
            public string text;
            public string prefabNamne;
        }
        protected List<JumpFontData> jumpList = new List<JumpFontData>();
        protected Timer jumpFontTimer = new Timer();
        protected void addToJumpFontList(string text, string prefabName)
        {
            JumpFontData tempData = new JumpFontData();
            tempData.text = text;
            tempData.prefabNamne = prefabName;
            jumpList.Add(tempData);
        }
        private void jumpFont(JumpFontData data)
        {
            GameObject tempGO = SelfBaseGlobal.GRMgr.GetUI(data.prefabNamne);
            if (tempGO != null)
            {
                var temp = HUDView.Jump(data.text, tempGO, SelfBaseUnit);
                temp.SetFollowObj(GetNode(temp.NodeType));
            }
        }
        void UpdateJumpFontList()
        {
            if (jumpFontTimer.Elapsed() > NextInterval)
            {
                jumpFontTimer.Restart();
                if (jumpList.Count > 0)
                {
                    jumpFont(jumpList[0]);
                    NextInterval = 0.4f;
                    jumpList.RemoveAt(0);
                }
            }
        }
        public void JumpDamage(float val, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer))
                return;
            addToJumpFontList(BaseUIUtils.RoundDigit(val), DamageText);
        }
        public void JumpDamageStr(string str, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer))
                return;
            addToJumpFontList(str, DamageText);
        }
        public void JumpTreatment(string key, bool needPlayer = false, params string[] objs)
        {
            if (!IsCanJump(needPlayer))
                return;
            string final = BaseLanguageMgr.Get(key, objs);
            addToJumpFontList(final, TreatmentText);
        }
        public void JumpTreatmentStr(string str, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer))
                return;
            addToJumpFontList(str, TreatmentText);
        }
        public void JumpState(string key, bool needPlayer = false)
        {
            if (!IsCanJump(needPlayer))
                return;
            string final = BaseLanguageMgr.Get(key);
            addToJumpFontList(final, StateJumpText);
        }
        protected virtual bool IsCanJump(bool needPlayer = false)
        {
            return IsCanJumpText;
        }
        #endregion

        #region Chat bubble
        BaseHUDItem CurChatBubble;
        public BaseHUDItem JumpChatBubbleStr(string str)
        {
            if (str.IsInvStr())
                return null;
            if (CurChatBubble != null)
            {
                CurChatBubble.DoDestroy();
                CurChatBubble = null;
            }
            GameObject tempGO = SelfBaseGlobal.GRMgr.GetUI(ChatBubble);
            if (tempGO != null)
            {
                CurChatBubble = HUDView.Jump(str ,tempGO, SelfBaseUnit);
                if (CurChatBubble == null) return null;
                CurChatBubble.SetFollowObj(GetNode(CurChatBubble.NodeType));
                return CurChatBubble;
            }
            return null;
        }
        public BaseHUDItem JumpChatBubble(string key)
        {
            if (key.IsInvStr())
                return null;
            return JumpChatBubbleStr(BaseLanguageMgr.Get(key));
        }
        #endregion

        #region set
        protected BaseHUDItem Spawn(string prefabName)
        {
            GameObject tempGO = SelfBaseGlobal.GRMgr.GetUI(prefabName);
            if (tempGO != null)
            {
                var temp = HUDView.Jump(null ,tempGO, SelfBaseUnit);
                temp.SetFollowObj(GetNode(temp.NodeType));
                return temp;
            }
            return null;
        }
        #endregion

        #region must Override
        protected virtual BaseHUDView HUDView
        {
            get
            {
                throw new NotImplementedException("此函数必须被实现");
            }
        }
        protected virtual Transform GetNode(NodeType type)
        {
            throw new NotImplementedException("此函数必须被实现");
        }
        #endregion

    }
}