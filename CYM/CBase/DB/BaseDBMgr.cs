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
using System.IO;
using System.Collections.Generic;

namespace CYM
{
    /// <summary>
    /// 自动存储类型
    /// </summary>
    public enum AutoSaveType
    {
        None,
        Monthly,
        Yearly,
    }
    public class BaseDBMgr<TGameData> : BaseGFlowMgr, IBaseDBMgr where TGameData:BaseDBGameData, new()
    {
        #region 存档
        public ArchiveMgr<TGameData> CurArchiveMgr;
        public ArchiveMgr<TGameData> LocalArchiveMgr=new ArchiveMgr<TGameData>();
        public ArchiveMgr<TGameData> RemoteArchiveMgr = new ArchiveMgr<TGameData>();
        #endregion

        #region prop
        /// <summary>
        /// 当前游戏存储的数据
        /// </summary>
        public TGameData CurGameData { get; set; } = new TGameData();
        /// <summary>
        /// 是有拥有snapshot
        /// </summary>
        public bool HasSnapshot { get; protected set; } = false;
        /// <summary>
        /// 开始自动存档
        /// </summary>
        public event Callback<AutoSaveType> Callback_OnStartAutoSave;
        /// <summary>
        /// 结束自动存档
        /// </summary>
        public event Callback<AutoSaveType> Callback_OnEndAutoSave;
        /// <summary>
        /// 游戏存储
        /// </summary>
        public event Callback<ArchiveFile<TGameData>> Callback_OnSaveGame;
        /// <summary>
        /// 玩的时间
        /// </summary>
        public int PlayTime { get; protected set; } = 0;
        #endregion

        #region mgr
        IBaseSettingsMgr SettingsMgr => SelfBaseGlobal.SettingsMgr;
        BasePrefsMgr PrefsMgr => SelfBaseGlobal.PrefsMgr;
        #endregion

        #region 生命周期
        public override void OnEnable()
        {
            base.OnEnable();
            try
            {
                if (LocalArchiveMgr != null)
                {
                    LocalArchiveMgr.Init(GetLocalArchivePath());
                }

                if (RemoteArchiveMgr != null)
                {
                    RemoteArchiveMgr.Init(GetCloudArchivePath());
                }
            }
            catch (Exception e)
            {
                if (e != null)
                    CLog.Error(e.ToString());
            }
            //UseRemoteArchives(false);
            UseRemoteArchives(!PrefsMgr.GetLastAchiveLocal());
        }
        public override void OnStart()
        {
            base.OnStart();
            LocalArchiveMgr.RefreshArchiveList();
            RemoteArchiveMgr.RefreshArchiveList();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        public override void GameLogicTurn()
        {
            base.GameLogicTurn();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否存在当前的存档
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool IsHaveSameArchives(string ID)
        {
            return CurArchiveMgr.IsHaveArchive(ID);
        }
        /// <summary>
        /// 是否有游戏数据
        /// </summary>
        /// <returns></returns>
        public bool IsHaveGameData()
        {
            return CurGameData != null;
        }
        /// <summary>
        /// 是否可以使用云存档
        /// </summary>
        /// <returns></returns>
        public bool IsCanUseRemoteArchives()
        {
            return SelfBaseGlobal.PlatSDKMgr.IsSuportCloudArchive();
        }
        /// <summary>
        /// 是否为本地存档
        /// </summary>
        /// <returns></returns>
        public bool IsCurArchivesLocal()
        {
            return CurArchiveMgr == LocalArchiveMgr;
        }
        /// <summary>
        /// 是否可以继续游戏
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCanContinueGame()
        {
            string id = PrefsMgr.GetLastAchiveID();
            var tempAchive = GetAchieveMgr(PrefsMgr.GetLastAchiveLocal());
            return tempAchive.IsHaveArchive(id) && tempAchive.IsArchiveValid(id);
        }
        #endregion

        #region Get
        /// <summary>
        /// 获得基础数据
        /// </summary>
        /// <returns></returns>
        public BaseDBGameData CurBaseGameData => CurGameData;
        /// <summary>
        /// 获得默认的存档名称
        /// </summary>
        /// <returns></returns>
        public string GetDefaultSaveName()
        {
            return string.Format($"{SelfBaseGlobal.VersionMgr.Config.Name}{DateTime.Today.Year}{DateTime.Today.Month}{DateTime.Today.Day}{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}");
        }
        /// <summary>
        /// 获得默认的自动存档名称
        /// </summary>
        /// <returns></returns>
        public string GetDefaultAutoSaveName()
        {
            return BaseConstMgr.Prefix_AutoSave + GetDefaultSaveName();
        }
        /// <summary>
        /// 获取所有的存档
        /// </summary>
        /// <returns></returns>
        public List<ArchiveFile<TGameData>> GetAllArchives(bool isRefresh=false)
        {
            return CurArchiveMgr.GetAllArchives(isRefresh);
        }
        /// <summary>
        /// 云存档的路径
        /// </summary>
        /// <returns></returns>
        public virtual string GetCloudArchivePath()
        {
            return BaseConstMgr.Path_CloudDB;
        }
        /// <summary>
        /// 本地存档路劲
        /// </summary>
        /// <returns></returns>
        public virtual string GetLocalArchivePath()
        {
            return BaseConstMgr.Path_LocalDB;
        }
        public ArchiveMgr<TGameData> GetAchieveMgr(bool isLocal=true)
        {
            if (isLocal)
                return LocalArchiveMgr;
            return RemoteArchiveMgr;
        }
        #endregion

        #region Set
        /// <summary>
        /// 设置使用远程云存档
        /// </summary>
        /// <param name="isUse"></param>
        public void UseRemoteArchives(bool isUse)
        {
            if (IsCanUseRemoteArchives() && isUse)
            {
                CurArchiveMgr = RemoteArchiveMgr;
            }
            else
            {
                CurArchiveMgr = LocalArchiveMgr;
            }
        }
        /// <summary>
        /// 加载游戏
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public BaseDBGameData LoadGame(string ID)
        {
            var archive = CurArchiveMgr.LoadArchive(ID, true);
            CurGameData = archive.GameDatas;
            return CurGameData;
        }
        /// <summary>
        /// 另存当前游戏
        /// isSetDirty=true 刷新存储文件(会卡) ,否则不刷新,比如自动存储的时候不需要刷新
        /// isSnapshot=true 通过最近的一次快照存储游戏
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public virtual TGameData SaveCurGameAs(string ID,bool isSnapshot=false ,bool isSetDirty = true,bool isHide=false,bool forceLocalArchive=false)
        {
            //保存
            if (ID != BaseConstMgr.DBTempSaveName)
            {
                PrefsMgr.SetLastAchiveID(ID);
                PrefsMgr.SetLastAchiveLocal(IsCurArchivesLocal());
                SettingsMgr.Save();
            }

            ArchiveFile<TGameData> archiveFile;
            if (isSnapshot)
            {
                //使用最近的一次快照
                if (!HasSnapshot)
                {
                    throw new NotImplementedException("最近一次没有快照,请手动调用Sanpshot");
                }
            }
            else
            {
                //临时快照
                Snapshot(false);
            }

            if (forceLocalArchive)
                archiveFile = LocalArchiveMgr.SaveFromRuntimeData(ID, CurGameData, isSetDirty, isHide);
            else
                archiveFile = CurArchiveMgr.SaveFromRuntimeData(ID, CurGameData, isSetDirty, isHide);

            Callback_OnSaveGame?.Invoke(archiveFile);
            return CurGameData;
        }
        /// <summary>
        /// 自动存储
        /// </summary>
        /// <param name="isSnapshot"></param>
        /// <returns></returns>
        public TGameData AutoSave(bool isSnapshot = false)
        {
            Callback_OnStartAutoSave?.Invoke(AutoSaveType.None);
            var ret = SaveCurGameAs(BaseConstMgr.Prefix_AutoSave+"Last",  isSnapshot , false , false);
            Callback_OnEndAutoSave?.Invoke(AutoSaveType.None);
            return ret;
        }
        /// <summary>
        /// 快照
        /// isSnapshot=true 设置快照标记,否则表示临时快照表示内部使用
        /// </summary>
        /// <returns></returns>
        public TGameData Snapshot(bool isSnapshot = true)
        {
            CurGameData = new TGameData();
            WriteGameDBData();
            HasSnapshot = isSnapshot;
            return CurGameData;
        }
        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param name="ID"></param>
        public void DeleteArchives(string ID)
        {
            CurArchiveMgr.DeleteArchives(ID);
        }
        /// <summary>
        /// 开始新游戏
        /// </summary>
        /// <returns></returns>
        public BaseDBGameData StartNewGame()
        {
            CurGameData = GenerateNewGameData();
            return CurGameData;
        }
        /// <summary>
        /// 创建自定义存档
        /// </summary>
        /// <returns></returns>
        protected virtual TGameData GenerateNewGameData()
        {
            return new TGameData();
        }
        #endregion

        #region DB
        public override void Read1<TDBData>(TDBData data)
        {
            base.Read1(data);
            PlayTime = data.PlayTime;
        }
        public override void Write<TDBData>(TDBData data)
        {
            base.Write(data);
            data.PlayTime = PlayTime+(int)Time.realtimeSinceStartup;
        }
        #endregion

        #region DB All
        /// <summary>
        /// 统一读取:手动调用
        /// </summary>
        public void ReadGameDBData()
        {
            var data = CurGameData;
            SelfBaseGlobal.Read1(data);
            SelfBaseGlobal.Read2(data);
            SelfBaseGlobal.Read3(data);
            SelfBaseGlobal.ReadEnd(data);
        }
        /// <summary>
        /// 统一写入:手动调用
        /// </summary>
        public void WriteGameDBData()
        {
            var data = CurGameData;
            SelfBaseGlobal.Write(data);
        }
        #endregion
    }

}