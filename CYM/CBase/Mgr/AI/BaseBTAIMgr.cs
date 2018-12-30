//**********************************************
// Class Name	: BaseBTAIMgr
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CYM
{
	public class BaseBTAIMgr<TData> : BaseAIMgr,ITableDataMgr<TData> where TData: TDBaseBTData,new()
	{
        #region life
        public override void RealDeath()
        {
            RemoveBT();
            base.RealDeath();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (!IsActiveAI)
                return;
            if (CurData != null)
            {
                CurData.OnUpdate();
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 改变行为树
        /// </summary>
        public override void ChangeBT(string btKey)
        {
            if (btKey.IsInvStr())
                return;
            RemoveBT();
            TData tempData = Table.Find(btKey);
            if (tempData != null)
            {
                CurData = tempData.Copy() as TData;
                CurData.OnBeAdded(SelfBaseUnit);
            }
            else
            {
                CLog.Error("错误,没有这个类型的BT:{0}", btKey);
            }
        }
        /// <summary>
        /// 移除行为树
        /// </summary>
        protected override void RemoveBT()
        {
            if (CurData != null)
                CurData.OnBeRemoved();
            CurData = null;
        }
        /// <summary>
        /// 重置行为树
        /// </summary>
        public void SetTreeDirty()
        {
            CurData.SetTreeDirty();
        }
      
        /// <summary>
        /// 是否拥有行树
        /// </summary>
        /// <returns></returns>
        public override bool IsHaveBT()
        {
            return CurData != null;
        }
        #endregion

        #region must override
        public TData CurData { get; set; }
        /// <summary>
        /// 找到BT
        /// </summary>
        /// <returns></returns>
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                //DataParse->TDBuff
                throw new NotImplementedException("这个函数必须被实现");
            }
        }
        #endregion

    }
}