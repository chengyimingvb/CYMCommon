using UnityEngine;
using System.Collections.Generic;

//**********************************************
// Class Name	: CYMGameobjectRegister
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class GameobjectRegister : IResRegister<GameObject>
    {
        public GameobjectRegister()
        {
        }
        public GameobjectRegister(string resPath)
        {
            path = resPath;
        }
        string path = "";
        public GameObject this[string name]
        {
            get
            {
                if (!ContainsKey(name))
                {
                    GameObject temp = Resources.Load(path + "/"+ name,typeof(GameObject)) as GameObject;
                    if (temp)
                    {
                        Add(name, temp);
                    }
                    else
                    {
                        CLog.Error("Error Path:" + path + "/"+ name);
                        return null;
                    }
                }
                return Data(name);
            }
        }

        public virtual void Add(GameObject c)
        {
            Add(c.name, c);
        }
        public virtual void Add(string name, GameObject c)
        {
            if (!data.ContainsKey(name))
            {
                data.Add(name, c);
            }
        }
        public virtual void Remove(GameObject c)
        {
            Remove(c.name);
        }
        public virtual void Remove(string name)
        {
            data.Remove(name);
        }
        public virtual GameObject Data(string name)
        {
            if (!ContainsKey(name))
                return null;
            return (data[name]);
        }
        public virtual bool ContainsKey(string name)
        {
            return data.ContainsKey(name);
        }
        public virtual void InitObjects(GameObject[] childrenIn)
        {
            for (int i = 0; i < childrenIn.Length; ++i)
            {
                Add(childrenIn[i]);
            }
        }
        public virtual void Clear()
        {
            data.Clear();
        }
        Dictionary<string, GameObject> data = new Dictionary<string, GameObject>();
    }
}
