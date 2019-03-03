using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using CYM;
public class UIPackingTagWindow : EditorWindow
{

    static Vector2 scrollPos = Vector2.zero;

    static private List<string> IconList = new List<string>();
    static private List<string> UIList = new List<string>();

    static int FoldoutIndex = 0;

    public static void CheckUI()
    {
        StartCheck(ref IconList, BaseConstMgr.Path_Bundles);
        StartCheck(ref UIList, BaseConstMgr.Path_AtrUI);

        List<string> newTemp = new List<string>();
        newTemp.AddRange(IconList);
        newTemp.AddRange(UIList);

        foreach (string path in newTemp)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.filterMode = FilterMode.Bilinear;
            ti.crunchedCompression = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
    }

    static private void StartCheck(ref List<string> list,string path)
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

    static private void ShowList(int foldOut,string name,List<string> list)
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
                EditorGUILayout.LabelField("Filter : " + ti.filterMode);
                EditorGUILayout.LabelField("Crunch : " + ti.crunchedCompression);
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
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("检查UI"))
        {
            UIPackingTagWindow.CheckUI();
        }
        EditorGUILayout.EndHorizontal();
        ShowList(0,"Icon", IconList);
        ShowList(1,"UI", UIList);
    }
}
