//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
using System;
namespace CYM.AI
{
    public class Var<T>
    {
        Func<T> Get;

        public Var(System.Func<T> getter)
        {
            Get = getter;
        }

        public T Value
        {
            get { return Get(); }
            set { throw new InvalidOperationException("不允许set"); }
        }
    }

    public class Float: Var<float>
    {
        public Float(float val):base(()=>val)
        {
        }
    }

    // 语言
    public class BT
    {
        /// <summary>
        /// 执行一个函数,并且返回成功
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoSucc(Func<Status> action)
        {
            return new DoStatus(action, Status.Succ);
        }
        /// <summary>
        /// 执行所有,并且返回成功
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoSucc(params Func<Status>[] action)
        {
            return new DoListStatus(Status.Succ, action);
        }
        /// <summary>
        /// 执行随机动作,并且返回成功
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoSuccRand(params Func<Status>[] action)
        {
            return new DoRandStatus(Status.Succ, action);
        }

        /// <summary>
        /// 执行一个函数,并且返回失败
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoFail(Func<Status> action)
        {
            return new DoStatus(action, Status.Fail);
        }
        /// <summary>
        /// 执行所有,并且返回失败
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoFail(params Func<Status>[] action)
        {
            return new DoListStatus(Status.Fail, action);
        }
        /// <summary>
        /// 执行随机动作,并且返回失败
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoFailRand(params Func<Status>[] action)
        {
            return new DoRandStatus(Status.Fail, action);
        }

        /// <summary>
        /// 执行一个函数,返回一个值
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node Do(Func<Status> action)
        {
            return new Do(action);
        }

        /// <summary>
        /// 执行随机动作,并且返回值
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node DoRand(params Func<Status>[] action)
        {
            return new DoRand(action);
        }

        /// <summary>
        /// 初始化一个函数
        /// </summary>
        /// <param name="start"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Node Do(Action start, Func<Status> action)
        {
            return new Do(start, action);
        }
        /// <summary>
        /// 如果判断
        /// </summary>
        /// <param name="iis"></param>
        /// <param name="node"></param>
        /// <param name="elseNode"></param>
        /// <returns></returns>
        public static Decision If(Node iis, Node node, Node elseNode = null)
        {
            return new If(iis, node, elseNode != null ? elseNode : Fail());
        }
        /// <summary>
        /// 如果判断
        /// </summary>
        /// <param name="iis"></param>
        /// <param name="node"></param>
        /// <param name="elseNode"></param>
        /// <returns></returns>
        public static Decision If(Func<Status> iis, Node node, Node elseNode = null)
        {
            return new If(Do(iis), node, elseNode != null ? elseNode : Fail());
        }

        /// <summary>
        /// 返回成功
        /// </summary>
        /// <returns></returns>
        public static Node Succ()
        {
            return new Empty(Status.Succ);
        }

        /// <summary>
        /// 返回失败
        /// </summary>
        /// <returns></returns>
        public static Node Fail()
        {
            return new Empty(Status.Fail);
        }

        /// <summary>
        /// 执行节点并且返回成功
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Node Succ(Node node)
        {
            return new Result(Status.Succ, node);
        }

        /// <summary>
        /// 执行节点并且返回失败
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Node Fail(Node node)
        {
            return new Result(Status.Fail, node);
        }

        /// <summary>
        /// 执行节点,取反
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Node Not(Node node)
        {
            return new Not(node);
        }
        /// <summary>
        /// 执行节点,取反
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static Node Not(Func<Status> node)
        {
            return new Not(Do(node));
        }


        /// <summary>
        /// 顺序执行子节点
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public static Decision Seq(params Node[] children)
        {
            return new Sequence(children);
        }

        /// <summary>
        /// 选择节点
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public static Node Fall(params Node[] children)
        {
            return new Fallback(children);
        }

        /// <summary>
        /// 执行所有节点
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public static Node All(params Node[] children)
        {
            return new All(children);
        }

        /// <summary>
        /// 概率节点 0-1
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Node Prob(float prob)
        {
            return new Prob(prob);
        }


        /// <summary>
        /// 概率动作 0-1
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Node ProbAction(Var<float> prob, Func<Status> node)
        {
            return new ProbAction(prob, node);
        }

        /// <summary>
        /// 概率节点 0-1
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static Node ProbNode(Var<float> prob, Node node)
        {
            return new ProbNode(prob, node);
        }

        /// <summary>
        /// log
        /// </summary>
        /// <param name="format"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static Node Log(string format, params object[] ps)
        {
            return DoSucc(() => { CLog.Log(format, ps); return Status.Succ; });
        }

        /// <summary>
        /// 重复执行Fall
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public static Decision RepFall(params Node[] children)
        {
            var ret = new Repeat(Fall(children));
            ret.CustomTypeName = "RepFall";
            return ret;
        }

        /// <summary>
        /// 重复执行Seq
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public static Decision RepSeq(params Node[] children)
        {
            var ret = new Repeat(Seq(children));
            ret.CustomTypeName = "RepSeq";
            return ret;
        }

        /// <summary>
        /// 重复执行所有
        /// </summary>
        /// <returns></returns>
        public static Decision RepAll(params Node[] children)
        {
            var ret = new Repeat(All(children));
            ret.CustomTypeName = "RepAll";
            return ret;
        }

        /// <summary>
        /// 重复执行 直到失败
        /// </summary>
        /// <returns></returns>
        public static Decision UntilFail(Node child)
        {
            return new UntilFail(child);
        }
        /// <summary>
        /// 重复执行 直到失败
        /// </summary>
        /// <returns></returns>
        public static Decision UntilFail(Func<Status> child)
        {
            return new UntilFail(Do(child));
        }

        /// <summary>
        /// 重复执行 直到成功
        /// </summary>
        /// <returns></returns>
        public static Decision UntilSucc(Node child)
        {
            return new UntilSuccess(child);
        }
        /// <summary>
        /// 重复执行 直到成功
        /// </summary>
        /// <returns></returns>
        public static Decision UntilSucc(Func<Status> child)
        {
            var temp = new UntilSuccess(Do(child));
            temp.IsNewLine = false;
            return temp;
        }

        /// <summary>
        /// 当条件成熟的时候重复执行
        /// </summary>
        /// <returns></returns>
        public static Decision When(Node cond, Node normalNode, Node elseNode=null)
        {
            return new When( cond,  normalNode,  elseNode!=null? elseNode:new Empty(Status.Fail));
        }
        /// <summary>
        /// 当条件成熟的时候重复执行
        /// </summary>
        /// <returns></returns>
        public static Decision When(Func<Status> action, Node normalNode, Node elseNode=null)
        {
            return new When(Do(action), normalNode, elseNode != null ? elseNode : new Empty(Status.Fail));
        }

        /// <summary>
        /// 等待
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static Node Wait(Var<float> seconds)
        {
            return new Wait(seconds);
        }
        /// <summary>
        /// 等待Fxedtime
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static Node WaitFixed(Var<float> seconds)
        {
            return new WaitFixed(seconds);
        }

        /// <summary>
        /// 检测事件触发
        /// </summary>
        /// <param name="e"></param>
        /// <param name="normal"></param>
        /// <param name="interrupt"></param>
        /// <returns></returns>
        public static Node EventInterrupt(BaseEvent e, Node normal, Node interrupt)
        {
            return
                When(e.IsTrigger,
                    normal,
                    Seq(
                        Do(e.Reset),
                        interrupt)
                    );
        }
    }
}
