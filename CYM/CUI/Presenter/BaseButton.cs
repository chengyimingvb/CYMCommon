using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
namespace CYM.UI
{
    public class BaseButtonData: BaseTextData
    {
        public Func<Sprite> Icon = () => { return null; };
    }

    public class BaseButton : Presenter<BaseButtonData>
    {
        #region 组建
        public Text Text;
        public Image Icon;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Text != null)
            {
                Text.text = Data.GetName();
            }
            if (Icon != null&& Data.Icon!=null)
            {
                Icon.sprite = Data.Icon.Invoke();
            }
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
