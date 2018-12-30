//------------------------------------------------------------------------------
// UIShowAlphaSimpleTransition.cs
// Copyright 2018 2018/11/29 
// Created by CYM on 2018/11/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using DG.Tweening;
using UnityEngine.UI;
using System;

namespace CYM
{
    public class UIShowAlphaSimpleTransition : UIShowTransition
    {
        #region prop
        public float From = 0.5f;
        public float To = 1.0f;
        Color Color;
        #endregion

        protected override void Awake()
        {
            base.Awake();
            Color = Graphic.color;
        }

        public override void OnShow(bool b, bool isActiveByShow)
        {
            base.OnShow(b, isActiveByShow);
            if (RectTrans != null)
            {
                if (Graphic == null)
                    Graphic = RectTrans.GetComponent<Graphic>();

                if (IsReset)
                {
                    Color.a = b ? From : To;
                }

                tweener = DOTween.To(() => Color.a, x => Color.a = x, b ? To : From, Duration)
                    .SetEase(GetEase(b))
                    .OnComplete(OnTweenComplete)
                    .SetDelay(Delay)
                    .OnUpdate(OnAlphaTweenUpdate)
                    .OnStart(OnAlpahTweenStart);
            }
        }

        private void OnAlpahTweenStart()
        {
            Color = Graphic.color;
        }

        private void OnAlphaTweenUpdate()
        {
            Graphic.color = Color;
        }
    }
}