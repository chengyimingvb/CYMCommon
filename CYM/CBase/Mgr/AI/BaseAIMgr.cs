using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace CYM
{
    public class BaseAIMgr : BaseCoreMgr
    {
        #region prop
        /// <summary>
        /// 自定义,禁止AI标志
        /// </summary>
        public virtual bool IsEnableAI { get; protected set; } = true;
        #endregion

        #region life
        public override void Birth()
        {
            base.Birth();
            EnableAI(true);
        }
        public override void Death(BaseUnit caster)
        {
            base.Death(caster);
            EnableAI(false);
        }
        #endregion

        #region is
        /// <summary>
        /// AI 总控开关
        /// </summary>
        public virtual bool IsActiveAI
        {
            get
            {
                return IsEnableAI;
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 开启AI
        /// </summary>
        /// <param name="b"></param>
        public void EnableAI(bool b)
        {
            IsEnableAI = b;
        }
        /// <summary>
        /// 改变行为树
        /// </summary>
        public virtual void ChangeBT(string btKey)
        {
        }
        /// <summary>
        /// 移除行为树
        /// </summary>
        protected virtual void RemoveBT()
        {
        }
        /// <summary>
        /// 是否拥有行树
        /// </summary>
        /// <returns></returns>
        public virtual bool IsHaveBT()
        {
            return false;
        }
        #endregion

    }

}