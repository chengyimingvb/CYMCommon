using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MoonSharp.Interpreter;
using CYM.Lua;
//**********************************************
// Class Name	: Battle_BaseLuaBattle
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public enum CloneType
    {
        None,       //不做任何拷贝,返回引用
        Memberwise, //浅层拷贝,拷贝所有值字段
        Deep,       //拷贝所有值字段,包括用户自定义的深层拷贝
    }
    #region base Config
    /// <summary>
    /// 通用的数据表格基类
    /// </summary>
    [Serializable]
    public class BaseConfig<TClass> : TDValue, ICloneable  where TClass : BaseConfig<TClass>, new()
    {
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        protected BaseGRMgr GRMgr => SelfBaseGlobal.GRMgr;
        protected BaseUnit SelfBaseUnit { get; set; }
        protected object[] AddedObjs { get; private set; }

        #region prop
        public bool IsSystem { get; set; } = false;
        public CloneType CloneType { get; set; } = CloneType.Memberwise;
        public string Icon { get; set; } = "";
        public string Desc { get; set; } = "";
        public string Name { get; set; } = "";
        public string Prefab { get; set; } = "";
        public string Template { get; set; } = "";
        #endregion

        #region 手动生命周期函数
        /// <summary>
        /// 被添加的时候触发:手动调用
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="obj"></param>
        public virtual void OnBeAdded(BaseCoreMono selfMono,params object[] obj)
        {
            AddedObjs = obj;
            SelfBaseUnit = selfMono as BaseUnit;
        }
        /// <summary>
        /// 被移除的时候,手动调用
        /// </summary>
        public virtual void OnBeRemoved()
        {

        }
        /// <summary>
        /// update:手动调用
        /// </summary>
        public virtual void OnUpdate()
        {

        }
        /// <summary>
        /// 帧同步:手动调用
        /// </summary>
        /// <param name="gameFramesPerSecond"></param>
        public virtual void GameFrameTurn(int gameFramesPerSecond)
        {
        }
        /// <summary>
        /// 更新:手动调用
        /// </summary>
        public virtual void GameLogicTurn()
        {

        }
        #endregion

        #region get
        /// <summary>
        /// 安全获得输入对象
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="obj"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected TType GetAddedObjData<TType>(int index) where TType :class
        {
            if (AddedObjs == null|| AddedObjs.Length<= index)
                return default(TType);
            return (AddedObjs[index] as TType);
        }
        /// <summary>
        /// 返回翻译后的名字
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            if (!Name.IsInvStr())
                BaseLanguageMgr.Get(Name);
            return BaseLanguageMgr.Get(TDID);
        }
        /// <summary>
        /// 获取自动提示
        /// </summary>
        /// <returns></returns>
        public virtual string GetDesc(params object[] ps)
        {
            if (!string.IsNullOrEmpty(Desc))
                return BaseLanguageMgr.Get(Desc, ps);
            string temp = BaseConstMgr.Prefix_DescTrans + TDID;
            if (BaseLanguageMgr.IsContain(temp))
                return BaseLanguageMgr.Get(temp, ps);
            return BaseLanguageMgr.Get(BaseConstMgr.STR_Desc_NoDesc);
        }
        /// <summary>
        /// 获取icon
        /// </summary>
        /// <returns></returns>
        public virtual Sprite GetIcon()
        {
            if (!Icon.IsInvStr())
                return GRMgr.GetIcon(Icon);
            return GRMgr.GetIcon(TDID);
        }
        /// <summary>
        /// 获得禁用的图标,有可能没有
        /// </summary>
        /// <returns></returns>
        public virtual Sprite GetDisIcon()
        {
            if (!Icon.IsInvStr())
                return GRMgr.GetIcon(Icon + BaseConstMgr.Suffix_Disable);
            return GRMgr.GetIcon(TDID + BaseConstMgr.Suffix_Disable);
        }

        /// <summary>
        /// prefab
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetPrefab()
        {
            if (!Prefab.IsInvStr())
                return GRMgr.GetPrefab(Prefab);
            return GRMgr.GetPrefab(TDID);
        }
        /// <summary>
        /// 获得animator
        /// </summary>
        /// <returns></returns>
        public virtual RuntimeAnimatorController GetAnimator()
        {
            return GRMgr.GetAnimator(TDID);
        }
        #endregion

        /// <summary>
        /// 复制对象
        /// </summary>
        /// <returns></returns>
        public virtual TClass Copy()
        {
            TClass tempBuff=null;
            {
                //不做任何拷贝,返回引用
                if (CloneType == CloneType.None)
                    tempBuff = (TClass)this;
                //浅层拷贝,拷贝所有值字段
                else if (CloneType == CloneType.Memberwise)
                    tempBuff = Clone() as TClass;
                //拷贝所有值字段,包括用户自定义的深层拷贝
                else if (CloneType == CloneType.Deep)
                {
                    tempBuff = Clone() as TClass;
                    tempBuff.DeepClone();
                }
            }
            return tempBuff;
        }
        protected virtual void DeepClone()
        {
            throw new NotImplementedException("此函数必须被实现");
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
    #endregion

    #region Mgr
    /// <summary>
    /// 会根据类名自动生成Lua方发, e.g. TDNationData 会截头去尾 变成:AddNation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LuaTDMgr<T> : TDManager<T>, ITDBase, ITDLuaMgr where T : TDValue, new()
    {
        public List<string> Keys { get; private set; } = new List<string>();
        public LuaTDMgr()
        {
            Init();
        }
        public LuaTDMgr(string keyName)
        {
            Init(keyName);
        }

        void Init(string keyName=null)
        {
            if (keyName == null)
                LuaTableKey = typeof(T).Name;
            else
                LuaTableKey = keyName;

            //去除头部TD前缀和尾部Data后缀
            {
                LuaTableKey = LuaTableKey.TrimStart("TD");
                LuaTableKey = LuaTableKey.TrimEnd("Data");
            }

            NameSpace = typeof(T).Namespace.ToString();
            AddMethonName = string.Format("Add{0}", LuaTableKey);
            BaseLuaMgr.AddGlobalAction(AddMethonName, Add);
            BaseDataParseMgr.AddTDLuaMgr(this);
        }

        public static readonly string Template = "Template";
        /// <summary>
        /// 默认lua表格数据
        /// </summary>
        protected string LuaTableKey = BaseConstMgr.STR_Inv;
        protected DynValue MetaTable;
        protected string AddMethonName;
        /// <summary>
        /// 默认的命名控件
        /// </summary>
        protected string NameSpace = BaseConstMgr.STR_Inv;
        protected DynValue baseTable;
        protected T TempClassData;
        public T Default { get; private set; }
       

        public virtual void Add(DynValue table)
        {
            if (MetaTable == null)
                MetaTable = GetDynVal(LuaTableKey);
            if (!MetaTable.IsNil())
                baseTable = AddByDefault(MetaTable, table);
            else
                baseTable = table;

            //获得Lua类模板
            string temp = GetStrByBaseTable(Template);
            Type classType=typeof(T);
            if (temp != null)
            {
                classType = Type.GetType(NameSpace + "." + temp, true, false);
                if (classType == null)
                {
                    CLog.Error("无法找到此类型:" + temp);
                }
                TempClassData = classType.Assembly.CreateInstance(classType.FullName) as T;
            }
            else
            {
                TempClassData = new T();
            }

            TempClassData = (T)LuaReader.Read(baseTable, classType);
            if (TempClassData == null) return;
            string key = TempClassData.TDID;
            if (Data.ContainsKey(key))
            {
                CLog.Error(baseTable.ToString() + "已经存在这个key:" + key);
                return;
            }
            TempClassData.OnBeAddedToData();
            Add(key, TempClassData);
            Keys.Add(key);
        }

        public void Add<TSubClass>(string key) where TSubClass:T,new()
        {
            TSubClass tempclass = new TSubClass();
            tempclass.OnBeAddedToData();
            Add(key, tempclass);
            Keys.Add(key);
        }
        public void Add<TSubClass>() where TSubClass : T, new()
        {
            Add<TSubClass>(typeof(TSubClass).Name);
        }

        protected Table GetTable(string name)
        {
            DynValue temp = BaseLuaMgr.Lua.Globals.Get(name);
            if (temp == null)
                return null;
            return temp.Table ;
        }
        protected DynValue GetDynVal(string name)
        {
            DynValue temp = BaseLuaMgr.Lua.Globals.Get(name);
            return temp;
        }
        protected string GetStrByBaseTable(string name)
        {
            DynValue temp = baseTable.Table.RawGet(name);
            if (temp == null)
                return null;
            return temp.String;
        }
        public DynValue AddByDefault(DynValue defaultTable, DynValue table)
        {
            if (defaultTable == null) return null;
            if (table == null) return null;
            Closure funcCopyPairs = GetDynVal("CopyPairs").Function;
            Closure funcGetNewTable = GetDynVal("GetNewTable").Function;
            DynValue ret = funcGetNewTable.Call();
            funcCopyPairs.Call(ret, defaultTable);
            funcCopyPairs.Call(ret, table);
            return ret;
        }

        #region Callback
        public virtual void OnLuaParseStart()
        {

        }

        public virtual void OnLuaParseEnd()
        {
            if (MetaTable == null)
                MetaTable = GetDynVal(LuaTableKey);
            Default = (T)LuaReader.Read(MetaTable, typeof(T));
        }
        #endregion
    }
    #endregion
}