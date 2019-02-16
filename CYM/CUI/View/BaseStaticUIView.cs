//------------------------------------------------------------------------------
// BaseStaticUIView.cs
// Copyright 2019 2019/2/12 
// Created by CYM on 2019/2/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseStaticUIView<T> : BaseUIView where T: BaseStaticUIView<T>
    {
        public static T Default { get; protected set; }

        public override void Awake()
        {
            base.Awake();
            if (Default == null)
                Default = this as T;
        }

        /// <summary>
        /// 设置为默认Tooltip
        /// </summary>
        public void SetAsDefault()
        {
            Default = this as T;
        }
    }
}