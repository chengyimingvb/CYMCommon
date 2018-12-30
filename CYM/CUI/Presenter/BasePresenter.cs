using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData;
using System;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BasePresenter: UIBehaviour
    {
        public virtual void SetDirty() { }
        public virtual void SetActive(bool b) { }
        public virtual void Show(bool b,bool isForce=false) { }
        public virtual void Refresh() { }
        public virtual void OnFixedUpdate() { }
        public virtual void Cleanup() { }
        public virtual void SetIndex(int i) { }
        public virtual void OnInteractable(bool b) { }
        public virtual void OnSelected(bool b) { }
        public virtual void ShowByPanel(bool isDefault = false) { }
        public virtual void Toggle() { }
        public virtual void OnViewShow(bool b) { }

        /// <summary>
        /// 属否自动刷新
        /// </summary>
        public bool IsAutoRefresh { get; protected set; }
        /// <summary>
        /// 是否已经调用了Show函数
        /// </summary>
        public bool IsInitedShow { get; set; }
        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool IsInited { get; set; }
        /// <summary>
        /// 是否显示,默认为null,表示显示状态未知
        /// </summary>
        public bool IsShow { get; set; }
        public bool IsInteractable { get; protected set; } = true;
        public bool IsSelected { get; protected set; } = false;
        /// <summary>
        /// 子集presenter
        /// </summary>
        public bool IsSubPresenter { set; get; } = false;
        /// <summary>
        /// dirty
        /// </summary>
        public bool IsDirty { get; protected set; }
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { get; set; } = 0;
        /// <summary>
        /// 数据索引
        /// </summary>
        public int DataIndex { get; set; } = 0;
        /// <summary>
        /// 是否需要Update
        /// </summary>
        public virtual bool NeedUpdate { get; } = false;

        [Tooltip("是否可以点击,优先级低")]
        public bool IsCanClick = false;
        [Tooltip("是否默认为显示状态")]
        public bool IsShowDefault = true;
        [Tooltip("是否Active通过打开/关闭")]
        public bool IsActiveByShow=true;

        public PresenterGroup OwnerdPresenterGroup { get; set; }
        public RectTransform RectTrans { get; protected set; }
        public GameObject GO { get;protected set; }
        public BaseUIView BaseUIView { get; set; }
        public virtual string Name { get; }
    }

    public class PresenterData
    {
        public UIBehaviour Presenter;
        //触发
        public Callback<BasePresenter> OnEnter;
        public Callback<BasePresenter> OnExit;
        public Callback<BasePresenter, PointerEventData> OnClick;
        public Callback<BasePresenter, PointerEventData> OnDown;
        public Callback<BasePresenter, PointerEventData> OnUp;
        public Callback<BasePresenter, bool> OnInteractable;
        public Callback<BasePresenter, bool> OnSelected;
        public Callback<bool> OnShow;

        public Callback<BasePresenter, PointerEventData> OnBeginDrag;
        public Callback<BasePresenter, PointerEventData> OnEndDrag;
        public Callback<BasePresenter, PointerEventData> OnDrag;

        //判断
        public Func<int,bool> IsInteractable = (i) => { return true; };
        public Func<int, bool> IsSelected = (i) => { return false; };

        //值
        public string ClickClip { get; set; }=  "UI_Tabclick";
        public string HoverClip { get; set; } = "";
        public string OpenClip { get; set; } = "";
        public string CloseClip { get; set; } = "";

        //默认为null
        public Func<int, bool> IsCanClick;
        public Func<int, bool> IsShow;
    }

    public class Presenter<TData> : BasePresenter, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler where TData: PresenterData,new() 
    {
        #region Callback Val
        //控件事件响应,可以直接赋值
        public TData Data { get; protected set; } = new TData();
        #endregion

        #region prop
        public HashList<BasePresenter> Children { get; set; } = new HashList<BasePresenter>();
        public Vector3 Pos { get { return RectTrans.anchoredPosition3D; } set { RectTrans.anchoredPosition3D = value; } }
        public Transform Trans { get; private set; }
        protected UITransition[] Transitions;
        protected UIShowTransition[] ShowTransitions;
        protected Text[] Texts;
        protected BaseGlobal SelfBaseGlobal { get; set; }
        protected UIConfig UIConfig => UIConfig.Ins;
        #endregion

        #region get
        public bool GetIsCanClick()
        {
            if (Data.IsCanClick != null)
            {
                if (Data.IsCanClick.Invoke(Index))
                    return true;
            }
            else
            {
                if (IsCanClick)
                    return true;
                if (Data == null)
                    return true;
            }
            return false;
        }
        public Font GetFontByLanguage()
        {
            LanguageType type = SelfBaseGlobal.LangMgr.CurLangType;
            if (UIConfig.Fonts.ContainsKey(type))
            {
                return UIConfig.Fonts[type];
            }
            else
            {
                return UIConfig.DefaultFont;
            }
        }
        #endregion

        #region life
        protected override void Awake()
        {
            SelfBaseGlobal = BaseGlobal.Ins;
            base.Awake();
            RectTrans = transform as RectTransform;
            Trans = transform;
            GO = gameObject;
            if (RectTrans.localScale == Vector3.zero)
            {
                CLog.Error("错误:scale 是 0 error presenter:" + name);
            }
            GO.layer = (int)BaseConstMgr.Layer_UI;
            //作为顶级Pressenter可以获取自身层级下面的子对象,子对象随着父对象自动刷新
            var childPresenters= GO.GetComponentsInChildren<BasePresenter>();
            if(childPresenters!=null)
            {
                foreach (var item in childPresenters)
                    AddChild(item);
            }
            Transitions = GO.GetComponentsInChildren<UITransition>(true);
            ShowTransitions = GO.GetComponentsInChildren<UIShowTransition>(true);
            Texts = GO.GetComponentsInChildren<Text>(true);
            if (Texts != null && UIConfig.EnableSharpText && SelfBaseGlobal!=null)
            {
                foreach (var item in Texts)
                {
                    if(item!=null)
                        item.material = SelfBaseGlobal.GRMgr.FontRendering;
                }
            }
            ShowDefault();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnDestroy()
        {
            Cleanup();
            base.OnDestroy();
        }
        
        #endregion

        #region set
        /// <summary>
        /// 添加自对象
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(BasePresenter child,bool force=false)
        {
            if (!force&&this.IsSubPresenter)
            {
                //作为子对象不能在拥有自己的子对象,除了BaseDupplicate
                return;
            }
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (child == this)
                return;
            if (child.IsSubPresenter)
                return;
            child.IsSubPresenter = true;
            Children.Add(child);
        }
        /// <summary>
        /// 设置index
        /// </summary>
        /// <param name="i"></param>
        public override void SetIndex(int i)
        {
            Index = i;
            foreach (var child in Children)
            {
                child.SetIndex(Index);
            }
        }
        /// <summary>
        /// 打开关闭
        /// </summary>
        /// <param name="b"></param>
        public override void Show(bool b,bool isForce=false)
        {
            if (IsInitedShow)
            {
                if (IsShow == b&&!isForce)
                    return;
                OnShow(b);
            }
            IsInitedShow = true;
            IsShow = b;
  
            if (b)
            {
                if(IsAutoRefresh)
                    Refresh();
            }
            //如果presenter 带了 UITranslate 那么等tween完了以后在设置SetActive
            if (ShowTransitions != null&& ShowTransitions.Length>0)
            {
                foreach (var item in ShowTransitions)
                    item.OnShow(b,IsActiveByShow);
            }
            //没有带presenter 直接设置 SetActive
            else
            {
                if (IsActiveByShow)
                    SetActive(b);
            }
        }
        public override void Toggle()
        {
            Show(!IsShow);
        }
        public override void SetActive(bool b)
        {
            if (GO.activeSelf == b)
                return;
            GO.SetActive(b);
        }
        /// <summary>
        /// 显示默认状态
        /// </summary>
        public virtual void ShowDefault()
        {
            if (IsInitedShow)
            {
                if (IsShow == IsShowDefault)
                    return;
            }
            Show(IsShowDefault);
        }
        /// <summary>
        /// 通过panel打开
        /// </summary>
        public override void ShowByPanel(bool isDefault=false)
        {
            if(OwnerdPresenterGroup==null)
            {
                CLog.Error("这个presenter 没有挂在任何一个panel下面,请使用Show代替");
                return;
            }
            if (isDefault)
                OwnerdPresenterGroup.ShowDefault();
            else
                OwnerdPresenterGroup.ShowPanel(this);
        }
        /// <summary>
        /// 初始化,使用这个函数初始化,将会通过View或者父级Pressenter的Refresh自动刷新
        /// </summary>
        /// <param name="data"></param>
        public virtual void Init(TData data)
        {
            if (IsInited)
            {
                CLog.Error("一个Presenter 不能初始化2次");
                return;
            }
            if (data == null)
            {
                CLog.Error("Presenter 的 data为空!!");
                return;
            }
            Data = data;
            Data.Presenter = this;
            IsAutoRefresh = true;
            IsInited = true;
        }
        public virtual void CancleInit()
        {
            if (!IsInited)
            {
                CLog.Error("一个Presenter 不能在没有初始化的时候取消初始化");
                return;
            }
            Data = new TData();
            Data.Presenter = null;
            IsAutoRefresh = false;
            IsInited = false;
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public override void Refresh()
        {
            if (Data.IsShow != null)
                Show(Data.IsShow.Invoke(Index));
            if (IsShow)
            {
                foreach (var child in Children)
                {
                    if (child.IsShow && child.IsAutoRefresh)
                        child.Refresh();
                }
                SetInteractable(Data.IsInteractable.Invoke(Index));
                SetSelected(Data.IsSelected.Invoke(Index));

                Font newFont = GetFontByLanguage();
                foreach (var item in Texts)
                {
                    if (item != null&&item.font!= newFont)
                        item.font = newFont;
                }
            }
            IsDirty = false;

        }
        /// <summary>
        /// 清除
        /// </summary>
        public override void Cleanup()
        {
            IsInited = false;
            Data = null;
            foreach (var child in Children)
            {
                child.Cleanup();
            }
            Children.Clear();
        }
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip"></param>
        protected AudioSource PlayClip(string clip,bool isLoop=false)
        {
            if (clip.IsInvStr())
            {
                return null;
            }
            return BaseGlobal.Ins.AudioMgr.PlayUI(clip, isLoop);
        }
        /// <summary>
        /// 设置点击状态
        /// </summary>
        public void SetInteractable(bool b)
        {
            if (IsInteractable == b)
                return;
            IsInteractable = b;
            OnInteractable(b);
            if(Transitions!=null)
            {
                foreach (var item in Transitions)
                    item.OnInteractable(b);
            }
        }
        /// <summary>
        /// 设置选择状态
        /// </summary>
        /// <param name="b"></param>
        public void SetSelected(bool b)
        {
            if (IsSelected == b)
                return;
            IsSelected = b;
            OnSelected(b);
            if (Transitions != null)
            {
                foreach (var item in Transitions)
                    item.OnSelected(b);
            }
        }
        /// <summary>
        /// 设置Dirty
        /// </summary>
        public override void SetDirty()
        {
            IsDirty = true;
            BaseUIView.ActivePresenterUpdate(this);
        }
        #endregion

        #region get
        /// <summary>
        /// 空间的路径
        /// </summary>
        public string Path
        {
            get
            {
                return BaseUIUtils.GetPath(GO);
            }
        }
        /// <summary>
        /// 名称
        /// </summary>
        public override string Name
        {
            get
            {
                return gameObject.name;
            }
        }
        /// <summary>
        /// 获得翻译
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        protected string GetStr(string key, params object[] ps)
        {
            return BaseLanguageMgr.Get(key, ps);
        }
        /// <summary>
        /// 获得某个组建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetOrAddComponent<T>() where T : Component
        {
            T c = GetComponent<T>();
            if (c == null)
            {
                c = gameObject.AddComponent<T>();
            }
            return c;
        }
        /// <summary>
        /// 是否Active
        /// </summary>
        /// <returns></returns>
        public bool GetIsActiveByShow()
        {
            return IsActiveByShow;
        }
        #endregion

        #region callback
        /// <summary>
        /// 鼠标进入
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Data?.OnEnter?.Invoke(this);

            if (!GetIsCanClick())
                return;
            if (!IsInteractable)
                return;
            if (!Data.HoverClip.IsInvStr())
                PlayClip(Data.HoverClip);
        }
        /// <summary>
        /// 鼠标退出
        /// </summary>
        /// <param name="eventData"></param>
		public virtual void OnPointerExit(PointerEventData eventData)
        {
            Data?.OnExit?.Invoke(this);
            if (Data != null && Data.OnEnter != null)
            {
                BaseTooltipView.CloseAllTip();
            }
        }
        /// <summary>
        /// 鼠标点击
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!GetIsCanClick())
                return;
            if (!IsInteractable)
                return;

            Data?.OnClick?.Invoke(this, eventData);

            if (!Data.ClickClip.IsInvStr())
                PlayClip(Data.ClickClip);

        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!GetIsCanClick())
                return;
            if (!IsInteractable)
                return;
            Data?.OnDown?.Invoke(this, eventData);
        }
        /// <summary>
        /// 鼠标按下
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (!GetIsCanClick())
                return;
            if (!IsInteractable)
                return;
            Data?.OnUp?.Invoke(this, eventData);
        }
        /// <summary>
        /// 点击状态变化
        /// </summary>
        /// <param name="b"></param>
        public override void OnInteractable(bool b)
        {
            Data?.OnInteractable?.Invoke(this,b);
        }

        public override void OnSelected(bool b)
        {
            Data?.OnSelected?.Invoke(this, b);

            if (!Data.HoverClip.IsInvStr())
                PlayClip(Data.HoverClip);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Data?.OnBeginDrag?.Invoke(this, eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Data?.OnEndDrag?.Invoke(this, eventData);
        }
        public virtual void OnDrag(PointerEventData eventData)
        {
            Data?.OnDrag?.Invoke(this, eventData);
        }
        public virtual void OnShow(bool isShow)
        {
            if (isShow)
            {
                PlayClip(Data?.OpenClip);
            }
            else
            {
                PlayClip(Data?.CloseClip);
            }
            Data?.OnShow?.Invoke(isShow);
        }
        #endregion

        #region editor

        /// <summary>
        /// 自动设置
        /// </summary>
        public virtual void AutoSetup()
        {
        }
        #endregion

    }

}