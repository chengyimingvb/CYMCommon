using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CYM.Audio;
namespace CYM
{
    public class BaseBGMMgr : BaseGFlowMgr
    {
        protected SoundConnection StartSoundConnection;
        protected SoundConnection BattleSoundConnection;
        protected SoundConnection CreditsSoundConnection;
        protected SoundConnection TempSoundConnection;

        #region get
        /// <summary>
        /// 得到当前歌曲的名称
        /// </summary>
        /// <returns></returns>
        public string GetCurSong()
        {
            var temp = SoundManager.GetCurrentSong();
            if (temp == null)
                return "";
            return temp.name;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否暂停
        /// </summary>
        /// <returns></returns>
        public bool IsPaused()
        {
            return SoundManager.IsPaused();
        }
        #endregion

        #region Set
        /// <summary>
        /// 下一首音乐
        /// </summary>
        public void Next()
        {
            SoundManager.Next();
        }
        /// <summary>
        /// 暂停
        /// </summary>
        public void PauseToggle()
        {
            SoundManager.PauseToggle();

        }
        /// <summary>
        /// 上一首
        /// </summary>
        public void Prev()
        {
            SoundManager.Prev();
        }
        #endregion

        #region get
        /// <summary>
        /// 创建音乐
        /// </summary>
        /// <returns></returns>
        protected SoundConnection CreateConnection(List<string> musics, SoundManager.PlayMethod type = SoundManager.PlayMethod.ContinuousPlayThroughWithRandomDelayInRange)
        {
            return SoundManager.CreateSoundConnection("", type, SelfBaseGlobal.GRMgr.GetMusics(musics));
        }
        #endregion

        #region set
        public void StartBGM(List<string> musics)
        {
            TempSoundConnection = null;
            TempSoundConnection = CreateConnection(musics);
            PlayConnection(TempSoundConnection);
        }
        public void StartMain()
        {
            PlayConnection(StartSoundConnection);
        }
        public void StartCredits()
        {
            PlayConnection(CreditsSoundConnection);
        }
        public void StartBattle()
        {
            PlayConnection(BattleSoundConnection);
        }
        public void PlayConnection(SoundConnection connection)
        {
            if (connection == null)
                return;
            SoundManager.PlayConnection(connection);
        }
        #endregion

        #region must Override
        protected virtual SoundConnection CreateMainBGM()
        {
            throw new NotImplementedException("此函数必须实现");
        }
        protected virtual SoundConnection CreateCreditsBGM()
        {
            return null;
        }
        protected virtual SoundConnection CreateBattleBGM()
        {
            throw new NotImplementedException("此函数必须实现");
        }
        #endregion

        #region Callback
        protected override void OnLuaParseEnd()
        {
            StartSoundConnection = CreateMainBGM();
            CreditsSoundConnection = CreateCreditsBGM();
            BattleSoundConnection = CreateBattleBGM();
        }
        protected override void OnAllLoadEnd()
        {
            //StartMain();
        }
        #endregion

    }

}