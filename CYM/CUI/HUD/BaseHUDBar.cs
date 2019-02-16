//------------------------------------------------------------------------------
// BaseHUDBar.cs
// Copyright 2019 2019/2/8 
// Created by CYM on 2019/2/8
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseHUDBar : BaseHUDItem 
    {
        #region prop
        BaseCameraMgr BaseCameraMgr=>SelfBaseGlobal.CameraMgr;
        BaseFOWMgr BaseFOWMgr => SelfBaseGlobal.FOWMgr;
        #endregion

        #region life
        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateAnchoredPosition();
        }
        #endregion

        /// <summary>
        /// 根据单位世界坐标的位置实时更新当前血条的映射点
        /// </summary>
        protected virtual void UpdateAnchoredPosition()
        {
            if (CanvasScaler == null)
                return;
            if (SelfBaseGlobal == null)
                return;
            if (followObj != null)
            { 
                if (HideCondition())
                {
                    Show(false);
                    return;
                }
                else
                    Show(true);
                float resolutionX = CanvasScaler.referenceResolution.x;
                float resolutionY = CanvasScaler.referenceResolution.y;
                float offect = (Screen.width / CanvasScaler.referenceResolution.x) * (1 - CanvasScaler.matchWidthOrHeight)
                    + (Screen.height / CanvasScaler.referenceResolution.y) * CanvasScaler.matchWidthOrHeight;
                Vector2 a = RectTransformUtility.WorldToScreenPoint(Camera.main, followObj.position+Offset);
                Vector2 relationPos = new Vector2(a.x / offect, a.y / offect);
                float distance = BaseCameraMgr.CameraHight;
                Vector3 anchorPos = relationPos;
                if (RectTrans.localPosition != anchorPos)
                    RectTrans.localPosition = anchorPos;
            }
        }

        protected virtual bool HideCondition()
        {
            if (BaseFOWMgr != null)
            {
                if (BaseFOWMgr.IsInFog(SelfUnit.Pos))
                    return true;
            }
            if (!SelfUnit.IsRendered)
                return true;
            if (BaseCameraMgr.IsTopHight)
            {
                return true;
            }
            return false;
        }
    }
}