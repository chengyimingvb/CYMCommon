using UnityEngine;
#if UNITY_EDITOR	
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using CYM;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using CYM.DLC;
/*
AssetBundle 管理类
作者：CYM
*/

namespace CYM
{
    public class BaseDLCMgr : BaseGFlowMgr,ILoader
    {
        #region Callback Val
        /// <summary>
        /// 开始下载AssetBundle
        /// </summary>
        public Callback Callback_OnStartDownloadAllBundle { get; set; }
        /// <summary>
        /// 资源全部Download完毕
        /// </summary>
        public Callback Callback_OnDownloadedAllBundle { get; set; }
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedGUI = true;
        }
        public override void OnStart()
        {
            base.OnStart();
            Callback_OnDownloadedAllBundle?.Invoke();
        }
        public override void OnUpdate()
        {
        }
        public override void OnGUIPaint()
        {
        }
        #endregion

        #region Set
        public void UnLoadLoadedAssetBundle()
        {
            AssetBundle.UnloadAllAssetBundles(false);
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        public virtual void UnLoadBattleAssetBundle()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        #endregion

        #region load
        public virtual T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            var temp = DLCAssetMgr.LoadAsset<T>(bundleName, assetName);
            if (temp == null)
                return null;
            return temp.asset as T;
        }
        public virtual Asset LoadAssetAsync<T>(string bundleName, string assetName)
        {
            return DLCAssetMgr.LoadAssetAsync<T>(bundleName, assetName);
        }
        public virtual Asset LoadScene(string bundleName, string assetName)
        {
            return DLCAssetMgr.LoadScene(bundleName, assetName);
        }
        /// <summary>
        /// 卸载资源
        /// </summary>
        public virtual void UnloadAsset(Asset asset)
        {
            DLCAssetMgr.UnloadAsset(asset);
        }

        public virtual IEnumerator Load()
        {
            string[] directories = Directory.GetDirectories(DLCAssetMgr.GetDLCRootPath());
            foreach (var directoryPath in directories)
            {
                DLCAssetMgr.LoadDLC(BaseFileUtils.GetFinalDirectoryName(directoryPath));
                yield return new WaitForEndOfFrame();
            }
        }

        public string GetLoadInfo()
        {
            return "Load DLC";
        }
        #endregion

    }

}