//------------------------------------------------------------------------------
// BaseSLGCamera.cs
// Copyright 2018 2018/11/13 
// Created by CYM on 2018/11/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using CYM.Cam;
namespace CYM
{
    public class BaseSLGCameraMgr : BaseCameraMgr
    {
        #region prop
        RTSCamera RTSCamera;
        IBaseBattleMgr BattleMgr => SelfBaseGlobal.BattleMgr;
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            RTSCamera = Mono.GetComponentInChildren<RTSCamera>();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (RTSCamera==null || CameraHight == float.NaN)
                return;
            if (BattleMgr.IsInBattle)
            {
                RTSCamera.DesktopMoveDragSpeed =  (ZoomPercent* RTSCamera.desktopMoveDragSpeed);
                RTSCamera.DesktopMoveSpeed = (ZoomPercent * RTSCamera.desktopMoveSpeed);
                //CLog.Error("测试:"+ RTSCamera.desktopMoveDragSpeed);
            }
        }
        #endregion

        #region set
        public void Move(Vector3 dir, float offset)
        {
            RTSCamera.Move(dir,offset);
        }
        #endregion
    }
}