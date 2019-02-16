using UnityEngine;
using System.Collections;
using CYM;
using System;
using System.Collections.Generic;
using UnityEngine.Video;
using DG.Tweening;

//**********************************************
// Class Name	: GlobalComponet
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BaseVideoMgr : BaseGFlowMgr
    {
        public VideoPlayer VideoPlayer { get; set; }
        GameObject VideoPlayerGO;
        #region life
        protected override void OnAllLoadEnd()
        {
            base.OnAllLoadEnd();
            VideoPlayerGO=SelfBaseGlobal.GRMgr.GetResources("BaseVideoPlayer",true);
            VideoPlayer = VideoPlayerGO.GetComponentInChildren<VideoPlayer>();
            Stop();
        }
        #endregion

        #region set
        public void Play(string videoName,float startAlpha,float duration,float delay)
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.targetCamera = Camera.main;
                VideoPlayer.clip = SelfBaseGlobal.GRMgr.GetVideo(videoName);
                VideoPlayer?.Play();
                VideoPlayer.targetCameraAlpha = startAlpha;
                DOTween.To(() => VideoPlayer.targetCameraAlpha, x => VideoPlayer.targetCameraAlpha = x, 1.0f, duration).SetDelay(delay).OnComplete(OnTweenPlay);
            }
            
        }
        public void Stop()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.targetCameraAlpha = 0.0f;
                VideoPlayer.targetCamera = null;
                VideoPlayer?.Stop();
            }
        }
        #endregion

        #region Callback
        void OnTweenPlay()
        {
            
        }
        void OnTweenStop()
        {

        }
        #endregion
    }

}