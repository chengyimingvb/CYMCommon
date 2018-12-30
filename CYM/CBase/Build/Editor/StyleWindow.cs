using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using System.Reflection;

namespace CYM
{

    public class StyleWindows : EditorWindow
    {

        static List<GUIStyle> styles = null;

        public Vector2 scrollPosition = Vector2.zero;
        private void OnEnable()
        {
            titleContent =new GUIContent( "Style");
            styles = new List<GUIStyle>();
            foreach (PropertyInfo fi in typeof(EditorStyles).GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                object o = fi.GetValue(null, null);
                if (o.GetType() == typeof(GUIStyle))
                {
                    styles.Add(o as GUIStyle);
                }
            }
        }
        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < styles.Count; i++)
            {
                GUILayout.Label("EditorStyles." + styles[i].name, styles[i]);
            }
            GUILayout.EndScrollView();
        }
    }
}