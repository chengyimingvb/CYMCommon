using System;
using System.Collections.Generic;
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
    public class TDBaseBuffData<TType> : BaseConfig<TDBaseBuffData<TType>> where TType : struct
    {
        #region config
        public List<AttrAdditon<TType>> IntervalAttr { get; set; } = new List<AttrAdditon<TType>>();
        public List<AttrAdditon<TType>> Attr { get; set; } = new List<AttrAdditon<TType>>();
        public List<AttrConvertData<TType>> Convert { get; set; } = new List<AttrConvertData<TType>>();
        public int MaxLayer { get; set; } = 1;
        public bool IsHide { get; set; } = true;
        public float MaxTime { get; set; } = 0;
        public int IntervalTime { get; set; } = 0;
        public string BuffGroupID { get; set; }//有配置Buff组的会叠加,没有配置Buff组的会合并
        public ImmuneGainType Immune { get; set; } = ImmuneGainType.Positive;
        #endregion

        #region runtime
        protected BaseUnit R_Caster = null;
        protected TDBaseSkillData R_Skill=null;
        protected List<AttrAdditon<TType>> R_Attr = null;
        protected List<AttrConvertData<TType>> R_Convert = null;
        public int MergeLayer;
        public float CurTime = 0;
        public float Step;
        public float CurInterval = 0;
        public float PercentCD
        {
            get
            {
                if (MaxTime <= 0)
                    return 0.0f;
                return CurTime / MaxTime;
            }
        }
        #endregion

        #region def
        public const float MAX_TIME = float.MaxValue;
        public const float NO_INTERVAL = 0.0f;
        public const float FOREVER = 0.0f;
        public static int MAX_LAYER = 5;
        public static float TIME_STEP = 1.0f;
        #endregion

        #region func
        /// <summary>
        /// obj1:来源对象
        /// obj2:来源技能
        /// </summary>
        /// <param name="mono"></param>
        /// <param name="obj"></param>
        public override void OnBeAdded(BaseCoreMono mono, params object[] obj)
        {
            base.OnBeAdded(mono, obj);
            R_Caster = GetAddedObjData<BaseUnit>(0);
            R_Skill = GetAddedObjData<TDBaseSkillData>(1);
            R_Attr = AttrMgr.Add(Attr);
            R_Convert = AttrMgr.Add(Convert);
            CurTime = 0;
        }
        public override void OnBeRemoved()
        {
            base.OnBeRemoved();
            AttrMgr.Remove(R_Attr);
            AttrMgr.Remove(R_Convert);
        }
        public virtual void OnMerge(TDBaseBuffData<TType> newBuff, params object[] obj)
        {
            MergeLayer++;
            CurTime = 0;
        }
        protected virtual void OnInterval()
        {
            if (IntervalAttr != null)
            {
                AttrMgr.Add(IntervalAttr);
            }
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (MaxTime != FOREVER)
                CurTime += TIME_STEP;
            if (IntervalTime != NO_INTERVAL)
            {
                CurInterval += TIME_STEP;
                if (CurInterval >= IntervalTime)
                {
                    OnInterval();
                    CurInterval = 0;
                }
            }
        }
        #endregion

        #region is
        /// <summary>
        /// buff时间是否结束
        /// </summary>
        public bool IsTimeOver
        {
            get
            {
                if (MaxTime == FOREVER)
                    return false;
                return CurTime >= MaxTime;
            }
        }
        /// <summary>
        /// 是否永久
        /// </summary>
        /// <returns></returns>
        public bool IsForever
        {
            get
            {
                if (MaxTime == FOREVER)
                    return true;
                return false;
            }
        }
        #endregion

        #region methon
        /// <summary>
        /// 设置属性修改因子
        /// </summary>
        /// <param name="step"></param>
        public virtual void SetStep(float step)
        {
            if (R_Attr == null)
                return;
            Step = step;
            foreach (var item in R_Attr)
            {
                item.SetStep(step);
            }
            AttrMgr.SetDirty();
        }
        /// <summary>
        /// 强制结束Buff
        /// </summary>
        protected void ForceOverBuffTime()
        {
            CurTime = MaxTime;
        }

        #endregion

        #region desc
        /// <summary>
        /// 获取buff的加成描述列表 翻译
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetAddtionDescs(float? anticipationFaction = null)
        {
            List<string> data = new List<string>();
            for (int i = 0; i < Attr.Count; ++i)
                data.Add(Attr[i].GetDesc(false, true, anticipationFaction));
            return data;
        }
        /// <summary>
        /// 通过Layer 获得加成列表
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public virtual List<string> GetAddtionDescsByLayer(int layer = 1)
        {
            List<string> data = new List<string>();
            for (int i = 0; i < Attr.Count; ++i)
                data.Add(Attr[i].GetDescByLayer(false, true, layer));
            return data;
        }
        /// <summary>
        /// 获得buff的加成描述字符串组合
        /// </summary>
        /// <returns></returns>
        public string GetAdtionsDescStr(string splite = "\n")
        {
            string addition = "";
            List<string> temp = GetAddtionDescs();
            for (int i = 0; i < temp.Count; ++i)
            {
                if (i < temp.Count - 1)
                    addition += temp[i] + splite;
                else
                    addition += temp[i];
            }
            return addition;
        }
        public string GetAdtionsDescStrByLayer(int layer = 1, string splite = "\n")
        {
            string addition = "";
            List<string> temp = GetAddtionDescsByLayer(layer);
            for (int i = 0; i < temp.Count; ++i)
            {
                if (i < temp.Count - 1)
                    addition += temp[i] + splite;
                else
                    addition += temp[i];
            }
            return addition;
        }
        #endregion

        #region must override
        /// <summary>
        /// 属性管理器
        /// </summary>
        protected virtual BaseAttrMgr<TType> AttrMgr
        {
            get
            {
                throw new NotImplementedException("此函数没有被实现");
            }
        }
        #endregion
    }

}


