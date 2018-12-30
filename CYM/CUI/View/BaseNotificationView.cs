//**********************************************
// Class Name	: LoadingView
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
using System.Collections.Generic;
using System.Linq;
using System;

namespace CYM.UI
{
    public class BaseNotificationView : BaseUIView
    {
        #region inspector
        [SerializeField]
        BaseText Desc;
        #endregion

        #region prop
        Timer Timer;
        SortedDictionary<string,Tuple<string,string>> DescCache = new SortedDictionary<string, Tuple<string, string>>();
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            Title.CancleInit();
            Timer = new Timer();
        }
        public override void OnEnable()
        {
            base.OnEnable();

        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (DescCache.Count > 0&&!IsShow)
            {
                Show(true);
                Timer.Restart();
                var temp = DescCache.FirstOrDefault();
                Desc.Refresh(temp.Value.Item2);
                Title.Refresh(temp.Value.Item1);
                DescCache.Remove(temp.Key);
            }
            else if (Timer.Elapsed()>2.0f)
            {
                Timer.Restart();
                if (DescCache.Count > 0)
                {
                    Timer.Restart();
                    var temp = DescCache.FirstOrDefault();
                    Desc.Refresh(temp.Value.Item2);
                    Title.Refresh(temp.Value.Item1);
                    DescCache.Remove(temp.Key);
                }
                else if (IsShow)
                {
                    Show(false);
                }
            }
        }
        public override void Destroy()
        {

            base.Destroy();
        }
        public override void Refresh()
        {
            //base.Refresh();
            //刷新工作
        }
        public void Show(string descKey,params object[] ps)
        {
            //Show(true);
            if(!DescCache.ContainsKey(descKey))
                DescCache.Add(descKey,new Tuple<string, string>("Notification", GetStr(descKey,ps)));
        }
        public void Show(string titleKey, string descKey, params object[] ps)
        {
            if (!DescCache.ContainsKey(descKey))
                DescCache.Add(descKey, new Tuple<string, string>(GetStr(titleKey), GetStr(descKey, ps)));
        }

        #endregion
    }

}