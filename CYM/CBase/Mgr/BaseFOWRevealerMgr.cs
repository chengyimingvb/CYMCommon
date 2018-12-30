//------------------------------------------------------------------------------
// BaseFOWRevealerMgr.cs
// Copyright 2018 2018/11/12 
// Created by CYM on 2018/11/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using FoW;

namespace CYM
{
    public class BaseFOWRevealerMgr : BaseCoreMgr
    {
        #region prop
        FogOfWarUnit FOWRevealer { get; set; }
        public bool IsVisible { get; private set; }
        public bool IsInFog { get; private set; }
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            FOWRevealer = Mono.GetComponent<FogOfWarUnit>();
        }
        #endregion

        #region set
        public virtual void RefreshVisible()
        {
            throw new System.Exception("此函数必须被实现");
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
            if (FOWRevealer == null)
                return;
            FOWRevealer.enabled = b;
        }
        #endregion

        #region is
        bool IsInFogRange()
        {
            if (SelfBaseGlobal.FOWMgr.IsInFog(SelfBaseUnit.Pos, 70))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}