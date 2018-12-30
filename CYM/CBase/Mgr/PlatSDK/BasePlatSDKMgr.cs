using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    #region 辅助类

    public class PlatformLeaderBoardItem
    {
        public ulong Id;
        public int GlobalRank;
        public string Name;
        public int Score;
    }

    /// <summary>
    /// 计分板数据
    /// </summary>
    public class PlatformLeaderBoard
    {
        public List<PlatformLeaderBoardItem> Items = new List<PlatformLeaderBoardItem>();
    }

    /// <summary>
    /// 平台好友数据
    /// </summary>
    public class PlatformFriend
    {
        public ulong ID;
        public string Name;
        public bool IsPlaying;
        public bool IsPlayingThisGame;
        public bool IsSnoozing;
        public bool IsBusy;
        public bool IsAway;
        public bool IsOnline;
        public bool IsFriend;
        public bool IsBlocked;
        public Sprite Icon;
    }
    #endregion

    public class BasePlatSDKMgr : BaseGlobalCoreMgr
    {
        #region prop
        protected uint fileAppId;
        protected Dictionary<string, LanguageType> LanguageDic { get; set; } = new Dictionary<string, LanguageType>();
        #endregion

        #region 生命周期
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            AddLanguageConvert("schinese", LanguageType.Chinese);
            AddLanguageConvert("tchinese", LanguageType.Traditional);
            AddLanguageConvert("english", LanguageType.English);
        }
        public override void OnBeAdded(CYM.IMono mono)
        {
            fileAppId = ReadFileAppId();
            base.OnBeAdded(mono);
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否已经初始化
        /// </summary>
        /// <returns></returns>
        public bool IsInited
        {
            get; protected set;
        }
        /// <summary>
        /// 是否支持这类语言
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected virtual bool IsSupportLanguage(string lang)
        {
            return false;
        }
        /// <summary>
        /// 是否为正版
        /// </summary>
        public virtual bool IsLegimit
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 文件APPid不一致
        /// </summary>
        public virtual bool IsDifferentAppId
        {
            get
            {
                return GetAppId() != fileAppId;
            }
        }
        /// <summary>
        /// 是否支持云存档
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportCloudArchive()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 通过id判断此人是否为自己
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool IsSelf(ulong id)
        {
            return false;
        }
        /// <summary>
        /// 是否支持平台语言
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportPlatformLanguage()
        {
            return false;
        }
        /// <summary>
        /// 是否支持平台界面
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportPlatformUI()
        {
            return false;
        }
        /// <summary>
        /// 是否支持menu prop UI
        /// </summary>
        /// <returns></returns>
        public virtual bool IsSuportMenuPropUI()
        {
            return true;
        }
        #endregion

        #region Set
        /// <summary>
        /// 添加langue转换
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        protected void AddLanguageConvert(string key,LanguageType type)
        {
            if (LanguageDic.ContainsKey(key))
            {
                LanguageDic[key]=type;
            }
            else
            {
                LanguageDic.Add(key, type);
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// 获得标题描述
        /// </summary>
        /// <returns></returns>
        public virtual string GetMainMenuTitle()
        {
            return "";
        }
        /// <summary>
        /// 本地APP文件路径
        /// </summary>
        /// <returns></returns>
        protected virtual string LocalAppIDFilePath()
        {
            return string.Empty;
        }
        /// <summary>
        /// 得到APPid
        /// </summary>
        /// <returns></returns>
        protected virtual uint GetAppId()
        {
            return 0;
        }
        /// <summary>
        /// 读取文件中的APPid
        /// </summary>
        /// <returns></returns>
        private uint ReadFileAppId()
        {
            if (LocalAppIDFilePath().IsInvStr())
                return 0;
            uint r = 0;
            string id = null;
            try
            {
                id = File.ReadAllText(LocalAppIDFilePath());
            }
            catch (Exception e)
            {
                CLog.Error("无法打开appid文件, {0}", e);
                return 0;
            }

            if (!uint.TryParse(id, out r))
            {
                CLog.Error("无法读取AppID {0}", id);
                return 0;
            }
            else
            {
                return r;
            }
        }
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <returns></returns>
        public virtual string GetErrorInfo()
        {
            return "Error";
        }
        /// <summary>
        /// 得到云存档路径
        /// </summary>
        /// <returns></returns>
        public virtual string GetCloudArchivePath()
        {
            return Application.persistentDataPath;
        }
        /// <summary>
        /// 获得当前平台类型
        /// </summary>
        public virtual Distribution DistributionType
        {
            get;
        }
        /// <summary>
        /// 获得语言
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public virtual LanguageType GetLanguageType()
        {
            string str = GetCurLanguageStr();
            if (LanguageDic.ContainsKey(str))
                return LanguageDic[str];
            return LanguageType.English;
        }
        public virtual string GetCurLanguageStr()
        {
            return "";
        }
        #endregion

        #region Callback
        protected virtual void OnInitSetting()
        {

        }
        #endregion

        #region 成就
        /// <summary>
        /// 更新玩成就
        /// </summary>
        public virtual void RefreshAchievements()
        {
        }
        public Dictionary<string, TDBaseAchieveData> Achievements { get; set; } = new Dictionary<string, TDBaseAchieveData>();
        /// <summary>
        /// 触发成就
        /// </summary>
        /// <param name="b"></param>
        public virtual void RandomTriggerAllAchievement()
        {

        }
        /// <summary>
        /// 重置所有成就
        /// </summary>
        public virtual void ResetAllAchievement()
        {
        }
        /// <summary>
        /// 激活成就
        /// </summary>
        public virtual void TriggerAchievement(TDBaseAchieveData data)
        {
        }
        /// <summary>
        /// 重置成就
        /// </summary>
        public virtual void ResetAchievement(TDBaseAchieveData data)
        {

        }
        /// <summary>
        /// 获取成就成功
        /// </summary>
        public Callback Callback_OnFatchAchievementSuccess { get; set; }
        #endregion

        #region 统计
        /// <summary>
        /// 设置统计数值
        /// </summary>
        public virtual void SetStats(string id, int val)
        {

        }
        /// <summary>
        /// 设置统计数值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        public virtual void SetStats(string id, float val)
        {

        }
        /// <summary>
        /// 增加统计数值
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        public virtual void AddStats(string id, int val)
        {
        }
        /// <summary>
        /// 获得统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual float GetFloatStats(string id)
        {
            return 0.0f;
        }
        /// <summary>
        /// 获得统计
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual int GetIntStats(string id)
        {
            return 0;
        }
        /// <summary>
        /// 存储统计
        /// </summary>
        public virtual void StoreStats()
        {
        }
        #endregion

        #region 好友
        /// <summary>
        /// 好友数据
        /// </summary>
        public List<PlatformFriend> Friends { get; protected set; } = new List<PlatformFriend>();
        /// <summary>
        /// 好友id
        /// </summary>
        public HashSet<ulong> FriendsID { get; protected set; } = new HashSet<ulong>();
        /// <summary>
        /// 刷行朋友数据
        /// </summary>
        public virtual void RefreshFriends()
        {
        }
        public virtual bool IsFriend(ulong id)
        {
            return true;
        }
        #endregion

        #region overlay
        /// <summary>
        /// 打开成就overlay
        /// </summary>
        public virtual void OpenAchievement(ulong id)
        {

        }
        /// <summary>
        /// 打开聊天
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenChat(ulong id)
        {
        }
        /// <summary>
        /// 打开简介
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenProfile(ulong id)
        {
        }
        /// <summary>
        /// 打开统计
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenStats(ulong id)
        {
        }
        /// <summary>
        /// 打开贸易
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenTrade(ulong id)
        {
        }
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="id"></param>
        public virtual void OpenAddFriend(ulong id)
        {

        }
        /// <summary>
        /// 打开URL
        /// </summary>
        public virtual void OpenURL(string URL)
        {
            Application.OpenURL(URL);
        }
        #endregion

        #region leader board
        /// <summary>
        /// 排行榜
        /// </summary>
        public Dictionary<string, PlatformLeaderBoard> LeaderBoards { get; protected set; } = new Dictionary<string, PlatformLeaderBoard>();
        /// <summary>
        /// 刷行排行榜
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual void RefreshLeaderBoard(string id = "")
        {
            return;
        }
        /// <summary>
        /// 添加分数
        /// </summary>
        /// <param name="leaderBoardId"></param>
        /// <param name="step"></param>
        public virtual void SetScore(string leaderBoardId, int step = 1)
        {
        }
        /// <summary>
        /// 获取计分成功
        /// </summary>
        public Callback<PlatformLeaderBoard> Callback_OnFatchScoreSuccess { get; set; }
        /// <summary>
        /// 获取计分失败
        /// </summary>
        public Callback Callback_OnFatchScoreFaild { get; set; }
        #endregion

        #region shop
        public virtual void GoToShop()
        {

        }
        #endregion

        #region test
        protected virtual void Test()
        {

        }
        #endregion

        #region must override
        protected TDBaseAchieveData GetAchieveData { get { throw new NotImplementedException("此函数没有被实现!!!"); } }
        #endregion

    }

}