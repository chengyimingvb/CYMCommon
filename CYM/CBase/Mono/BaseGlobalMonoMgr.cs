using System;
using System.Collections.Generic;
using System.Reflection;
using CYM;
using UnityEngine;
//using WhatA2D;
using UnityEngine.Profiling;

//**********************************************
// Discription	：Base Core Calss .All the Mono will inherit this class
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class BaseGlobalMonoMgr : MonoBehaviour
    {
        /// <summary>
        /// 需要暂停的mono类型
        /// </summary>
        static MonoType PauseType= MonoType.None;

        public static List<BaseCoreMono> UpdateIns = new List<BaseCoreMono>();
        public static List<BaseCoreMono> FixedUpdateIns = new List<BaseCoreMono>();
        public static List<BaseCoreMono> LateUpdateIns = new List<BaseCoreMono>();
        public static List<BaseCoreMono> GUIIns = new List<BaseCoreMono>();
        public static List<BaseCoreMono> GizmosIns = new List<BaseCoreMono>();

        static List<BaseCoreMono> update_addList = new List<BaseCoreMono>();
        static List<BaseCoreMono> update_removeList = new List<BaseCoreMono>();

        static List<BaseCoreMono> fixedupdate_addList = new List<BaseCoreMono>();
        static List<BaseCoreMono> fixedupdate_removeList = new List<BaseCoreMono>();

        static List<BaseCoreMono> lateupdate_addList = new List<BaseCoreMono>();
        static List<BaseCoreMono> lateupdate_removeList = new List<BaseCoreMono>();
        static List<BaseCoreMono> gui_addList = new List<BaseCoreMono>();
        static List<BaseCoreMono> gui_removeList = new List<BaseCoreMono>();
        static List<BaseCoreMono> gizmos_addList = new List<BaseCoreMono>();
        static List<BaseCoreMono> gizmos_removeList = new List<BaseCoreMono>();

        /// <summary>
        /// 设置暂停类型
        /// </summary>
        /// <param name="type"></param>
        public static void SetPauseType(MonoType type)
        {
                PauseType = type;
        }

        public static void ActiveMono(BaseCoreMono mono)
        {
            mono.IsEnable = true;
        }
        public static void DeactiveMono(BaseCoreMono mono)
        {
            mono.IsEnable = false;
        }
        public static void AddMono(BaseCoreMono mono)
        {
            if (mono.NeedUpdate)
            {
                update_addList.Add(mono);
            }
            if(mono.NeedLateUpdate)
            {
                lateupdate_addList.Add(mono);
            }
            if (mono.NeedGUI)
            {
                gui_addList.Add(mono);
            }
            if (mono.NeedFixedUpdate)
            {
                fixedupdate_addList.Add(mono);
            }
        }
        public static void RemoveMono(BaseCoreMono mono)
        {
            if (mono.NeedUpdate)
            {
                update_removeList.Add(mono);
            }
            if (mono.NeedLateUpdate)
            {
                lateupdate_removeList.Add(mono);
            }
            if(mono.NeedGUI)
            {
                gui_removeList.Add(mono);
            }
            if (mono.NeedFixedUpdate)
            {
                fixedupdate_removeList.Add(mono);
            }
        }
        public static void RemoveAllNull()
        {
            UpdateIns.RemoveAll((p) => p == null);
            FixedUpdateIns.RemoveAll((p) => p == null);
            LateUpdateIns.RemoveAll((p) => p == null);
            GUIIns.RemoveAll((p)=>p==null);
            GizmosIns.RemoveAll((p) => p == null);
        }

        bool IsPause(BaseCoreMono mono)
        {
            if (mono.IsEnable == false)
                return true;

            return (PauseType & mono.MonoType) != 0;
        }

        void Awake()
        {
        }
        private void Start()
        {

        }
        void Update()
        {
            foreach (var item in UpdateIns)
            {
                if (IsPause(item))
                    continue;
                item.OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (var item in FixedUpdateIns)
            {
                if (IsPause(item))
                    continue;
                item.OnFixedUpdate();
            }
        }


        public void LateUpdate()
        {
            AddList(UpdateIns, update_addList);
            RemoveList(UpdateIns, update_removeList);

            AddList(FixedUpdateIns, fixedupdate_addList);
            RemoveList(FixedUpdateIns, fixedupdate_removeList);

            AddList(LateUpdateIns, lateupdate_addList);
            RemoveList(LateUpdateIns, lateupdate_removeList);

            AddList(GUIIns, gui_addList);
            RemoveList(GUIIns, gui_removeList);

            AddList(GizmosIns,gizmos_addList);
            RemoveList(GizmosIns,gizmos_removeList);

            foreach (var item in LateUpdateIns)
            {
                if (IsPause(item))
                    continue;
                item.OnLateUpdate();
            }
        }

        void OnGUI()
        {
            foreach (var item in GUIIns)
            {
                if (IsPause(item))
                    continue;
                item.OnGUIPaint();
            }
        }
        private void OnDestroy()
        {
            UpdateIns.Clear();
            FixedUpdateIns.Clear();
            LateUpdateIns.Clear();
            GUIIns.Clear();
            GizmosIns.Clear();
        }

        void RemoveList(List<BaseCoreMono> ins, List<BaseCoreMono> list)
        {
            if (list.Count <= 0)
                return;
            foreach (var temp in list)
                ins.Remove(temp);
            list.Clear();
        }
        void AddList(List<BaseCoreMono> ins, List<BaseCoreMono> list)
        {
            if (list.Count <= 0)
                return;
            foreach (var temp in list)
                ins.Add(temp);
            list.Clear();
        }
    }
}