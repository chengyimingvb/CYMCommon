using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using DG.Tweening.Core;
namespace CYM.UI
{
    public class BaseUIView : BaseView
    {
        #region Inspector
        [SerializeField]
        protected BaseButton BntClose;
        [SerializeField]
        protected BaseText Title;
        #endregion

        #region 公共属性
        public GraphicRaycaster GraphicRaycaster { get; protected set; }
        public CanvasScaler CanvasScaler { get; protected set; }
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
            GraphicRaycaster = GetComponent<GraphicRaycaster>();
            CanvasScaler = GetComponent<CanvasScaler>();
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
        }
        #endregion

        #region set
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
            float tempFade = 0.0f;
            if (fadeTime != null)
                tempFade = fadeTime.Value;
            else
                tempFade = b ? InTime : OutTime;
            IsShow = b;
            if (alphaTween != null)
                alphaTween.Kill();
            if (scaleTween != null)
                scaleTween.Kill();
            alphaTween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, b ? 1.0f : 0.0f, tempFade);
            if (IsShow)
            {
                OnOpen(this,useGroup);
                alphaTween.OnStart(OnFadeIn);
                alphaTween.SetDelay(delay);
            }
            else
            {
                OnClose();
                alphaTween.OnComplete(OnFadeOut);
                alphaTween.SetDelay(delay);
            }
            if (IsScale)
            {
                scaleTween = Trans.DOScale(IsShow ? 1.0f : 0.001f, tempFade).SetEase(IsShow ? InEase : OutEase).SetDelay(delay);
            }
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = IsShow;


            //触发控件的Show事件
            foreach (var item in presenters)
                item.OnViewShow(b);

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
        }

        /// <summary>
        /// 刷新多语言，以及其他显示
        /// </summary>
        public override void Refresh()
        {
            foreach (var item in presenters)
            {
                if( !item.IsSubPresenter&&item.IsAutoRefresh)//item.IsShow&&
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
        public virtual void ShowTip(string key,params string[] ps)
        {
            throw new System.NotImplementedException();
        }
        public virtual void ShowTipStr(string str)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region get
        protected virtual string GetTitle()
        {
            return "None";
        }

        #endregion

        #region Callback
        protected virtual void OnClickClose(BasePresenter presenter, PointerEventData data)
        {
            Show(false);
        }
        #endregion

    }
}
