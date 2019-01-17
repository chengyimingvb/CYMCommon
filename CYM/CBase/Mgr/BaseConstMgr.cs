using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

//**********************************************
// Class Name	: CYMConstans
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class LayerData
    {
        private int Layer { get; set; } = 0;
        private LayerMask Mask { get; set; }
        public string Name { get; private set; }

        public static implicit operator LayerData(string str)
        {
            LayerData ret = new LayerData();
            ret.Name = str;
            ret.Layer = NameToLayer(str);
            ret.Mask =  GetMask(str);
            return ret;
        }

        public static int GetMask(params LayerData[] ps)
        {
            if (ps == null)
            {
                return default(LayerMask);
            }
            if(ps.Length==0)
                return default(LayerMask);
            int mask = 0;
            foreach (var item in ps)
            {
                mask |= item.Mask;
            }
            return mask;
        }

        public static explicit operator int(LayerData data)
        {
            return data.Layer;
        }

        public static explicit operator LayerMask(LayerData data)
        {
            return data.Mask;
        }

        public static explicit operator string(LayerData data)
        {
            return data.Name;
        }

        public static void SetLayerAndMask(string str, out int layer, out LayerMask mask)
        {
            layer = NameToLayer(str);
            mask = GetMask(str);
        }
        public static int NameToLayer(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }
        public static int GetMask(params string[] layerNames)
        {
            return LayerMask.GetMask(layerNames);
        }
    }

    public class BaseConstMgr : BaseGlobalCoreMgr
    {
        #region member variable
        public static readonly int InvInt = int.MinValue;
        public static readonly float InvFloat = float.NaN;
        public static readonly Vector3 FarawayPos = Vector3.one * 9999;
        public static readonly Vector3 MiniScale = Vector3.one * 0.00001f;
        public static readonly Vector3 WorldUICameraPos = new Vector3(9999,9999,0.0f);
        #endregion

        #region config
        public readonly static int MaxTalkOptionCount = 4;
        public const string DBTempSaveName = "TempSave";
        #endregion

        #region layer
        public static readonly LayerData Layer_System = "System";
        public static readonly LayerData Layer_Default = "Default";
        public static readonly LayerData Layer_UI = "UI";
        public static readonly LayerData Layer_Water = "Water";
        public static readonly LayerData Layer_TransparentFX = "TransparentFX";
        public static readonly LayerData Layer_IgnoreRaycast = "Ignore Raycast";
        public static readonly LayerData Layer_Sense = "Sense";
        public static readonly LayerData Layer_Perform="Perform";
        #endregion

        #region STR
        public const string STR_Infinite = "∞";
        public const string STR_None = "None";
        public const string STR_Inv ="";
        public const string STR_Unkown = "???";
        public const string STR_Indent = "<color=white>\n     *</color>";
        public const string STR_DoubbleIndent = "<color=white>\n          *</color>";
        public const string STR_IndentOr = "<color=white>\n     -</color>";
        public const string STR_Append = "  ";
        public const string STR_AppendStar = "  *";
        public const string STR_Space = " ";
        public const string STR_Enter = "\n";
        public const string STR_Model = "Model";
        public const string STR_Desc_NoDesc = "Desc_NoDesc";
        public const string STR_Base = "Base";
        public const string STR_ABManifest = "AssetBundleManifest";
        public const string STR_NativeDLC = "Native";
        public const string STR_DLCManifest = "DLCManifest";
        public const string STR_DLCItem = "DLCItem";
        #endregion

        #region bundle name
        public const string BN_Shared = "shared";
        public const string BN_Shader = "shaders";
        public const string BN_Icon = "icon";
        public const string BN_Music = "music";
        public const string BN_Prefab = "prefab";
        public const string BN_Perform = "perform";
        public const string BN_Audio = "audio";
        public const string BN_Materials = "material";
        public const string BN_UI = "ui";
        public const string BN_BG = "bg";
        public const string BN_Texture = "texture";
        public const string BN_Lua = "lua";
        public const string BN_Scene = "scene";
        public const string BN_PhysicMaterial = "physicmaterial";
        public const string BN_Video = "video";
        public const string BN_AudioMixer = "audiomixer";
        public const string BN_Common = "common";
        public const string BN_Animator = "animator";
        public const string BN_System = "system";
        #endregion

        #region scene
        public const string SCE_Start = "Start";
        public const string SCE_Preview = "Preview";
        public const string SCE_Test = "Test";
        #endregion

        #region Prefix Suffix
        //注释前缀
        public const string Prefix_Notes = "#";
        //动态ID,所有动态资源的前缀,比如头像,音效,语音,包括翻译字符串等等
        public const string Prefix_DynamicTrans = "$";
        //翻译前缀
        public const string Prefix_DescTrans = "Desc_";
        //场景前缀
        public const string Prefix_Scene = "Scene_";
        //存档前缀
        public const string Prefix_AutoSave = "AutoSave_";
        //快捷键
        public const string Prefix_Shortcut = "Shortcut_";
        //属性图标
        public const string Prefix_Attr = "Attr_";
        //禁用图片后缀
        public const string Suffix_Disable = "_Dis";
        //选项后缀,用于对话等系统
        public const string Suffix_Op = "_Op";
        #endregion

        #region dir
        //lua文件夹
        public static readonly string Dir_Lua = "Lua";
        //text assets文件夹
        public static readonly string Dir_TextAssets = "TextAssets";
        //bundles
        public static readonly string Dir_Bundles = "_Bundles";
        public const string Dir_NoBundles = "_NoBundles";
        public const string Dir_Scene = "Scene";
        //语言文件包
        public const string Dir_Language = "Language";
        //配置文件
        public const string Dir_Config = "Config";
        #endregion

        #region file
        public static string File_CYMCommon_asmdef = "CYMCommon.asmdef";
        #endregion

        #region extention
        public static readonly string Extention_Save = ".dbs";
        #endregion

        #region format
        /// <summary>
        /// 场景Bundle的格式路径
        /// </summary>
        public readonly static string Format_BundleScenesPath = "/_Bundles/"+STR_NativeDLC+"/Scene/{0}.unity";
        public readonly static string Format_BundleSystemScenesPath = "/_Bundles/"+ Dir_NoBundles + "/Scene/{0}.unity";
        /// <summary>
        /// 全局配置资源的格式路径
        /// </summary>
        public readonly static string Format_ConfigAssetPath = "Assets/Resources/"+ Dir_Config + "/{0}.asset";
        #endregion

        #region Regex
        /// <summary>
        /// 富文本,图标正则
        /// </summary>
        public readonly static string Regex_RichTextIcon = @"\[(.*?)\]";
        /// <summary>
        /// 富文本,动态参数正则
        /// </summary>
        public readonly static string Regex_RichTextDyn = @"\%(.*?)\%";
        #endregion

        #region project path
        /// <summary>
        /// CYMCommon
        /// </summary>
        public static readonly string Path_Common = "Assets/CYMCommon";
        /// <summary>
        /// 工程目录
        /// </summary>
        public static readonly string Path_Project = Application.dataPath;
        /// <summary>
        /// 美术资源文件夹
        /// </summary>
        public static readonly string Path_Arts = Path.Combine(Path_Project, "_Arts");
        /// <summary>
        /// 美术资源UI文件夹
        /// </summary>
        public static readonly string Path_AtrUI = Path.Combine(Path_Arts, "UI");
        /// <summary>
        /// Bundle资源文件夹
        /// </summary>
        public static readonly string Path_Bundles = Path.Combine(Path_Project, Dir_Bundles);
        public static readonly string Path_NoBundles = Path.Combine(Path_Bundles, Dir_NoBundles);
        public static readonly string Path_NoBundlesScene = Path.Combine(Path_NoBundles, Dir_Scene);
        /// <summary>
        /// 原生Bundle资源文件夹
        /// </summary>
        public static readonly string Path_NativeDLC = Path.Combine(Path_Bundles, STR_NativeDLC);
        /// <summary>
        /// 程序脚本文件夹
        /// </summary>
        public static readonly string Path_Funcs = Path.Combine(Path_Project, "_Funcs");
        /// <summary>
        /// 测试文件夹
        /// </summary>
        public static readonly string Path_Tests = Path.Combine(Path_Project, "_Tests");
        /// <summary>
        /// Gizmos
        /// </summary>
        public static readonly string Path_Gizmos = Path.Combine(Path_Project, "Gizmos");
        /// <summary>
        /// Resources
        /// </summary>
        public static readonly string Path_Resources = Path.Combine(Path_Project, "Resources");
        /// <summary>
        /// Resources Script Template
        /// </summary>
        public static readonly string Path_ScriptTemplate = Path.Combine(Path_Resources, "ScriptTemplate");
        /// <summary>
        /// Resources
        /// </summary>
        public static readonly string Path_Editor = Path.Combine(Path_Project, "Editor");
        /// <summary>
        /// StreamingAssets
        /// </summary>
        public static readonly string Path_StreamingAssets = Path.Combine(Path_Project, "StreamingAssets");
        /// <summary>
        /// Config
        /// </summary>
        public static readonly string Path_ResourcesConfig = Path.Combine(Path_Resources,Dir_Config);
        #endregion

        #region path
        /// <summary>
        /// 本地语言文件包
        /// </summary>
        public static readonly string Path_NativeDLCLLanguage = Path.Combine(Path_NativeDLC, Dir_Language);
        /// <summary>
        /// 本地lua脚本位置
        /// </summary>
        public readonly static string Path_NativeDLCLuaScript = Path.Combine(Path_NativeDLC, Dir_Lua); 
        /// <summary>
        /// build exe 目录
        /// </summary>
        public readonly static string Path_Build = Path.GetFullPath(Path.Combine(Application.dataPath, "../Bin"));
        /// <summary>
        /// 开发目录
        /// </summary>
        public static readonly string Path_Dev = Path.Combine(Application.persistentDataPath, "dev");
        /// <summary>
        /// 开发者配置目录
        /// </summary>
        public static readonly string Path_DevSettings = Path.Combine(Path_Dev, "dev_settings.json");
        /// <summary>
        /// logger目录
        /// </summary>
        public static readonly string Path_LoggerTag = Path.Combine(Path_Dev, "logger_tag.json");
        /// <summary>
        /// 本地存档
        /// </summary>
        public static readonly string Path_LocalDB = Path.Combine(Application.persistentDataPath, "Save");
        /// <summary>
        /// 本地存档
        /// </summary>
        public static readonly string Path_CloudDB = Path.Combine(Application.persistentDataPath, "CloudSave");
        /// <summary>
        /// 设置文件名称
        /// </summary>
        public static readonly string Path_Settings = Path.Combine(Application.persistentDataPath, "Settings.json");
        /// <summary>
        /// CYMMonobehaviour
        /// </summary>
        public static readonly string Path_XTempCYMMonobehaviour = Path.Combine(Path_Common, "CYM/CUtile/Template/XenoTemplateFiles/CYMMonobehaviour.asset");
        /// <summary>
        /// CYMMonobehaviour
        /// </summary>
        public static readonly string Path_XTempMonobehaviour = Path.Combine(Path_Common, "CYM/CUtile/Template/XenoTemplateFiles/Monobehaviour.asset");
        /// <summary>
        /// CYMCommon/CYMCommon.asmdef
        /// </summary>
        public static readonly string Path_CYMCommonASMdef = Path.Combine(Path_Common, File_CYMCommon_asmdef);
        /// <summary>
        /// CYMCommon/CYMCommon.asmdef temp
        /// </summary>
        public static readonly string Path_CYMCommonASMdefTemp = Path.Combine(Application.temporaryCachePath, File_CYMCommon_asmdef);
        /// <summary>
        /// 内部bundle
        /// </summary>
        public static readonly string Path_InternalBundle = Path.Combine(Path_Common, "CYM/_Bundle");
        #endregion

        #region color
        public static readonly Color Color_BuffPositive = BaseUtils.FromHex("136700FF");
        public static readonly Color Color_BuffNegative = BaseUtils.FromHex("670000FF");
        public static readonly Color Color_BuffAll = BaseUtils.FromHex("280067FF");
        #endregion
    }
}
