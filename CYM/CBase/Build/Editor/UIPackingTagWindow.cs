using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using CYM;
public class UIPackingTagWindow : EditorWindow
{

    Vector2 scrollPos = Vector2.zero;

    private List<string> IconList = new List<string>();
    private List<string> UIList = new List<string>();

    int FoldoutIndex = 0;

    private void StartCheck(ref List<string> list,string path)
    {
        list.Clear();
        List<string> withoutExtensions = new List<string>() { ".png", ".jpg" };
        string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();

        foreach (string file in files)
        {
            string strFile = file.Substring(file.IndexOf("Asset")).Replace('\\', '/');
            list.Add(strFile);
        }
    }

    private void ShowList(int foldOut,string name,List<string> list)
    {
        bool b = EditorGUILayout.Foldout(foldOut==FoldoutIndex, name);
        if (b)
        {
            FoldoutIndex = foldOut;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (string path in list)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Path : " + path, GUILayout.Width(400));
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
                EditorGUILayout.ObjectField(texture, typeof(Texture2D), false);
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                EditorGUILayout.Space();
                ti.spritePackingTag = EditorGUILayout.TextField(ti.spritePackingTag);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void OnEnable()
    {
        StartCheck(ref IconList, BaseConstMgr.Path_Bundles);
        StartCheck(ref UIList,BaseConstMgr.Path_AtrUI);
    }

    void OnGUI()
    {
        ShowList(0,"Icon", IconList);
        ShowList(1,"UI", UIList);
    }
}
