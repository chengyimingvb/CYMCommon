//------------------------------------------------------------------------------
// GOPool.cs
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

namespace CYM.Pool
{
    public class GOPool 
    {
        #region prop
        public List<GameObject> UnUsed { get; private set; } = new List<GameObject>();
        public List<GameObject> Used { get; private set; } = new List<GameObject>();
        GameObject Prefab;
        Transform Parent;
        #endregion

        #region life
        public GOPool(GameObject prefab, Transform parent)
        {
            Prefab = prefab;
            Parent = parent;
        }
        #endregion

        #region set
        public GameObject Spawn()
        {
            GameObject ret=null;
            if (UnUsed.Count > 0)
            {
                ret = UnUsed[0];
                UnUsed.RemoveAt(0);
            }
            else
            {
                ret = GameObject.Instantiate(Prefab, Parent) as GameObject;
            }
            ret.transform.position = Prefab.transform.position;
            ret.transform.rotation = Prefab.transform.rotation;
            Used.Add(ret);
            ret.SetActive(true);
            return ret;
        }
        public void Despawn(GameObject go)
        {
            Used.Remove(go);
            UnUsed.Add(go);
            go.SetActive(false);
        }
        public void DespawnAll()
        {
            foreach (var item in Used)
            {
                UnUsed.Add(item);
                item.SetActive(false);
            }
            Used.Clear();
        }
        public void Destroy()
        {
            foreach (var item in Used)
                GameObject.Destroy(item);
            foreach (var item in UnUsed)
                GameObject.Destroy(item);
            Used.Clear();
            UnUsed.Clear();
        }
        #endregion
    }
}