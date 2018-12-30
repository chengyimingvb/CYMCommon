//------------------------------------------------------------------------------
// EmojConfig.cs
// Copyright 2018 2018/3/23 
// Created by CYM on 2018/3/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.UI
{
    [Serializable]
    public class SpritesData
    {
        [ReadOnly]
        [ShowIf("IsHave")]
        public string Name = "";
        public Sprite[] Sprites;
        public Sprite First
        {
            get {
                if (Sprites == null)
                    return null;
                if (Sprites.Length <= 0)
                    return null;
                return Sprites[0];
            }
        }
        public bool IsAnim
        {
            get
            {
                if (Sprites == null)
                    return false;
                return Sprites.Length > 1;
            }
        }
        [ShowIf("IsAnim")] 
        public float AnimSpeed = 0.05f;//多少秒播放一个图片

        public bool IsHave
        {
            get
            {
                return First != null;
            }
        }

        public void Init()
        {
            if(First!=null)
                Name = First.name;
        }
    }

    [Serializable]
    [CreateAssetMenu]
    public class SpriteGroupConfig : ScriptableObject
    {
        public string Name = "";
        public SpritesData[] SpritesData;
        [NonSerialized]
        public Dictionary<string, SpritesData> KeySpritesData = new Dictionary<string, SpritesData>();

        private void OnEnable()
        {
            KeySpritesData.Clear();
            if (SpritesData != null)
            {
                foreach (var Item in SpritesData)
                {
                    if (!KeySpritesData.ContainsKey(Item.Name))
                    {
                        Item.Init();
                        KeySpritesData.Add(Item.Name, Item);
                    }
                }
            }
        }

        public SpriteGroupConfig()
        {

        }

        public void OnAfterDeserialize()
        {

        }
    }

}