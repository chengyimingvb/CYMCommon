//------------------------------------------------------------------------------
// BaseRichText.cs
// Copyright 2018 2018/3/23 
// Created by CYM on 2018/3/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;

namespace CYM.UI
{
    [RequireComponent(typeof(RichText))]
    public class BaseRichText :BaseText
    {
        protected override void Awake()
        {
            base.Awake();
            RichText = (RichText)Text;
        }

        public RichText RichText
        {
            get;private set;
        }
    }
}