//**********************************************
// Class Name	: HelpItem
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
namespace CYM.UI
{
    public class BaseHelpItemData: PresenterData
    {
        //public BaseTextData Info;
        public BaseButtonData Close;
    }

    public class BaseHelpItem : Presenter<BaseHelpItemData>
    {
        #region inspector
        [SerializeField]
        BaseText Info;
        [SerializeField]
        BaseButton Close;
        #endregion

        #region prop
        Timer timer = new Timer();
        float Duration = 0.0f;
        #endregion

        #region life
        public override void Init(BaseHelpItemData data)
        {
            base.Init(data);
            Close.Init(data.Close);
            //Info.Init(data.Info);
        }
        private void Update()
        {
            if(IsShow&&IsInitedShow)
            {
                if (timer.Elapsed() > Duration)
                {
                    Show(false);
                    timer.Restart();
                }

            }
        }
        #endregion

        #region set
        public override void Show(bool b, bool isForce = false)
        {
            base.Show(b, isForce);
        }
        /// <summary>
        /// 显示帮助内容
        /// </summary>
        /// <param name="info"></param>
        /// <param name="duration">如果不为null 则在指定时间后自动关闭</param>
        public void Show(string key,float? duration=null,params object[] ps)
        {
            Show(true);
            Info.text = GetStr(key,ps);
            if (duration.HasValue)
                Duration = duration.Value;
            else
                Duration = float.MaxValue;
            timer.Restart();
        }
        /// <summary>
        /// 显示字符
        /// </summary>
        public void ShowStr(string str, float? duration = null)
        {
            Show(true);
            Info.text = str;
            if (duration.HasValue)
                Duration = duration.Value;
            else
                Duration = float.MaxValue;
            timer.Restart();
        }
        #endregion
    }
}