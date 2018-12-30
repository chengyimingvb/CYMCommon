using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 通过响应式触发
/// OnEnter
/// OnNormal
/// OnDisable
/// OnInteratable
/// </summary>
namespace CYM.UI
{
    public enum UITransitionTrigerEvent
    {
        OnPointerEnter,
        OnPointerExit,
        OnPointerDown,
        OnPointerUp,
        OnInteractable,
        OnShow,
}

    //[ExecuteInEditMode]
    [RequireComponent(typeof(BasePresenter))]
    public class UITransition : UIBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Inspector
        [SerializeField]
        public RectTransform RectTrans;
        [SerializeField]
        public float Duration = 0.2f;
        [SerializeField]
        public float Delay = 0.0f;
        [SerializeField][HideInInspector]
        public BasePresenter Presenter;
        [SerializeField]
        protected bool IsReset = false;
        #endregion

        protected BaseMeshEffect Effect;
        protected Shadow Shadow;
        protected Outline Outline;

        protected Graphic Graphic;
        protected Text Text;
        protected Image Image;
        protected bool IsInteractable { get; set; } = true;
        protected bool IsSelected { get; set; } = false;

        #region LIFE
        protected override void OnEnable()
        {
            base.OnEnable();
            if (RectTrans == null)
                RectTrans = GetComponent<RectTransform>();
            if (Presenter == null)
                Presenter = GetComponent<BasePresenter>();
        }

        protected override void Awake()
        {
            base.Awake();
            if(RectTrans==null)
                RectTrans = GetComponent<RectTransform>();
            if(Presenter==null)
                Presenter = GetComponent<BasePresenter>();
            if (Presenter == null)
            {
                CLog.Error("UITransition 没有 Presenter:{0}",BaseUIUtils.GetPath(gameObject));
            }
            if (RectTrans!=null)
            {
                Graphic=RectTrans.GetComponent<Graphic>();
                Effect= RectTrans.GetComponent<BaseMeshEffect>();
            }
            if (Graphic != null)
            {
                if (Graphic is Text)
                    Text = (Text)Graphic;
                if (Graphic is Image)
                    Image = (Image)Graphic;
                RectTrans = Graphic.rectTransform;
            }
            if (Effect != null)
            {
                if (Effect is Shadow)
                    Shadow = (Shadow)Effect;
                if(Effect is Outline)
                    Outline = (Outline)Effect;
            }
        }
        #endregion

        #region callback
        public virtual void OnPointerEnter(PointerEventData eventData)
        {

        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {

        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {

        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {

        }

        public virtual void OnInteractable(bool b)
        {
            IsInteractable = b;
        }
        public virtual void OnSelected(bool b)
        {
            IsSelected = b;
        }
        public virtual void OnShow(bool b,bool isActiveByShow)
        {
            if (Presenter == null)
                Presenter = GetComponent<BasePresenter>();
            if (Presenter.IsShow)
            {
                if (isActiveByShow)
                    Presenter.SetActive(true);
            }
        }

        #endregion
    }

}