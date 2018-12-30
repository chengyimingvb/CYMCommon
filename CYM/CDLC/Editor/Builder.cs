using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CYM.DLC
{
    public class Builder
    {
        #region prop
        /// <summary>
        /// 所有的资源
        /// string1:资源路径
        /// string2:bundle 名字
        /// </summary>
        static Dictionary<string, string> AllAssets { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// 已经决定被打包的资源,防止重复打包
        /// </summary>
        static HashSet<string> PackedAssets_Native { get; set; } = new HashSet<string>();
        static HashSet<string> PackedAssets_DLC { get; set; } = new HashSet<string>();
        /// <summary>
        /// 即将要buid的Bundle数据
        /// </summary>
        static List<AssetBundleBuild> Builds { get; set; } = new List<AssetBundleBuild>();
        /// <summary>
        /// 打包规则
        /// </summary>
        static List<BuildRule> Rules { get; set; } = new List<BuildRule>();
        /// <summary>
        /// 所有的共享文件
        /// string:被依赖的路径key 
        /// List<string>:依赖源的路径
        /// </summary>
        static Dictionary<string, HashSet<string>> AllDependencies { get; set; } = new Dictionary<string, HashSet<string>>();
        /// <summary>
        /// 上一次BuildRule的缓存
        /// </summary>
        static List<AssetBundleBuild> AssetBundleBuildsCache = new List<AssetBundleBuild>();
        static BuildConfig BuildConfig => BuildConfig.Ins;
        static DLCItem Native => DLCConfig.Native;
        static DLCConfig DLCConfig => DLCConfig.Ins;
        #endregion

        #region Build utile
        public static void BuildManifest(DLCItem dlc)
        {
            //如果不是NativeDLC则先BuildNativeDLC,防止资源被其他DLC重复打包
            if (!dlc.IsNative)
            {
                BuildManifestInternel(Native);
            }
            BuildManifestInternel(dlc);
        }
        public static void BuildEXE()
        {
            OnPreBuild();
            _BuildEXE();
            OnPostBuild();
        }
        public static void BuildBundle(DLCItem dlc)
        {
            OnPreBuild();
            _BuildBundle(dlc);
            CopyDLCToEXE(dlc);
            OnPostBuild();
        }
        public static void BuildBundleAndEXE(DLCItem dlc)
        {
            OnPreBuild();
            _BuildBundle(dlc);
            _BuildEXE();
            OnPostBuild();
        }
        #endregion

        #region build AssetBundle name
        /// <summary>
        /// 返回完整的路径作为Bundle名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static string GetABNameWithFullPath(string filePath)
        {
            return Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)).Replace('\\', '/').ToLower();
        }
        /// <summary>
        /// 根据文件夹以及文件名生成Bundle名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        static string GetABNameWithDirectoryAndFile(string finalDirectory, string filePath)
        {
            return (finalDirectory + "/" + Path.GetFileNameWithoutExtension(filePath)).ToLower();
        }
        #endregion

        #region private
        /// <summary>
        /// 获得文件路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        static List<string> GetFilesWithoutDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                CLog.Error("没有这个路径:{0}", path);
                return new List<string>();
            }
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            List<string> items = new List<string>();
            foreach (var item in files)
            {
                if (item.EndsWith(".meta", StringComparison.CurrentCulture))
                    continue;
                var assetPath = item.Replace('\\', '/');
                if (!Directory.Exists(assetPath))
                {
                    items.Add(assetPath);
                }
            }
            return items;
        }
        static void BuildManifestInternel(DLCItem dlcItem)
        {
            var builds = AssetBundleBuildsCache = BuildBuildRules(dlcItem);
            string dlcName = dlcItem.Name;
            string manifestPath = DLCAssetMgr.GetDLCManifestPath(dlcName);
            string dlcItemPath = DLCAssetMgr.GetDLCItemPath(dlcName);

            List<string> bundles = new List<string>();
            List<string> assets = new List<string>();

            if (builds.Count > 0)
            {
                foreach (var item in builds)
                {
                    bundles.Add(item.assetBundleName);
                    foreach (var assetPath in item.assetNames)
                    {
                        assets.Add(assetPath + ":" + (bundles.Count - 1));
                    }
                }
            }

            #region 创建Manifest文件
            if (File.Exists(manifestPath)) File.Delete(manifestPath);
            DLCManifest dlcManifest = new DLCManifest();
            foreach (var item in builds)
            {
                BundleData tempData = new BundleData();
                tempData.DLCName = dlcItem.Name;
                tempData.BundleName = item.assetBundleName;
                foreach (var asset in item.assetNames)
                {
                    AssetPathData pathData = new AssetPathData();
                    pathData.FullPath = asset;
                    pathData.FileName = Path.GetFileNameWithoutExtension(asset);
                    if (AllAssets.ContainsKey(asset))
                    {
                        pathData.SourceBundleName = AllAssets[asset];
                    }
                    tempData.AssetFullPaths.Add(pathData);
                }
                dlcManifest.Data.Add(tempData);
            }
            BaseFileUtils.SaveJson(manifestPath, dlcManifest, true);
            #endregion

            #region dlcitem
            if (File.Exists(dlcItemPath)) File.Delete(dlcItemPath);
            BaseFileUtils.SaveJson(dlcItemPath,dlcItem,true);
            #endregion

            CLog.Debug("[BuildScript] BuildManifest with " + assets.Count + " assets and " + bundles.Count + " bundels.");
        }
        private static void _BuildBundle(DLCItem dlcItem)
        {
            BaseFileUtils.EnsureDirectory(dlcItem.TargetPath);
            BuildManifest(dlcItem);
            BuildPipeline.BuildAssetBundles(dlcItem.TargetPath, 
                AssetBundleBuildsCache.ToArray(), 
                BuildAssetBundleOptions.ChunkBasedCompression,
                EditorUserBuildSettings.activeBuildTarget);
            dlcItem.CopyAllFiles();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 活的构建的数据
        /// </summary>
        /// <param name="manifestPath"></param>
        /// <returns></returns>
        private static List<AssetBundleBuild> BuildBuildRules(DLCItem dlcItem)
        {
            if (dlcItem.IsNative)
                PackedAssets_Native.Clear();
            PackedAssets_DLC.Clear();
            Builds.Clear();
            Rules.Clear();
            AllDependencies.Clear();
            AllAssets.Clear();

            //建立其他文件
            List<BuildRuleData> tempBuildDatas = dlcItem.Data;
            if (tempBuildDatas == null && tempBuildDatas.Count == 0)
            {
                CLog.Error("没有配置相关的AssetBundle信息");
                return null;
            }

            foreach (var item in tempBuildDatas)
            {
                BuildRule buildRule = null;
                if (item.BuildRuleType == BuildRuleType.Directroy)
                    buildRule = new BuildAssetsWithDirectroyName();
                else if (item.BuildRuleType == BuildRuleType.FullPath)
                    buildRule = new BuildAssetsWithFullPath();
                else if (item.BuildRuleType == BuildRuleType.File)
                    buildRule = new BuildAssetsWithFile();

                buildRule.Data = item;
                Rules.Add(buildRule);
            }

            //搜集依赖的资源
            foreach (var item in Rules)
            {
                CollectDependencies(item.Data.FullSearchPath);
            }
            //打包共享的资源
            BuildSharedAssets(dlcItem);

            foreach (var item in Rules)
            {
                item.Build();
            }
            EditorUtility.ClearProgressBar();
            return Builds;
        }
        static void AddToPackedAssets(string path)
        {
            PackedAssets_Native.Add(path);
            PackedAssets_DLC.Add(path);
        }
        static bool IsContainInPackedAssets(string path)
        {
            if (PackedAssets_Native.Contains(path) ||
                PackedAssets_DLC.Contains(path))
                return true;
            return false;
        }
        private static void CollectDependencies(string path)
        {
            var files = GetFilesWithoutDirectories(path);
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                var dependencies = AssetDatabase.GetDependencies(item);
                if (EditorUtility.DisplayCancelableProgressBar(string.Format("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }

                foreach (var assetPath in dependencies)
                {
                    if (!AllDependencies.ContainsKey(assetPath))
                    {
                        AllDependencies[assetPath] = new HashSet<string>();
                    }

                    if (!AllDependencies[assetPath].Contains(item))
                    {
                        AllDependencies[assetPath].Add(item);
                    }
                }
            }
        }
        /// <summary>
        /// 打包共享的资源
        /// </summary>
        protected static void BuildSharedAssets(DLCItem dlcItem)
        {
            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            foreach (var item in AllDependencies)
            {
                var name = "None";
                var assetPath = item.Key;
                if (!assetPath.EndsWith(".cs", StringComparison.CurrentCulture) &&
                    !assetPath.EndsWith(".js", StringComparison.CurrentCulture))
                {
                    if (IsContainInPackedAssets(assetPath))
                        continue;
                    //打包共享的Sahder资源,制作成一个完整的单独包
                    if (assetPath.EndsWith(".shader", StringComparison.CurrentCulture))
                        name = BaseConstMgr.BN_Shader;
                    else
                    {
                        //如果依赖的资源小于2个,则跳过
                        if (item.Value.Count <= 1)
                            continue;
                        //打包Native共享资源
                        if (dlcItem.IsNative)
                            name = BaseConstMgr.BN_Shared;
                        //打包其他DLC资源,打成散包
                        else
                            name = BaseConstMgr.BN_Shared+ "/" + GetABNameWithFullPath(Path.GetDirectoryName(assetPath));
                    }

                    //添加到Bundles
                    List<string> list = null;
                    if (!bundles.TryGetValue(name, out list))
                    {
                        list = new List<string>();
                        bundles.Add(name, list);
                    }
                    if (!list.Contains(assetPath))
                    {
                        list.Add(assetPath);
                        AddToPackedAssets(assetPath);
                    }
                }
            }


            //放入打包容器
            foreach (var item in bundles)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = item.Key;// + "_" + item.Value.Count;
                build.assetNames = item.Value.ToArray();
                Builds.Add(build);
            }
        }
        private static void _BuildEXE()
        {
            string path = BuildConfig.DirPath;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string location = BuildConfig.ExePath;
            CLog.Log("location = {0}", location);
            BuildOptions op = BuildOptions.None;
            if (BuildConfig.IsUnityDevelopmentBuild)
                op |= BuildOptions.Development;

            List<string> names = new List<string>();
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null)
                    continue;
                if (e.enabled)
                    names.Add(e.path);
            }
            string[] result = names.ToArray();
            BuildPipeline.BuildPlayer(result, location, GetBuildTarget(), op);


            Distribution distribution = BuildConfig.Distribution;
            BuildConfig.GetBuildData(distribution).PostBuilder();
        }
        private static BuildTarget GetBuildTarget()
        {
            if (BuildConfig.Platform == Platform.Windows)
                return BuildTarget.StandaloneWindows;
            else if (BuildConfig.Platform == Platform.Windows64)
                return BuildTarget.StandaloneWindows64;
            else if (BuildConfig.Platform == Platform.Android)
                return BuildTarget.Android;
            return BuildTarget.StandaloneWindows64;
        }
        private static BuildTargetGroup GetBuildTargetGroup()
        {
            var temp = GetBuildTarget();
            if (temp == BuildTarget.StandaloneWindows ||
                temp == BuildTarget.StandaloneWindows64 ||
                temp == BuildTarget.StandaloneOSX)
            {
                return BuildTargetGroup.Standalone;
            }
            else if (temp == BuildTarget.WebGL)
                return BuildTargetGroup.WebGL;
            else if (temp == BuildTarget.Android)
                return BuildTargetGroup.Android;
            return BuildTargetGroup.Standalone;
        }
        static void OnPreBuild()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(GetBuildTargetGroup(), GetBuildTarget());
            AssetDatabase.Refresh();
        }
        static void OnPostBuild()
        {
        }
        static void CopyDLCToEXE(DLCItem item)
        {
            string sourcePath = Path.Combine(BaseConstMgr.Path_StreamingAssets, item.Name);
            string targetPath = Path.Combine(BuildConfig.DirPath,BuildConfig.FullName + "_Data/StreamingAssets", item.Name);
            BaseFileUtils.CopyDir(sourcePath, targetPath);
        }
        #endregion

        #region build rule
        abstract class BuildRule
        {

            #region utile


            public BuildRuleData Data { get; set; }
            protected BuildRule()
            {

            }
            public abstract void Build();
            #endregion
        }
        class BuildAssetsWithDirectroyName : BuildRule
        {
            public override void Build()
            {
                List<string> packedAsset = new List<string>();
                var files = GetFilesWithoutDirectories(Data.FullSearchPath);
                string bundleName = Data.FinalDirectory.ToLower();
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    if (!IsContainInPackedAssets(item))
                    {
                        packedAsset.Add(item);
                        AddToPackedAssets(item);
                    }
                    if (!AllAssets.ContainsKey(item))
                        AllAssets.Add(item, bundleName);
                }

                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = bundleName;
                build.assetNames = packedAsset.ToArray();
                Builds.Add(build);
            }
        }
        class BuildAssetsWithFullPath : BuildRule
        {
            public override void Build()
            {
                var files = GetFilesWithoutDirectories(Data.FullSearchPath);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    string bundleName = GetABNameWithFullPath(item);
                    if (!IsContainInPackedAssets(item))
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName;
                        build.assetNames = new string[] { item };
                        Builds.Add(build);
                        AddToPackedAssets(item);
                    }
                    if (!AllAssets.ContainsKey(item))
                        AllAssets.Add(item, bundleName);
                }
            }
        }
        class BuildAssetsWithFile : BuildRule
        {
            public override void Build()
            {
                var files = GetFilesWithoutDirectories(Data.FullSearchPath);
                for (int i = 0; i < files.Count; i++)
                {
                    var item = files[i];
                    if (EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                    {
                        EditorUtility.ClearProgressBar();
                        break;
                    }
                    string bundleName = GetABNameWithDirectoryAndFile(Data.FinalDirectory, item);
                    if (!IsContainInPackedAssets(item))
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = bundleName;
                        build.assetNames = new string[] { item };
                        Builds.Add(build);
                        AddToPackedAssets(item);
                    }
                    if (!AllAssets.ContainsKey(item))
                        AllAssets.Add(item, bundleName);
                }
            }
        }
        #endregion
    }
}