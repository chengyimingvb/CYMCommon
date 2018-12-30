//**********************************************
// Class Name	: Unit
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CYM;
using Sirenix.OdinInspector;

namespace CYM
{
    /// <summary>
    /// 角色材质
    /// </summary>
    public enum UnitMaterialType
    {
        Unknow,
        Flesh,
        Metal,
        Wood,
        Stone,
    }
    public class BaseUnit : BaseCoreMono
    {
        #region inspector
        [FoldoutGroup("Base"),SerializeField,TextArea, Tooltip("用户自定义描述")]
        protected string Desc = "";
        /// <summary>
        /// 单位的ID
        /// </summary>
        [FoldoutGroup("Base"), SerializeField, Tooltip("单位的TDID")]
        protected string TDID = "";
        /// <summary>
        /// 队伍
        /// </summary>
        [FoldoutGroup("Base"), MinValue(0), SerializeField, Tooltip("单位的队伍")]
        public int Team = 0;
        #endregion

        #region prop
        /// <summary>
        /// 是否自动初始化数据,通过Spawn创建的AutoInit = false
        /// </summary>
        public bool AutoInit { get; set; } = true;
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsLive { get; protected set; } = false;
        /// <summary>
        /// 是否真的死亡
        /// </summary>
        public bool IsRealDeath { get; protected set; } = false;
        /// <summary>
        /// 是否被渲染
        /// </summary>
        public bool IsRendered { get; private set; } = false;
        /// <summary>
        /// 上一帧被渲染
        /// </summary>
        public bool IsLastRendered { get; private set; } = false;
        #endregion

        #region life
        public override void Awake()
        {
            MonoType = MonoType.Unit;
            base.Awake();
        }
        public override void Start()
        {
            if (AutoInit)
            {
                OnSpawnInit(TDID, Team);
            }
            base.Start();
        }
        public override void OnEnable()
        {
            Trans.localScale = Vector3.one;
            base.OnEnable();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateRendered();
        }
        #endregion

        #region unit life
        /// <summary>
        /// 对象产生的时候必然会经过的一个流程
        /// </summary>
        /// <param name="id"></param>
        /// <param name="team"></param>
        public virtual void OnSpawnInit(string id,int team)
        {
            SetTDID(id);
            SetTeam(team);
            Init();
        }
        public override void Birth()
        {
            if (IsLive)
                return;
            IsLive = true;
            IsRealDeath = false;
            base.Birth();
        }
        public override void Death(BaseUnit caster)
        {
            if (!IsLive)
                return;
            IsLive = false;
            base.Death(caster);
        }
        public override void RealDeath()
        {
            IsRealDeath = true;
            base.RealDeath();
        }
        protected void UpdateRendered()
        {
            Vector3 pos = SelfBaseGlobal.CameraMgr.MainCamera.WorldToViewportPoint(Trans.position);
            IsRendered = (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
            if (IsRendered != IsLastRendered)
            {
                if (IsRendered)
                    OnBeRender();
                else
                    OnBeUnRender();

                IsLastRendered = IsRendered;
            }
        }
        #endregion

        #region set
        /// <summary>
        /// 设置是否存活
        /// </summary>
        /// <param name="b"></param>
        public void SetLive(bool b)
        {
            IsLive = b;
        }
        /// <summary>
        /// 设置小队
        /// </summary>
        /// <param name="team"></param>
        public void SetTeam(int team)
        {
            Team = team;
        }
        /// <summary>
        /// 设置TDID
        /// </summary>
        /// <param name="tdid"></param>
        public void SetTDID(string tdid)
        {
            if (tdid.IsInvStr())
            {
                TDID = gameObject.name;
            }
            else
            {
                TDID = tdid;
            }
        }
        #endregion

        #region get
        public virtual string GetTDID()
        {
            return TDID;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否为本地玩家
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLocalPlayer()
        {
            return true;
        }
        /// <summary>
        /// 是否为其他玩家
        /// </summary>
        /// <returns></returns>
        public virtual bool IsPlayerCtrl()
        {
            return true;
        }
        /// <summary>
        /// 是否是敌人
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team != Team;
        }
        /// <summary>
        /// 是否为本地玩家的对立面
        /// </summary>
        /// <returns></returns>
        public virtual bool IsOpposite()
        {
            return false;
        }
        /// <summary>
        /// 是否为自己
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool IsSelf(BaseUnit other)
        {
            return this == other;
        }
        /// <summary>
        /// 是否为中立怪
        /// </summary>
        /// <returns></returns>
        public virtual bool IsNeutral()
        {
            return Team == 2;
        }
        /// <summary>
        /// 非中立怪 敌人
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool IsUnNeutralEnemy(BaseUnit other)
        {
            if (other.IsNeutral())
                return false;
            return IsEnemy(other);
        }
        #endregion

        #region Callback
        protected virtual void OnBeRender()
        {

        }
        protected virtual void OnBeUnRender()
        {

        }
        #endregion
    }
}