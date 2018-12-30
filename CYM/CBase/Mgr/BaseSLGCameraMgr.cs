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
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            RTSCamera = Mono.GetComponentInChildren<RTSCamera>();
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