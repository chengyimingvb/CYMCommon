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
        [SerializeField]
        bool IsAnim = false;
        [SerializeField]
        protected Text Text;
        #endregion

        #region prop
        Tween Tween;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            text = Data.GetName();

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
        public void SetText(string key,params string[] ps)
        {
            text = BaseLanguageMgr.Get(key,ps);
        }
        public void SetTextStr(string str)
        {
            text = str;
        }
        #endregion

        #region wrap
        public string text
        {
            get { return Text.text; }
            set
            {
                if (IsAnim)
                {
                    if (Tween != null)
                        Tween.Kill();
                    Tween = DOTween.To(() => Text.text, (x) => Text.text = x, value, 0.3f);
                }
                else
                {
                    Text.text = value;
                }
            }
        }
        public bool IsAnimation
        {
            get { return IsAnim; }
            set { IsAnim = value; }
        }
        #endregion
    }
}
