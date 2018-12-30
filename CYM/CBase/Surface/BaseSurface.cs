//**********************************************
// Class Name	: BaseSurface
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;
using System;
namespace CYM
{
	public class BaseSurface 
	{
        protected Material UseMaterial;
        protected Dictionary<Renderer,Material> sourceMats = new Dictionary<Renderer,Material>();
        protected BaseSurfaceMgr surfaceMgr;

        public virtual void Init(BaseSurfaceMgr mgr,string matName="")
        {
            surfaceMgr = mgr;
            sourceMats.Clear();
            for (int i = 0; i < surfaceMgr.ModelRenders.Length; ++i)
            {
                sourceMats.Add(surfaceMgr.ModelRenders[i], surfaceMgr.ModelRenders[i].material);
            }
            string tempMatName = matName.IsInvStr() ? GetDefaultMatName() : matName;
            if (UseMaterial == null&&!tempMatName.IsInvStr())
                UseMaterial = surfaceMgr.SelfBaseGlobal.GRMgr.GetMaterial(tempMatName);
        }

        public virtual void SetParam(params object[] param)
        {

        }

        public virtual void Revert()
        {
            if (surfaceMgr.ModelRenders != null)
            {
                foreach (var item in surfaceMgr.ModelRenders)
                {
                    item.material = sourceMats[item];
                }
            }
        }

        public virtual void Use()
        {
            surfaceMgr.CurSurface = this;
            if (surfaceMgr.ModelRenders != null&&UseMaterial!=null)
            {
                for (int i = 0; i < surfaceMgr.ModelRenders.Length; ++i)
                {

                    Material preMat = surfaceMgr.ModelRenders[i].material;
                    surfaceMgr.ModelRenders[i].material = UseMaterial;
                    SetMaterial( preMat,  surfaceMgr.ModelRenders[i].material);
                }
            }
        }
        public virtual void Fade(float alpha)
        {

        }
        public virtual void FadeOut()
        {

        }
        public virtual void FadeIn()
        {

        }
        public virtual void Update()
        {

        }

        protected virtual void SetMaterial( Material preMat, Material mat)
        {
            throw new NotImplementedException("必须重载此函数");
        }
        protected virtual void ForeachMaterial(Callback<Material> setMaterial)
        {
            for (int i = 0; i < surfaceMgr.ModelRenders.Length; ++i)
            {
                setMaterial(surfaceMgr.ModelRenders[i].material);
            }
        }
        #region get
        public virtual string GetDefaultMatName()
        {
            return "";
        }
        #endregion
    }
}