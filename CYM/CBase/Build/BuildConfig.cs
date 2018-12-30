using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CYM
{
    public enum VersionTag
    {
        Preview,//预览版本
        Beta,//贝塔版本
        EA,//尝鲜版本
        Release,//发行版本
        Patch,//补丁版本
    }

    public enum Platform
    {
        Windows,
        Windows64,
        Android,
    }

    public enum Distribution{
        Steam,//Steam平台
        Rail,//腾讯WeGame平台
        Turbo,//多宝平台
        Trial,//试玩平台
        Gaopp,//版署版本
    }

    /// <summary>
    /// 发布类型
    /// </summary>
    public enum BuildType
    {
        Default,//内部默认版本
        Develop,//内部开发版本
        Public,//上传发行版本
    }

    [CreateAssetMenu]
    public class BuildConfig : ScriptableObjectConfig<BuildConfig>
    {
        public override void OnCreated()
        {
            int pathCount = Enum.GetNames(typeof(Distribution)).Length;
            if (Ins.DistributionSetupPaths == null || Ins.DistributionSetupPaths.Length != pathCount)
            {
                Ins.DistributionSetupPaths = new string[pathCount];
            }

            //if (Targets == null)
            //{
            //    Targets = new BuildTargetData[1];
            //    Targets[0] = new BuildTargetData { Architecture = Architecture.x64, Platform = Platform.Windows };
            //}
        }

        public Platform Platform = Platform.Windows64;
        public string[] DistributionSetupPaths;

        public string CurDistributionSetupPath
        {
            set
            {
                DistributionSetupPaths[(int)Distribution] = value;
            }
            get
            {
                string temp = DistributionSetupPaths[(int)Distribution];
                if (temp.IsInvStr())
                    temp = "您可以将安装文件夹拖放至此";
                return temp;
            }
        }

        public string FullVersionName
        {
            get
            {
                return string.Format("{0} {1} {2}", FullName, ToString(), Platform);
            }
        }

        public string DirPath
        {
            get
            {
                if (IsPublic)
                {
                    if (IsTrial)
                    {
                        return Path.Combine(BaseConstMgr.Path_Build, Platform.ToString()) +" "+ Distribution;//xxxx/Windows_x64 Trail
                    }
                    else
                        return Path.Combine(BaseConstMgr.Path_Build, Platform.ToString());//xxxx/Windows_x64
                }
                else
                {
                    return Path.Combine(BaseConstMgr.Path_Build, FullVersionName);//xxxx/BloodyMary v0.0 Preview1 Windows_x64 Steam
                }
            }
        }

        public string ExePath
        {
            get
            {
                return Path.Combine(DirPath, FullName + ".exe");
            }
        }


        #region Inspector
        public string Name = "MainTitle";
        public string SubTitle = "SubTitle";
        public Distribution Distribution;
        public string NameSpace = "~~~";
        public string FullName => Name + SubTitle;
        #endregion

        #region version data
        public int Major;
        public int Minor;
        public int Data;
        public int Suffix = 1;
        public int Prefs = 0;
        public VersionTag Tag;
        public bool IsUnityDevelopmentBuild;
        public bool IgnoreChecker;
        public BuildType BuildType = BuildType.Default;

        public bool IsDevBuild
        {
            get
            {
                return BuildType == BuildType.Develop;
            }
        }
        public bool IsPublic
        {
            get
            {
                return BuildType == BuildType.Public;
            }
        }
        public bool IsTrial
        {
            get
            {
                return Distribution == Distribution.Trial;
            }
        }

        public override string ToString()
        {
            string str = string.Format("v{0}.{1} {2}{3} {4}", Major, Minor, Tag, Suffix, Distribution);
            if (IsDevBuild)
            {
                str += " Dev";
            }
            return str;
        }
        #endregion

        #region Build data
        [System.NonSerialized]
        public string Username;
        [System.NonSerialized]
        public string Password;
        //[SerializeField]
        //public BuildTargetData[] Targets = null;
        public BuildData GetBuildData(Distribution type)
        {
            if (type == Distribution.Steam)
                return new BuildSteamData();
            throw new Exception("GetBuildData:错误的平台!");
        }
        //public int CurTargetIndex;
        //public BuildTargetData CurTarget
        //{
        //    get
        //    {
        //        return BuildConfig.Ins.Targets[CurTargetIndex];
        //    }
        //}
        #endregion
    }


}