using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;
using TGS.PathFinding;

namespace TGS {
	
	/* Event definitions */
	public delegate void CellEvent (int cellIndex);
	public delegate void CellHighlightEvent (int cellIndex, ref bool cancelHighlight);
	public delegate void CellClickEvent (int cellIndex, int buttonIndex);

	public partial class TerrainGridSystem : MonoBehaviour {

		public event CellEvent OnCellEnter;
		public event CellEvent OnCellExit;

		/// <summary>
		/// Occurs when user presses the mouse button on a cell
		/// </summary>
		public event CellClickEvent OnCellMouseDown;

		/// <summary>
		/// Occurs when user releases the mouse button on the same cell that started clicking
		/// </summary>
		public event CellClickEvent OnCellClick;

		/// <summary>
		/// Occurs when user releases the mouse button on any cell
		/// </summary>
		public event CellClickEvent OnCellMouseUp;

		/// <summary>
		/// Occurs when a cell is about to get highlighted
		/// </summary>
		public event CellHighlightEvent OnCellHighlight;

		/// <summary>
		/// Complete array of states and cells and the territory name they belong to.
		/// </summary>
		[NonSerialized]
		public List<Cell> cells;


		[SerializeField]
		int _numCells = 3;

		/// <summary>
		/// Gets or sets the desired number of cells in irregular topology.
		/// </summary>
		public int numCells { 
			get { return _numCells; } 
			set {
				if (_numCells != value) {
					_numCells = Mathf.Clamp (value, 2, 20000);
					needGenerateMap = true;
					isDirty = true;
				}
			}
		}


		[SerializeField]
		bool _showCells = true;

		/// <summary>
		/// Toggle cells frontiers visibility.
		/// </summary>
		public bool showCells { 
			get {
				return _showCells; 
			}
			set {
				if (value != _showCells) {
					_showCells = value;
					isDirty = true;
					if (cellLayer != null) {
						cellLayer.SetActive (_showCells);
						ClearLastOver ();
					} else if (_showCells) {
						Redraw ();
					}
				}
			}
		}


		[SerializeField]
		Vector2
			_cellSize = Misc.Vector2zero;

		/// <summary>
		/// Cells border thickness
		/// </summary>
		public Vector2 cellSize {
			get {
				return _cellSize;
			}
			set {
				if (value != _cellSize) {
					_cellSize = value;
					SetScaleByCellSize ();
					isDirty = true;
					needGenerateMap = true;
				}
			}
		}


		[SerializeField]
		Color
			_cellBorderColor = new Color (0, 1, 0, 1.0f);

		/// <summary>
		/// Cells border color
		/// </summary>
		public Color cellBorderColor {
			get {
				return _cellBorderColor;
			}
			set {
				if (value != _cellBorderColor) {
					_cellBorderColor = value;
					isDirty = true;
					if (cellsThinMat != null && _cellBorderColor != cellsThinMat.color) {
						cellsThinMat.color = _cellBorderColor;
					}
					if (cellsGeoMat != null && _cellBorderColor != cellsGeoMat.color) {
						cellsGeoMat.color = _cellBorderColor;
					}
				}
			}
		}

		[SerializeField]
		float
			_cellBorderThickness = 1f;

		/// <summary>
		/// Cells border thickness
		/// </summary>
		public float cellBorderThickness {
			get {
				return _cellBorderThickness;
			}
			set {
				if (value != _cellBorderThickness) {
					_cellBorderThickness = value;
					if (_showCells)
						DrawCellBorders ();
					isDirty = true;
				}
			}
		}

		public float cellBorderAlpha {
			get {
				return _cellBorderColor.a;
			}
			set {
				if (_cellBorderColor.a != value) {
					cellBorderColor = new Color (_cellBorderColor.r, _cellBorderColor.g, _cellBorderColor.b, Mathf.Clamp01 (value));
				}
			}
		}


		[SerializeField]
		Color
			_cellHighlightColor = new Color (1, 0, 0, 0.9f);

		/// <summary>
		/// Fill color to use when the mouse hovers a cell's region.
		/// </summary>
		public Color cellHighlightColor {
			get {
				return _cellHighlightColor;
			}
			set {
				if (value != _cellHighlightColor) {
					_cellHighlightColor = value;
					isDirty = true;
					if (hudMatCellOverlay != null && _cellHighlightColor != hudMatCellOverlay.color) {
						hudMatCellOverlay.color = _cellHighlightColor;
					}
					if (hudMatCellGround != null && _cellHighlightColor != hudMatCellGround.color) {
						hudMatCellGround.color = _cellHighlightColor;
					}
				}
			}
		}

		[SerializeField]
		bool _cellHighlightNonVisible = true;

		/// <summary>
		/// Gets or sets whether invisible cells should also be highlighted when pointer is over them
		/// </summary>
		public bool cellHighlightNonVisible {
			get { return _cellHighlightNonVisible; }
			set {
				if (_cellHighlightNonVisible != value) {
					_cellHighlightNonVisible = value;
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Returns Cell under mouse position or null if none.
		/// </summary>
		public Cell cellHighlighted { get { return _cellHighlighted; } }

		/// <summary>
		/// Returns current highlighted cell index.
		/// </summary>
		public int cellHighlightedIndex { get { return _cellHighlightedIndex; } }

		/// <summary>
		/// Returns Cell index which has been clicked
		/// </summary>
		public int cellLastClickedIndex { get { return _cellLastClickedIndex; } }


		[SerializeField]
		float _cellsMaxSlope = 1f;

		/// <summary>
		/// Gets or sets the cells max slope. Cells with a greater slope will be hidden.
		/// </summary>
		/// <value>The cells max slope.</value>
		public float cellsMaxSlope {
			get { return _cellsMaxSlope; }
			set {
				if (_cellsMaxSlope != value) {
					_cellsMaxSlope = value;
					needUpdateTerritories = true;
					if (!Application.isPlaying) {
						Redraw (true);
					} else {
						issueRedraw = RedrawType.Full;
					}
				}
			}
		}

		[SerializeField]
		float _cellsMinimumAltitude = 0f;

		/// <summary>
		/// Gets or sets the minimum cell altitude. Useful to hide cells under certain altitude, for instance, under water.
		/// </summary>
		public float cellsMinimumAltitude {
			get { return _cellsMinimumAltitude; }
			set {
				if (_cellsMinimumAltitude != value) {
					_cellsMinimumAltitude = value;
					needUpdateTerritories = true;
					Redraw (true);
				}
			}
		}


		#region Public Cell Functions


		[NonSerialized]
		List<Vector2> _voronoiSites;

		/// <summary>
		/// Sets or gets a list of Voronoi sites. Full list will be completed with random number up to numCells amount.
		/// </summary>
		/// <value>The voronoi sites.</value>
		public List<Vector2> voronoiSites {
			get { return _voronoiSites; }
			set {
				if (_voronoiSites != value) {
					_voronoiSites = value;
					needGenerateMap = true;
				}
			}
		}


		
		/// <summary>
		/// Returns the_numCellsrovince in the cells array by its reference.
		/// </summary>
		public int CellGetIndex (Cell cell) {
			if (cell == null)
				return -1;
			int index;
			if (cellLookup.TryGetValue (cell, out index))
				return index;
			else
				return -1;
		}

		/// <summary>
		/// Returns the_numCellsrovince in the cells array by its reference.
		/// </summary>
		/// <returns>The get index.</returns>
		/// <param name="row">Row.</param>
		/// <param name="column">Column.</param>
		/// <param name="clampToBorders">If set to <c>true</c> row and column values will be clamped inside current grid size (in case their values exceed the number of rows or columns). If set to false, it will wrap around edges.</param>
		public int CellGetIndex (int row, int column, bool clampToBorders = true) {
			if (_gridTopology != GRID_TOPOLOGY.Box && _gridTopology != GRID_TOPOLOGY.Hexagonal) {
				Debug.LogWarning ("Grid topology does not support row/column indexing.");
				return -1;
			}

			if (clampToBorders) {
				row = Mathf.Clamp (row, 0, _cellRowCount - 1);
				column = Mathf.Clamp (column, 0, _cellColumnCount - 1);
			} else {
				row = (row + _cellRowCount) % _cellRowCount;
				column = (column + _cellColumnCount) % _cellColumnCount;
			}

			return row * _cellColumnCount + column;
		}



		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		/// <returns>The generated color surface positioned and oriented over the given cell.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="visible">If the colored surface is shown or not.</param>
		/// <param name="texture">Texture to be used.</param>
		public GameObject CellToggleRegionSurface (int cellIndex, bool visible, Texture2D texture) {
			return CellToggleRegionSurface (cellIndex, visible, Color.white, false, texture, Misc.Vector2one, Misc.Vector2zero, 0, false);
		}

		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		/// <returns>The generated color surface positioned and oriented over the given cell.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="visible">If the colored surface is shown or not.</param>
		/// <param name="color">Color. Can be partially transparent.</param>
		/// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
		public GameObject CellToggleRegionSurface (int cellIndex, bool visible, Color color, bool refreshGeometry = false) {
			return CellToggleRegionSurface (cellIndex, visible, color, refreshGeometry, null, Misc.Vector2one, Misc.Vector2zero, 0, false);
		}

		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		/// <returns>The generated color surface positioned and oriented over the given cell.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="visible">If the colored surface is shown or not.</param>
		/// <param name="color">Color. Can be partially transparent.</param>
		/// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
		/// <param name="textureIndex">The index of the texture configured in the list of textures of the inspector.</param>
		public GameObject CellToggleRegionSurface (int cellIndex, bool visible, Color color, bool refreshGeometry, int textureIndex) {
			Texture2D texture = null;
			if (textureIndex >= 0 && textureIndex < textures.Length)
				texture = textures [textureIndex];
			return CellToggleRegionSurface (cellIndex, visible, color, refreshGeometry, texture, Misc.Vector2one, Misc.Vector2zero, 0, false);
		}

		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		/// <returns>The generated color surface positioned and oriented over the given cell.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="visible">If the colored surface is shown or not.</param>
		/// <param name="color">Color. Can be partially transparent.</param>
		/// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
		/// <param name="texture">An optional texture. If you pass a color different than white, the texture will be tinted using that color.</param>
		public GameObject CellToggleRegionSurface (int cellIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture) {
			return CellToggleRegionSurface (cellIndex, visible, color, refreshGeometry, texture, Misc.Vector2one, Misc.Vector2zero, 0, false);
		}


		
		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		/// <returns>The generated color surface positioned and oriented over the given cell.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="visible">If the colored surface is shown or not.</param>
		/// <param name="color">Color. Can be partially transparent.</param>
		/// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
		/// <param name="texture">An optional texture. If you pass a color different than white, the texture will be tinted using that color.</param>
		/// <param name="textureScale">Texture scale.</param>
		/// <param name="textureOffset">Texture offset.</param>
		/// <param name="textureRotation">Texture rotation.</param>
		public GameObject CellToggleRegionSurface (int cellIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace) {
			return CellToggleRegionSurface (cellIndex, visible, color, refreshGeometry, texture, textureScale, textureOffset, textureRotation, false, rotateInLocalSpace);
		}


		/// <summary>
		/// Colorize specified region of a cell by indexes.
		/// </summary>
		/// <returns>The generated color surface positioned and oriented over the given cell.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="visible">If the colored surface is shown or not.</param>
		/// <param name="color">Color. Can be partially transparent.</param>
		/// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
		/// <param name="texture">An optional texture. If you pass a color different than white, the texture will be tinted using that color.</param>
		/// <param name="textureScale">Texture scale.</param>
		/// <param name="textureOffset">Texture offset.</param>
		/// <param name="textureRotation">Texture rotation.</param>
		/// <param name="overlay">If set to <c>true</c> the colored surface will be shown on top of objects.</param>
		public GameObject CellToggleRegionSurface (int cellIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool overlay, bool rotateInLocalSpace) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;

			if (!visible) {
				CellHideRegionSurface (cellIndex);
				return null;
			}
			int cacheIndex = GetCacheIndexForCellRegion (cellIndex); 
			GameObject surf;
			bool existsInCache = surfaces.TryGetValue (cacheIndex, out surf);
			if (existsInCache && surf == null) {
				surfaces.Remove (cacheIndex);
				existsInCache = false;
			}
			if (refreshGeometry && existsInCache) {
				surfaces.Remove (cacheIndex);
				DestroyImmediate (surf);
				existsInCache = false;
				surf = null;
			}
			Region region = cells [cellIndex].region;
			
			// Should the surface be recreated?
			Material surfMaterial;
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (texture != null && (textureScale != region.customTextureScale || textureOffset != region.customTextureOffset || textureRotation != region.customTextureRotation)) {
					surfaces.Remove (cacheIndex);
					DestroyImmediate (surf);
					surf = null;
				}
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = cellHighlightedIndex == cellIndex;
			if (surf != null) {
				Material coloredMat = overlay ? coloredMatOverlay : coloredMatGround;
				Material texturizedMat = overlay ? texturizedMatOverlay : texturizedMatGround;
				if (!surf.activeSelf)
					surf.SetActive (true);
				// Check if material is ok
				Renderer renderer = surf.GetComponent<Renderer> ();
				surfMaterial = renderer.sharedMaterial;
				if ((texture == null && !surfMaterial.name.Equals (coloredMat.name)) || (texture != null && !surfMaterial.name.Equals (texturizedMat.name))
				    || (surfMaterial.color != color && !isHighlighted) || (texture != null && (region.customMaterial == null || region.customMaterial.mainTexture != texture))) {
					Material goodMaterial = GetColoredTexturedMaterial (color, texture, overlay);
					region.customMaterial = goodMaterial;
					ApplyMaterialToSurface (renderer, goodMaterial);
				}
			} else {
				surfMaterial = GetColoredTexturedMaterial (color, texture, overlay);
				surf = GenerateCellRegionSurface (cellIndex, surfMaterial, textureScale, textureOffset, textureRotation, rotateInLocalSpace);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = textureOffset;
				region.customTextureRotation = textureRotation;
				region.customTextureScale = textureScale;
				region.customRotateInLocalSpace = rotateInLocalSpace;
			}
			// If it was highlighted, highlight it again
			if (isHighlighted && region.customMaterial != null && _highlightedObj != null) {
				if (region.customMaterial != null) {
					hudMatCell.mainTexture = region.customMaterial.mainTexture;
				} else {
					hudMatCell.mainTexture = null;
				}
				surf.GetComponent<Renderer> ().sharedMaterial = hudMatCell;
				_highlightedObj = surf;
			}

			if (!cells [cellIndex].visible) {
				surf.SetActive (false);
			}
			return surf;
		}


		/// <summary>
		/// Uncolorize/hide specified cell by index in the cells collection.
		/// </summary>
		public void CellHideRegionSurface (int cellIndex) {
			if (_cellHighlightedIndex != cellIndex) {
				int cacheIndex = GetCacheIndexForCellRegion (cellIndex);
				GameObject surf;
				if (surfaces.TryGetValue (cacheIndex, out surf)) {
					if (surf == null) {
						surfaces.Remove (cacheIndex);
					} else {
						surf.SetActive (false);
					}
				}
			}
			cells [cellIndex].region.customMaterial = null;
		}

		
		/// <summary>
		/// Uncolorize/hide specified all cells.
		/// </summary>
		public void CellHideRegionSurfaces () {
			int cellsCount = cells.Count;
			for (int k = 0; k < cellsCount; k++) {
				CellHideRegionSurface (k);
			}
		}

		/// <summary>
		/// Colors a cell and fades it out for "duration" in seconds.
		/// </summary>
		public void CellFadeOut (Cell cell, Color color, float duration) {
			int cellIndex = CellGetIndex (cell);
			CellFadeOut (cellIndex, color, duration);
		}

		/// <summary>
		/// Colors a cell and fades it out during "duration" in seconds.
		/// </summary>
		public void CellFadeOut (int cellIndex, Color color, float duration, int repetitions = 1) {
			CellAnimate (FADER_STYLE.FadeOut, cellIndex, Misc.ColorNull, color, duration, repetitions);
		}

		/// <summary>
		/// Fades out a list of cells with "color" and "duration" in seconds.
		/// </summary>
		public void CellFadeOut (List<int>cellIndices, Color color, float duration, int repetitions = 1) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.FadeOut, cellIndices [k], Misc.ColorNull, color, duration, repetitions);
			}
		}

		/// <summary>
		/// Flashes a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (int cellIndex, Color color, float duration, int repetitions = 1) {
			CellAnimate (FADER_STYLE.Flash, cellIndex, Misc.ColorNull, color, duration, repetitions);
		}


		/// <summary>
		/// Flashes a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (Cell cell, Color color, float duration, int repetitions = 1) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.Flash, cellIndex, Misc.ColorNull, color, duration, repetitions);
		}

		/// <summary>
		/// Flashes a list of cells with "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (List<int>cellIndices, Color color, float duration, int repetitions = 1) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.Flash, cellIndices [k], Misc.ColorNull, color, duration, repetitions);
			}
		}


		/// <summary>
		/// Temporarily colors a cell for "duration" in seconds.
		/// </summary>
		public void CellColorTemp (int cellIndex, Color color, float duration) {
			CellAnimate (FADER_STYLE.ColorTemp, cellIndex, Misc.ColorNull, color, duration, 1);
		}

		/// <summary>
		/// Temporarily colors a cell for "duration" in seconds.
		/// </summary>
		public void CellColorTemp (Cell cell, Color color, float duration, int repetitions = 1) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.ColorTemp, cellIndex, Misc.ColorNull, color, duration, repetitions);
		}

		/// <summary>
		/// Temporarily colors a list of cells for "duration" in seconds.
		/// </summary>
		public void CellColorTemp (List<int>cellIndices, Color color, float duration, int repetitions = 1) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.ColorTemp, cellIndices [k], Misc.ColorNull, color, duration, repetitions);
			}
		}

		/// <summary>
		/// Blinks a cell with colors "color1" and "color2" and "duration" in seconds.
		/// </summary>
		public void CellBlink (Cell cell, Color color1, Color color2, float duration, int repetitions = 1) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.Blink, cellIndex, color1, color2, duration, repetitions);
		}


		/// <summary>
		/// Blinks a cell with colors "color1" and "color2" and "duration" in seconds.
		/// </summary>
		public void CellBlink (int cellIndex, Color color1, Color color2, float duration, int repetitions = 1) {
			CellAnimate (FADER_STYLE.Blink, cellIndex, color1, color2, duration, repetitions);
		}

		/// <summary>
		/// Blinks a list of cells with colors "color1" and "color2" and "duration" in seconds.
		/// </summary>
		public void CellBlink (List<int>cellIndices, Color color1, Color color2, float duration, int repetitions = 1) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.Blink, cellIndices [k], color1, color2, duration, repetitions);
			}
		}


		/// <summary>
		/// Flashes a cell from "initialColor" to "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (Cell cell, Color initialColor, Color color, float duration, int repetitions = 1) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.Flash, cellIndex, initialColor, color, duration, repetitions);
		}

		
		/// <summary>
		/// Flashes a cell from "initialColor" to "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (int cellIndex, Color initialColor, Color color, float duration, int repetitions = 1) {
			CellAnimate (FADER_STYLE.Flash, cellIndex, initialColor, color, duration, repetitions);
		}

		/// <summary>
		/// Flashes a list of cells from "initialColor" to "color" and "duration" in seconds.
		/// </summary>
		public void CellFlash (List<int>cellIndices, Color initialColor, Color color, float duration, int repetitions = 1) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.Flash, cellIndices [k], initialColor, color, duration, repetitions);
			}
		}

		/// <summary>
		/// Blinks a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellBlink (Cell cell, Color color, float duration, int repetitions = 1) {
			int cellIndex = CellGetIndex (cell);
			CellAnimate (FADER_STYLE.Blink, cellIndex, Misc.ColorNull, color, duration, repetitions);
		}

		
		/// <summary>
		/// Blinks a cell with "color" and "duration" in seconds.
		/// </summary>
		public void CellBlink (int cellIndex, Color color, float duration, int repetitions = 1) {
			CellAnimate (FADER_STYLE.Blink, cellIndex, Misc.ColorNull, color, duration, repetitions);
		}

		/// <summary>
		/// Blinks a list of cells with "color" and "duration" in seconds.
		/// </summary>
		public void CellBlink (List<int>cellIndices, Color color, float duration, int repetitions = 1) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellAnimate (FADER_STYLE.Blink, cellIndices [k], Misc.ColorNull, color, duration, repetitions);
			}
		}

		/// <summary>
		/// Returns the rect enclosing the cell in local or world space coordinates
		/// </summary>
		public Rect CellGetRect (int cellIndex) {
			if (cells == null || cellIndex < 0 || cellIndex >= cells.Count)
				return new Rect (0, 0, 0, 0);
			Rect rect = cells [cellIndex].region.rect2D;
			return rect;
		}

		/// <summary>
		/// Cancels any ongoing visual effect on a cell
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		public void CellCancelAnimations (int cellIndex) {
			CellCancelAnimation (cellIndex);
		}

		/// <summary>
		/// Cancels any ongoing visual effect on a list of cells
		/// </summary>
		/// <param name="cellIndices">Cell indices.</param>
		public void CellCancelAnimations (List<int>cellIndices) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellCancelAnimation (cellIndices [k]);
			}
		}


		/// <summary>
		/// Returns the rect enclosing the cell in world space
		/// </summary>
		public Bounds CellGetRectWorldSpace (int cellIndex) {
			if (cells == null || cellIndex < 0 || cellIndex >= cells.Count)
				return new Bounds (Misc.Vector3zero, Misc.Vector3zero);
			Rect rect = cells [cellIndex].region.rect2D;
			Vector3 min = GetWorldSpacePosition (rect.min);
			Vector3 max = GetWorldSpacePosition (rect.max);
			Bounds bounds = new Bounds ((min + max) * 0.5f, max - min);
			return bounds;
		}

		/// <summary>
		/// Returns the size in normalized viewport coordinates (0..1) for the given cell if that cell was on the center of the screen
		/// </summary>
		/// <returns>The get rect screen space.</returns>
		/// <param name="cellIndex">Cell index.</param>
		public Vector2 CellGetViewportSize (int cellIndex) {
			Transform t = cameraMain.transform;
			Vector3 oldPos = t.position;
			Quaternion oldRot = t.rotation;

			Plane p = new Plane (transform.forward, transform.position);
			float dist = p.GetDistanceToPoint (oldPos);
			Vector3 cellPos = CellGetPosition (cellIndex);
			t.position = cellPos - transform.forward * dist;
			t.LookAt (cellPos);
			Vector3 cellRectMin = transform.TransformPoint (cells [cellIndex].region.rect2D.min);
			Vector3 cellRectMax = transform.TransformPoint (cells [cellIndex].region.rect2D.max);
			Vector3 screenMin = cameraMain.WorldToViewportPoint (cellRectMin);
			Vector3 screenMax = cameraMain.WorldToViewportPoint (cellRectMax);

			t.rotation = oldRot;
			t.position = oldPos;
			return new Vector2 (Mathf.Abs (screenMax.x - screenMin.x), Mathf.Abs (screenMax.y - screenMin.y));
		}

		/// <summary>
		/// Gets the cell's center position in world space or local space.
		/// </summary>
		public Vector3 CellGetPosition (int cellIndex, bool worldSpace = true) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return Misc.Vector3zero;
			return CellGetPosition (cells [cellIndex], worldSpace);
		}

		/// <summary>
		/// Gets the cell's center position in world space.
		/// </summary>
		public Vector3 CellGetPosition (Cell cell, bool worldSpace = true) {
			if (cell == null)
				return Misc.Vector3zero;
			Vector2 cellGridCenter = cell.scaledCenter;
			if (worldSpace) {
				return GetWorldSpacePosition (cellGridCenter);
			} else {
				return cellGridCenter;
			}
		}


		/// <summary>
		/// Returns the normal at the center of a cell
		/// </summary>
		/// <returns>The normal in world space coordinates.</returns>
		/// <param name="cellIndex">Cell index.</param>
		public Vector3 CellGetNormal (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || terrain == null)
				return Misc.Vector3zero;
			Vector2 cellCenter = cells [cellIndex].scaledCenter;
			return terrain.terrainData.GetInterpolatedNormal (cellCenter.x + 0.5f, cellCenter.y + 0.5f);
		}

		/// <summary>
		/// Returns the number of vertices of the cell
		/// </summary>
		public int CellGetVertexCount (int cellIndex) {
			if (cells == null || cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			return cells [cellIndex].region.points.Count;
		}

		/// <summary>
		/// Returns the world space position of the vertex
		/// </summary>
		public Vector3 CellGetVertexPosition (int cellIndex, int vertexIndex) {
			Vector2 localPosition = cells [cellIndex].region.points [vertexIndex];
			return GetWorldSpacePosition (localPosition);
		}


		/// <summary>
		/// Returns a list of neighbour cells for specificed cell.
		/// </summary>
		public List<Cell> CellGetNeighbours (Cell cell) {
			int cellIndex = CellGetIndex (cell);
			return CellGetNeighbours (cellIndex);
		}

		/// <summary>
		/// Returns a list of neighbour cells for specificed cell index.
		/// </summary>
		public List<Cell> CellGetNeighbours (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;
			List<Cell> neighbours = new List<Cell> ();
			Region region = cells [cellIndex].region;
			int nCount = region.neighbours.Count;
			for (int k = 0; k < nCount; k++) {
				neighbours.Add ((Cell)region.neighbours [k].entity);
			}
			return neighbours;
		}

		/// <summary>
		/// Get a list of cells which are nearer than a given distance in cell count
		/// </summary>
		public List<int> CellGetNeighbours (int cellIndex, int distance, int cellGroupMask = -1, int maxCost = -1) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;
			Cell cell = cells [cellIndex];
			List<int> cc = new List<int> ();
			for (int x = cell.column - distance; x <= cell.column + distance; x++) {
				if (x < 0 || x >= _cellColumnCount)
					continue;
				for (int y = cell.row - distance; y <= cell.row + distance; y++) {
					if (y < 0 || y >= _cellRowCount)
						continue;
					if (x == cell.column && y == cell.row)
						continue;
					int ci = CellGetIndex (y, x);
					List<int> steps = FindPath (cellIndex, ci, maxCost, 0, cellGroupMask);
					if (steps != null && steps.Count <= distance) {
						cc.Add (ci);
					}
				}
			}
			return cc;
		}


		/// <summary>
		/// Returns cell's territory index to which it belongs to.
		/// </summary>
		public int CellGetTerritoryIndex (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].territoryIndex;
		}

		/// <summary>
		/// Returns current cell's fill color
		/// </summary>
		public Color CellGetColor (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || cells [cellIndex].region.customMaterial == null)
				return new Color (0, 0, 0, 0);
			return cells [cellIndex].region.customMaterial.color;
		}

		/// <summary>
		/// Returns current cell's fill texture
		/// </summary>
		public Texture2D CellGetTexture (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || cells [cellIndex].region.customMaterial == null)
				return null;
			return (Texture2D)cells [cellIndex].region.customMaterial.mainTexture;
		}


		/// <summary>
		/// Sets current cell's fill color. Use CellToggleRegionSurface for more options
		/// </summary>
		public void CellSetColor (int cellIndex, Color color) {
			CellToggleRegionSurface (cellIndex, true, color, false, null, Misc.Vector2one, Misc.Vector2zero, 0, false, false);
		}




		/// <summary>
		/// Sets cells' fill color.
		/// </summary>
		public void CellSetColor (List<int>cellIndices, Color color) {
			int cellCount = cellIndices.Count;
			for (int k = 0; k < cellCount; k++) {
				CellToggleRegionSurface (cellIndices [k], true, color, false, null, Misc.Vector2one, Misc.Vector2zero, 0, false, false);
			}
		}



		/// <summary>
		/// Sets current cell's fill texture. Use CellToggleRegionSurface for more options
		/// </summary>
		public void CellSetTexture (int cellIndex, Texture2D texture) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			Cell cell = cells [cellIndex];
			GameObject cellSurface = null;
			int cacheIndex = GetCacheIndexForCellRegion (cellIndex);
			surfaces.TryGetValue (cacheIndex, out cellSurface);
			if (cellSurface == null) {
				CellToggleRegionSurface (cellIndex, true, texture);
				return;
			}
			if (texture != null) {
				if (cell.region.customMaterial == null) {
					cell.region.customMaterial = GetColoredTexturedMaterial (Color.white, texture, _overlayMode == OVERLAY_MODE.Overlay);
				}
				if (cellSurface != null) {
					surfaces [cacheIndex].SetActive (true);
				}
			}
			if (cell.region.customMaterial != null) {
				cell.region.customMaterial.mainTexture = texture;
				Renderer renderer = cellSurface.GetComponent<Renderer> ();
				if (renderer != null)
					renderer.sharedMaterial = cell.region.customMaterial;
			}
			if (_highlightedObj == cellSurface) {
				hudMatCell.mainTexture = texture;
			}
		}


		/// <summary>
		/// Returns current cell's fill texture index (if texture exists in textures list).
		/// Texture index is from 1..32. It will return 0 if texture does not exist or it does not match any texture in the list of textures.
		/// </summary>
		public int CellGetTextureIndex (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count || cells [cellIndex].region.customMaterial == null)
				return 0;
			Texture2D tex = (Texture2D)cells [cellIndex].region.customMaterial.mainTexture;
			for (int k = 1; k < textures.Length; k++) {
				if (tex == textures [k])
					return k;
			}
			return 0;
		}

		/// <summary>
		/// Returns cell's row or -1 if cellIndex is not valid.
		/// </summary>
		public int CellGetRow (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].row;
		}

		/// <summary>
		/// Returns cell's column or -1 if cellIndex is not valid.
		/// </summary>
		public int CellGetColumn (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].column;
		}



		public bool CellIsBorder (int cellIndex) {

			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			Cell cell = cells [cellIndex];
			return (cell.column == 0 || cell.column == _cellColumnCount - 1 || cell.row == 0 || cell.row == _cellRowCount - 1);
		}


		/// <summary>
		/// Returns the_numCellsrovince in the cells array by its reference.
		/// </summary>
		public int TerritoryGetIndex (Territory territory) {
			if (territory == null)
				return -1;
			int index;
			if (territoryLookup.TryGetValue (territory, out index))
				return index;
			else
				return -1;
		}

		/// <summary>
		/// Returns true if cell is visible
		/// </summary>
		public bool CellIsVisible (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			return cells [cellIndex].visible;
		}


		/// <summary>
		/// Merges cell2 into cell1. Cell2 is removed.
		/// Only cells which are neighbours can be merged.
		/// </summary>
		public bool CellMerge (Cell cell1, Cell cell2) {
			if (cell1 == null || cell2 == null)
				return false;
			if (!cell1.region.neighbours.Contains (cell2.region))
				return false;
			cell1.center = (cell2.center + cell1.center) / 2.0f;
			// Polygon UNION operation between both regions
			PolygonClipper pc = new PolygonClipper (cell1.polygon, cell2.polygon);
			pc.Compute (PolygonOp.UNION);
			// Remove cell2 from lists
			CellRemove (cell2);
			// Updates geometry data on cell1
			UpdateCellGeometry (cell1, pc.subject);
			return true;
		}

		/// <summary>
		/// Removes a cell from the cells and territories lists. Note that this operation only removes cell structure but does not affect polygons - mostly internally used
		/// </summary>
		public void CellRemove (Cell cell) {
			if (cell == _cellHighlighted)
				HideCellRegionHighlight ();
			if (cell == _cellLastOver) {
				ClearLastOver ();
			}
			int territoryIndex = cell.territoryIndex;
			if (territoryIndex >= 0) {
				if (territories [territoryIndex].cells.Contains (cell)) {
					territories [territoryIndex].cells.Remove (cell);
				}
			}
			// remove cell from global list
			if (cells.Contains (cell))
				cells.Remove (cell);

			needRefreshRouteMatrix = true;
			needUpdateTerritories = true;
		}

		/// <summary>
		/// Tags a cell with a user-defined integer tag. Cell can be later retrieved very quickly using CellGetWithTag.
		/// </summary>
		public void CellSetTag (Cell cell, int tag) {
			// remove previous tag register
			if (cellTagged.ContainsKey (cell.tag)) {
				cellTagged.Remove (cell.tag);
			}
			// override existing tag
			if (cellTagged.ContainsKey (tag)) {
				cellTagged.Remove (tag);
			}
			cellTagged.Add (tag, cell);
			cell.tag = tag;
		}

		/// <summary>
		/// Tags a cell with a user-defined integer tag. Cell can be later retrieved very quickly using CellGetWithTag.
		/// </summary>
		public void CellSetTag (int cellIndex, int tag) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			CellSetTag (cells [cellIndex], tag);
		}

		/// <summary>
		/// Returns the tag value of a given cell.
		/// </summary>
		public int CellGetTag (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			return cells [cellIndex].tag;
		}

		/// <summary>
		/// Retrieves Cell object with associated tag.
		/// </summary>
		public Cell CellGetWithTag (int tag) {
			Cell cell;
			if (cellTagged.TryGetValue (tag, out cell))
				return cell;
			return null;
		}

		/// <summary>
		/// Returns the shape/surface gameobject of the cell.
		/// </summary>
		/// <returns>The get game object.</returns>
		/// <param name="cellIndex">Cell index.</param>
		public GameObject CellGetGameObject (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return null;
			Cell cell = cells [cellIndex];
			if (cell.region.surfaceGameObject != null)
				return cell.region.surfaceGameObject;
			GameObject go = CellToggleRegionSurface (cellIndex, true, Misc.ColorNull);
			CellToggleRegionSurface (cellIndex, false, Misc.ColorNull);
			return go;
		}

		
		/// <summary>
		/// Specifies if a given cell can be crossed by using the pathfinding engine.
		/// </summary>
		public void CellSetCanCross (int cellIndex, bool canCross) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].canCross = canCross;
			needRefreshRouteMatrix = true;
		}

		/// <summary>
		/// Sets the additional cost of crossing an hexagon side.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side of the hexagon.</param>
		/// <param name="cost">Crossing cost.</param>
		/// <param name="symmetrical">Applies crossing cost to both directions of the given side.</param>
		public void CellSetSideCrossCost (int cellIndex, CELL_SIDE side, float cost, CELL_DIRECTION direction = CELL_DIRECTION.Both) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			Cell cell = cells [cellIndex];
			if (direction != CELL_DIRECTION.Entering) {
				cell.SetSideCrossCost (side, cost);
			}
			if (direction != CELL_DIRECTION.Exiting) {
				int r = cell.row;
				int c = cell.column;
				int or = r, oc = c;
				CELL_SIDE os = side;
				if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
					switch (side) {
					case CELL_SIDE.Bottom:
						or--;
						os = CELL_SIDE.Top;
						break;
					case CELL_SIDE.Top:
						or++;
						os = CELL_SIDE.Bottom;
						break;
					case CELL_SIDE.BottomRight: 
						if (oc % 2 != 0) {
							or--;
						}
						oc++;
						os = CELL_SIDE.TopLeft;
						break;
					case CELL_SIDE.TopRight: 
						if (oc % 2 == 0) {
							or++;
						}
						oc++;
						os = CELL_SIDE.BottomLeft;
						break;
					case CELL_SIDE.TopLeft: 
						if (oc % 2 == 0) {
							or++;
						}
						oc--;
						os = CELL_SIDE.BottomRight;
						break;
					case CELL_SIDE.BottomLeft: 
						if (oc % 2 != 0) {
							or--;
						}
						oc--;
						os = CELL_SIDE.TopRight;
						break;
					}
				} else {
					switch (side) {
					case CELL_SIDE.Bottom:
						or--;
						os = CELL_SIDE.Top;
						break;
					case CELL_SIDE.Top:
						or++;
						os = CELL_SIDE.Bottom;
						break;
					case CELL_SIDE.BottomRight: 
						or--;
						oc++;
						os = CELL_SIDE.TopLeft;
						break;
					case CELL_SIDE.TopRight: 
						or++;
						oc++;
						os = CELL_SIDE.BottomLeft;
						break;
					case CELL_SIDE.TopLeft: 
						or++;
						oc--;
						os = CELL_SIDE.BottomRight;
						break;
					case CELL_SIDE.BottomLeft: 
						or--;
						oc--;
						os = CELL_SIDE.TopRight;
						break;
					case CELL_SIDE.Left:
						oc--;
						os = CELL_SIDE.Right;
						break;
					case CELL_SIDE.Right:
						oc++;
						os = CELL_SIDE.Left;
						break;
					}
				}
				if (or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount) {
					int oindex = CellGetIndex (or, oc);
					if (oindex >= 0) {
						cells [oindex].SetSideCrossCost (os, cost);
					}
				}
			}
		}

		/// <summary>
		/// Gets the cost of crossing any hexagon side.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side of the hexagon.</param>/// 
		/// <param name="direction">The direction for getting the cost. Entering or exiting values are acceptable. Both will return the entering cost.</param>
		public float CellGetSideCrossCost (int cellIndex, CELL_SIDE side, CELL_DIRECTION direction = CELL_DIRECTION.Entering) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			Cell cell = cells [cellIndex];
			if (direction == CELL_DIRECTION.Exiting) {
				return cell.GetSideCrossCost (side);
			}
			int r = cell.row;
			int c = cell.column;
			int or = r, oc = c;
			CELL_SIDE os = side;
			if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					if (oc % 2 != 0) {
						or--;
					}
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					if (oc % 2 == 0) {
						or++;
					}
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					if (oc % 2 == 0) {
						or++;
					}
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					if (oc % 2 != 0) {
						or--;
					}
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				}
			} else {
				switch (side) {
				case CELL_SIDE.Bottom:
					or--;
					os = CELL_SIDE.Top;
					break;
				case CELL_SIDE.Top:
					or++;
					os = CELL_SIDE.Bottom;
					break;
				case CELL_SIDE.BottomRight: 
					or--;
					oc++;
					os = CELL_SIDE.TopLeft;
					break;
				case CELL_SIDE.TopRight: 
					or++;
					oc++;
					os = CELL_SIDE.BottomLeft;
					break;
				case CELL_SIDE.TopLeft: 
					or++;
					oc--;
					os = CELL_SIDE.BottomRight;
					break;
				case CELL_SIDE.BottomLeft: 
					or--;
					oc--;
					os = CELL_SIDE.TopRight;
					break;
				case CELL_SIDE.Right:
					oc++;
					os = CELL_SIDE.Left;
					break;
				case CELL_SIDE.Left:
					oc--;
					os = CELL_SIDE.Right;
					break;
				}
			}
			if (or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount) {
				int oindex = CellGetIndex (or, oc);
				return cells [oindex].GetSideCrossCost (os);
			} else {
				return 0;
			}
		}

		/// <summary>
		/// Sets cost of entering a given hexagonal cell.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="cost">Crossing cost.</param>
		[Obsolete ("Use CellSetCrossCost")]
		public void CellSetAllSidesCrossCost (int cellIndex, float cost) {
			CellSetCrossCost (cellIndex, cost);
		}


		/// <summary>
		/// Sets cost of entering or exiting a given hexagonal cell across any edge.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="cost">Crossing cost.</param>
		public void CellSetCrossCost (int cellIndex, float cost, CELL_DIRECTION direction = CELL_DIRECTION.Entering) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			for (int side = 0; side < 8; side++) {
				CellSetSideCrossCost (cellIndex, (CELL_SIDE)side, cost, direction);
			}
		}


		/// <summary>
		/// Returns the cost of entering or exiting a given hexagonal cell without specifying a specific edge. This method is used along CellSetCrossCost which doesn't take into account per-edge costs.
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="cost">Crossing cost.</param>
		public float CellGetCrossCost (int cellIndex, CELL_DIRECTION direction = CELL_DIRECTION.Entering) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return 0;
			return CellGetSideCrossCost (cellIndex, CELL_SIDE.Top, direction);
		}


		/// <summary>
		/// Specifies the cell group (by default 1) used by FindPath cellGroupMask optional argument
		/// </summary>
		public void CellSetGroup (int cellIndex, int group) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].group = group;
			needRefreshRouteMatrix = true;
		}

		/// <summary>
		/// Returns cell group (default 1)
		/// </summary>
		public int CellGetGroup (int cellIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return -1;
			return cells [cellIndex].group;
		}



		/// <summary>
		/// Returns the indices of all cells belonging to a group in the indices array which must be fully allocated when passed. Also the length of this array determines the maximum number of indices returned.
		/// This method returns the actual number of indices returned, regardless of the length the array. This design helps reduce heap allocations.
		/// </summary>
		public int CellGetFromGroup (int group, int[] indices) {
			if (indices == null || cells == null)
				return 0;
			int cellCount = cells.Count;
			int count = 0;
			for (int k = 0; k < cellCount && k < indices.Length; k++) {
				if (cells [k].group == group) {
					indices [count++] = k;
				}
			}
			return count;
		}


		/// <summary>
		/// Specifies if a given cell is visible.
		/// </summary>
		public void CellSetVisible (int cellIndex, bool visible) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			if (cells [cellIndex].visible == visible)
				return; // nothing to do

			cells [cellIndex].visible = visible;
			if (cellIndex == _cellLastOverIndex) {
				ClearLastOver ();
			}
			needRefreshRouteMatrix = true;
			refreshCellMesh = true;
			issueRedraw = RedrawType.Full;
		}


		CELL_SIDE GetSideByVector (Vector2 dir) {
			if (dir.x == 0) {
				return dir.y > 0 ? CELL_SIDE.Top : CELL_SIDE.Bottom;
			} else if (dir.x > 0) {
				return dir.y > 0 ? CELL_SIDE.TopRight : CELL_SIDE.BottomRight;
			} else {
				return dir.y > 0 ? CELL_SIDE.TopLeft : CELL_SIDE.BottomLeft;
			}
		}

		/// <summary>
		/// Sets the cost for going from one cell to another (both cells must be adjacent).
		/// </summary>
		/// <param name="cellStartIndex">Cell start index.</param>
		/// <param name="cellEndIndex">Cell end index.</param>
		public void CellSetCrossCost (int cellStartIndex, int cellEndIndex, float cost) {
			if (cellStartIndex < 0 || cellStartIndex >= cells.Count || cellEndIndex < 0 || cellEndIndex >= cells.Count)
				return;
			CELL_SIDE side = GetSideByVector (cells [cellStartIndex].center - cells [cellEndIndex].center);
			CellSetSideCrossCost (cellEndIndex, side, cost, CELL_DIRECTION.Entering);
		}

		/// <summary>
		/// Returns the cost of going from one cell to another (both cells must be adjacent)
		/// </summary>
		/// <returns>The get cross cost.</returns>
		/// <param name="cellStartIndex">Cell start index.</param>
		/// <param name="cellEndIndex">Cell end index.</param>
		public float CellGetCrossCost (int cellStartIndex, int cellEndIndex) {
			if (cellStartIndex < 0 || cellStartIndex >= cells.Count || cellEndIndex < 0 || cellEndIndex >= cells.Count)
				return 0;
			CELL_SIDE side = GetSideByVector (cells [cellStartIndex].center - cells [cellEndIndex].center);
			return CellGetSideCrossCost (cellEndIndex, side, CELL_DIRECTION.Entering);
		}

		/// <summary>
		/// Specified visibility for a group of cells that lay within a given rectangle
		/// </summary>
		/// <param name="rect">Rect or boundary. If local space is used, coordinates must be in range (-0.5..0.5)</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public bool CellSetVisible (Rect rect, bool visible, bool worldSpace = false) {
			return ToggleCellsVisibility (rect, visible, worldSpace);
		}

		/// <summary>
		/// Specified visibility for a group of cells that lay within a given gameObject
		/// </summary>
		/// <param name="obj">GameObject whose mesh or collider will be used.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public bool CellSetVisible (GameObject obj, bool visible) {
			Collider collider = obj.GetComponent<Collider> ();
			if (collider != null)
				return CellSetVisible (collider.bounds, visible);
			Renderer renderer = obj.GetComponent<Renderer> ();
			if (renderer != null)
				return CellSetVisible (renderer.bounds, visible);
			return false;
		}


		/// <summary>
		/// Specified visibility for a group of cells that lay within a given gameObject
		/// </summary>
		/// <param name="renderer">Renderer whose bounds will be used.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public bool CellSetVisible (Renderer renderer, bool visible) {
			return CellSetVisible (renderer.bounds, visible);
		}


		/// <summary>
		/// Specified visibility for a group of cells that lay within a given collider
		/// </summary>
		/// <param name="collider">Collider that provides boundary.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public bool CellSetVisible (Collider collider, bool visible) {
			if (collider == null)
				return false;
			return CellSetVisible (collider.bounds, visible);
		}


		/// <summary>
		/// Specified visibility for a group of cells that lay within a given bounds
		/// </summary>
		/// <param name="bounds">Bounds in world space coordinates.</param>
		/// <param name="visible">If set to <c>true</c> visible.</param>
		public bool CellSetVisible (Bounds bounds, bool visible) {
			Rect rect = new Rect ();
			Vector3 pos = bounds.min;
			if (!GetLocalHitFromWorldPosition (ref pos))
				return false;
			rect.min = pos;
			pos = bounds.max;
			if (!GetLocalHitFromWorldPosition (ref pos))
				return false;
			rect.max = pos;
			return ToggleCellsVisibility (rect, visible, false);
		}


		/// <summary>
		/// Specifies if a given cell's border is visible.
		/// </summary>
		public void CellSetBorderVisible (int cellIndex, bool visible) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return;
			cells [cellIndex].borderVisible = visible;
		}



		/// <summary>
		/// Returns the cell object under position in local or worldSpace coordinates
		/// </summary>
		/// <returns>The get at position.</returns>
		/// <param name="position">Position.</param>
		/// <param name="worldSpace">If set to <c>true</c>, position is given in world space coordinate units, otherwise position refer to local coordinates.</param>
		/// <param name="territoryIndex">Optional territory index to restrict the search and make it faster.</param>
		public Cell CellGetAtPosition (Vector3 position, bool worldSpace = false, int territoryIndex = -1) {
			return GetCellAtPoint (position, worldSpace, territoryIndex);
		}



		/// <summary>
		/// Returns the indices of the cells within or under a volume
		/// </summary>
		/// <param name="bounds">The bounds of volume or area in world space coordinates, for example the collider bounds.</param>
		/// <param name="cellIndices">An initialized list where results will be added</param>
		/// <param name="padding">Optional margin that's added or substracted to the resulting area.</param>
		public int CellGetInArea (Bounds bounds, List<int> cellIndices, float padding = 0) {
			if (cellIndices == null) {
				Debug.LogError ("CellGetInArea: cellIndices must be initialized.");
				return 0;
			}
			return GetCellInArea (bounds, cellIndices, padding);
		}



		/// <summary>
		/// Sets the territory of a cell triggering territory boundary recalculation
		/// </summary>
		/// <returns><c>true</c>, if cell was transferred., <c>false</c> otherwise.</returns>
		public bool CellSetTerritory (int cellIndex, int territoryIndex) {
			if (cellIndex < 0 || cellIndex >= cells.Count)
				return false;
			Cell cell = cells [cellIndex];
			int terrCount = territories.Count;
			if (cell.territoryIndex >= 0 && cell.territoryIndex < terrCount && territories [cell.territoryIndex].cells.Contains (cell)) {
				territories [cell.territoryIndex].isDirty = true;
				territories [cell.territoryIndex].cells.Remove (cell);
			}
			cells [cellIndex].territoryIndex = territoryIndex;
			if (territoryIndex >= 0 && territoryIndex < terrCount) {
				territories [territoryIndex].isDirty = true;
			}
			needUpdateTerritories = true;
			issueRedraw = RedrawType.Incremental;
			return true;
		}

		/// <summary>
		/// Returns a string-packed representation of current cells settings.
		/// Each cell separated by ;
		/// Individual settings mean:
		/// Position	Meaning
		/// 0			Visibility (0 = invisible, 1 = visible)
		/// 1			Territory Index
		/// 2			Color R (0..1)
		/// 3			Color G (0..1)
		/// 4			Color B (0..1)
		/// 5			Color A (0..1)
		/// 6			Texture Index
		/// </summary>
		/// <returns>The get configuration data.</returns>
		public string CellGetConfigurationData () {
			StringBuilder sb = new StringBuilder ();
			int cellsCount = cells.Count;
			for (int k = 0; k < cellsCount; k++) {
				if (k > 0)
					sb.Append (";");
				// 0
				Cell cell = cells [k];
				if (cell.visible) {
					sb.Append ("1");
				} else {
					sb.Append ("0");
				}
				// 1 territory index
				sb.Append (",");
				sb.Append (cell.territoryIndex);
				// 2 color.a
				sb.Append (",");
				Color color = CellGetColor (k);
				sb.Append (color.a.ToString ("F3"));
				// 3 color.r
				sb.Append (",");
				sb.Append (color.r.ToString ("F3"));
				// 4 color.g
				sb.Append (",");
				sb.Append (color.g.ToString ("F3"));
				// 5 color.b
				sb.Append (",");
				sb.Append (color.b.ToString ("F3"));
				// 6 texture index
				sb.Append (",");
				sb.Append (CellGetTextureIndex (k));
				// 7 tag
				sb.Append (",");
				sb.Append (cell.tag);
			}
			return sb.ToString ();
		}

		public void CellSetConfigurationData (string cellData, int[] filterTerritories) {
			if (cells == null)
				return;
			string[] cellsInfo = cellData.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			char[] separators = new char[] { ',' };
			for (int k = 0; k < cellsInfo.Length && k < cells.Count; k++) {
				string[] cellInfo = cellsInfo [k].Split (separators, StringSplitOptions.RemoveEmptyEntries);
				int length = cellInfo.Length;
				if (length > 1) {
					int territoryIndex = FastConvertToInt (cellInfo [1]);
					if (filterTerritories != null && !filterTerritories.Contains (territoryIndex))
						continue;
					cells [k].territoryIndex = territoryIndex;
				}
				if (length > 0) {
					if (cellInfo [0].Length > 0) {
						cells [k].visible = cellInfo [0] [0] != '0';
					}
				}
				Color color = new Color (0, 0, 0, 0);
				if (length > 5) {
					Single.TryParse (cellInfo [2], out color.a);
					if (color.a > 0) {
						Single.TryParse (cellInfo [3], out color.r);
						Single.TryParse (cellInfo [4], out color.g);
						Single.TryParse (cellInfo [5], out color.b);
					}
				} 
				int textureIndex = -1;
				if (length > 6) {
					textureIndex = FastConvertToInt (cellInfo [6]);
				}
				if (color.a > 0 || textureIndex >= 1) {
					CellToggleRegionSurface (k, true, color, false, textureIndex);
				}
				if (length > 7) {
					CellSetTag (k, FastConvertToInt (cellInfo [7]));
				}
			}
			needUpdateTerritories = true;
			needRefreshRouteMatrix = true;
			Redraw ();
			isDirty = true;
		}

		/// <summary>
		/// Returns the cell located at given row and column
		/// </summary>
		public Cell CellGetAtPosition (int column, int row) {
			int index = row * _cellColumnCount + column;
			if (index >= 0 && index < cells.Count)
				return cells [index];
			return null;
		}

		/// <summary>
		/// Traces a line between two positions and check if there's no cell blocking the line
		/// </summary>
		/// <returns><c>true</c>, if there's a straight path of non-blocking cells between the two positions<c>false</c> otherwise.</returns>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		/// <param name="cellIndices">Cell indices.</param>
		/// <param name="cellGroupMask">Optional cell layer mask</param>
		/// <param name="lineResolution">Resolution of the line. Minimum is 1, increase to improve line accuracy.</param>
		/// <param name="exhaustiveCheck">If set to true, all vertices of destination cell will be considered instead of its center</param>
		public bool CellGetLineOfSight (Vector3 startPosition, Vector3 endPosition, ref List<int> cellIndices, ref List<Vector3> worldPositions, int cellGroupMask = -1, float lineResolution = 1f, bool exhaustiveCheck = false) {

			cellIndices = null;

			Cell startCell = CellGetAtPosition (startPosition, true);
			Cell endCell = CellGetAtPosition (endPosition, true);
			if (startCell == null || endCell == null) {
				return false;
			}

			int cell1 = CellGetIndex (startCell);
			int cell2 = CellGetIndex (endCell);
			if (cell1 < 0 || cell2 < 0)
				return false;

			return CellGetLineOfSight (cell1, cell2, ref cellIndices, ref worldPositions, cellGroupMask, lineResolution, exhaustiveCheck);
		}

		/// <summary>
		/// Traces a line between two positions and check if there's no cell blocking the line
		/// </summary>
		/// <returns><c>true</c>, if there's a straight path of non-blocking cells between the two positions<c>false</c> otherwise.</returns>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		/// <param name="cellIndices">Cell indices.</param>
		/// <param name="cellGroupMask">Optional cell layer mask</param>
		/// <param name="lineResolution">Resolution of the line. Defaults to 1, increase to improve line accuracy.</param>
		/// <param name="exhaustiveCheck">If set to true, all vertices of destination cell will be considered instead of its center</param>
		public bool CellGetLineOfSight (int startCellIndex, int endCellIndex, ref List<int> cellIndices, ref List<Vector3> worldPositions, int cellGroupMask = -1, float lineResolution = 1f, bool exhaustiveCheck = false) {

			if (cellIndices == null)
				cellIndices = new List<int> ();
			else
				cellIndices.Clear ();
			if (worldPositions == null)
				worldPositions = new List<Vector3> ();
			else
				worldPositions.Clear ();
			if (startCellIndex < 0 || startCellIndex >= cells.Count || endCellIndex < 0 || endCellIndex >= cells.Count)
				return false;

			Vector3 startPosition = CellGetPosition (startCellIndex);
			Vector3 endPosition;
			int vertexCount = exhaustiveCheck ? cells [endCellIndex].region.points.Count : 0;
			bool success = true;

			for (int p = 0; p <= vertexCount; p++) {
				if (p == 0) {
					endPosition = CellGetPosition (endCellIndex);
				} else {
					cellIndices.Clear ();
					worldPositions.Clear ();
					endPosition = CellGetVertexPosition (endCellIndex, p - 1);
				}

				float stepSize;
				int numSteps;
				if (_gridTopology != GRID_TOPOLOGY.Irregular) {
					// Hexagon distance
					numSteps = CellGetHexagonDistance (startCellIndex, endCellIndex);
				} else {
					stepSize = 1f / lineResolution;
					float dist = Vector3.Distance (startPosition, endPosition);
					numSteps = (int)(dist / stepSize);
				}

				Cell lastCell = null;
				success = true;
				for (int k = 1; k <= numSteps; k++) {
					Vector3 position = Vector3.Lerp (startPosition, endPosition, (float)k / numSteps);
					Cell cell = k == numSteps ? cells [endCellIndex] : CellGetAtPosition (position, true);
					if (cell != null && cell != lastCell) {
						if (cell != cells [endCellIndex]) {
							if (!cell.canCross || (cell.group & cellGroupMask) == 0) {
								success = false;
								break;
							}
						}
						cellIndices.Add (CellGetIndex (cell));
					}
					worldPositions.Add (position);
					lastCell = cell;
				}
				if (success) {
					if (p == 0)
						return true;
					break;
				}
			}
			if (success && exhaustiveCheck) {
				CellGetLine (startCellIndex, endCellIndex, ref cellIndices, ref worldPositions, lineResolution);
				return true;
			}
			return success;
		}

		/// <summary>
		/// Returns a line composed of cells and world positions from starting cell to ending cell
		/// </summary>
		/// <returns><c>true</c>, if there's a straight path of non-blocking cells between the two positions<c>false</c> otherwise.</returns>
		/// <param name="startPosition">Start position.</param>
		/// <param name="endPosition">End position.</param>
		/// <param name="cellIndices">Cell indices.</param>
		/// <param name="lineResolution">Resolution of the line. Defaults to 1, increase to improve line accuracy.</param>
		public void CellGetLine (int startCellIndex, int endCellIndex, ref List<int> cellIndices, ref List<Vector3> worldPositions, float lineResolution = 1f) {

			if (cellIndices == null)
				cellIndices = new List<int> ();
			else
				cellIndices.Clear ();
			if (worldPositions == null)
				worldPositions = new List<Vector3> ();
			else
				worldPositions.Clear ();
			if (startCellIndex < 0 || startCellIndex >= cells.Count || endCellIndex < 0 || endCellIndex >= cells.Count)
				return;

			Vector3 startPosition = CellGetPosition (startCellIndex);
			Vector3 endPosition = CellGetPosition (endCellIndex);

			float stepSize;
			int numSteps;
			if (_gridTopology != GRID_TOPOLOGY.Irregular) {
				// Hexagon distance
				numSteps = CellGetHexagonDistance (startCellIndex, endCellIndex);
			} else {
				stepSize = 1f / lineResolution;
				float dist = Vector3.Distance (startPosition, endPosition);
				numSteps = (int)(dist / stepSize);
			}

			Cell lastCell = null;
			for (int k = 1; k <= numSteps; k++) {
				Vector3 position = Vector3.Lerp (startPosition, endPosition, (float)k / numSteps);
				Cell cell = CellGetAtPosition (position, true);
				if (cell != null && cell != lastCell) {
					cellIndices.Add (CellGetIndex (cell));
				}
				worldPositions.Add (position);
				lastCell = cell;
			}
		}

		/// <summary>
		/// Returns the hexagon distance between two cells (number of steps to reach end cell from start cell).
		/// This method does not take into account cell masks or blocking cells. It just returns the distance.
		/// </summary>
		/// <returns>The get hexagon distance.</returns>
		/// <param name="startCellIndex">Start cell index.</param>
		/// <param name="endCellIndex">End cell index.</param>
		public int CellGetHexagonDistance (int startCellIndex, int endCellIndex) {
			if (cells == null)
				return -1;
			int cellCount = cells.Count;
			if (startCellIndex < 0 || startCellIndex >= cellCount || endCellIndex < 0 || endCellIndex >= cellCount)
				return -1;
			int r0 = cells [startCellIndex].row;
			int c0 = cells [startCellIndex].column;
			int r1 = cells [endCellIndex].row;
			int c1 = cells [endCellIndex].column;
			int offset = _evenLayout ? 0 : 1;
			int y0 = r0 - Mathf.FloorToInt ((c0 + offset) / 2);
			int x0 = c0;
			int y1 = r1 - Mathf.FloorToInt ((c1 + offset) / 2);
			int x1 = c1;
			int dx = x1 - x0;
			int dy = y1 - y0;
			int numSteps = Mathf.Max (Mathf.Abs (dx), Mathf.Abs (dy));
			numSteps = Mathf.Max (numSteps, Mathf.Abs (dx + dy));
			return numSteps;
		}

		/// <summary>
		/// Removes any color or texture from a cell and hides it
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		public void CellClear (int cellIndex) {
			List<int> cellIndices = new List<int> ();
			cellIndices.Add (cellIndex);
			CellClear (cellIndices);
		}

		/// <summary>
		/// Removes any color or texture from a list of cells and hides them
		/// </summary>
		/// <param name="cellIndices">Cell indices.</param>
		public void CellClear (List<int> cellIndices) {
			if (cellIndices == null)
				return;

			int count = cellIndices.Count;
			for (int k = 0; k < count; k++) {
				// Check if cell has a SurfaceFader animator
				int cellIndex = cellIndices [k];
				Cell cell = cells [cellIndex];
				if (cell.region.surfaceGameObject != null) {
					SurfaceFader sf = cell.region.surfaceGameObject.GetComponent<SurfaceFader> ();
					if (sf != null) {
						sf.Finish ();
					}
					cell.region.surfaceGameObject.SetActive (false);
				}
				if (cell.region.customMaterial != null) {
					cell.region.customMaterial = null;
				}
			}
		}


		/// <summary>
		/// Pregenerates and caches cell geometry for faster performance during gameplay
		/// </summary>
		public void WarmCells () {
			int cellCount = cells.Count;
			Material mat = GetColoredTexturedMaterial (Color.white, null, false);
			for (int k = 0; k < cellCount; k++) {
				GenerateCellRegionSurface (k, mat, Misc.Vector2one, Misc.Vector2zero, 0, false);
			}
		}


		/// <summary>
		/// Draws a line over a cell side.
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="cellIndex">Cell index.</param>
		/// <param name="side">Side.</param>
		/// <param name="color">Color.</param>
		/// <param name="width">Width.</param>
		public GameObject DrawLine (int cellIndex, CELL_SIDE side, Color color, float width) {
			GameObject line = new GameObject ("Line");
			LineRenderer lr = line.AddComponent<LineRenderer> ();
			if (cellLineMat == null)
				cellLineMat = Resources.Load<Material> ("Materials/CellLine") as Material;
			Material mat = Instantiate (cellLineMat) as Material;
			disposalManager.MarkForDisposal (mat);
			mat.color = color;
			lr.sharedMaterial = mat;
			lr.useWorldSpace = true;
			lr.positionCount = 2;
			lr.startWidth = width;
			lr.endWidth = width;
			int v1, v2;
			switch (side) {
			case CELL_SIDE.BottomLeft:
				v1 = 0;
				v2 = 1;
				break;
			case CELL_SIDE.Bottom:
				v1 = 1;
				v2 = 2;
				break;
			case CELL_SIDE.BottomRight:
				v1 = 2;
				v2 = 3;
				break;
			case CELL_SIDE.TopRight:
				v1 = 3;
				v2 = 4;
				break;
			case CELL_SIDE.Top:
				v1 = 4;
				v2 = 5;
				break;
			default: // BottomLeft
				v1 = 5;
				v2 = 0;
				break;
			}
			Vector3 offset = transform.forward * 0.05f;
			lr.SetPosition (0, CellGetVertexPosition (cellIndex, v1) - offset);
			lr.SetPosition (1, CellGetVertexPosition (cellIndex, v2) - offset);
			return line;
		}

		/// <summary>
		/// Draws a line connecting two cells centers
		/// </summary>
		/// <returns>The line.</returns>
		/// <param name="cellIndex1">Cell index1.</param>
		/// <param name="cellIndex2">Cell index2.</param>
		/// <param name="color">Color.</param>
		/// <param name="width">Width.</param>
		public GameObject DrawLine (int cellIndex1, int cellIndex2, Color color, float width) {

			GameObject line = new GameObject ("Line");
			LineRenderer lr = line.AddComponent<LineRenderer> ();
			if (cellLineMat == null)
				cellLineMat = Resources.Load<Material> ("Materials/CellLine") as Material;
			Material mat = Instantiate (cellLineMat) as Material;
			disposalManager.MarkForDisposal (mat);
			mat.color = color;
			lr.sharedMaterial = mat;
			lr.useWorldSpace = true;
			lr.positionCount = 2;
			lr.startWidth = width;
			lr.endWidth = width;
			Vector3 offset = transform.forward * 0.05f;
			lr.SetPosition (0, CellGetPosition (cellIndex1) - offset);
			lr.SetPosition (1, CellGetPosition (cellIndex2) - offset);
			return line;
		}


		/// <summary>
		/// Escales the gameobject of a colored/textured surface
		/// </summary>
		/// <param name="surf">Surf.</param>
		/// <param name="cell">Cell.</param>
		/// <param name="scale">Scale.</param>
		public void CellScaleSurface (int cellIndex, float scale) {
			if (cellIndex < 0)
				return;
			Cell cell = cells [cellIndex];
			GameObject surf = cell.region.surfaceGameObject;
			ScaleSurface (surf, cell.center, scale);
		}



		#endregion


	
	}
}

