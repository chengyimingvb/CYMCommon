//**********************************************
// Discription	：CYMBaseCoreComponent
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public abstract class BaseNodeMgr : BaseCoreMgr
    {
        #region member variable
        Dictionary<int, Transform> bones = null;
        Dictionary<string, Transform> extendBones = null;
        protected Transform Model { get; private set; }
        #endregion

        #region property

        #endregion

        #region methon
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            RegetNode();
        }
        public override void Init()
        {

        }
        public override void Birth()
        {
            base.Birth();
        }

        void RegetNode()
        {
            Model = SelfBaseUnit.Trans;
            bones = new Dictionary<int, Transform>();
            extendBones = new Dictionary<string, Transform>();
            bones.Clear();
            extendBones.Clear();
            Transform[] trans = Mono.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; ++i)
            {
                OnMapNodes(trans[i]);

                if (trans[i].name == BaseConstMgr.STR_Model)
                    Model = trans[i];
            }

            BaseBone[] bonescom = Mono.GetComponentsInChildren<BaseBone>();
            if(bonescom != null)
            {
                foreach(var item in bonescom)
                {
                    int index = (int)item.Type;
                    if (index != -1)
                    {
                        if (bones.ContainsKey(index))
                        {
                            bones[index] = item.Trans;
                        }
                        else
                        {
                            bones.Add(index, item.Trans);
                        }
                    }
                    else
                    {
                        string name = item.ExtendName;
                        if (extendBones.ContainsKey(name))
                        {
                            CLog.Error("ExtenBone 名称重复:{0}", name);
                        }
                        else
                        {
                            extendBones.Add(name, item.Trans);
                        }
                    }
                }

            }
        }


        //protected void AddBone(NodeType nodeType,Transform trans)
        //{
        //    int index = (int)nodeType;
        //    if (bones.ContainsKey(index))
        //    {
        //        bones[index] = trans;
        //    }
        //    else
        //        bones.Add(index, trans);
        //}

        public Transform GetExtendBone(string name)
        {
            if (name.IsInvStr())
                return null;
            if (extendBones.ContainsKey(name))
            {
                return extendBones[name];
            }
            else
            {
                CLog.Error("没有这个拓展骨骼:{0}", name);
                return null;
            }
        }

        public Transform GetBone(NodeType nodeType)
        {
            if (nodeType == NodeType.None)
                return null;
            if (bones == null)
                return null;
            int boneIndex = (int)nodeType;
            if (!bones.ContainsKey(boneIndex))
            {
                if (nodeType == NodeType.Center ||
                    nodeType == NodeType.Top ||
                    nodeType == NodeType.Pivot||
                    //nodeType == NodeType.Footsteps ||
                    nodeType == NodeType.Muzzle)
                {
                    if (Model == null)
                        CLog.Error("单位没有Model error id=" + ID);
                    GameObject temp = new GameObject("pos-" + nodeType);                   
                    temp.transform.parent = Model;
                    temp.transform.localScale = Vector3.one;
                    temp.transform.localRotation = Quaternion.identity;
                    temp.transform.position = GetVirtualPos(nodeType);
                    SelfBaseGlobal.CommonCoroutine.Run(DelayPos(temp, nodeType));
                    bones.Add((int)nodeType, temp.transform);
                }
                else
                {
                    CLog.Error("no this bone in the unit" + Mono.name + " ,error bone name=" + nodeType);
                    return null;
                }
            }
            //var ret = bones[boneIndex];
            //ret.transform.position = GetVirtualPos(nodeType);
            return bones[boneIndex];
        }
        IEnumerator<float> DelayPos(GameObject go, NodeType type)
        {
            yield return Timing.WaitForOneFrame;
            if(go!=null)
                go.transform.position = GetVirtualPos(type);
        }
        public Vector3 GetPos(NodeType nodeType)
        {
            if (nodeType == NodeType.None)
                return BaseConstMgr.FarawayPos;
            Transform boneTrans = GetBone(nodeType);
            if (boneTrans == null)
            {
                CLog.Error("no this bone in the unit" + Mono.name + " ,error bone name=" + nodeType);
                return Vector3.one;
            }
            return boneTrans.position;

        }

        private Vector3 GetVirtualPos(NodeType nodeType)
        {
            if (nodeType == NodeType.Top)
                return GetTop();
            if (nodeType == NodeType.Center)
                return GetCenter();
            if (nodeType == NodeType.Pivot)
                return Mono.Pos;
            //if (nodeType == NodeType.Footsteps)
            //    return Mono.Pos;
            return GetCenter();
        }

        public Vector3 GetCenter()
        {
            return Mono.Pos + GetHight() * 0.5f * Vector3.up;//
        }
        public Vector3 GetTop()
        {
            return Mono.Pos + GetHight() * Vector3.up;// + Mono.Pos;
        }
        #endregion

        #region 
        /// <summary>
        /// 映射节点
        /// </summary>
        protected virtual void OnMapNodes(Transform trans)
        {
            //throw new NotImplementedException("这个函数必须实现");
        }
        public virtual float GetHight()
        {

            throw new NotImplementedException("这个函数必须实现");
        }
        #endregion
    }

}