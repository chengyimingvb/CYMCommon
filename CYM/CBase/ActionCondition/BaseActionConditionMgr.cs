using System.Collections;
using System.Collections.Generic;
using CYM;
using System.IO;
using System;
using UnityEngine.SceneManagement;

namespace CYM
{
    public enum ErrorCodeRet
    {
        InvalidParam,//无效参数
        CostNotEnu,//消耗不足
        Successful,//执行成功
        Faild,//执行失败
    }
    /// <summary>
    /// 基类条件管理器
    /// </summary>
    public class BaseActionConditionMgr : BaseGlobalCoreMgr
    {
        #region prop
        private List<SimpleActionCondition> andSimpleConditions;
        private List<SimpleActionCondition> orSimpleConditions;
        private List<BaseActionCondition> andCacheConditions;
        private List<BaseActionCondition> orCacheConditions;
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            andSimpleConditions = new List<SimpleActionCondition>();
            orSimpleConditions = new List<SimpleActionCondition>();
            andCacheConditions = new List<BaseActionCondition>();
            orCacheConditions = new List<BaseActionCondition>();
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public override void OnStart()
        {
            base.OnStart();
        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
        #endregion

        #region normal
        /// <summary>
        /// 添加条件
        /// </summary>
        /// <param name="isReset"></param>
        /// <param name="accType"></param>
        /// <param name="conditions"></param>
        public void Add(bool isReset, ACCType accType, params BaseActionCondition[] conditions)
        {
            if (isReset)
                Clear();
            if(accType== ACCType.And)
                andCacheConditions.AddRange(conditions);
            else if(accType == ACCType.Or)
                orCacheConditions.AddRange(conditions);
        }
        /// <summary>
        /// 添加简单条件
        /// </summary>
        /// <param name="isReset"></param>
        /// <param name="accType"></param>
        /// <param name="conditions"></param>
        public void Add(bool isReset, ACCType accType, params SimpleActionCondition[] conditions)
        {
            if (isReset)
                Clear();
            if (accType == ACCType.And)
                andSimpleConditions.AddRange(conditions);
            else if (accType == ACCType.Or)
                orSimpleConditions.AddRange(conditions);
        }
        /// <summary>
        /// 添加目标
        /// </summary>
        /// <param name="targets"></param>
        public void Add(bool isReset,List<BaseTarget> targets)
        {
            if (isReset)
                Clear();
            if (targets != null)
            {
                foreach (var item in targets)
                    item.DoCondition();
            }
        }
        /// <summary>
        /// 清空
        /// </summary>
        protected void Clear()
        {
            andSimpleConditions.Clear();
            orSimpleConditions.Clear();
            andCacheConditions.Clear();
            orCacheConditions.Clear();
        }
        #endregion

        #region get
        /// <summary>
        /// 获取条件描述
        /// </summary>
        /// <returns></returns>
        public string GetCacheDesc()
        {
            if (IgnoreCondition)
                return "";
            // 进行一次全面判断
            IsTrue(false, false);
            string mustCondition = "";
            string orCondition = "";
            //必须条件
            string andDesc = "";

            for (int i = 0; i < andCacheConditions.Count; ++i)
            {
                if (!andCacheConditions[i].IsIgnore)
                {
                    andDesc += andCacheConditions[i].GetDesc();
                }
            }
            for (int i = 0; i < andSimpleConditions.Count; ++i)
                andDesc += andSimpleConditions[i].GetDesc();

            if (andDesc != "")
                mustCondition = BaseLanguageMgr.Get("AC_需要满足条件", andDesc);

            //条件之一
            string orDesc = "";

            for (int i = 0; i < orCacheConditions.Count; ++i)
            {
                if (!orCacheConditions[i].IsIgnore)
                {
                    orDesc += orCacheConditions[i].GetDesc();
                }
            }

            if (orSimpleConditions != null)
            {
                for (int i = 0; i < orSimpleConditions.Count; ++i)
                    orDesc += orSimpleConditions[i].GetDesc();
            }
            if (orDesc != "")
                orCondition = BaseLanguageMgr.Get("AC_需要满足以下条件之一", orDesc);
            return mustCondition + orCondition;
        }

        /// <summary>
        /// 获得消耗字符窜
        /// </summary>
        /// <returns></returns>
        public string GetCacheCost()
        {
            if (IgnoreCondition)
                return "";

            string mustCondition = "";
            //必须条件
            string cost = "";
            if (andCacheConditions != null)
            {
                for (int i = 0; i < andCacheConditions.Count; ++i)
                {
                    if (andCacheConditions[i].IsCost)
                        cost += andCacheConditions[i].GetCost() + BaseConstMgr.STR_Append;
                }
                //andCacheConditions = null;
            }
            if (cost != "")
                mustCondition = "<color=yellow>" + BaseLanguageMgr.Get("AC_消耗", cost) + "</color>";
            return mustCondition;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool IgnoreCondition
        {
            get;
            set;
        }
        /// <summary>
        /// 根据所有的条件判断
        /// </summary>
        /// <returns></returns>
        public bool IsTrue(bool isFast = true, bool isReset = true)
        {
            if (IgnoreCondition)
                return true;

            bool isAnd = true;
            bool isAndSimple = true;
            bool isOr = false;
            bool isOrSimple = false;

            if (isReset)
            {
                for (int i = 0; i < andCacheConditions.Count; i++)
                {
                    if (andCacheConditions[i] == null)
                        continue;
                    andCacheConditions[i].Reset();
                }
                for (int i = 0; i < orCacheConditions.Count; i++)
                {
                    if (orCacheConditions[i] == null)
                        continue;
                    orCacheConditions[i].Reset();
                }
            }

            //判断必须的条件
            for (int i = 0; i < andCacheConditions.Count; ++i)
            {
                BaseActionCondition con = andCacheConditions[i];
                //SetParam(con);
                con.DoAction();
                bool ret = con.GetRet();
                if (!ret)
                {
                    isAnd = false;
                    if (isFast)
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < andSimpleConditions.Count; ++i)
            {
                if (!andSimpleConditions[i].isTrue)
                {
                    isAndSimple = false;
                    if (isFast)
                    {
                        return false;
                    }
                }
            }

            //判断多选一的条件           
            for (int i = 0; i < orCacheConditions.Count; ++i)
            {
                BaseActionCondition con = orCacheConditions[i];
                //SetParam(con);
                con.DoAction();
                bool r = con.GetRet();
                if (r)
                {
                    isOr = true;
                    if (isFast)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < orSimpleConditions.Count; ++i)
            {
                if (orSimpleConditions[i].isTrue)
                {
                    isOrSimple = true;
                    if (isFast)
                    {
                        return true;
                    }
                }
            }


            return isAnd && isAndSimple && (orCacheConditions.Count == 0 || isOr) && (orSimpleConditions.Count == 0 || isOrSimple);
        }
        #endregion

    }

}