//**********************************************
// Class Name	: CYMBaseDynamicScript
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.IO;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using MoonSharp.Interpreter;
using CYM.DLC;
namespace CYM
{
    public class BaseLuaMgr : BaseGlobalCoreMgr, ILoader
    {
        #region callback val
        public event Callback Callback_OnLuaParseEnd;
        public event Callback Callback_OnLuaParseStart;
        #endregion

        #region property
        public static Script Lua { get; protected set; } = new Script();
        public static readonly string CoreLuaString = @"
                    --复制表格所有数据
                    CopyPairs = function(targetTable, sourceTable)
                      for key, val in pairs(sourceTable) do
	                    --if targetTable[key] ~=nil and type(targetTable[key])==""table"" then
		                    --if #val ~=0 then
			                --    targetTable[key]={};
			                --    CYM.CopyPairs(targetTable[key], val);
		                    --else
		                    --	CYM.CopyPairs(targetTable[key], val);
		                    --end
	                    --else
		                    targetTable[key] = val;
	                    --end
                      end
                    end

                    --获得一个新的表格
                    GetNewTable=function()
                        local t = { }
	                    return t;
                    end
        ";
        #endregion

        #region methon
        public object this[string key]
        {
            get { return Lua.Globals[key]; }
            set { Lua.Globals[key] = value; }
        }
        public override void Enable(bool b)
        {
            base.Enable(b);
        }
        public void DoFileByFullPath(string path)
        {
            CLog.Debug(CLog.Tag_Lua, "DoLua=>" + path);
            try
            {
                path = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(path));
                Lua.DoFile(path);
            }
            catch (Exception e)
            {
                CLog.Error( "文件<" + path + ">编译错误" + e.ToString());
            }
        }
        public void DoFullDirection(string path)
        {
            string[] files = Directory.GetFiles(path, "*.txt",SearchOption.AllDirectories);
            foreach (var item in files)
            {
                string newStr = item.Replace('\\', '/');
                DoFileByFullPath(newStr);
            }
        }
        public void DoDirection(string path)
        {
            CLog.Debug(CLog.Tag_Lua, "DoDirection=>" + path);
            string[] fileNames = Directory.GetFiles(path, "*.txt");
            for (var i = 0; i < fileNames.Length; i++)
            {
                string fullPath = fileNames[i];
                DoFileByFullPath(fullPath);
            }
        }
        public virtual void DoString(string luaStr,string fileName="", bool isLogFileName=true)
        {
            if (fileName != "" && isLogFileName) {     
                CLog.Debug (CLog.Tag_Lua, "DoString=>" + fileName);
            }
            try
            {
                Lua.DoString(luaStr);
            }
            catch(Exception e)
            {
                CLog.Error( "文件<" + fileName + ">编译错误" + e.ToString() + "\n");
            }
        }
        public void DoString(string[] luaStrs)
        {
            for (int i = 0; i < luaStrs.Length; ++i)
            {
                DoString(luaStrs[i]);
            }
        }
        public static void AddGlobalAction(string key,Action<DynValue> action)
        {
            Lua.Globals[key] = action;
        }
        #endregion

        #region Life

        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            DoString(CoreLuaString);
            DoString(CustomCoreLuaString());
        }

        protected virtual string CustomCoreLuaString()
        {
            return "";
        }
        #endregion

        #region loader
        public IEnumerator Load()
        {
            Callback_OnLuaParseStart?.Invoke();

            foreach (var dlc in DLCAssetMgr.DLCItems.Values)
            {
                string[] fileNames = BaseFileUtils.GetFiles(dlc.LuaPath, "*.txt", SearchOption.AllDirectories);
                if (fileNames == null)
                    continue;
                LoadLuaData(fileNames);
                yield return new WaitForEndOfFrame();
            }
            Callback_OnLuaParseEnd?.Invoke();
            yield break;
        }
        void LoadLuaData(string[] files)
        {
            for (int i = 0; i < files.Length; ++i)
            {
                DoString(File.ReadAllText(files[i]), files[i]);
                SelfBaseGlobal.LoaderMgr.ExtraLoadInfo = "加载 Lua " + files[i];                
            }
        }
        public string GetLoadInfo()
        {
            return "Load Lua";
        }
        #endregion
    }

}

