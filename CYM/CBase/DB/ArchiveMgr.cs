using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace CYM
{
    public class ArchiveMgr<T> where T : BaseDBGameData, new()
    {
        //默认刷新一次
        bool _isArchiveListDirty = true;
        List<ArchiveFile<T>> _allArchives = new List<ArchiveFile<T>>();
        List<ArchiveFile<T>> _lastArchives = new List<ArchiveFile<T>>();
        string BasePath;
        public void Init(string path)
        {
            BasePath = path;
            BaseFileUtils.EnsureDirectory(path);
        }

        /// <summary>
        /// 当前存档
        /// </summary>
        public string CurArchives { get; set; }
        public int CurArchiveVersion
        {
            get
            {
                return BuildConfig.Ins.Data;
            }
        }

        /// <summary>
        /// 得到存档
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ArchiveFile<T> GetArchive(string id)
        {
            return _allArchives.Find(ar => ar.Name == id);
        }
        /// <summary>
        /// 得到最后修改的时间
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public DateTime GetLastWriteTime(string name)
        {
            return new FileInfo(Path.Combine(BasePath, name)).LastWriteTime;
        }
        /// <summary>
        /// 获取所有存档
        /// </summary>
        /// <returns></returns>
        public List<ArchiveFile<T>> GetAllArchives(bool isRefresh=false)
        {
            if (isRefresh)
                SetArchiveListDirty();
            if (_isArchiveListDirty)
            {
                RefreshArchiveList();
            }
            return _allArchives;
        }
        /// <summary>
        /// 得到存档路径
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetArchivePath(string name)
        {
            return Path.Combine(BasePath,name + BaseConstMgr.Extention_Save);
        }
        /// <summary>
        /// 得到所有文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string[] GetFiles()
        {
            return BaseFileUtils.GetFiles(BasePath,"*"+BaseConstMgr.Extention_Save,SearchOption.AllDirectories);
        }

        #region set
        /// <summary>
        /// 设置dirty
        /// </summary>
        public void SetArchiveListDirty()
        {
            _isArchiveListDirty = true;
        }
        /// <summary>
        /// 从运行中的游戏保存
        /// </summary>
        public ArchiveFile<T> SaveFromRuntimeData(string ID, T GameData, bool isSetDirty = true,bool isHide=false)
        {
            ArchiveFile<T> archive = new ArchiveFile<T>(ID, CurArchiveVersion,default(DateTime), isHide);
            archive.Save(GameData, GetArchivePath(ID));
            if (isSetDirty)
                SetArchiveListDirty();
            return archive;
        }
        /// <summary>
        /// 刷新存档列表
        /// </summary>
        public void RefreshArchiveList()
        {
            _lastArchives.Clear();
            _lastArchives.AddRange(_allArchives);
            _allArchives.Clear();
            foreach (var file in GetFiles())
            {
                if (Path.GetExtension(file) == BaseConstMgr.Extention_Save)
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    DateTime fileTime = GetLastWriteTime(file);
                    ArchiveFile<T> a = null;
                    // 如果以前就存在这个存档的，而且修改时间符合，则使用以前的
                    a = _lastArchives.Find(ac => ac.Name == name && ac.FileTime == fileTime);
                    if (a == null)
                    {
                        a = LoadArchive(name, false);
                    }
                    else
                    {
                    }
                    _allArchives.Add(a);
                }
            }

            // 按时间排序
            _allArchives.Sort((a1, a2) => -a1.SaveTime.CompareTo(a2.SaveTime));
            _isArchiveListDirty = false;
        }
        /// <summary>
        /// 删除指定存档
        /// </summary>
        /// <param name="ID"></param>
        public void DeleteArchives(string ID)
        {
            if (!IsHaveArchive(ID))
            {
                CLog.Error("没有这个存档,错误id=" + ID);
                return;
            }
            else
            {
                File.Delete(GetArchivePath(ID));
            }
            SetArchiveListDirty();
        }
        /// <summary>
        /// 加载存档
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="isReadContnet"></param>
        /// <returns></returns>
        public ArchiveFile<T> LoadArchive(string ID, bool isReadContnet)
        {
            string path = GetArchivePath(ID);
            ArchiveFile<T> archive = new ArchiveFile<T>(ID, CurArchiveVersion);
            archive.Load(path, ID, CurArchiveVersion, isReadContnet, GetLastWriteTime(path));
            return archive;
        }
        #endregion

        #region is
        /// <summary>
        /// 存档是否可以载入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsArchiveValid(string id)
        {
            ArchiveFile<T> a = GetArchive(id);
            return a != null && a.IsLoadble;
        }
        /// <summary>
        /// 是否存在相同的存档
        /// </summary>
        /// <returns></returns>
        /// 
        public bool IsHaveArchive(string ID)
        {
            return GetArchive(ID) != null;
        }
        public bool IsHaveArchive()
        {
            return _allArchives.Count > 0;
        }
        #endregion
    }

}