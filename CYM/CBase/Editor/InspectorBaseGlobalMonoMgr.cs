//------------------------------------------------------------------------------
// InspectorBaseGlobalMonoMgr.cs
// Copyright 2018 2018/6/1 
// Created by CYM on 2018/6/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;

namespace CYM
{
    [CustomEditor(typeof(BaseGlobalMonoMgr), false)]
    public class InspectorBaseGlobalMonoMgr : Editor
    {
        BaseGlobalMonoMgr BaseGlobalMonoMgr;

        private void OnEnable()
        {
            BaseGlobalMonoMgr = (BaseGlobalMonoMgr)target;
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if (BaseGlobalMonoMgr == null)
                return;
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Update Count:", BaseGlobalMonoMgr.UpdateIns.Count.ToString());
            EditorGUILayout.LabelField("FixedUpdate Count:", BaseGlobalMonoMgr.FixedUpdateIns.Count.ToString());
            EditorGUILayout.LabelField("LateUpdate Count:", BaseGlobalMonoMgr.LateUpdateIns.Count.ToString());
            EditorGUILayout.LabelField("GUI Count:", BaseGlobalMonoMgr.GUIIns.Count.ToString());
            EditorGUILayout.EndVertical();
        }
    }
}