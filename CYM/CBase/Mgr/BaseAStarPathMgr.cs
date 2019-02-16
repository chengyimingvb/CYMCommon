//------------------------------------------------------------------------------
// BaseAStarPath.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Pathfinding;

namespace CYM
{
    public class BaseAStarPathMgr : BaseGFlowMgr
    {
        #region prop
        AstarPath Ins => AstarPath.active;
        #endregion

        #region set
        //计算常量路径
        public ConstantPath StartConstantPath(Vector3 start, int maxGScore, ITraversalProvider traversalProvider, OnPathDelegate callback)
        {
            var path = ConstantPath.Construct(start, maxGScore + 1, callback);
            path.traversalProvider = traversalProvider;
            AstarPath.StartPath(path);
            if (callback != null){ }
            else
                path.BlockUntilCalculated();
            return path;
        }
        //计算AB路径
        public ABPath StartABPath(Vector3 start,Vector3 end, ITraversalProvider traversalProvider, OnPathDelegate callback)
        {
            var path = ABPath.Construct(start, end, callback);
            path.traversalProvider = traversalProvider;
            if (callback != null) { }
            else
                path.BlockUntilCalculated();
            AstarPath.StartPath(path);
            return path;
        }
        #endregion

        #region get
        public NNInfo GetNearest(Vector3 position)
        {
            return Ins.GetNearest(position);
        }
        #endregion
    }
}