//------------------------------------------------------------------------------
// BaseLogicTurnMgr.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseLogicTurnMgr : BaseGlobalCoreMgr
    {
        #region set
        public void NextTurn()
        {
            SelfBaseGlobal.GameLogicTurn();
        }
        #endregion
    }
}