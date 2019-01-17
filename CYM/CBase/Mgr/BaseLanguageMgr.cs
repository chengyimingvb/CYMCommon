
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using UnityEngine;
using CYM.DLC;
//**********************************************
// Class Name	: CYMBaseLanguage
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    [Serializable]
    public enum LanguageType
    {
        Chinese = 0,
        Traditional = 1,
        English = 2,
        Japanese = 3,
        Spanish = 4,
        Classical = 5,
    }

    public class LanguageData
    {
        public string ID { get; set; }
        public string Chinese { get; set; }
        public string English { get; set; }
        public string Traditional { get; set; }
        public string Japanese { get; set; }
        public string Spanish { get; set; }
        public string Classical { get; set; }
    }

    public class BaseLanguageMgr : BaseGlobalCoreMgr, ILoader
    {
        #region Callback Val
        /// <summary>
        /// 切换语言的时候
        /// </summary>
        public event Callback Callback_OnSwitchLanguage;
        #endregion

        #region Prop
        static public LanguageType LanguageType { get; set; }
        static Dictionary<LanguageType, Dictionary<string, string>> data = new Dictionary<LanguageType, Dictionary<string, string>>();
        static Dictionary<string, string> curDic = new Dictionary<string, string>();
        static Dictionary<string, Func<string>> dynamicDic = new Dictionary<string, Func<string>>();
        static HashSet<string> lanKeys = new HashSet<string>();
        public string this[string key]
        {
            get { return Get(key); }
        }
        BasePlatSDKMgr PlatSDKMgr => SelfBaseGlobal.PlatSDKMgr;
        #endregion

        #region life
        protected override void OnAllLoadEnd()
        {
            base.OnAllLoadEnd();
            if (PlatSDKMgr.IsSuportPlatformLanguage())
            {
                Switch(PlatSDKMgr.GetLanguageType());
            }
        }
        #endregion

        #region property
        public static string Space
        {
            get
            {
                if (LanguageType == LanguageType.English ||
                    LanguageType == LanguageType.Spanish)
                    return " ";
                return "";
            }
        }
        #endregion

        #region get
        /// <summary>
        /// 当前的语言
        /// </summary>
        public LanguageType CurLangType
        {
            get { return LanguageType; }
        }
        /// <summary>
        /// 获得翻译
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isIgnoreTrans"></param>
        /// <returns></returns>
        public static string Get(string key)
        {
            if (curDic == null)
                return BaseConstMgr.STR_Unkown + key;
            if (curDic.Count == 0)
            {
                if (data.ContainsKey(LanguageType))
                    curDic = data[LanguageType];
            }
            if (curDic.ContainsKey(key))
            {
                return curDic[key];
            }
            else if (dynamicDic.ContainsKey(key))
            {
                return dynamicDic[key].Invoke();
            }
            return  key;
        }
        public static string Get(string key, params object[] param)
        {
            return string.Format(Get(key), param);
        }
        #endregion

        #region is
        /// <summary>
        /// 是否包含这个翻译
        /// </summary>
        /// <returns></returns>
        public static bool IsContain(string key)
        {
            if (lanKeys == null)
                return false;
            if (lanKeys.Contains(key))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region set
        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="type"></param>
        public virtual void Switch(LanguageType type)
        {
            LanguageType = type;
            if (data.ContainsKey(type))
            {
                curDic = data[type];
            }
            Callback_OnSwitchLanguage?.Invoke();

            SelfBaseGlobal.SettingsMgr.GetBaseSettings().LanguageType = type;
        }
        public void Next()
        {
            int nextLang = (int)LanguageType;
            nextLang += 1;
            Array arrays = Enum.GetValues(typeof(LanguageType));
            if (nextLang >= arrays.Length)
                nextLang = 0;
            Switch((LanguageType)nextLang);
        }
        public void Prev()
        {
            int preLang = (int)LanguageType;
            preLang -= 1;
            Array arrays = Enum.GetValues(typeof(LanguageType));
            if (preLang < 0)
                preLang = arrays.Length - 1;
            Switch((LanguageType)preLang);
        }

        /// <summary>
        /// 添加语言
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="desc"></param>
        public virtual void Add(LanguageType type, string key, string desc, string fileName = "")
        {
            if (key.StartsWith(BaseConstMgr.Prefix_Notes))
                return;
            if (key.IsInvStr() || desc.IsInvStr())
                return;
            if (!lanKeys.Contains(key))
                lanKeys.Add(key);
            if (!data.ContainsKey(type))
                data.Add(type, new Dictionary<string, string>());
            if (!data[type].ContainsKey(key))
            {
                data[type].Add(key, desc);
            }
            else
            {
                if (Application.isEditor)
                    CLog.Error("错误!重复的组建key:" + key + " 当前语言:" + type.ToString() + "  file Name:" + fileName);
                data[type][key] = desc;
            }
        }
        /// <summary>
        /// 添加动态翻译器
        /// </summary>
        public virtual void AddDynamic(string key, Func<string> func)
        {
            string key1 = BaseConstMgr.Prefix_DynamicTrans + key;
            if (dynamicDic.ContainsKey(key1))
                return;
            if (func == null)
                return;
            dynamicDic.Add(key1, func);
        }
        #endregion

        #region Life
        /// <summary>
        /// 添加动态翻译
        /// </summary>
        protected virtual void OnAddDynamicDic()
        {

        }

        List<LanguageType> LangTypes = new List<LanguageType>();
        public IEnumerator Load()
        {
            foreach (var dlc in DLCAssetMgr.DLCItems.Values)
            {
                string[] fileNames = BaseFileUtils.GetFiles(dlc.LanguagePath, "*.xls", SearchOption.AllDirectories);
                if (fileNames == null)
                    continue;
                foreach (var item in fileNames)
                {
                    LoadLanguageData(item);
                    yield return new WaitForEndOfFrame();
                }
            }

            OnAddDynamicDic();
            yield break;
        }

        void LoadLanguageData(string item)
        {
            HSSFWorkbook dataSet = BaseExcelUtils.ReadExcelNPOI(item);
            if (dataSet == null)
            {
                CLog.Error("无法读取下面文件{0}:", item);
                return;
            }
            //读取每一个Sheet
            for (int i = 0; i < dataSet.NumberOfSheets; ++i)
            {
                ISheet sheet = dataSet.GetSheetAt(i);
                var collect = sheet.GetRowEnumerator();
                //读取每一行
                while (collect.MoveNext())
                {
                    //读取语言类型:中文,英文,繁体等等
                    IRow row = (IRow)collect.Current;
                    if (row.RowNum == 0)
                    {
                        foreach (var cell in row.Cells)
                        {
                            if (cell.ColumnIndex == 0)
                                continue;
                            else
                            {
                                try
                                {
                                    LangTypes.Add((LanguageType)Enum.Parse(typeof(LanguageType), cell.ToString()));
                                }
                                catch (Exception e)
                                {
                                    CLog.Error("解析Excel表格错误:" + e.ToString());
                                }
                            }
                        }
                    }
                    //读取翻译
                    else
                    {
                        string key = "";
                        //读取每一列
                        foreach (var cell in row.Cells)
                        {
                            //读取每一行的key
                            if (cell.ColumnIndex == 0)
                            {
                                key = cell.ToString();
                                if (key.IsInvStr())
                                    continue;
                            }
                            //读取每一行的翻译
                            else
                            {
                                string val = cell.ToString();
                                if (val.IsInvStr())
                                    val = key;
                                Add(LangTypes[cell.ColumnIndex - 1], key, val);
                            }
                        }
                    }
                }
            }
        }

        public string GetLoadInfo()
        {
            return "Load language";
        }
        #endregion
    }

}

