using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
namespace CYM.UI
{
    public enum ViewLevel
    {
        Root,//根界面
        Main,//主界面
        Sub,//子界面
    }
    public class BaseView : BaseCoreMono
    {
        #region Callback Value
        public Callback Callback_OnClose { get; set; }
        public Callback<BaseView, bool> Callback_OnOpen { get; set; }
        #endregion

        #region 内部
        public BaseView ParentView { get; private set; }
        public BaseView RootView { get; private set; }
        #endregion

        #region 公共属性
        /// <summary>
        /// 0意味着不在任何的组里面
        /// </summary>
        public int Group = 0;
        /// <summary>
        /// 默认是打开还是显示
        /// </summary>
        public bool DefaultShow = false;
        /// <summary>
        /// GO 是否根据界面IsShow变量 自动Active
        /// </summary>
        public bool IsActiveByShow = true;
        public bool IsShow { get; protected set; }
        public bool IsScale = true;
        public Ease InEase = Ease.OutBack;
        public Ease OutEase = Ease.InBack;
        public float InTime = 0.3f;
        public float OutTime = 0.3f;
        public Canvas Canvas { get; protected set; }
        public RectTransform RectTrans { get; private set; }
        public RectTransform CanvasTrans { get; private set; }
        public Camera WorldCamera { get { return Canvas.worldCamera; }  }
        #endregion

        #region 内部
        protected TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> alphaTween;
        protected Tweener scaleTween;
        protected bool IsDirty { get; set; } = false;
        protected Vector3 sourceLocalPos;
        #endregion

        #region other
        /// <summary>
        /// 界面等级
        /// </summary>
        public ViewLevel ViewLevel { get; set; } = ViewLevel.Main;
        /// <summary>
        /// 子界面的集合
        /// </summary>
        protected List<BaseView> SubViews { get; private set; } = new List<BaseView>();
        /// <summary>
        /// 界面所在的UI管理器
        /// </summary>
        public BaseUIMgr UIMgr { get; set; }
        #endregion

        #region life
        public override LayerData LayerData => BaseConstMgr.Layer_UI;
        public override void Awake()
        {
            MonoType = MonoType.View;
            base.Awake();
            Canvas = GetComponentInChildren<Canvas>();
            RectTrans = GetComponent<RectTransform>();
            if (Canvas != null)
                CanvasTrans = Canvas.transform as RectTransform;
            IsShow = true;
            sourceLocalPos = Trans.localPosition;
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        /// <summary>
        /// 将界面挂到其他界面下
        /// </summary>
        public virtual void Attach(BaseView parentView, BaseView rootView, Object mono)
        {
            this.ParentView = parentView;
            this.RootView = rootView;

        }
        public override void OnEnable()
        {
            base.OnEnable();
            if (SelfBaseGlobal == null)
                return;
        }
        /// <summary>
        /// 销毁UI
        /// </summary>
        public virtual void Destroy()
        {
            SelfBaseGlobal.LangMgr.Callback_OnSwitchLanguage -= OnSwitchLanguage;
            foreach (var item in SubViews)
                item.Destroy();
            Destroy(GO);
            Callback_OnClose = null;
            Callback_OnOpen = null;
        }
        public override void Start()
        {
            base.Start();
            OnCreatedView();
            FetchPresenters();
            ShowDefault();
        }
        /// <summary>
        /// 获取控件
        /// </summary>
        protected virtual void FetchPresenters()
        {
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (IsDirty)
            {
                Refresh();
                IsDirty = false;
            }
        }
        #endregion

        #region set
        public virtual void AutoSetup()
        {
        }
        public virtual void SetDirty()
        {
            IsDirty = true;
        }
        /// <summary>
        /// 创建子界面
        /// </summary>
        public virtual T CreateSubView<T>(string path) where T : BaseView
        {
            GameObject tempGo = null;
            var tempPrefab = SelfBaseGlobal.GRMgr.GetUI(path);
            if (tempPrefab == null)
                CLog.Error("没有这个prefab:" + path);
            tempGo = Object.Instantiate(tempPrefab);
            T tempUI = null;
            if (tempGo != null)
            {
                tempUI = tempGo.GetComponent<T>();
                if (tempUI == null)
                {
                    CLog.Error("无法获取组建:" + typeof(T).Name + " Error path=" + path);
                    return null;
                }
                tempUI.ViewLevel = ViewLevel.Sub;
                tempUI.UIMgr = UIMgr;
                tempUI.Attach(this, RootView, SelfBaseGlobal);
                //移动到父节点下面
                tempUI.Trans.SetSiblingIndex(Trans.GetSiblingIndex()+SubViews.Count+1);
                SubViews.Add(tempUI);
            }

            return tempUI;
        }
        /// <summary>
        /// 回到默认的开启状态
        /// </summary>
        public void ShowDefault()
        {
            if (!DefaultShow)
                Show(false, 0.0f, 0, false, true);
            else
                Show(true, 0.0f, 0, false, true);
        }
        /// <summary>
        /// 显示或者关闭界面
        /// </summary>
        /// <param name="b"></param>
        public virtual void Show(bool b=true, float? fadeTime = null, float delay = 0, bool useGroup = true, bool force = false)
        {
        }

        //sugar
        public void Show(bool b)
        {
            Show(b, null);
        }

        public void Toggle()
        {
            Show(!IsShow);
        }

        /// <summary>
        /// 刷新多语言，以及其他显示
        /// </summary>
        public virtual void Refresh()
        {
        }
        public virtual void Enable(bool b)
        {
            IsEnable = b;

        }
        public virtual void Interactable(bool b)
        {
        }
        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isLoop"></param>
        /// <returns></returns>
        protected AudioSource PlayAudio(string id, bool isLoop = false)
        {
            return SelfBaseGlobal.AudioMgr.PlayUI(id, isLoop);
        }
        protected void PlayAudio(string[] id)
        {
            SelfBaseGlobal.AudioMgr.PlayUI(BaseMathUtils.RandArray(id), false);
        }
        #endregion

        #region get
        /// <summary>
        /// 获得翻译
        /// </summary>
        /// <param name="key"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public string GetStr(string key, params object[] objs)
        {
            return BaseLanguageMgr.Get(key, objs);
        }
        /// <summary>
        /// 获得条件描述
        /// </summary>
        /// <returns></returns>
        public string GetACDesc()
        {
            return SelfBaseGlobal.ACM.GetCacheDesc();
        }
        /// <summary>
        /// 获得消耗描述
        /// </summary>
        /// <returns></returns>
        public string GetACCost()
        {
            return SelfBaseGlobal.ACM.GetCacheCost();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否为根界面
        /// </summary>
        public bool IsRootView
        {
            get { return ViewLevel == ViewLevel.Root; }
        }
        #endregion

        #region Callback
        protected virtual void OnCreatedView()
        {
            //绑定事件
            SelfBaseGlobal.LangMgr.Callback_OnSwitchLanguage += OnSwitchLanguage;
        }
        protected virtual void OnFadeIn()
        {

        }
        protected virtual void OnFadeOut()
        {
            if (GO == null)
                return;
            Callback_OnClose?.Invoke();
            if (IsActiveByShow)
                GO.SetActive(false);
        }
        protected virtual void OnOpen(BaseView baseView, bool useGroup)
        {
            if (GO == null)
                return;
            if (IsActiveByShow)
                GO.SetActive(true);
            Callback_OnOpen?.Invoke(baseView, useGroup);
        }
        protected virtual void OnClose()
        {

        }
        protected virtual void OnSwitchLanguage()
        {
            SetDirty();
        }
        /// <summary>
        /// 显示状态发生变化的时候调用
        /// </summary>
        protected virtual void OnShow()
        {

        }
        #endregion

    }

}