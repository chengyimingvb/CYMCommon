using System;
using System.Collections.Generic;
using CYM.DLC;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CYM
{
    /// <summary>
    /// 战场管理器
    /// OnLoadBattleStart
    /// RandTip
    /// Callback_OnBattleLoad
    /// OnLoadSceneStart
    /// LoadScene
    /// SetActiveScene
    /// Callback_OnBattleLoadedScene
    /// BeforeLoadResources
    /// Callback_OnReadBattleDataStart
    /// StartReadGameData
    /// Callback_OnReadBattleDataEnd
    /// System.GC.Collect()
    /// Callback_OnBattleLoaded
    /// OnLoadBattleEnd
    /// Callback_OnGameStart
    /// BattleStart
    /// Callback_OnStartCustomBattleCoroutine
    /// CustomStartBattleCoroutine
    /// Callback_OnEndCustomBattleCoroutine
    /// Callback_OnGameStartOver
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public class BaseBattleMgr<TData> : BaseGlobalCoreMgr, IBaseBattleMgr, ITableDataMgr<TData> where TData : TDBaseBattleData, new()
    {
        #region Callback
        public event Callback Callback_OnStartNewGame;
        public event Callback Callback_OnGameStartOver;
        public event Callback Callback_OnBackToStart;
        public event Callback Callback_OnBattleLoad;
        public event Callback Callback_OnBattleLoaded;
        public event Callback Callback_OnBattleLoadedScene;
        public event Callback Callback_OnBattleUnLoad;
        public event Callback Callback_OnBattleUnLoaded;
        public event Callback Callback_OnGameStart;
        public event Callback Callback_OnReadBattleDataStart;
        public event Callback Callback_OnReadBattleDataEnd;
        public event Callback<string, float> Callback_OnLoadingProgressChanged;
        public event Callback Callback_OnStartCustomBattleCoroutine;
        public event Callback Callback_OnEndCustomBattleCoroutine;
        public event Callback Callback_OnRandTip;
        #endregion

        #region prop
        /// <summary>
        /// 延迟几秒加载场景(因为加载场景会卡顿),留给UI淡入淡出的时间
        /// </summary>
        protected float DelayLoadSceneTime = 0.5f;
        /// <summary>
        /// 战场内游戏的时间
        /// </summary>
        public Timer PlayTimer { get; protected set; } = new Timer();
        /// <summary>
        /// 加载时间
        /// </summary>
        public float LoadTime { get; private set; }

        /// <summary>
        /// 是否正在加载场景
        /// </summary>
        public bool IsLoadingBattle { get; private set; }

        /// <summary>
        /// 是否在战场
        /// </summary>
        public bool IsInBattle { get { return CurData != null; } }

        public int LoadBattleCount { get;private set; }

        /// <summary>
        /// 是否已经加载完毕战场
        /// </summary>
        public bool IsStartBattle { get; protected set; } = false;
        /// <summary>
        /// 当前游戏存档数据
        /// </summary>
        protected BaseDBGameData CurBaseGameData => SelfBaseGlobal.DBMgr.CurBaseGameData;

        /// <summary>
        /// 当前战场
        /// </summary>
        public virtual TData CurData { get; set; }

        /// <summary>
        /// Table
        /// </summary>
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 锁定游戏开始流程
        /// </summary>
        public bool IsLockGameStartFlow { get; private set; } = false;

        /// <summary>
        /// 场景的Bundle资源
        /// </summary>
        protected Asset SceneAsset;

        protected BaseCoroutineMgr BattleCoroutine => SelfBaseGlobal.BattleCoroutine;
        protected BaseCoroutineMgr CommonCoroutine => SelfBaseGlobal.CommonCoroutine;
        protected BaseCoroutineMgr MainUICoroutine => SelfBaseGlobal.MainUICoroutine;
        #endregion

        #region 生命周期函数
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();

        }
        public override void OnEnable()
        {
            base.OnEnable();


        }
        public override void OnDisable()
        {

            base.OnDisable();
        }
        public override void GameLogicTurn()
        {

        }
        #endregion

        #region set
        /// <summary>
        /// 解锁/锁定游戏开始流程
        /// </summary>
        /// <param name="b"></param>
        public void LockGameStartFlow(bool b)
        {
            IsLockGameStartFlow = b;
        }
        #endregion

        #region Load
        /// <summary>
        /// 加载新游戏
        /// </summary>
        public virtual void StartNewGame(string battleId = "")
        {
            if (IsInBattle)
            {
                CLog.Error("正在游戏中");
                return;
            }

            TDBaseBattleData tempData = Table.Find(battleId);
            if (tempData == null)
            {
                CLog.Error("没有这个战场:{0}", battleId);
                return;
            }

            BaseDBGameData data = SelfBaseGlobal.DBMgr.StartNewGame(); 
            if (data == null)
            {
                CLog.Error("游戏存档为空");
                return;
            }
            data.GameNetMode = GameNetMode.PVE;
            data.GamePlayStateType = GamePlayStateType.NewGame;
            LoadBattle(tempData);
            Callback_OnStartNewGame?.Invoke();
        }
        /// <summary>
        /// 继续游戏
        /// </summary>
        public virtual void ContinueGame()
        {
            SelfBaseGlobal.DBMgr.UseRemoteArchives(!SelfBaseGlobal.PrefsMgr.GetLastAchiveLocal());
            LoadGame(SelfBaseGlobal.PrefsMgr.GetLastAchiveID());
        }
        /// <summary>
        /// 加载游戏
        /// </summary>
        public virtual void LoadGame(string dbKey)
        {
            if (IsInBattle)
            {
                ReloadGame(dbKey);
                return;
            }

            BaseDBGameData data = SelfBaseGlobal.DBMgr.LoadGame(dbKey);
            if (data == null)
            {
                CLog.Error("游戏存档为空");
                return;
            }

            TDBaseBattleData tempData = Table.Find(data.BattleID);
            if (tempData == null)
            {
                CLog.Error("没有这个战场:{0}", data.BattleID);
                return;
            }
            data.GamePlayStateType = GamePlayStateType.LoadGame;
            LoadBattle(tempData);
        }
        /// <summary>
        /// 重载游戏
        /// </summary>
        /// <param name="dbKey"></param>
        protected void ReloadGame(string dbKey)
        {
            if (!IsInBattle)
            {
                CLog.Error("不在游戏中");
                return;
            }
            UnLoadBattle(() => LoadGame(dbKey));
        }
        public void GoToStart()
        {
            UnLoadBattle(() =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(BaseConstMgr.SCE_Start));
                Callback_OnBackToStart?.Invoke();
                MainUICoroutine.Run(BackToStart());
            });
        }
        /// <summary>
        /// 加载战斗场景
        /// </summary>
        protected void LoadBattle(TDBaseBattleData data, bool readData = true)
        {
            GC.Collect();
            MainUICoroutine.Kill();
            CurData = data.Copy() as TData;
            if (CurData != null)
            {
                CurData.OnBeAdded(SelfBaseGlobal);
                BattleCoroutine.Run(_LoadBattle(readData));
            }
            else
            {
                CLog.Error("Battle not found ！error id=" + data.TDID);
            }
        }
        /// <summary>
        /// 卸载战斗场景
        /// </summary>
        protected void UnLoadBattle(Callback onDone = null)
        {
            GC.Collect();
            BattleCoroutine.Kill();
            MainUICoroutine.Run(_UnLoadBattle(onDone));
        }
        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="tdid"></param>
        public void LoadBattle(string tdid)
        {
            CurBaseGameData.GamePlayStateType = GamePlayStateType.LoadGame;
            if (!IsInBattle)
            {
                TDBaseBattleData data = Table.Find(tdid);
                if (data == null)
                    return;
                LoadBattle(data, false);
            }
            else
            {
                UnLoadBattle(() => LoadBattle(tdid));
            }
        }
        #endregion

        #region phrase
        protected virtual void OnLoadBattleStart()
        {
            IsStartBattle = false;
        }
        protected virtual void OnLoadSceneStart()
        {

        }
        protected virtual void OnLoadBattleEnd()
        {
            //开始计时
            PlayTimer.Restart();
        }
        protected virtual void OnBattleOver()
        {
            PlayTimer.Stop();
        }
        #endregion

        #region enumator
        /// <summary>
        /// 随机提示
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> _RandTip()
        {
            while (IsLoadingBattle)
            {
                Callback_OnRandTip?.Invoke();
                yield return Timing.WaitForSeconds(2.0f);
            }
            yield break;
        }
        /// <summary>
        /// 加载战场
        /// </summary>
        /// <returns></returns>
        IEnumerator<float> _LoadBattle(bool readData = true)
        {
            SelfBaseGlobal.ResumeGame();
            SelfBaseGlobal.PlotMgr.ResumePlotMode();
            yield return Timing.WaitForOneFrame;
            OnLoadBattleStart();
            float startTime = Time.realtimeSinceStartup;
            IsLoadingBattle = true;
            //开始Tip
            BattleCoroutine.Run(_RandTip());
            //开始加载
            Callback_OnBattleLoad?.Invoke();
            yield return Timing.WaitForOneFrame;
            OnLoadSceneStart();
            //演示几秒,给UI渐变的时间
            yield return Timing.WaitForSeconds(DelayLoadSceneTime);
            //加载场景
            string sceneName = CurData.GetSceneName();
            SceneAsset = SelfBaseGlobal.DLCMgr.LoadScene("scene/" + sceneName, sceneName);
            while (!SceneAsset.isDone)
            {
                yield return Timing.WaitForOneFrame;
                Callback_OnLoadingProgressChanged?.Invoke("LoadingScene", SceneAsset.progress * 0.8f);
            }
            //延时一帧
            yield return Timing.WaitForOneFrame;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            Callback_OnBattleLoadedScene?.Invoke();
            //这里必须延迟一帧,等待UI创建,注册事件
            yield return Timing.WaitForOneFrame;
            //读取数据前,资源加载
            yield return Timing.WaitUntilDone(BattleCoroutine.Run(BeforeLoadResources()));
            Callback_OnLoadingProgressChanged?.Invoke("BeforeLoadResources", 0.9f);
            if (readData)
            {
                //读取战场数据
                Callback_OnReadBattleDataStart?.Invoke();
                SelfBaseGlobal.DBMgr.ReadGameDBData();
                Callback_OnReadBattleDataEnd?.Invoke();
            }
            else
            {
                yield return Timing.WaitForSeconds(0.1f);
            }
            //增加加载战场次数
            LoadBattleCount++;
            //读取数据后资源加载
            yield return Timing.WaitUntilDone(BattleCoroutine.Run(AffterLoadResources()));
            Callback_OnLoadingProgressChanged?.Invoke("AffterLoadResources", 0.95f);
            //卸载未使用的资源
            SelfBaseGlobal.DLCMgr.UnLoadBattleAssetBundle();
            GC.Collect();
            Callback_OnLoadingProgressChanged?.Invoke("GC", 1.0f);
            IsLoadingBattle = false;
            //场景加载结束
            Callback_OnBattleLoaded?.Invoke();
            OnLoadBattleEnd();
            //游戏开始
            Callback_OnGameStart?.Invoke();
            while (IsLockGameStartFlow)
                yield return Timing.WaitForSeconds(0.01f);
            yield return Timing.WaitUntilDone(BattleCoroutine.Run(BattleStart()));
            //进入自定义流程的时候暂停
            Callback_OnStartCustomBattleCoroutine?.Invoke();
            SelfBaseGlobal.PlotMgr.EnablePlotMode(true);
            yield return Timing.WaitUntilDone(SelfBaseGlobal.PlotMgr.CustomStartBattleCoroutine());
            SelfBaseGlobal.PlotMgr.EnablePlotMode(false);
            Callback_OnEndCustomBattleCoroutine.Invoke();
            IsStartBattle = true;
            Callback_OnGameStartOver?.Invoke();
        }
        /// <summary>
        /// 卸载战场
        /// </summary>
        /// <param name="onDone"></param>
        /// <returns></returns>
        IEnumerator<float> _UnLoadBattle(Callback onDone)
        {
            //暂停一段时间
            SelfBaseGlobal.PauseGame(true);
            yield return Timing.WaitForSeconds(0.1f);
            Callback_OnBattleUnLoad?.Invoke();
            yield return Timing.WaitForSeconds(1.0f);
            string sceneName = CurData.GetSceneName();
            var wait = SceneManager.UnloadSceneAsync(sceneName);
            while (!wait.isDone)
            {
                yield return Timing.WaitForOneFrame;
                Callback_OnLoadingProgressChanged?.Invoke("UnloadScene", wait.progress);
            }

            //卸载未使用的资源
            SelfBaseGlobal.DLCMgr.UnloadAsset(SceneAsset);
            SelfBaseGlobal.DLCMgr.UnLoadBattleAssetBundle();
            CurData.OnBeRemoved();
            CurData = null;

            Callback_OnLoadingProgressChanged?.Invoke("GC", 1.0f);
            BaseGlobalMonoMgr.RemoveAllNull();
            GC.Collect();
            yield return Timing.WaitForSeconds(0.1f);
            Callback_OnBattleUnLoaded?.Invoke();
            SelfBaseGlobal.ResumeGame();
            onDone?.Invoke();
        }
        /// <summary>
        /// 读取数据前加载资源
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<float> BeforeLoadResources()
        {
            yield return Timing.WaitForOneFrame;
        }
        /// <summary>
        /// 读取数据后加载资源
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<float> AffterLoadResources()
        {
            yield return Timing.WaitForOneFrame;
        }
        /// <summary>
        /// 关卡开始
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<float> BattleStart()
        {
            yield return Timing.WaitForOneFrame;
        }
        /// <summary>
        /// 回到初始场景
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator<float> BackToStart()
        {
            yield return Timing.WaitForOneFrame;
        }
        #endregion

        #region is
        public bool IsFirstLoad()
        {
            return LoadBattleCount == 1;
        }
        #endregion

        #region DB
        public override void Read1<TDBData>(TDBData data)
        {
            base.Read1(data);
            LoadBattleCount = data.LoadBattleCount;
        }
        public override void Write<TDBData>(TDBData data)
        {
            base.Write(data);
            data.BattleID = CurData.TDID;
            data.LoadBattleCount = LoadBattleCount;
        }
        #endregion
    }

}