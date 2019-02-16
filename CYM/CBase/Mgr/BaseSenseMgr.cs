using System;
using System.Collections.Generic;
using UnityEngine;
namespace CYM
{
    public class BaseSenseMgr<TUnit> : BaseCoreMgr, IBaseSenseMgr where TUnit : BaseUnit
    {
        #region prop
        protected GameObject SenseGameObj;
        protected SphereCollider SphereCollider;
        protected BaseSenseObj SenseObject;
        protected Timer Timer = new Timer();
        protected Collider[] ColliderResults;
        protected virtual LayerData CheckLayer { get { throw new NotImplementedException("必须重载"); } }
        protected virtual float UpdateTimer => float.MaxValue;
        protected virtual float Radius => 4;
        public List<TUnit> SenseUnits { get; private set; } = new List<TUnit>();
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
            Timer = new Timer(UpdateTimer);
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
                foreach (var item in ColliderResults)
                {
                    OnTriggerEnter(item);
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

        #region utile
        protected void CustomCollect()
        {
            SenseUnits.Clear();
            int count = Physics.OverlapSphereNonAlloc(SelfBaseUnit.Pos, Radius, ColliderResults, (LayerMask)CheckLayer, QueryTriggerInteraction.Ignore);
            {
                foreach (var item in ColliderResults)
                {
                    TUnit unit = item.GetComponent<TUnit>();
                    if (unit != null)
                    {
                        SenseUnits.Add(unit);
                    }
                }
            }
        }
        #endregion
    }

}