//**********************************************
// Discription	：CYMBaseCoreComponent
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
namespace CYM
{
    public static class CYMExtension
    {
        #region str
        public static bool IsInv(this int i)
        {
            return i == BaseConstMgr.InvInt;
        }
        public static bool IsInv(this float f)
        {
            return f == BaseConstMgr.InvFloat;
        }
        public static bool IsNone(this string str)
        {
            if (str == null)
                return true;
            return str == BaseConstMgr.STR_None;
        }
        public static bool IsUnknow(this string str)
        {
            if (str == null)
                return true;
            return str == BaseConstMgr.STR_Unkown;
        }
        public static bool IsInvStr(this string str)
        {
            if (str == null)
                return true;
            return str == BaseConstMgr.STR_Inv || str == BaseConstMgr.STR_None || str == BaseConstMgr.STR_Unkown || str == string.Empty;
        }
        /// <summary>
        /// Wraps a class around a json array so that it can be deserialized by JsonUtility
        /// </summary>
        /// <param name="source"></param>
        /// <param name="topClass"></param>
        /// <returns></returns>
        public static string WrapToClass(this string source, string topClass)
        {
            return string.Format("{{\"{0}\": {1}}}", topClass, source);
        }
        ///<summary>
        /// 移除前缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">前缀字符串</param>
        ///<returns></returns>
        public static string TrimStart(this string val, string str)
        {
            string strRegex = @"^(" + str + ")";
            return Regex.Replace(val, strRegex, "");
        }
        ///<summary>
        /// 移除后缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">后缀字符串</param>
        ///<returns></returns>
        public static string TrimEnd(this string val, string str)
        {
            string strRegex = @"(" + str + ")" + "$";
            return Regex.Replace(val, strRegex, "");
        }
#endregion

        #region cost
        /// <summary>
        /// 获得消耗的字符窜描述
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Separator"></param>
        /// <returns></returns>
        public static string ToString<T>(this List<CostData<T>> data, string Separator = "", string EndSeparator = ",", bool isHaveSign = false) where T : struct
                {
                    string temp = "";
                    if (data == null)
                    {
                        CLog.Error("data is null");
                    }
                    foreach (var item in data)
                    {
                        if (item.RealVal == 0)
                            continue;
                        temp += Separator + item.ToString(isHaveSign, false) + EndSeparator;
                    }
                    return temp.TrimEnd(EndSeparator.ToCharArray());
                }
                #endregion

        #region enum
        /// <summary>
        /// 通过枚举获得翻译的名称
        /// </summary>
        /// <param name="myEnum"></param>
        /// <returns></returns>
        public static string[] GetEnumTransNames(this Type myEnum)
        {
            List<string> ret = new List<string>();
            var array = GetFullEnumArray(myEnum);
            if (array != null)
            {
                foreach (var item in array)
                    ret.Add(BaseLanguageMgr.Get(item));
            }
            return ret.ToArray();
        }
        /// <summary>
        /// 通过全名获得枚举翻译
        /// </summary>
        /// <returns></returns>
        public static string GetName(this Enum myEnum)
        {
            return BaseLanguageMgr.Get(GetFull(myEnum));
        }
        /// <summary>
        /// 获取枚举翻译描述
        /// </summary>
        /// <param name="myEnum"></param>
        /// <returns></returns>
        public static string GetDesc(this Enum myEnum, params string[] objs)
        {
            return BaseLanguageMgr.Get("Desc_" + GetFull(myEnum), objs);
        }
        /// <summary>
        /// 获得枚举的全名
        /// </summary>
        /// <param name="myEnum"></param>
        /// <returns></returns>
        public static string GetFull(this Enum myEnum)
        {
            return string.Format("{0}.{1}", myEnum.GetType().Name, myEnum.ToString());
        }
        /// <summary>
        /// 获得枚举类型的全名数组
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string[] GetFullEnumArray(this Type enumType)
        {
            List<string> list = new List<string>();
            Array temp = System.Enum.GetValues(enumType);
            foreach (var item in temp)
            {
                list.Add(((Enum)item).GetFull());
            }
            return list.ToArray();
        }

        /// <summary>
        /// 获得枚举类型的全名数组
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string[] GetEnumArray(this Type enumType)
        {
            List<string> list = new List<string>();
            Array temp = System.Enum.GetValues(enumType);
            foreach (var item in temp)
            {
                list.Add(((Enum)item).GetName());
            }
            return list.ToArray();
        }
        #endregion

        #region unit
        /// <summary>
        /// 判断单位是否有效
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsValid(this BaseUnit unit)
        {
            if (unit == null)
                return false;
            if (!unit.IsLive)
                return false;
            return true;
        }
        #endregion

        #region vector3
        
        #endregion
    }

}