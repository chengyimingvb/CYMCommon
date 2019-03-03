//**********************************************
// Class Name	: SettingsView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
using CYM;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class BaseModalBoxView : BaseStaticUIView<BaseModalBoxView>
    {
        [SerializeField]
        BaseText Desc;
        [SerializeField]
        BaseButton Bnt1;
        [SerializeField]
        BaseButton Bnt2;
        [SerializeField]
        BaseButton Bnt3;

        event Callback Callback_Bnt1;
        event Callback Callback_Bnt2;
        event Callback Callback_Bnt3;

        string InputTitle="";
        string InputDesc="";
        string InputBntStr1="";
        string InputBntStr2 = "";
        string InputBntStr3 = "";

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Title.Data.IsTrans = false;
            Desc.Init(new BaseTextData() { Name = () => InputDesc, IsTrans = false });
            Bnt1?.Init(new BaseButtonData() { OnClick = OnClickBnt1, Name = () => InputBntStr1, IsTrans = false });
            Bnt2?.Init(new BaseButtonData() { OnClick = OnClickBnt2, Name = () => InputBntStr2, IsTrans = false });
            Bnt3?.Init(new BaseButtonData() { OnClick = OnClickBnt3, Name = () => InputBntStr3, IsTrans = false });
        }
        #endregion

        #region set
        /// <summary>
        /// 语法糖对话框调用函数
        /// 直接传KEY
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descKey"></param>
        /// <param name="BntOK"></param>
        /// <param name="paras"></param>
        public new void ShowOK(string key,string descKey,Callback BntOK,params object[] paras)
        {
            Show(GetStr(key),GetStr(descKey,paras),GetStr("Bnt_确认"),BntOK,null,null,null,null,true);
        }
        /// <summary>
        /// 语法糖对话框调用函数
        /// </summary>
        /// <param name="descKey"></param>
        /// <param name="BntOK"></param>
        /// <param name="paras"></param>
        public new void ShowOK(string descKey, Callback BntOK, params object[] paras)
        {
            Show("Information", GetStr(descKey, paras), GetStr("Bnt_确认"), BntOK, null, null, null, null, true);
        }
        /// <summary>
        /// 语法糖
        /// </summary>
        /// <param name="key"></param>
        /// <param name="descKey"></param>
        /// <param name="BntOK"></param>
        /// <param name="BntCancle"></param>
        /// <param name="paras"></param>
        public new void ShowOKCancle(string key, string descKey, Callback BntOK, Callback BntCancle=null, params object[] paras)
        {
            Show(GetStr(key), GetStr(descKey, paras), GetStr("Bnt_确认"), BntOK, GetStr("Bnt_取消"), BntCancle, null, null, true);
        }
        public void ShowOKCancle(string descKey, Callback BntOK, Callback BntCancle=null, params object[] paras)
        {
            Show("Information", GetStr(descKey, paras), GetStr("Bnt_确认"), BntOK, GetStr("Bnt_取消"), BntCancle, null, null, true);
        }
        /// <summary>
        /// 原始的模式对话框调用函数
        /// </summary>
        /// <param name="title"></param>
        /// <param name="desc"></param>
        /// <param name="BntStr1"></param>
        /// <param name="Bnt1"></param>
        /// <param name="BntStr2"></param>
        /// <param name="Bnt2"></param>
        /// <param name="BntStr3"></param>
        /// <param name="Bnt3"></param>
        /// <param name="isCanClose"></param>
        public void Show(string title,string desc,string BntStr1="None", Callback Bnt1 = null, string BntStr2="None", Callback Bnt2 = null, string BntStr3="None",  Callback Bnt3=null, bool isCanClose = true)
        {
            if(IsShow)
            {
                CLog.Error("ModalBox不能重复打开!!");
                return;
            }
            Show(true);
            InputTitle = title;
            InputDesc = desc;
            InputBntStr1 = BntStr1;
            InputBntStr2 = BntStr2;
            InputBntStr3 = BntStr3;
            Callback_Bnt1 = Bnt1;
            Callback_Bnt2 = Bnt2;
            Callback_Bnt3 = Bnt3;
            this.Bnt1?.Show(!BntStr1.IsInvStr());
            this.Bnt2?.Show(!BntStr2.IsInvStr());
            this.Bnt3?.Show(!BntStr3.IsInvStr());
            BntClose.Show(isCanClose);

        }
        #endregion

        #region get
        protected override string GetTitle()
        {
            return InputTitle;
        }
        #endregion

        #region Utile

        #endregion

        #region Callback
        void OnClickBnt1(BasePresenter presenter, PointerEventData data)
        {
            Callback_Bnt1?.Invoke();
            Show(false);
        }
        void OnClickBnt2(BasePresenter presenter, PointerEventData data)
        {
            Callback_Bnt2?.Invoke();
            Show(false);
        }
        void OnClickBnt3(BasePresenter presenter, PointerEventData data)
        {
            Callback_Bnt3?.Invoke();
            Show(false);
        }
        #endregion
    }

}