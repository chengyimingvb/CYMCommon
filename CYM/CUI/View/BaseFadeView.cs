//**********************************************
// Class Name	: BaseFadeView
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

namespace CYM.UI
{
	public class BaseFadeView : BaseUIView
	{

        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
            //SelfBaseGlobal.CommonCoroutine.Run(_InitFade());
        }

        //IEnumerator<float> _InitFade()
        //{
        //    yield return Timing.WaitForSeconds(1.0f);
        //    Show(false, 0.5f);
        //}

    }
}