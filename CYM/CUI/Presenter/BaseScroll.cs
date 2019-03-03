using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BaseScrollData : PresenterData
    {
        public Func<IList<object>> GetCustomDatas;
        /// <summary>
        /// object1=presenter
        /// object2=custom data
        /// </summary>
        public Callback<object, object> OnRefresh;

        public List<Func<object, object>> Sorter = new List<Func<object, object>>();
    }

    [RequireComponent(typeof(ScrollRect))]
    public class BaseScroll : Presenter<BaseScrollData>
    {
        #region inspector
        [FoldoutGroup("Inspector"), SerializeField]
        BasePresenter BasePrefab;
        [FoldoutGroup("Inspector"), SerializeField]
        Scrollbar Scrollbar;
        #endregion

        #region data
        [FoldoutGroup("Data"), SerializeField]
        bool IsFixedScrollBar = false;
        [FoldoutGroup("Data"), SerializeField]
        TextAnchor Anchor = TextAnchor.UpperLeft;
        /// <summary>
        /// The direction the scroller is handling
        /// </summary>
        [FoldoutGroup("Data"), SerializeField]
        public ScrollDirectionEnum scrollDirection;

        /// <summary>
        /// The number of pixels between cell views, starting after the first cell view
        /// </summary>
        [FoldoutGroup("Data"), SerializeField]
        public float spacing;

        /// <summary>
        /// The padding inside of the scroller: top, bottom, left, right.
        /// </summary>
        [FoldoutGroup("Data"), SerializeField]
        public RectOffset padding;

        /// <summary>
        /// Whether the scroller should loop the cell views
        /// </summary>
        [FoldoutGroup("Data"), SerializeField]
        private bool loop;

        /// <summary>
        /// Whether the scollbar should be shown
        /// </summary>
        [FoldoutGroup("Data"), SerializeField]
        private ScrollbarVisibilityEnum scrollbarVisibility;

        /// <summary>
        /// Whether snapping is turned on
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public bool snapping;

        /// <summary>
        /// This is the speed that will initiate the snap. When the
        /// scroller slows down to this speed it will snap to the location
        /// specified.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public float snapVelocityThreshold;

        /// <summary>
        /// The snap offset to watch for. When the snap occurs, this
        /// location in the scroller will be how which cell to snap to 
        /// is determined.
        /// Typically, the offset is in the range 0..1, with 0 being
        /// the top / left of the scroller and 1 being the bottom / right.
        /// In most situations the watch offset and the jump offset 
        /// will be the same, they are just separated in case you need
        /// that added functionality.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public float snapWatchOffset;

        /// <summary>
        /// The snap location to move the cell to. When the snap occurs,
        /// this location in the scroller will be where the snapped cell
        /// is moved to.
        /// Typically, the offset is in the range 0..1, with 0 being
        /// the top / left of the scroller and 1 being the bottom / right.
        /// In most situations the watch offset and the jump offset 
        /// will be the same, they are just separated in case you need
        /// that added functionality.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public float snapJumpToOffset;

        /// <summary>
        /// Once the cell has been snapped to the scroller location, this
        /// value will determine how the cell is centered on that scroller
        /// location. 
        /// Typically, the offset is in the range 0..1, with 0 being
        /// the top / left of the cell and 1 being the bottom / right.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public float snapCellCenterOffset;

        /// <summary>
        /// Whether to include the spacing between cells when determining the
        /// cell offset centering.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public bool snapUseCellSpacing;

        /// <summary>
        /// What function to use when interpolating between the current 
        /// scroll position and the snap location. This is also known as easing. 
        /// If you want to go immediately to the snap location you can either 
        /// set the snapTweenType to immediate or set the snapTweenTime to zero.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public TweenType snapTweenType;

        /// <summary>
        /// The time it takes to interpolate between the current scroll 
        /// position and the snap location.
        /// If you want to go immediately to the snap location you can either 
        /// set the snapTweenType to immediate or set the snapTweenTime to zero.
        /// </summary>
        [FoldoutGroup("Snap"), SerializeField]
        public float snapTweenTime;
        #endregion

        #region prop val
        public IList<object> CacheCustomData { get; private set; } = new List<object>();
        float BasePrefabSize = 100;
        RectTransform BasePrefabRect;
        bool SortReversedList = false;
        int SortBy = -1;
        float ScrollBarSize = 0.0f;
        bool IsDirtyReload = false;
        #endregion

        #region life
        protected override void Awake()
        {
            if (BasePrefab == null)
            {
                CLog.Error("Scroll的BasePrefab不能为空,Error gameobject:{0}", gameObject.name);
                return;
            }
            InitializesScroller();
            BasePrefabRect = BasePrefab.transform as RectTransform;
            base.Awake();
        }
        void Update()
        {
            if (IsDirtyReload)
            {
                IsDirtyReload = false;
                RefreshData();
                Refresh();
            }

            if (_updateSpacing)
            {
                _UpdateSpacing(spacing);
                _reloadData = false;
            }

            if (ScrollRectSize > _cellViewOffsetArray.Last())
            {
                ScrollRect.scrollSensitivity = 2;
            }
            else
            {
                ScrollRect.scrollSensitivity = 15;
            }

            if (_reloadData)
            {
                // if the reload flag is true, then reload the data
                ReloadData();
            }

            // if the scroll rect size has changed and looping is on,
            // or the loop setting has changed, then we need to resize
            if (
                    (loop && _lastScrollRectSize != ScrollRectSize)
                    ||
                    (loop != _lastLoop)
                )
            {
                _Resize(true);
                _lastScrollRectSize = ScrollRectSize;

                _lastLoop = loop;
            }

            // update the scroll bar visibility if it has changed
            if (_lastScrollbarVisibility != scrollbarVisibility)
            {
                ScrollbarVisibility = scrollbarVisibility;
                _lastScrollbarVisibility = scrollbarVisibility;
            }

            // determine if the scroller has started or stopped scrolling
            // and call the delegate if so.
            if (LinearVelocity != 0 && !IsScrolling)
            {
                IsScrolling = true;
                scrollerScrollingChanged?.Invoke(this, true);
            }
            else if (LinearVelocity == 0 && IsScrolling)
            {
                IsScrolling = false;
                scrollerScrollingChanged?.Invoke(this, false);
            }

            if (Scrollbar && IsFixedScrollBar)
                Scrollbar.size = ScrollBarSize;
        }
        new void OnValidate()
        {
            // if spacing changed, update it
            if (_initialized && spacing != _layoutGroup.spacing)
            {
                _updateSpacing = true;
            }
        }

        new void OnEnable()
        {
            // when the scroller is enabled, add a listener to the onValueChanged handler
            ScrollRect.onValueChanged.AddListener(_ScrollRect_OnValueChanged);
        }

        new void OnDisable()
        {
            // when the scroller is disabled, remove the listener
            ScrollRect.onValueChanged.RemoveListener(_ScrollRect_OnValueChanged);
        }
        public override void Refresh()
        {
            base.Refresh();
            if (_activeCellViews == null || _activeCellViews.data == null)
                return;
            foreach (var item in _activeCellViews.data)
            {
                if (item == null)
                    continue;
                OnRefreshCell(item, item.DataIndex, item.Index);
            }
        }
        void RefreshData()
        {
            if (Data != null && Data.GetCustomDatas != null && BasePrefab != null)
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    BasePrefabSize = BasePrefabRect.sizeDelta.y;
                else
                    BasePrefabSize = BasePrefabRect.sizeDelta.x;

                //重新获取数据
                CacheCustomData = Data.GetCustomDatas.Invoke();
                //数据排序
                var sortCall = GetSortCall();
                if (sortCall != null)
                {
                    var sortBy = SortBy;
                    if (SortReversedList)
                    {
                        CacheCustomData = new List<object>(CacheCustomData.OrderByDescending(sortCall));
                    }
                    else
                    {
                        CacheCustomData = new List<object>(CacheCustomData.OrderBy(sortCall));
                    }
                }
                //重载数据
                ReloadData(0.0f);
                if (Scrollbar != null)
                {
                    Scrollbar.size = ScrollBarSize;
                }
            }
        }
        public override void OnViewShow(bool b)
        {
            if (b)
            {
                RefreshData();
            }
            base.OnViewShow(b);
        }
        public override void OnShow(bool isShow)
        {
            if (isShow)
            {
                RefreshData();
            }
            base.OnShow(isShow);
        }
        void InitializesScroller()
        {
            GameObject go;

            ScrollRect = this.GetComponent<ScrollRect>();
            _scrollRectTransform = ScrollRect.GetComponent<RectTransform>();
            if (Scrollbar != null)
                ScrollBarSize = Scrollbar.size;
            if (ScrollRect != null)
            {
                ScrollRect.inertia = true;
                ScrollRect.decelerationRate = 0.2f;
                ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
                ScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            }

            go = ScrollRect.content.gameObject;
            if (scrollDirection == ScrollDirectionEnum.Vertical)
                _layoutGroup=go.AddComponent<VerticalLayoutGroup>();
            else
                _layoutGroup=go.AddComponent<HorizontalLayoutGroup>();
            _container = go.GetComponent<RectTransform>();

            _layoutGroup.spacing = spacing;
            _layoutGroup.padding = padding;
            _layoutGroup.childAlignment = Anchor;

            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                ScrollRect.verticalScrollbar = Scrollbar;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.childForceExpandWidth = false;
                ScrollRect.vertical = true;
            }
            else
            {
                ScrollRect.horizontalScrollbar = Scrollbar;
                _layoutGroup.childForceExpandHeight = false;
                _layoutGroup.childForceExpandWidth = false;
                ScrollRect.horizontal = true;
            }

            // create the padder objects

            go = new GameObject("First Padder", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(_container, false);
            _firstPadder = go.GetComponent<LayoutElement>();

            go = new GameObject("Last Padder", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(_container, false);
            _lastPadder = go.GetComponent<LayoutElement>();

            // create the recycled cell view container
            go = new GameObject("Recycled Cells", typeof(RectTransform));
            go.transform.SetParent(ScrollRect.transform, false);
            _recycledCellViewContainer = go.GetComponent<RectTransform>();
            _recycledCellViewContainer.gameObject.SetActive(false);

            // set up the last values for updates
            _lastScrollRectSize = ScrollRectSize;
            _lastLoop = loop;
            _lastScrollbarVisibility = scrollbarVisibility;

            _initialized = true;
        }

        #endregion

        #region set
        public void Init(Func<IList<object>> getData, Callback<object, object> onRefresh, List<Func<object, object>> sorter=null)
        {
            var temp = new BaseScrollData { GetCustomDatas = getData, OnRefresh = onRefresh, Sorter = sorter };
            Init(temp);
        }
        /// <summary>
        /// 初始化Scroll
        /// </summary>
        /// <typeparam name="TP">presenter</typeparam>
        /// <typeparam name="TD">presenter data</typeparam>
        /// <typeparam name="TCD">custom data </typeparam>
        /// <param name="getCustomDatas"></param>
        /// <param name="onRefresh"></param>
        public override void Init(BaseScrollData data) 
        {
            base.Init(data);
            if (BasePrefab == null)
            {
                CLog.Error("Scroll:{0} 基础Prefab不能为Null", name);
                return;
            }
            if (BasePrefab.name.StartsWith(BaseConstMgr.STR_Base))
                CLog.Error($"无法使用基础UI Prefab初始化:{BasePrefab.name}");
            if (Data.GetCustomDatas == null)
                CLog.Error("TableData 的 GetCustomDatas 必须设置");
            if (Data.OnRefresh == null)
                CLog.Error("TableData 的 OnRefresh 必须设置");
        }
        public void SortData(int by)
        {
            if (by > Data.Sorter.Count)
            {
                CLog.Error("ListSort,数组越界!!!");
            }
            if (SortBy == by)
            {
                SortReversedList = !SortReversedList;
            }
            else if (SortBy == -1)
            {
                SortReversedList = true;
            }
            else
            {
                SortReversedList = true;
            }
            SortBy = by;
        }
        public void SetDirtyReloadData()
        {
            IsDirtyReload = true;
        }
        #endregion

        #region get
        public Func<object, object> GetSortCall()
        {
            if (Data.Sorter == null)
                return null;
            if (Data.Sorter.Count <= SortBy)
                return null;
            if (SortBy == -1)
                return null;
            return Data.Sorter[SortBy];
        }
        /// <summary>
        /// 获得控件数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T GetData<T>(int dataIndex) where T : class
        {
            if (dataIndex < 0)
                return null;
            if (dataIndex >= CacheCustomData.Count)
                return null;
            return CacheCustomData[dataIndex] as T;
        }
        #endregion

        #region Callback
        protected virtual BasePresenter GetCellView(int dataIndex, int cellIndex)
        {
            if (BasePrefab == null)
            {
                throw new NotImplementedException("没有BasePrefab");
            }
            BasePresenter cellPresenter = GetCellPresenter(BasePrefab);
            return cellPresenter;
        }

        protected virtual void OnRefreshCell(BasePresenter presenter, int dataIndex, int cellIndex)
        {
            Data.OnRefresh.Invoke(presenter, CacheCustomData[dataIndex]);
        }

        protected virtual float GetCellViewSize(BaseScroll scroller, int dataIndex)
        {
            return BasePrefabSize;
        }

        protected virtual int GetNumberOfCells(BaseScroll scroller)
        {
            return CacheCustomData.Count;
        }
        #endregion

        #region Public

        /// <summary>
        /// The direction this scroller is handling
        /// </summary>
        public enum ScrollDirectionEnum
        {
            Vertical,
            Horizontal
        }

        /// <summary>
        /// Which side of a cell to reference.
        /// For vertical scrollers, before means above, after means below.
        /// For horizontal scrollers, before means to left of, after means to the right of.
        /// </summary>
        public enum CellViewPositionEnum
        {
            Before,
            After
        }

        /// <summary>
        /// This will set how the scroll bar should be shown based on the data. If no scrollbar
        /// is attached, then this is ignored. OnlyIfNeeded will hide the scrollbar based on whether
        /// the scroller is looping or there aren't enough items to scroll.
        /// </summary>
        public enum ScrollbarVisibilityEnum
        {
            OnlyIfNeeded,
            Always,
            Never
        }

        /// <summary>
        /// This delegate is called when a cell view is hidden or shown
        /// </summary>
        public Callback<BasePresenter> cellViewVisibilityChanged;

        /// <summary>
        /// This delegate is called just before a cell view is hidden by recycling
        /// </summary>
        public Callback<BasePresenter> cellViewWillRecycle;

        /// <summary>
        /// This delegate is called when the scroll rect scrolls
        /// </summary>
        public Callback<BaseScroll, Vector2, float> scrollerScrolled;

        /// <summary>
        /// This delegate is called when the scroller has snapped to a position
        /// </summary>
        public Callback<BaseScroll, int, int, BasePresenter> scrollerSnapped;

        /// <summary>
        /// This delegate is called when the scroller has started or stopped scrolling
        /// </summary>
        public Callback<BaseScroll, bool> scrollerScrollingChanged;

        /// <summary>
        /// This delegate is called when the scroller has started or stopped tweening
        /// </summary>
        public Callback<BaseScroll, bool> scrollerTweeningChanged;

        /// <summary>
        /// This delegate is called when the scroller creates a new cell view from scratch
        /// </summary>
        public Callback<BaseScroll, BasePresenter> cellViewInstantiated;

        /// <summary>
        /// This delegate is called when the scroller reuses a recycled cell view
        /// </summary>
        public Callback<BaseScroll, BasePresenter> cellViewReused;

        /// <summary>
        /// The absolute position in pixels from the start of the scroller
        /// </summary>
        public float ScrollPosition
        {
            get
            {
                return _scrollPosition;
            }
            set
            {
                // make sure the position is in the bounds of the current set of views
                value = Mathf.Clamp(value, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));

                // only if the value has changed
                if (_scrollPosition != value)
                {
                    _scrollPosition = value;
                    if (scrollDirection == ScrollDirectionEnum.Vertical)
                    {
                        // set the vertical position
                        ScrollRect.verticalNormalizedPosition = 1-(_scrollPosition / ScrollSize);
                    }
                    else
                    {
                        // set the horizontal position
                        ScrollRect.horizontalNormalizedPosition = (_scrollPosition / ScrollSize);
                    }

                    // flag that we need to refresh
                    //_refreshActive = true;
                }
            }
        }

        /// <summary>
        /// The size of the active cell view container minus the visibile portion
        /// of the scroller
        /// </summary>
        public float ScrollSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    return Mathf.Max(_container.rect.height - _scrollRectTransform.rect.height, 0);
                else
                    return Mathf.Max(_container.rect.width - _scrollRectTransform.rect.width, 0);
            }
        }

        /// <summary>
        /// The normalized position of the scroller between 0 and 1
        /// </summary>
        public float NormalizedScrollPosition
        {
            get
            {
                var scrollPosition = ScrollPosition;
                return (scrollPosition <= 0 ? 0 : _scrollPosition / ScrollSize);
            }
        }

        /// <summary>
        /// Whether the scroller should loop the resulting cell views.
        /// Looping creates three sets of internal size data, attempting
        /// to keep the scroller in the middle set. If the scroller goes
        /// outside of this set, it will jump back into the middle set,
        /// giving the illusion of an infinite set of data.
        /// </summary>
        public bool Loop
        {
            get
            {
                return loop;
            }
            set
            {
                // only if the value has changed
                if (loop != value)
                {
                    // get the original position so that when we turn looping on
                    // we can jump back to this position
                    var originalScrollPosition = _scrollPosition;

                    loop = value;

                    // call resize to generate more internal elements if loop is on,
                    // remove the elements if loop is off
                    _Resize(false);

                    if (loop)
                    {
                        // set the new scroll position based on the middle set of data + the original position
                        ScrollPosition = _loopFirstScrollPosition + originalScrollPosition;
                    }
                    else
                    {
                        // set the new scroll position based on the original position and the first loop position
                        ScrollPosition = originalScrollPosition - _loopFirstScrollPosition;
                    }

                    // update the scrollbars
                    ScrollbarVisibility = scrollbarVisibility;
                }
            }
        }

        /// <summary>
        /// Sets how the visibility of the scrollbars should be handled
        /// </summary>
        public ScrollbarVisibilityEnum ScrollbarVisibility
        {
            get
            {
                return scrollbarVisibility;
            }
            set
            {
                scrollbarVisibility = value;

                // only if the scrollbar exists
                if (Scrollbar != null)
                {
                    // make sure we actually have some cell views
                    if (_cellViewOffsetArray != null && _cellViewOffsetArray.Count > 0)
                    {
                        if (_cellViewOffsetArray.Last() < ScrollRectSize || loop)
                        {
                            // if the size of the scrollable area is smaller than the scroller
                            // or if we have looping on, hide the scrollbar unless the visibility
                            // is set to Always.
                            Scrollbar.gameObject.SetActive(scrollbarVisibility == ScrollbarVisibilityEnum.Always);
                        }
                        else
                        {
                            // if the size of the scrollable areas is larger than the scroller
                            // or looping is off, then show the scrollbars unless visibility
                            // is set to Never.
                            Scrollbar.gameObject.SetActive(scrollbarVisibility != ScrollbarVisibilityEnum.Never);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is the velocity of the scroller.
        /// </summary>
        public Vector2 Velocity
        {
            get
            {
                return ScrollRect.velocity;
            }
            set
            {
                ScrollRect.velocity = value;
            }
        }

        /// <summary>
        /// The linear velocity is the velocity on one axis.
        /// The scroller should only be moving one one axix.
        /// </summary>
        public float LinearVelocity
        {
            get
            {
                // return the velocity component depending on which direction this is scrolling
                return (scrollDirection == ScrollDirectionEnum.Vertical ? ScrollRect.velocity.y : ScrollRect.velocity.x);
            }
            set
            {
                // set the appropriate component of the velocity
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                {
                    ScrollRect.velocity = new Vector2(0, value);
                }
                else
                {
                    ScrollRect.velocity = new Vector2(value, 0);
                }
            }
        }

        /// <summary>
        /// Whether the scroller is scrolling or not
        /// </summary>
        public bool IsScrolling
        {
            get; private set;
        }

        /// <summary>
        /// Whether the scroller is tweening or not
        /// </summary>
        public bool IsTweening
        {
            get; private set;
        }

        /// <summary>
        /// This is the first cell view index showing in the scroller's visible area
        /// </summary>
        public int StartCellViewIndex { get; private set; }

        /// <summary>
        /// This is the last cell view index showing in the scroller's visible area
        /// </summary>
        public int EndCellViewIndex { get; private set; }

        /// <summary>
        /// This is the first data index showing in the scroller's visible area
        /// </summary>
        public int StartDataIndex
        {
            get
            {
                return StartCellViewIndex % NumberOfCells;
            }
        }

        /// <summary>
        /// This is the last data index showing in the scroller's visible area
        /// </summary>
        public int EndDataIndex
        {
            get
            {
                return EndCellViewIndex % NumberOfCells;
            }
        }

        /// <summary>
        /// This is the number of cells in the scroller
        /// </summary>
        public int NumberOfCells
        {
            get
            {
                return GetNumberOfCells(this);
            }
        }

        /// <summary>
        /// This is a convenience link to the scroller's scroll rect
        /// </summary>
        public ScrollRect ScrollRect { get; private set; }

        /// <summary>
        /// The size of the visible portion of the scroller
        /// </summary>
        public float ScrollRectSize
        {
            get
            {
                if (scrollDirection == ScrollDirectionEnum.Vertical)
                    return _scrollRectTransform.rect.height;
                else
                    return _scrollRectTransform.rect.width;
            }
        }

        /// <summary>
        /// Create a cell view, or recycle one if it already exists
        /// </summary>
        /// <param name="cellPrefab">The prefab to use to create the cell view</param>
        /// <returns></returns>
        public BasePresenter GetCellPresenter(BasePresenter cellPrefab)
        {
            // see if there is a view to recycle
            var cellView = _GetRecycledCellView(cellPrefab);
            if (cellView == null)
            {
                // no recyleable cell found, so we create a new view
                // and attach it to our container
                var go = Instantiate(cellPrefab.gameObject);
                go.name = cellPrefab.name;
                cellView = go.GetComponent<BasePresenter>();
                cellView.transform.SetParent(_container);
                cellView.transform.localPosition = Vector3.zero;
                cellView.transform.localRotation = Quaternion.identity;

                // call the instantiated callback
                cellViewInstantiated?.Invoke(this, cellView);
            }
            else
            {
                // call the reused callback
                cellViewReused?.Invoke(this, cellView);
            }

            return cellView;
        }

        /// <summary>
        /// This resets the internal size list and refreshes the cell views
        /// </summary>
        /// <param name="scrollPositionFactor">The percentage of the scroller to start at between 0 and 1, 0 being the start of the scroller</param>
        public void ReloadData(float scrollPositionFactor = 0)
        {
            _reloadData = false;

            // recycle all the active cells so
            // that we are sure to get fresh views
            _RecycleAllCells();

            // if we have a delegate handling our data, then
            // call the resize
            //if (_delegate != null)
            _Resize(false);

            if (ScrollRect == null || _scrollRectTransform == null || _container == null)
            {
                _scrollPosition = 0f;
                return;
            }

            _scrollPosition = Mathf.Clamp(scrollPositionFactor * ScrollSize, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                // set the vertical position
                ScrollRect.verticalNormalizedPosition =  1-scrollPositionFactor;
            }
            else
            {
                // set the horizontal position
                ScrollRect.horizontalNormalizedPosition = scrollPositionFactor;
            }
        }

        /// <summary>
        /// Removes all cells, both active and recycled from the scroller.
        /// This will call garbage collection.
        /// </summary>
        public void ClearAll()
        {
            ClearActive();
            ClearRecycled();
        }

        /// <summary>
        /// Removes all the active cell views. This should only be used if you want
        /// to get rid of cells because of settings set by Unity that cannot be
        /// changed at runtime. This will call garbage collection.
        /// </summary>
        public void ClearActive()
        {
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                DestroyImmediate(_activeCellViews[i].gameObject);
            }
            _activeCellViews.Clear();
        }

        /// <summary>
        /// Removes all the recycled cell views. This should only be used after you
        /// load in a completely different set of cell views that will not use the 
        /// recycled views. This will call garbage collection.
        /// </summary>
        public void ClearRecycled()
        {
            for (var i = 0; i < _recycledCellViews.Count; i++)
            {
                DestroyImmediate(_recycledCellViews[i].gameObject);
            }
            _recycledCellViews.Clear();
        }

        /// <summary>
        /// Turn looping on or off. This is just a helper function so 
        /// you don't have to keep track of the state of the looping
        /// in your own scripts.
        /// </summary>
        public void ToggleLoop()
        {
            Loop = !loop;
        }

        public enum LoopJumpDirectionEnum
        {
            Closest,
            Up,
            Down
        }

        /// <summary>
        /// Jump to a position in the scroller based on a dataIndex. This overload allows you
        /// to specify a specific offset within a cell as well.
        /// </summary>
        /// <param name="dataIndex">he data index to jump to</param>
        /// <param name="scrollerOffset">The offset from the start (top / left) of the scroller in the range 0..1.
        /// Outside this range will jump to the location before or after the scroller's viewable area</param>
        /// <param name="cellOffset">The offset from the start (top / left) of the cell in the range 0..1</param>
        /// <param name="useSpacing">Whether to calculate in the spacing of the scroller in the jump</param>
        /// <param name="tweenType">What easing to use for the jump</param>
        /// <param name="tweenTime">How long to interpolate to the jump point</param>
        /// <param name="jumpComplete">This delegate is fired when the jump completes</param>
        public void JumpToDataIndex(int dataIndex,
            float scrollerOffset = 0,
            float cellOffset = 0,
            bool useSpacing = true,
            TweenType tweenType = TweenType.immediate,
            float tweenTime = 0f,
            Action jumpComplete = null,
            LoopJumpDirectionEnum loopJumpDirection = LoopJumpDirectionEnum.Closest
            )
        {
            var cellOffsetPosition = 0f;

            if (cellOffset != 0)
            {
                // calculate the cell offset position

                // get the cell's size
                var cellSize = (GetCellViewSize(this, dataIndex));

                if (useSpacing)
                {
                    // if using spacing add spacing from one side
                    cellSize += spacing;

                    // if this is not a bounday cell, then add spacing from the other side
                    if (dataIndex > 0 && dataIndex < (NumberOfCells - 1)) cellSize += spacing;
                }

                // calculate the position based on the size of the cell and the offset within that cell
                cellOffsetPosition = cellSize * cellOffset;
            }

            if (scrollerOffset == 1f)
            {
                cellOffsetPosition += padding.bottom;
            }

            // cache the offset for quicker calculation
            var offset = -(scrollerOffset * ScrollRectSize) + cellOffsetPosition;

            var newScrollPosition = 0f;

            if (loop)
            {
                // if looping, then we need to determine the closest jump position.
                // we do that by checking all three sets of data locations, and returning the closest one

                // get the scroll positions for each data set.
                // Note: we are calculating the position based on the cell view index, not the data index here
                var set1Position = GetScrollPositionForCellViewIndex(dataIndex, CellViewPositionEnum.Before) + offset;
                var set2Position = GetScrollPositionForCellViewIndex(dataIndex + NumberOfCells, CellViewPositionEnum.Before) + offset;
                var set3Position = GetScrollPositionForCellViewIndex(dataIndex + (NumberOfCells * 2), CellViewPositionEnum.Before) + offset;

                // get the offsets of each scroll position from the current scroll position
                var set1Diff = (Mathf.Abs(_scrollPosition - set1Position));
                var set2Diff = (Mathf.Abs(_scrollPosition - set2Position));
                var set3Diff = (Mathf.Abs(_scrollPosition - set3Position));

                switch (loopJumpDirection)
                {
                    case LoopJumpDirectionEnum.Closest:

                        // choose the smallest offset from the current position (the closest position)
                        if (set1Diff < set2Diff)
                        {
                            if (set1Diff < set3Diff)
                            {
                                newScrollPosition = set1Position;
                            }
                            else
                            {
                                newScrollPosition = set3Position;
                            }
                        }
                        else
                        {
                            if (set2Diff < set3Diff)
                            {
                                newScrollPosition = set2Position;
                            }
                            else
                            {
                                newScrollPosition = set3Position;
                            }
                        }

                        break;

                    case LoopJumpDirectionEnum.Up:

                        newScrollPosition = set1Position;
                        break;

                    case LoopJumpDirectionEnum.Down:

                        newScrollPosition = set3Position;
                        break;

                }
            }
            else
            {
                // not looping, so just get the scroll position from the dataIndex
                newScrollPosition = GetScrollPositionForDataIndex(dataIndex, CellViewPositionEnum.Before) + offset;
            }

            // clamp the scroll position to a valid location
            newScrollPosition = Mathf.Clamp(newScrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));

            // if spacing is used, adjust the final position
            if (useSpacing)
            {
                // move back by the spacing if necessary
                newScrollPosition = Mathf.Clamp(newScrollPosition - spacing, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));
            }

            // ignore the jump if the scroll position hasn't changed
            if (newScrollPosition == _scrollPosition)
            {
                jumpComplete?.Invoke();
                return;
            }

            // start tweening
            StartCoroutine(TweenPosition(tweenType, tweenTime, ScrollPosition, newScrollPosition, jumpComplete));
        }

        /// <summary>
        /// Snaps the scroller on command. This is called internally when snapping is set to true and the velocity
        /// has dropped below the threshold. You can use this to manually snap whenever you like.
        /// </summary>
        public void Snap()
        {
            if (NumberOfCells == 0) return;

            // set snap jumping to true so other events won't process while tweening
            _snapJumping = true;

            // stop the scroller
            LinearVelocity = 0;

            // cache the current inertia state and turn off inertia
            _snapInertia = ScrollRect.inertia;
            ScrollRect.inertia = false;

            // calculate the snap position
            var snapPosition = ScrollPosition + (ScrollRectSize * Mathf.Clamp01(snapWatchOffset));

            // get the cell view index of cell at the watch location
            _snapCellViewIndex = GetCellViewIndexAtPosition(snapPosition);

            // get the data index of the cell at the watch location
            _snapDataIndex = _snapCellViewIndex % NumberOfCells;

            // jump the snapped cell to the jump offset location and center it on the cell offset
            JumpToDataIndex(_snapDataIndex, snapJumpToOffset, snapCellCenterOffset, snapUseCellSpacing, snapTweenType, snapTweenTime, SnapJumpComplete);
        }

        /// <summary>
        /// Gets the scroll position in pixels from the start of the scroller based on the cellViewIndex
        /// </summary>
        /// <param name="cellViewIndex">The cell index to look for. This is used instead of dataIndex in case of looping</param>
        /// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
        /// <returns></returns>
        public float GetScrollPositionForCellViewIndex(int cellViewIndex, CellViewPositionEnum insertPosition)
        {
            if (NumberOfCells == 0) return 0;
            if (cellViewIndex < 0) cellViewIndex = 0;

            if (cellViewIndex == 0 && insertPosition == CellViewPositionEnum.Before)
            {
                return 0;
            }
            else
            {
                if (cellViewIndex < _cellViewOffsetArray.Count)
                {
                    // the index is in the range of cell view offsets

                    if (insertPosition == CellViewPositionEnum.Before)
                    {
                        // return the previous cell view's offset + the spacing between cell views
                        return _cellViewOffsetArray[cellViewIndex - 1] + spacing + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left);
                    }
                    else
                    {
                        // return the offset of the cell view (offset is after the cell)
                        return _cellViewOffsetArray[cellViewIndex] + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left);
                    }
                }
                else
                {
                    // get the start position of the last cell (the offset of the second to last cell)
                    return _cellViewOffsetArray[_cellViewOffsetArray.Count - 2];
                }
            }
        }

        /// <summary>
        /// Gets the scroll position in pixels from the start of the scroller based on the dataIndex
        /// </summary>
        /// <param name="dataIndex">The data index to look for</param>
        /// <param name="insertPosition">Do we want the start or end of the cell view's position</param>
        /// <returns></returns>
        public float GetScrollPositionForDataIndex(int dataIndex, CellViewPositionEnum insertPosition)
        {
            return GetScrollPositionForCellViewIndex(loop ? GetNumberOfCells(this) + dataIndex : dataIndex, insertPosition);
        }

        /// <summary>
        /// Gets the index of a cell view at a given position
        /// </summary>
        /// <param name="position">The pixel offset from the start of the scroller</param>
        /// <returns></returns>
        public int GetCellViewIndexAtPosition(float position)
        {
            // call the overrloaded method on the entire range of the list
            return _GetCellIndexAtPosition(position, 0, _cellViewOffsetArray.Count - 1);
        }

        /// <summary>
        /// Get a cell view for a particular data index. If the cell view is not currently
        /// in the visible range, then this method will return null.
        /// Note: this is against MVC principles and will couple your controller to the view
        /// more than this paradigm would suggest. Generally speaking, the view can have knowledge
        /// about the controller, but the controller should not know anything about the view.
        /// Use this method sparingly if you are trying to adhere to strict MVC design.
        /// </summary>
        /// <param name="dataIndex">The data index of the cell view to return</param>
        /// <returns></returns>
        public BasePresenter GetCellViewAtDataIndex(int dataIndex)
        {
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                if (_activeCellViews[i].DataIndex == dataIndex)
                {
                    return _activeCellViews[i];
                }
            }

            return null;
        }
        public BasePresenter GetCellViewAtIndex(int index)
        {
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                if (_activeCellViews[i].Index == index)
                {
                    return _activeCellViews[i];
                }
            }

            return null;
        }

        #endregion

        #region Private

        /// <summary>
        /// Set after the scroller is first created. This allwos
        /// us to ignore OnValidate changes at the start
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Set when the spacing is changed in the inspector. Since we cannot
        /// make changes during the OnValidate, we have to use this flag to
        /// later call the _UpdateSpacing method from Update()
        /// </summary>
        private bool _updateSpacing = false;

        /// <summary>
        /// Cached reference to the scrollRect's transform
        /// </summary>
        private RectTransform _scrollRectTransform;

        /// <summary>
        /// Cached reference to the active cell view container
        /// </summary>
        private RectTransform _container;

        /// <summary>
        /// Cached reference to the layout group that handles view positioning
        /// </summary>
        private HorizontalOrVerticalLayoutGroup _layoutGroup;

        /// <summary>
        /// Flag to tell the scroller to reload the data
        /// </summary>
        private bool _reloadData;

        /// <summary>
        /// Flag to tell the scroller to refresh the active list of cell views
        /// </summary>
        private bool _refreshActive;

        /// <summary>
        /// List of views that have been recycled
        /// </summary>
        private SmallList<BasePresenter> _recycledCellViews = new SmallList<BasePresenter>();

        /// <summary>
        /// Cached reference to the element used to offset the first visible cell view
        /// </summary>
        private LayoutElement _firstPadder;

        /// <summary>
        /// Cached reference to the element used to keep the cell views at the correct size
        /// </summary>
        private LayoutElement _lastPadder;

        /// <summary>
        /// Cached reference to the container that holds the recycled cell views
        /// </summary>
        private RectTransform _recycledCellViewContainer;

        /// <summary>
        /// Internal list of cell view sizes. This is created when the data is reloaded 
        /// to speed up processing.
        /// </summary>
        private SmallList<float> _cellViewSizeArray = new SmallList<float>();

        /// <summary>
        /// Internal list of cell view offsets. Each cell view offset is an accumulation 
        /// of the offsets previous to it.
        /// This is created when the data is reloaded to speed up processing.
        /// </summary>
        private SmallList<float> _cellViewOffsetArray = new SmallList<float>();

        /// <summary>
        /// The scrollers position
        /// </summary>
        private float _scrollPosition;

        /// <summary>
        /// The list of cell views that are currently being displayed
        /// </summary>
        private SmallList<BasePresenter> _activeCellViews = new SmallList<BasePresenter>();

        /// <summary>
        /// The index of the first element of the middle section of cell view sizes.
        /// Used only when looping
        /// </summary>
        private int _loopFirstCellIndex;

        /// <summary>
        /// The index of the last element of the middle seciton of cell view sizes.
        /// used only when looping
        /// </summary>
        private int _loopLastCellIndex;

        /// <summary>
        /// The scroll position of the first element of the middle seciotn of cell views.
        /// Used only when looping
        /// </summary>
        private float _loopFirstScrollPosition;

        /// <summary>
        /// The scroll position of the last element of the middle section of cell views.
        /// Used only when looping
        /// </summary>
        private float _loopLastScrollPosition;

        /// <summary>
        /// The position that triggers the scroller to jump to the end of the middle section
        /// of cell views. This keeps the scroller in the middle section as much as possible.
        /// </summary>
        private float _loopFirstJumpTrigger;

        /// <summary>
        /// The position that triggers the scroller to jump to the start of the middle section
        /// of cell views. This keeps the scroller in the middle section as much as possible.
        /// </summary>
        private float _loopLastJumpTrigger;

        /// <summary>
        /// The cached value of the last scroll rect size. This is checked every frame to see
        /// if the scroll rect has resized. If so, it will refresh.
        /// </summary>
        private float _lastScrollRectSize;

        /// <summary>
        /// The cached value of the last loop setting. This is checked every frame to see
        /// if looping was toggled. If so, it will refresh.
        /// </summary>
        private bool _lastLoop;

        /// <summary>
        /// The cell view index we are snapping to
        /// </summary>
        private int _snapCellViewIndex;

        /// <summary>
        /// The data index we are snapping to
        /// </summary>
        private int _snapDataIndex;

        /// <summary>
        /// Whether we are currently jumping due to a snap
        /// </summary>
        private bool _snapJumping;

        /// <summary>
        /// What the previous inertia setting was before the snap jump.
        /// We cache it here because we need to turn off inertia while
        /// manually tweeing.
        /// </summary>
        private bool _snapInertia;

        /// <summary>
        /// The cached value of the last scrollbar visibility setting. This is checked every
        /// frame to see if the scrollbar visibility needs to be changed.
        /// </summary>
        private ScrollbarVisibilityEnum _lastScrollbarVisibility;

        /// <summary>
        /// Where in the list we are
        /// </summary>
        private enum ListPositionEnum
        {
            First,
            Last
        }

        /// <summary>
        /// This function will create an internal list of sizes and offsets to be used in all calculations.
        /// It also sets up the loop triggers and positions and initializes the cell views.
        /// </summary>
        /// <param name="keepPosition">If true, then the scroller will try to go back to the position it was at before the resize</param>
        private void _Resize(bool keepPosition)
        {
            // cache the original position
            var originalScrollPosition = _scrollPosition;

            // clear out the list of cell view sizes and create a new list
            _cellViewSizeArray.Clear();
            var offset = _AddCellViewSizes();

            // if looping, we need to create three sets of size data
            if (loop)
            {
                // if the cells don't entirely fill up the scroll area, 
                // make some more size entries to fill it up
                if (offset < ScrollRectSize)
                {
                    int additionalRounds = Mathf.CeilToInt(ScrollRectSize / offset);
                    _DuplicateCellViewSizes(additionalRounds, _cellViewSizeArray.Count);
                }

                // set up the loop indices
                _loopFirstCellIndex = _cellViewSizeArray.Count;
                _loopLastCellIndex = _loopFirstCellIndex + _cellViewSizeArray.Count - 1;

                // create two more copies of the cell sizes
                _DuplicateCellViewSizes(2, _cellViewSizeArray.Count);
            }

            // calculate the offsets of each cell view
            _CalculateCellViewOffsets();

            // set the size of the active cell view container based on the number of cell views there are and each of their sizes
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                if (ScrollRectSize > _cellViewOffsetArray.Last())
                {
                    _container.sizeDelta = new Vector2(_container.sizeDelta.x, ScrollRectSize + BasePrefabSize + padding.top + padding.bottom);
                }
                else
                {
                    _container.sizeDelta = new Vector2(_container.sizeDelta.x, _cellViewOffsetArray.Last() + BasePrefabSize + padding.top + padding.bottom);
                }
            }
            else
            {
                if (ScrollRectSize > _cellViewOffsetArray.Last())
                {
                    _container.sizeDelta = new Vector2(ScrollRectSize  + padding.left + padding.right, _container.sizeDelta.y);
                }
                else
                {
                    _container.sizeDelta = new Vector2(_cellViewOffsetArray.Last() + padding.left + padding.right, _container.sizeDelta.y);
                }
            }
            // if looping, set up the loop positions and triggers
            if (loop)
            {
                _loopFirstScrollPosition = GetScrollPositionForCellViewIndex(_loopFirstCellIndex, CellViewPositionEnum.Before) + (spacing * 0.5f);
                _loopLastScrollPosition = GetScrollPositionForCellViewIndex(_loopLastCellIndex, CellViewPositionEnum.After) - ScrollRectSize + (spacing * 0.5f);

                _loopFirstJumpTrigger = _loopFirstScrollPosition - ScrollRectSize;
                _loopLastJumpTrigger = _loopLastScrollPosition + ScrollRectSize;
            }

            // create the visibile cells
            _ResetVisibleCellViews();

            // if we need to maintain our original position
            if (keepPosition)
            {
                ScrollPosition = originalScrollPosition;
            }
            else
            {
                if (loop)
                {
                    ScrollPosition = _loopFirstScrollPosition;
                }
                else
                {
                    ScrollPosition = 0;
                }
            }

            // set up the visibility of the scrollbar
            ScrollbarVisibility = scrollbarVisibility;
        }

        /// <summary>
        /// Updates the spacing on the scroller
        /// </summary>
        /// <param name="spacing">new spacing value</param>
        private void _UpdateSpacing(float spacing)
        {
            _updateSpacing = false;
            _layoutGroup.spacing = spacing;
            ReloadData(NormalizedScrollPosition);
        }

        /// <summary>
        /// Creates a list of cell view sizes for faster access
        /// </summary>
        /// <returns></returns>
        private float _AddCellViewSizes()
        {
            var offset = 0f;
            // add a size for each row in our data based on how many the delegate tells us to create
            for (var i = 0; i < NumberOfCells; i++)
            {
                // add the size of this cell based on what the delegate tells us to use. Also add spacing if this cell isn't the first one
                _cellViewSizeArray.Add(GetCellViewSize(this, i) + (i == 0 ? 0 : _layoutGroup.spacing));
                offset += _cellViewSizeArray[_cellViewSizeArray.Count - 1];
            }

            return offset;
        }

        /// <summary>
        /// Create a copy of the cell view sizes. This is only used in looping
        /// </summary>
        /// <param name="numberOfTimes">How many times the copy should be made</param>
        /// <param name="cellCount">How many cells to copy</param>
        private void _DuplicateCellViewSizes(int numberOfTimes, int cellCount)
        {
            for (var i = 0; i < numberOfTimes; i++)
            {
                for (var j = 0; j < cellCount; j++)
                {
                    _cellViewSizeArray.Add(_cellViewSizeArray[j] + (j == 0 ? _layoutGroup.spacing : 0));
                }
            }
        }

        /// <summary>
        /// Calculates the offset of each cell, accumulating the values from previous cells
        /// </summary>
        private void _CalculateCellViewOffsets()
        {
            _cellViewOffsetArray.Clear();
            var offset = 0f;
            for (var i = 0; i < _cellViewSizeArray.Count; i++)
            {
                offset += _cellViewSizeArray[i];
                _cellViewOffsetArray.Add(offset);
            }
        }

        /// <summary>
        /// Get a recycled cell with a given identifier if available
        /// </summary>
        /// <param name="cellPrefab">The prefab to check for</param>
        /// <returns></returns>
        private BasePresenter _GetRecycledCellView(BasePresenter cellPrefab)
        {
            for (var i = 0; i < _recycledCellViews.Count; i++)
            {
                if (_recycledCellViews[i].GOName == cellPrefab.GOName)
                {
                    // the cell view was found, so we use this recycled one.
                    // we also remove it from the recycled list
                    var cellView = _recycledCellViews.RemoveAt(i);
                    return cellView;
                }
            }

            return null;
        }

        /// <summary>
        /// This sets up the visible cells, adding and recycling as necessary
        /// </summary>
        private void _ResetVisibleCellViews()
        {
            int startIndex;
            int endIndex;

            // calculate the range of the visible cells
            _CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            // go through each previous active cell and recycle it if it no longer falls in the range
            var i = 0;
            SmallList<int> remainingCellIndices = new SmallList<int>();
            while (i < _activeCellViews.Count)
            {
                if (_activeCellViews[i].Index < startIndex || _activeCellViews[i].Index > endIndex)
                {
                    _RecycleCell(_activeCellViews[i]);
                }
                else
                {
                    // this cell index falls in the new range, so we add its
                    // index to the reusable list
                    remainingCellIndices.Add(_activeCellViews[i].Index);
                    i++;
                }
            }

            if (remainingCellIndices.Count == 0)
            {
                // there were no previous active cells remaining, 
                // this list is either brand new, or we jumped to 
                // an entirely different part of the list.
                // just add all the new cell views

                for (i = startIndex; i <= endIndex; i++)
                {
                    _AddCellView(i, ListPositionEnum.Last);
                }
            }
            else
            {
                // we are able to reuse some of the previous
                // cell views

                // first add the views that come before the 
                // previous list, going backward so that the
                // new views get added to the front
                for (i = endIndex; i >= startIndex; i--)
                {
                    if (i < remainingCellIndices.First())
                    {
                        _AddCellView(i, ListPositionEnum.First);
                    }
                }

                // next add teh views that come after the
                // previous list, going forward and adding
                // at the end of the list
                for (i = startIndex; i <= endIndex; i++)
                {
                    if (i > remainingCellIndices.Last())
                    {
                        _AddCellView(i, ListPositionEnum.Last);
                    }
                }
            }

            // update the start and end indices
            StartCellViewIndex = startIndex;
            EndCellViewIndex = endIndex;

            // adjust the padding elements to offset the cell views correctly
            _SetPadders();
        }

        /// <summary>
        /// Recycles all the active cells
        /// </summary>
        private void _RecycleAllCells()
        {
            while (_activeCellViews.Count > 0) _RecycleCell(_activeCellViews[0]);
            StartCellViewIndex = 0;
            EndCellViewIndex = 0;
        }

        /// <summary>
        /// Recycles one cell view
        /// </summary>
        /// <param name="cellView"></param>
        private void _RecycleCell(BasePresenter cellView)
        {
            cellViewWillRecycle?.Invoke(cellView);

            // remove the cell view from the active list
            _activeCellViews.Remove(cellView);

            // add the cell view to the recycled list
            _recycledCellViews.Add(cellView);

            // move the GameObject to the recycled container
            cellView.transform.SetParent(_recycledCellViewContainer);

            // reset the cellView's properties
            cellView.DataIndex = 0;
            cellView.Index = 0;

            cellViewVisibilityChanged?.Invoke(cellView);
        }

        /// <summary>
        /// Creates a cell view, or recycles if it can
        /// </summary>
        /// <param name="cellIndex">The index of the cell view</param>
        /// <param name="listPosition">Whether to add the cell to the beginning or the end</param>
        private void _AddCellView(int cellIndex, ListPositionEnum listPosition)
        {
            if (NumberOfCells == 0) return;

            // get the dataIndex. Modulus is used in case of looping so that the first set of cells are ignored
            var dataIndex = cellIndex % NumberOfCells;
            // request a cell view from the delegate
            var cellView = GetCellView(dataIndex, cellIndex);

            // set the cell's properties
            cellView.Index = cellIndex;
            cellView.DataIndex = dataIndex;

            OnRefreshCell(cellView, cellView.DataIndex, cellView.Index);

            // add the cell view to the active container
            cellView.transform.SetParent(_container, false);
            cellView.transform.localScale = Vector3.one;

            // add a layout element to the cellView
            LayoutElement layoutElement = cellView.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = cellView.gameObject.AddComponent<LayoutElement>();

            // set the size of the layout element
            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                layoutElement.minHeight = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);
                layoutElement.minWidth = BasePrefabRect.sizeDelta.x;
            }
            else
            {
                layoutElement.minWidth = _cellViewSizeArray[cellIndex] - (cellIndex > 0 ? _layoutGroup.spacing : 0);
                layoutElement.minHeight = BasePrefabRect.sizeDelta.y;
            }

            // add the cell to the active list
            if (listPosition == ListPositionEnum.First)
                _activeCellViews.AddStart(cellView);
            else
                _activeCellViews.Add(cellView);

            // set the hierarchy position of the cell view in the container
            if (listPosition == ListPositionEnum.Last)
                cellView.transform.SetSiblingIndex(_container.childCount - 2);
            else if (listPosition == ListPositionEnum.First)
                cellView.transform.SetSiblingIndex(1);

            // call the visibility change delegate if available
            cellViewVisibilityChanged?.Invoke(cellView);
        }

        /// <summary>
        /// This function adjusts the two padders that control the first cell view's
        /// offset and the overall size of each cell.
        /// </summary>
        private void _SetPadders()
        {
            if (NumberOfCells == 0) return;

            // calculate the size of each padder
            var firstSize = _cellViewOffsetArray[StartCellViewIndex] - _cellViewSizeArray[StartCellViewIndex];
            var lastSize = _cellViewOffsetArray.Last() - _cellViewOffsetArray[EndCellViewIndex];

            if (scrollDirection == ScrollDirectionEnum.Vertical)
            {
                // set the first padder and toggle its visibility
                _firstPadder.minHeight = firstSize;
                _firstPadder.gameObject.SetActive(_firstPadder.minHeight > 0);

                // set the last padder and toggle its visibility
                _lastPadder.minHeight = lastSize;
                _lastPadder.gameObject.SetActive(_lastPadder.minHeight > 0);
            }
            else
            {
                // set the first padder and toggle its visibility
                _firstPadder.minWidth = firstSize;
                _firstPadder.gameObject.SetActive(_firstPadder.minWidth > 0);

                // set the last padder and toggle its visibility
                _lastPadder.minWidth = lastSize;
                _lastPadder.gameObject.SetActive(_lastPadder.minWidth > 0);
            }
        }

        /// <summary>
        /// This function is called if the scroller is scrolled, updating the active list of cells
        /// </summary>
        private void _RefreshActive()
        {
            //_refreshActive = false;

            int startIndex;
            int endIndex;
            var velocity = Vector2.zero;

            // if looping, check to see if we scrolled past a trigger
            if (loop)
            {
                if (_scrollPosition < _loopFirstJumpTrigger)
                {
                    velocity = ScrollRect.velocity;
                    ScrollPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - _scrollPosition) + spacing;
                    ScrollRect.velocity = velocity;
                }
                else if (_scrollPosition > _loopLastJumpTrigger)
                {
                    velocity = ScrollRect.velocity;
                    ScrollPosition = _loopFirstScrollPosition + (_scrollPosition - _loopLastJumpTrigger) - spacing;
                    ScrollRect.velocity = velocity;
                }
            }

            // get the range of visibile cells
            _CalculateCurrentActiveCellRange(out startIndex, out endIndex);

            // if the index hasn't changed, ignore and return
            if (startIndex == StartCellViewIndex && endIndex == EndCellViewIndex) return;

            // recreate the visibile cells
            _ResetVisibleCellViews();
        }

        /// <summary>
        /// Determines which cells can be seen
        /// </summary>
        /// <param name="startIndex">The index of the first cell visible</param>
        /// <param name="endIndex">The index of the last cell visible</param>
        private void _CalculateCurrentActiveCellRange(out int startIndex, out int endIndex)
        {
            startIndex = 0;
            endIndex = 0;

            // get the positions of the scroller
            var startPosition = _scrollPosition;
            var endPosition = _scrollPosition + (scrollDirection == ScrollDirectionEnum.Vertical ? _scrollRectTransform.rect.height : _scrollRectTransform.rect.width);

            // calculate each index based on the positions
            startIndex = GetCellViewIndexAtPosition(startPosition);
            endIndex = GetCellViewIndexAtPosition(endPosition);
        }

        /// <summary>
        /// Gets the index of a cell at a given position based on a subset range.
        /// This function uses a recursive binary sort to find the index faster.
        /// </summary>
        /// <param name="position">The pixel offset from the start of the scroller</param>
        /// <param name="startIndex">The first index of the range</param>
        /// <param name="endIndex">The last index of the rnage</param>
        /// <returns></returns>
        private int _GetCellIndexAtPosition(float position, int startIndex, int endIndex)
        {
            // if the range is invalid, then we found our index, return the start index
            if (startIndex >= endIndex) return startIndex;

            // determine the middle point of our binary search
            var middleIndex = (startIndex + endIndex) / 2;

            // if the middle index is greater than the position, then search the last
            // half of the binary tree, else search the first half
            if ((_cellViewOffsetArray[middleIndex] + (scrollDirection == ScrollDirectionEnum.Vertical ? padding.top : padding.left)) >= position)
                return _GetCellIndexAtPosition(position, startIndex, middleIndex);
            else
                return _GetCellIndexAtPosition(position, middleIndex + 1, endIndex);
        }

        /// <summary>
        /// Handler for when the scroller changes value
        /// </summary>
        /// <param name="val">The scroll rect's value</param>
        private void _ScrollRect_OnValueChanged(Vector2 val)
        {
            // set the internal scroll position
            if (scrollDirection == ScrollDirectionEnum.Vertical)
                _scrollPosition = (1f - val.y) * ScrollSize;
            else
                _scrollPosition = val.x * ScrollSize;
            //_refreshActive = true;
            _scrollPosition = Mathf.Clamp(_scrollPosition, 0, GetScrollPositionForCellViewIndex(_cellViewSizeArray.Count - 1, CellViewPositionEnum.Before));

            // call the handler if it exists
            scrollerScrolled?.Invoke(this, val, _scrollPosition);

            // if the snapping is turned on, handle it
            if (snapping && !_snapJumping)
            {
                // if the speed has dropped below the threshhold velocity
                if (Mathf.Abs(LinearVelocity) <= snapVelocityThreshold && LinearVelocity != 0)
                {
                    // Make sure the scroller is not on the boundary if not looping
                    var normalized = NormalizedScrollPosition;
                    if (loop || (!loop && normalized > 0 && normalized < 1.0f))
                    {
                        // Call the snap function
                        Snap();
                    }
                }
            }

            _RefreshActive();

            if(Scrollbar && IsFixedScrollBar)
                Scrollbar.size = ScrollBarSize;
        }

        /// <summary>
        /// This is fired by the tweener when the snap tween is completed
        /// </summary>
        private void SnapJumpComplete()
        {
            // reset the snap jump to false and restore the inertia state
            _snapJumping = false;
            ScrollRect.inertia = _snapInertia;

            BasePresenter cellView = null;
            for (var i = 0; i < _activeCellViews.Count; i++)
            {
                if (_activeCellViews[i].DataIndex == _snapDataIndex)
                {
                    cellView = _activeCellViews[i];
                    break;
                }
            }

            // fire the scroller snapped delegate
            scrollerSnapped?.Invoke(this, _snapCellViewIndex, _snapDataIndex, cellView);
        }

        #endregion

        #region Tweening

        /// <summary>
        /// The easing type
        /// </summary>
        public enum TweenType
        {
            immediate,
            linear,
            spring,
            easeInQuad,
            easeOutQuad,
            easeInOutQuad,
            easeInCubic,
            easeOutCubic,
            easeInOutCubic,
            easeInQuart,
            easeOutQuart,
            easeInOutQuart,
            easeInQuint,
            easeOutQuint,
            easeInOutQuint,
            easeInSine,
            easeOutSine,
            easeInOutSine,
            easeInExpo,
            easeOutExpo,
            easeInOutExpo,
            easeInCirc,
            easeOutCirc,
            easeInOutCirc,
            easeInBounce,
            easeOutBounce,
            easeInOutBounce,
            easeInBack,
            easeOutBack,
            easeInOutBack,
            easeInElastic,
            easeOutElastic,
            easeInOutElastic
        }

        private float _tweenTimeLeft;

        /// <summary>
        /// Moves the scroll position over time between two points given an easing function. When the
        /// tween is complete it will fire the jumpComplete delegate.
        /// </summary>
        /// <param name="tweenType">The type of easing to use</param>
        /// <param name="time">The amount of time to interpolate</param>
        /// <param name="start">The starting scroll position</param>
        /// <param name="end">The ending scroll position</param>
        /// <param name="jumpComplete">The action to fire when the tween is complete</param>
        /// <returns></returns>
        IEnumerator TweenPosition(TweenType tweenType, float time, float start, float end, Action tweenComplete)
        {
            if (tweenType == TweenType.immediate || time == 0)
            {
                // if the easing is immediate or the time is zero, just jump to the end position
                ScrollPosition = end;
            }
            else
            {
                // zero out the velocity
                ScrollRect.velocity = Vector2.zero;

                // fire the delegate for the tween start
                IsTweening = true;
                if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, true);

                _tweenTimeLeft = 0;
                var newPosition = 0f;

                // while the tween has time left, use an easing function
                while (_tweenTimeLeft < time)
                {
                    switch (tweenType)
                    {
                        case TweenType.linear: newPosition = linear(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.spring: newPosition = spring(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInQuad: newPosition = easeInQuad(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutQuad: newPosition = easeOutQuad(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutQuad: newPosition = easeInOutQuad(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInCubic: newPosition = easeInCubic(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutCubic: newPosition = easeOutCubic(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutCubic: newPosition = easeInOutCubic(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInQuart: newPosition = easeInQuart(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutQuart: newPosition = easeOutQuart(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutQuart: newPosition = easeInOutQuart(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInQuint: newPosition = easeInQuint(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutQuint: newPosition = easeOutQuint(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutQuint: newPosition = easeInOutQuint(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInSine: newPosition = easeInSine(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutSine: newPosition = easeOutSine(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutSine: newPosition = easeInOutSine(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInExpo: newPosition = easeInExpo(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutExpo: newPosition = easeOutExpo(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutExpo: newPosition = easeInOutExpo(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInCirc: newPosition = easeInCirc(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutCirc: newPosition = easeOutCirc(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutCirc: newPosition = easeInOutCirc(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInBounce: newPosition = easeInBounce(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutBounce: newPosition = easeOutBounce(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutBounce: newPosition = easeInOutBounce(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInBack: newPosition = easeInBack(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutBack: newPosition = easeOutBack(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutBack: newPosition = easeInOutBack(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInElastic: newPosition = easeInElastic(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeOutElastic: newPosition = easeOutElastic(start, end, (_tweenTimeLeft / time)); break;
                        case TweenType.easeInOutElastic: newPosition = easeInOutElastic(start, end, (_tweenTimeLeft / time)); break;
                    }

                    if (loop)
                    {
                        // if we are looping, we need to make sure the new position isn't past the jump trigger.
                        // if it is we need to reset back to the jump position on the other side of the area.

                        if (end > start && newPosition > _loopLastJumpTrigger)
                        {
                            //Debug.Log("name: " + name + " went past the last jump trigger, looping back around");
                            newPosition = _loopFirstScrollPosition + (newPosition - _loopLastJumpTrigger);
                        }
                        else if (start > end && newPosition < _loopFirstJumpTrigger)
                        {
                            //Debug.Log("name: " + name + " went past the first jump trigger, looping back around");
                            newPosition = _loopLastScrollPosition - (_loopFirstJumpTrigger - newPosition);
                        }
                    }

                    // set the scroll position to the tweened position
                    ScrollPosition = newPosition;

                    // increase the time elapsed
                    _tweenTimeLeft += Time.unscaledDeltaTime;

                    yield return null;
                }

                // the time has expired, so we make sure the final scroll position
                // is the actual end position.
                ScrollPosition = end;
            }

            // the tween jump is complete, so we fire the delegate
            if (tweenComplete != null) tweenComplete();

            // fire the delegate for the tween ending
            IsTweening = false;
            if (scrollerTweeningChanged != null) scrollerTweeningChanged(this, false);
        }


        private float linear(float start, float end, float val)
        {
            return Mathf.Lerp(start, end, val);
        }

        private static float spring(float start, float end, float val)
        {
            val = Mathf.Clamp01(val);
            val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) * (1f + (1.2f * (1f - val)));
            return start + (end - start) * val;
        }

        private static float easeInQuad(float start, float end, float val)
        {
            end -= start;
            return end * val * val + start;
        }

        private static float easeOutQuad(float start, float end, float val)
        {
            end -= start;
            return -end * val * (val - 2) + start;
        }

        private static float easeInOutQuad(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val + start;
            val--;
            return -end / 2 * (val * (val - 2) - 1) + start;
        }

        private static float easeInCubic(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val + start;
        }

        private static float easeOutCubic(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val + 1) + start;
        }

        private static float easeInOutCubic(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val + 2) + start;
        }

        private static float easeInQuart(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val + start;
        }

        private static float easeOutQuart(float start, float end, float val)
        {
            val--;
            end -= start;
            return -end * (val * val * val * val - 1) + start;
        }

        private static float easeInOutQuart(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val + start;
            val -= 2;
            return -end / 2 * (val * val * val * val - 2) + start;
        }

        private static float easeInQuint(float start, float end, float val)
        {
            end -= start;
            return end * val * val * val * val * val + start;
        }

        private static float easeOutQuint(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * (val * val * val * val * val + 1) + start;
        }

        private static float easeInOutQuint(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * val * val * val * val * val + start;
            val -= 2;
            return end / 2 * (val * val * val * val * val + 2) + start;
        }

        private static float easeInSine(float start, float end, float val)
        {
            end -= start;
            return -end * Mathf.Cos(val / 1 * (Mathf.PI / 2)) + end + start;
        }

        private static float easeOutSine(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Sin(val / 1 * (Mathf.PI / 2)) + start;
        }

        private static float easeInOutSine(float start, float end, float val)
        {
            end -= start;
            return -end / 2 * (Mathf.Cos(Mathf.PI * val / 1) - 1) + start;
        }

        private static float easeInExpo(float start, float end, float val)
        {
            end -= start;
            return end * Mathf.Pow(2, 10 * (val / 1 - 1)) + start;
        }

        private static float easeOutExpo(float start, float end, float val)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * val / 1) + 1) + start;
        }

        private static float easeInOutExpo(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return end / 2 * Mathf.Pow(2, 10 * (val - 1)) + start;
            val--;
            return end / 2 * (-Mathf.Pow(2, -10 * val) + 2) + start;
        }

        private static float easeInCirc(float start, float end, float val)
        {
            end -= start;
            return -end * (Mathf.Sqrt(1 - val * val) - 1) + start;
        }

        private static float easeOutCirc(float start, float end, float val)
        {
            val--;
            end -= start;
            return end * Mathf.Sqrt(1 - val * val) + start;
        }

        private static float easeInOutCirc(float start, float end, float val)
        {
            val /= .5f;
            end -= start;
            if (val < 1) return -end / 2 * (Mathf.Sqrt(1 - val * val) - 1) + start;
            val -= 2;
            return end / 2 * (Mathf.Sqrt(1 - val * val) + 1) + start;
        }

        private static float easeInBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            return end - easeOutBounce(0, end, d - val) + start;
        }

        private static float easeOutBounce(float start, float end, float val)
        {
            val /= 1f;
            end -= start;
            if (val < (1 / 2.75f))
            {
                return end * (7.5625f * val * val) + start;
            }
            else if (val < (2 / 2.75f))
            {
                val -= (1.5f / 2.75f);
                return end * (7.5625f * (val) * val + .75f) + start;
            }
            else if (val < (2.5 / 2.75))
            {
                val -= (2.25f / 2.75f);
                return end * (7.5625f * (val) * val + .9375f) + start;
            }
            else
            {
                val -= (2.625f / 2.75f);
                return end * (7.5625f * (val) * val + .984375f) + start;
            }
        }

        private static float easeInOutBounce(float start, float end, float val)
        {
            end -= start;
            float d = 1f;
            if (val < d / 2) return easeInBounce(0, end, val * 2) * 0.5f + start;
            else return easeOutBounce(0, end, val * 2 - d) * 0.5f + end * 0.5f + start;
        }

        private static float easeInBack(float start, float end, float val)
        {
            end -= start;
            val /= 1;
            float s = 1.70158f;
            return end * (val) * val * ((s + 1) * val - s) + start;
        }

        private static float easeOutBack(float start, float end, float val)
        {
            float s = 1.70158f;
            end -= start;
            val = (val / 1) - 1;
            return end * ((val) * val * ((s + 1) * val + s) + 1) + start;
        }

        private static float easeInOutBack(float start, float end, float val)
        {
            float s = 1.70158f;
            end -= start;
            val /= .5f;
            if ((val) < 1)
            {
                s *= (1.525f);
                return end / 2 * (val * val * (((s) + 1) * val - s)) + start;
            }
            val -= 2;
            s *= (1.525f);
            return end / 2 * ((val) * val * (((s) + 1) * val + s) + 2) + start;
        }

        private static float easeInElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;
            val = val / d;
            if (val == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }
            val = val - 1;
            return -(a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
        }

        private static float easeOutElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;

            val = val / d;
            if (val == 1) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            return (a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) + end + start);
        }

        private static float easeInOutElastic(float start, float end, float val)
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s = 0;
            float a = 0;

            if (val == 0) return start;

            val = val / (d / 2);
            if (val == 2) return start + end;

            if (a == 0f || a < Mathf.Abs(end))
            {
                a = end;
                s = p / 4;
            }
            else
            {
                s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
            }

            if (val < 1)
            {
                val = val - 1;
                return -0.5f * (a * Mathf.Pow(2, 10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p)) + start;
            }
            val = val - 1;
            return a * Mathf.Pow(2, -10 * val) * Mathf.Sin((val * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
        }

        #endregion
    }

}