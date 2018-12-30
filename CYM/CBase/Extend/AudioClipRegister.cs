using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CYM
{
    public class AudioClipRegister : IResRegister<AudioClip>
    {

        public AudioClipRegister()
        {
        }
        public AudioClipRegister(string resPath)
        {
            path = resPath;
        }
        string path = "";
        public AudioClip this[string name]
        {
            get
            {
                if (!ContainsKey(name))
                {
                    AudioClip temp = Resources.Load(path + "/" + name, typeof(AudioClip)) as AudioClip;
                    if (temp)
                    {
                        Add(name, temp);
                    }
                    else
                    {
                        CLog.Error("Error Path:" + path + "/" + name);
                        return null;
                    }
                }
                return Data(name);
            }
        }

        public virtual void Add(AudioClip c)
        {
            Add(c.name, c);
        }
        public virtual void Add(string name, AudioClip c)
        {
            if (!data.ContainsKey(name))
            {
                data.Add(name, c);
            }
        }
        public virtual void Remove(AudioClip c)
        {
            Remove(c.name);
        }
        public virtual void Remove(string name)
        {
            data.Remove(name);
        }
        public virtual AudioClip Data(string name)
        {
            if (!ContainsKey(name))
                return null;
            return (data[name]);
        }
        public virtual bool ContainsKey(string name)
        {
            return data.ContainsKey(name);
        }
        public virtual void InitObjects(AudioClip[] childrenIn)
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
        Dictionary<string, AudioClip> data = new Dictionary<string, AudioClip>();
    }
}