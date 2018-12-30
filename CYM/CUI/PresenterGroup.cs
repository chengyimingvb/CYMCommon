using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CYM;

namespace CYM.UI
{
    /// <summary>
    /// 用来控制子界面的打开或者关闭状态
    /// </summary>
    public class PresenterGroup
    {
        /// <summary>
        /// 当前的Panel列表
        /// </summary>
        public List<BasePresenter> Presenters { get; private set; } = new List<BasePresenter>();
        /// <summary>
        /// 当前Panel的index
        /// </summary>
        public int Index
        {
            get;
            private set;
        }

        #region 构造函数
        public PresenterGroup()
        {

        }
        public PresenterGroup(List<BasePresenter> panels)
        {
            if (panels == null)
            {
                throw new ArgumentNullException("panels");
            }
            if (panels.Count == 0)
            {
                throw new InvalidOperationException("panels.Length == 0");
            }
            Presenters.Clear();
            foreach (var item in panels)
            {
                if (item.OwnerdPresenterGroup != null)
                {
                    CLog.Error("item 已经被挂在某个PanelGroup下面");
                    continue;
                }
                item.IsSubPresenter = true;
                item.OwnerdPresenterGroup = this;
                Presenters.Add(item);
            }
        }
        #endregion

        public void AddPanel(BasePresenter panel)
        {
            if (panel == null)
                return;
            if (Presenters.Contains(panel))
                return;
            panel.IsSubPresenter = true;
            Presenters.Add(panel);
        }

        /// <summary>
        /// 通过index设置当前的panel
        /// </summary>
        /// <param name="state"></param>
        /// <param name="type"></param>
        public void ShowPanel(int state)
        {
            if (Presenters.Count == 0)
                return;
            if (state < -1 || state >= Presenters.Count)
            {
                return;
            }
            if (Index != state)
            {
                Index = state;
            }
            RefreshShow();
        }
        /// <summary>
        /// 通过showable设置当前的panel
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        public void ShowPanel(BasePresenter obj)
        {
            if (Presenters.Count == 0)
                return;
            var temp = Presenters.FindIndex((x) => { return x == obj; });
            ShowPanel(temp);

        }
        public void TogglePanel(BasePresenter obj)
        {
            if (Presenters.Count == 0)
                return;
            var temp = Presenters.Find((x) => { return x == obj; });
            var tempIndex = Presenters.FindIndex((x) => { return x == obj; });
            if (!temp.IsShow)
                ShowPanel(tempIndex);
            else
                ShowDefault();
        }

        public void ShowDefault()
        {
            if (Presenters.Count == 0)
                return;
            ShowPanel(0);
        }

        public void RefreshShow()
        {
            if (Presenters.Count == 0)
                return;
            //UI互斥
            for (int i = 0; i < Presenters.Count; i++)
            {
                Presenters[i].Show(i == Index);
            }
            Refresh();
        }

        /// <summary>
        /// 刷新Panel,注意不要手动调用
        /// </summary>
        public void Refresh()
        {
            if (Presenters.Count == 0)
                return;
            //刷新
            if (Index != -1)
            {
                if(Presenters[Index].IsAutoRefresh)
                    Presenters[Index].Refresh();
            }
        }

        public void Clear()
        {
            Presenters.Clear();
        }
    }
}