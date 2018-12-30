//------------------------------------------------------------------------------
// CursorConfig.cs
// Copyright 2018 2018/11/1 
// Created by CYM on 2018/11/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;

namespace CYM
{
    [CreateAssetMenu]
    public class CursorConfig : ScriptableObjectConfig<CursorConfig>
    {
        #region inspector
        public List<Texture2D> Normal = new List<Texture2D>();
        public List<Texture2D> Wait = new List<Texture2D>();
        public List<Texture2D> Press = new List<Texture2D>();
        public AudioClip PressSound;
        #endregion

        public override void OnInited()
        {
        }
    }
}