//------------------------------------------------------------------------------
// SortList.cs
// Copyright 2018 2018/10/28 
// Created by CYM on 2018/10/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using System;
using System.Linq;

namespace CYM
{
    // 通用的表格排序类
    public class ListSorter<T>
    {
        // 排序依据
        // -1表示不排序
        private int By { get; set; }
        // 是否升序
        // 排序依据为-1时，这个不起作用
        private bool IsReversed { get; set; }

        public delegate IEnumerable<T> Call(IEnumerable<T> raw);

        Call[] _sorts;

        public ListSorter(Call[] sorts)
        {
            if (sorts == null)
            {
                throw new ArgumentNullException();
            }
            _sorts = sorts;
        }

        public IEnumerable<T> Sort(IEnumerable<T> datas,int by)
        {
            SortBy(by);
            IEnumerable<T> result = null;
            if (By > -1)
            {
                result = _sorts[By](datas);
                if (IsReversed)
                {
                    result = result.Reverse();
                }
            }
            else
            {
                result = datas;
            }
            return result;
        }

        // 实现一个按钮点第二次就按相反的顺序排序
        void SortBy(int by)
        {
            if (by > _sorts.Length)
            {
                CLog.Error("ListSort,数组越界!!!");
            }
            if (By == by)
            {
                IsReversed = !IsReversed;
            }
            else
            {
                By = by;
                IsReversed = false;
            }
        }
    }
}