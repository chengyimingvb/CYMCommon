using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using Sirenix.OdinInspector;

namespace CYM.UI
{
    public class BaseButtonData: BaseTextData
    {
    }

    public class BaseButton : Presenter<BaseButtonData>
    {
        #region 组建
        [FoldoutGroup("Inspector")]
        public Text Text;
        [FoldoutGroup("Inspector"), Tooltip("可以位空")]
        public Image Icon;
        [FoldoutGroup("Inspector"), Tooltip("可以位空")]
        public Image Bg;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Text != null)
                Text.text = Data.GetName();
            if (Icon != null)
                Icon.sprite = Data.GetIcon();
            if (Bg != null)
                Bg.sprite = Data.GetBg();
        }
        #endregion

        #region editor
        public override void AutoSetup()
        {
            base.AutoSetup();        
            if (Text == null)
                Text = GetComponentInChildren<Text>();
        }
        #endregion
    }
}
