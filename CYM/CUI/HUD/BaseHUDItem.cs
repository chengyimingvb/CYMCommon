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
namespace CYM.UI
{
    public class BaseHUDItem : Presenter<PresenterData> 
    {
        #region member variable
        public float LifeTime=1;
        public Color Color;
        public NodeType NodeType= NodeType.Center;
        public Vector3 Offset = Vector3.zero;
        protected Transform followObj;
        protected float curTime;
        protected Vector3 pos;
        protected float Offset_y = 0f;
        protected float Offset_x = 0f;
        protected float curPercent
        {
            get { return curTime / LifeTime; }
        }
        #endregion

        #region property
        public bool IsLifeOver { get { return curTime >= LifeTime; } }
        public Callback<BaseHUDItem> OnLifeOver;
        #endregion

        #region methon
        /// <summary>
        /// 位置偏移曲线
        /// </summary>
        public AnimationCurve offsetCurve_y = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(0.2f, 120f), new Keyframe(0.3f, 140f), new Keyframe(0.8f, 70f) });
        public AnimationCurve offsetCurve_x = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(0.8f, 130f) });

        /// <summary>
        /// alpha曲线
        /// </summary>
        public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.4f), new Keyframe(0.7f, 1f), new Keyframe(0.9f, 1f), new Keyframe(1f, 0.8f) });

        /// <summary>
        /// 缩放曲线
        /// </summary>
        public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.5f), new Keyframe(0.33f, 0.7f), new Keyframe(0.67f, 1f), new Keyframe(1f, 1f) });

        /// <summary>
        /// 跳字创建的时候需要调用此函数
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pool"></param>
        /// <param name="follow"></param>
        public virtual void Init(object obj/*自定义参数*/,Transform followObj=null)
        {
            this.followObj = followObj;
            //if (posOffset.HasValue)
            //    positionOffset = posOffset.Value;
            
        }
        public void SetFollowObj(Transform followObj = null)
        {
            this.followObj = followObj;
        }
        protected override void OnEnable()
        {
            curTime = 0.0f;
            Offset_y = 0f;
            Offset_x = 0f;
            Color.a = 0.0f;
            base.OnEnable();
        }

        protected virtual void Update()
        {
            curTime += Time.deltaTime;
            Offset_y = offsetCurve_y.Evaluate(curPercent);
            Offset_x = offsetCurve_x.Evaluate(curPercent);
            Color.a = alphaCurve.Evaluate(curPercent);
            Trans.localScale = scaleCurve.Evaluate(curPercent) * Vector3.one;
            if (curTime > LifeTime)
            {
                if (OnLifeOver != null)
                    OnLifeOver(this);
            }
            if (followObj != null)
                pos = followObj.position + Offset;
            Trans.position = Camera.main.WorldToScreenPoint(pos);
            Trans.position += new Vector3(Offset_x, Offset_y, 0f);
            //Text.color = Color;
        }
        public void DoDestroy()
        {
            curTime = LifeTime;
        }
        #endregion

        #region get
        #endregion
    }
}