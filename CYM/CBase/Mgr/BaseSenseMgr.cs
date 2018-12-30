using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace CYM
{
    public class BaseSenseMgr : BaseCoreMgr
    {
        #region prop
        public float Radius { get; private set; } = 4;
        protected GameObject SenseGameObj;
        protected SphereCollider SphereCollider;
        protected BaseSenseObj SenseObject;
        protected Timer Timer = new Timer(3.0f);
        protected Collider[] ColliderResults;
        protected virtual LayerData CheckLayer { get { throw new NotImplementedException("必须重载"); } }
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SenseGameObj = new GameObject("SenseObj");
            SenseGameObj.layer = (int)BaseConstMgr.Layer_Sense;
            SenseGameObj.transform.SetParent(Mono.Trans);
            SenseGameObj.transform.localPosition = Vector3.zero;
            SenseGameObj.transform.localScale = Vector3.one;
            SenseGameObj.transform.localRotation = Quaternion.identity;
            SenseObject = BaseMono.GetUnityComponet<BaseSenseObj>(SenseGameObj);
            SenseObject.Init(this);
            SphereCollider = SenseGameObj.AddComponent<SphereCollider>();
            SphereCollider.isTrigger = true;
            SphereCollider.radius = Radius;
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (Timer.CheckOver())
            {
                int count = Physics.OverlapSphereNonAlloc(SelfBaseUnit.Pos, Radius, ColliderResults, (LayerMask)CheckLayer, QueryTriggerInteraction.Ignore);
                if (count > 0)
                {
                    foreach (var item in ColliderResults)
                    {
                        OnTriggerEnter(item);
                    }
                }
            }
        }
        #endregion

        #region override
        public virtual void OnTriggerEnter(Collider col)
        {

        }
        public virtual void OnTriggerExit(Collider col)
        {

        }
        #endregion
    }

}