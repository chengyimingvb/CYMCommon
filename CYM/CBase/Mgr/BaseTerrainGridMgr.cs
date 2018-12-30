//------------------------------------------------------------------------------
// BaseTerrainGridMgr.cs
// Copyright 2018 2018/11/16 
// Created by CYM on 2018/11/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using TGS;
using DG.Tweening.Core;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using System;

namespace CYM
{
    public class BaseTerrainGridMgr : BaseGlobalCoreMgr 
    {
        #region prop
        TerrainGridSystem TerrainGridSystem;
        TweenerCore<float, float, FloatOptions> mapAlphaTween;
        TweenerCore<float, float, FloatOptions> cellAlphaTween;
        Tweener frontierColor;
        Tweener gridElevation;
        float mapTweenDuration = 0.3f;
        public bool IsShowMap { get; private set; } = false;
        #endregion

        #region life
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            TerrainGridSystem = TerrainGridSystem.instance;
            TerrainGridSystem.cameraMain = Mono.GetComponentInChildren<Camera>();
            OnSetTerrainGridSystem();
            TerrainGridSystem.GenerateMap();
            CloseMap();
        }
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            TerrainGridSystem.OnTerritoryEnter += OnTerritoryEnter;
            TerrainGridSystem.OnTerritoryExit += OnTerritoryExit;
            TerrainGridSystem.OnTerritoryClick += OnTerritoryClick;

            TerrainGridSystem.OnCellEnter += OnCellEnter;
            TerrainGridSystem.OnCellExit += OnCellExit;
            TerrainGridSystem.OnCellClick += OnCellClick;
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            TerrainGridSystem.OnTerritoryEnter -= OnTerritoryEnter;
            TerrainGridSystem.OnTerritoryExit -= OnTerritoryExit;
            TerrainGridSystem.OnTerritoryClick -= OnTerritoryClick;

            TerrainGridSystem.OnCellEnter -= OnCellEnter;
            TerrainGridSystem.OnCellExit -= OnCellExit;
            TerrainGridSystem.OnCellClick -= OnCellClick;
        }
        #endregion

        #region set
        public void Show(bool b)
        {
            if (IsShowMap == b)
                return;
            IsShowMap = b;
            if (mapAlphaTween != null)
                mapAlphaTween.Kill();
            if (frontierColor != null)
                frontierColor.Kill();
            if (cellAlphaTween != null)
                cellAlphaTween.Kill();
            //为了防止行政图内部,不发生dirty,外部强制改下alpha值
            if (TerrainGridSystem.colorizedTerritoriesAlpha == 0.0f)
                TerrainGridSystem.colorizedTerritoriesAlpha = 0.01f;
            else if (TerrainGridSystem.colorizedTerritoriesAlpha == 1.0f)
                TerrainGridSystem.colorizedTerritoriesAlpha = 0.99f;
            //tween地图颜色
            mapAlphaTween = DOTween.To(() => TerrainGridSystem.colorizedTerritoriesAlpha, x => TerrainGridSystem.colorizedTerritoriesAlpha = x,b ? 0.6f : 0.0f, mapTweenDuration);
            mapAlphaTween.OnComplete(OnMapShowed);
            //tween地图边界
            frontierColor = DOTween.ToAlpha(() => TerrainGridSystem.territoryDisputedFrontierColor, x => TerrainGridSystem.territoryDisputedFrontierColor = x,b ? 1.0f : 0.0f, mapTweenDuration);
            //tweenCell边界Alpha
            cellAlphaTween = DOTween.To(() => TerrainGridSystem.cellBorderAlpha, x => TerrainGridSystem.cellBorderAlpha = x,b ? 0.0f : 0.2f, mapTweenDuration);
        }
        /// <summary>
        /// 是否显示网格
        /// </summary>
        public bool IsShowCells
        {
            get { return TerrainGridSystem.showCells; }
            set { TerrainGridSystem.showCells = value; }
        }
        public void CloseMap()
        {
            TerrainGridSystem.colorizedTerritoriesAlpha = 0.0f;
            Color tempColor = TerrainGridSystem.territoryDisputedFrontierColor;
            tempColor.a = 0.0f;
            TerrainGridSystem.territoryDisputedFrontierColor= tempColor;
            TerrainGridSystem.cellBorderAlpha = 0.2f;
        }
        #endregion

        #region Callback
        protected virtual void OnSetTerrainGridSystem()
        {
            TerrainGridSystem.highlightMinimumTerrainDistance = 9000.0f;
            TerrainGridSystem.highlightMode = HIGHLIGHT_MODE.None;
            TerrainGridSystem.showTerritories = true;
            TerrainGridSystem.colorizeTerritories = true;
        }
        private void OnMapShowed()
        {
            //TerrainGridSystem.highlightMinimumTerrainDistance = IsShowMap ? 1.0f : 9000.0f;
            if (IsShowMap)
            {

            }
            else
            {
                //TerrainGridSystem.highlightMode = HIGHLIGHT_MODE.Cells;
            }
        }
        protected virtual void OnTerritoryClick(int territoryIndex, int buttonIndex)
        {
            //throw new NotImplementedException();
        }

        protected virtual void OnTerritoryExit(int territoryIndex)
        {
            //throw new NotImplementedException();
        }

        protected virtual void OnTerritoryEnter(int territoryIndex)
        {
            //throw new NotImplementedException();
        }
        protected virtual void OnCellClick(int cellIndex, int buttonIndex)
        {
            //throw new NotImplementedException();
        }

        protected virtual void OnCellExit(int cellIndex)
        {
            //throw new NotImplementedException();
        }

        protected virtual void OnCellEnter(int cellIndex)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}