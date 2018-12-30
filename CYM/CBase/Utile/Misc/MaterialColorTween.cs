//#define CYM_LUA_DEBUG
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using CYM;
using System;

//*********************************************************
// 简单的lua工具们
// 2016/1/19
// hakerzhuli
//***************************************************

namespace CYM
{
    public class MaterialColorTween : MonoBehaviour
    {
        //[SerializeField]
        Material Mat;
        [SerializeField]
        Color fromCol = Color.green;
        [SerializeField]
        Color targetCol = Color.yellow;
        float curTime = 0;
        bool isForward=true;

        public void Awake()
        {
            Renderer renderer = GetComponent<Renderer> ();
            Mat = renderer.material;
        }

        public void OnDisable()
        {
            //alpha = 0.0f;
        }

        public void OnEnable()
        {
            //color = Mat.color;
        }

        public void SetScale(float val)
        {


        }

        public void Update()
        {
            if (Mat != null && !isForward)
            {
                curTime -= Time.deltaTime * 0.1f;
                if (curTime <= 0.0f)
                {
                    isForward = true;
                    curTime = 0.0f;
                }
            }
            else if (Mat != null && isForward)
            {
                curTime += Time.deltaTime * 0.1f;
                if (curTime >= 1.0f)
                {
                    isForward = false;
                    curTime = 1.0f;
                }
            }
            Mat.color = Color.Lerp(fromCol, targetCol, curTime);
        }
    }

}