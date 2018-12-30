//**********************************************
// Class Name	: TDBaseBT
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;
using CYM.AI;
namespace CYM
{
    public class TDBaseBTData : BaseConfig<TDBaseBTData>
    {
        #region 属性
        public CYM.AI.Tree Tree { get; set; }
        bool IsDirtyReset=false;
        #endregion

        #region 生命周期
        protected virtual void InitParam()
        {

        }
        public override void OnBeAdded(BaseCoreMono mono, params object[] obj)
        {
            base.OnBeAdded(mono, obj);
            InitParam();
            if (Tree == null)
            {
                RebiuldTree();
            }
            else
            {
                CLog.Error("没有移除Tree");
            }
        }
        public override void OnBeRemoved()
        {
            base.OnBeRemoved();
            Tree = null;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            Tree.Update();
            if (CheckNeedResetTree())
            {
                SetTreeDirty();
            }
            if (IsDirtyReset)
            {
                Tree.Reset();
                IsDirtyReset = false;
            }
        }
        #endregion

        #region set
        public void RebiuldTree()
        {
            Tree = new AI.Tree(CreateTreeNode());
        }
        public void ResetTree()
        {
            Tree.Reset();
        }
        public void SetTreeDirty()
        {
            IsDirtyReset = true;
        }
        protected virtual Node CreateTreeNode()
        {
            return null;
        }
        #endregion

        #region is
        protected virtual bool CheckNeedResetTree()
        {
            return false;
        }
        #endregion
    }
}