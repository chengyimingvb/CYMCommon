//------------------------------------------------------------------------------
// BaseHUDBubble.cs
// Copyright 2018 2018/2/28 
// Created by CYM on 2018/2/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BaseHUDBubble : BaseHUDText
    {
        [SerializeField]
        Image Bg;

        CanvasGroup Group;
        protected override void Awake()
        {
            base.Awake();
            Group = BaseMono.GetUnityComponet<CanvasGroup>(GO);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            Group.alpha = Color.a;
        }
    }
}