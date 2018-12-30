using CYM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class FlickerEffect : BaseMono
    {    
        public float pauzeBeforeStart = 1.3f;
        public float flickerSpeedStart = 15f;
        public float flickerSpeedEnd = 35f;
        public float Duration = 2f;
        public bool DestroyOnFinish=false;

        public MeshRenderer[] GFX;

        CoroutineHandle CoroutineHandle;

        public override void Awake()
        {
            GFX = GetComponentsInChildren<MeshRenderer>();
        }

        public void StopEffect()
        {
            //if (CoroutineHandle != null)
            BaseGlobal.Ins.BattleCoroutine.Kill(CoroutineHandle);
            foreach (var g in GFX) g.enabled = (true);
        }

        public void StartEffect(float pauzeBeforeStart=0.0f,float duration=1.0f)
        {
            this.pauzeBeforeStart = pauzeBeforeStart;
            this.Duration = duration;
            //if (CoroutineHandle != null)
            BaseGlobal.Ins.BattleCoroutine.Kill(CoroutineHandle);
            CoroutineHandle = BaseGlobal.Ins.BattleCoroutine.Run(FlickerCoroutine());
        }

        IEnumerator<float> FlickerCoroutine()
        {

            //pause before start
            yield return Timing.WaitForSeconds(pauzeBeforeStart); //new WaitForSeconds(pauzeBeforeStart);

            //flicker
            float t = 0;
            while (t < 1)
            {
                float speed = Mathf.Lerp(flickerSpeedStart, flickerSpeedEnd, BaseMathUtils.Coserp(0, 1, t));
                float i = Mathf.Sin(Time.time * speed);
                foreach (var g in GFX) g.enabled=(i > 0);
                t += Time.deltaTime / Duration;
                yield return Timing.WaitForOneFrame;
            }

            //show
            //foreach (var g in GFX) g.enabled=(true);

            //destroy
            if (DestroyOnFinish)
            {
                //Destroy(gameObject);
                BaseGlobal.Ins.PoolMgr.Perform.Despawn(this);
            }
        }
    }

}