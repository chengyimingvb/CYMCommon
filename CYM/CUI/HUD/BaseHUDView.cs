using CYM.Pool;
using UnityEngine;
//**********************************************
// Class Name	: CYMUIBaseHUD
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM.UI
{ 
    public class BaseHUDView : BaseUIView
    {
        #region member variable
        protected BasePoolMgr spawnPool;
        #endregion

        #region property

        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            spawnPool = SelfBaseGlobal.PoolMgr;
        }
        #endregion

        #region methon
        public BaseHUDItem Jump(object obj , GameObject prefab,Transform node=null)
        {
            if (prefab == null)
            {
                CLog.Error("没有这个prefab");
                return null;
            }

            if (spawnPool != null&& spawnPool.HUD!=null)
            {
                Transform temp = spawnPool.HUD.SpawnTrans(prefab, null, null, Trans);
                if (temp != null)
                {
                    BaseHUDItem tempText = temp.GetComponent<BaseHUDItem>();
                    if (tempText != null)
                    {
                        tempText.Init(obj, node);
                        tempText.OnLifeOver = OnTextLifeOver;
                    }
                    return tempText;
                }
            }
            return null;
        }
        #endregion

        #region Callback
        void OnTextLifeOver(BaseHUDItem item)
        {
            if (item == null)
                return;
            if (item.GO == null)
                return;
            spawnPool.HUD.Despawn(item.GO);
        }
        #endregion
    }
}