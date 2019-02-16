using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace CYM
{
    public class BaseAchieveMgr<TData> : BaseGFlowMgr, ITableDataMgr<TData> where TData : TDBaseAchieveData, new()
    {
        #region override
        public LuaTDMgr<TData> Table { get { throw new System.NotImplementedException();}}
        public TData CurData { get { throw new System.NotImplementedException(); } set { throw new System.NotImplementedException(); } }
        public Dictionary<string, TData> Data { get { throw new System.NotImplementedException(); } }
        #endregion

        #region prop
        #endregion

        #region set
        /// <summary>
        /// 刷新成就数据
        /// </summary>
        public void RefreshData()
        {
            //_self.PlatformSDKMgr.RefreshAchievements();
            RefreshTryTriger();
        }
        public void RefreshTryTriger()
        {
            //if (_self.GameDifficultyMgr.IsSettedGMMod())
            //    return;
            foreach (var item in Data)
            {
                TryTriger(item.Value);
            }
        }
        /// <summary>
        /// 解锁成就
        /// </summary>
        /// <param name="data"></param>
        public void Triger(TData data)
        {
            if (data == null)
                return;
            data.State = true;
            data.UnlockTime = System.DateTime.Now;
            //_self.PlatformSDKMgr.TriggerAchievement(data);
        }
        public void TryTriger(TData data)
        {
            if (data.State)
                return;
            if (!IsCanTrigger(data))
                return;
            Triger(data);
        }
        /// <summary>
        /// 重置成就
        /// </summary>
        /// <param name="data"></param>
        public void Reset(TData data)
        {
            if (data == null)
                return;
            data.State = false;
            //SelfBaseGlobal.PlatformSDKMgr.ResetAchievement(data);
        }
        #endregion

        #region Get
        /// <summary>
        /// 获得成就
        /// </summary>
        /// <returns></returns>
        public TData Get(string id)
        {
            if (Data.ContainsKey(id))
                return Data[id];
            return null;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否可以触发
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsCanTrigger(TData data)
        {
            if (Data == null) return false;
            SelfBaseGlobal.ACM.Add(true, data.Targets);
            if (!SelfBaseGlobal.ACM.IsTrue())
            {
                return false;
            }
            return true;
        }
        #endregion
    }

}