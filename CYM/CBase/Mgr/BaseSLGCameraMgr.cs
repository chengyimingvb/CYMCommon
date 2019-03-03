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
        BaseDBSettingsData SettingsData => SelfBaseGlobal.SettingsMgr.GetBaseSettings();
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            RTSCamera = MainCamera.GetComponentInChildren<RTSCamera>();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (RTSCamera==null || CameraHight == float.NaN)
                return;
            if (BattleMgr.IsInBattle)
            {
                var data = SettingsData;
                float speedFaction = 1;
                if (IsLowHight)
                    speedFaction = 0.9f;
                else if (IsNearHight)
                    speedFaction = 0.8f;
                RTSCamera.DesktopMoveDragSpeed = (ZoomPercent* RTSCamera.desktopMoveDragSpeed) * data.CameraMoveSpeed* speedFaction;
                RTSCamera.DesktopMoveSpeed = (ZoomPercent * RTSCamera.desktopMoveSpeed) * data.CameraMoveSpeed* speedFaction;
                RTSCamera.DesktopScrollSpeed = (RTSCamera.desktopScrollSpeed)* data.CameraScrollSpeed;
                if (Input.GetMouseButtonDown(2))
                {
                    Vector3 pos = SelfBaseGlobal.ScreenMgr.GetMouseHitPoint();
                    RTSCamera.JumpToTargetForMain(SelfBaseGlobal.GetTransform(pos));
                }
                bool isOnUI = BaseUIUtils.CheckGuiObjects();
                RTSCamera.MouseScrollControl(!isOnUI);
                RTSCamera.MouseDragControl(!isOnUI);
                RTSCamera.ControlDisabled.Set(BaseView.IsFullScreenState.IsIn());
            }
        }
        #endregion

        #region set
        public void Move(Vector3 dir)
        {
            if(RTSCamera==null)
                return;
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