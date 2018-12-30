#if UNITY_5 || UNITY_2017
#define WEB_WINDOW_SUPPORTED
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace CYM.Utile
{
    public class ConfigWindow : EditorWindow
    {
        GameConfig config => GameConfig.Ins;

        private const string WINDOW_TITLE = "Feedback Configuration";
        private const int WIDTH = 312;
        private const int HEIGHT = 132;

        private Trello trello;
        private string lastValidToken;

        public static void Init()
        {
            // get existing window or make a new one
            ConfigWindow window = GetWindow<ConfigWindow>(true, WINDOW_TITLE);

            // set window size
            window.maxSize = new Vector2(WIDTH, HEIGHT);
            window.minSize = window.maxSize;
        }


        private void OnGUI()
        {
            // show Trello API token field
            config.FBToken = EditorGUILayout.TextField("Token", config.FBToken);
            config.FBBoard = EditorGUILayout.TextField("Board", config.FBBoard);
            // direct user to Trello API auth page if the window isn't already open
            if (GUILayout.Button("Get Trello API Token"))
            {
                // prompt user to auth via browser
                Application.OpenURL(Trello.AuthURL);
            }
            if (!config.FBToken.IsInvStr()&& !config.FBBoard.IsInvStr())
            {
                if (GUILayout.Button("Save"))
                {
                    trello = new Trello();
                    var boards = trello.GetBoards();
                    if (boards != null&&boards.Length>0)
                    {
                        foreach (var item in boards)
                        {
                            if (item.name == config.FBBoard)
                            {
                                var labs = trello.GetLabels(item.id);
                                config.FBLabelID = labs[0].id;

                                var lists = trello.GetLists(item.id);
                                if (lists != null && lists.Length > 0)
                                {
                                    foreach (var item2 in lists)
                                    {
                                        if (item2.name == "Feedback")
                                        {
                                            config.FBListID = item2.id;
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogError("没有List:Feedback");
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("没有找到Board:"+config.FBBoard);
                    }
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssets();
                }
            }
        }


    }
}

