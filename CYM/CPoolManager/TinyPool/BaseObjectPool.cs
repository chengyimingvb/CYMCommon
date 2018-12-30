//------------------------------------------------------------------------------
// BaseObjectPool.cs
// Copyright 2018 2018/3/29 
// Created by CYM on 2018/3/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using System;

namespace CYM.Pool
{
    public class BaseObjectPool<T>
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        System.Func<T> _create;

        public int CountTotal
        {
            get;
            private set;
        }

        public int CountOutPool
        {
            get
            {
                return this.CountTotal - this.CountInPool;
            }
        }

        public int CountInPool
        {
            get
            {
                return this.m_Stack.Count;
            }
        }

        public string PoolInfo
        {
            get
            {
                return string.Format("BaseObjectPool<{0}>, countInPool = {1}, countTotal = {2}", typeof(T).Name, CountInPool, CountTotal);
            }
        }

        public BaseObjectPool(System.Func<T> create = null, int startCount = 0)
        {
            _create = create;
            for (int i = 0; i < startCount; i++)
            {
                m_Stack.Push(CreateInstance());
            }
        }

        T CreateInstance()
        {
            T t;
            if (_create != null)
            {
                t = _create();
            }
            else
            {
                t = CreateInstanceDefualt();
            }
            this.CountTotal++;
            return t;
        }

        public virtual T Get()
        {
            T t;
            if (CountInPool == 0)
            {
                t = CreateInstance();
            }
            else
            {
                t = GetInPool();
            }

            return t;
        }

        // 获取已经存在与pool中的对象
        public T GetInPool()
        {
            return this.m_Stack.Pop();
        }

        protected virtual T CreateInstanceDefualt()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual void Release(T element)
        {
            if (this.m_Stack.Count > 0 && object.ReferenceEquals(this.m_Stack.Peek(), element))
            {
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }

            this.m_Stack.Push(element);
        }
    }
}