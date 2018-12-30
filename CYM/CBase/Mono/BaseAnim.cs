using UnityEngine;
using System.Collections;
namespace CYM
{
    public class BaseAnim : StateMachineBehaviour
    {
        protected BaseUnit self;
        protected float sourceSpeed = 1.0f;
        [SerializeField]
        bool IsApplyRootMotion = false;
        [SerializeField][Tooltip("状态机会等待Transition后进入OnStateEntered")]
        bool IsIgnoreTransition = true;

        bool IsEnterTrigger = false;
        public virtual void Init(BaseUnit self)
        {
            if (self == null)
            {
                CLog.Error("没有self对象");
            }
            this.self = self;
        }
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.applyRootMotion = IsApplyRootMotion;
            IsEnterTrigger = true;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (IsEnterTrigger)
            {
                if (IsIgnoreTransition && animator.IsInTransition(0))
                    return;
                IsEnterTrigger = false;
                OnStateEntered();
            }
        }

        protected virtual void OnStateEntered()
        {

        }
    }
}
