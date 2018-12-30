//**********************************************
// Class Name	: Surface_Source
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
namespace CYM
{
	public class Surface_Source : BaseSurface
	{
        #region set
        protected override void SetMaterial(Material preMat, Material mat)
        {
            return;
        }
        public override void Use()
        {
            base.Use();
            Revert();
        }
        #endregion
    }
}