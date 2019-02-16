//------------------------------------------------------------------------------
// MutiDic.cs
// Copyright 2019 2019/1/21 
// Created by CYM on 2019/1/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using System;

namespace CYM
{
    public class MultiDic<TKey,TValue>:Dictionary<Tuple<TKey,TKey>,TValue>
    {
        /// <summary>
        /// 如果没有,则会自动添加一个
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="value"></param>
        public void Change(TKey key1, TKey key2,TValue value)
        {
            var oneKey = Tuple.Create(key1, key2);
            var twoKey = Tuple.Create(key2, key1);
            bool haveOne = ContainsKey(oneKey);
            bool haveTwo = ContainsKey(twoKey);

            if (haveOne)
            {
                this[oneKey] = value;
            }
            else if (haveTwo)
            {
                this[twoKey] = value;
            }
            else
            {
                Add(oneKey,value);
            }
        }
        public void Remove(TKey key1, TKey key2)
        {
            var oneKey = Tuple.Create(key1, key2);
            var twoKey = Tuple.Create(key2, key1);
            Remove(oneKey);
            Remove(twoKey);
        }
        public bool ContainsKey(TKey key1, TKey key2)
        {
            var oneKey = Tuple.Create(key1, key2);
            var twoKey = Tuple.Create(key2, key1);
            bool haveOne = ContainsKey(oneKey);
            bool haveTwo = ContainsKey(twoKey);

            if (!haveOne && !haveTwo)
            {
                return false;
            }

            return true;
        }
        public TValue Get(TKey key1, TKey key2)
        {
            var oneKey = Tuple.Create(key1, key2);
            var twoKey = Tuple.Create(key2, key1);

            if (ContainsKey(oneKey))
                return this[oneKey];
            else if (ContainsKey(twoKey))
                return this[twoKey];
            else
                return default;
        }
    }
}