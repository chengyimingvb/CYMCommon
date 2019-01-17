//**********************************************
// Class Name	: UIShowTransition
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
using DG.Tweening;

namespace CYM.UI
{
	public class UIShowTransition :UITransition
	{
        #region inspector
        [SerializeField]
        float OpenDuration = 0.2f;
        [SerializeField]
        float CloseDuration = 0.2f;
        [SerializeField]
        public Ease InEase = Ease.OutBack;
        [SerializeField]
        public Ease OutEase = Ease.InBack;
        #endregion

        protected Tweener tweener;

        #region set
        public override void OnShow(bool b,bool isActiveByShow)
        {
            if (tweener != null)
                tweener.Kill();
            Duration = GetDuration(b);
            base.OnShow(b, isActiveByShow);
        }
        #endregion

        #region get
        /// <summary>
        /// 获得开/关闭的Duration
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        float GetDuration(bool b)
        {
            if (b)
            {
                return OpenDuration;
            }
            else
            {
                return CloseDuration;
            }
        }
        protected Ease GetEase(bool b)
        {
            if (b)
            {
                return InEase;
            }
            else
            {
                return OutEase;
            }
        }
        #endregion

        #region
        /// <summary>
        /// 手动设置,在Tween完毕的时候Dective
        /// </summary>
        protected void OnTweenComplete()
        {
            if (!Presenter.IsShow)
            {
                if (Presenter.IsActiveByShow)
                    Presenter.SetActive(false);
            }
        }
        #endregion

        #region Inspector Editor
        protected override bool Inspector_HideDuration()
        {
            return true;
        }
        #endregion
    }
}