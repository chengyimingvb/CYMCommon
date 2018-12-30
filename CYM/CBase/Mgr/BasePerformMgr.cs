using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BasePerformMgr : BaseCoreMgr
    {

        #region member variable
        protected List<BasePerform> data = new List<BasePerform>();

        #endregion

        #region property

        #endregion

        #region methon
        public virtual void ReBirthInit()
        {
            //ParticleSystem particleSystem;
            //particleSystem.e
        }

        public virtual void OnRealDeath()
        {

        }

        public override void OnDisable()
        {
            //删除角色身上的所有特效
            for (int i = 0; i < data.Count; ++i)
            {
                if (data[i].IsAttachedUnit)
                    Despawn(data[i]);
            }
            base.OnDisable();
        }

        public virtual T Spawn<T>(string performName,BaseCoreMono castMono, Vector3? position = null,Quaternion? quaternion=null, params object[] ps) where T : BasePerform
        {
            if (performName.IsInvStr())
                return null;
            if (SelfBaseGlobal.PoolMgr.Perform == null)
                return null;
            GameObject temp = null;
            temp = SelfBaseGlobal.PoolMgr.Perform.Spawn(SelfBaseGlobal.GRMgr.GetPerform(performName), position, quaternion);
            return Spawn<T>(castMono, position, quaternion, temp, ps);
        }

        public virtual T Spawn<T>(GameObject prefab, BaseCoreMono castMono, Vector3? position = null, Quaternion? quaternion = null, params object[] ps) where T : BasePerform
        {
            if (prefab==null)
                return null;
            var temp = SelfBaseGlobal.PoolMgr.Perform.Spawn(prefab, position, quaternion);
            return Spawn<T>(castMono, position, quaternion, temp, ps);
        }

        private T Spawn<T>(BaseCoreMono castMono, Vector3? position, Quaternion? quaternion, GameObject temp,params object[] ps) where T : BasePerform
        {
            if (temp == null)
                return null;
            if (position.HasValue)
                temp.transform.position = position.Value;
            if (quaternion.HasValue)
                temp.transform.rotation = quaternion.Value;
            T temPerform = BaseCoreMono.GetUnityComponet<T>(temp);
            //特效创建之时先将碰撞合都设为禁用,因为这个时候回掉函数还没注册
            temPerform.SetCollidersActive(false);
            if (position.HasValue || quaternion.HasValue)
                temPerform.UseFollow = false;
            else
                temPerform.UseFollow = true;
            temPerform.OnCreate(Mono, castMono==null? Mono: castMono, ps);
            //回掉函数注册完以后开启碰撞盒
            temPerform.SetCollidersActive(true);
            temPerform.Callback_OnDoDestroy += OnPerformDoDestroy;
            data.Add(temPerform);
            return temPerform;
        }

        public void Despawn(BasePerform perform)
        {
            if (perform != null)
            {
                float closeTime = perform.CloseTime;
                GameObject mono = perform.GO;
                perform.Callback_OnTriggerIn = null;
                perform.Callback_OnTriggering = null;
                perform.Callback_OnTriggerOut = null;
                perform.Callback_OnDoDestroy = null;
                perform.Callback_OnLifeOver = null;
                perform.OnClose();
                data.Remove(perform);

                SelfBaseGlobal.PoolMgr.Perform.Despawn(mono, closeTime);

                perform = null;
            }
        }
        public void DespawnAll()
        {
            var temp = new List<BasePerform>(data) ;
            foreach (var item in temp)
            {
                Despawn(item);
            }
        }
        public void SetVisible(bool b)
        {
            for (int i = 0; i < data.Count; ++i)
            {
                data[i].SetVisible(b);
            }
        }
        #endregion

        #region CallBack

        void OnPerformDoDestroy(BasePerform perform)
        {
            Despawn(perform);
        }
        #endregion

        #region must override

        #endregion
    }

}