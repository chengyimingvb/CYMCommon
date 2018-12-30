using System;
using UnityEngine;
using CYM;
using System.Collections.Generic;
using MoonSharp.Interpreter;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public enum SkillInteraputType
    {
        /// <summary>
        /// 不可被打断
        /// </summary>
        None = 0,
        /// <summary>
        /// 移动打断
        /// </summary>
        Move = 1,
        /// <summary>
        /// 技能打断
        /// </summary>
        Skill = 2,
        /// <summary>
        /// 攻击打断
        /// </summary>
        Attack = 4,
        /// <summary>
        /// 被动打断，比如晕眩，击飞
        /// </summary>
        Passive = 8,
    }
    public enum SkillPhase
    {
        None = 0,
        Hold = 1,
        Start = 2,
        Cast = 4,
        CastEnd = 8,
    }
    public enum SkillReleaseDataType
    {
        /// <summary>
        /// 对点释放
        /// </summary>
        Point = 1,
        /// <summary>
        /// 对目标释放
        /// </summary>
        Target = 2,
    }
    public enum SkillUseType
    {
        /// <summary>
        /// 瞬发类型：点击立即释放
        /// </summary>
        Instant = 0,
        /// <summary>
        /// 指示器类型，使用后的操作委托给指示器
        /// </summary>
        Pointer = 1,
        /// <summary>
        /// 被动
        /// </summary>
        Passive=2,
    }

    public enum SkillOpType
    {
        Attack = 0,
        Skill = 1,
    }

    public enum SkillReleaseResult
    {
        None,
        Succ,
        UnCheckDistance,
        UnCheckCost,
        UnCheckTarget,
        UnCheckCD,
        UnCheckUnlock,
        UnCheckState,
        Dead,
    }

    /// <summary>
    /// 技能释放数据
    /// </summary>
    public struct SkillReleaseData
    {
        public SkillReleaseDataType Type;
        public Vector3 Position;
        public BaseUnit Target;
        public SkillReleaseData(SkillReleaseDataType type, BaseUnit target, Vector3 position)
        {
            Type = type;
            Target = target;
            Position = position;
        }
    }

    public class TDBaseSkillData : BaseConfig<TDBaseSkillData>, IOnAnimTrigger,IPrespawner
    {
        #region const
        public static float NO_CD = 0;
        public static float TIME_STEP = 1.0f;
        #endregion

        #region lua
        /// <summary>
        /// 技能释放类型
        /// </summary>
        public SkillUseType UseType { get; set; } = SkillUseType.Instant;
        /// <summary>
        /// 技能操作类型
        /// </summary>
        public SkillOpType OpType { get; set; } = SkillOpType.Skill;
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHide { get; set; } = false;
        /// <summary>
        /// 技能CD时间
        /// </summary>
        public float MaxCD { get; set; } = 10.0f;
        /// <summary>
        /// 原始基数
        /// </summary>
        public float BaseVal { get; set; }

        /// <summary>
        /// 自定义前摇时间
        /// </summary>
        public float BeforeCastTime { get; set; }
        /// <summary>
        /// 自定义施法中时间
        /// </summary>
        public float InCastTime { get; set; }
        /// <summary>
        /// 后摇时间
        /// </summary>
        public float AffterCastTime { get; set; }

        /// <summary>
        /// 前摇音效
        /// </summary>
        public string[] StartSFX { get; set; }
        /// <summary>
        /// 施法中音效
        /// </summary>
        public string[] CastSFX { get; set; }

        public string[] StartPerform { get; set; }
        public string[] CastPerform { get; set; }
        public string[] CastBulletPerform { get; set; }
        public string[] CastCustomPerform { get; set; }
        public string[] CastEndPerform { get; set; }
        public string[] HitPerform { get; set; }

        /// <summary>
        /// AOE
        /// </summary>
        public int AOE { get; set; } = 99;
        /// <summary>
        /// 技能产生的buff
        /// </summary>
        public string[] StartBuffs { get; set; }
        /// <summary>
        /// 技能命中产生的buff
        /// </summary>
        public string[] HitBuffs { get; set; }

        public string Pointer { get; set; }
        public int AnimIndex { get; set; }
        public int VoiceIndex { get; set; }
        public int Index { get; set; }

        /// <summary>
        /// 物理伤害百分比
        /// </summary>
        public float PhysicDamagePercent { get; set; }
        /// <summary>
        /// 魔法伤害百分比
        /// </summary>
        public float MagicDamagePercent { get; set; }
        /// <summary>
        /// 真实伤害
        /// </summary>
        public float RealDamage { get; set; }
        #endregion

        #region Callback
        public event Callback<TDBaseSkillData> Callback_OnCDOver;
        #endregion

        #region runtime
        public SkillReleaseResult ReleaseResult { get; protected set; } = SkillReleaseResult.None;
        /// <summary>
        /// 技能打断类型
        /// </summary>
        public SkillInteraputType OnStart_InteraputType { get; protected set; }
        public SkillInteraputType OnCast_InteraputType { get; protected set; }
        public SkillInteraputType OnCastEnd_InteraputType { get; protected set; }
        /// <summary>
        /// 技能阶段
        /// </summary>
        public SkillPhase Phase { get; protected set; }
        /// <summary>
        /// 技能等级
        /// </summary>
        public int Lv { get; protected set; }
        /// <summary>
        /// 当前的CD
        /// </summary>
        public float CurCDTime { get; protected set; }
        /// <summary>
        /// 自定义流程
        /// </summary>
        protected bool IsCustomProcess { get; set; } = true;
        /// <summary>
        /// 技能释放数据
        /// </summary>
        protected SkillReleaseData? skillReleaseData;
        /// <summary>
        /// 自定义协程
        /// </summary>
        protected CoroutineHandle coroutineHandle;
        protected Timer AOETimer = new Timer(1.0f);
        protected int CurAOE { get;private set; } = 0;
        #endregion

        #region mgr
        protected BaseAudioMgr GlobalAudioMgr => SelfBaseGlobal.AudioMgr;
        #endregion

        #region life
        public override void OnBeAdded(BaseCoreMono mono, params object[] obj)
        {
            Phase = SkillPhase.None;
            OnStart_InteraputType =
                SkillInteraputType.Attack |
                SkillInteraputType.Move |
                SkillInteraputType.Passive |
                SkillInteraputType.Skill;
            OnCast_InteraputType = SkillInteraputType.None;
            OnCastEnd_InteraputType =
                SkillInteraputType.Attack |
                SkillInteraputType.Move |
                SkillInteraputType.Passive |
                SkillInteraputType.Skill;
            CurCDTime = RealMaxCDTime;
            base.OnBeAdded(mono, obj);
        }
        public override void OnBeRemoved()
        {
            base.OnBeRemoved();
        }

        protected virtual void OnInterval()
        {
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            AddCDTime(TIME_STEP);
            if (AOETimer.CheckOver())
            {
                CurAOE = 0;
            }
        }
        #endregion

        #region check
        public virtual bool CheckCondition(SkillReleaseData? data = null)
        {
            ReleaseResult = SkillReleaseResult.None;
            if (!SelfBaseUnit.IsLive)
                ReleaseResult = SkillReleaseResult.Dead;
            else if (!CheckUnlock())
                ReleaseResult = SkillReleaseResult.UnCheckUnlock;
            else if (!CheckCD())
                ReleaseResult = SkillReleaseResult.UnCheckCD;
            else if (!CheckDistance(data))
                ReleaseResult = SkillReleaseResult.UnCheckDistance;
            else if (!CheckTarget(data))
                ReleaseResult = SkillReleaseResult.UnCheckTarget;
            else if (!CheckCost(data))
                ReleaseResult = SkillReleaseResult.UnCheckCost;
            else if (!CheckState())
                ReleaseResult = SkillReleaseResult.UnCheckState;
            else
                ReleaseResult = SkillReleaseResult.Succ;

            return ReleaseResult == SkillReleaseResult.Succ;
        }
        public virtual bool CheckState()
        {
            return true;
        }
        public virtual bool CheckUnlock()
        {
            return true;
        }
        public virtual bool CheckDistance(SkillReleaseData? data = null)
        {
            return true;
        }
        public virtual bool CheckCost(SkillReleaseData? data = null)
        {
            return true;
        }
        public virtual bool CheckTarget(SkillReleaseData? data = null)
        {
            return true;
        }
        public virtual bool CheckCD()
        {
            return IsCDOver;
        }
        protected virtual void Rotate()
        {
            if (skillReleaseData.HasValue)
            {
            }
        }
        public virtual void UpgradeLevel()
        {
            Lv++;
        }
        #endregion

        #region enumator
        /// <summary>
        /// 主动释放技能
        /// </summary>
        /// <param name="ent">释放技能的对象</param>
        /// 
        /// <returns></returns>
        protected virtual IEnumerator<float> Custom_AffterRelease()
        {
            if (RealBeforeCastTime > 0.0f)
                yield return Timing.WaitForSeconds(RealBeforeCastTime);
            OnCast();
            if (RealInCastTime > 0.0f)
                yield return Timing.WaitForSeconds(RealInCastTime);
            OnCastEnd();
            if (RealAffterCastTime > 0.0f)
                yield return Timing.WaitForSeconds(RealAffterCastTime);
            OnEnd();
        }
        #endregion

        #region 触发函数
        protected virtual void OnStart()
        {
            CurAOE = 0;
            Rotate();
            Phase = SkillPhase.Start;
            GlobalAudioMgr.PlaySFX(StartSFX,SelfBaseUnit.Pos);
            PlayStartPerform();
        }
        protected virtual void OnCast()
        {
            Phase = SkillPhase.Cast;
            GlobalAudioMgr.PlaySFX(CastSFX, SelfBaseUnit.Pos);
            PlayCastPerform();
            OnCD();
            OnCost();
        }
        protected virtual void OnCastEnd()
        {
            Phase = SkillPhase.CastEnd;
            PlayCastEndPerform();
        }
        protected virtual void OnEnd()
        {
            Phase = SkillPhase.None;
        }
        protected virtual void OnCost()
        {
        }
        protected virtual void OnCD()
        {
            CurCDTime = 0.0f;
        }

        public virtual void OnTriggerEnter(Collider hitUnit, BasePerform perform)
        {
            BaseUnit tempUnit = hitUnit.GetComponent<BaseUnit>();
            if (tempUnit != null)
            {
            }
        }

        public virtual void OnTriggerExit(Collider hitUnit, BasePerform perform)
        {
        }

        public virtual void OnTriggerStay(Collider hitUnit, BasePerform perform)
        {

        }
        protected virtual void PlayStartPerform()
        {

        }
        protected virtual void PlayCastPerform()
        {

        }
        protected virtual void PlayCastEndPerform()
        {

        }
        #endregion

        #region 打断
        /// <summary>
        /// 是否可以被打断
        /// </summary>
        /// <param name="interruptType"></param>
        /// <returns></returns>
        public bool CanInterrupt(SkillInteraputType interruptType)
        {
            if (Phase == SkillPhase.None ||
                Phase == SkillPhase.Hold)
                return false;
            if (Phase == SkillPhase.Start)
                return (OnStart_InteraputType & interruptType) > 0;
            else if (Phase == SkillPhase.Cast)
                return (OnCast_InteraputType & interruptType) > 0;
            else if (Phase == SkillPhase.CastEnd)
                return (OnCastEnd_InteraputType & interruptType) > 0;
            return true;
        }

        /// <summary>
        /// 打断
        /// </summary>
        /// <param name="interruptType"></param>
        public void Interrupt(SkillInteraputType interruptType)
        {
            if (CanInterrupt(interruptType))
            {
                Phase = SkillPhase.None;
            }
            else
            {
            }
        }
        #endregion

        #region is
        /// <summary>
        /// 技能是否运行中
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return Phase != SkillPhase.None && Phase != SkillPhase.Hold;
            }
        }
        /// <summary>
        /// CD时间是否结束
        /// </summary>
        public bool IsCDOver
        {
            get
            {
                if (RealMaxCDTime == NO_CD ||
                    RealMaxCDTime == 0.0f)
                    return true;
                return CurCDTime > RealMaxCDTime;
            }
        }
        /// <summary>
        /// 是否达到了最大AOE
        /// </summary>
        public bool IsMaxAOE
        {
            get
            {
                return CurAOE >= AOE;
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 释放技能(技能入口)
        /// </summary>
        public virtual bool Release(SkillReleaseData? data = null)
        {
            skillReleaseData = data;
            if (CheckCondition(data))
            {
                OnStart();
                //进入自定义流程
                if (IsCustomProcess)
                {
                    SelfBaseGlobal.BattleCoroutine.Kill(coroutineHandle);
                    coroutineHandle = SelfBaseGlobal.BattleCoroutine.Run(Custom_AffterRelease());
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 添加CD时间
        /// </summary>
        /// <param name="step"></param>
        public virtual void AddCDTime(float step)
        {
            if (!IsCDOver)
            {
                CurCDTime += step;
                if (IsCDOver)
                {
                    Callback_OnCDOver?.Invoke(this);
                }
            }
        }
        public void AddAOE()
        {
            CurAOE++;
        }
        /// <summary>
        /// 强制结束
        /// </summary>
        public virtual void ForceOverCDTime()
        {
            CurCDTime = RealMaxCDTime;
        }
        /// <summary>
        /// 设置cd
        /// </summary>
        /// <param name="val"></param>
        public virtual void SetCDTime(float val)
        {
            CurCDTime = val;
        }
        #endregion

        #region get
        public override string GetDesc(params object[] ps)
        {
            return base.GetDesc(ps);
        }
        public virtual List<string> GetPrespawnPerforms()
        {
            List<string> ret = new List<string>();

            if(StartPerform!=null) ret.AddRange(StartPerform);
            if (CastPerform != null) ret.AddRange(CastPerform);
            if (CastBulletPerform != null) ret.AddRange(CastBulletPerform);
            if (CastEndPerform != null) ret.AddRange(CastEndPerform);
            if (HitPerform != null) ret.AddRange(HitPerform);

            return ret;
        }
        /// <summary>
        /// 真实的CD时间
        /// </summary>
        public float RealMaxCDTime
        {
            get
            {
                return MaxCD;
            }
        }
        /// <summary>
        /// 剩余的时间
        /// </summary>
        public float PercentOfRemainderCD
        {
            get
            {
                if (MaxCD == NO_CD)
                    return 0.0f;
                if (RealMaxCDTime == 0)
                    return 0.0f;
                return 1.0f - Mathf.Clamp01(CurCDTime / RealMaxCDTime);
            }
        }
        /// <summary>
        /// 剩余的时间
        /// </summary>
        public float RemainderCD
        {
            get
            {
                if (MaxCD == NO_CD)
                    return 0.0f;
                if (RealMaxCDTime == 0)
                    return 0.0f;
                return RealMaxCDTime - CurCDTime;
            }
        }
        //真实的前摇，施法中，后摇时间
        public virtual float RealBeforeCastTime { get { return BeforeCastTime; } }
        public virtual float RealInCastTime { get { return InCastTime; } }
        public virtual float RealAffterCastTime { get { return AffterCastTime; } }
        public float FullTime
        {
            get
            {
                return BeforeCastTime + InCastTime + AffterCastTime;
            }
        }
        #endregion

        #region override
        public virtual void OnAnimTrigger(int param)
        {

        }
        #endregion
    }

}