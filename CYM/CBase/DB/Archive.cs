//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using CYM;
using UnityEngine;
//using iBoxDB.LocalServer;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace CYM
{
    [System.Serializable]
    // 存档文件头，用于存储少量信息
    public class ArchiveHeader
    {
        // 用来测试兼容性
        public int Version;
        // 游戏时间
        public int PlayTime;
        public int ContentLength;
        // 游戏日期，ticks
        public long SaveTimeTicks;
        // hash,用来检测文件是否在传输过程中出错或者被意外修改
        public string ContentHash;
        // 内容是否为压缩，格式为GZ
        public bool IsCompressed;
        //是否为隐藏
        public bool IsHide=false;

        public DateTime SaveTime
        {
            get
            {
                return new DateTime(SaveTimeTicks);
            }
        }

        public ArchiveHeader()
        {
            Version = -1;
            PlayTime = 0;
        }
    }

    // 游戏存档
    public class ArchiveFile<T> where T:BaseDBGameData
    {
        public string Name { get; private set; }
        public DateTime SaveTime { get { return Header.SaveTime; } }
        public bool IsBroken { get; private set; }
        public ArchiveHeader Header { get; private set; }
        // 当存档载入仅读取文件头时，GameDatas为空
        public T GameDatas { get; private set; }
        byte[] Content;
        int _curVersion;
        bool _isHide=false;
        // FileTime用于快速发现文件是否没变
        public DateTime FileTime { get; private set; }


        // 未损坏且版本为最新
        // 则认为可以读取
        public bool IsLoadble
        {
            get
            {
                return !IsBroken && IsCompatible;
            }
        }

        // 存档版本是否兼容
        public bool IsCompatible
        {
            get
            {
                return Header.Version == _curVersion;
            }
        }

        public TimeSpan PlayTime
        {
            get
            {
                return new TimeSpan(0, 0, Header.PlayTime);
            }
        }

        public ArchiveFile(string name, int curVersion, DateTime fileTime = new DateTime(),bool isHide=false)
        {
            _isHide = isHide;
            _curVersion = curVersion;
            Name = name;
            FileTime = fileTime;
            Header = new ArchiveHeader();
        }

        /// <summary>
        /// 载入存档
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="name"></param>
        /// <param name="curVersion"></param>
        /// <param name="isReadContent"></param>
        /// <param name="fileTime"></param>
        /// <returns></returns>
        public void Load(string path, string name, int curVersion, bool isReadContent, DateTime fileTime)
        {
            ArchiveFile<T> archive = this;
            archive.Name = name;
            archive._curVersion = curVersion;
            archive.FileTime = fileTime;
            using (Stream stream = File.OpenRead(path))
            {
                try
                {
                    BinaryReader reader = new BinaryReader(stream);
                    string headerStr = null;
                    //使用try防止无效的存档
                    headerStr = reader.ReadString();
                    if (string.IsNullOrEmpty(headerStr))
                    {
                        archive.IsBroken = true;
                    }
                    else
                    {
                        archive.Header = JsonUtility.FromJson<ArchiveHeader>(headerStr);
                        int contentSize = archive.Header.ContentLength;
                        if (contentSize <= 0)
                        {
                            archive.IsBroken = true;
                        }
                        else
                        {
                            archive.Content = reader.ReadBytes(contentSize);
                            if (!string.IsNullOrEmpty(archive.Header.ContentHash))
                            {
                                // 内容损坏
                                if (archive.Header.ContentHash != BaseFileUtils.Hash(archive.Content))
                                {
                                    archive.IsBroken = true;
                                    return ;
                                }
                            }
                            if (isReadContent && archive.IsCompatible && contentSize > 0)
                            {
                                byte[] toBeDeserialized = null;
                                if (archive.Header.IsCompressed)
                                {
                                    toBeDeserialized = BaseFileUtils.GZDecompressToBytes(archive.Content);
                                }
                                else
                                {
                                    toBeDeserialized = archive.Content;
                                }
                                using (MemoryStream ms = new MemoryStream(toBeDeserialized))
                                {
                                    archive.GameDatas = BaseFileUtils.DeSerializeStream<T>(ms);
                                }
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception e)
                {
                    archive.IsBroken = true;
                    CLog.Error("读取存档{0}时出现异常:{1}, 因此认为是损坏的存档。", archive.Name, e.Message);
                }
            }
            return;
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="stream"></param>
        public void Save(T datas, string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Create))
            {
                if (datas == null)
                {
                    throw new System.ArgumentNullException("datas");
                }
                GameDatas = datas;
                using (MemoryStream ms = BaseFileUtils.SerializeToMemoryStream(GameDatas))
                {
                    byte[] uncompressed = ms.ToArray();
                    Content = BaseFileUtils.GZCompressToBytes(uncompressed);
                    Header.PlayTime = GameDatas.PlayTime;
                    Header.Version = _curVersion;
                    Header.IsHide = _isHide;
                    Header.ContentLength = Content.Length;
                    Header.SaveTimeTicks = DateTime.Now.Ticks;
                    Header.ContentHash = BaseFileUtils.Hash(Content);
                    Header.IsCompressed = true;
                    string headerStr = JsonUtility.ToJson(Header);

                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(headerStr);
                        writer.Write(Content);
                        writer.Close();
                    }
                }
                stream.Close();
            }
        }

    }

}