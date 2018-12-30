//**********************************************
// Class Name	: CYMBaseHUDText
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine.UI;
using UnityEngine;
namespace CYM.UI
{
    public class BaseHUDText : BaseHUDItem
    {
        #region inspector
        [SerializeField]
        Text Text;
        #endregion

        #region methon
        public override void Init(object text,Transform followObj)
        {
            base.Init(text, followObj);
            if(Text != null)
                Text.text = (string)text;
        }
        protected override void Update()
        {
            base.Update();
            if(Text!=null)
                Text.color = Color;
        }
        #endregion
    }
}