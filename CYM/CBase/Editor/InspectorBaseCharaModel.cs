//------------------------------------------------------------------------------
// InspectorBT.cs
// Copyright 2018 CopyrightHolderName 
// Created by CYM on 2018/2/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

using CYM;
using CYM.UI;
using CYM.AI;
using Tree = CYM.AI.Tree;
namespace CYM
{
    [CustomEditor(typeof(BaseCharaModel), true)]
    public class InspectorBaseCharaModel : Editor
    {
        BaseCharaModel charaModel;

        #region AI
        Tree Tree;
        Node Root;
        int rootInternel = 0;
        GUIStyle succ = new GUIStyle();
        GUIStyle reset = new GUIStyle();
        GUIStyle fail = new GUIStyle();
        GUIStyle run = new GUIStyle();
        #endregion

        #region bone
        BaseBone[] Bones;
        #endregion

        #region Attr
        Dictionary<string, float> Attr = new Dictionary<string, float>();
        #endregion

        void OnEnable()
        {
            //获取当前编辑自定义Inspector的对象
            charaModel = (BaseCharaModel)target;
            InitBT();
            InitBone();
            InitAttr();
        }
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if (charaModel == null)
                return;
            DrawBT();
            DrawBone();
            DrawAttr();
        }

        #region Attr
        void InitAttr()
        {
            RefreshAttr();
        }
        void DrawAttr()
        {
            if (Attr == null)
                return;
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            BasePreviewUtile.Header("属性:");
            foreach(var item in Attr)
            {
                EditorGUILayout.LabelField(item.Key+":"+ BaseUIUtils.OptionalTwoDigit(item.Value));
            }

            if (GUILayout.Button("刷新"))
            {
                RefreshAttr();
            }

            EditorGUILayout.EndVertical();
        }
        void RefreshAttr()
        {
            Attr = charaModel.GetAttr();
        }
        #endregion

        #region bone
        void InitBone()
        {
            Bones = charaModel.GetComponentsInChildren<BaseBone>();
        }
        void DrawBone()
        {
            if (Bones == null)
                return;
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            BasePreviewUtile.Header("角色骨骼信息:");
            for (int i=0;i<Bones.Length;++i)
            {
                if (Bones[i].Type != NodeType.None)
                {
                    EditorGUILayout.ObjectField(new GUIContent(Bones[i].Type.ToString()), Bones[i].transform, typeof(Transform), true);
                }
                else
                {
                    EditorGUILayout.ObjectField(new GUIContent(Bones[i].ExtendName.ToString()), Bones[i].transform, typeof(Transform), true);
                }
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region BT
        void DrawNode(Node node,int internel=0,bool isDraw=true)
        {
            string appendStr = "";
            if (node is Decision)
            {
                var d = node as Decision;
                foreach (var item in d.Children)
                {
                    if (!item.IsNewLine && node.IsDrawChildren)
                        appendStr += " " + item.FinalInspectorStr;
                }
            }

            if (isDraw&&!node.IsHide)
                EditorGUILayout.LabelField(GetSpace(internel) + node.FinalInspectorStr+ appendStr, GetStyle(node));
            else
                internel -= 1;

            if (node is Decision)
            {
                var d = node as Decision;
                foreach (var item in d.Children)
                {
                    DrawNode(item, internel+1, node.IsDrawChildren&& item.IsNewLine);
                }
            }
        }

        string GetSpace(int internel)
        {
            string str = "";
            for (int i = 0; i < internel; ++i)
            {
                str += "   ";
            }
            return str;
        }

        GUIStyle GetStyle(Node node)
        {
            if (node.Status == Status.Reset)
                return reset;
            else if (node.Status == Status.Run)
                return run;
            else if (node.Status == Status.Succ)
                return succ;
            else if (node.Status == Status.Fail)
                return fail;
            return null;
        }

        void DrawBT()
        {
            if (Tree != null && Tree.Root != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical();
                BasePreviewUtile.Header("行为树:");
                rootInternel = 0;
                DrawNode(Root);
                EditorGUILayout.EndVertical();
            }
        }
        void InitBT()
        {
            Tree = charaModel.GetTree();
            if (Tree != null)
            {
                Root = Tree.Root;
                Tree.ExcudeAllNodeForInspector();
                succ.richText = true;
                succ.normal.textColor = Color.green;
                reset.richText = true;
                reset.normal.textColor = Color.grey;
                fail.richText = true;
                fail.normal.textColor = Color.red;
                run.richText = true;
                run.normal.textColor = Color.yellow;
            }
        }
        #endregion

    }
}