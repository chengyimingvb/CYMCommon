using UnityEngine;
using UnityEngine.UI;
using CYM;
using System;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class BaseTabMenuData:PresenterData
    {
        /// <summary>
        /// int1 =当前的index
        /// int2 =上次的index
        /// </summary>
        public Callback<int, int> OnSelectChange;
        /// <summary>
        /// 当前选得index
        /// </summary>
        public Callback<int> OnClickSelected;

        public BaseTabData[] TabDatas;
    }

    public class BaseTabMenu : Presenter<BaseTabMenuData>
    {
        #region prop
        [SerializeField]
        BaseDupplicate DP;
        BaseTab[] Tabs;
        /// <summary>
        /// 当前选择的index
        /// </summary>
        public int CurSelectIndex { get;protected set; } = 0;
        /// <summary>
        /// 上一次的选择
        /// </summary>
        public int PreSelectIndex { get; protected set; } = 0;
        /// <summary>
        /// 控件组
        /// </summary>
        PresenterGroup PresenterGroup=new PresenterGroup();
        #endregion

        #region life
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void OnDestroy()
        {
            PresenterGroup.Clear();
            base.OnDestroy();
        }
        #endregion

        #region get
        BaseUIView CurView
        {
            get
            {
                if (Tabs == null)
                    return null;
                if (Tabs.Length > CurSelectIndex)
                    return Tabs[CurSelectIndex].Data.LinkView;
                return null;
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 初始化tab menu
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override void Init(BaseTabMenuData tabMenuData)
        {
            if (DP == null)
            {
                CLog.Error("没有BaseDupplicate组建");
            }
            base.Init(tabMenuData);
            if (Data.TabDatas == null)
            {
                CLog.Error("BaseTabMenu的TabData必须被赋值!");
            }
            Tabs = DP.Init<BaseTab, BaseTabData>(tabMenuData.TabDatas);
            foreach (var item in Tabs)
            {
                item.Data.OnClick += OnTabClick;
                PresenterGroup.AddPanel(item.Data.LinkPresenter);
            }
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            foreach (var item in Tabs)
            {
                if (item.Index == CurSelectIndex)
                    item.Select(true);
                else
                    item.Select(false);
            }
            Data?.OnSelectChange(CurSelectIndex,PreSelectIndex);

            //刷新控件或者界面
            PresenterGroup.ShowPanel(CurSelectIndex);
            CurView?.Show(true);
        }
        /// <summary>
        /// 选择Index
        /// </summary>
        /// <param name="index"></param>
        public void SetSelectIndex(int index)
        {
            CurSelectIndex = index;
        }
        #endregion

        #region Callback
        void OnTabClick(BasePresenter presenter, PointerEventData data)
        {
            PreSelectIndex = CurSelectIndex;
            CurSelectIndex = presenter.Index;
            Data?.OnClickSelected(CurSelectIndex);
            SetDirty();
        }
        #endregion

    }

}