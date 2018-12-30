//------------------------------------------------------------------------------
// BaseDupplicateEditor.cs
// Copyright 2018 2018/3/2 
// Created by CYM on 2018/3/2
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;

namespace CYM.UI
{
    [CustomEditor(typeof(BaseDupplicate),true)]
    public class BaseDupplicateEditor : Editor
    {
        BaseDupplicate dp;
        private void OnEnable()
        {
            dp = target as BaseDupplicate;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.BeginVertical();
            if (GUILayout.Button("自动适应"))
            {
                dp.AutoFix();
            }
            GUILayout.EndVertical();
        }
    }
}