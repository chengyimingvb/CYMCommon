using System.Collections;
using System.Collections.Generic;
using CYM;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace CYM
{
    public enum GameDiffType
    {
        Simple,//简单
        Ordinary,//普通
        Difficulty,//困难
        Extremely,//变态
    }
    public enum GamePropType
    {
        Low,//简单
        Middle,//普通
        Hight,//困难
    }
    [Serializable]
    public class BaseGameDiffData
    {
        public GameDiffType GameDifficultyType = GameDiffType.Simple;
        public bool IsGMMod = false;
        public bool IsAnalytics = false;
        public bool IsHavePlot = true;
    }
    public class BaseDifficultMgr<T> : BaseGlobalCoreMgr, IBaseDifficultMgr where T: BaseGameDiffData,new()
    {
        #region prop
        public T Setting { get; protected set; } = new T();
        #endregion

        #region Set
        /// <summary>
        /// 设置游戏难度
        /// </summary>
        /// <param name="type"></param>
        public void SetDifficultyType(GameDiffType type)
        {
            Setting.GameDifficultyType = type;
        }
        /// <summary>
        /// 设置游戏GM模式
        /// </summary>
        public void SetGMMod(bool b)
        {
            Setting.IsGMMod = b;
        }
        /// <summary>
        /// 设置数据分析
        /// </summary>
        /// <param name="b"></param>
        public void SetAnalytics(bool b)
        {
            Setting.IsAnalytics = b;
        }
        /// <summary>
        /// 设置剧情模式
        /// </summary>
        /// <param name="b"></param>
        public void SetHavePlot(bool b)
        {
            Setting.IsHavePlot = b;
        }
        #endregion

        #region get
        public BaseGameDiffData GetBaseSettings()
        {
            return Setting;
        }
        #endregion

        #region is 
        /// <summary>
        /// 是否允许数据统计
        /// </summary>
        /// <returns></returns>
        public bool IsAnalytics()
        {
            return Setting.IsAnalytics && !Application.isEditor;
        }
        /// <summary>
        /// 是否为GMmod
        /// </summary>
        /// <returns></returns>
        public bool IsGMMode()
        {
            if (SelfBaseGlobal.VersionMgr.IsTrial)
                return false;
            return Setting.IsGMMod || SelfBaseGlobal.VersionMgr.IsDevBuild;
        }
        /// <summary>
        /// 是否设置了GM模式
        /// </summary>
        /// <returns></returns>
        public bool IsSettedGMMod()
        {
            return Setting.IsGMMod;
        }
        /// <summary>
        /// 是有有剧情
        /// </summary>
        /// <returns></returns>
        public bool IsHavePlot()
        {
            return Setting.IsHavePlot;
        }
        /// <summary>
        /// 是否为玩家
        /// </summary>
        /// <param name="nation"></param>
        /// <returns></returns>
        private bool IsPlayerCtrl(BaseUnit unit)
        {
            if (unit == null) return false;

            if (!unit.IsPlayerCtrl())
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}