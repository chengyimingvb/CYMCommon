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
        public HashList<BaseHUDItem> Data { get; protected set; } = new HashList<BaseHUDItem>();
        #endregion

        #region life
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void Awake()
        {
            base.Awake();
            spawnPool = SelfBaseGlobal.PoolMgr;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            foreach (var item in Data)
            {
                item.OnUpdate();
            }
        }
        #endregion

        #region methon
        public BaseHUDItem Jump(object obj, GameObject prefab, BaseUnit unit, Transform node = null)
        {
            if (prefab == null)
            {
                CLog.Error("没有这个prefab");
                return null;
            }

            if (spawnPool != null && spawnPool.HUD != null)
            {
                Transform temp = spawnPool.HUD.SpawnTrans(prefab, null, null, Trans);
                if (temp != null)
                {
                    BaseHUDItem tempText = temp.GetComponent<BaseHUDItem>();
                    if (tempText != null)
                    {
                        tempText.Init(obj, unit, node);
                        tempText.OnLifeOver = OnTextLifeOver;
                        tempText.BaseUIView = this;
                        Data.Add(tempText);
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
            Data.Remove(item);
        }
        #endregion
    }
}