using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using CYM;

//**********************************************
// Class Name	: CYMHashManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    #region HashManager
    public class HashManager<K,T> : ICYMManager where T : ICYMBase
    {
        T defaultVal = default(T);
        public T this[K id]
        {
            get
            {
                return Data[id];
            }
        }

        Dictionary<K, T> _data = new Dictionary<K, T>();

        public Dictionary<K, T> Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public virtual bool Contains(K id)
        {
            return Data.ContainsKey(id);
        }

        public virtual void Add(K id, T ent)
        {
            if(Data.ContainsKey(id))
            {
                CLog.Error("重复的id:"+id);
                return;
            }
            Data.Add(id,ent);
        }

        public virtual void Add(T ent)
        {
        }

        public virtual void Remove(K id)
        {
            Data.Remove(id);
        }
        public virtual T Find(K id)
        {
            T temp = defaultVal;
            if (id == null)
                return temp;
            Data.TryGetValue(id, out temp);
            return temp;
        }

        public int Count()
        {
            return Data.Count;
        }

        public bool IsEmpty()
        {
            return Data.Count == 0;
        }

        public void Clear()
        {
            Data.Clear();
        }

        public virtual T FindDeeapCopy(K id)
        {
            return (T)BaseCoreMono.GetDeepCopy(Find(id));
        }
    }

    public class ComponetHashManager: ICYMManager
    {
        public BaseCoreMgr this[Type id]
        {
            get
            {
                return Data[id];
            }
        }

        Dictionary<Type, BaseCoreMgr> _data = new Dictionary<Type, BaseCoreMgr>();

        public Dictionary<Type, BaseCoreMgr> Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public virtual bool Contains(Type id)
        {
            return Data.ContainsKey(id);
        }

        public virtual void Add(BaseCoreMgr ent)
        {
            Data.Add(ent.GetType(), ent);
        }

        public virtual void Remove(Type id)
        {
            Data.Remove(id);
        }
        public virtual BaseCoreMgr Find(Type id)
        {
            BaseCoreMgr temp;
            Data.TryGetValue(id, out temp);
            return temp;
        }

        public int Count()
        {
            return Data.Count;
        }

        public bool IsEmpty()
        {
            return Data.Count == 0 ? true : false;
        }

        public void Clear()
        {
            Data.Clear();
        }
    }
    #endregion

    #region HashManagerForId
    public class HashManagerAutoId<T> : HashManager<int,T> where T : ICYMBase
    {
        IDMgr idMgr = new IDMgr();
        public override void Add(T ent)
        {
            int nextId = ent.ID = idMgr.GetNextId();
            Data.Add(nextId, ent);
        }
        public override void Add(int id, T ent)
        {
            if (Data.ContainsKey(id))
            {
                CLog.Error("重复的id:" + id);
                return;
            }
            ent.ID = id;
            Data.Add(id, ent);
            idMgr.Add(id);
        }
        public void ClearAll()
        {
            Clear();
            idMgr.Clear();
        }
        public override void Remove(int id)
        {
            base.Remove(id);
            idMgr.Remove(id);
        }
        //public void SetNextId(int nextId)
        //{
        //    this.nextId = nextId;
        //}
        //public void AddNextId()
        //{
        //    nextId++;
        //}
        //public int GetNextId()
        //{
        //    int ret = 0;
        //    while (true)
        //    {
        //        if (Data.ContainsKey(ret))
        //        {
        //            ret++;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return ret;
        //}
    }
    #endregion

    #region TD
    [Serializable]
    public class TDValue : ICYMBase
    {
        public int ID { get; set; }

        public string TDID { get; set; } = BaseConstMgr.STR_Inv;

        /// <summary>
        /// 被添加到数据表里
        /// </summary>
        public virtual void OnBeAddedToData()
        {

        }
    }

    public class TDManager<T>:HashManager<string,T> where T : TDValue
    {
        public override void Add(string id, T ent)
        {
            if (Data.ContainsKey(id))
                return;
            ent.TDID = id;
            Data.Add(id, ent);
        }

        public override void Add(T ent)
        {
            Data.Add(ent.TDID, ent);
        }

    }
    #endregion

    #region ListManager

    //public class ListManager<T> : ICYMManager where T : ICYMBase
    //{
    //    [NotNull] private List<T> _data = new List<T>();

    //    public List<T> Data
    //    {
    //        get { return _data; }
    //        set { _data = value; }
    //    }

    //    public virtual T FindByIndex(int index)
    //    {
    //        return _data[index];
    //    }

    //    public virtual T FindById(int id)
    //    {
    //        return _data.Find(delegate(T p) { return p.ID == id; });
    //    }

    //    public virtual void Remove(int id)
    //    {
    //        _data.RemoveAll(delegate(T p) { return p.ID == id; });
    //    }

    //    public virtual void Remove(T ent)
    //    {
    //        _data.Remove(ent);
    //    }

    //    public virtual void Add(T ent)
    //    {
    //        _data.Add(ent);
    //    }

    //    public T this[int index]
    //    {
    //        get
    //        {
    //            return _data[index];
    //        }
    //        set
    //        {
    //            _data[index] = value;
    //        }
    //    }

    //    public int Count()
    //    {
    //        return _data.Count;
    //    }

    //    public bool IsEmpty()
    //    {
    //        return _data.Count == 0 ? true : false;
    //    }

    //    public void Clear()
    //    {
    //        _data.Clear();
    //    }

    //    public void Reverse()
    //    {
    //        _data.Reverse();
    //    }

    //    public bool Contains(T ent)
    //    {
    //        return _data.Contains(ent);
    //    }

    //    public void Sort()
    //    {
    //        _data.Sort();
    //    }

    //    public int FindIndex(T ent)
    //    {
    //        for (int i = 0; i < Count(); i++)
    //        {
    //            if (_data[i].Equals(ent))
    //                return i;
    //        }
    //        return -1;
    //    }
    //}

    public class ListManagerAutoId<T> : ICYMManager where T : ICYMBase
    {
        [NotNull] private List<T> _data = new List<T>();
        protected int nextId = 0;

        public List<T> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public virtual T FindByIndex(int index)
        {
            return _data[index];
        }

        public virtual T FindById(int id)
        {
            return _data.Find(delegate(T p) { return p.ID == id; });
        }

        public virtual void Remove(int id)
        {
            _data.RemoveAll(delegate(T p) { return p.ID == id; });
        }

        public virtual void Remove(T ent)
        {
            _data.Remove(ent);
        }

        public virtual void Add(T ent)
        {
            ent.ID = nextId;
            _data.Add(ent);
            nextId++;
        }

        public T this[int index]
        {
            get
            {
                return _data[index];
            }
            set
            {
                _data[index] = value;
            }
        }

        public int Count()
        {
            return _data.Count;
        }

        public bool IsEmpty()
        {
            return _data.Count == 0 ? true : false;
        }

        public void Clear()
        {
            _data.Clear();
        }

        public void Reverse()
        {
            _data.Reverse();
        }

        public bool Contains(T ent)
        {
            return _data.Contains(ent);
        }

        public void Sort()
        {
            _data.Sort();
        }

        public int FindIndex(T ent)
        {
            for (int i = 0; i < Count(); i++)
            {
                if (_data[i].Equals(ent))
                    return i;
            }
            return -1;
        }
    }

    #endregion
}
