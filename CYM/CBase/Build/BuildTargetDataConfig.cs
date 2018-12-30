
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
namespace CYM
{
    #region Base
    [System.Serializable]
    public class BuildData
    {
        protected BuildConfig BuildConfig { get { return BuildConfig.Ins; } }
        /// <summary>
        /// 开启控制台
        /// </summary>
        /// <param name="cmdPath"></param>
        /// <param name="args"></param>
        protected void StartCmd(string args)
        {
            string path =Path.Combine(BaseConstMgr.Path_Project, CmdPath);
            if (!File.Exists(path))
            {
                CLog.Error(string.Format("steamcmd.exe路径不正确, {0}", path));
            }
            else
            {
                
                Process.Start("cmd.exe", string.Format("/K \"{0} {1}\"", path, args));
            }
        }

        public virtual string CmdPath
        {
            get;
        }

        public virtual void Upload()
        {

        }
        public virtual void UploadTrial()
        {

        }

        public virtual void PostBuilder()
        {

        }
    }
    #endregion

    #region Steam Data
    [System.Serializable]
    public class BuildSteamData : BuildData
    {
        public override string CmdPath
        {
            get
            {
                return Path.Combine("..\\Plat_Steam", "builder\\steamcmd.exe");
            }
        }

        public override void Upload()
        {
            StartCmd(string.Format("+login {0} {1} +run_app_build_http ..\\{2} +quit", BuildConfig.Username, BuildConfig.Password, "scripts\\app_build.vdf"));
        }
        public override void UploadTrial()
        {
            StartCmd(string.Format("+login {0} {1} +run_app_build_http ..\\{2} +quit", BuildConfig.Username, BuildConfig.Password, "scripts\\app_build Trial.vdf"));
        }

        public override void PostBuilder()
        {
            //if (BuildConfig.CurTarget.Architecture == Architecture.x64)
            //{
            //    BaseFileUtils.CopyFileToDir("steam_api64.dll", BuildConfig.DirPath);
            //}
            //else if (BuildConfig.CurTarget.Architecture == Architecture.x86)
            //{
            //    BaseFileUtils.CopyFileToDir("steam_api.dll", BuildConfig.DirPath);
            //}
            //else if (BuildConfig.CurTarget.Architecture == Architecture.Universal)
            //{

            //}
            BaseFileUtils.CopyFileToDir("steam_api64.dll", BuildConfig.DirPath);
            BaseFileUtils.CopyFileToDir("steam_api.dll", BuildConfig.DirPath);
            BaseFileUtils.CopyFileToDir("steam_appid.txt", BuildConfig.DirPath);
        }
    }
    #endregion


    //[System.Serializable]
    //public class BuildTargetData
    //{
    //    public string Name { get { return Platform.ToString() + "_" + Architecture.ToString(); } }//Windows_x64
    //    public Architecture Architecture;
    //    public Platform Platform;
    //}
}