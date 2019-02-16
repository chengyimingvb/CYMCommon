//------------------------------------------------------------------------------
// LookAtCamera.cs
// Copyright 2019 2019/1/25 
// Created by CYM on 2019/1/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class LookAtCamera : MonoBehaviour 
    {
        void Update()
        {
            if (Camera.main == null) return;
            transform.LookAt(new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z));
        }
    }
}