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

    #region 可序列化trans
    [Serializable]
    public struct Vec3S
    {
        public float x;
        public float y;
        public float z;

        public void Fill(Vector3 v3)
        {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public Vector3 V3
        { get { return new Vector3(x, y, z); } }
    }

    [Serializable]
    public struct QuaS
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public void Fill(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion Q
        { get { return new Quaternion(x, y, z, w); } }
    }
    #endregion

}