//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DG.Tweening;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Experimental.Input;

namespace CYM
{
    [RequireComponent(typeof(BaseGlobalMonoMgr))]
    public class BaseGlobal : BaseCoreMono
    {
        #region Inspector
        public List<GameObject> InstantiateObjs;
        public InputActionAssetReference InputAssetReference;
        #endregion

        #region need New :这里的对象必须在基类里面手动赋值
        public static BaseGlobal Ins { get; protected set; }
        public IBaseSettingsMgr SettingsMgr { get; protected set; }
        public BaseConstMgr ConstMgr { get; protected set; }
        public BaseVersionMgr VersionMgr { get; protected set; }
        public BaseDLCMgr DLCMgr { get; protected set; }
        public BaseGRMgr GRMgr { get; protected set; }
        public BaseLoaderMgr LoaderMgr { get; protected set; }
        public BaseLuaMgr LuaMgr { get; protected set; }
        public BaseTextAssetsMgr TextAssetsMgr { get; protected set; }
        public BaseAudioMgr AudioMgr { get; protected set; }
        public BaseLanguageMgr LangMgr { get; protected set; }
        public BaseCameraMgr CameraMgr { get; protected set; }
        public BaseActionConditionMgr ACM { get; protected set; }
        public BaseInputMgr InputMgr { get; protected set; }
        public BaseDataParseMgr DPMgr { get; protected set; }
        public BasePoolMgr PoolMgr { get; protected set; }
        public BaseVideoMgr VideoMgr { get; protected set; }
        public BaseBGMMgr BGMMgr { get; protected set; }
        public BaseDevConsoleMgr DevConsoleMgr { get; protected set; }
        public BaseFPSMgr FPSMgr { get; protected set; }
        public BasePlatSDKMgr PlatSDKMgr { get; protected set; }
        public BaseProfilterMgr ProfilterMgr { get; protected set; }
        public IBaseBattleMgr BattleMgr { get; protected set; }
        public IBasePlotMgr PlotMgr { get; protected set; }
        public IBaseDBMgr DBMgr { get; protected set; }
        public IBaseDifficultMgr DiffMgr { get; protected set; }
        public IBaseScreenMgr ScreenMgr { get; protected set; }
        public IBaseTalkMgr TalkMgr { get; protected set; }
        public IBaseNarrationMgr NarrationMgr { get; protected set; }
        public BaseAnalyticsMgr AnalyticsMgr { get; protected set; }
        public BaseFeedbackMgr FeedbackMgr { get; protected set; }
        public BasePrefsMgr PrefsMgr { get; protected set; }
        public BaseCursorMgr CursorMgr { get; protected set; }
        public BaseDateTimeMgr DateTimeMgr { get; protected set; }
        public BaseLogicTurnMgr LogicTurnMgr { get; protected set; }
        public BaseLogoMgr LogoMgr { get; protected set; }
        public BaseExcelMgr ExcelMgr { get; protected set; }
        #endregion

        #region 非必要组件
        public BaseFOWMgr FOWMgr { get; protected set; }
        public BaseTerrainGridMgr TerrainGridMgr { get; protected set; }
        #endregion

        #region prop
        public static GameObject TempGO { get; private set; }
        public static Transform TempTrans => TempGO.transform;
        public BaseCoroutineMgr CommonCoroutine { get; protected set; }
        public BaseCoroutineMgr MainUICoroutine { get; protected set; }
        public BaseCoroutineMgr BattleCoroutine { get; protected set; }
        #endregion

        #region methon
        public override LayerData LayerData =>BaseConstMgr.Layer_System;
        public override void Awake()
        {
            if (InstantiateObjs != null)
                foreach (var item in InstantiateObjs)
                    GameObject.Instantiate(item);
            if (TempGO == null)
            {
                TempGO = new GameObject("TempGO");
                TempGO.hideFlags = HideFlags.HideInHierarchy;
            }
            Application.wantsToQuit += OnWantsToQuit;
            Ins = this;
            MonoType = MonoType.Global;
            base.Awake();
            DontDestroyOnLoad(this);
            AddPlatformSDKComponet();
            DOTween.Init();
            CLog.LoadTag(BaseConstMgr.Path_LoggerTag);
            //携程
            CommonCoroutine = new BaseCoroutineMgr("Common");
            MainUICoroutine = new BaseCoroutineMgr("MainUI");
            BattleCoroutine = new BaseCoroutineMgr("Battle");
            Pos = BaseConstMgr.FarawayPos;

            //CALLBACK
            LuaMgr.Callback_OnLuaParseEnd += OnLuaParsed;
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedGUI = true;
            NeedUpdate = true;
            NeedLateUpdate = true;
            NeedGameLogicTurn = true;
        }
        public override void OnDestroy()
        {
            //CALLBACK
            LuaMgr.Callback_OnLuaParseEnd -= OnLuaParsed;
            Application.wantsToQuit -= OnWantsToQuit;
            base.OnDestroy();
        }
        /// <summary>
        /// 添加组建
        /// </summary>
        protected override void AttachComponet()
        {
        }
        /// <summary>
        /// 添加平台SDK组建
        /// </summary>
        protected void AddPlatformSDKComponet()
        {
            var type = VersionMgr.Config.Distribution;
            if (type == Distribution.Steam)
                AddSteamSDKMgr();
            else if (type == Distribution.Rail)
                AddRailSDKMgr();
            else if (type == Distribution.Turbo)
                AddTurboSDKMgr();
            else if (type == Distribution.Trial)
                AddTrialSDKMgr();
            else if (type == Distribution.Gaopp)
                AddGaoppSDKMgr();
            else
                CLog.Error("未知SDK:" + type.ToString());
        }
        public override void Start()
        {
            base.Start();
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public void OnApplicationQuit()
        {
        }
        #endregion

        #region Add Platform SDK
        protected virtual void AddSteamSDKMgr()
        {
            PlatSDKMgr = AddComponent<BaseSteamSDKMgr>();
        }
        protected virtual void AddRailSDKMgr()
        {
            PlatSDKMgr = AddComponent<BaseRailSDKMgr>();
        }
        protected virtual void AddTurboSDKMgr()
        {
            PlatSDKMgr = AddComponent<BaseTurboSDKMgr>();
        }
        protected virtual void AddTrialSDKMgr()
        {
            PlatSDKMgr = AddComponent<BaseTrialSDKMgr>();
        }
        protected virtual void AddGaoppSDKMgr()
        {
            PlatSDKMgr = AddComponent<BaseGaoppSDKMgr>();
        }
        #endregion

        #region Callback
        protected virtual void OnLuaParsed()
        {

        }
        protected bool OnWantsToQuit()
        {
            return false;
        }
        #endregion

        #region set
        public void Quit()
        {
            if (!Application.isEditor)
            {
                DLCMgr.UnLoadLoadedAssetBundle();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                Application.Quit();
                System.Environment.Exit(0);
            }
        }
        BoolState BoolPause { get; set; } = new BoolState();
        /// <summary>
        /// 停止游戏
        /// </summary>
        public void PauseGame(bool b)
        {
            BoolPause.Push(b);
            _doPauseGame();
        }
        public void ResumeGame()
        {
            BoolPause.Reset();
            _doPauseGame();
        }
        private void _doPauseGame()
        {
            if (BoolPause.IsIn())
            {
                BaseGlobalMonoMgr.SetPauseType(MonoType.Unit);
                BattleCoroutine.Pause();
                KinematicCharacterSystem.AutoSimulation = false;
            }
            else
            {
                BaseGlobalMonoMgr.SetPauseType(MonoType.None);
                BattleCoroutine.Resume();
                KinematicCharacterSystem.AutoSimulation = true;
            }
        }
        #endregion

        #region get
        public Transform GetTransform(Vector3 pos)
        {
            TempTrans.position = pos;
            return TempTrans;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否暂停游戏
        /// </summary>
        public bool IsPause
        {
            get
            {
                return BoolPause.IsIn();
            }
        }
        #endregion

        #region window 专有
        const uint SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        const int WS_BORDER = 1;
        const int WS_POPUP = 0x800000;
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        IEnumerator HideWindowBorder()
        {
            yield return new WaitForSeconds(0.1f);      //不知道为什么发布于行后，设置位置的不会生效，我延迟0.1秒就可以
            SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_POPUP);      //无边框
        }
        #endregion
    }
}
