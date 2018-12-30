//------------------------------------------------------------------------------
// BaseScrollPresenterEditor.cs
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
    [CustomEditor(typeof(BaseScroll))]
    public class BaseScrollPresenterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //GUILayout.BeginVertical();
            //if (GUILayout.Button("自动适应"))
            //{

            //}
            //GUILayout.EndVertical();
        }
    }
}