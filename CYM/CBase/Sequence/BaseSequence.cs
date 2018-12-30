//**********************************************
// Class Name	: BaseSequence
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CYM;

namespace CYM
{
	public class BaseSequence
    {
        protected object[] AddedObjs { get; private set; }
        protected CoroutineHandle Handle;
        protected BaseGlobal SelfBaseGlobal;
        protected BaseUnit SelfBaseUnit;
        protected BaseCoreMono SelfMono;

        public BaseSequence()
        {
            SelfBaseGlobal = BaseGlobal.Ins;
        }

        public virtual void Init(BaseCoreMono mono)
        {
            SelfMono = mono;
            SelfBaseGlobal = BaseGlobal.Ins;
            if (SelfMono is BaseUnit)
                SelfBaseUnit = SelfMono as BaseUnit;

        }

        protected TType GetAddedObjData<TType>(int index, TType defaultVal = default(TType)) 
        {
            if (AddedObjs == null || AddedObjs.Length <= index)
                return defaultVal;
            return (TType)(AddedObjs[index]);
        }

        public virtual void Start(params object[] ps)
        {
            AddedObjs = ps;
            SelfBaseGlobal.BattleCoroutine.Kill(Handle);
            Handle = SelfBaseGlobal.BattleCoroutine.Run(_ActionSequence());
            IsInState = true;
        }

        public void Stop()
        {
            if (Handle.IsRunning)
            {
                OnStop();
            }
            //if (Handle != null)
            {
                //Timing.KillCoroutines(Handle);
                SelfBaseGlobal.BattleCoroutine.Kill(Handle);
            }
        }

        public bool IsInState { get; protected set; } = false;
                

        protected virtual void OnStop()
        {
            IsInState = false;
        }
        protected virtual IEnumerator<float> _ActionSequence()
        {
            yield return Timing.WaitForOneFrame;
        }
    }
}