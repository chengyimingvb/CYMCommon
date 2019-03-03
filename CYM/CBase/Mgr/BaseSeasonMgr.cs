//------------------------------------------------------------------------------
// BaseWeatherMgr.cs
// Copyright 2019 2019/2/25 
// Created by CYM on 2019/2/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using System.Collections.Generic;

namespace CYM
{
    public enum SeasonType
    {
        Spring,
        Summer,
        Fall,
        Winter,
    }
    public enum SubSeasonType
    {
        Normal,//正常
        Deep, //嚴冬.盛夏
    }

    public struct SeasonData
    {
        /// <summary>
        /// 太阳关照强度
        /// </summary>
        public float SunIntensity { get; set; }
        /// <summary>
        /// 积雪百分比
        /// </summary>
        public float AccumulatedSnow { get; set; }
        /// <summary>
        /// 风力
        /// </summary>
        public float WindzonePower { get; set; }

        public SubSeasonType Type { get; set; }

    }

    public class BaseSeasonMgr : BaseGFlowMgr
    {
        #region Callback
        /// <summary>
        /// 季节变化
        /// </summary>
        public event Callback<SeasonType, SubSeasonType> Callback_OnSeasonChanged;
        #endregion

        #region prop
        private TweenerCore<float, float, FloatOptions> sunTween;
        private TweenerCore<float, float, FloatOptions> snowTween;
        Light Sun => BaseSceneObject.Sun;
        WindZone Wind => BaseSceneObject.Wind;
        Dictionary<SeasonType, List<SeasonData>> Data = new Dictionary<SeasonType, List<SeasonData>>();
        public SeasonData CurData { get;private set; } = new SeasonData();
        public SeasonType SeasonType { get; private set; } = SeasonType.Spring;
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            Data.Add(SeasonType.Spring,new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.8f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.1f,
                    Type = SubSeasonType.Normal
                }
            });

            Data.Add(SeasonType.Summer, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.8f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.2f,
                    Type = SubSeasonType.Normal
                },

                new SeasonData
                {
                    SunIntensity = 0.9f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.25f,
                    Type = SubSeasonType.Deep
                }
            });

            Data.Add(SeasonType.Fall, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.75f,
                    AccumulatedSnow = 0.0f,
                    WindzonePower = 0.1f,
                    Type = SubSeasonType.Normal
                }
            });

            Data.Add(SeasonType.Winter, new List<SeasonData> {
                new SeasonData
                {
                    SunIntensity = 0.8f,
                    AccumulatedSnow = 0.3f,
                    WindzonePower = 0.3f,
                    Type = SubSeasonType.Normal
                },

                new SeasonData
                {
                    SunIntensity = 0.7f,
                    AccumulatedSnow = 0.4f,
                    WindzonePower = 0.45f,
                    Type = SubSeasonType.Deep
                }
            });
        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            Change(SeasonType.Spring, true);
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
        }
        #endregion

        #region set
        public void Change(SeasonType type,bool isForce=false)
        {
            if (!isForce)
            {
                if (type == SeasonType)
                    return;

            }
            SeasonType = type;
            CurData = BaseMathUtils.RandArray(Data[type]);
            if (sunTween != null) DOTween.Kill(sunTween);
            if (snowTween != null) DOTween.Kill(snowTween);
            sunTween = DOTween.To(() => Sun.intensity, x => Sun.intensity = x, CurData.SunIntensity, 1.0f);
            Wind.windMain = CurData.WindzonePower;
            snowTween = DOTween.To(() => ActiveTerrainMat.GetFloat("_SnowAmount"), x => ActiveTerrainMat.SetFloat("_SnowAmount", x), CurData.AccumulatedSnow, 1.0f);         
            Callback_OnSeasonChanged?.Invoke(type, CurData.Type);
        }
        public void Next()
        {
            int val = (int)SeasonType + 1;
            if (val > (int)SeasonType.Winter)
            {
                val = 0;
            }
            Change((SeasonType)val);
        }
        #endregion
    }
}