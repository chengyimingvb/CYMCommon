//------------------------------------------------------------------------------
// BaseAlertItem.cs
// Copyright 2019 2019/3/2 
// Created by CYM on 2019/3/2
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;
using UnityEngine.UI;
namespace CYM
{
    public class BaseAlertItem : BaseButton
    {
        [FoldoutGroup("Inspector")]
        public Image AlertBg;
    }
}