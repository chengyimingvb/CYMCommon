//------------------------------------------------------------------------------
// AssetBundleMono.cs
// Copyright 2018 2018/5/21 
// Created by CYM on 2018/5/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using CYM.DLC;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace CYM.DLC
{
    /// <summary>
    /// AB加载方式
    /// </summary>
    public enum AssetBundleLoadType
    {
        Normal, 
        Async,
        Scene,
    }

    public class DLCAssetMgr : MonoBehaviour
    {
        #region prop
        static DLCAssetMgr Ins;
        static DLCConfig DLCConfig => DLCConfig.Ins;
        readonly internal static Dictionary<string, Bundle> Bundles = new Dictionary<string, Bundle>();
        readonly internal static Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
        readonly internal static Dictionary<string, DLCItem> DLCItems = new Dictionary<string, DLCItem>();
        readonly internal static List<Asset> AsyncAssets = new List<Asset>();
        readonly internal static List<Asset> clearAsyncAssets = new List<Asset>();
        #endregion

        #region 映射
        /// <summary>
        /// 资源路径映射
        /// string:资源路径
        /// int:bundle的index ,这里使用Index可以节省资源
        /// </summary>
        static internal readonly Dictionary<string, int> amap = new Dictionary<string, int>();
        /// <summary>
        /// Bundle路径映射
        /// string:bundle的名字
        /// List<int>:资源的路径,这里用Index可以节省资源
        /// </summary>
        static internal readonly Dictionary<string, List<int>> bmap = new Dictionary<string, List<int>>();
        /// <summary>
        /// Bundle/AssetName 映射表
        /// string1:bundle 名称
        /// T:int:资源路径的index,这里使用Index可以节省资源
        /// T:string:DLC的名称
        /// </summary>
        static internal readonly Dictionary<string, Tuple<int, string>> bamap = new Dictionary<string, Tuple<int, string>>();
        /// <summary>
        /// 所有的资源数组
        /// </summary>
        static internal readonly List<string> allAssets = new List<string>();
        /// <summary>
        /// 所有的Bundle数组
        /// </summary>
        static internal readonly List<string> allBundles = new List<string>();
        #endregion

        #region life
        /// <summary>
        /// 初始化资源管理
        /// </summary>
        /// <returns></returns>
        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            Ins=BaseUtils.CreateGlobalObj<DLCAssetMgr>("DLCAssetMgr");
        }
        /// <summary>
        /// update
        /// </summary>
        void FixedUpdate()
        {
            if (AsyncAssets.Count > 0)
            {
                foreach (var item in AsyncAssets)
                {
                    item.OnUpdate();
                    if (item.isDone)
                    {
                        clearAsyncAssets.Add(item);
                    }
                }
                if (clearAsyncAssets.Count > 0)
                {
                    AsyncAssets.RemoveAll((x) => x.isDone);
                    clearAsyncAssets.Clear();
                }
            }
        }
        private void OnDestroy()
        {
            ClearDLCManifest();
        }
        #endregion

        #region set
        /// <summary>
        /// 加载DLC
        /// </summary>
        /// <param name="config"></param>
        public static bool LoadDLC(string name)
        {
            //屏蔽NoBundle下得目录
            if (name == BaseConstMgr.Dir_NoBundles)
                return false;
            if (DLCItems.ContainsKey(name))
            {
                CLog.Error("重复加载!!!!");
                return false;
            }
            //DLC名称
            string dlcName = name;      
            //AB清单
            AssetBundleManifest abManifest = null;
            //加载dlc item
            DLCItem dlcItem = BaseFileUtils.LoadJson<DLCItem>(GetDLCItemPath(dlcName));
            //加载自定义Manifest
            DLCManifest dlcManifest = BaseFileUtils.LoadJson<DLCManifest>(GetDLCManifestPath(dlcName));
            //bundle模式
            if (!DLCConfig.Ins.IsEditorMode)
            {
                //加载Unity Assetbundle Manifest
                Bundle tempBundle = LoadBundle(dlcName, dlcName, false);
                abManifest = tempBundle.LoadAsset<AssetBundleManifest>(BaseConstMgr.STR_ABManifest);
                //如果是Native DLC 直接加载共享Shader和Shared包
                if (dlcItem.IsNative)
                {
                    LoadBundle(BaseConstMgr.BN_Shared, dlcName);
                    LoadBundle(BaseConstMgr.BN_Shader, dlcName);
                }
            }
            DLCItems.Add(dlcItem.Name, dlcItem);
            dlcItem.Load(abManifest);
            LoadDLCManifest(dlcManifest);
            return true;

        }
        /// <summary>
        /// 加载Manifest
        /// </summary>
        /// <param name="reader"></param>
        private static void LoadDLCManifest(DLCManifest reader)
        {
            if (reader == null)
            {
                CLog.Error("错误:reader 为 null");
                return;
            }


            foreach (var item in reader.Data)
            {
                string bundle = item.BundleName;
                allBundles.Add(bundle);
                if (!bmap.ContainsKey(bundle))
                    bmap.Add(bundle, new List<int>());

                foreach (var assetPathData in item.AssetFullPaths)
                {
                    //完整路径
                    string fullpath = assetPathData.FullPath;
                    //添加完整路径到资源表
                    allAssets.Add(fullpath);
                    //计算资源路径Index
                    int assetPathIndex = allAssets.Count - 1;
                    //计算资源bunleIndex
                    int bundleNameIndex = allBundles.Count - 1;
                    //添加完整资源路径的Index到映射表中
                    bmap[bundle].Add(assetPathIndex);
                    //添加完整的Bundle名称的Index到映射表中
                    amap[fullpath] = bundleNameIndex;

                    //如果资源有自己的Bundle设定,则添加到映射表中
                    if (!assetPathData.SourceBundleName.IsInvStr())
                    {
                        string bampKey = assetPathData.SourceBundleName + assetPathData.FileName;
                        bamap[bampKey] = new Tuple<int, string>(assetPathIndex, item.DLCName);
                    }
                }
            }
        }
        void ClearDLCManifest()
        {
            amap.Clear();
            bmap.Clear();
            bamap.Clear();
            allAssets.Clear();
            allBundles.Clear();
        }
        #endregion

        #region get
        /// <summary>
        /// 获得DLC的根目录
        /// </summary>
        /// <returns></returns>
        public static string GetDLCRootPath(string dlcName)
        {
            if (DLCConfig.IsEditorMode)
                return Path.Combine("Assets/"+BaseConstMgr.Dir_Bundles, dlcName);
            else
                return Path.Combine(BaseConstMgr.Path_StreamingAssets, dlcName);
        }
        public static string GetDLCRootPath()
        {
            if (DLCConfig.IsEditorMode)
                return "Assets/" + BaseConstMgr.Dir_Bundles;
            else
                return BaseConstMgr.Path_StreamingAssets;
        }
        /// <summary>
        /// 获得DLCitem路径
        /// </summary>
        /// <param name="dlcName"></param>
        /// <returns></returns>
        public static string GetDLCItemPath(string dlcName)
        {
            if (DLCConfig.IsEditorMode)
                return Path.Combine(BaseConstMgr.Path_Bundles, dlcName, BaseConstMgr.Dir_Config, BaseConstMgr.STR_DLCItem+".json");
            else
                return Path.Combine(BaseConstMgr.Path_StreamingAssets, dlcName, BaseConstMgr.Dir_Config, BaseConstMgr.STR_DLCItem + ".json");
        }
        public static string GetDLCManifestPath(string dlcName)
        {
            if (DLCConfig.IsEditorMode)
                return Path.Combine(BaseConstMgr.Path_Bundles, dlcName, BaseConstMgr.Dir_Config, BaseConstMgr.STR_DLCManifest + ".json");
            else
                return Path.Combine(BaseConstMgr.Path_StreamingAssets, dlcName, BaseConstMgr.Dir_Config, BaseConstMgr.STR_DLCManifest + ".json");
        }
        /// <summary>
        /// 获得DLC
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DLCItem GetDLCItem(string name)
        {
            if (!DLCItems.ContainsKey(name))
            {
                throw new Exception("没有这个DLC:"+name);
            }
            //从外部已经加载进来的DLC中获取
            return DLCItems[name];
        }
        /// <summary>
        /// 根据资源路径获得Bundle名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetBundleName(string assetPath)
        {
            if (assetPath == null)
            {
                return "";
            }
            if (!amap.ContainsKey(assetPath))
            {
                Error("没有这个资源:{0}", assetPath);
            }
            return allBundles[amap[assetPath]];
        }
        /// <summary>
        /// 根据完整路径获得资源名称
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string GetAssetName(string assetPath)
        {
            return Path.GetFileName(assetPath);
        }
        /// <summary>
        /// 根据Bundle名称和资源名称获得其完整路径
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetAssetPath(string bundleName, string assetName)
        {
            string key = bundleName.ToLower() + assetName;
            if (!bamap.ContainsKey(key))
            {
                Error("没有这个资源,Bundle:{0},Asset:{1}", bundleName, assetName);
                return null;
            }
            return allAssets[bamap[key].Item1];
        }
        /// <summary>
        /// 获得DLC的名称
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string GetDLCName(string bundleName, string assetName)
        {
            string key = bundleName.ToLower() + assetName;
            if (!bamap.ContainsKey(key))
            {
                Error("没有这个资源,Bundle:{0},Asset:{1}", bundleName, assetName);
                return null;
            }
            return bamap[key].Item2;
        }
        /// <summary>
        /// 获得Bundle/Asset映射值
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static Tuple<int, string> GetBAValue(string bundleName, string assetName)
        {
            string key = bundleName.ToLower() + assetName;
            if (!bamap.ContainsKey(key))
            {
                Error("没有这个资源,Bundle:{0},Asset:{1}", bundleName, assetName);
                return null;
            }
            return bamap[key];
        }
        #endregion

        #region Asset
        /// <summary>
        /// 根据Bundle名称和资源名称加载资源
        /// 例子: Prefab,Chara_xxx
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static Asset LoadAsset<T>(string bundleName, string assetName) 
        {
            return LoadAssetInternal(bundleName, assetName, typeof(T),  AssetBundleLoadType.Normal);
        }
        public static Asset LoadAssetAsync<T>(string bundleName, string assetName) 
        {
            return LoadAssetInternal(bundleName, assetName, typeof(T),  AssetBundleLoadType.Async);
        }
        public static Asset LoadScene(string bundleName, string assetName)
        {
            return LoadAssetInternal(bundleName, assetName, typeof(Scene),  AssetBundleLoadType.Scene);
        }
        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="asset"></param>
        public static void UnloadAsset(Asset asset)
        {
            if (!asset.isDone)
            {
                Error("错误!资源没有加载完毕不能被卸载");
                return;
            }
            asset.Release();
            if (asset.references <= 0)
            {
                asset.Unload();
                Assets.Remove(asset.mapkey);
                AsyncAssets.Remove(asset);
                asset = null;
            }
        }
        static Asset LoadAssetInternal(string bundleName, string assetName, System.Type type, AssetBundleLoadType loadType)
        {
            if (bundleName == null|| assetName == null) return null;
            string mapkey = bundleName + assetName;
            Asset asset = null;
            if (Assets.ContainsKey(mapkey))
            {
                if (asset is BundleAssetScene)
                {
                    CLog.Error("同一个场景资源不能重复加载,请先卸载资源!");
                    return null;
                }
                asset = Assets[mapkey];
            }
            else
            {
                if (loadType == AssetBundleLoadType.Normal)
                    asset = new BundleAsset(bundleName, assetName, type);
                else if (loadType == AssetBundleLoadType.Async)
                    asset = new BundleAssetAsync(bundleName, assetName, type);
                else if (loadType == AssetBundleLoadType.Scene)
                    asset = new BundleAssetScene(bundleName, assetName, type);
                asset.Load();
                Assets.Add(mapkey, asset);
            }
            asset.Retain();
            if (loadType== AssetBundleLoadType.Async||
                loadType== AssetBundleLoadType.Scene)
            {
                AsyncAssets.Add(asset);
            }
            return asset;
        }
        #endregion

        #region Bundle
        public static void UnloadBundle(Bundle bundle)
        {
            if (!bundle.isDone|| bundle == null)
            {
                CLog.Error("错误!不能再资源没有加载完毕的时候卸载");
                return;
            }
            bundle.Release();
            if (bundle.references <= 0)
            {
                bundle.Unload();

                UnloadDependencies(bundle);

                Bundles.Remove(bundle.name);
                bundle = null;
            }
        }
        static void UnloadDependencies(Bundle bundle)
        {
            foreach (var item in bundle.dependencies)
            {
                item.Release();
            }
            bundle.dependencies.Clear();
        }
        public static Bundle LoadBundle(string bundleName,string dlc,bool asyncRequest=false)
        {
            if (bundleName.IsInvStr() || dlc.IsInvStr())
                return null;
            //如果bundleName == dlc 表示LoadingAssetBundleManifest
            bool isLoadingAssetBundleManifest = bundleName == dlc;
            //是否为预加载Bundle
            bool isPreload = bundleName == BaseConstMgr.BN_Shader|| bundleName== BaseConstMgr.BN_Shared;
            var url =Path.Combine(BaseConstMgr.Path_StreamingAssets, dlc) + "/" + bundleName;

            Bundle bundle = null;
            if (Bundles.ContainsKey(bundleName))
            {
                bundle = Bundles[bundleName];
            }
            else
            {
                if (asyncRequest)
                    bundle = new BundleAsync(url);
                else
                    bundle = new Bundle(url);
                bundle.name = bundleName;
                Bundles.Add(bundleName, bundle);
                bundle.Load();

                //加载依赖的的Bundle
                if (!isLoadingAssetBundleManifest&&
                    !isPreload)
                {
                    DLCItem dlcItem = GetDLCItem(dlc);
                    var dependencies = dlcItem.GetAllDependencies(bundleName);
                    if (dependencies!=null && dependencies.Length > 0)
                    {
                        foreach (var item in dependencies)
                        {
                            var dependencieBundle = LoadBundle(item, dlc, asyncRequest);
                            bundle.dependencies.Add(dependencieBundle);
                        }
                    }
                }
            }
            bundle.Retain();
            return bundle;
        }
        #endregion

        #region utile
        public static bool IsNextLogError { get; set; } = true;
        public static void Error(string str, params object[] ps)
        {
            if (IsNextLogError)
                CLog.Error(str, ps);
        }
        #endregion
    }
}