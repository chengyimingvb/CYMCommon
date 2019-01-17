using System;
using CYM;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace CYM.UI
{
    public class BaseDupplicate : BaseGOProvider
    {
        #region prop
        public GameObject Prefab;
        private int Count;
        /// <summary>
        /// obj1=presenter
        /// obj2=用户自定义数据
        /// </summary>
        Callback<object, object> OnRefresh;
        /// <summary>
        /// obj1=presenter
        /// obj2=用户自定义数据
        /// </summary>
        Callback<object, object> OnFixedRefresh;
        /// <summary>
        /// 获取用户自定义数据 方法
        /// </summary>
        Func<IList<object>> GetCustomDatas;
        /// <summary>
        /// 用户自定义数据缓存
        /// </summary>
        IList<object> CustomDatas=new List<object>();
        /// <summary>
        /// 刷新Layout
        /// </summary>
        Timer RefreshLayoutTimer = new Timer(0.02f);
        #endregion

        #region life
        public override bool NeedUpdate => true;
        protected override void Awake()
        {
            var dupps = GetComponentsInChildren<BaseDupplicate>();
            if (dupps.Length > 1)
            {
                CLog.Error("BaseDupplicate 重叠,无法嵌套使用:{0}",name);
            }

            base.Awake();
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            if (OnRefresh != null)
            {
                CustomDatas = GetCustomDatas?.Invoke();
                if (CustomDatas != null)
                {
                    if (Children.Count != CustomDatas.Count)
                    {
                        CLog.Error("数量不相等 uiduplicate");
                        return;
                    }
                }
                foreach (var item in Children)
                {
                    OnRefresh.Invoke(item, CustomDatas == null ? null : CustomDatas[item.Index]);
                }
                RefreshLayoutTimer.Restart();
            }
        }
        public override void OnFixedUpdate()
        {
            if (!IsShow)
                return;
            if (OnFixedRefresh != null)
            {
                if (CustomDatas != null)
                {
                    if (Children.Count != CustomDatas.Count)
                    {
                        CLog.Error("数量不相等 uiduplicate");
                        return;
                    }
                }
                foreach (var item in Children)
                {
                    OnFixedRefresh.Invoke(item, CustomDatas == null ? null : CustomDatas[item.Index]);
                }
            }
            else
            {
                base.OnFixedUpdate();
            }

            if (RefreshLayoutTimer.CheckOverOnce())
            {
                //刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(RectTrans);
            }
        }
        public void AutoFix()
        {
            RectTransform tempTrans = Prefab.transform as RectTransform;
            var gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();
            if (gridLayoutGroup != null)
            {
                gridLayoutGroup.cellSize = new Vector2(tempTrans.sizeDelta.x, tempTrans.sizeDelta.y);
            }
        }
        #endregion

        #region init
        /// <summary>
        /// 自动刷新
        /// </summary>
        /// TP=控件
        /// TD=控件数据
        /// TCD=用户数据
        /// customDatas=用户数据列表
        /// onRefresh=自定义刷新方法
        /// <returns></returns>
        public virtual TP[] Init<TP, TD, TCD>(Func<IList<object>> getCustomDatas, Callback<object, object> onRefresh) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (getCustomDatas == null)
            {
                CLog.Error("customDatas 为 null");
            }
            GetCustomDatas = getCustomDatas;
            CustomDatas = getCustomDatas();
            Init(new PresenterData());
            InitCount(CustomDatas.Count);
            OnRefresh = onRefresh;
            OnFixedRefresh = null;
            var retData = GetGOs<TP, TD>();
            return retData;
        }
        /// <summary>
        /// 自动刷新
        /// </summary>
        /// TP=控件
        /// TD=控件数据
        /// count=初始化数量
        /// onRefresh=自定义刷新方法
        /// <returns></returns>
        public virtual TP[] Init<TP, TD>(int count, Callback<object, object> onRefresh, Callback<object, object> onFixedRefresh=null, TD pData=null) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (onRefresh == null)
            {
                CLog.Error("onRefresh 为 null");
            }
            Init(new PresenterData());
            InitCount(count);
            OnRefresh = onRefresh;
            OnFixedRefresh = onFixedRefresh;
            //BaseUIView.ActivePresenterUpdate(this);
            var retData = GetGOs<TP, TD>(pData);
            return retData;
        }
        /// <summary>
        /// 自动刷新
        /// </summary>
        /// TP=控件
        /// TD=控件数据
        /// data=控件数据列表
        /// <returns></returns>
        public virtual TP[] Init<TP,TD>(params TD[] data) where TP:Presenter<TD>,new() where TD:PresenterData,new()
        {
            if (data == null) return null;
            Init(new PresenterData());
            InitCount(data.Length);
            OnRefresh = null;
            OnFixedRefresh = null;
            return GetGOs<TP,TD>(data);
        }
        /// <summary>
        /// 自动刷新
        /// </summary>
        /// TP=控件
        /// TD=控件数据
        /// data=控件数据列表
        /// onRefresh=自定义刷新方法
        /// <returns></returns>
        public virtual TP[] Init<TP, TD>(Callback<object, object> onRefresh,Callback<object, object> onFixedRefresh,params TD[] data) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (data == null) return null;
            Init(new PresenterData());
            InitCount(data.Length);
            OnRefresh = onRefresh;
            OnFixedRefresh = onFixedRefresh;
            //BaseUIView.ActivePresenterUpdate(this);
            return GetGOs<TP, TD>(data);
        }
        /// <summary>
        /// 自动刷新
        /// </summary>
        /// TP=控件
        /// TD=控件数据
        /// data=控件数据列表
        /// <returns></returns>
        public virtual TP[] Init<TP, TD>(PresenterData pdata, params TD[] data) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (data == null) return null;
            Init(pdata);
            InitCount(data.Length);
            OnRefresh = null;
            OnFixedRefresh = null;
            return GetGOs<TP, TD>(data);
        }
        /// <summary>
        /// 通过数量初始化
        /// </summary>
        /// <param name="count"></param>
        private void InitCount(int count)
        {
            GOs.Clear();
            for (int i = 0; i < Trans.childCount; ++i)
            {
                Transform temp = Trans.GetChild(i);
                GOs.Add(temp.gameObject);
            }
            if (Prefab == null)
            {
                CLog.Error("{0}: Prefab == null", Path);
                return;
            }
            if (count <= 0)
                CLog.Error("Count <= 0");
            if(Prefab.name.StartsWith(BaseConstMgr.STR_Base))
                CLog.Error($"不能使用基础UI Prefab 初始化:{Prefab.name}");

            //差值
            int subCount = BaseMathUtils.Clamp0(count - GOs.Count);

            //生成剩余的游戏对象
            for (int i = 0; i < subCount; ++i)
            {
                GameObject temp = GameObject.Instantiate(Prefab, this.RectTrans.position, this.RectTrans.rotation) as GameObject;
                (temp.transform as RectTransform).SetParent(this.RectTrans);
                (temp.transform as RectTransform).localScale = Vector3.one;
                GOs.Add(temp);
            }

            //设置数量
            Count = GOs.Count;
        }
        #endregion
    }

}