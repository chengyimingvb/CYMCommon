using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BaseToggle : BaseButton
    {
        #region inspector
        [SerializeField]
        Image ActiveImage;
        #endregion

        /// <summary>
        /// 设置选中状态
        /// </summary>
        /// <param name="b"></param>
        public virtual void Selected(bool b)
        {
            if (ActiveImage != null)
            {
                if (b)
                {
                    ActiveImage.CrossFadeAlpha(1.0f,0.3f,true);
                }
                else
                {
                    ActiveImage.CrossFadeAlpha(0.0f, 0.3f, true);
                }
            }
        }
    }

}