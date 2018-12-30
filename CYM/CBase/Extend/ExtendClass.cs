using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
//**********************************************
// Class Name	: CYMTimer
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    #region extend
    [Serializable]
    public class Range
    {
        public float min, max;
        public float sum { get { return min + max; } }
        public float length { get { return max - min; } }

        public Range(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
    /// <summary>
    /// 定时器
    /// </summary>
    public class Timer
    {
        float lastUpdateTime = 0.0f;
        float overTime = float.MaxValue;
        int overCount = 0;
        float stopTime = 0;
        public bool IsStop { get; private set; } = false;

        public Timer()
        {

        }
        public Timer(float overTime)
        {
            this.overTime = overTime;
        }
        /// <summary>
        /// 重置时间
        /// </summary>
        public void Restart(float? overTime = null)
        {
            Resume();
            overCount = 0;
            lastUpdateTime = Time.time;
            if (overTime != null)
                this.overTime = overTime.Value;
            SetOverTime(this.overTime);
        }
        /// <summary>
        /// 设置结束时间
        /// </summary>
        /// <param name="overTime"></param>
        public void SetOverTime(float overTime)
        {
            this.overTime = overTime;
        }
        /// <summary>
        /// 当前流逝的时间
        /// </summary>
        /// <returns></returns>
        public float Elapsed()
        {
            if (IsStop)
                return stopTime - lastUpdateTime;
            return Time.time - lastUpdateTime;
        }
        public override string ToString()
        {
            return new TimeSpan(0, 0, (int)Elapsed()).ToString("c");
        }
        /// <summary>
        /// 检查是否结束,如果结束,自动调用Restar 
        /// </summary>
        /// <returns></returns>
        public bool CheckOver()
        {
            if (IsOver())
            {
                Restart(overTime);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 检查是否结束,如果结束,在手动调用Restart之前,一直返回false,确保只会进入一次
        /// </summary>
        /// <returns></returns>
        public bool CheckOverOnce()
        {
            if (IsOver())
            {
                if (overCount == 0)
                {
                    overCount++;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断是否结束
        /// </summary>
        /// <returns></returns>
        public bool IsOver()
        {
            //如果为0则表示无效计时
            if (overTime <= 0)
                return false;
            //时间直接流逝
            if (lastUpdateTime == float.MinValue)
                return true;
            return Elapsed() > overTime;
        }
        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            stopTime = Time.time;
            IsStop = true;
        }
        /// <summary>
        /// 恢复时间
        /// </summary>
        public void Resume()
        {
            IsStop = false;
            lastUpdateTime = stopTime = Time.time;
        }
        /// <summary>
        /// 让时间快速流逝
        /// </summary>
        public void Gone()
        {
            lastUpdateTime = float.MinValue;
        }
    }

    /// <summary>
    /// 触发器
    /// </summary>
    public class Triger
    {
        bool isTrue = false;
        Callback Callback;
        public Triger(Callback callback)
        {
            Callback = callback;
        }
        public void DoTriger(Callback callback)
        {
            if (isTrue == false)
            {
                callback?.Invoke();
                isTrue = true;
            }
        }
        public void DoTriger()
        {
        }
        public void UnTriger()
        {
            isTrue = false;
        }
        public bool IsTriger()
        {
            return isTrue;
        }
    }

    /// <summary>
    /// 队列基类
    /// </summary>
    [Serializable]
    public class BaseQueue
    {
        public float CurCount;
        public float TotalCout;
        public virtual void Update(float step)
        {
            CurCount += step;
        }
        public virtual bool IsOver()
        {
            return CurCount >= TotalCout;
        }
        public float Progress { get { return CurCount / TotalCout; } }
        public virtual void Reset(float count)
        {
            CurCount = 0;
            TotalCout = count;
        }
        public virtual void ForceOver()
        {
            CurCount = TotalCout;
        }
        public virtual float GetRemainderCount()
        {
            return TotalCout - CurCount;
        }
    }
    [Serializable]
    public class CD : BaseQueue
    {
        public CD()
        {

        }
        public CD(float time)
        {
            Reset(time);
        }
        public override void Update(float step)
        {
            CurCount -= step;
            CurCount = Mathf.Clamp(CurCount, 0, TotalCout);
        }
        public override bool IsOver()
        {
            return CurCount <= 0;
        }
        public override void Reset(float count)
        {
            CurCount = count;
            TotalCout = count;
        }
        public override void ForceOver()
        {
            CurCount = 0;
        }
        public override float GetRemainderCount()
        {
            return CurCount;
        }
    }
    public class BoolState
    {
        int count = 0;
        public bool Push(bool b)
        {
            if (b)
                Add();
            else
                Remove();
            return IsIn();
        }
        public void Reset()
        {
            count = 0;
        }
        public void Add()
        {
            count++;
        }
        public void Remove()
        {
            count--;
            count = Mathf.Clamp(count, 0, int.MaxValue);
        }
        public bool IsIn()
        {
            return count > 0;
        }
    }
    public class IndexState
    {
        public IndexState(int max)
        {
            maxCount = max;
        }

        int preCount = 0;
        int count = 0;
        int maxCount = 3;
        public int Reset(int index = 0)
        {
            preCount = count;
            count = index;
            return count;
        }
        public int Add()
        {
            int ret = count;
            preCount = ret;
            count++;
            if (count > maxCount)
                count = 0;
            return ret;
        }
        public int Added()
        {
            preCount = count;
            count++;
            if (count > maxCount)
                count = 0;
            return count;
        }
        public int Remove()
        {
            int ret = count;
            preCount = ret;
            count--;
            if (count < 0)
                count = maxCount;
            return ret;
        }
        public int Cur()
        {
            return count;
        }
        public int Pre()
        {
            return preCount;
        }
        public bool IsMaxCount()
        {
            return count >= maxCount;
        }
        public bool IsPreMaxCount()
        {
            return preCount >= maxCount;
        }
    }
    /// <summary>
    /// 运行时表格数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RuntimeTD<T> where T : TDValue
    {
        public T Data { get; set; }

    }
    #endregion

    #region Callback
    //定义回调函数类型
    public delegate void Callback();
    public delegate void Callback<T>(T arg1);
    public delegate void Callback<T, U>(T arg1, U arg2);
    public delegate void Callback<T, U, V>(T arg1, U arg2, V arg3);
    public delegate void Callback<T, U, V, W>(T arg1, U arg2, V arg3, W arg4);
    #endregion

    #region enum
    public class EnumComparer<T> : IEqualityComparer<T> where T : struct
    {
        public bool Equals(T first, T second)
        {
            var firstParam = Expression.Parameter(typeof(T), "first");
            var secondParam = Expression.Parameter(typeof(T), "second");
            var equalExpression = Expression.Equal(firstParam, secondParam);

            return Expression.Lambda<Func<T, T, bool>>
                (equalExpression, new[] { firstParam, secondParam }).
                Compile().Invoke(first, second);
        }

        public int GetHashCode(T instance)
        {
            var parameter = Expression.Parameter(typeof(T), "instance");
            var convertExpression = Expression.Convert(parameter, typeof(int));

            return Expression.Lambda<Func<T, int>>
                (convertExpression, new[] { parameter }).
                Compile().Invoke(instance);
        }
    }


    [Serializable]
    public struct FastEnumIntEqualityComparer<TEnum> : IEqualityComparer<TEnum>
        where TEnum : struct
    {

        public static class BoxAvoidance
        {
            static readonly Func<TEnum, int> _wrapper;

            public static int ToInt(TEnum enu)
            {
                return _wrapper(enu);
            }

            static BoxAvoidance()
            {
                var p = Expression.Parameter(typeof(TEnum), null);
                var c = Expression.ConvertChecked(p, typeof(int));

                _wrapper = Expression.Lambda<Func<TEnum, int>>(c, p).Compile();
            }
        }
        public bool Equals(TEnum firstEnum, TEnum secondEnum)
        {
            return BoxAvoidance.ToInt(firstEnum) ==
                BoxAvoidance.ToInt(secondEnum);
        }

        public int GetHashCode(TEnum firstEnum)
        {
            return BoxAvoidance.ToInt(firstEnum);
        }
    }

    public static class BoxAvoidance<T>
    {
        static readonly Func<T, int> _wrapper;

        static readonly Func<int, T> _wrapperInvert;

        public static int ToInt(T enu)
        {
            return _wrapper(enu);
        }
        public static T Invert(int val)
        {
            return _wrapperInvert(val);
        }

        static BoxAvoidance()
        {
            var p = Expression.Parameter(typeof(T), null);
            var c = Expression.ConvertChecked(p, typeof(int));
            _wrapper = Expression.Lambda<Func<T, int>>(c, p).Compile();

            //逆向
            var p2 = Expression.Parameter(typeof(int), null);
            var c2 = Expression.ConvertChecked(p2, typeof(T));
            _wrapperInvert = Expression.Lambda<Func<int, T>>(c2, p2).Compile();
        }
    }
    #endregion

    #region other

    #endregion

}
