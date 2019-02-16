//------------------------------------------------------------------------------
// EffectScale.cs
// Copyright 2019 2019/1/25 
// Created by CYM on 2019/1/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using DG.Tweening;

namespace CYM
{
    public class EffectScale : BaseMono
    {
        [SerializeField]
        float Duration = 0.5f;
        [SerializeField]
        Ease Ease = Ease.Linear;

        Tween tween;

        public override void OnEnable()
        {
            base.OnEnable();
            Trans.localScale = Vector3.one * 0.1f;
            if (tween!=null) tween.Kill();
            tween =Trans.DOScale(Vector3.one, Duration).SetEase(Ease);
        }
    }
}