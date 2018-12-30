//**********************************************
// Class Name	: HelpView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
using CYM;
using CYM.UI;
using UnityEngine.EventSystems;
namespace CYM.UI
{
    public class BaseHelpView : BaseUIView
    {
        #region presenter
        [SerializeField]
        public BaseHelpItem Top;
        [SerializeField]
        public BaseHelpItem Bot;
        #endregion

        #region life
        public override void Init()
        {
            base.Init();
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Top.Init(new BaseHelpItemData(){
                Close=new BaseButtonData() { OnClick=(x,y)=> Top.Show(false)},
                //Info=new BaseTextData() { }
            });
            Bot.Init(new BaseHelpItemData()
            {
                Close = new BaseButtonData() { OnClick = (x, y) => Bot.Show(false) },
                //Info = new BaseTextData() { }
            });
        }
        public override void Refresh()
        {
            base.Refresh();
            //刷新工作
        }
        public override void Show(bool b=true, float? fadeTime = null, float delay = 0, bool useGroup = true, bool force = false)
        {
            base.Show(b, fadeTime, delay, useGroup, force);
            if (!IsShow)
            {
                Top.Show(false);
                Bot.Show(false);
            }
        }
        #endregion

        #region set
        public void ShowBot(string key,float time = 60,params string[] ps)
        {
            Bot.ShowStr(GetStr(key,ps), time);
        }
        public void CloseBot()
        {
            Bot.Show(false);
        }
        #endregion

        #region Callback

        #endregion

    }

}