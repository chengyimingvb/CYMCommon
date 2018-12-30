//------------------------------------------------------------------------------
// BaseProcessView.cs
// Copyright 2018 2018/3/22 
// Created by CYM on 2018/3/22
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM.UI
{
    public class BaseProcessView : BaseUIView
    {
        [SerializeField]
        BaseProgress ProgressBar;
        float curAmount = 0.0f;

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            curAmount += Time.deltaTime;
            ProgressBar.Fill.fillAmount = curAmount;
            if (curAmount >= 1.0f)
            {
                curAmount = 0.0f;
            }
        }
        #endregion

        #region set
        public void Show(string key=null)
        {
            curAmount = 0.0f;
            if (key == null)
            {
                ProgressBar.Text.text = "Please Wait..";
            }
            else
            {
                ProgressBar.Text.text = BaseLanguageMgr.Get(key);
            }
            base.Show(true);
        }
        #endregion
    }
}