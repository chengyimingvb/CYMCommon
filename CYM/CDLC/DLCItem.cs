//------------------------------------------------------------------------------
// DLCConfig.cs
// Copyright 2018 2018/11/7 
// Created by CYM on 2018/11/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM.DLC
{
    /// <summary>
    /// Build rule type
    /// </summary>
    public enum BuildRuleType
    {
        Directroy,//根据文件夹打包
        FullPath, //根据完整路径打包
        File,     //根据文件打包
    }

    /// <summary>
    /// Build rule data
    /// </summary>
    [Serializable]
    public class BuildRuleData:ICloneable 
    {
        //自定义根目录
        public string CustomRootPath { get; set; } = null;
        //打包规则
        public BuildRuleType BuildRuleType = BuildRuleType.Directroy;
        //搜索路径
        public string SearchPath;
        //提取后的最后文件名,eg. Level/Data = Data
        public string FinalDirectory { get; set; }
        //完整路径 eg. Level/Data = Assets/Bundles/Native/Level/Data
        public string FullSearchPath { get; set; }

        public object Clone()
        {
            return MemberwiseClone();  
        }
    }

    /// <summary>
    /// dlc 资源配置
    /// </summary>
    [Serializable]
    public class DLCItem
    {
        #region prop
        //完整Root路径
        private string AbsRootPath { get; set; }
        //需拷贝的文件的完整路径
        private List<string> AbsCopyDirectory { get; set; } = new List<string>();
        //生成目标路径
        public string TargetPath { get; private set; }
        //内置DLC
        public DLCItem InternalDLC { get; set; }
        //语言包路径
        public string LanguagePath { get; private set; }
        //lua脚本路径
        public string LuaPath { get; private set; }
        //依赖文件
        private AssetBundleManifest ABManifest { get; set; }
        //根目录
        public string RootPath { get; private set; }
        //DLC配置文件
        private DLCConfig DLCConfig => DLCConfig.Ins;
        public List<BuildRuleData> Data { get; protected set; } = new List<BuildRuleData>();
        public List<string> CopyDirectory { get; protected set; } = new List<string>();
        #endregion

        #region inspector
        //DLC的名称 eg. Native
        public string Name;
        #endregion

        #region Init
        /// <summary>
        /// 导出的初始化
        /// </summary>
        void Init()
        {
            //计算DLC的跟目录
            RootPath = DLCAssetMgr.GetDLCRootPath(Name) ;
            //计算出绝对路径(拷贝文件使用)
            AbsRootPath = Path.Combine(BaseConstMgr.Path_Project, RootPath.Replace("Assets/", ""));
            //计算出目标路径
            TargetPath = Path.Combine(BaseConstMgr.Path_StreamingAssets, Name);

            //计算语言包路径
            if(DLCConfig.IsEditorMode) LanguagePath = Path.Combine(RootPath, BaseConstMgr.Dir_Language);
            else LanguagePath = Path.Combine(TargetPath, BaseConstMgr.Dir_Language);
            //计算lua路径
            if(DLCConfig.IsEditorMode) LuaPath = Path.Combine(RootPath,BaseConstMgr.Dir_Lua);
            else LuaPath = Path.Combine(TargetPath, BaseConstMgr.Dir_Lua);

            #region func
            EnsureDirectories();
            GenerateCopyPath();
            GeneralPath();
            //确保DLC相关路径存在
            void EnsureDirectories()
            {
                if (DLCConfig.IsEditorMode)
                {
                    BaseFileUtils.EnsureDirectory(AbsRootPath);
                    foreach (var item in Data)
                        BaseFileUtils.EnsureDirectory(Path.Combine(AbsRootPath, item.SearchPath));
                    foreach (var item in CopyDirectory)
                        BaseFileUtils.EnsureDirectory(Path.Combine(AbsRootPath, item));
                }
            }
            //建立拷贝路径
            void GenerateCopyPath()
            {
                AbsCopyDirectory.Clear();
                if (CopyDirectory != null)
                {
                    for (int i = 0; i < CopyDirectory.Count; ++i)
                    {
                        AbsCopyDirectory.Add(Path.Combine(AbsRootPath, CopyDirectory[i]));
                    }
                }
            }
            //建立打包路径
            void GeneralPath()
            {
                foreach (var item in Data)
                {
                    var vals = item.SearchPath.Replace('\\', '/');
                    var temps = vals.Split('/');
                    if (temps == null || temps.Length == 0)
                    {
                        CLog.Error("路径错误:{0}", item.SearchPath);
                    }
                    item.FinalDirectory = temps[temps.Length - 1];
                    if (item.FinalDirectory == null)
                        CLog.Error("错误");

                    string tempRootPath = RootPath;
                    if (!item.CustomRootPath.IsInvStr())
                        tempRootPath = item.CustomRootPath;
                    item.FullSearchPath = tempRootPath + "/" + item.SearchPath;
                }
            }
            #endregion
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="data"></param>
        /// <param name="copyDir"></param>
        public void LoadConfig(List<BuildRuleData> datas, List<string> copyDir)
        {
            foreach (var item in datas)
                Data.Add(item.Clone() as BuildRuleData);
            CopyDirectory.AddRange(copyDir.ToArray());
            Init();
        }
        /// <summary>
        /// 导入的初始化
        /// </summary>
        public void Load(AssetBundleManifest assetBundleManifest)
        {
            ABManifest = assetBundleManifest;
            Init();
        }
        #endregion

        #region set
        //添加打包规则
        public void AddBuildData(string name, BuildRuleType type = BuildRuleType.Directroy,string customRootPath=null)
        {
            var data = new BuildRuleData();
            data.BuildRuleType = type;
            data.SearchPath = name;
            data.CustomRootPath = customRootPath;
            Data.Add(data);
        }
        //拷贝非打包资源到指定目录
        public void CopyAllFiles()
        {
            for (int i = 0; i < AbsCopyDirectory.Count; ++i)
            {
                string absPath = AbsCopyDirectory[i];
                string dir = CopyDirectory[i];
                string finalTargetPath = Path.Combine(TargetPath, dir);
                BaseFileUtils.CopyDir(absPath, finalTargetPath, false, true);
            }
            //如果是默认DLC,拷贝指定的内部资源
            if (IsNative)
            {
                BaseFileUtils.CopyDir(BaseConstMgr.Path_InternalLanguage, Path.Combine(TargetPath, "Language"), false, false);
                BaseFileUtils.CopyDir(BaseConstMgr.Path_InternalLua, Path.Combine(TargetPath, "Lua"), false, false);
            }
        }
        #endregion

        #region get
        public string[] GetAllDependencies(string assetBundleName)
        {
            if (ABManifest == null)
            {
                CLog.Error("没有AssetBundleManifest文件");
                return null;
            }
            return ABManifest.GetAllDependencies(assetBundleName);
        }
        public string[] GetAllLanguages()
        {
            List<string> fileList = new List<string>();
            //编辑器模式下获得内部语言包,非编辑器模式下直接从StreamingAssets里面获取
            if (DLCConfig.IsEditorMode && IsNative)
            {
                var temp = BaseFileUtils.GetFiles(BaseConstMgr.Path_InternalLanguage, "*.xls", SearchOption.AllDirectories);
                if (temp != null)
                    fileList.AddRange(temp);
            }
            {
                var temp = BaseFileUtils.GetFiles(LanguagePath, "*.xls", SearchOption.AllDirectories);
                if (temp != null)
                    fileList.AddRange(temp);
            }
            return fileList.ToArray();
        }
        public string[] GetAllLuas()
        {
            List<string> fileList = new List<string>();
            //编辑器模式下获得内部Lua脚本,非编辑器模式下直接从StreamingAssets里面获取
            if (DLCConfig.IsEditorMode && IsNative)
            {
                var temp = BaseFileUtils.GetFiles(BaseConstMgr.Path_InternalLua, "*.txt", SearchOption.AllDirectories);
                if(temp!=null)
                    fileList.AddRange(temp);
            }
            {
                var temp = BaseFileUtils.GetFiles(LuaPath, "*.txt", SearchOption.AllDirectories);
                if (temp != null)
                    fileList.AddRange(temp);
            }
            return fileList.ToArray();
        }
        #endregion

        #region is
        public bool IsNative => Name == BaseConstMgr.STR_NativeDLC;
        #endregion
    }

}