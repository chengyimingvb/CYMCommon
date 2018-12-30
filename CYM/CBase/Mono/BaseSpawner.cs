//**********************************************
// Class Name	: BaseSpawner
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
using System;
using Sirenix.OdinInspector;

namespace CYM
{
    public class BaseSpawner<T> : BaseCoreMono where T : BaseMono
    {
        #region inspector
        [FoldoutGroup("Base"), PreFabOverride, SerializeField, OnValueChanged("OnPropChanged", true)]
        protected string CustomName = "";
        [FoldoutGroup("Base"), SerializeField, PreFabOverride, OnValueChanged("OnPropChanged", true)]
        public string TDID, TDID1;
        [FoldoutGroup("Base"), MinValue(0), PreFabOverride,SerializeField, OnValueChanged("OnPropChanged", true)]
        public int Team = 1;
        [FoldoutGroup("Base"), MinValue(0), SerializeField, PreFabOverride]
        protected float Delay = 0.0f;
        [FoldoutGroup("Base"),MinValue(1),MaxValue(10), SerializeField,PreFabOverride,Tooltip("小于等于0表示不限次数")]
        protected int MaxSpawnCount = 1;
        [FoldoutGroup("Trigger"), SerializeField, PreFabOverride,ShowIf("Inspector_ShowIsAutoSpawn")]
        public bool IsAutoSpawn = false;
        #endregion

        #region prop
        protected CoroutineHandle coroutineHandle;
        public int SpawnedCount { get; protected set; } = 0;
        public List<string> SpawnTDIDList { get; private set; } = new List<string>();
        /// <summary>
        /// 至少已经Spawn了一个单位,会被标记为IsActived=true
        /// </summary>
        public bool IsActived { get; protected set; } = false;
        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            SpawnTDIDList.Clear();
            if (!TDID.IsInvStr())
                SpawnTDIDList.Add(TDID);
            if (!TDID1.IsInvStr())
                SpawnTDIDList.Add(TDID1);
        }
        public override void Start()
        {
            base.Start();
            if (IsAutoSpawn)
                DoSpawn();
        }
        #endregion

        #region set
        /// <summary>
        /// 手动调用
        /// </summary>
        /// <param name="delay"></param>
        public void DoSpawn()
        {
            if (!IsCanSpawn())
                return;
            IsActived = true;
            if (MaxSpawnCount > 0)
            {
                if (SpawnedCount >= MaxSpawnCount)
                    return;
            }
            SpawnedCount++;
            if (Delay == 0)
            {
                SpawnInternel();
            }
            else
            {
                SelfBaseGlobal.BattleCoroutine.Kill(coroutineHandle);
                coroutineHandle = SelfBaseGlobal.BattleCoroutine.Run(_Spawn(Delay));
            }
        }
        /// <summary>
        /// 生成单位
        /// </summary>
        protected virtual void SpawnInternel()
        {

        }
        /// <summary>
        /// 检查是否死亡
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIsDeath()
        {
            return false;
        }
        #endregion

        #region get 
        protected string RandTDID
        {
            get
            {
                if (SpawnTDIDList == null || SpawnTDIDList.Count <= 0)
                    return TDID;
                return BaseMathUtils.RandArray(SpawnTDIDList);
            }
        }
        protected string FirstTDID
        {
            get
            {
                return TDID;
            }
        }
        public override string Name
        {
            get
            {
                if (!CustomName.IsInvStr())
                    return CustomName;
                return name;
            }
        }
        #endregion

        #region is
        /// <summary>
        /// 是否可以Spawn
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsCanSpawn()
        {
            return true;
        }
        public bool IsMaxSpawnCount
        {
            get
            {
                if (MaxSpawnCount <= 0)
                    return false;
                return SpawnedCount >= MaxSpawnCount;
            }
        }
        #endregion

        #region inspector
        protected override void OnFirstDrawGizmos()
        {
            base.OnFirstDrawGizmos();
            RefreshGizmos();
        }
        string[] spliteNames;
        protected virtual void OnPropChanged()
        {
            RefreshGizmos();
        }
        public virtual void RefreshGizmos()
        {
            if (!CustomName.IsInvStr())
            {
                gameObject.name = CustomName;
            }
            else
            {
                if (FirstTDID == null)
                    return;
                spliteNames = FirstTDID.Split('_');
                if (spliteNames.Length >= 2)
                    gameObject.name = spliteNames[1];
                else
                    gameObject.name = FirstTDID;
            }
        }
        #endregion

        #region IEnumerator
        IEnumerator<float> _Spawn(float delay)
        {
            yield return Timing.WaitForSeconds(delay);
            SpawnInternel();
        }
        #endregion

        #region inspector
        protected virtual bool Inspector_ShowIsAutoSpawn()
        {
            return true;
        }
        [Button(ButtonSizes.Small),PropertyOrder(-10)]
        private void DuplicateName()
        {
            BaseUtils.CopyTextToClipboard(Name);
        }
        #endregion
    }
}