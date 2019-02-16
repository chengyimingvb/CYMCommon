//------------------------------------------------------------------------------
// BaseVirtual.cs
// Copyright 2018 2018/11/26 
// Created by CYM on 2018/11/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
namespace CYM
{
    public class BaseVirtual : ICYMBase, IDBDataConvert, IUnit
    {
        public int ID { get; set; }
        public string TDID { get; set; }

        #region unit
        public virtual void Birth()
        {
        }

        public virtual void Birth2()
        {
        }

        public virtual void Birth3()
        {
        }

        public virtual void Death(BaseUnit caster)
        {
            RealDeath();
        }
        public virtual void RealDeath()
        {
        }

        public virtual void ReBirth()
        {
        }
        public virtual void GameFrameTurn(int gameFramesPerSecond)
        {
        }

        public virtual void GameLogicTurn()
        {
        }

        public virtual void Init()
        {
            Birth();
            Birth2();
            Birth3();
        }

        public virtual void ManualUpdate()
        {
        }
        #endregion

        #region DB
        public virtual void Read1<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
        }

        public virtual void Read2<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
        }

        public virtual void Read3<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
        }

        public virtual void ReadEnd<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
        }
        public virtual void Write<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
        }
        #endregion
    }
}