using System;
using UnityEngine;
using System.Collections.Generic;
//**********************************************
// Class Name	: CYMTimer
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class IDMgr 
    {
        HashSet<int> data = new HashSet<int>();
        int nextId = int.MinValue;
        public int GetNextId()
        {
            if (nextId < int.MaxValue)
            {
                nextId++;
            }
            else
            {
                int ret = int.MinValue;
                while (true)
                {
                    if (data.Contains(ret))
                    {
                        ret++;
                    }
                    else
                    {
                        break;
                    }
                }
                nextId = ret;
            }
            
            data.Add(nextId);
            return nextId;
        }
        public void Add(int id)
        {
            if (!data.Contains(id))
            {
                data.Add(id);
                if(id>= nextId)
                {
                    nextId = id++;
                }
            }
        }
        public void Remove(int id)
        {
            data.Remove(id);
        }
        public void Clear()
        {
            data.Clear();
            nextId = 0;
        }
        public void Init(IEnumerator<ICYMBase> list)
        {
            data.Clear();
            if (list == null)
                return;
            while(list.MoveNext())
                data.Add(list.Current.ID);
                
        }
    }

}