//------------------------------------------------------------------------------
// BasePrefsMgr.cs
// Copyright 2018 2018/8/2 
// Created by CYM on 2018/8/2
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BasePrefsMgr : BaseCoreMgr
    {
        #region key
        public const string Key_LastAchiveID = "LastAchiveID";
        public const string Key_LastAchiveLocal = "LastAchiveLocal";
        public const string Key_LastPrefsVer = "LastPrefsVer";
        #endregion

        #region prop
        BaseVersionMgr VersionMgr => SelfBaseGlobal.VersionMgr;
        #endregion

        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            if (GetInt(Key_LastPrefsVer) != VersionMgr.Config.Prefs)
            {
                DeleteAll();
                SetInt(Key_LastPrefsVer, VersionMgr.Config.Prefs);
            }
        }

        #region set
        public void SetStr(string key,string data)
        {
            PlayerPrefs.SetString(key,data);
        }
        public void SetInt(string key,int data)
        {
            PlayerPrefs.SetInt(key,data);
        }
        public void SetFloat(string key ,float data)
        {
            PlayerPrefs.SetFloat(key,data);
        }
        public void SetBool(string key,bool data)
        {
            PlayerPrefs.SetInt(key,data?1:0);
        }
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
        #endregion

        #region get
        public string GetStr(string key,string defaultVal="")
        {
            return PlayerPrefs.GetString(key, defaultVal);
        }
        public int GetInt(string key,int defaultVal=0)
        {
            return PlayerPrefs.GetInt(key, defaultVal);
        }
        public float GetFloat(string key,float defaultVal=0.0f)
        {
            return PlayerPrefs.GetFloat(key, defaultVal);
        }
        public bool GetBool(string key,bool defaultVal=false)
        {
            var temp = defaultVal? 1 : 0;
            return PlayerPrefs.GetInt(key, temp) ==0?false:true;
        }
        #endregion

        #region set
        public void SetLastAchiveID(string id)
        {
            SetStr(Key_LastAchiveID, id);
        }
        public void SetLastAchiveLocal(bool b)
        {
            SetBool(Key_LastAchiveLocal, b);
        }
        #endregion

        #region get
        public string GetLastAchiveID()
        {
            return GetStr(Key_LastAchiveID, "");
        }
        public bool GetLastAchiveLocal()
        {
            return GetBool(Key_LastAchiveLocal, true);
        }
        #endregion
    }
}