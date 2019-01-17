using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYM.DLC
{
    public enum LoadStateType
    {
        None,
        Loading,
        Succ,
        Fail,
    }
    public class Asset : IEnumerator
    {
        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get
            {
                return null;
            }
        }

        #endregion

        public int references { get; private set; }

        public string assetName { get; protected set; }
        public string bundleName { get; protected set; }
        public string assetFullPath { get; private set; }
        public string realBundleName { get; private set; }
        public string dlcName { get; private set; }
        public string mapkey { get; private set; }

        public System.Type assetType { get; protected set; }

        public virtual bool isDone { get { return true; } }

        public virtual float progress { get { return 1; } }

        public Object asset { get; protected set; }

        internal Asset(string bundleName, string assetName,System.Type type)
        {
            this.assetName = assetName;
            this.bundleName = bundleName;
            this.assetFullPath = DLCAssetMgr.GetAssetPath(bundleName, assetName);
            this.mapkey = bundleName + assetName;
            this.realBundleName = DLCAssetMgr.GetBundleName(assetFullPath);
            this.dlcName = DLCAssetMgr.GetDLCName(bundleName, assetName);
            assetType = type;
        }

        internal void Load()
        {
            OnLoad();
        }

        internal void Unload()
        {
            if (asset != null)
            {
                if (asset.GetType() != typeof(GameObject))
                {
                    Resources.UnloadAsset(asset);
                }
                asset = null;
            }
            OnUnload();
        }

        protected virtual void OnLoad()
        {
        }

        public virtual void OnUpdate()
        {

        }

        protected virtual void OnUnload()
        {

        }

        /// <summary>
        /// 不要手动调用此函数
        /// </summary>
		internal void Retain()
        {
            references++;
        }

        /// <summary>
        /// 不要手动调用此函数
        /// </summary>
        internal void Release()
        {
            if (--references < 0)
            {
                CLog.Error("refCount < 0");
            }
        }
    }

    public class BundleAsset : Asset
    {
        protected Bundle request;

        internal BundleAsset(string bundleName, string assetName, System.Type type) : base( bundleName, assetName, type)
        {
        }

        protected override void OnLoad()
        {
#if UNITY_EDITOR
            if (DLCConfig.Ins.IsEditorMode)
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetFullPath, assetType);
                return;
            }
#endif
            {
                request = DLCAssetMgr.LoadBundle(realBundleName,dlcName);
                if (request == null)
                {
                    if(DLCAssetMgr.IsNextLogError)
                        CLog.Error("错误:realBundleName:{0} dlcName:{1}", realBundleName, dlcName);
                    return;
                }
                asset = request.LoadAsset(assetName, assetType);
            }

        }


        protected override void OnUnload()
        {
            if (request != null)
                DLCAssetMgr.UnloadBundle(request);
        }
    }

    public class BundleAssetAsync : BundleAsset
    {
        protected AssetBundleRequest abRequest;
        protected int loadCount=0;

        internal BundleAssetAsync(string bundleName, string assetName, System.Type type) : base(bundleName,assetName, type)
        {

        }

        protected override void OnLoad()
        {
#if UNITY_EDITOR
            if (DLCConfig.Ins.IsEditorMode)
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetFullPath, assetType);
                return;
            }
#endif
            {
                request = DLCAssetMgr.LoadBundle(realBundleName, dlcName,true);
                loadState = LoadStateType.Loading;
                loadCount = 0;
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            abRequest = null;
        }

        public override void OnUpdate()
        {
            if (isDone)
                return;

#if UNITY_EDITOR
            if (DLCConfig.Ins.IsEditorMode)
            {
                
                if (asset == null)
                    loadState = LoadStateType.Fail;
                else
                    loadState = LoadStateType.Succ;
                return;
            }
#endif
            {

                if (request == null || request.error != null)
                {
                    loadState = LoadStateType.Fail;
                    return;
                }

                if (request.isDone && loadCount == 0)
                {
                    abRequest = request.LoadAssetAsync(Path.GetFileName(assetFullPath), assetType);
                    loadCount++;
                    if(abRequest==null)
                        loadState = LoadStateType.Fail;
                }
                else if (abRequest!=null&&abRequest.isDone && loadCount == 1)
                {
                    loadCount++;
                    asset = abRequest.asset;
                    if (asset == null)
                        loadState = LoadStateType.Fail;
                    else
                        loadState = LoadStateType.Succ;
                }
                else
                {
                    loadState = LoadStateType.Loading;
                }
                
            }
        }

        protected LoadStateType loadState = LoadStateType.None;

        public override bool isDone
        {
            get
            {
                return loadState == LoadStateType.Fail ||
                    loadState == LoadStateType.Succ;

            }
        }

        public override float progress
        {
            get
            {

#if UNITY_EDITOR
                if (DLCConfig.Ins.IsEditorMode)
                {

                    if (asset == null)
                        return 0.0f;
                    else
                        return 1.0f;
                }
#endif
                {
                    if (request == null|| request.error != null)
                        return 0.0f;

                    return (abRequest==null?0.0f: abRequest.progress + request.progress) * 0.5f;
                }
            }
        }
    }

    public class BundleAssetScene : BundleAssetAsync
    {
        AsyncOperation asyncOperation;
        internal BundleAssetScene(string bundleName,string assetName, System.Type type) : base(bundleName, assetName, type)
        {

        }


        protected override void OnLoad()
        {
#if UNITY_EDITOR
            if (DLCConfig.Ins.IsEditorMode)
            {
                asyncOperation = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(assetFullPath);
                loadState = LoadStateType.Loading;
                loadCount = 0;
                return;
            }
#endif
            {
                request = DLCAssetMgr.LoadBundle(realBundleName,dlcName,true);
                loadState = LoadStateType.Loading;
                loadCount = 0;
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            asyncOperation = null;
        }

        public override void OnUpdate()
        {
            if (isDone)
                return;

#if UNITY_EDITOR
            if (DLCConfig.Ins.IsEditorMode)
            {

                if (asyncOperation == null)
                    loadState = LoadStateType.Fail;
                else if(asyncOperation.isDone)
                    loadState = LoadStateType.Succ;
                else
                    loadState = LoadStateType.Loading;
                return;
            }
#endif
            {

                if (request == null || request.error != null)
                {
                    loadState = LoadStateType.Fail;
                    return;
                }

                if (request.isDone && loadCount == 0)
                {
                    asyncOperation = SceneManager.LoadSceneAsync(Path.GetFileNameWithoutExtension(assetFullPath), LoadSceneMode.Additive);
                    loadCount++;
                    if (asyncOperation == null)
                        loadState = LoadStateType.Fail;
                }
                else if (asyncOperation!=null&&asyncOperation.isDone && loadCount == 1)
                {
                    loadCount++;
                    loadState = LoadStateType.Succ;
                }
                else
                {
                    loadState = LoadStateType.Loading;
                }

            }
        }

        public override float progress
        {
            get
            {
#if UNITY_EDITOR
                if (DLCConfig.Ins.IsEditorMode)
                {
                    if (asyncOperation == null)
                        return 0.0f;
                    return asyncOperation.progress;
                }
#endif
                {
                    if (request == null)
                        return 0.0f;
                    return (asyncOperation == null ? 0.0f : asyncOperation.progress + request.progress) * 0.5f;
                }

            }
        }
    }
}
