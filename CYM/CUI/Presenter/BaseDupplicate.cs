using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BaseDupplicate : Presenter<PresenterData>
    {
        #region Inspector
        [FoldoutGroup("Data"), SerializeField]
        bool IsAutoInit = false;
        [FoldoutGroup("Data"),ShowIf("Inspector_IsInitCount"), SerializeField]
        int Count = 0;
        [FoldoutGroup("Data"), SerializeField]
        bool IsToggleGroup = false;
        [FoldoutGroup("Data"), SerializeField]
        GameObject Prefab;
        #endregion

        #region prop
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
        IList<object> CustomDatas = new List<object>();
        /// <summary>
        /// 刷新Layout
        /// </summary>
        Timer RefreshLayoutTimer = new Timer(0.02f);
        /// <summary>
        /// int1 =当前的index
        /// int2 =上次的index
        /// </summary>
        Callback<int, int> Callback_OnSelectChange;
        /// <summary>
        /// 当前选得index
        /// </summary>
        Callback<int> Callback_OnClickSelected;
        public List<GameObject> GOs { get; set; } = new List<GameObject>();
        public List<BasePresenter> Presenters { get; set; } = new List<BasePresenter>();
        List<BaseCheckBox> ToggleGroupCheckBoxs { get; set; } = new List<BaseCheckBox>();
        /// <summary>
        /// 子对象数量
        /// </summary>
        public int GOCount => GOs.Count;
        /// <summary>
        /// 当前选择的index
        /// </summary>
        public int CurSelectIndex { get; protected set; } = 0;
        /// <summary>
        /// 上一次的选择
        /// </summary>
        public int PreSelectIndex { get; protected set; } = 0;
        bool IsInitedCount = false;
        #endregion

        #region life
        public override bool NeedUpdate => true;
        protected override void Awake()
        {
            var dupps = GetComponentsInChildren<BaseDupplicate>();
            if (dupps.Length > 1)
            {
                CLog.Error("BaseDupplicate 重叠,无法嵌套使用:{0}", name);
            }

            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
            if (IsAutoInit)
            {
                InitCount<BaseEmptyPresenter, PresenterData>(Count);
            }
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
            Callback_OnSelectChange?.Invoke(CurSelectIndex, PreSelectIndex);
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
        [Button("自动适应")]
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
            InitCount<TP, TD>(CustomDatas.Count);
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
        public virtual TP[] Init<TP, TD>(int count, Callback<object, object> onRefresh=null, Callback<object, object> onFixedRefresh = null, TD pData = null) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            Init(new PresenterData());
            InitCount<TP, TD>(count);
            OnRefresh = onRefresh;
            OnFixedRefresh = onFixedRefresh;
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
        public virtual TP[] Init<TP, TD>(params TD[] data) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (data == null) return null;
            Init(new PresenterData());
            InitCount<TP, TD>(data.Length);
            OnRefresh = null;
            OnFixedRefresh = null;
            return GetGOs<TP, TD>(data);
        }
        /// <summary>
        /// 自动刷新
        /// </summary>
        /// TP=控件
        /// TD=控件数据
        /// data=控件数据列表
        /// onRefresh=自定义刷新方法
        /// <returns></returns>
        public virtual TP[] Init<TP, TD>(Callback<object, object> onRefresh, Callback<object, object> onFixedRefresh, params TD[] data) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (data == null) return null;
            Init(new PresenterData());
            InitCount<TP, TD>(data.Length);
            OnRefresh = onRefresh;
            OnFixedRefresh = onFixedRefresh;
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
            InitCount<TP, TD>(data.Length);
            OnRefresh = null;
            OnFixedRefresh = null;
            return GetGOs<TP, TD>(data);
        }
        /// <summary>
        /// 通过数量初始化
        /// </summary>
        /// <param name="count"></param>
        public void InitCount<TP, TD>(int count) where TP : Presenter<TD>, new() where TD : PresenterData, new()
        {
            if (IsInitedCount)
            {
                CLog.Error("InitCount 无法初始化2次!!!!");
                return;
            }
            IsInitedCount = true;
            GOs.Clear();
            for (int i = 0; i < Trans.childCount; ++i)
            {
                Transform temp = Trans.GetChild(i);
                var ele = temp.GetComponent<LayoutElement>();
                if (ele != null && ele.ignoreLayout)
                {
                    continue;
                }
                GOs.Add(temp.gameObject);
                temp.gameObject.SetActive(false);
            }
            if (Prefab == null && GOs.Count > 0)
            {
                Prefab = GOs[0];
            }
            if (Prefab == null)
            {
                CLog.Error("{0}: Prefab == null", Path);
                return;
            }
            if (count <= 0)
                CLog.Error("Count <= 0");
            if (Prefab.name.StartsWith(BaseConstMgr.STR_Base))
                CLog.Error($"不能使用基础UI Prefab 初始化:{Prefab.name}");

            //差值
            int subCount = count - GOs.Count;

            if (subCount > 0)
            {
                //生成剩余的游戏对象
                for (int i = 0; i < subCount; ++i)
                {
                    GameObject temp = GameObject.Instantiate(Prefab, this.RectTrans.position, this.RectTrans.rotation) as GameObject;
                    (temp.transform as RectTransform).SetParent(this.RectTrans);
                    (temp.transform as RectTransform).localScale = Vector3.one;
                    GOs.Add(temp);
                }
            }

            for (int i = 0; i < count; ++i)
            {
                GOs[i].SetActive(true);
                var tempPresenter = GOs[i].GetComponent<TP>();
                Presenters.Add(tempPresenter);

                if (tempPresenter is BaseCheckBox checkBox)
                {
                    checkBox.IsToggleGroup = IsToggleGroup;
                    ToggleGroupCheckBoxs.Add(checkBox);
                }
            }

            //设置数量
            //Count = GOs.Count;
        }
        #endregion

        #region get GOs
        public TP[] GetGOs<TP, TD>(TD data = null) where TP : Presenter<TD> where TD : PresenterData, new()
        {
            if (data == null)
            {
                data = new TD();
            }
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            TP[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<TP>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0}", typeof(TP)));
                    break;
                }
                else
                {
                    ts[i].SetIndex(i);
                    ts[i].BaseDupplicate = this;
                    AddChild(ts[i], true);
                    if (data != null)
                    {
                        ts[i].Init(data);
                    }
                }
            }
            return ts;
        }
        public TP[] GetGOs<TP, TD>(TD[] data) where TP : Presenter<TD> where TD : PresenterData, new()
        {
            for (int i = 0; i < GOs.Count; i++)
            {
                if (GOs[i] == null)
                {
                    CLog.Error("有的GO为null");
                }
            }
            TP[] ts = GOs.Where(go => go != null).Select(go => go.GetComponent<TP>()).ToArray();
            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                {
                    CLog.Error(string.Format("取出组件为null, type={0},如果想要忽略,请添加IgnoreElement组件", typeof(TP)));
                    break;
                }
                else
                {
                    ts[i].SetIndex(i);
                    ts[i].BaseDupplicate = this;
                    AddChild(ts[i], true);
                    if (data != null)
                    {
                        if (i < data.Length)
                            ts[i].Init(data[i]);
                    }
                    else
                    {
                        ts[i].Init(new TD());
                    }
                }
            }
            return ts;
        }
        #endregion

        #region callback
        public void OnTabClick(BasePresenter arg1, PointerEventData arg2)
        {
            PreSelectIndex = CurSelectIndex;
            CurSelectIndex = arg1.Index;
            Callback_OnClickSelected?.Invoke(CurSelectIndex);
            Refresh();
        }
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerEnter(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标退出
        /// </summary>
        /// <param name="eventData"></param>
		public override void OnPointerExit(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerClick(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerDown(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerUp(PointerEventData eventData)
        {
        }
        /// <summary>
        /// 点击状态变化
        /// </summary>
        /// <param name="b"></param>
        public override void OnInteractable(bool b)
        {
        }
        #endregion

        bool Inspector_IsInitCount()
        {
            return IsAutoInit;
        }
    }

}