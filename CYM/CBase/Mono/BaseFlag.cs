//------------------------------------------------------------------------------
// BaseFlag.cs
// Copyright 2019 2019/2/6 
// Created by CYM on 2019/2/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseFlag : MonoBehaviour 
    {
        #region inspector
        [SerializeField]
        MeshRenderer Banner;
        #endregion

        #region set
        public void Change(Texture tex)
        {
            Banner.material.SetTexture("_MainTex", tex);
        }
        #endregion
    }
}