//**********************************************
// Class Name	: CYMStateMathine
// Discription	：State Mathine. Useful calss for AI
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using UnityEngine;

namespace CYM
{
    public class State<T2>
    {
        public float UpdateTime = 0.0f;
        public virtual void ExecuteLogic(T2 human) { }
        public virtual void ExecutePhysical(T2 human) { }
        public virtual void Update(T2 human)
        {
            UpdateTime += Time.deltaTime;
        }
        public virtual void Enter(T2 human)
        {
            UpdateTime = 0;
        }
        public virtual void Exit(T2 human) { }
    }

    public class StateMachine<T> where T : class
    {
        public StateMachine()
        {
            owner = null;
            curState = null;
            preState = null;
            globalState = null;
        }
        public StateMachine(State<T> curState)
        {
            owner = null;
            this.curState = curState;
            preState = null;
            globalState = null;
        }

        public void SetOwner(T owner)
        {
            this.owner = owner;
        }

        public void SetCurState(State<T> state) { curState = state; curState.Enter(owner); }
        public void SetPreState(State<T> state) { preState = state; }
        public void SetGlobalState(State<T> state) { globalState = state; }
        public State<T> GetCurState() { return curState; }
        public State<T> GetPreState() { return preState; }
        public State<T> GetGlobalState() { return preState; }
        public bool IsInState(State<T> state)
        {
            return curState == state;
        }
        public void Update()
        {
            if (globalState != null)
                globalState.Update(owner);
            if (curState != null)
                curState.Update(owner);
        }
        public void UpdateLogic()
        {
            if (globalState != null)
                globalState.ExecuteLogic(owner);
            if (curState != null)
                curState.ExecuteLogic(owner);
        }
        public void UpdatePhysical()
        {
            if (globalState != null)
                globalState.ExecutePhysical(owner);
            if (curState != null)
                curState.ExecutePhysical(owner);
        }
        public void ChangeState(State<T> state)
        {
            if (state == null)
                return;
            preState = curState;
            if(curState!=null)curState.Exit(owner);
            curState = state;
            curState.Enter(owner);
        }
        public void RevertToPreState()
        {
            ChangeState(preState);
        }

        private T owner;
        State<T> curState;
        State<T> preState;
        State<T> globalState;

        public T Owner
        {
            get
            {
                return owner;
            }

            set
            {
                owner = value;
            }
        }
    }
}
