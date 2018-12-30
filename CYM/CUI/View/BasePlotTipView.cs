//------------------------------------------------------------------------------
// BasePlotTipView.cs
// Copyright 2018 2018/5/4 
// Created by CYM on 2018/5/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BasePlotTipView : BaseUIView
    {
        #region inspector
        [SerializeField]
        BaseText Desc;
        #endregion

        #region prop
        Timer Timer=new Timer(5);
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsShow)
            {
                if (Timer.CheckOverOnce())
                {
                    Show(false);
                }
            }
        }
        public void Show(string descKey,float delay=2.0f, params string[] ps)
        {
            Timer.Restart();
            Show(true,null, delay);
            Desc.SetText(descKey,ps);
        }
        #endregion
    }
}