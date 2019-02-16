using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    #region data
    /// <summary>
    /// 属性加成
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AttrAdditon<T> : ICYMBase, ICloneable where T : struct
    {
        public AttrAdditon(T _type, AttrOpType _addType, float _val)
        {
            Type = _type;
            AddType = _addType;
            Val = _val;
        }
        public AttrAdditon()
        {

        }
        public int ID { get; set; }
        public string TDID { get; set; }
        public T Type { get; set; }
        public AttrOpType AddType { get; set; } = AttrOpType.DirectAdd;
        public AttrFactionType FactionType { get; set; } = AttrFactionType.Percent;
        public float Val { get; set; }
        /// <summary>
        /// 每个step产生的faction因子
        /// </summary>
        public float Faction { get; set; } = 0.0f;
        /// <summary>
        /// 模步 动态设置
        /// </summary>
        public float Step = 0;
        /// <summary>
        /// 真实的数值
        /// </summary>
        public float RealVal=> AnticipationVal(RealFaction);
        /// <summary>
        /// 真实的修正因子
        /// </summary>
        public float RealFaction=> Step * Faction;

        #region set
        public float AnticipationVal(float val)
        {
            return BaseAttrMgr<T>.GetAttrValByFaction(Val, val, FactionType);
        }
        public void SetStep(float step)
        {
            Step = step;
        }
        #endregion

        #region is
        public bool IsPercentOp()
        {
            return AddType == AttrOpType.PercentAdd || AddType == AttrOpType.Percent;
        }
        #endregion

        #region get
        /// <summary>
        /// 获得加成的描述
        /// anticipationFaction:用户自定义的因子
        /// </summary>
        /// <returns></returns>
        public string GetDesc(bool isIgnoreName = false, bool isColor = true, float? anticipationFaction = null)
        {
            return BaseAttrMgr<T>.GetAttrStr(
                Type, 
                anticipationFaction == null ? RealVal : AnticipationVal(anticipationFaction.Value), 
                IsPercentOp(), 
                isIgnoreName, 
                isColor);
        }

        /// <summary>
        /// layer:用户自定义层数
        /// </summary>
        /// <param name="isIgnoreName"></param>
        /// <param name="isColor"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public string GetDescByLayer(bool isIgnoreName = false, bool isColor = true, int layer = 1)
        {
            return BaseAttrMgr<T>.GetAttrStr(
                Type, 
                RealVal * layer, 
                IsPercentOp(), 
                isIgnoreName, 
                isColor);
        }
        public object Clone()
        {
            return base.MemberwiseClone();
        }
        #endregion

    }
    /// <summary>
    /// 属性转换结构体
    /// </summary>
    public class AttrConvertData<T>
    {
        public float Max { get; set; } = float.MaxValue;  //最大转换
        public float Min { get; set; } = float.MinValue;  //最小转换
        public float Step { get; set; } = 1;              //转换阶梯
        public float Faction { get; set; } = 1;           //转换因子
        public bool IsReverse { get; set; } = false;      //是否反转
        public T From { get; set; }//来源属性类型
        public T To { get; set; }  //转换的属性类型

        public List<TDAttrData> AttrData;//属性数据
    }
    /// <summary>
    /// 消耗数据结构体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class CostData<T> : ICloneable where T : struct
    {
        public T Type { get; set; }
        public float Val { get; set; }
        /// <summary>
        /// 修正因子
        /// </summary>
        public float Faction { get; set; } = 0.0f;
        /// <summary>
        /// 模步 动态设置
        /// </summary>
        private float Step = 0.0f;
        private float Add = 0.0f;
        public float RealFaction
        {
            get
            {
                return Step * Faction;
            }
        }
        public float RealVal
        {
            get
            {
                return BaseAttrMgr<T>.GetAttrValByFaction(Val, RealFaction, FactionType) + Add;
            }
        }
        public bool IsCondition { get; set; } = true;
        public AttrFactionType FactionType { get; set; } = AttrFactionType.Percent;

        public void SetStep(float val)
        {
            Step = val;
        }
        public void SetAdd(float val)
        {
            Add = val;
        }

        public CostData(T type, float v)
        {
            Type = type;
            Val = v;
        }

        public CostData()
        {
        }


        public string ToString(bool isHaveSign = false, bool isHaveColor = true, bool isHeveAttrName = true)
        {
            return BaseAttrMgr<T>.GetFullAttrString(Type, RealVal, isHaveSign, isHaveColor, isHeveAttrName);
        }
        public string ToJumpCostStr(bool isReserve)
        {
            if (isReserve)
                return "+" + (Type as Enum).GetName() + BaseUIUtils.OptionalTwoDigit(RealVal);
            else
                return "-" + (Type as Enum).GetName() + BaseUIUtils.OptionalTwoDigit(RealVal);
        }
        public bool IsHaveVal()
        {
            return RealVal != 0.0f;
        }
        public object Clone()
        {
            return null;
        }
    }
    #endregion

    /// <summary>
    /// 属性管理器基类,T必须为属性枚举
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseAttrMgr<T> : BaseCoreMgr where T : struct
    {
        #region Callback val
        public Callback Callback_OnAttrChange { get; set; }
        #endregion

        #region prop
        protected List<TDAttrData> attrDataList = new List<TDAttrData>();
        protected Dictionary<string, TDAttrData> attrDataDic = new Dictionary<string, TDAttrData>();
        protected List<AttrConvertData<T>> _attrConvertData = new List<AttrConvertData<T>>();
        protected readonly List<AttrAdditon<T>> _data = new List<AttrAdditon<T>>();
        protected readonly List<AttrAdditon<T>> _percentData = new List<AttrAdditon<T>>();
        protected float[] _baseDataPool;
        protected float[] _curDataPool;
        T defaultType = default(T);
        #endregion

        #region set
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            var tempArray = Enum.GetValues(typeof(T));
            _curDataPool = new float[tempArray.Length];
            _baseDataPool = new float[tempArray.Length];
            attrDataList = GetAttrDataList();
            attrDataDic = GetAttrDataDic();
        }
        /// <summary>
        /// 初始化所有属性
        /// </summary>
        /// <param name="attrs"></param>
        public void InitAllAttr(Dictionary<T, float> attrs)
        {
            if (attrs == null)
                return;

            BaseUtils.ForeachEnum<T>((x) =>
            {
                int index = (int)(object)x;
                if (attrDataList.Count< index)
                {
                    CLog.Error("没有这个属性:{0}",x);
                    return;
                }
                var tempAttrData = attrDataList[index];
                float obj = tempAttrData.Default;
                if (attrs.ContainsKey(x))
                {
                    obj = attrs[x];
                }
                InitAttr(x, obj);
            });
        }
        /// <summary>
        /// 获得所有属性
        /// </summary>
        /// <returns></returns>
        public Dictionary<T, float> GetAllBaseAttr()
        {
            Dictionary<T, float> ret = new Dictionary<T, float>();

            for (int i = 0; i < _baseDataPool.Length; ++i)
            {
                ret.Add((T)(object)(i), _baseDataPool[i]);
            }
            return ret;
        }
        public Dictionary<T, float> GetAllCurAttr()
        {
            Dictionary<T, float> ret = new Dictionary<T, float>();

            for (int i = 0; i < _curDataPool.Length; ++i)
            {
                ret.Add((T)(object)(i), _curDataPool[i]);
            }
            return ret;
        }
        public Dictionary<string, float> GetAllCurAttrStr()
        {
            Dictionary<string, float> ret = new Dictionary<string, float>();

            for (int i = 0; i < _curDataPool.Length; ++i)
            {
                ret.Add(((T)(object)(i)).ToString(), _curDataPool[i]);
            }
            return ret;
        }
        public List<AttrAdditon<T>> Add(List<AttrAdditon<T>> array)
        {
            if (array == null) return null;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    var index = BoxAvoidance<T>.ToInt(array[i].Type);
                    var tempAttrData = attrDataList[index];

                    //固定值,持续变化
                    if (tempAttrData.Type == AttrType.Fixed)
                    {
                        if (array[i].AddType == AttrOpType.Direct ||
                            array[i].AddType == AttrOpType.DirectAdd)
                            _data.Add(array[i]);
                        else
                            _percentData.Add(array[i]);
                    }
                    //动态值一次性变化
                    else if (tempAttrData.Type == AttrType.Dynamic)
                    {
                        if (array[i].AddType == AttrOpType.Direct)
                            InitAttr(array[i].Type, array[i].RealVal);
                        else if (array[i].AddType == AttrOpType.DirectAdd)
                            ChangeVal(array[i].Type, array[i].RealVal);
                        else if (array[i].AddType == AttrOpType.Percent)
                            InitAttr(array[i].Type, array[i].RealVal * _curDataPool[index]);
                        else if (array[i].AddType == AttrOpType.PercentAdd)
                            ChangeVal(array[i].Type, array[i].RealVal * _curDataPool[index]);
                    }
                }
            }
            SetDirty();
            return array;
        }
        public void Remove(List<AttrAdditon<T>> array)
        {
            if (array == null) return;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    if (array[i].AddType == AttrOpType.Direct ||
                        array[i].AddType == AttrOpType.DirectAdd)
                        _data.Remove(array[i]);
                    else
                        _percentData.Remove(array[i]);
                }
            }
            SetDirty();
        }

        public List<AttrConvertData<T>> Add(List<AttrConvertData<T>> array)
        {
            if (array == null) return null;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    array[i].AttrData = attrDataList;
                    _attrConvertData.Add(array[i]);
                }
            }
            SetDirty();
            return array;
        }
        public void Remove(List<AttrConvertData<T>> array)
        {
            if (array == null) return;
            for (var i = 0; i < array.Count; ++i)
            {
                if (array[i] != null)
                {
                    _attrConvertData.Remove(array[i]);
                }
            }
            SetDirty();
        }
        protected virtual void AddAttrVal(T type, float val)
        {
            var index = BoxAvoidance<T>.ToInt(type);
            if (attrDataList != null)
            {
                TDAttrData tempAttrData = null;
                if (index >= attrDataList.Count)
                {
                    CLog.Error("SetAttrVal:" + type.ToString() + ":没有配置属性表");
                    return;
                }
                tempAttrData = attrDataList[index];
                if (tempAttrData == null)
                {
                    _curDataPool[index] += val;
                }
                else
                {
                    _curDataPool[index] += val;
                    _curDataPool[index] = Mathf.Clamp(_curDataPool[index], tempAttrData.Min, tempAttrData.Max);
                }
            }
            else
            {
                _curDataPool[index] += val;
            }
        }
        public virtual void InitAttr(T type, float val)
        {
            var index = BoxAvoidance<T>.ToInt(type);
            if (attrDataList != null)
            {
                TDAttrData tempAttrData = null;
                if (index >= attrDataList.Count)
                {
                    CLog.Error("InitAttr:" + type.ToString() + ":没有配置属性表");
                    return;
                }
                tempAttrData = attrDataList[index];
                if (tempAttrData == null)
                {
                    _curDataPool[index] = val;
                    _baseDataPool[index] = val;
                }
                else
                {
                    _curDataPool[index] = Mathf.Clamp(val, tempAttrData.Min, tempAttrData.Max);
                    _baseDataPool[index] = Mathf.Clamp(val, tempAttrData.Min, tempAttrData.Max);
                }
            }
            else
            {
                _curDataPool[index] = val;
                _baseDataPool[index] = val;
            }
            SetDirty();
        }
        public override void Refresh()
        {
            base.Refresh();
            //重置当前值
            for (int i = 0; i < _baseDataPool.Length; ++i)
            {
                _curDataPool[i] = _baseDataPool[i];
            }
            //计算非百分值
            for (int i = 0; i < _data.Count; ++i)
            {
                AddAttrVal(_data[i].Type, _data[i].RealVal);
            }
            //计算百分值
            for (int i = 0; i < _percentData.Count; ++i)
            {
                AddAttrVal(_percentData[i].Type, _percentData[i].RealVal * _baseDataPool[BoxAvoidance<T>.ToInt(_percentData[i].Type)]);
            }
            //额外加成
            if (_attrConvertData != null)
            {
                foreach (var item in _attrConvertData)
                {
                    if (item != null)
                    {
                        AddAttrVal(item.To, GetExtraAddtion(item));
                    }
                }
            }
            Callback_OnAttrChange?.Invoke();
        }
        public virtual void ChangeVal(T type, float val, float? minVal = null, float? maxVal = null)
        {
            if (val == 0)
                return;
            int index = BoxAvoidance<T>.ToInt(type);
            float baseVal = _baseDataPool[index];
            baseVal += val;
            if (minVal != null)
            {
                if (baseVal < minVal.Value)
                    baseVal = minVal.Value;
            }
            if (maxVal != null)
            {
                if (baseVal > maxVal)
                    baseVal = maxVal.Value;
            }
            InitAttr(type, baseVal);
        }
        /// <summary>
        /// 执行消耗
        /// </summary>
        /// <param name="datas"></param>
        public virtual void DoCost(List<CostData<T>> datas, bool isReverse = false)
        {
            if (datas == null)
                return;
            if (isReverse)
            {
                foreach (var item in datas)
                    ChangeVal(item.Type, item.RealVal);
            }
            else
            {
                foreach (var item in datas)
                    ChangeVal(item.Type, -item.RealVal);
            }
        }
        public virtual void DoCost(CostData<T> datas, bool isReverse = false)
        {
            if (datas == null)
                return;
            if (isReverse)
            {
                ChangeVal(datas.Type, datas.RealVal);
            }
            else
            {
                ChangeVal(datas.Type, -datas.RealVal);
            }
        }
        #endregion

        #region get
        /// <summary>
        /// 是否有消耗
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsHaveCost(CostData<T> data)
        {
            if (data == null)
                return false;
            return GetAttrVal(data.Type) >= data.RealVal;
        }
        /// <summary>
        /// 是否有消耗
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsHaveCost(List<CostData<T>> data)
        {
            if (data == null)
                return false;
            foreach (var item in data)
            {
                if (GetAttrVal(item.Type) < item.RealVal)
                {
                    return false;
                }
            }
            return true;
        }
        public virtual float GetAttrVal(T type)
        {
            return _curDataPool[BoxAvoidance<T>.ToInt(type)];
        }
        public virtual Sprite GetIcon(T type)
        {
            if (attrDataList == null)
                return null;
            return SelfBaseGlobal.GRMgr.GetIcon(attrDataList[BoxAvoidance<T>.ToInt(type)].Icon);
        }
        /// <summary>
        /// 获得额外的加成
        /// </summary>
        protected float GetExtraAddtion(AttrConvertData<T> data, float? customVal = null)
        {
            if (data == null) return 0;
            int fromIndex = BoxAvoidance<T>.ToInt(data.From);
            TDAttrData tempData = attrDataList[fromIndex];
            var fromVal = _curDataPool[fromIndex];
            if (customVal.HasValue)
                fromVal = customVal.Value;
            var toVal = 0.0f;
            var Step = data.Step;
            if (Step == 0)
            {
                CLog.Error("Step不能为0");
                return 0.0f;
            }

            if (!data.IsReverse)
                toVal = (fromVal / Step) * data.Faction;
            else
            {
                toVal = ((tempData.Max - fromVal) / Step) * data.Faction;
            }
            return Mathf.Clamp(toVal, data.Min, data.Max);
        }
        /// <summary>
        /// 得到额外加成的字符窜
        /// customVal:可以使用这个值替代原来的属性值
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public string GetAllConvertStr(T from, string Indent = "\n", float? customVal = null)
        {
            Dictionary<T, float> tempVal = new Dictionary<T, float>();
            string hint = "";
            if (_attrConvertData != null)
            {
                foreach (var item in _attrConvertData)
                {
                    if (item != null && BoxAvoidance<T>.ToInt(item.From) == BoxAvoidance<T>.ToInt(from))
                    {
                        if (!tempVal.ContainsKey(item.To))
                            tempVal.Add(item.To, 0.0f);
                        tempVal[item.To] += GetExtraAddtion(item, customVal);
                    }
                }
            }
            foreach (var item in tempVal)
            {
                hint += Indent + GetAttrStr(item.Key, item.Value);
            }
            return hint;
        }
        /// <summary>
        /// 获得翻译后的描述
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetAttrDesc(T type)
        {
            if (attrDataList == null) return "attrData is null:" + type.ToString();
            var index = BoxAvoidance<T>.ToInt(type);
            if (index >= attrDataList.Count)
                return "out of index:" + type.ToString();
            return attrDataList[index].GetDesc();
        }
        #endregion

        #region Utile
        /// <summary>
        /// 获取带有颜色的加成字符,附带正面效果和负面效果的颜色变化
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Val"></param>
        /// <returns></returns>
        public static string GetAttrStr(T Type, float Val, bool? isPercent = null, bool isIgnoreName = false, bool isColor = true)
        {
            List<TDAttrData> tempAttrData = GetAttrDataList();
            var tempData = tempAttrData[BoxAvoidance<T>.ToInt(Type)];
            string color = GetAttrColor(Type, Val);
            bool tempPercent = false;
            bool tempBool = false;
            //设置输入的百分比
            if (isPercent.HasValue) tempPercent = isPercent.Value;
            //如果是百分比数值则直接使用百分比
            if (tempData.NumberType == NumberType.Percent)tempPercent = true;
            if (tempData.NumberType == NumberType.Bool) tempBool = true;

            //属性名称
            string name = isIgnoreName ? "" : (Type as Enum).GetName();
            //设置属性值
            string strVal = "No";
            if(tempBool)
                strVal = BaseUIUtils.Bool(Val);
            else if (tempPercent)
                strVal = BaseUIUtils.PercentSign(Val);
            else
                strVal = BaseUIUtils.Sign((float)Math.Round(Val, 2));
            //组合
            string str1 = name + strVal;
            //属性颜色
            if (isColor) str1 = color + str1 + "</color>";
            return str1;
        }
        /// <summary>
        /// 加过加成因子获得
        /// </summary>
        /// <param name="val"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static float GetAttrValByFaction(float val, float step, AttrFactionType type)
        {
            if (type == AttrFactionType.Percent)
                return val * (1 + step);
            else if (type == AttrFactionType.Direct)
                return val * (step);
            else if (type == AttrFactionType.None)
                return val;
            return val * (1 + step);
        }
        /// <summary>
        /// 获得完整的属性字符窜
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="RealVal"></param>
        /// <param name="isHaveSign"></param>
        /// <param name="isHaveColor"></param>
        /// <param name="isHeveAttrName"></param>
        /// <returns></returns>
        public static string GetFullAttrString(T Type, float RealVal, bool isHaveSign = false, bool isHaveColor = true, bool isHeveAttrName = true)
        {
            string color = GetAttrColor(Type, RealVal);
            string strVal = GetAttrNumber(Type, RealVal);
            string strSign = "";
            if (isHaveSign)
                strSign = GetAttrSign(RealVal);
            string finalStr = strSign + strVal;
            if (isHeveAttrName)
                finalStr = BaseUIUtils.AttrTypeNameSuffix(finalStr, (Type as Enum));
            string colorFormat = "{0}{1}</color>";
            if (isHaveColor)
                return string.Format(colorFormat, color, finalStr);
            return finalStr;
        }
        /// <summary>
        /// 获取属性配置数据
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static List<TDAttrData> GetAttrDataList()
        {
            List<TDAttrData> tempAttrData = new List<TDAttrData>();
            if (TDAttr<T>.AttrDataList == null)
                return tempAttrData;
            Type tempType = typeof(T);
            if (TDAttr<T>.AttrDataList.ContainsKey(tempType))
                return TDAttr<T>.AttrDataList[tempType];
            return tempAttrData;
        }
        /// <summary>
        /// 获得属性配置数据Dic
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, TDAttrData> GetAttrDataDic()
        {
            Dictionary<string, TDAttrData> tempAttrData = new Dictionary<string, TDAttrData>();
            if (TDAttr<T>.AttrDataDic == null)
                return tempAttrData;
            Type tempType = typeof(T);
            if (TDAttr<T>.AttrDataList.ContainsKey(tempType))
                return TDAttr<T>.AttrDataDic[tempType];
            return tempAttrData;
        }
        /// <summary>
        /// 获得转换后的属性字符窜,比如百分比,KGM
        /// </summary>
        /// <returns></returns>
        public static string GetAttrNumber(T Type, float Val)
        {
            List<TDAttrData> tempAttrData = GetAttrDataList();
            if (tempAttrData != null)
            {
                var tempData = tempAttrData[BoxAvoidance<T>.ToInt(Type)];
                if (tempData.NumberType == NumberType.KMG)
                    return BaseUIUtils.KMG(Val);
                else if (tempData.NumberType == NumberType.Percent)
                    return BaseUIUtils.Percent(Val);
                else if (tempData.NumberType == NumberType.Normal)
                    return BaseUIUtils.OptionalTwoDigit(Val);
                else if (tempData.NumberType == NumberType.Integer)
                    return BaseUIUtils.Round(Val);
                else if (tempData.NumberType == NumberType.Bool)
                    return BaseUIUtils.Bool(Val);
            }
            return Val.ToString();
        }
        /// <summary>
        /// 获得属性的正负符号
        /// </summary>
        /// <returns></returns>
        public static string GetAttrSign(float Val)
        {
            if (Val <= 0)
                return "";
            return "+";
        }
        /// <summary>
        /// 获得属性颜色
        /// </summary>
        /// <returns></returns>
        public static string GetAttrColor(T Type, float Val, bool isReverse = false)
        {
            string color = "<color=yellow>";
            List<TDAttrData> tempAttrData = GetAttrDataList();
            if (tempAttrData != null)
            {
                var tempData = tempAttrData[BoxAvoidance<T>.ToInt(Type)];
                if (tempData.IsForwardBuff)
                {
                    if (Val > 0)
                        color = "<color=green>";
                    else if (Val < 0)
                        color = "<color=red>";
                }
                else
                {
                    if (Val < 0)
                        color = "<color=green>";
                    else if (Val > 0)
                        color = "<color=red>";
                }

                if (isReverse)
                {
                    if (color.Contains("green"))
                    {
                        color.Replace("green", "red");
                    }
                    else if (color.Contains("red"))
                    {
                        color.Replace("red", "green");
                    }
                }
            }
            return color;
        }
        /// <summary>
        /// 根据传入的属性类型,以及值判断是否为正面效果
        /// </summary>
        /// <returns></returns>
        public static bool IsPositive(T Type, float Val)
        {
            bool ret = true;
            List<TDAttrData> tempAttrData = GetAttrDataList();
            if (tempAttrData != null)
            {
                var tempData = tempAttrData[BoxAvoidance<T>.ToInt(Type)];
                if (tempData.IsForwardBuff)
                {
                    if (Val < 0)
                        ret = false;
                    else if (Val > 0)
                        ret = true;
                }
                else
                {
                    if (Val < 0)
                        ret = true;
                    else if (Val > 0)
                        ret = false;
                }

            }
            return ret;
        }
        #endregion

        #region db
        public override void ReadEnd<TDBData>(TDBData data)
        {
            base.ReadEnd(data);
            Refresh();
        }
        #endregion
    }

}