using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//**********************************************
// Class Name	: CYMObjectRegister
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class ObjectRegister<T> where T : Object
    {
        AssetsPathType pathType;
        public ObjectRegister()
        {
        }
        public ObjectRegister(string resPath, AssetsPathType type)
        {
            path = resPath;
            pathType = type;
        }
        public string Path
        {
            get
            {
                if (pathType == AssetsPathType.Resources)
                    return Application.dataPath + "/Resources/" + path + "/";
                else if (pathType == AssetsPathType.AssetBundle)
                    return path;
                return "No Path";
            }
        }
        private string path = "";
        public T this[string name]
        {
            get
            {
                if (!ContainsKey(name))
                {

                    T temp = null;
                    if (pathType == AssetsPathType.Resources)
                        temp = Resources.Load(path + "/" + name, typeof(T)) as T;
                    else if (pathType == AssetsPathType.AssetBundle)
                    {
                        CLog.Error("因为是bundle资源，请先通过Add函数加载资源");
                        return null;
                    }

                    if (temp)
                    {
                        Add(name, temp);
                    }
                    else
                    {
                        CLog.Error(Path+name);
                        return null;
                    }
                }
                return data[name];
            }
        }

        public virtual bool ContainsKey(string name)
        {
            return data.ContainsKey(name);
        }

        public virtual void Add(T c)
        {
            Add(c.name, c);
        }

        public virtual void Add(string name, T c)
        {
			if (!ContainsKey (name)) {
				data.Add (name, c);
			}
        }

        public virtual void Remove(string name)
        {
            data.Remove(name);
        }
        public virtual T Find(string c)
        {
            return data[c];
        }
        public virtual void InitObjects(T[] childrenIn)
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
        Dictionary<string, T> data = new Dictionary<string, T>();

    }

}
