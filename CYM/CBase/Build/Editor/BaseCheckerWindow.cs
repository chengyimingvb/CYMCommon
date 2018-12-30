using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CYM
{
    public class BaseCheckerWindow : EditorWindow
    {
        List<BaseBuildChecker> Checkers = new List<BaseBuildChecker>();
        void OnEnable()
        {
            titleContent.text = "资源检查";
            //Checkers.Add(new Checker_Localization());
            //Checkers.Add(new Checker_Lua());
            //Checkers.Add(new Checker_ResourcesSize());
            AddChecker();
        }
        void OnDisable()
        {
            Checkers.Clear();
        }
        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("检查所有"))
            {
                CheckAll();
            }
            foreach(var item in Checkers)
            {
                if (GUILayout.Button(item.GetType().Name))
                {
                    item.Check();
                    item.Dispose();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public string CheckAll()
        {
            BaseBuildChecker.ClearLog();
            foreach (var item in Checkers)
            {
                item.Check();
                item.Dispose();
                //Debug.ClearDeveloperConsole();
                //Debug.LogError(BaseBuildChecker.ErrorStr);
            }
            return BaseBuildChecker.ErrorStr;
        }
        public bool IsHaveError()
        {
            return BaseBuildChecker.ErrorStr != "";
        }

        #region

        protected virtual void AddChecker()
        {

        }
        #endregion

    }
}
