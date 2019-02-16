//------------------------------------------------------------------------------
// DicList.cs
// Copyright 2019 2019/1/18 
// Created by CYM on 2019/1/18
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;

namespace CYM
{
    public class DicList<T> : List<T> where T:ICYMBase
    {
        protected Dictionary<string,T> Hash;

        public DicList()
        {
            Hash = new Dictionary<string, T>();
        }

        public T First()
        {
            if (this.Count <= 0)
                return default(T);
            return this[0];
        }

        public new void Add(T ent)
        {
            if (Hash.ContainsKey(ent.TDID))
            {
                Hash[ent.TDID] = ent;
            }
            else
            {
                Hash.Add(ent.TDID, ent);
            }
            base.Add(ent);
        }
        public new void Remove(T ent)
        {
            Hash.Remove(ent.TDID);
            if (Hash.ContainsKey(ent.TDID))
            {
                base.Remove(Hash[ent.TDID]);
            }
            base.Remove(ent);
        }
        public bool Contain(string id)
        {
            return Hash.ContainsKey(id);
        }
        public T Get(string id)
        {
            if (Hash.ContainsKey(id))
                return Hash[id];
            return default;
        }
        public new void Clear()
        {
            Hash.Clear();
            base.Clear();
        }
    }
}