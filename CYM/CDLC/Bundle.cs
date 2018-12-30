using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.DLC
{
    public class Bundle 
    {
        public int references { get; private set; }
        public virtual string error { get; protected set; }
        public virtual float progress { get { return 1; } }
        public virtual bool isDone { get { return true; } }
        public virtual AssetBundle assetBundle { get { return _assetBundle; } }
		public readonly HashList<Bundle> dependencies = new HashList<Bundle>();
        public string path { get; protected set; }
		public string name { get; internal set; }
		//protected Hash128 version;

        AssetBundle _assetBundle; 

		internal Bundle(string url)
        {
			path = url; 
			//version = hash;
        }

        internal void Load()
        {
			//CLog.Info("Load " + path); 
            OnLoad(); 
        }

        internal void Unload()
        {
            //CLog.Info("Unload " + path);
            OnUnload(); 
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (error != null)
            {
                return null;
            }
            return assetBundle.LoadAsset(assetName, typeof(T)) as T;
        }

        public Object LoadAsset(string assetName, System.Type assetType)
        {
            if (error != null)
            {
                return null;
            }
            return assetBundle.LoadAsset(assetName, assetType);
        }

        public T[] LoadAllAssets<T>() where T : Object
        {
            if (error != null)
            {
                return null;
            }
            return assetBundle.LoadAllAssets<T>();
        }

        public AssetBundleRequest LoadAssetAsync(string assetName, System.Type assetType)
        {
            if (error != null)
            {
                return null;
            }
            return assetBundle.LoadAssetAsync(assetName, assetType);
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

        protected virtual void OnLoad()
        {
			_assetBundle = AssetBundle.LoadFromFile(path);
            if (_assetBundle == null)
            {
				error = path + " LoadFromFile failed.";
            } 
        }

        protected virtual void OnUnload()
        {
            if (_assetBundle != null)
            {
                _assetBundle.Unload(false);
                _assetBundle = null;
            }
        }  
    }

    public class BundleAsync : Bundle, IEnumerator
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
                return assetBundle;
            }
        }

        #endregion

        public override AssetBundle assetBundle
        {
            get
            {
                if (error != null)
                {
                    return null;
                }

                if (dependencies.Count == 0)
                {
                    return _request.assetBundle;
                }

                for (int i = 0, I = dependencies.Count; i < I; i++)
                {
                    var item = dependencies[i];
                    if (item.assetBundle == null)
                    {
                        return null;
                    }
                }

                return _request.assetBundle;
            }
        }

        public override float progress
        {
            get
            {
                if (error != null)
                {
                    return 1;
                }

                if (dependencies.Count == 0)
                {
                    return _request.progress;
                }

                float value = _request.progress;
                for (int i = 0, I = dependencies.Count; i < I; i++)
                {
                    var item = dependencies[i];
                    value += item.progress;
                }
                return value / (dependencies.Count + 1);
            }
        }

        public override bool isDone
        {
            get
            {
                if (error != null)
                {
                    return true;
                }

                if (dependencies.Count == 0)
                {
                    return _request.isDone;
                }

                for (int i = 0, I = dependencies.Count; i < I; i++)
                {
                    var item = dependencies[i];
                    if (item.error != null)
                    {
                        error = "Falied to load Dependencies " + item;
                        return true;
                    }
                    if (!item.isDone)
                    {
                        return false;
                    }
                }
                return _request.isDone;
            }
        }

        AssetBundleCreateRequest _request;

        protected override void OnLoad()
        {
			_request = AssetBundle.LoadFromFileAsync(path);
            if (_request == null)
            {
				error = path + " LoadFromFileAsync falied.";
            }
        }

        protected override void OnUnload()
        {
            if (_request != null)
            {
                if (_request.assetBundle != null)
                {
                    _request.assetBundle.Unload(false);
                }
                _request = null;
            }
        }

		internal BundleAsync(string url) : base(url)
        {

        }
    }
}
