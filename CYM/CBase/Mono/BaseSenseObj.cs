using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseSenseObj : BaseMono
    {
        #region prop
        BaseSenseMgr SenseMgr;
        #endregion

        #region set
        public void Init(BaseSenseMgr sense)
        {
            SenseMgr = sense;
        }
        #endregion

        protected virtual void OnTriggerEnter(Collider other)
        {
            SenseMgr.OnTriggerEnter(other);
        }

        protected void OnTriggerExit(Collider other)
        {
            SenseMgr.OnTriggerExit(other);
        }
    }

}