using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class BaseWeapon<T> : BaseUnit where T: BaseAttackObj
    {
        #region set
        public virtual void SetAttackObj(List<T> objs, List<T> selfObjs, int index)
        {
        }
        #endregion

        #region get
        /// <summary>
        /// 通过索引获得AttackObj,如果超出索引则获取第一个,如果没有则获得null
        /// </summary>
        /// <param name="AttackObjs"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual T GetObj(List<T> AttackObjs, int index)
        {
            if (AttackObjs == null)
                return null;
            if (index < 0)
                return null;
            if (index >= AttackObjs.Count)
            {
                return AttackObjs[0];
            }
            return AttackObjs[index];
        }
        #endregion
    }

}