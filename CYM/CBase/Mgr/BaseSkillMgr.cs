//**********************************************
// Class Name	: CYMBaseSettingsManager
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace CYM
{
    public class BaseSkillMgr<TData> : BaseCoreMgr, IOnAnimTrigger,IPrespawner, ITableDataMgr<TData> where TData : TDBaseSkillData, new()
    {
        public struct SkillKey
        {
            public string skillId;
            public int lv;
        }

        #region Callback
        /// <summary>
        /// 释放技能
        /// 1.Data:技能数据
        /// 2.成功或者失败
        /// </summary>
        public event Callback<TData,bool> Callback_OnRelease;
        /// <summary>
        /// 添加
        /// </summary>
        public event Callback<TData> Callback_OnAdded;
        /// <summary>
        /// 移除
        /// </summary>
        public event Callback<TData> Callback_OnRemoved;
        /// <summary>
        /// CD结束
        /// </summary>
        public Callback<TData> Callback_OnCDOver;
        #endregion

        #region member variable
        private List<TData> _baseSkills = new List<TData>();
        public List<TData> Skills
        {
            get
            {
                return _baseSkills;
            }

            set
            {
                _baseSkills = value;
            }
        }
        #endregion

        #region life
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            foreach (var item in _baseSkills)
                item.OnUpdate();
        }
        #endregion

        #region set
        public virtual bool Release_Skill(TData skill, SkillReleaseData? data = null)
        {
            if (skill != null)
            {
                CurData = skill;
                bool temp = skill.Release(data);
                Callback_OnRelease?.Invoke(skill, temp);
                return temp;
            }
            return false;
        }
        public bool Release_Skill(int index, SkillReleaseData? releaseData = null)
        {
            TData data = GetSkill(index);
            if (data == null)
                return false;
            Release_Skill(data, releaseData);
            return true;
        }
        public void Interrupt(SkillInteraputType interruptType)
        {
            foreach (var item in _baseSkills)
                if (item.IsRunning)
                    item.Interrupt(interruptType);
        }
        public void AddSkill(string skillName)
        {
            if (!Table.Contains(skillName))
                return;
            TData tempSkill = Table.Find(skillName).Copy() as TData;
            if (tempSkill != null)
            {
                tempSkill.Index = _baseSkills.Count;
                tempSkill.OnBeAdded(Mono);
                _baseSkills.Add(tempSkill);
                Callback_OnAdded?.Invoke(tempSkill);
                tempSkill.Callback_OnCDOver += OnSkillCDOver;
            }
        }
        public void AddSkills(string[] skillNames)
        {
            foreach (var item in skillNames)
                AddSkill(item);
        }

        public void RemoveSkill(TData data)
        {
            Callback_OnRemoved?.Invoke(data);
            _baseSkills.Remove(data);
            data.Callback_OnCDOver -= OnSkillCDOver;
        }
        public void RemoveAllSkills()
        {
            List<TData> temp = new List<TData>(_baseSkills);
            foreach (var item in temp)
                RemoveSkill(item);
        }
        public void RemoveAttack(string skillName)
        {

        }
        #endregion

        #region get
        public TData GetSkill(int index)
        {
            if (Skills.Count > index)
            {
                return Skills[index];
            }
            return null;
        }
        public List<string> GetPrespawnPerforms()
        {
            List<string> ret = new List<string>();

            foreach (var item in Skills)
            {
                ret.AddRange(item.GetPrespawnPerforms());
            }
            return ret;
        }
        #endregion

        #region is
        public bool IsHaveSkill(int index)
        {
            if (Skills.Count > index)
            {
                return true;
            }
            return false;
        }
        public bool IsCDOver(int index)
        {
            if (!IsHaveSkill(index))
                return false;
            return Skills[index].IsCDOver;
        }
        #endregion

        #region override
        public virtual LuaTDMgr<TData> Table
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual TData CurData { get; set; }

        public virtual void OnAnimTrigger(int param)
        {
        }
        protected virtual void OnSkillCDOver(TDBaseSkillData skillData)
        {
            Callback_OnCDOver?.Invoke(skillData as TData);
        }
        #endregion

    }

}