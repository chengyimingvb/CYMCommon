//**********************************************
// Discription	：CYMBaseCoreComponent
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;

namespace CYM
{
    /// <summary>
    /// 组建启动顺序:
    /// OnCreate
    /// OnBeAdded
    /// OnBeAttachedParent(可选)
    /// OnEnable
    /// OnStart
    /// OnUpdate
    /// OnFixUpdate
    /// OnLateUpdate
    /// OnDisable
    /// OnDestroy
    /// </summary>
    public class BaseCoreMgr:ICYMBase, IDBDataConvert, IUnit
    {
        #region member variable
        private int id;
        protected List<BaseCoreMgr> subComponets = new List<BaseCoreMgr>();
        public bool IsSubComponent { get; private set; }
        protected BaseCoreMgr parentComponet { get; set; }
        public bool IsEnable { protected set; get; }
        public bool NeedUpdate { protected set; get; }
        public bool NeedGUI { protected set; get; }
        public bool NeedFixedUpdate { protected set; get; }
        public bool NeedLateUpdate { protected set; get; }
        public bool NeedGameLogicTurn {protected set; get;}
        #endregion

        #region property

        public int ID
        {
            get
            {
                return id;
            }
            set { id=value; }
        }

        public BaseCoreMono Mono { get; private set; }
        public BaseGlobal SelfBaseGlobal { get; protected set; }
        public BaseUnit SelfBaseUnit { get; protected set; }
        public BaseSceneObject BaseSceneObject => BaseSceneObject.Ins;

        public bool Finished
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region methon
        public virtual void Enable(bool b)
        {
            IsEnable = b;
        }

        public T AddSubComponent<T>() where T : BaseCoreMgr, new()
        {
            BaseCoreMgr component = Create<T>();
            subComponets.Add(component);
            component.IsSubComponent = true;
            component.parentComponet = this;
            component.NeedGameLogicTurn = true;
            component.OnBeAttachedToParentComponet(this);
            return (T)component;
        }
        /// <summary>
        /// 组建创建的时候
        /// </summary>
        public virtual void OnCreate()
        {
            IsEnable = true;
            OnSetNeedFlag();
        }
        protected virtual void OnSetNeedFlag()
        {
        }
        /// <summary>
        /// 组建被关联到伏组件的时候
        /// </summary>
        /// <param name="parentComponet"></param>
        protected virtual void OnBeAttachedToParentComponet(BaseCoreMgr parentComponet)
        {
            if (parentComponet.Mono != null)
                OnBeAdded(parentComponet.Mono);
        }
        /// <summary>
        /// 组建被添加到mono的时候
        /// </summary>
        /// <param name="mono"></param>
        public virtual void OnBeAdded(IMono mono)
        {
            Mono = (BaseCoreMono)mono;
            SelfBaseGlobal = BaseGlobal.Ins;
            if (mono is BaseUnit)
                SelfBaseUnit = (BaseUnit)mono;
            foreach (var item in subComponets)
                item.OnBeAdded(Mono);
        }
        /// <summary>
        /// mono的OnEnable
        /// </summary>
        public virtual void OnEnable()
        {
            foreach (var item in subComponets)
                item.OnEnable();
        }
        /// <summary>
        /// mono的OnStart
        /// </summary>
        public virtual void OnStart()
        {
            foreach (var item in subComponets)
                item.OnStart();
        }
        public virtual void OnAffterStart()
        {
            foreach (var item in subComponets)
                item.OnAffterStart();
        }
        /// <summary>
        /// 手动更新
        /// </summary>
        public virtual void ManualUpdate()
        {

        }
        /// <summary>
        /// mono的渲染帧
        /// </summary>
        public virtual void OnUpdate()
        {
            foreach (var item in subComponets)
                if(item.IsEnable)
                    item.OnUpdate();
        }
        /// <summary>
        /// mono的逻辑帧
        /// </summary>
        public virtual void OnFixedUpdate()
        {
            foreach (var item in subComponets)
                if(item.IsEnable)
                    item.OnFixedUpdate();
        }
        /// <summary>
        /// mono的回合帧
        /// </summary>
        public virtual void OnLateUpdate()
        {
            foreach (var item in subComponets)
                if(item.IsEnable)
                    item.OnLateUpdate();
        }
        /// <summary>
        /// mono的日期帧
        /// </summary>
        /// <param name="monthChange"></param>
        /// <param name="yearChange"></param>
        /// 
        public virtual void GameLogicTurn()
        {
            foreach (var item in subComponets)
            {
                if (item.NeedGameLogicTurn&&item.IsEnable)
                    item.GameLogicTurn();
            }      
        }
        /// <summary>
        /// 帧同步
        /// </summary>
        /// <param name="gameFramesPerSecond"></param>
        public virtual void GameFrameTurn(int gameFramesPerSecond)
        {
            foreach (var item in subComponets)
                if(item.IsEnable)
                    item.GameFrameTurn(gameFramesPerSecond);
        }
        /// <summary>
        /// mono的OnGui
        /// </summary>
        public virtual void OnGUIPaint()
        {

        }
        /// <summary>
        /// mono的OnDisable
        /// </summary>
        public virtual void OnDisable()
        {
            foreach (var item in subComponets)
                item.OnDisable();
        }
        /// <summary>
        /// mono的OnDestroy
        /// </summary>
        public virtual void OnDestroy()
        {
            foreach (var item in subComponets)
                item.OnDestroy();
        }
        /// <summary>
        /// 组建被移除的时候
        /// </summary>
        public virtual void OnBeRemoved()
        {
            foreach(var item in subComponets)
                item.OnBeRemoved();
        }
        /// <summary>
        /// 创建组建的时候
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>() where  T:BaseCoreMgr, new()
        {
            T t=new T();
            t.OnCreate();
            return t;
        }

        #endregion

        #region unit life
        public virtual void Init()
        {
            foreach (var item in subComponets)
            {
                if(item.IsEnable)
                    item.Init();
            }
        }

        public virtual void Birth()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.Birth();
            }
        }

        public virtual void Birth2()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.Birth2();
            }
        }

        public virtual void Birth3()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.Birth3();
            }
        }


        public virtual void ReBirth()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.ReBirth();
            }
        }

        public virtual void Death(BaseUnit caster)
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.Death(caster);
            }
        }

        public virtual void RealDeath()
        {
            foreach (var item in subComponets)
            {
                if (item.IsEnable)
                    item.RealDeath();
            }
        }
        #endregion

        #region get
        protected T Cast<T>(BaseMono obj)where T: BaseMono
        {
            return obj as T;
        }
        protected T SelfGlobal<T>() where T: BaseGlobal
        {

            return SelfBaseGlobal as T;
        }
        protected TType GetAddedObjData<TType>(object[] ps, int index, TType defaultVal = default(TType))
        {
            if (ps == null || ps.Length <= index)
                return defaultVal;
            return (TType)(ps[index]);
        }
        #endregion

        #region DB
        public virtual void Read1<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in subComponets)
                item.Read1(data);
        }

        public virtual void Read2<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in subComponets)
                item.Read2(data);
        }

        public virtual void Read3<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in subComponets)
                item.Read3(data);
        }

        public virtual void ReadEnd<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in subComponets)
                item.ReadEnd(data);
        }

        public virtual void Write<TDBData>(TDBData data) where TDBData : BaseDBGameData, new()
        {
            foreach (var item in subComponets)
                item.Write(data);
        }
        #endregion
    }
}
