using System.Collections;
using System.Collections.Generic;
using CYM;
using System.IO;
using System;
using UnityEngine.SceneManagement;

namespace CYM
{
    public enum RemoveBuffType
    {
        Once,//移除一层
        Group,//移除一组
    }
    /// <summary>
    /// BUFF管理器
    /// </summary>
    /// <typeparam name="TTable">buff table</typeparam>
    /// <typeparam name="TData">buff data</typeparam>
    /// <typeparam name="TType">属性的类型枚举</typeparam>
    public class BaseBuffMgr<TData, TType> : BaseCoreMgr, ITableDataMgr<TData>  where TData : TDBaseBuffData<TType>, new() where TType : struct
    {
        #region buff group
        /// <summary>
        /// buff组
        /// </summary>
        public class BuffGroup
        {
            public TData this[int index]
            {
                get
                {
                    return BuffList[index];
                }
            }
            public BuffGroup()
            {
                MaxLayer = TDBaseBuffData<TType>.MAX_LAYER;
            }
            private List<TData> buffList = new List<TData>();
            public int Layer { get { return BuffList.Count; } }
            public int MaxLayer { get; set; }

            public List<TData> BuffList
            {
                get
                {
                    return buffList;
                }

                set
                {
                    buffList = value;
                }
            }

            public TData Add(TData buff, BaseUnit self, BaseUnit caster, TDBaseSkillData fromSkill)
            {
                MaxLayer = buff.MaxLayer > MaxLayer ? buff.MaxLayer : MaxLayer;
                if (MaxLayer <= 0)
                    return null;
                if (MaxLayer > Layer)
                {
                    BuffList.Add(buff);
                    buff.OnBeAdded(self, caster, fromSkill);
                }
                else
                {
                    Remove(buff);
                    BuffList.Add(buff);
                    buff.OnBeAdded(self, caster, fromSkill);
                }
                return buff;
            }
            public TData Merge(TData buff, BaseUnit self, BaseUnit caster, TDBaseSkillData fromSkill)
            {
                MaxLayer = buff.MaxLayer > MaxLayer ? buff.MaxLayer : MaxLayer;
                if (MaxLayer <= 0)
                    return null;
                TData newBuff = null;
                if (BuffList.Count == 0)
                    newBuff = Add(buff, self, caster, fromSkill);
                else
                {
                    newBuff = BuffList[0];
                    if (newBuff.MergeLayer >= MaxLayer)
                        return newBuff;
                    newBuff.OnMerge(buff, caster, fromSkill);
                }
                return newBuff;
            }
            public void Remove(TData buff)
            {
                BuffList[0].OnBeRemoved();
                BuffList.RemoveAt(0);
            }
            public TData GeBuff()
            {
                if (BuffList == null || BuffList.Count <= 0)
                    return null;
                return BuffList[0];
            }
        }
        #endregion

        #region member variable
        public Callback Callback_OnBuffChange { get; set; }
        public Dictionary<string, BuffGroup> Data { get; private set; } = new Dictionary<string, BuffGroup>();
        public List<BuffGroup> ListData { get; private set; } = new List<BuffGroup>();
        public List<BuffGroup> UpdateDataList { get; private set; } = new List<BuffGroup>();
        private List<TData> clearBuff = new List<TData>();
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfBaseUnit = mono as BaseUnit;
        }
        public override void Death(BaseUnit caster)
        {
            Clear();
            base.Death(caster);
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void ManualUpdate()
        {
            if (SelfBaseGlobal.PlotMgr.IsPlotMode)
                return;
            if (clearBuff.Count > 0)
                clearBuff.Clear();
            foreach (var item in UpdateDataList)
            {
                var temp = item;
                for (int i = 0; i < temp.Layer; ++i)
                {
                    temp[i].OnUpdate();
                    if (temp[i].IsTimeOver)
                        clearBuff.Add(temp[i]);
                }
            }
            foreach (var item in clearBuff)
            {
                Remove(item);
            }
        }
        #endregion

        #region methon
        public virtual void Clear()
        {
            foreach (var item in Data)
            {
                var temp = item.Value;
                for (int i = 0; i < temp.Layer; ++i)
                {
                    clearBuff.Add(temp[i]);
                }
            }
            foreach (var item in clearBuff)
            {
                Remove(item);
            }
            ListData.Clear();
            Data.Clear();
        }
        public virtual List<TData> Add(List<string> buffName, BaseUnit caster = null, float step = 0.0f,TDBaseSkillData skill=null)
        {
            if (buffName == null) return null;
            if (buffName.Count == 0) return null;
            List<TData> ret = new List<TData>();
            for (int i = 0; i < buffName.Count; ++i)
            {
                ret.Add(Add(buffName[i], caster, step, skill));
            }
            return ret;
        }
        public virtual List<TData> Add(string[] buffName, BaseUnit caster = null, float step = 0.0f, TDBaseSkillData skill = null)
        {
            if (buffName == null) return null;
            if (buffName.Length == 0) return null;
            List<TData> ret = new List<TData>();
            for (int i = 0; i < buffName.Length; ++i)
            {
                ret.Add(Add(buffName[i], caster, step, skill));
            }
            return ret;
        }
        /// <summary>
        /// 添加一个buff
        /// </summary>
        /// <param name="buffName"></param>
        /// <param name="caster">如果为null，caster默认为自己</param>
        public virtual TData Add(string buffName, BaseUnit caster = null, float step = 0.0f,TDBaseSkillData skill=null)
        {
            if (buffName.IsInvStr())
                return null;
            if (!Table.Contains(buffName))
            {
                CLog.Error("未找到buff errorId=" + buffName);
                return null;
            }
            TData tempBuff = Table.Find(buffName).Copy() as TData;
            return Add(tempBuff, caster, step, skill);
        }
        public virtual void Remove(List<string> buffName)
        {
            if (buffName == null) return;
            for (int i = 0; i < buffName.Count; ++i)
            {
                Remove(buffName[i]);
            }
        }
        public virtual void Remove(string buffName, RemoveBuffType type = RemoveBuffType.Once)
        {
            if (buffName.IsInvStr())
                return;
            if (!Table.Contains(buffName))
                return;
            if (!Data.ContainsKey(buffName))
                return;
            BuffGroup group = Data[buffName];
            if (type == RemoveBuffType.Group)
            {
                for (int i = 0; i < group.Layer; ++i)
                    Remove(group[i]);
            }
            else if (type == RemoveBuffType.Once)
            {
                if (group != null && group.Layer > 0)
                    Remove(group[0]);
            }
        }
        public virtual void RemoveAll(Predicate<TData> func)
        {
            List<TData> clearBuff = new List<TData>();
            foreach (var item in Data)
            {
                if (func(item.Value.GeBuff()))
                {
                    clearBuff.Add(item.Value.GeBuff());
                }
            }
            foreach (var item in clearBuff)
            {
                Remove(item);
            }
        }
        #endregion

        #region is
        public bool IsHave(List<string> buffName)
        {
            if (buffName == null) return false;
            for (int i = 0; i < buffName.Count; ++i)
            {
                if (Data.ContainsKey(buffName[i]))
                    return true;
            }
            return false;
        }
        public bool IsHave(string buffName)
        {
            if (buffName == null) return false;
            if (Data.ContainsKey(buffName))
                return true;
            return false;
        }
        #endregion

        #region get
        public TData Get(string id)
        {
            if (Data.ContainsKey(id))
                return Data[id].GeBuff();
            return null;
        }
        public TData Get(int index)
        {
            if (ListData.Count>index)
                return ListData[index].GeBuff();
            return null;
        }
        public List<TData> GeBuffs(List<string> buffs)
        {
            List<TData> ret = new List<TData>();

            foreach (var buffid in buffs)
            {
                if (Data.ContainsKey(buffid))
                {
                    BuffGroup group = Data[buffid];
                    foreach (var item in group.BuffList)
                    {
                        ret.Add(item);
                    }
                }
            }

            return ret;
        }
        #endregion

        #region final action
        public virtual TData Add(TData buff, BaseUnit caster = null, float step = 0.0f,TDBaseSkillData skill=null)
        {
            if (!SelfBaseUnit.IsLive)
                return null;
            TData newBuff = null;
            BuffGroup buffGroup = null;
            //有buff组的buff叠加
            if (!string.IsNullOrEmpty(buff.BuffGroupID))
            {
                if (!Data.ContainsKey(buff.BuffGroupID))
                {
                    var tempGroup = new BuffGroup();
                    Data.Add(buff.BuffGroupID, tempGroup);
                    ListData.Add(tempGroup);

                    //非永久buff加入 update队列
                    if (buff.MaxTime > 0)
                        UpdateDataList.Add(tempGroup);
                }
                buffGroup = Data[buff.BuffGroupID];
                newBuff = buffGroup.Add(buff, SelfBaseUnit, caster, skill);
            }
            //没有buff组的buff合并
            else
            {
                if (!Data.ContainsKey(buff.TDID))
                {
                    var tempGroup = new BuffGroup();
                    Data.Add(buff.TDID, tempGroup);
                    ListData.Add(tempGroup);

                    //非永久buff加入 update队列
                    if (buff.MaxTime > 0)
                        UpdateDataList.Add(tempGroup);
                }
                buffGroup = Data[buff.TDID];
                newBuff = buffGroup.Merge(buff, SelfBaseUnit, caster, skill);
            }
            Callback_OnBuffChange?.Invoke();
            if (newBuff != null)
                newBuff.SetStep(step);
            return newBuff;
        }
        public virtual void Remove(TData buff)
        {
            //buff叠加
            if (!string.IsNullOrEmpty(buff.BuffGroupID))
            {
                if (Data.ContainsKey(buff.BuffGroupID))
                {
                    var tempGroup = Data[buff.BuffGroupID];
                    tempGroup.Remove(buff);
                    if (tempGroup.Layer <= 0)
                    {
                        Data.Remove(buff.BuffGroupID);
                        ListData.Remove(tempGroup);

                        if (buff.MaxTime > 0)
                            UpdateDataList.Remove(tempGroup);
                    }
                }
            }
            //buff合并
            else
            {
                if (Data.ContainsKey(buff.TDID))
                {
                    var tempGroup = Data[buff.TDID];
                    tempGroup.Remove(buff);
                    if (tempGroup.Layer <= 0)
                    {
                        Data.Remove(buff.TDID);
                        ListData.Remove(tempGroup);

                        if (buff.MaxTime > 0)
                            UpdateDataList.Remove(tempGroup);
                    }
                }
            }
            Callback_OnBuffChange?.Invoke();
        }
        #endregion

        #region utile
        /// <summary>
        /// 通过ID获得buff实体
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<TData> GetTableBuffs(List<string> ids)
        {
            List<TData> temp = new List<TData>();
            foreach (var item in ids)
            {
                var tempBuff = Table.Find(item);
                if (tempBuff == null)
                {
                    CLog.Error("没有这个buff:" + item);
                    continue;
                }
                temp.Add(tempBuff);
            }
            return temp;
        }
        #endregion

        #region AddtionStr
        /// <summary>
        /// 设置buff信息
        /// </summary>
        /// <returns></returns>
        private string AppendBuffHeadInfo(TData buff)
        {
            return BaseLanguageMgr.Get("Buff_HeadInfo", buff.GetName(), buff.MaxTime == 0.0f ? BaseLanguageMgr.Get("永久") : buff.MaxTime.ToString());
        }
        /// <summary>
        /// 拼接所有传入的buff addtion 的字符窜
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public string GetAddtionStr(List<string> ids, string split = " ", float? anticipationFaction = null, bool appendHeadInfo = false)
        {
            return GetAddtionStr(GetTableBuffs(ids), split, anticipationFaction, appendHeadInfo);
        }
        /// <summary>
        /// 拼接所有传入的buff addtion 的字符窜
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public string GetAddtionStr(List<TData> buffs, string split = " ", float? anticipationFaction = null, bool appendHeadInfo = false)
        {
            string showStr = "";
            foreach (var item in buffs)
            {
                if (appendHeadInfo)
                {
                    showStr += AppendBuffHeadInfo(item);
                }
                foreach (var addStr in item.GetAddtionDescs(anticipationFaction))
                    showStr += split + addStr;
            }
            return showStr;
        }
        /// <summary>
        /// 拼接所有传入的buff addtion 的字符窜
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="appendHeadInfo">是否包含buff头信息,比如cd,buff名称</param>
        /// <returns></returns>
        public string GetAddtionStr(string id, string split = " ", float? anticipationFaction = null, bool appendHeadInfo = false)
        {
            var tempBuff = Table.Find(id);
            if (tempBuff == null)
            {
                CLog.Error("没有这个buff:" + id);
                return "";
            }
            return GetAddtionStr(tempBuff, split, anticipationFaction, appendHeadInfo);
        }
        /// <summary>
        /// 通过所有传入的buff获得加成字符窜
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public string GetAddtionStr(TData buff, string split = " ", float? anticipationFaction = null, bool appendHeadInfo = false)
        {
            string showStr = "";
            if (buff == null)
                return "";
            if (appendHeadInfo)
            {
                showStr += AppendBuffHeadInfo(buff);
            }
            foreach (var addStr in buff.GetAddtionDescs(anticipationFaction))
                showStr += split + addStr;
            return showStr;
        }
        #endregion

        #region Must overide
        /// <summary>
        /// 找到buff
        /// </summary>
        /// <returns></returns>
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                throw new NotImplementedException("这个函数必须被实现");
            }
        }

        public virtual TData CurData
        {
            get;

            set;
        }
        #endregion
    }

}