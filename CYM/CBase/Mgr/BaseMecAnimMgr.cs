using UnityEngine;

namespace CYM
{
    public class BaseMecAnimMgr : BaseAnimMgr
    {
        #region member variable
        private BaseAnim[] BaseAnims;
        #endregion

        #region property
        /// <summary>
        /// 动画播放器：注意这个对象是可以为空的，比如塔，水晶，基地等一些建筑物
        /// </summary>
        public Animator Animator { get; private set; }
        public AnimatorTransitionInfo BaseAnimatorTransitionInfo;
        public AnimatorStateInfo BaseAnimatorStateInfo;
        public RuntimeAnimatorController SourceAnimator { get; private set; }
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
            Animator = Mono.GetComponentInChildren<Animator>();
            if (Animator == null)
            {
                CLog.Error("错误 该对象没有Animator" + Mono.name);
            }
            SourceAnimator = Animator.runtimeAnimatorController;
        }

        public override void Birth()
        {
            base.Birth();
            InitAnims();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Animator != null && Mono.GO.activeSelf&& Animator.runtimeAnimatorController!=null)
            {
                if (!Animator.isInitialized)
                    return;
                BaseAnimatorTransitionInfo = Animator.GetAnimatorTransitionInfo(0);
                BaseAnimatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            }
        }

        protected void InitAnims()
        {
            BaseAnims = Animator.GetBehaviours<BaseAnim>();
            if (BaseAnims != null)
            {
                for (int i = 0; i < BaseAnims.Length; ++i)
                    BaseAnims[i].Init(SelfBaseUnit);
            }
        }

        public override void EnableAnim(bool b)
        {
            Animator.enabled = b;
        }

        public virtual void SetTrigger(string name)
        {
            Animator.SetTrigger(name);
        }

        public override void Play(string animName, bool fixedTime = true)
        {
            if (Animator == null)
                return;
            else
            {
                if (fixedTime)
                    Animator.PlayInFixedTime(animName);
                else
                    Animator.Play(animName);
            }
        }
        public override void CrossFade(string animName, float transDuration = 0.05f, bool fixedTime = true)
        {
            if (Animator == null)
                return;
            else
            {
                if (fixedTime)
                    Animator.CrossFadeInFixedTime(animName, transDuration);
                else
                    Animator.CrossFade(animName, transDuration);
            }
        }

        public override bool IsName(string animName, int layer = 0)
        {
            base.IsName(animName);
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetCurrentAnimatorStateInfo(layer).IsName(animName);
            }
        }
        public bool IsTag(string animTag, int layer = 0)
        {
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetCurrentAnimatorStateInfo(layer).IsTag(animTag);
            }
        }
        public bool IsInEnd(string name)
        {
            if (IsName(name) && BaseAnimatorStateInfo.normalizedTime >= 1)
                return true;
            return false;
        }
        public bool IsInEnd()
        {
            if (BaseAnimatorStateInfo.normalizedTime >= 1)
                return true;
            return false;
        }
        public override bool IsTransitionName(string animName, int layer = 0)
        {
            base.IsTransitionName(animName);
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetAnimatorTransitionInfo(layer).IsName(animName);
            }
        }
        public virtual bool IsNextName(string animName, int layer = 0)
        {
            if (Animator == null)
                return false;
            else
            {
                return Animator.GetNextAnimatorStateInfo(layer).IsName(animName);
            }
        }
        public override bool IsInTranslation(int layer = 0)
        {
            if (Animator == null)
                return false;

            return Animator.IsInTransition(layer);
        }
        #endregion

        #region set
        public void SetSourceAndCurAnimator(RuntimeAnimatorController animator)
        {
            SetRuntimeAnimatorController(animator);
            SourceAnimator = animator;
        }
        public void SetAnimator(RuntimeAnimatorController animator)
        {
            if (animator != null)
            {
                SetRuntimeAnimatorController(animator);
            }
            else
            {
                RevertAnimator();
            }

        }
        public void RevertAnimator()
        {
            SetRuntimeAnimatorController(SourceAnimator);
        }
        private void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeCtrl)
        {
            if (runtimeCtrl == null)
                return;
            Animator.runtimeAnimatorController = runtimeCtrl;
            InitAnims();
        }
        public void ApplyRootMotion(bool b)
        {
            Animator.applyRootMotion = b;
        }
        #endregion

        #region get
        /// <summary>
        /// 转换hash
        /// </summary>
        /// <returns></returns>
        public static int StringToHash(string id)
        {
            return Animator.StringToHash(id);
        }
        public AnimatorStateInfo GetCurAnimatorStateInfo()
        {
            BaseAnimatorStateInfo = Animator.GetCurrentAnimatorStateInfo(0);
            return BaseAnimatorStateInfo;
        }
        #endregion
    }
}
