using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
using CYM;
using UnityEngine.Events;
using System;

namespace CYM.UI
{
    public class BaseTextData : PresenterData
    {
        /// <summary>
        /// 是否自动翻译
        /// </summary>
        public bool IsTrans = true;
        public Func<string> Name = () => { return "None"; };
        public string GetName()
        {
            if (IsTrans)
                return BaseLanguageMgr.Get(Name.Invoke());
            return Name.Invoke();
        }
    }
   
    [RequireComponent(typeof(Text))]
    public class BaseText : Presenter<BaseTextData>
    {
        #region 组建
        /// <summary>
        /// 组建
        /// </summary>
        public Text Text;
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region set
        public void Refresh(string key,params object[] objs)
        {
            Text.text = BaseLanguageMgr.Get(key,objs);
        }
        public void Refresh(string desc)
        {
            Text.text = desc;
        }
        public override void Refresh()
        {
            base.Refresh();
            Text.text = Data.GetName();
        }
        public void SetText(string key,params string[] ps)
        {
            Text.text = BaseLanguageMgr.Get(key,ps);
        }
        public void SetTextStr(string str)
        {
            Text.text = str;
        }
        #endregion

        #region wrap
        public string text
        {
            get { return Text.text; }
            set { Text.text = value; }
        }
        #endregion
    }
}
