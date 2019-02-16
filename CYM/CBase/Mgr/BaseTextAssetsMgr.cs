using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace CYM
{
    public class BaseTextAssetsMgr : BaseGFlowMgr, ILoader
    {
        public Dictionary<string, string> Data { get;protected set; } = new Dictionary<string, string>();
        #region loader
        public IEnumerator Load()
        {
            //Callback_OnLuaParseStart?.Invoke();
            //string[] files = BaseFileUtils.GetFiles(BaseConstansMgr.Path_LocalTextAssets, "*.txt", SearchOption.AllDirectories);
            //if (files != null)
            //{
            //    for (int i = 0; i < files.Length; ++i)
            //    {
            //        Data.Add(Path.GetFileNameWithoutExtension(files[i]), File.ReadAllText(files[i]));
            //        SelfBaseGlobal.LoaderMgr.ExtraLoadInfo = "加载 Lua " + files[i];
            //        yield return new WaitForEndOfFrame();
            //    }
            //}
            //Callback_OnLuaParseEnd?.Invoke();
            yield break;
        }
        public string GetLoadInfo()
        {
            return "Load TextAssets";
        }
        #endregion

        #region get
        /// <summary>
        /// 获得text
        /// </summary>
        /// <returns></returns>
        public string GetText(string id)
        {
            if (!Data.ContainsKey(id))
                return "";
            return Data[id];
        }
        #endregion
    }

}