using System;
using UnityEngine;
using CYM;
using System.Collections.Generic;
using MoonSharp.Interpreter;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    #region enum
    /// <summary>
    /// 条件类型
    /// </summary>
    public enum ACCType
    {
        And,
        Or,
    }
    /// <summary>
    /// 条件比较
    /// </summary>
    public enum ACCompareType
    {
        /// <summary>
        /// 大于
        /// </summary>
        More,
        /// <summary>
        /// 大于等于
        /// </summary>
        MoreEqual,
        /// <summary>
        /// 小于
        /// </summary>
        Less,
        /// <summary>
        /// 小于等于
        /// </summary>
        LessEqual,
        /// <summary>
        /// 等于
        /// </summary>
        Equal,
        /// <summary>
        /// 拥有
        /// </summary>
        Have,
        /// <summary>
        /// 没有
        /// </summary>
        NotHave,
        /// <summary>
        /// 是
        /// </summary>
        Is,
        /// <summary>
        /// 不是
        /// </summary>
        Not,
    }
    /// <summary>
    /// 数字类型
    /// </summary>
    public enum NumberType
    {
        Normal, //正常显示,选择性2位小数
        Percent,//百分比 e.g. 10%,选择性1位小数
        KMG,    //单位显示 e.g. 1K/1M/1G,数字小于1K取整 
        Integer, //取整
        Bool,//布尔 0:false 1:true
    }
    /// <summary>
    /// 属性操作类型
    /// </summary>
    public enum AttrOpType
    {
        DirectAdd = 0,  //直接累加
        PercentAdd, //百分比累加
        Direct,     //直接赋值
        Percent,    //百分比赋值
    }
    /// <summary>
    /// 属性加成因子使用的类型
    /// </summary>
    public enum AttrFactionType
    {
        None,       //忽略加成因子
        Direct,     //直接赋值
        Percent,    //百分比赋值
    }
    /// <summary>
    /// 属性类型
    /// </summary>
    public enum AttrType
    {
        /// <summary>
        /// 固定值,比如最大的血量
        /// </summary>
        Fixed,
        /// <summary>
        /// 动态值,比如当前的血量
        /// </summary>
        Dynamic,
    }
    #endregion

    /// <summary>
    /// 属性配置，包括默认值,最大值，最小值，图标，转换数值，是否为正向加成,是否为百分比数值
    /// </summary>
    public class TDAttrData : BaseConfig<TDAttrData>
    {
        public AttrType Type { get; set; } = AttrType.Fixed;
        public float Max { get; set; } = float.MaxValue;
        public float Min { get; set; } = float.MinValue;
        public float Default { get; set; } = 0.0f;
        public bool IsForwardBuff { get; set; } = true;
        public NumberType NumberType { get; set; } = NumberType.Normal;
        public bool IsHide { get; set; } = false;
        public string GetMax()
        {
            if (Max == float.MaxValue)
                return BaseConstMgr.STR_Infinite;
            return Max.ToString();
        }
        public string GetMin()
        {
            if (Min == float.MinValue)
                return BaseConstMgr.STR_Infinite;
            return Min.ToString();
        }
        public override Sprite GetIcon()
        {
            var ret = GRMgr.GetIcon(BaseConstMgr.Prefix_Attr+TDID);
            if(ret==null)
                ret = base.GetIcon();
            return ret;
        }
    }

    /// <summary>
    /// 这里的T必须是枚举
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TDAttr<T> : LuaTDMgr<TDAttrData> where T:struct
    {
        public static Dictionary<Type, List<TDAttrData>> AttrDataList = new Dictionary<Type, List<TDAttrData>>();
        public TDAttr() : base()
        {

        }
        public TDAttr(string keyName):base(keyName)
        {

        }
        public override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();

            //添加到全局属性表
            Type type = typeof(T);
            List<TDAttrData> data = new List<TDAttrData>();
            string[] names = Enum.GetNames(type);
            foreach (var item in names)
            {
                var val = Find(item);
                if (val==null || val.TDID.IsInvStr())
                {
                    CLog.Error("没有这个属性:{0}",item);
                    continue;
                }
                data.Add(val);
            }
            AttrDataList.Add(type, data);
        }
    }

}