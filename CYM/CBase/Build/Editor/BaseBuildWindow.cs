using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using CYM;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using System.Reflection; 
using Sirenix.OdinInspector.Editor;
using CYM.DLC;
using CYM.UI;
namespace CYM 
{

    public class BaseBuildWindow : EditorWindow
    {

        [MenuItem("Tools/Build")]
        public static void ToolBuildWindow()
        {
            ShowWindow<BaseBuildWindow>();
        }

        [MenuItem("Tools/BuildAllConfig")]
        public static void ToolBuildAllConfig()
        {
            if (DLCConfig.Ins != null ||
                BuildConfig.Ins != null ||
                BuildLocalConfig.Ins != null ||
                GameConfig.Ins != null ||
                CursorConfig.Ins != null ||
                DLCConfig.Ins != null ||
                UIConfig.Ins!=null
                )
            {
                CLog.Info("创建所有的Config成功!!!");
            }
        }

        #region prop
        static bool IsRefreshed = false;
        static GUIStyle TitleStyle = new GUIStyle();
        static GUIStyle FoldOutStyle = new GUIStyle();
        static Dictionary<string, GUIStyle> styles = new Dictionary<string, GUIStyle>();

        static BuildConfig BuildConfig=>BuildConfig.Ins;
        static BuildLocalConfig BuildLocalConfig=> BuildLocalConfig.Ins;
        static DLCItem Native => DLCConfig.Native;
        static DLCConfig DLCConfig => DLCConfig.Ins;

        protected static Dictionary<string, string> SceneNames { get; private set; } = new Dictionary<string,  string>();

        protected static string VerticalStyle = "HelpBox";
        protected static string ButtonStyle = "minibutton";
        protected static string FoldStyle = "FoldOutPreDrop;";
        protected static string SceneButtonStyle = "ButtonMid;";
        #endregion

        #region life
        protected void OnEnable()
        {
            EnsureProjectFiles();
            RefreshData();
        }
        void RefreshData()
        {
            titleContent.text = "开发界面";
            TitleStyle.fixedWidth = 100;
            
            RefreshSceneNames();
            CLog.Info("打开开发者界面");   
        }
        void DrawGUI()
        {
            Present_Main();
            Present_DLC();
            Present_Setting();
            Present_Explorer();
            Present_ExpressSetup();
            Present_SubWindow();
            Present_LevelList();
            Present_ScriptTemplate();
            Present_Other();
        }
        #endregion

        #region 构建
        void Present_Main()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_Main = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_Main, "构建",true))
            {    
                BuildConfig.Platform = (Platform)EditorGUILayout.Popup("目标", (int)BuildConfig.Platform,Enum.GetNames(typeof(Platform)));
                BuildConfig.Distribution = (Distribution)EditorGUILayout.EnumPopup("发布平台", BuildConfig.Distribution);
                EditorGUILayout.LabelField(string.Format("版本号预览:{0}", BuildConfig));
                EditorGUILayout.LabelField(string.Format("完整版本号预览:{0}", BuildConfig.FullVersionName));
                BuildConfig.Name = EditorGUILayout.TextField("名称", BuildConfig.Name);
                BuildConfig.SubTitle = EditorGUILayout.TextField("副标题", BuildConfig.SubTitle);
                BuildConfig.Major = EditorGUILayout.IntField("主版本", BuildConfig.Major);
                BuildConfig.Minor = EditorGUILayout.IntField("副版本", BuildConfig.Minor);
                BuildConfig.Data = EditorGUILayout.IntField("数据库版本", BuildConfig.Data);
                BuildConfig.Prefs = EditorGUILayout.IntField("Prefs", BuildConfig.Prefs);

                EditorGUILayout.BeginHorizontal();
                BuildConfig.Tag = (VersionTag)EditorGUILayout.EnumPopup("后缀", BuildConfig.Tag);
                BuildConfig.Suffix = EditorGUILayout.IntField(BuildConfig.Suffix);
                EditorGUILayout.EndHorizontal();

                BuildConfig.BuildType = (BuildType)EditorGUILayout.EnumPopup("打包版本", BuildConfig.BuildType);

                BuildConfig.IgnoreChecker = EditorGUILayout.Toggle("忽略检查", BuildConfig.IgnoreChecker);
                bool preDevelopmentBuild = BuildConfig.IsUnityDevelopmentBuild;
                BuildConfig.IsUnityDevelopmentBuild = EditorGUILayout.Toggle("UnityDevBuild", BuildConfig.IsUnityDevelopmentBuild);
                if (preDevelopmentBuild != BuildConfig.IsUnityDevelopmentBuild)
                {
                    EditorUserBuildSettings.development = BuildConfig.IsUnityDevelopmentBuild;
                }

                if (PlayerSettings.bundleVersion != BuildConfig.ToString())
                    PlayerSettings.bundleVersion = BuildConfig.ToString();


                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("构建Manifest"))
                {
                    Builder.BuildManifest(Native);
                }
                if (GUILayout.Button("构建AB"))
                {
                    Builder.BuildBundle(Native);
                }
                if (GUILayout.Button("构建EXE"))
                {
                    if (CheckEorr())
                        return;
                    if (!CheckDevBuildWarring())
                        return;
                    if (!CheckAuthority())
                        return;
                    Builder.BuildEXE();
                }
                if (GUILayout.Button("构建AB&EXE"))
                {
                    if (CheckEorr())
                        return;
                    if (!CheckDevBuildWarring())
                        return;
                    if (!CheckAuthority())
                        return;
                    Builder.BuildBundleAndEXE(Native);
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("运行游戏"))
                {
                    BaseFileUtils.OpenFile(BuildConfig.ExePath);
                    CLog.Info("Run:{0}", BuildConfig.ExePath);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region DLC
        void Present_DLC()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_DLC = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_DLC, "DLC", true))
            {
                if (DLCConfig.DLCItems.Count == 0)
                {
                    EditorGUILayout.LabelField("没有DLC,请手动创建");
                }
                else
                {
                    foreach (var item in DLCConfig.DLCItems)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(item.Value.Name);
                        if (GUILayout.Button("构建Manifest"))
                        {
                            Builder.BuildManifest(item.Value);
                        }
                        if (GUILayout.Button("构建AB"))
                        {
                            Builder.BuildBundle(item.Value);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 编辑器设置
        void Present_Setting()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_Setting = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_Setting, "设置", true))
            {
                EditorGUILayout.BeginVertical();
                BuildConfig.NameSpace = EditorGUILayout.TextField("命名空间", BuildConfig.NameSpace);
                BuildLocalConfig.Ins.IsScrollBuildWindow = EditorGUILayout.Toggle("Enable Scroll", BuildLocalConfig.Ins.IsScrollBuildWindow);
                if (BuildConfig.IsDevBuild)
                {
                    BuildLocalConfig.Ins.IsEnableDevSetting = EditorGUILayout.Toggle("Is enable dev setting", BuildLocalConfig.Ins.IsEnableDevSetting);
                }
                DLCConfig.Ins.IsSimulationDLCEditor = EditorGUILayout.Toggle("Is Simulation DLC Editor", DLCConfig.Ins.IsSimulationDLCEditor);
                UIConfig.Ins.IsShowLogo = EditorGUILayout.Toggle("Is Show Logo", UIConfig.Ins.IsShowLogo);
                OnDrawSettings();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 资源管理器
        void Present_Explorer()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_Explorer = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_Explorer, "链接",true))
            {
                EditorGUILayout.LabelField(Application.persistentDataPath);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Persistent"))
                {
                    BaseFileUtils.OpenExplorer(Application.persistentDataPath);
                }
                else if (GUILayout.Button("删除 Persistent"))
                {
                    BaseFileUtils.DeletePath(Application.persistentDataPath);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Project File"))
                {
                    BaseFileUtils.OpenExplorer(BaseConstMgr.Path_Project);
                }
                else if (GUILayout.Button("Bin"))
                {
                    BaseFileUtils.OpenExplorer(BaseConstMgr.Path_Build);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                OnDrawPresentExplorer();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 关卡列表
        [HideInInspector]
        public Vector2 scrollSceneList = Vector2.zero;
        void Present_LevelList()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_SceneList = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_SceneList, "场景",true))
            {
                if(SceneNames.Count > 5)
                    scrollSceneList = EditorGUILayout.BeginScrollView(scrollSceneList, GUILayout.ExpandHeight(true), GUILayout.MinHeight(300));
                else
                    scrollSceneList = EditorGUILayout.BeginScrollView(scrollSceneList);

                EditorGUILayout.BeginHorizontal();
                OnDrawPresentScene();
                EditorGUILayout.EndHorizontal();
                if (SceneNames != null)
                {
                    foreach (var item in SceneNames)
                    {
                        if (item.Key == BaseConstMgr.SCE_Preview ||
                            item.Key == BaseConstMgr.SCE_Start ||
                            item.Key == BaseConstMgr.SCE_Test)
                            continue;
                        DrawGoToBundleSceneButton(item.Key, item.Value);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 平台
        void Present_ExpressSetup()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_ExpressSetup = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_ExpressSetup, "平台",true))
            {
                string path = BuildConfig.CurDistributionSetupPath;
                Rect dragAreaRect = new Rect();
                //获得一个长300的框  
                dragAreaRect = EditorGUILayout.GetControlRect(/*GUILayout.Width(600)*/true,18);
                //将上面的框作为文本输入框  
                BuildConfig.CurDistributionSetupPath = EditorGUI.TextField(dragAreaRect,"路径" ,BuildConfig.CurDistributionSetupPath);

                //如果鼠标正在拖拽中或拖拽结束时，并且鼠标所在位置在文本输入框内  
                if ((Event.current.type == EventType.DragUpdated
                  || Event.current.type == EventType.DragExited)
                  && dragAreaRect.Contains(Event.current.mousePosition))
                {
                    //改变鼠标的外表  
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        BuildConfig.CurDistributionSetupPath = DragAndDrop.paths[0];
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("安装到本地"))
                {
                    if (BuildConfig.CurDistributionSetupPath != null)
                    {
                        BaseFileUtils.DeletePath(BuildConfig.CurDistributionSetupPath);
                        BaseFileUtils.EnsureDirectory(BuildConfig.CurDistributionSetupPath);
                        BaseFileUtils.CopyDir(BuildConfig.DirPath, BuildConfig.CurDistributionSetupPath);
                    }
                }
                if (GUILayout.Button("打开安装目录"))
                {
                    if (BuildConfig.CurDistributionSetupPath != null)
                    {
                        BaseFileUtils.OpenExplorer(BuildConfig.CurDistributionSetupPath);
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                BuildConfig.Username = EditorGUILayout.TextField("用户名", BuildConfig.Username);
                BuildConfig.Password = EditorGUILayout.PasswordField("密码", BuildConfig.Password);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("上传"))
                {
                    if (string.IsNullOrEmpty(BuildConfig.Username) || string.IsNullOrEmpty(BuildConfig.Password))
                    {
                        EditorUtility.DisplayDialog("错误", "请先输入用户名密码", "好的");
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("重要操作", "确定要上传吗.", "上传", "取消"))
                        {
                            BuildConfig.GetBuildData(BuildConfig.Distribution).Upload();
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 代码模版
        void Present_ScriptTemplate()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_ScriptTemplate = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_ScriptTemplate, "代码模版", true))
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("创建模版"))
                {
                    Utile.XenoTemplateTool.CreateXenoTemplate(BaseConstMgr.Path_ScriptTemplate);
                }
                else if (GUILayout.Button("创建映射"))
                {
                    Utile.XenoTemplateTool.CreateXenoMappingSet(BaseConstMgr.Path_ScriptTemplate);
                }
                EditorGUILayout.EndHorizontal();
                if (GUILayout.Button("刷新"))
                {
                    Utile.XenoTemplateTool.RefreshTemplates(BaseConstMgr.Path_Editor);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 其他
        void Present_Other()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_Other = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_Other, "其他",true))
            {
                EditorGUILayout.BeginVertical();
                if (GUILayout.Button("预览场景"))
                {
                    GoToScene(GetScenesPath(BaseConstMgr.SCE_Preview), OpenSceneMode.Additive);
                }
                else if (GUILayout.Button("保存"))
                {
                    EditorUtility.SetDirty(BuildLocalConfig);
                    EditorUtility.SetDirty(BuildConfig);
                    AssetDatabase.SaveAssets();
                }
                else if (GUILayout.Button("编译"))
                {
                    AssetDatabase.Refresh();
                }
                else if (GUILayout.Button("刷新"))
                {
                    RefreshData();
                    Repaint();
                }
                else if (GUILayout.Button("运行"))
                {                    
                    AssetDatabase.Refresh(); 
                    GoToScene(GetSystemScenesPath(BaseConstMgr.SCE_Start), OpenSceneMode.Single);
                    EditorApplication.isPlaying = true;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                OnDrawPresentScriptTemplate();             
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region sub Window
        void Present_SubWindow()
        {
            EditorGUILayout.BeginVertical(VerticalStyle);
            if (BuildLocalConfig.Ins.Fold_Present_SubWindow = EditorGUILayout.Foldout(BuildLocalConfig.Ins.Fold_Present_SubWindow, "窗口", true))
            {
                if (GUILayout.Button("ScriptTemplateSettings"))
                {
                    Utile.XenoTemplateSettingsWindow.ShowWindow();
                }
                else if (GUILayout.Button("AssetImportOption"))
                {
                    ShowWindow<AssetImportOptionsWindow>();
                }
                else if (GUILayout.Button("ParticleScaler"))
                {
                    ParticleScaler.ShowWindow();
                }
                else if (GUILayout.Button("Pivot"))
                {
                    Utile.Window.OpenWindow();
                }
                else if (GUILayout.Button("Feedback"))
                {
                    Utile.ConfigWindow.Init();
                }
                else if (GUILayout.Button("Shelf"))
                {
                    Utile.ShelfEditor.OpenShelf(0);
                }
                else if (GUILayout.Button("Prefs"))
                {
                    Utile.PrefsWindow.Init();
                }
                else if (GUILayout.Button("PackingTag"))
                {
                    GetWindow<UIPackingTagWindow>("PackingTag").Show();
                }
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region utile
        protected static bool CheckDevBuildWarring()
        {
            if (BuildConfig.IsDevBuild)
            {
                return EditorUtility.DisplayDialog("警告!", "您确定要构建吗?因为当前是Dev版本", "确定要构建Dev版本", "取消");
            }
            return true;
        }
        protected static bool CheckAuthority()
        {
            //CLog.Error(SystemInfo.deviceName);
            if (SystemInfo.deviceName == "DESKTOP-DEVLK0F")
                return true;
            return false;
        }
        protected bool CheckEorr()
        {
            if (BuildConfig.IgnoreChecker)
                return false;

            if (CheckIsHaveError())
                return true;
            return false;
        }
        protected static bool DoCheckWindow<T>() where T : BaseCheckerWindow
        {
            T window = GetWindow<T>();
            window.CheckAll();
            window.Close();
            return window.IsHaveError();
        }
        protected static void ShowWindow<T>() where T : EditorWindow
        {
            GetWindow<T>().ShowPopup();
        }
        protected static string GetScenesPath(string name)
        {
            return string.Format(BaseConstMgr.Format_BundleScenesPath, name);
        }
        protected static string GetSystemScenesPath(string name)
        {
            return string.Format(BaseConstMgr.Format_BundleSystemScenesPath, name);
        }
        protected static void DrawGoToBundleSystemSceneButton(string name)
        {
            if (GUILayout.Button(name))
            {
                GoToScene(GetSystemScenesPath(name));
            }
        }
        protected static void DrawGoToBundleSceneButton(string name, string fullPath)
        {
            if (GUILayout.Button(name))
            {
                GoToSceneByFullPath(fullPath);
            }
        }
        protected static void DrawButton(string name, Callback doAction)
        {
            if (GUILayout.Button(name))
            {
                doAction?.Invoke();
            }
        }
        protected static void GoToScene(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            GoToSceneByFullPath(Application.dataPath + path, mode);
        }
        protected static void GoToSceneByFullPath(string path, OpenSceneMode mode = OpenSceneMode.Single)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(path, mode);
        }
        protected static void LookAtPos(Vector3 pos)
        {
            SceneView view = SceneView.lastActiveSceneView;
            view.LookAt(pos, view.rotation, 20);
        }
        protected static void SafeOpenJsonFile<T>(string path, T data) where T : class
        {
            BaseFileUtils.UpdateFile(path, data);
            BaseFileUtils.OpenFile(path);
        }
        protected static void RefreshSceneNames()
        {
            var paths = AssetDatabase.GetAssetPathsFromAssetBundle(BaseConstMgr.BN_Scene);
            SceneNames.Clear();
            foreach (var item in paths)
            {
                string tempName = Path.GetFileNameWithoutExtension(item);
                if (!SceneNames.ContainsKey(tempName))
                    SceneNames.Add(tempName, item);
            }
        }
        /// <summary>
        /// 确保标准项目文件夹存在
        /// </summary>
        public static void EnsureProjectFiles()
        {
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Arts);

            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Bundles);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_NoBundles);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_NoBundlesScene);
            //BaseFileUtils.EnsureDirectory(BaseConstansMgr.Path_TargetNativeDLC);

            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Resources);
            //BaseFileUtils.EnsureDirectory(BaseConstansMgr.Path_ResourcesDLC);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_ResourcesConfig);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_ScriptTemplate);

            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Funcs);
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "Editor"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "GlobalMgr"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "GlobalMono"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "TableDatas"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "UI"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "UnitMgr"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "UnitMono"));
            BaseFileUtils.EnsureDirectory(Path.Combine(BaseConstMgr.Path_Funcs, "Utils"));

            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Tests);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Gizmos);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_Editor);

            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_StreamingAssets);
            BaseFileUtils.EnsureDirectory(BaseConstMgr.Path_NativeDLC);

            Utile.XenoTemplateTool.RefreshTemplates(BaseConstMgr.Path_Editor,false);
        }
        #endregion

        #region Override
        [HideInInspector]
        public Vector2 scrollPosition = Vector2.zero;
        protected void OnGUI()
        {
            //base.DrawEditor(index);
            if (BuildConfig == null)
                return;
            if (BuildLocalConfig.Ins.IsScrollBuildWindow)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawGUI();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                DrawGUI();
            }
        }
        protected virtual void OnDrawPresentScene()
        {
            //e.x.
            DrawGoToBundleSystemSceneButton(BaseConstMgr.SCE_Start);
            DrawGoToBundleSystemSceneButton(BaseConstMgr.SCE_Preview);
            DrawGoToBundleSystemSceneButton(BaseConstMgr.SCE_Test);
            //DrawGoToBundleSystemSceneButton(BaseConstMgr.SCE_Logo);
        }
        protected virtual void OnDrawPresentScriptTemplate()
        {
            //e.x.
            //DrawButton("Unity模版", () => { BaseFileUtils.OpenExplorer(BaseConstansMgr.Path_EditorScriptTeamplates); });
        }
        protected virtual void OnDrawPresentExplorer()
        {
            DrawButton("LogTag", () => SafeOpenJsonFile(BaseConstMgr.Path_LoggerTag, CLog.CreateDefaultData()));
            DrawButton("Language", () => BaseFileUtils.OpenExplorer(BaseConstMgr.Path_NativeDLCLLanguage, true));
            DrawButton("Lua", () => BaseFileUtils.OpenExplorer(BaseConstMgr.Path_NativeDLCLuaScript, true));
        }
        protected virtual void OnDrawSubwindow()
        {

        }
        protected virtual void OnDrawSettings()
        {

        }
        protected virtual bool CheckIsHaveError()
        {
            //e.x.
            return DoCheckWindow<BaseCheckerWindow>();
        }
        #endregion
    }
}