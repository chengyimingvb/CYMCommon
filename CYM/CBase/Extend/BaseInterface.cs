//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using MoonSharp;
using UnityEngine;

namespace CYM
{
    public interface ICYMBase
    {
        int ID { get; set; }
        string TDID { get; set; }
    }
    public interface IMono
    {
        void OnEnable();
        void OnSetNeedFlag();
        void Awake();
        void Start();
        void OnAffterStart();
        void OnUpdate();
        void OnFixedUpdate();
        void OnDisable();
        void OnDestroy();
        T AddComponent<T>() where T : BaseCoreMgr, new();
        void RemoveComponent(BaseCoreMgr component);
    }
    public interface IUnit
    {
        /// <summary>
        /// 角色第一次创建，逻辑初始化的时候
        /// </summary>
        void Init();
        /// <summary>
        /// 角色复活后触发
        /// </summary>
        void ReBirth();
        /// <summary>
        /// 角色第一次创建或者复活都会触发
        /// </summary>
        void Birth();
        /// <summary>
        /// 角色第一次创建或者复活都会触发
        /// </summary>
        void Birth2();
        /// <summary>
        /// 角色第一次创建或者复活都会触发
        /// </summary>
        void Birth3();
        /// <summary>
        /// 角色假死亡
        /// </summary>
        /// <param name="caster"></param>
        void Death(BaseUnit caster);
        /// <summary>
        /// 角色真的死亡
        /// </summary>
        void RealDeath();
        /// <summary>
        /// 逻辑回合
        /// </summary>
        void GameLogicTurn();
        /// <summary>
        /// 帧回合
        /// </summary>
        /// <param name="gameFramesPerSecond"></param>
        void GameFrameTurn(int gameFramesPerSecond);
        /// <summary>
        /// 手动更新
        /// </summary>
        void ManualUpdate();

    }
    public interface IDBDataConvert
    {
        /// <summary>
        /// 游戏加载读取数据
        /// </summary>
        /// <param name="data"></param>
        void Read1<TDBData>(TDBData data) where TDBData : BaseDBGameData, new();
        void Read2<TDBData>(TDBData data) where TDBData : BaseDBGameData, new();
        void Read3<TDBData>(TDBData data) where TDBData : BaseDBGameData, new();
        /// <summary>
        /// 读取数据结束
        /// </summary>
        /// <param name="data"></param>
        void ReadEnd<TDBData>(TDBData data) where TDBData : BaseDBGameData, new();
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="data"></param>
        void Write<TDBData>(TDBData data) where TDBData : BaseDBGameData, new();
    }
    public interface ILoader
    {
        IEnumerator Load();
        string GetLoadInfo();
    }
    interface ICYMManager
    {
        int Count();
        bool IsEmpty();
        void Clear();
    }

    public enum AssetsPathType
    {
        Resources,
        AssetBundle,
    }
    public interface IResRegister<T2>
    {
        T2 this[string name] { get; }
        void Add(T2 c);
        void Add(string name, T2 c);
        void Remove(T2 c);
        void Remove(string name);
        T2 Data(string name);
        bool ContainsKey(string name);
        void Clear();
    }

    public interface ITDBase
    {
        void Add(MoonSharp.Interpreter.DynValue table);
    }

    public interface ITDLuaMgr
    {
        void OnLuaParseStart();
        void OnLuaParseEnd();
    }
    /// <summary>
    /// 动画触发器
    /// </summary>
    public interface IOnAnimTrigger
    {
        void OnAnimTrigger(int param);
    }

    /// <summary>
    /// 预加载对象
    /// </summary>
    public interface IPrespawner
    {
        List<string> GetPrespawnPerforms();
    }
    public interface ISpawnMgr<T> where T:ICYMBase
    {
        T Gold { get; set; }
        DicList<T> Data { get; set; }
        event Callback<T> Callback_OnAdd;
        event Callback<T> Callback_OnSpawnGold;
        event Callback<T> Callback_OnSpawn;
        event Callback<T> Callback_OnDespawn;
        void Clear();
        void OnSpawned(string id, T unit);
    }
    /// <summary>
    /// 表格数据管理器接口
    /// </summary>
    /// <typeparam name="TTable"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public interface ITableDataMgr<TData> where TData : TDValue, new()
    {
        LuaTDMgr<TData> Table { get; }
        TData CurData { get; set; }
    }

    /// <summary>
    /// 战场管理器接口
    /// </summary>
    public interface IBaseBattleMgr
    {
        void LockGameStartFlow(bool b);
        bool IsInBattle { get; }
        #region Callback
        event Callback Callback_OnStartNewGame;
        event Callback Callback_OnGameStartOver;
        event Callback Callback_OnBackToStart;
        event Callback Callback_OnBattleLoad;
        event Callback Callback_OnBattleLoaded;
        event Callback Callback_OnBattleLoadedScene;
        event Callback Callback_OnBattleUnLoad;
        event Callback Callback_OnBattleUnLoaded;
        event Callback Callback_OnGameStart;
        event Callback Callback_OnReadBattleDataStart;
        event Callback Callback_OnReadBattleDataEnd;
        event Callback<string, float> Callback_OnLoadingProgressChanged;
        event Callback Callback_OnStartCustomBattleCoroutine;
        event Callback Callback_OnEndCustomBattleCoroutine;
        event Callback Callback_OnRandTip;
        #endregion
    }
    /// <summary>
    /// 剧情管理器接口
    /// </summary>
    public interface IBasePlotMgr
    {
        void EnableAI(bool b);
        void EnablePlotMode(bool b,int type=0);
        void ResumePlotMode();
        CoroutineHandle CustomStartBattleCoroutine();
        bool IsPlotMode { get; }
        int CurPlotIndex { get; }
        int AddIndex();
    }
    public interface IBaseDBMgr
    {
        BaseDBGameData CurBaseGameData { get; }
        BaseDBGameData StartNewGame();
        BaseDBGameData LoadGame(string ID);
        void UseRemoteArchives(bool isUse);
        void ReadGameDBData();
        void WriteGameDBData();
    }
    public interface IBaseSettingsMgr
    {
        BaseDBSettingsData GetBaseSettings();
        BaseDevSettingsData GetBaseDevSettings();
        void Save();
    }
    public interface IBaseDifficultMgr
    {
        #region set
        void SetDifficultyType(GameDiffType type);
        void SetGMMod(bool b);
        void SetAnalytics(bool b);
        void SetHavePlot(bool b);
        #endregion

        #region get
        BaseGameDiffData GetBaseSettings();
        #endregion

        #region is
        bool IsAnalytics();
        bool IsGMMode();
        bool IsSettedGMMod();
        bool IsHavePlot();
        #endregion
    }
    public interface IBaseScreenMgr
    {
        BaseUnit BaseLocalPlayer { get; set; }
        BaseUnit BasePrePlayer { get; set; }
        Vector3 GetMouseHitPoint();
        event Callback<BaseUnit, BaseUnit> Callback_OnSetPlayerBase;
    }

    public interface IBaseTalkMgr
    {
        #region set
        TalkFragment StartOption(string id);
        TalkFragment Start(string id, int index = 0);
        TalkFragment Next();
        void ClickOption(int index);
        void ClickTalk();
        string SelectOption(int index);
        void SelectPreOption();
        void SelectNextOption();
        void Stop();
        bool IsHave();
        bool IsInOption();
        bool IsLockNextTalk { get; }
        #endregion

        #region get
        TalkFragment CurTalkFragment();
        #endregion
    }

    public interface IBaseNarrationMgr
    {
        #region set
        NarrationFragment Start(string id);
        NarrationFragment Next();
        void Stop();
        bool IsHave();
        #endregion

        #region get
        NarrationFragment CurNarrationFragment();
        #endregion
    }

    public interface IBaseSenseMgr
    {
        void OnTriggerEnter(Collider col);
        void OnTriggerExit(Collider col);
    }
}
