//------------------------------------------------------------------------------
// SubTriggerObject.cs
// Copyright 2018 2018/4/4 
// Created by CYM on 2018/4/4
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public interface ITriggerObject
    {
        void DoTriggerObjectEnter(Collider other, TriggerObject triggerObj, bool forceSense );
        void DoTriggerObjectExit(Collider other, TriggerObject triggerObj, bool forceSense );
    }
    public class TriggerObject : BaseMono 
    {
        [SerializeField]
        GameObject TriggerObj;
        [SerializeField]
        bool IsSense=false;

        ITriggerObject triggerObject;
        Collider col;

        public override void Awake()
        {
            base.Awake();
            col = GetComponent<Collider>();
            col.isTrigger = true;
            SetTriggerObj(TriggerObj);
        }

        public void SetTriggerObj(GameObject go)
        {
            if (go == null)
                return;
            TriggerObj = go;
            triggerObject = go.GetComponent<ITriggerObject>();
            if (IsSense)
            {
                SetLayer(BaseConstMgr.Layer_Sense,false);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (triggerObject == null)
                return;
            triggerObject.DoTriggerObjectEnter(other,this,false);
        }

        public void OnTriggerExit(Collider other)
        {
            if (triggerObject == null)
                return;
            triggerObject.DoTriggerObjectExit(other,this,false);
        }

        public bool IsSenseObj()
        {
            return Layer == (int)BaseConstMgr.Layer_Sense;
        }

    }
}