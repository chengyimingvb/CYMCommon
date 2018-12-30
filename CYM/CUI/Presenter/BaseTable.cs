//------------------------------------------------------------------------------
// BaseTable.cs
// Copyright 2018 2018/10/28 
// Created by CYM on 2018/10/28
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CYM.UI
{
    public class BaseTableData : BaseScrollData
    {
        public Callback<BasePresenter,PointerEventData> OnTitleClick;
        public BaseButtonData[] TitleDatas;
    }
    public class BaseTable : Presenter<BaseTableData>
    {
        #region inspector
        [SerializeField]
        BaseDupplicate DP;
        [SerializeField]
        BaseScroll Scroll;
        #endregion

        #region prop
        BaseButton[] Titles;
        #endregion

        #region set
        /// <summary>
        /// 初始化Table menu
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override void Init(BaseTableData tableData) //where ScrollTP : Presenter<ScrollTD>, new() where ScrollTD : PresenterData, new()
        {
            base.Init(tableData);

            if (DP == null)
                CLog.Error("没有BaseDupplicate组件");
            if (Scroll == null)
                CLog.Error("没有BaseScroll组件");
            if (Data.GetCustomDatas == null)
                CLog.Error("TableData 的 GetCustomDatas 必须设置");
            if (Data.OnRefresh == null)
                CLog.Error("TableData 的 OnRefresh 必须设置");

            Scroll.Init(tableData);
            Titles = DP.Init<BaseButton, BaseButtonData>(Data.TitleDatas);
            foreach (var item in Titles)
                item.Data.OnClick += OnBntClick;           
        }
        #endregion



        #region Callback
        void OnBntClick(BasePresenter presenter, PointerEventData data)
        {
            Data?.OnTitleClick?.Invoke(presenter,data);
            Scroll.SortData(presenter.Index);
            SetDirty();
        }
        #endregion
    }
}