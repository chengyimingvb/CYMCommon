//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using CYM;
using UnityEngine;
//using iBoxDB.LocalServer;
using System.IO;
using System.Collections.Generic;

namespace CYM
{
    public enum GameNetMode
    {
        PVP,
        PVE,
    }
    public enum GamePlayStateType
    {
        NewGame,//新游戏
        LoadGame,//加载游戏
    }
    public interface IDBStructConvert
    {
        void Write();
        void Read();
    }

    [Serializable]
    public class BaseDBGameData
    {
        //战场id
        public string BattleID { get; set; } = BaseConstMgr.STR_Inv;
        public int PlayTime { get; set; }
        public int LoadBattleCount { get; set; } = 0;
        public GameNetMode GameNetMode { get; set; }
        public GamePlayStateType GamePlayStateType { get; set; }
        public bool IsNewGame()
        {
            return GamePlayStateType == GamePlayStateType.NewGame;
        }
        public bool IsFirstLoadBattle()
        {
            return LoadBattleCount == 1;
        }
    }


    #region base
    [Serializable]
    public class BaseDBStruct
    {
        public int ID;
        public string TDID;
        public static int GetID(ICYMBase data)
        {
            if (data == null)
                return BaseConstMgr.InvInt;
            return data.ID;
        }

        public static void Read<T, T2>(T data, CYM.Callback<T2> ondo = null) where T : IEnumerable<T2> where T2 : IDBStructConvert
        {
            if (data == null)
                return;
            foreach (var item in data)
            {
                item.Read();
                if (ondo != null)
                    ondo(item);
            }
        }
        public static void Wirte<T, T2>(T data, CYM.Callback<T2> ondo = null) where T : IEnumerable<T2> where T2 : IDBStructConvert
        {
            if (data == null)
                return;
            foreach (var item in data)
            {
                item.Write();
                if (ondo != null)
                    ondo(item);
            }
        }
    }
    #endregion

}