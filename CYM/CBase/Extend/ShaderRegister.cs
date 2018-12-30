using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//**********************************************
// Class Name	: CYMShaderRegister
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class ShaderRegister: IResRegister<Shader>
    {
        public ShaderRegister()
        {
        }
        public ShaderRegister(string resPath)
        {
            path = resPath;
        }
        string path = "";
        public Shader this[string name]
        {
            get
            {
                if (!ContainsKey(name))
                {
                    Shader temp = Resources.Load(path + "/" + name) as Shader;
                    if (temp)
                    {
                        Add(name, temp);
                    }
                    else
                    {
                        // UnityEngine.Debug.Log("Error Path:" + path + "/" + name);
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
        public virtual void Add(Shader c)
        {
            Add(c.name, c);
        }
        public virtual void Add(string name, Shader c)
        {
            data.Add(name, c);
        }
        public virtual void Remove(string name)
        {
            data.Remove(name);
        }
        public virtual Shader Find(string c)
        {
            return data[c];
        }
        public virtual void Clear()
        {
            data.Clear();
        }
        public virtual void Unload()
        {
            //Clear();
        }

        public void Remove(Shader c)
        {
            throw new NotImplementedException();
        }

        public Shader Data(string name)
        {
            if(data.ContainsKey(name))
                return data[name];
            return null;
        }

        Dictionary<string, Shader> data = new Dictionary<string, Shader>();
    }
}