using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
using Sirenix.OdinInspector;

namespace CYM.UI
{
    public class BaseUIView : BaseView
    {
        #region Inspector
        [FoldoutGroup("Inspector"),SerializeField]
        protected BaseButton BntClose;
        [FoldoutGroup("Inspector"), SerializeField]
        protected BaseText Title;
        #endregion

        #region 内部
        protected HashList<BasePresenter> updatePresenters { get; set; } = new HashList<BasePresenter>();
        protected List<BasePresenter> presenters { get; set; } = new List<BasePresenter>();
        protected Graphic[] graphics { get; set; }
        protected Vector3 sourceAnchoredPosition3D;
        protected Vector3 sourceLocalScale;
        protected Vector2 sourceAnchorMax;
        protected Vector2 sourceAnchorMin;
        protected Vector2 sourceAnchoredPosition;
        protected Vector2 sourceSizeData;
        protected CanvasGroup canvasGroup { get; private set; }
        protected BaseScroll[] Scrolls;
        protected IBaseScreenMgr BaseScreenMgr => SelfBaseGlobal.ScreenMgr;
        #endregion

        #region panel
        /// <summary>
        /// 子界面容器
        /// </summary>
        protected PresenterGroup PGMain { get; private set; }
        /// <summary>
        /// PG列表
        /// </summary>
        protected List<PresenterGroup> PGList { get; private set; } = new List<PresenterGroup>();
        /// <summary>
        /// 添加一组panelGroup
        /// 第一个将自动分配给PGMain
        /// </summary>
        /// <param name="panelGroupItems"></param>
        /// <returns></returns>
        public PresenterGroup AddPresenterGroup(params BasePresenter[] presenters)
        {
            var temp = new PresenterGroup(new List<BasePresenter>(presenters));
            if (PGMain == null)
                PGMain = temp;
            PGList.Add(temp);
            
            return temp;
        }
        /// <summary>
        /// 移除所有panel
        /// </summary>
        void RemoveAllPanelGroup()
        {
            foreach (var item in PGList)
            {
                item.Presenters.Clear();
            }
            PGList.Clear();
        }
        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            graphics = GetComponentsInChildren<Graphic>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null && RectTrans != null)
            {
                canvasGroup = GO.AddComponent<CanvasGroup>();
            }
            sourceAnchorMax = RectTrans.anchorMax;
            sourceAnchorMin = RectTrans.anchorMin;
            sourceLocalScale = RectTrans.localScale;
            sourceSizeData=RectTrans.sizeDelta;
            sourceAnchoredPosition = RectTrans.anchoredPosition;
            sourceAnchoredPosition3D = RectTrans.anchoredPosition3D;
        }
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            if (BntClose != null)
            {
                BntClose.Init(new BaseButtonData() { OnClick = OnClickClose });
            }
            if (Title != null)
            {
                Title.Init(new BaseTextData() { Name = GetTitle, IsTrans = false });
            }
            BaseScreenMgr.Callback_OnSetPlayerBase += OnSetPlayerBase;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            BaseScreenMgr.Callback_OnSetPlayerBase -= OnSetPlayerBase;
        }
        /// <summary>
        /// 将界面挂到其他界面下
        /// </summary>
        public override void Attach(BaseView parentView, BaseView rootView, Object mono)
        {
            base.Attach(parentView,  rootView,  mono);
            // 有利于找到UIprefab问题
            if (RectTrans == null)
            {
                CLog.Error("RectTrans没有，没有Awake？");
            }
            if (rootView != null)
            {
                RectTrans.SetParent(this.RootView.CanvasTrans);
            }

            RectTrans.localScale = sourceLocalScale;
            RectTrans.anchorMax = sourceAnchorMax;
            RectTrans.anchorMin = sourceAnchorMin;
            RectTrans.sizeDelta = sourceSizeData;
            Trans.localPosition = sourceLocalPos;
            RectTrans.anchoredPosition = sourceAnchoredPosition;
            RectTrans.anchoredPosition3D = sourceAnchoredPosition3D;

        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (SelfBaseGlobal == null)
                return;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            foreach (var item in updatePresenters)
            {
                if (item.IsDirty)
                {
                    item.Refresh();
                }
                item.OnFixedUpdate();
            }
        }
        /// <summary>
        /// 销毁UI
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            RemoveAllPanelGroup();
            presenters.Clear();
            updatePresenters.Clear();
        }
        /// <summary>
        /// 获取控件
        /// </summary>
        protected override void FetchPresenters ()
        {
            if (GO == null)
                return;
            if (IsRootView)
                return;
            var tempPresenters = GO.GetComponentsInChildren<BasePresenter>(true);
            if (tempPresenters != null)
            {
                foreach (var item in tempPresenters)
                {
                    if (!item.IsSubPresenter)
                    {
                        item.BaseUIView = this;
                        presenters.Add(item);
                        if (item.NeedUpdate)
                        {
                            ActivePresenterUpdate(item);
                        }
                    }
                }
            }
            Scrolls = GetComponentsInChildren<BaseScroll>();
        }
        #endregion

        #region set
        public override void SetDirty()
        {
            base.SetDirty();
        }
        public void SetDirtyRelaodData()
        {
            if (Scrolls != null)
            {
                foreach (var item in Scrolls)
                {
                    item.SetDirtyReloadData();
                }
            }
        }
        public void ActivePresenterUpdate(BasePresenter presenter)
        {
            updatePresenters.Add(presenter);
        }
        /// <summary>
        /// 显示或者关闭界面
        /// </summary>
        /// <param name="b"></param>
        public override void Show(bool b=true, float? fadeTime = null, float delay = 0, bool useGroup = true, bool force = false)
        {
            if (IsShow == b && !force)
                return;
            if (Delay == 0)
                Delay = delay;

            base.Show(b, fadeTime, delay, useGroup, force);
            float mainShowTime = 0.0f;
            IsShow = b;

            //设置时长
            if (fadeTime != null)mainShowTime = fadeTime.Value;
            else mainShowTime = IsSameTime? Duration : ( b ? InTime : OutTime );

            //停止之前的tween
            if (alphaTween != null)alphaTween.Kill();
            if (scaleTween != null)scaleTween.Kill();
            if (moveTween != null) moveTween.Kill();

            //Alpha效果
            alphaTween = DOTween.To(
                () => canvasGroup.alpha, 
                x => canvasGroup.alpha = x, 
                b ? 1.0f : 0.0f, 
                mainShowTime);
            if (IsShow)
            {
                OnOpen(this,useGroup);
                alphaTween.OnStart(OnFadeIn);
                alphaTween.SetDelay(Delay);
            }
            else
            {
                OnClose();
                alphaTween.OnComplete(OnFadeOut);
                alphaTween.SetDelay(Delay);
            }
            //缩放效果
            if (IsScale)
            {
                Vector3 minScale= sourceLocalScale * 0.001f;
                if (IsShow)                
                    Trans.localScale = minScale;
                    scaleTween = Trans.DOScale(IsShow ? sourceLocalScale : minScale,
                    mainShowTime)
                    .SetEase(IsShow ? TweenScale.InEase : TweenScale.OutEase)
                    .SetDelay(Delay);
            }
            //位移效果
            if (IsMove)
            {
                if (IsShow)RectTrans.anchoredPosition = TweenMove.StartPos;
                moveTween = DOTween.To(
                    () => RectTrans.anchoredPosition, 
                    (x) => RectTrans.anchoredPosition = x, b ? sourceAnchoredPosition : TweenMove.StartPos,
                    mainShowTime)
                    .SetEase(IsShow ? TweenMove.InEase : TweenMove.OutEase)
                    .SetDelay(Delay);
            }

            //屏蔽/取消屏蔽 UI点击
            if (canvasGroup != null)canvasGroup.blocksRaycasts = IsShow;
            //触发控件的ViewShow事件
            foreach (var item in presenters)item.OnViewShow(b);

            //执行UI互斥
            if (IsShow)
            {
                SetDirty();
                //UI互斥,相同UI组只能有一个UI被打开
                if (useGroup && Group > 0)
                {
                    for (int i = 0; i < SubViews.Count; ++i)
                    {
                        if (SubViews[i] != this && SubViews[i].Group == Group && SubViews[i].ViewLevel == ViewLevel)
                            SubViews[i].Show(false, null, 0, false);
                    }
                }
            }
            else
            {
                if (ParentView != null)
                {
                    //关闭界面的时候自动刷新父级界面
                    if (ParentView.IsShow && !ParentView.IsRootView)
                    {
                        ParentView.SetDirty();
                    }
                }
                foreach (var item in SubViews)
                    item.Show(false);
            }
            //刷新panel
            foreach (var item in PGList)
            {
                item.RefreshShow();
            }

            OnShow();
            Delay = 0;//重置Delay
        }

        /// <summary>
        /// 刷新多语言，以及其他显示
        /// </summary>
        public override void Refresh()
        {
            foreach (var item in presenters)
            {
                if( !item.IsSubPresenter&&item.IsAutoRefresh)
                    item.Refresh();
            }
            //刷新panel
            foreach (var item in PGList)
            {
                item.Refresh();
            }
        }
        public override void Enable(bool b)
        {
            IsEnable = b;
            if (Canvas != null)
                Canvas.enabled = IsEnable;
            if (GraphicRaycaster != null)
                GraphicRaycaster.enabled = IsEnable;
            if (CanvasScaler != null)
                CanvasScaler.enabled = IsEnable;

        }
        public override void Interactable(bool b)
        {
            if (canvasGroup != null)
                canvasGroup.interactable = b;
        }
        #endregion

        #region 工具函数包装
        protected static void ShowTip(string key, params string[] ps)
        {
            BaseTooltipView.Default.Show(key, ps);
        }
        protected static void ShowTipStr(string str)
        {
            BaseTooltipView.Default.ShowStr(str);
        }
        protected static void ShowOKCancle(string key, string descKey, Callback BntOK, Callback BntCancle, params object[] paras)
        {
            BaseModalBoxView.Default.ShowOKCancle(key, descKey, BntOK, BntCancle, paras);
        }
        protected static void ShowOK(string key, string descKey, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.Default.ShowOK(key, descKey, BntOK, paras);
        }
        protected static void ShowOK(string descKey, Callback BntOK, params object[] paras)
        {
            BaseModalBoxView.Default.ShowOK(descKey, BntOK, paras);
        }
        #endregion

        #region get
        protected virtual string TitleKey => BaseConstMgr.STR_Inv;
        protected virtual string GetTitle()
        {
            if (TitleKey.IsInvStr())
                return "None";
            return GetStr(TitleKey);
        }

        #endregion

        #region Callback
        protected virtual void OnClickClose(BasePresenter presenter, PointerEventData data)
        {
            Show(false);
        }
        protected virtual void OnSetPlayerBase(BaseUnit arg1, BaseUnit arg2)
        {
            SetDirty();
            SetDirtyRelaodData();
        }
        #endregion

    }
}
