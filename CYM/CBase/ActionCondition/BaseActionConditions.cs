using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;

namespace CYM
{
    #region 条件
    public class BaseTarget
    {
        public ACCType ACCType;
        public int IntParam1;
        public int IntParam2;
        public float FloatParam1;
        public string StrParam1;

        public virtual void DoCondition()
        {

        }
    }
    #endregion

    #region 基类
    /// <summary>
    /// 动作条件
    /// </summary>
    public class BaseActionCondition
    {
        protected ACCompareType CompareType;
        public bool IsIgnore { get { return isIgnore; } }
        protected bool isInvert = false;
        protected bool isInvertFalg = false;
        protected bool isIgnore = false;
        protected bool isIgnoreFlag = false;
        protected bool isTrue = false;
        protected float val = 0.0f;
        public bool IsCost { get; protected set; } = false;
        public void DoAction()
        {
            if (isIgnore)
            {
                isTrue = true;
            }
            else
            {
                bool r = DoActionImpl();
                isTrue = isInvert ? !r : r;
            }
        }

        public virtual bool DoActionImpl()
        {
            return true;
        }

        public void Reset()
        {
            reset();
        }

        void reset()
        {
            this.isTrue = false;
            this.isInvert = isInvertFalg;
            isInvertFalg = false;
            this.isIgnore = isIgnoreFlag;
            isIgnoreFlag = false;
        }
        //取反
        public BaseActionCondition Invert()
        {
            isInvertFalg = true;
            return this;
        }
        public BaseActionCondition Ignore(bool b)
        {
            isIgnoreFlag = b;
            return this;
        }
        public BaseActionCondition Compare(ACCompareType type, float val)
        {
            this.CompareType = type;
            this.val = val;
            return this;
        }
        public virtual bool GetRet()
        {
            if (isIgnore)
                return true;
            return isTrue;
        }
        protected string SetDesc(string str = "", params object[] obs)
        {
            str = BaseLanguageMgr.Get(str, obs);
            return BaseUIUtils.Condition(GetRet(), str);
            //if (GetRet())
            //    return BaseConstansMgr.STR_Indent + "<Color=green>" + str + "</Color>";
            //return BaseConstansMgr.STR_Indent + "<Color=red>" + str + "</Color>";
        }
        public virtual string GetDesc()
        {
            return "";
        }
        public virtual string GetCost()
        {
            return "";
        }
    }
    #endregion

    #region Cost
    /// <summary>
    /// 消耗判断
    /// </summary>
    public class BaseCostCondition<TUnit,Type> : BaseActionCondition where Type:struct where TUnit:BaseUnit
    {
        public List<CostData<Type>> CostDatas;
        public TUnit Unit;

        public BaseCostCondition() : base()
        {
            CostDatas = null;
            IsCost = true;
        }
        public BaseCostCondition<TUnit, Type> SetCost(List<CostData<Type>> datas)
        {
            if (datas == null)
            {
                throw new System.ArgumentNullException("datas");
            }
            CostDatas = datas;
            return this;
        }
        public BaseCostCondition<TUnit, Type> SetUnit(TUnit unit)
        {
            Unit = unit ;
            return this;
        }
        public override bool DoActionImpl()
        {
            if (CostDatas == null)
                return true;
            foreach (var item in CostDatas)
            {
                if (item.IsCondition)
                {
                    if (GetAttrVal(item.Type) < item.RealVal)
                        return false;
                }
            }
            return true;
        }
        public override string GetDesc()
        {
            string retStr = "";
            if (CostDatas == null)
                return retStr;
            foreach (var item in CostDatas)
            {
                if (item.RealVal == 0)
                    continue;
                if (!item.IsCondition)
                    continue;
                bool tempBool = true;
                if (BaseMathUtils.Round(GetAttrVal(item.Type), 2) < item.RealVal)
                    tempBool = false;
                string tempstr = BaseLanguageMgr.Get("AC_IsAttrToAct", (item.Type as Enum).GetName(), ACCompareType.MoreEqual.GetName(), item.ToString(false, false, false));
                if (tempBool)
                    retStr += BaseConstMgr.STR_Indent + "<Color=green>" + tempstr + "</Color>";
                else
                    retStr += BaseConstMgr.STR_Indent + "<Color=red>" + tempstr + "</Color>";
            }
            return retStr;
        }
        public override string GetCost()
        {
            return CostDatas.ToString("", BaseConstMgr.STR_Append);
        }

        #region 必须重载的函数
        protected virtual float GetAttrVal(Type type)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    #endregion

    #region 简单条件
    /// <summary>
    /// 简单的条件
    /// </summary>
    public class SimpleActionCondition
    {
        SimpleActionCondition(bool b, string key, params object[] objs)
        {
            isTrue = b;
            this.objs = objs;
            this.key = key;
        }

        public string key;
        public object[] objs;
        //public string desc;
        public bool isTrue;
        public string GetDesc()
        {
            string str = BaseLanguageMgr.Get(key, objs);
            return BaseUIUtils.Condition(isTrue, str);
            //if (isTrue)
            //    return BaseConstansMgr.STR_Indent + "<Color=green>" + str + "</Color>";
            //return BaseConstansMgr.STR_Indent + "<Color=red>" + str + "</Color>";
        }
    }
    #endregion
}