//**********************************************
// Class Name	: Surface_Dissolve
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
    public class Surface_Normal : BaseSurface
    {
        protected override void SetMaterial(Material preMat, Material mat)
        {
            Texture mainTex = preMat.GetTexture("_MainTex");
            mat.SetTexture("_MainTex", mainTex);
        }
    }

}