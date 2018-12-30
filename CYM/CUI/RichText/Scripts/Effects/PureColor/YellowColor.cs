//------------------------------------------------------------------------------
// YellowColor.cs
// Copyright 2018 2018/3/27 
// Created by CYM on 2018/3/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM.UI
{
    public class YellowColor : TextEffect
    {
        protected override void ProcessCharactersAtLine(VertexHelper vh, int lineIndex, int startCharIdx, int endCharIdx, IList<UILineInfo> lines, IList<UICharInfo> chars)
        {
            Color color = Color.yellow;

            while (startCharIdx <= endCharIdx)
            {
                var k = startCharIdx * 4;
                SetUIVertexColor(vh, k++, color);
                SetUIVertexColor(vh, k++, color);
                SetUIVertexColor(vh, k++, color);
                SetUIVertexColor(vh, k++, color);
                ++startCharIdx;
            }
        }
    }
}