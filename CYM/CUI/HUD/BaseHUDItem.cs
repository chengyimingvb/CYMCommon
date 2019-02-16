//------------------------------------------------------------------------------
// BaseHUDItem.cs
// Copyright 2018 CopyrightHolderName 
// Created by CYM on 2018/2/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace CYM.UI
{
    public class BaseHUDItem : Presenter<PresenterData> 
    {
        #region member variable
        public float LifeTime=1;
        public Color Color;
        public NodeType NodeType= NodeType.Center;
        public Vector3 Offset = Vector3.zero;
        public BaseUnit SelfUnit { get; private set; }
        #endregion

        #region 曲线
        [HideIf("Inspector_HideCurve")]
        public AnimationCurve OffsetCurveY = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(0.2f, 120f), new Keyframe(0.3f, 140f), new Keyframe(0.8f, 70f) });
        [HideIf("Inspector_HideCurve")]
        public AnimationCurve OffsetCurveX = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(0.8f, 130f) });
        [HideIf("Inspector_HideCurve")]
        public AnimationCurve AlphaCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.4f), new Keyframe(0.7f, 1f), new Keyframe(0.9f, 1f), new Keyframe(1f, 0.8f) });
        [HideIf("Inspector_HideCurve")]
        public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.5f), new Keyframe(0.33f, 0.7f), new Keyframe(0.67f, 1f), new Keyframe(1f, 1f) });
        #endregion

        #region property
        public Callback<BaseHUDItem> OnLifeOver;
        public bool IsLifeOver { get { return CurTime >= LifeTime; } }
        protected Transform followObj;
        protected float CurTime;
        protected Vector3 TempPos;
        protected float Offset_y = 0f;
        protected float Offset_x = 0f;
        protected float CurPercent => CurTime / LifeTime;
        protected CanvasScaler CanvasScaler;
        #endregion

        #region methon
        /// <summary>
        /// 跳字创建的时候需要调用此函数
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pool"></param>
        /// <param name="follow"></param>
        public virtual void Init(object obj/*自定义参数*/,BaseUnit unit,Transform followObj=null)
        {
            this.followObj = followObj;
            SelfUnit = unit;
        }
        public void SetFollowObj(Transform followObj = null)
        {
            this.followObj = followObj;
        }
        protected override void OnEnable()
        {
            CurTime = 0.0f;
            Offset_y = 0f;
            Offset_x = 0f;
            Color.a = 0.0f;
            base.OnEnable();
        }

        public virtual void OnUpdate()
        {
            if (CanvasScaler == null)
            {
                if(BaseUIView!=null&& BaseUIView.RootView!=null)
                    CanvasScaler  = BaseUIView.RootView.CanvasScaler;
            }
            if (LifeTime > 0)
            {
                CurTime += Time.deltaTime;
                Offset_y = OffsetCurveY.Evaluate(CurPercent);
                Offset_x = OffsetCurveX.Evaluate(CurPercent);
                Color.a = AlphaCurve.Evaluate(CurPercent);
                Trans.localScale = ScaleCurve.Evaluate(CurPercent) * Vector3.one;
                if (CurTime > LifeTime)
                {
                    OnLifeOver?.Invoke(this);
                }
                if (followObj != null)
                    TempPos = followObj.position + Offset;
                Trans.position = Camera.main.WorldToScreenPoint(TempPos);
                Trans.position += new Vector3(Offset_x, Offset_y, 0f);
            }
        }
        public void DoDestroy()
        {
            CurTime = LifeTime;
        }
        #endregion

        #region Callback
        bool Inspector_HideCurve()
        {
            return LifeTime <= 0;
        }
        #endregion
    }
}