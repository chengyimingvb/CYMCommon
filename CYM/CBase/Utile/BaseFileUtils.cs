﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using CYM;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SharpCompress.Archives.GZip;
using SharpCompress.Writers;
using System.Linq;
using SharpCompress.Archives;
using NPOI.HSSF.UserModel;
using Excel;
namespace CYM
{
    public class BaseFileUtils
    {

        #region normal
        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dir"></param>
        /// <param name="newFileName"></param>
        public static void CopyFileToDir(string file, string dir,string newFileName="")
        {
            string fileName = newFileName==""? Path.GetFileName (file): Path.GetFileName(newFileName);
            File.Copy (file, Path.Combine (dir, fileName), true);
        }

        /// <summary>
        /// 拷贝文件夹
        /// </summary>
        /// <param name="sourceDirPath"></param>
        /// <param name="targetDirPath"></param>
        /// <param name="overrideIfNewer">若为true，则只有源文件较新时覆盖，否则全都覆盖</param>
        ///  <param name="deleteTarget">若为true，先删除目标目录中的文件</param>
        public static void CopyDir(string sourceDirPath, string targetDirPath, bool overrideIfNewer = true, bool deleteTarget = false)
        {
            try
            {
                if(!Directory.Exists(sourceDirPath)){
                    CLog.Error("源文件目录不存在{0}", sourceDirPath);
                    return;
                }

                // 判断目标目录是否存在如果不存在则新建
                if (!System.IO.Directory.Exists(targetDirPath))
                {
                    
                }
                else
                {
                    if(deleteTarget)
                        System.IO.Directory.Delete(targetDirPath,true);
                }
                EnsureDirectory(targetDirPath);
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
                // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
                // string[] fileList = Directory.GetFiles（srcPath）；
                string[] entryList = System.IO.Directory.GetFileSystemEntries(sourceDirPath);
                // 遍历所有的文件和目录
                foreach (string entry in entryList)
                {
                    string targetPath = Path.Combine(targetDirPath, System.IO.Path.GetFileName(entry));
                    // 若是目录
                    if (System.IO.Directory.Exists(entry))
                    {
                        CopyDir(entry, targetPath);
                    }
                    else
                    {
                        if(overrideIfNewer && File.Exists(targetPath)){
                            if(File.GetLastWriteTime(entry) > File.GetLastWriteTime(targetPath)){
                                File.Copy(entry, targetPath, true);
                            }
                        }else{
                            File.Copy(entry, targetPath, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CLog.Error(e.ToString());
            }
        }

        /// <summary>
        /// 快速写文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="finalStr"></param>
        public static void WriteFile(string path,string finalStr)
        {
            //string path = Application.persistentDataPath + "/LanguageCheck.txt";
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            //获得字节数组
            byte[] filedata = System.Text.Encoding.Default.GetBytes(finalStr);
            //开始写入
            fs.Write(filedata, 0, filedata.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();
        }

        /// <summary>
        /// 确保路径
        /// </summary>
        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 获得指定目录下的所有文件
        /// </summary>
        /// <param name="luaPath"></param>
        /// <returns></returns>
        public static string[] GetFiles(string path,string searchParttern, SearchOption searchOption)
        {
            if (path.IsInvStr())
                return null;
            if (!Directory.Exists(path))
                return null;
            return Directory.GetFiles(path, searchParttern, searchOption);
        }

        /// <summary>
        /// 打开这个路径
        /// </summary>
        /// <param name="path"></param>
        public static void OpenExplorer(string path,bool ensureDirectory=false)
        {
            if (ensureDirectory)
                EnsureDirectory(path);
            Process.Start("explorer", path.Replace('/', '\\'));
        }

        /// <summary>
        /// 打开这个文件
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFile(string path)
        {
            Process.Start(path);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeletePath(string path)
        {
            Directory.Delete(path,true);
        }

        /// <summary>  
        /// 绝对路径转相对路径  
        /// </summary>  
        /// <param name="strBasePath">基本路径</param>  
        /// <param name="strFullPath">绝对路径</param>  
        /// <returns>strFullPath相对于strBasePath的相对路径</returns>  
        public static string GetRelativePath(string strBasePath, string strFullPath)
        {
            if (strBasePath == null)
                throw new ArgumentNullException("strBasePath");

            if (strFullPath == null)
                throw new ArgumentNullException("strFullPath");

            strBasePath = Path.GetFullPath(strBasePath);
            strFullPath = Path.GetFullPath(strFullPath);

            var DirectoryPos = new int[strBasePath.Length];
            int nPosCount = 0;

            DirectoryPos[nPosCount] = -1;
            ++nPosCount;

            int nDirectoryPos = 0;
            while (true)
            {
                nDirectoryPos = strBasePath.IndexOf('\\', nDirectoryPos);
                if (nDirectoryPos == -1)
                    break;

                DirectoryPos[nPosCount] = nDirectoryPos;
                ++nPosCount;
                ++nDirectoryPos;
            }

            if (!strBasePath.EndsWith("\\"))
            {
                DirectoryPos[nPosCount] = strBasePath.Length;
                ++nPosCount;
            }

            int nCommon = -1;
            for (int i = 1; i < nPosCount; ++i)
            {
                int nStart = DirectoryPos[i - 1] + 1;
                int nLength = DirectoryPos[i] - nStart;

                if (string.Compare(strBasePath, nStart, strFullPath, nStart, nLength, true) != 0)
                    break;

                nCommon = i;
            }

            if (nCommon == -1)
                return strFullPath;

            var strBuilder = new StringBuilder();
            for (int i = nCommon + 1; i < nPosCount; ++i)
                strBuilder.Append("..\\");

            int nSubStartPos = DirectoryPos[nCommon] + 1;
            if (nSubStartPos < strFullPath.Length)
                strBuilder.Append(strFullPath.Substring(nSubStartPos));

            string strResult = strBuilder.ToString();
            return strResult == string.Empty ? ".\\" : strResult;
        }
        //获得Final文件名称
        public static string GetFinalDirectoryName(string item)
        {
            var vals = item.Replace('\\', '/');
            var temps = vals.Split('/');
            if (temps == null || temps.Length == 0)
            {
                CLog.Error("路径错误:{0}", item);
            }
            return temps[temps.Length - 1];
        }
        #endregion

        #region MD5
        /// <summary>
        /// 从字符串获取MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5HashFromString(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue, bytHash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(str);
            bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToUpper();
        }

        /// <summary>
        /// 从文件中获取MD5
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMD5HashFromFile(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString().ToUpper();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
        #endregion

        #region bin
        public static void SerializeStream(Stream stream, object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
        }

        public static MemoryStream SerializeToMemoryStream(object obj)
        {
            MemoryStream ms = new MemoryStream();
            SerializeStream(ms, obj);
            return ms;
        }

        public static T DeSerializeStream<T>(Stream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            T o = (T)formatter.Deserialize(stream);
            return o;
        }

        public static void SaveBin(string path, object obj)
        {
            using (var stream = File.OpenWrite(path))
            {
                SerializeStream(stream, obj);
            }
        }

        public static T LoadBin<T>(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return DeSerializeStream<T>(stream);
            }
        }
        #endregion

        #region json
        static void SaveFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        static void SaveFile(Stream stream, string content)
        {
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(content);
            writer.Close();
        }

        static string LoadFile(Stream stream)
        {
            using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
            {
                return sr.ReadToEnd();
            }
        }

        static string LoadFile(string path)
        {
            return File.ReadAllText(path);
        }

        public static void SaveJson(string path, object obj, bool prettyPrint = true)
        {
            string dir = Path.GetDirectoryName(path);
            BaseFileUtils.EnsureDirectory(dir);
            SaveFile(path, JsonUtility.ToJson(obj, prettyPrint));
        }

        public static void SaveJson(Stream stream, object obj, bool prettyPrint = true)
        {
            SaveFile(stream, JsonUtility.ToJson(obj, prettyPrint));
        }

        public static void UpdateFile<T>(string path, T def) where T : class
        {
            T d = def;
            if (File.Exists(path))
            {
                d = LoadJson<T>(path, def);
            }
            SaveJson(path, d);
        }

        public static T LoadJson<T>(string path)
        {
            return JsonUtility.FromJson<T>(LoadFile(path));
        }

        public static T LoadJson<T>(Stream stream)
        {
            return JsonUtility.FromJson<T>(LoadFile(stream));
        }

        public static T LoadJson<T>(string path, T def) where T : class
        {
            JsonUtility.FromJsonOverwrite(LoadFile(path), def);
            return def;
        }

        public static T LoadJsonOrDefault<T>(string path, T def = default(T))
        {
            if (File.Exists(path))
            {
                return LoadJson<T>(path);
            }
            else
            {
                return def;
            }
        }
        #endregion

        #region 压缩
        // 字符串会转换成utf8存储
        public static byte[] GZCompressToBytes(string content)
        {
            return GZCompressToBytes(Encoding.UTF8.GetBytes(content));
        }

        // 假定字符串存储是utf8格式
        public static string GZDecompressToString(byte[] data)
        {
            return Encoding.UTF8.GetString(GZDecompressToBytes(data));
        }

        public static byte[] GZCompressToBytes(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return GZCompressToBytes(ms);
            }
        }

        public static byte[] GZCompressToBytes(Stream inStream)
        {
            var archive = GZipArchive.Create();
            archive.AddEntry("content", inStream, false);
            MemoryStream ms = new MemoryStream();
            archive.SaveTo(ms, new WriterOptions(SharpCompress.Common.CompressionType.Deflate));
            return ms.ToArray();
        }

        public static byte[] GZDecompressToBytes(byte[] data)
        {
            using (MemoryStream ms = GZDecompressToMemoryStream(data))
            {
                return ms.ToArray();
            }
        }

        static MemoryStream GZDecompressToMemoryStream(byte[] data)
        {
            using (MemoryStream inMs = new MemoryStream(data))
            {
                var archive = GZipArchive.Open(inMs);
                var entry = archive.Entries.First();
                MemoryStream ms = new MemoryStream();
                entry.WriteTo(ms);
                ms.Position = 0;
                return ms;
            }
        }
        #endregion

        #region hash
        public static string Hash(string input)
        {
            return Hash(Encoding.UTF8.GetBytes(input));
        }

        static string HashToString(byte[] hash)
        {
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static string Hash(byte[] input)
        {
            var hash = (new SHA1Managed()).ComputeHash(input);
            return HashToString(hash);
        }

        public static string HashFile(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            string hash = HashStream(fs);
            fs.Close();
            return hash;
        }

        internal static string HashStream(Stream s)
        {
            return HashToString((new SHA1Managed()).ComputeHash(s));
        }
        #endregion
    }
}