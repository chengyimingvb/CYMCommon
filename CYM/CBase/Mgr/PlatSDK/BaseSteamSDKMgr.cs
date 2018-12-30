using System;
using System.Collections.Generic;
using Facepunch.Steamworks;
using Facepunch.Steamworks.Callbacks;
using UnityEngine;
using static Facepunch.Steamworks.Leaderboard;

namespace CYM
{
    public class BaseSteamSDKMgr : BasePlatSDKMgr
    {

        #region life
        public override void OnBeAdded(CYM.IMono mono)
        {
            base.OnBeAdded(mono);
            uint appId = GetAppId();
            Config.ForUnity(Application.platform.ToString());
            // 使用try防止崩溃
            try
            {
                new Client(appId);
                //_client = new Client(appId);
            }
            catch (System.Exception e)
            {
                CLog.Error("Error starting steam client: {0}", e);
                Client.Instance.Dispose();
            }
            if (Client.Instance != null && Client.Instance.IsValid)
            {
                IsInited = true;
            }
            else
            {
                if (Client.Instance != null)
                {
                    Client.Instance.Dispose();
                }
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (Client.Instance != null)
            {
                Client.Instance.Achievements.OnUpdated += OnUpdatedAchievements;
            }
        }
        public override void OnStart()
        {
            base.OnStart();
            RefreshFriends();
        }
        public override void OnDisable()
        {
            if (Client.Instance != null)
            {
                Client.Instance.Achievements.OnUpdated -= OnUpdatedAchievements;
            }
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Client.Instance != null)
            {
                Client.Instance.Update();
            }
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        public override void OnDestroy()
        {
            if (Client.Instance != null)
                Client.Instance.Dispose();

            base.OnDestroy();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否支持这类语言
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected override bool IsSupportLanguage(string lang)
        {
            if (Client.Instance.AvailableLanguages == null)
                return false;
            if (Client.Instance.AvailableLanguages.Length == 0)
                return false;
            return Client.Instance.AvailableLanguages[0].Contains(lang);
        }
        /// <summary>
        /// 是否为正版
        /// </summary>
        public override bool IsLegimit
        {
            get
            {
                if (Client.Instance == null)
                    return false;
                if (!Client.Instance.IsValid)
                    return false;
                if (IsDifferentAppId)
                    return false;
                return true;
            }
        }
        /// <summary>
        /// 文件APPid不一致
        /// </summary>
        public override bool IsDifferentAppId
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
        public override bool IsSuportCloudArchive()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 通过id判断此人是否为自己
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool IsSelf(ulong id)
        {
            return Client.Instance.SteamId == id;
        }
        /// <summary>
        /// 是否支持平台UI
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformUI()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 是否支持平台语言
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformLanguage()
        {
            return true;
        }
        #endregion

        #region Get
        /// <summary>
        /// 本地APP文件路径
        /// </summary>
        /// <returns></returns>
        protected override string LocalAppIDFilePath()
        {
            return "steam_appid.txt";
        }
        /// <summary>
        /// 得到APPid
        /// </summary>
        /// <returns></returns>
        protected override uint GetAppId()
        {
            return GameConfig.Ins.SteamAppID;
        }
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <returns></returns>
        public override string GetErrorInfo()
        {
            if (IsDifferentAppId)
                return "The game is not activated, app found the different app id,do you changed any thing?";
            if (Client.Instance == null && !IsLegimit)
                return "Unable to connect to Steam";
            if (!Client.Instance.IsValid)
                return "The game is not activated";
            return "Error";
        }
        /// <summary>
        /// 得到云存档路径
        /// </summary>
        /// <returns></returns>
        public override string GetCloudArchivePath()
        {
            return Application.persistentDataPath;
        }
        /// <summary>
        /// steam平台
        /// </summary>
        public override Distribution DistributionType
        {
            get
            {
                return Distribution.Steam;
            }
        }
        public override string GetCurLanguageStr()
        {
            if (Client.Instance == null)
                return "";
            return Client.Instance.CurrentLanguage;
        }
        #endregion

        #region Callback
        #endregion

        #region 成就
        public override void RandomTriggerAllAchievement()
        {
            ResetAllAchievement();
            foreach (var item in Client.Instance.Achievements.All)
            {
                bool b = BaseMathUtils.Rand(0.5f);
                if (b)
                    item.Trigger(b);
            }
        }
        public override void ResetAllAchievement()
        {
            foreach (var item in Client.Instance.Achievements.All)
            {
                item.Reset();
            }
        }
        public override void TriggerAchievement(TDBaseAchieveData data)
        {
            Client.Instance?.Achievements.Trigger(data.TDID, true);
        }
        public override void ResetAchievement(TDBaseAchieveData data)
        {
            Client.Instance?.Achievements.Reset(data.TDID);
        }
        public override void RefreshAchievements()
        {
            //Achievements.Clear();
            foreach (var item in Client.Instance.Achievements.All)
            {
                if (Achievements.ContainsKey(item.Id))
                {

                }
                else
                {

                    var tempData = GetAchieveData.Copy();
                    //var tempData = new PlatformAchievement()
                    {
                        //tempData.Percent = item.Percentage;
                        tempData.State = item.State;
                        //if (!item.State)
                        //    tempData.Percent = 0;
                        tempData.UnlockTime = item.UnlockTime;
                        tempData.SourceName = item.Name;
                        tempData.SourceDesc = item.Description;
                    };
                    if (tempData == null)
                    {
                        CLog.Error("没有配置这个成就:{0}", item.Id);
                        continue;
                    }
                    Achievements.Add(item.Id, tempData);
                }
            }
        }
        void OnUpdatedAchievements()
        {
            RefreshAchievements();
            Callback_OnFatchAchievementSuccess?.Invoke();
        }
        #endregion

        #region 统计
        public override void SetStats(string id, float val)
        {
            Client.Instance?.Stats.Set(id, val, true);
        }
        public override void SetStats(string id, int val)
        {
            Client.Instance?.Stats.Set(id, val, true);
        }
        public override void AddStats(string id, int val)
        {
            Client.Instance?.Stats.Add(id, val, true);
        }
        public override float GetFloatStats(string id)
        {
            if (Client.Instance == null)
                return 0.0f;
            return Client.Instance.Stats.GetFloat(id);
        }
        public override int GetIntStats(string id)
        {
            if (Client.Instance == null)
                return 0;
            return Client.Instance.Stats.GetInt(id);
        }
        public override void StoreStats()
        {
            Client.Instance?.Stats.StoreStats();
        }
        #endregion

        #region 好友
        public override void RefreshFriends()
        {
            if (Client.Instance == null)
                return;
            Friends.Clear();
            FriendsID.Clear();
            Client.Instance.Friends.Refresh();
            foreach (var item in Client.Instance.Friends.All)
            {
                var temp = new PlatformFriend()
                {
                    Name = item.Name,
                    ID = item.Id,
                    IsPlaying = item.IsPlaying,
                    IsPlayingThisGame = item.IsPlayingThisGame,
                    IsSnoozing = item.IsSnoozing,
                    IsBusy = item.IsBusy,
                    IsAway = item.IsAway,
                    IsOnline = item.IsOnline,
                    IsFriend = item.IsFriend,
                    IsBlocked = item.IsBlocked,

                };
                Friends.Add(temp);
                FriendsID.Add(temp.ID);
            }
            Friends.Sort((X, Y) =>
            {
                if (X.IsOnline && !Y.IsOnline)
                    return -1;
                return 1;
            });
        }

        public override bool IsFriend(ulong id)
        {
            return FriendsID.Contains(id);
        }
        #endregion

        #region overlay
        public override void OpenAchievement(ulong id)
        {
            Client.Instance?.Overlay.OpenAchievements(id);
        }
        public override void OpenChat(ulong id)
        {
            Client.Instance?.Overlay.OpenChat(id);
        }
        public override void OpenProfile(ulong id)
        {
            Client.Instance?.Overlay.OpenProfile(id);
        }
        public override void OpenStats(ulong id)
        {
            Client.Instance?.Overlay.OpenStats(id);
        }
        public override void OpenTrade(ulong id)
        {
            Client.Instance?.Overlay.OpenTrade(id);
        }
        public override void OpenAddFriend(ulong id)
        {
            Client.Instance?.Overlay.AddFriend(id);
        }
        public override void OpenURL(string URL)
        {
            if (Application.isEditor)
            {
                base.OpenURL(URL);
            }
            else
            {
                Client.Instance?.Overlay.OpenUrl(URL);
            }
        }
        #endregion

        #region shop
        public override void GoToShop()
        {
            Client.Instance?.Overlay.OpenUrl("http://store.steampowered.com/app/669320/Nation_WarChronicles/?beta=0");
        }
        #endregion

        #region leader board
        Dictionary<string, Leaderboard> tempLeaderboards = new Dictionary<string, Leaderboard>();
        string curRefreshLeaderBoardId;
        Leaderboard curLeaderBoard;
        public override void RefreshLeaderBoard(string id = "")
        {
            curRefreshLeaderBoardId = id;
            curLeaderBoard = GetLeaderboard(id);
            if (curLeaderBoard == null)
                return;

            SelfBaseGlobal.CommonCoroutine.Run(_UpdateFetchLeaderBoard());
        }
        IEnumerator<float> _UpdateFetchLeaderBoard()
        {
            yield return Timing.WaitUntilTrue(() => { return curLeaderBoard.IsValid; });
            curLeaderBoard.FetchScores(
                Leaderboard.RequestType.GlobalAroundUser, 0, 20,
                OnFetchScoreSuccess,
                OnFecthScoreFaild);
        }
        public override void SetScore(string leaderBoardId, int step = 1)
        {
            Leaderboard temp = GetLeaderboard(leaderBoardId);
            if (temp == null)
                return;
            temp.AddScore(true, step,null,OnAddScoreSuccess,OnAddScoreFail);
        }

        private Leaderboard GetLeaderboard(string leaderBoardId)
        {
            Leaderboard temp = null;
            if (Client.Instance == null)
                return temp;
            if (tempLeaderboards.ContainsKey(leaderBoardId))
            {
                temp = tempLeaderboards[leaderBoardId];
            }
            else
            {
                temp = Client.Instance.GetLeaderboard(leaderBoardId, Client.LeaderboardSortMethod.Ascending, Client.LeaderboardDisplayType.Numeric);
                tempLeaderboards.Add(leaderBoardId, temp);
            }
            return temp;
        }
        void OnFetchScoreSuccess(Entry[] results)
        {
            PlatformLeaderBoard tempLeaderBoard;
            if (LeaderBoards.ContainsKey(curRefreshLeaderBoardId))
            {
                tempLeaderBoard = LeaderBoards[curRefreshLeaderBoardId];
                tempLeaderBoard.Items.Clear();
            }
            else
            {
                tempLeaderBoard = new PlatformLeaderBoard();
                LeaderBoards.Add(curRefreshLeaderBoardId, tempLeaderBoard);
            }
            if (results != null)
            {
                foreach (var item in results)
                {
                    var newItem = new PlatformLeaderBoardItem()
                    {
                        Name = item.Name,
                        Score = item.Score,
                        GlobalRank = item.GlobalRank,
                        Id = item.SteamId,
                    };
                    tempLeaderBoard.Items.Add(newItem);
                }
            }
            Callback_OnFatchScoreSuccess?.Invoke(tempLeaderBoard);
        }
        void OnFecthScoreFaild(Result result)
        {
            Callback_OnFatchScoreFaild?.Invoke();
        }
        private void OnAddScoreFail(Result reason)
        {
            CLog.Error("AddScore Fail :"+ reason.ToString());
        }

        private void OnAddScoreSuccess(AddScoreResult result)
        {

        }
        #endregion

        #region test
        protected override void Test()
        {
        }
        #endregion

    }

}