//**********************************************
// Class Name	: CYMBaseSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using CYM.Highlighting;
namespace CYM
{
    public class BaseSurfaceMgr : BaseCoreMgr
{
        #region member variable
        public Renderer[] ModelRenders { get; protected set; }//模型自身的渲染器
        public SkinnedMeshRenderer[] SkinnedMeshRenderers { get; protected set; }//蒙皮渲染
        public SkinnedMeshRenderer MainSkinnedMesh { get; protected set; }//主要的蒙皮
        public GameObject Model { get; protected set; }//模型自身的渲染器的跟节点
        private Highlighter highlighter;
        public bool IsEnableRenders { get; private set; }
        protected virtual bool IsNeedHighlighter { get; } //禁用高亮效果,这样可以使用GPUInstance
        protected virtual bool IsUseSurfaceMaterial { get; } //禁用材质效果,这样可以使用GPUInstance
        protected BaseGRMgr GRMgr => SelfBaseGlobal.GRMgr;
        #endregion

        #region property
        public BaseSurface CurSurface { get; set; }
        public Surface_Source Surface_Source { get; private set; } = new Surface_Source();
        #endregion

        #region private
        private CoroutineHandle CoroutineHandle_Flash;
        private CoroutineHandle CoroutineHandle_Constant;
        #endregion

        #region methon
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            AssianModel();
            if (Model == null)
            {
                CLog.Error("Unit 没有model");
                return;
            }
            ModelRenders = Model.GetComponentsInChildren<Renderer>();
            {
            }
            SkinnedMeshRenderers = Model.GetComponentsInChildren<SkinnedMeshRenderer>();
            {
                float lastSize = 0.0f;
                float curSize = 0.0f;
                foreach (var item in SkinnedMeshRenderers)
                {
                    Vector3 extents = item.bounds.extents;
                    curSize = extents.x + extents.y + extents.z;
                    if (curSize > lastSize)
                    {
                        lastSize = curSize;
                        MainSkinnedMesh = item;
                    }
                }
            }
            IsEnableRenders = true;
            if(IsUseSurfaceMaterial)
                Surface_Source.Init(this);
            if (IsNeedHighlighter)
                highlighter = Mono.EnsureComponet<Highlighter>();
        }
        public override void Birth()
        {
            base.Birth();
            if(IsUseSurfaceMaterial)
                Surface_Source.Use();
            Off();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (IsUseSurfaceMaterial)
                CurSurface?.Update();
        }
        protected virtual void AssianModel()
        {
            Model = Mono.GO;
        }
        public virtual void EnableRender(bool enable)
        {
            if (IsEnableRenders == enable)
                return;
            if (ModelRenders!=null)
            {
                for (int i = 0; i < ModelRenders.Length; ++i)
                    ModelRenders[i].enabled = enable;
                IsEnableRenders = enable;
            }
        }
        public virtual void SetShadowMode(ShadowCastingMode mode)
        {
            if(ModelRenders!=null)
            {
                foreach(var item in ModelRenders)
                {
                    item.shadowCastingMode = mode;
                }
            }
        }
        public void EnableReceiveShadows(bool b)
        {
            if (ModelRenders != null)
            {
                foreach (var item in ModelRenders)
                {
                    item.receiveShadows = b;
                }
            }
        }
        #endregion

        #region highlight
        public void ConstantOn(Color col, float time = 0.25f,float duration=1.0f)
        {
            SelfBaseGlobal.BattleCoroutine.Kill(CoroutineHandle_Constant);
            CoroutineHandle_Constant = SelfBaseGlobal.BattleCoroutine.Run(_Constant(col, time, duration));
        }
        public void ConstantOff(float time = 0.25f)
        {
            if(highlighter)
                highlighter.ConstantOff(time);
        }
        public void FlashingOn(Color color1, Color color2, float freq,float duration=1.0f)
        {
            SelfBaseGlobal.BattleCoroutine.Kill(CoroutineHandle_Flash);
            CoroutineHandle_Flash =SelfBaseGlobal.BattleCoroutine.Run(_Flash(color1,  color2,  freq,  duration ));
        }
        public void FlashingOn(float freq)
        {
            if (highlighter)
                highlighter.FlashingOn(freq);
        }
        public void FlashingOff()
        {
            if (highlighter)
                highlighter.FlashingOff();
        }
        public void Off()
        {
            if (highlighter)
                highlighter.Off();
        }
        #endregion

        #region
        IEnumerator<float> _Flash(Color color1, Color color2, float freq, float duration = 1.0f)
        {
            if (highlighter == null)
                yield break;
            highlighter.FlashingOn(color1, color2, freq);
            yield return Timing.WaitForSeconds(duration);
            highlighter.FlashingOff();
        }
        IEnumerator<float> _Constant(Color color1, float time, float duration = 1.0f)
        {
            if (highlighter == null)
                yield break;
            highlighter.ConstantOn(color1, time);
            yield return Timing.WaitForSeconds(duration);
            highlighter.ConstantOff(time);
        }
        #endregion

    }
}
