//------------------------------------------------------------------------------
// GameConfig.cs
// Copyright 2018 2018/12/14 
// Created by CYM on 2018/12/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    [CreateAssetMenu]
    public class GameConfig : ScriptableObjectConfig<GameConfig>
    {
        #region feedback
        public string FBToken;
        public string FBBoard = "";
        public string FBListID = "";
        public string FBLabelID = "";
        #endregion

        #region game
        public uint SteamAppID;
        #endregion
    }
}