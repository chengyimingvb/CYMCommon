//------------------------------------------------------------------------------
// BaseGlobalCoreMgr.cs
// Copyright 2018 2018/4/24 
// Created by CYM on 2018/4/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;

namespace CYM
{
    public class BaseGlobalCoreMgr : BaseCoreMgr
    {
        IBaseBattleMgr BattleMgr=>SelfBaseGlobal.BattleMgr;
        BaseLuaMgr LuaMgr=>SelfBaseGlobal.LuaMgr;
        BaseLoaderMgr LoaderMgr=>SelfBaseGlobal.LoaderMgr;
        BaseDLCMgr ABMgr => SelfBaseGlobal.DLCMgr;
        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            BattleMgr.Callback_OnStartNewGame += OnStartNewGame;
            BattleMgr.Callback_OnGameStartOver += OnGameStartOver;
            BattleMgr.Callback_OnBackToStart += OnBackToStart;
            BattleMgr.Callback_OnBattleLoad += OnBattleLoad;
            BattleMgr.Callback_OnBattleLoaded += OnBattleLoaded;
            BattleMgr.Callback_OnBattleLoadedScene += OnBattleLoadedScene;
            BattleMgr.Callback_OnBattleUnLoad += OnBattleUnLoad;
            BattleMgr.Callback_OnBattleUnLoaded += OnBattleUnLoaded;
            BattleMgr.Callback_OnGameStart += OnGameStart;
            BattleMgr.Callback_OnReadBattleDataStart += OnReadBattleDataStart;
            BattleMgr.Callback_OnReadBattleDataEnd += OnReadBattleDataEnd;
            BattleMgr.Callback_OnLoadingProgressChanged += OnLoadingProgressChanged;
            BattleMgr.Callback_OnStartCustomBattleCoroutine += OnStartCustomBattleCoroutine;
            BattleMgr.Callback_OnEndCustomBattleCoroutine += OnEndCustomBattleCoroutine;

            LuaMgr.Callback_OnLuaParseEnd += OnLuaParseEnd;
            LuaMgr.Callback_OnLuaParseStart += OnLuaParseStart;

            LoaderMgr.Callback_OnStartLoad += OnStartLoad;
            LoaderMgr.Callback_OnLoadEnd += OnLoadEnd;
            LoaderMgr.Callback_OnAllLoadEnd += OnAllLoadEnd;
            LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;


            ABMgr.Callback_OnStartDownloadAllBundle += OnStartDownloadAllBundle;
            ABMgr.Callback_OnDownloadedAllBundle += OnDownloadedAllBundle;
        }

        public override void OnDisable()
        {
            BattleMgr.Callback_OnStartNewGame -= OnStartNewGame;
            BattleMgr.Callback_OnGameStartOver -= OnGameStartOver;
            BattleMgr.Callback_OnBackToStart -= OnBackToStart;
            BattleMgr.Callback_OnBattleLoad -= OnBattleLoad;
            BattleMgr.Callback_OnBattleLoaded -= OnBattleLoaded;
            BattleMgr.Callback_OnBattleLoadedScene -= OnBattleLoadedScene;
            BattleMgr.Callback_OnBattleUnLoad -= OnBattleUnLoad;
            BattleMgr.Callback_OnBattleUnLoaded -= OnBattleUnLoaded;
            BattleMgr.Callback_OnGameStart -= OnGameStart;
            BattleMgr.Callback_OnReadBattleDataStart -= OnReadBattleDataStart;
            BattleMgr.Callback_OnReadBattleDataEnd -= OnReadBattleDataEnd;
            BattleMgr.Callback_OnLoadingProgressChanged -= OnLoadingProgressChanged;
            BattleMgr.Callback_OnStartCustomBattleCoroutine -= OnStartCustomBattleCoroutine;
            BattleMgr.Callback_OnEndCustomBattleCoroutine -= OnEndCustomBattleCoroutine;

            LuaMgr.Callback_OnLuaParseEnd -= OnLuaParseEnd;
            LuaMgr.Callback_OnLuaParseStart -= OnLuaParseStart;

            LoaderMgr.Callback_OnStartLoad -= OnStartLoad;
            LoaderMgr.Callback_OnLoadEnd -= OnLoadEnd;
            LoaderMgr.Callback_OnAllLoadEnd -= OnAllLoadEnd;
            LoaderMgr.Callback_OnAllLoadEnd2 -= OnAllLoadEnd2;

            ABMgr.Callback_OnStartDownloadAllBundle -= OnStartDownloadAllBundle;
            ABMgr.Callback_OnDownloadedAllBundle -= OnDownloadedAllBundle;
            base.OnDisable();
        }
        #endregion

        #region Callback
        protected virtual void OnStartNewGame()
        {

        }
        protected virtual void OnGameStartOver()
        {

        }
        protected virtual void OnBackToStart()
        {

        }
        protected virtual void OnBattleLoad()
        {

        }
        protected virtual void OnBattleLoaded()
        {

        }
        protected virtual void OnBattleLoadedScene()
        {

        }
        protected virtual void OnBattleUnLoad()
        {

        }
        protected virtual void OnBattleUnLoaded()
        {

        }
        protected virtual void OnGameStart()
        {

        }
        protected virtual void OnReadBattleDataStart()
        {

        }
        protected virtual void OnReadBattleDataEnd()
        {

        }
        protected virtual void OnLoadingProgressChanged(string info, float val)
        {

        }

        protected virtual void OnLuaParseEnd()
        {

        }
        protected virtual void OnLuaParseStart()
        {

        }

        protected virtual void OnStartLoad()
        {

        }
        protected virtual void OnLoadEnd(LoadEndType type, string info)
        {

        }
        protected virtual void OnAllLoadEnd()
        {

        }
        protected virtual void OnAllLoadEnd2()
        {

        }

        protected virtual void OnDownloadedAllBundle()
        {
        }

        protected virtual void OnStartDownloadAllBundle()
        {
        }

        protected virtual void OnStartCustomBattleCoroutine()
        {

        }
        protected virtual void OnEndCustomBattleCoroutine()
        {

        }
        #endregion
    }
}