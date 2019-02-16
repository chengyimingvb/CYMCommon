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
        BaseDBSettingsData SettingsData => SelfBaseGlobal.SettingsMgr.GetBaseSettings();
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
                var data = SettingsData;
                RTSCamera.DesktopMoveDragSpeed =  (ZoomPercent* RTSCamera.desktopMoveDragSpeed) * data.CameraMoveSpeed;
                RTSCamera.DesktopMoveSpeed = (ZoomPercent * RTSCamera.desktopMoveSpeed) * data.CameraMoveSpeed;
                RTSCamera.DesktopScrollSpeed = (RTSCamera.desktopScrollSpeed)* data.CameraScrollSpeed;
                if (Input.GetMouseButtonDown(2))
                {
                    Vector3 pos = SelfBaseGlobal.ScreenMgr.GetMouseHitPoint();
                    RTSCamera.JumpToTargetForMain(SelfBaseGlobal.GetTransform(pos));
                }
            }
        }
        #endregion

        #region set
        public void Move(Vector3 dir)
        {
            RTSCamera.Move(dir);
        }
        public void Jump(Transform target,float heightPercent=0.05f)
        {
            RTSCamera.JumpToTargetForMain(target);
            RTSCamera.scrollValue = heightPercent;
        }
        public void UnLock()
        {
        }
        public void SetCameraMoveSpeed(float v)
        {
            SettingsData.CameraMoveSpeed = v;
        }
        public void SetCameraScrollSpeed(float v)
        {
            SettingsData.CameraScrollSpeed = v;
        }
        #endregion
    }
}