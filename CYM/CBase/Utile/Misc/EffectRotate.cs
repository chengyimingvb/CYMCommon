//------------------------------------------------------------------------------
// EffectRotate.cs
// Copyright 2018 2018/4/17 
// Created by CYM on 2018/4/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class EffectRotate : BaseMono
    {
        [SerializeField]
        float UpSpeed = 10.1f;
        [SerializeField]
        float ForwardSpeed=0.0f;
        [SerializeField]
        float RightSpeed = 0.0f;

        float CurUp = 0.0f;
        float CurForward = 0.0f;
        float CurRight = 0.0f;

        public void Update()
        {
            transform.Rotate(Vector3.up, UpSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.forward, ForwardSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.right, RightSpeed * Time.deltaTime, Space.Self);
        }

    }
}