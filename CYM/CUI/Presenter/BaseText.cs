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
using Sirenix.OdinInspector;

namespace CYM.UI
{
    public class BaseTextData : PresenterData
    {
        public Func<Sprite> ActiveIcon = null;
        public string ActiveIconStr = BaseConstMgr.STR_Inv;
        public Func<Sprite> Icon = null;
        public string IconStr = BaseConstMgr.STR_Inv;
        public Func<Sprite> Bg = null;
        public string BgStr = BaseConstMgr.STR_Inv;
        public bool IsTrans = true;
        public Func<string> Name = null;
        public string NameStr = BaseConstMgr.STR_Inv;
        public string GetName()
        {
            string str = NameStr;
            if (Name != null)
            {
                str = Name.Invoke();
            }
            return GetTransStr(str);
        }
        public Sprite GetIcon()
        {
            if (Icon != null)
            {
                return Icon.Invoke();
            }
            if (!IconStr.IsInvStr())
                return BaseGRMgr.GetIcon(IconStr);
            return null;
        }
        public Sprite GetActiveIcon()
        {
            if (ActiveIcon != null)
            {
                return ActiveIcon.Invoke();
            }
            if (!ActiveIconStr.IsInvStr())
                return BaseGRMgr.GetIcon(ActiveIconStr);
            return null;
        }
        public Sprite GetBg()
        {
            if (Bg != null)
            {
                return Bg.Invoke();
            }
            if (!BgStr.IsInvStr())
                return BaseGRMgr.GetIcon(BgStr);
            return null;
        }
        public string GetTransStr(string str)
        {
            if(IsTrans)
                return BaseLanguageMgr.Get(str);
            return str;
        }
    }
   
    [RequireComponent(typeof(Text))]
    public class BaseText : Presenter<BaseTextData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), SerializeField]
        public Text Text;
        [FoldoutGroup("Inspector"), SerializeField,Tooltip("可以位空")]
        public Image Icon;
        [SerializeField]
        bool IsAnim = false;
        #endregion

        #region prop
        Tween Tween;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            text = Data.GetName();
            if (Icon != null)
            {
                Icon.sprite = Data.GetIcon();
            }
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
