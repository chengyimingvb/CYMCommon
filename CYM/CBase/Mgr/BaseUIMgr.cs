using System.Collections;
using System.Collections.Generic;
using CYM.Pool;
using UnityEngine;

//**********************************************
// Class Name	: CYMPoolManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM.UI
{
    public class BaseUIMgr : BaseGFlowMgr
    {

        #region prop
        public static int NextSortOrder = 0;
        protected int SortOrder = 0;
        /// <summary>
        /// 主界面互斥组
        /// </summary>
        public List<BaseView> MainViewGroup = new List<BaseView>();
        /// <summary>
        /// 根界面:画布
        /// </summary>
        public BaseView RootView { get; private set; }
        protected virtual string RootViewPrefab => "BaseRootView";
        #endregion

        #region Callback Val
        public event Callback Callback_OnCreatedUIViews;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            SortOrder = NextSortOrder;
            NextSortOrder++;
        }
        protected virtual void OnCreateUIView(string viewName="RootView")
        {
            RootView = CreateView<BaseUIView>("BaseRootView");
            RootView.GO.name = viewName;
        }
        private void OnCreateInterUIView()
        {

        }
        public override void OnDestroy()
        {
            DestroyView();
            base.OnDestroy();

        }
        #endregion

        #region 创建
        protected GameObject CreateGO(string path)
        {
            GameObject tempGo = null;
            var tempPrefab = SelfBaseGlobal.GRMgr.GetUI(path);
            if (tempPrefab == null)
            {
                CLog.Error("没有这个prefab:" + path);
                return null;
            }
            tempPrefab.SetActive(true);
            tempGo = Object.Instantiate(tempPrefab);
            return tempGo;
        }
        protected T CreateView<T>(string path) where T:BaseView
        {
            var tempGo = CreateGO(path);
            T tempUI = null;
            if (tempGo != null)
            {
                tempUI = tempGo.GetComponent<T>();
                if (tempUI == null) 
                {
                    CLog.Error("无法获取组建:" + typeof(T).Name + " Error path=" + path);
                    return null;
                }
                tempUI.UIMgr = this;
                tempUI.Attach(RootView, RootView, SelfBaseGlobal);
            }
            //默认第一个创建的为rootView
            if (RootView == null)
            {
                RootView = tempUI ;
                RootView.ViewLevel = ViewLevel.Root;

                RootView.Canvas.sortingOrder = SortOrder;
                GameObject.DontDestroyOnLoad(RootView);
            }
            //其余的为Mainview
            else
            {
                tempUI.ViewLevel = ViewLevel.Main;
                MainViewGroup.Add(tempUI);
            }
            tempUI.Callback_OnOpen += OnOpen;
            tempUI.Callback_OnClose += OnClose;
            return tempUI;
        }

        /// <summary>
        /// 手动调用:在适当的时机创建UI
        /// </summary>
        protected void CreateView()
        {
            OnCreateUIView();
            OnCreateInterUIView();
            Callback_OnCreatedUIViews?.Invoke();
        }
        /// <summary>
        /// 手动调用:销毁UI
        /// </summary>
        protected void DestroyView()
        {
            if (RootView != null)
                RootView.Destroy();
            foreach (var item in MainViewGroup)
                item.Destroy();
            MainViewGroup.Clear();
        }
        #endregion

        #region Callback
        void OnOpen(BaseView view,bool useGroup)
        {
            //UI互斥,相同UI组只能有一个UI被打开
            if (useGroup && view.Group>0)
            {
                for (int i = 0; i < MainViewGroup.Count; ++i)
                {
                    if (MainViewGroup[i] != view && MainViewGroup[i].Group == view.Group && MainViewGroup[i].ViewLevel== view.ViewLevel)
                        MainViewGroup[i].Show(false, null, 0, false);
                }
            }
        }
        void OnClose()
        {

        }

        public string GetInfo()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }

}