
//**********************************************
// Class Name	: Pool
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using CYM.Pool;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace CYM
{
    public class BaseCoroutineMgr 
    {
        string tag;
        public BaseCoroutineMgr(string tag)
        {
            this.tag = tag;
        }
        public CoroutineHandle Run(IEnumerator<float> coroutine)
        {
            return Timing.RunCoroutine(coroutine, Segment.Update,tag);
        }
        public void RunList(params IEnumerator<float>[] coroutine)
        {
            foreach (var item in coroutine)
                Run(item);
        }
        public void Kill()
        {
            Timing.KillCoroutines(tag);
        }
        public void Kill(CoroutineHandle coroutine)
        {
            coroutine.IsRunning = false;
        }
        public void Pause()
        {
            Timing.PauseCoroutines(tag);
        }
        public void Resume()
        {
            Timing.ResumeCoroutines(tag);
        }

    }
}
