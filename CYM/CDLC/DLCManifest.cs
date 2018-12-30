//------------------------------------------------------------------------------
// ManifestConfig.cs
// Copyright 2018 2018/5/22 
// Created by CYM on 2018/5/22
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;
using System.IO;

namespace CYM.DLC
{
    [Serializable]
    public class AssetPathData
    {
        //原始Bundle名称,比如原先这个资源是放在Icon下面的,在打包过程中被整理到Shared下面
        public string SourceBundleName;
        //完整路径 eg. Assets/CYMCommon/Plugins/CYM/_Bundle/Icon/Logo/Logo_巴西龟.png
        public string FullPath;
        //文件名称 eg. Logo_巴西龟
        public string FileName;
    }
    [Serializable]
    public class BundleData
    {
        //Bundle所在的路径
        public string BundlePath => Path.Combine(Application.streamingAssetsPath, DLCName);
        //所在DLC的名称 eg. Native
        public string DLCName;
        //Bundle名称
        public string BundleName;
        //资源完整路径
        public List<AssetPathData> AssetFullPaths = new List<AssetPathData>();
    }
    public class DLCManifest 
    {
        [SerializeField]
        //Bundle单位 eg. Icon,Music,Video,Shared的散包
        public List<BundleData> Data = new List<BundleData>();
    }
}