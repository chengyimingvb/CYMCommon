using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//**********************************************
// Class Name	: CYMTriggerManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public class BaseTriggerMgr <T>:BaseCoreMgr
    {
        #region member variable
        protected Dictionary<T, List<Callback>> triggerDic0 = new Dictionary<T, List<Callback>>();
        //protected Dictionary<int, Callback<object>> triggerDic1 = new Dictionary<int, Callback<object>>();
        #endregion

        #region property

        #endregion

        #region methon
        public void Register(T type,Callback callback)
        {
            if (!triggerDic0.ContainsKey(type))
                triggerDic0.Add(type, new List<Callback>());
            triggerDic0[type].Add(callback);
        }
        public void UnRegister(T type, Callback callback)
        {
            if (triggerDic0.ContainsKey(type))
                triggerDic0[type].Remove(callback);
        }
        public void Dispath(T type)
        {
            if (triggerDic0 == null)
                return;
            if (!triggerDic0.ContainsKey(type))
                return;
            List<Callback> temp = triggerDic0[type];
            for (int i = 0; i < temp.Count; ++i)
                temp[i]();
        }
        #endregion
    }
}