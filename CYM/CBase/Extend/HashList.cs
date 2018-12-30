//------------------------------------------------------------------------------
// HashList.cs
// Copyright 2018 2018/6/9 
// Created by CYM on 2018/6/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using System.Collections;

namespace CYM
{
    public class HashList<T> : List<T>
    {
        protected HashSet<T> Hash;
        int index;

        public HashList()
        {
            Hash = new HashSet<T>();
        }

        public T First()
        {
            if (this.Count <= 0)
                return default(T);
            return this[0];
        }

        public new void Add(T ent)
        {
            if (Hash.Contains(ent))
                return;
            Hash.Add(ent);
            base.Add(ent);
        }
        public new void Remove(T ent)
        {
            Hash.Remove(ent);
            base.Remove(ent);
        }
        public bool Contain(T ent)
        {
            return Hash.Contains(ent);
        }
        public new void Clear()
        {
            Hash.Clear();
            base.Clear();
        }

        public void Reset()
        {
            index = 0;
        }
    }
}