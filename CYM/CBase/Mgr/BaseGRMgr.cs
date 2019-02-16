//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CYM.DLC;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace CYM
{
    /// <summary>
    /// $LocalPlayer表示动态ID
    /// </summary>
    public class BaseGRMgr : BaseGFlowMgr, ILoader 
    {
        #region member variable
        public ObjectRegister<Material> Materials = new ObjectRegister<Material>();
        public ObjectRegister<GameObject> Prefabs = new ObjectRegister<GameObject>();
        public ObjectRegister<GameObject> Systems = new ObjectRegister<GameObject>();
        public ObjectRegister<GameObject> Perfomes = new ObjectRegister<GameObject>();
        public ObjectRegister<Sprite> Commonicons = new ObjectRegister<Sprite>();
        public ObjectRegister<AudioClip> Audios = new ObjectRegister<AudioClip>();
        public ObjectRegister<AudioClip> Musics = new ObjectRegister<AudioClip>();
        public ObjectRegister<GameObject> UIs = new ObjectRegister<GameObject>();
        public ObjectRegister<VideoClip> Videos = new ObjectRegister<VideoClip>();
        public ObjectRegister<AudioMixer> AudioMixer = new ObjectRegister<AudioMixer>();
        public ObjectRegister<RuntimeAnimatorController> Animators = new ObjectRegister<RuntimeAnimatorController>();
        public ObjectRegister<PhysicMaterial> PhysicMaterials = new ObjectRegister<PhysicMaterial>();
        public ObjectRegister<Sprite> BGs = new ObjectRegister<Sprite>();
        public ObjectRegister<Texture2D> Textures = new ObjectRegister<Texture2D>();
        #endregion

        #region dynamic
        /// <summary>
        /// 动态资源
        /// </summary>
        static Dictionary<string, Func<Sprite>> dynamicIcon = new Dictionary<string, Func<Sprite>>();
        static Dictionary<string, Func<AudioClip>> dynamicAudioClip= new Dictionary<string, Func<AudioClip>>();
        protected virtual void AddDynamic(string key,Func<Sprite> func)
        {
            dynamicIcon.Add(BaseConstMgr.Prefix_DynamicTrans + key, func);
        }
        protected virtual void AddDynamic(string key, Func<AudioClip> func)
        {
            dynamicAudioClip.Add(BaseConstMgr.Prefix_DynamicTrans + key, func);
        }
        #endregion

        #region methon
        public override void OnEnable()
        {
            base.OnEnable();
            OnAddDynamicRes();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }

        protected virtual void OnAddDynamicRes()
        {

        }

        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }

        public AudioClip GetMusic(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_Music, name, Musics);
        }

        public AudioClip[] GetMusics(List<string> names)
        {
            List<AudioClip> clips = new List<AudioClip>();
            for (int i = 0; i < names.Count; ++i)
                clips.Add(GetMusic(names[i]));
            return clips.ToArray();
        }

        public GameObject GetPrefab(string unitName, bool instance = false)
        {
            var ret = GetResWithCache(BaseConstMgr.BN_Prefab, unitName, Prefabs);
            if (instance)
                return GameObject.Instantiate(ret);
            else
                return ret;
        }

        public GameObject GetSystem(string unitName,bool instance=false)
        {
            var ret = GetResWithCache(BaseConstMgr.BN_System, unitName, Systems);
            if (instance)
                return GameObject.Instantiate(ret);
            else
                return ret;
        }

        public GameObject GetPerform(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_Perform, name, Perfomes);
        }

        public AudioClip GetAudioClip(string name)
        {
            if (dynamicAudioClip.ContainsKey(name))
                return dynamicAudioClip[name].Invoke();
            return GetResWithCache(BaseConstMgr.BN_Audio, name, Audios);
        }

        public Material GetMaterial(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_Materials, name, Materials);
        }

        public GameObject GetUI(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_UI, name, UIs);
        }

        public VideoClip GetVideo(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_Video, name, Videos);
        }

        public AudioMixer GetAudioMixer(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_AudioMixer, name, AudioMixer);
        }

        public Sprite GetBG(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_BG, name, BGs);            
        }

        public RuntimeAnimatorController GetAnimator(string name,bool logError=true)
        {
            return GetResWithCache(BaseConstMgr.BN_Animator, name, Animators, logError);
        }

        public PhysicMaterial GetPhysicMaterial(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_PhysicMaterial, name, PhysicMaterials);       
        }

        public Sprite GetIcon(string name, bool isLogError = true)
        {
            if (dynamicIcon.ContainsKey(name))
                return dynamicIcon[name].Invoke();
            return GetResWithCache(BaseConstMgr.BN_Icon, name, Commonicons, isLogError);
        }

        public Texture2D GetTexture(string name)
        {
            return GetResWithCache(BaseConstMgr.BN_Texture, name, Textures);
        }

        public GameObject GetResources(string path, bool instance = false) 
        {
            var temp = Resources.Load<GameObject>(path);

            if (instance)
                return GameObject.Instantiate(temp);
            else
                return temp;
        }

        public void ClearAll()
        {
            Commonicons.Clear();
            Materials.Clear();
            Prefabs.Clear();
            Systems.Clear();
            Perfomes.Clear();
            Audios.Clear();
            Musics.Clear();
            UIs.Clear();
            Videos.Clear();
            AudioMixer.Clear();
            Animators.Clear();
            PhysicMaterials.Clear();
            BGs.Clear();
        }
        #endregion

        #region 语法糖
        public Material FontRendering=> GetMaterial("FontRendering");
        public Material ImageGrey => GetMaterial("ImageGrey");
        #endregion

        #region utile
        private T GetResWithCache<T>(string bundle, string name, ObjectRegister<T> cache, bool logError = true) where T : UnityEngine.Object
        {
            if (name.IsInvStr())
                return null;
            if (cache.ContainsKey(name))
            {
                return cache[name];
            }
            else
            {
                DLCAssetMgr.IsNextLogError = logError;
                T retGO = SelfBaseGlobal.DLCMgr.LoadAsset<T>(bundle, name);
                if (retGO == null)
                {
                    if (logError)
                    {
                        CLog.Error("no this res in bundle {0}, name = {1}", bundle, name);
                    }
                }
                else
                {
                    cache.Add(retGO);
                }
                return retGO;
            }
        }
        #endregion

        #region Callback
        public IEnumerator Load()
        {
            yield break;
        }
        public string GetLoadInfo()
        {
            return "Load Resources";
        }
        #endregion
    }

}