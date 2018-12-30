//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public enum LoadEndType
    {
        Success = 0,
        Failed = 1,
    }
    public class BaseLoaderMgr : BaseGlobalCoreMgr
    {
        #region Callback Val
        /// <summary>
        /// 当一个Loader加载完成
        /// </summary>
        public event Callback<LoadEndType, string> Callback_OnLoadEnd;
        /// <summary>
        /// 加载开始
        /// </summary>
        public event Callback Callback_OnStartLoad;
        /// <summary>
        /// 当所有的loader都加载完成
        /// </summary>
        public event Callback Callback_OnAllLoadEnd;
        /// <summary>
        /// 当所有的loader都加载完成
        /// </summary>
        public event Callback Callback_OnAllLoadEnd2;
        #endregion

        #region member variable
        readonly List<ILoader> loderList = new List<ILoader>();
        private string LoadInfo;
        public bool IsLoadEnd { get; set; }
        public string ExtraLoadInfo { get; set; }
        Timer GUITimer = new Timer(0.2f);
        #endregion

        #region property

        public float Percent { get; set; }
        #endregion

        #region methon
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedGUI = true;
        }
        string[] dot = new string[] { ".", "..", "...", "....", ".....", "......" };
        int dotIndex = 0;
        Timer updateTimer = new Timer(0.02f);
        public override void OnGUIPaint()
        {
            if (!IsLoadEnd)
            {
                GUI.Label(new Rect(10, 10, Screen.width, Screen.height), LoadInfo + dot[dotIndex]);
                if (GUITimer.CheckOver())
                {
                    dotIndex++;
                    if (dotIndex >= dot.Length)
                        dotIndex = 0;
                }
            }
        }
        IEnumerator IEnumerator_Load()
        {
            Callback_OnStartLoad?.Invoke();
            for (int i = 0; i < loderList.Count; ++i)
            {
                LoadInfo = loderList[i].GetLoadInfo();
                yield return Mono.StartCoroutine(loderList[i].Load());
                Percent = i / loderList.Count;
            }
            Callback_OnLoadEnd?.Invoke(LoadEndType.Success, LoadInfo);
            IsLoadEnd = true;
            Callback_OnAllLoadEnd?.Invoke();
            Callback_OnAllLoadEnd2?.Invoke();
        }
        void StartLoader(params ILoader[] loaders)
        {
            if (loaders == null || loaders.Length == 0)
            {
                CLog.Error("错误,没有Loader");
                return;
            }
            foreach (var item in loaders)
            {
                loderList.Add(item);
            }
            IsLoadEnd = false;
            Mono.StartCoroutine(IEnumerator_Load());
        }
        #endregion

        #region Callback
        protected override void OnDownloadedAllBundle()
        {
            base.OnDownloadedAllBundle();
            StartLoader(
                SelfBaseGlobal.LogoMgr,
                SelfBaseGlobal.DLCMgr,
                SelfBaseGlobal.LangMgr,
                SelfBaseGlobal.LuaMgr,
                SelfBaseGlobal.GRMgr,
                SelfBaseGlobal.TextAssetsMgr
                );
        }
        #endregion
    }

}

