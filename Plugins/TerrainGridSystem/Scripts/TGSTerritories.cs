using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TGS.Geom;
using TGS.PathFinding;

namespace TGS {
	
	/* Event definitions */
	public delegate void TerritoryEvent (int territoryIndex);
	public delegate void TerritoryHighlightEvent (int territoryIndex, ref bool cancelHighlight);
	public delegate void TerritoryClickEvent (int territoryIndex, int buttonIndex);

	public partial class TerrainGridSystem : MonoBehaviour {

		public event TerritoryEvent OnTerritoryEnter;
		public event TerritoryEvent OnTerritoryExit;

		/// <summary>
		/// Occurs when user press the mouse button on a territory
		/// </summary>
		public event TerritoryClickEvent OnTerritoryMouseDown;

		/// <summary>
		/// Occurs when user releases the mouse button on the same territory that started clicking
		/// </summary>
		public event TerritoryClickEvent OnTerritoryClick;

		/// <summary>
		/// Occurs when user releases the mouse button on a territory
		/// </summary>
		public event TerritoryClickEvent OnTerritoryMouseUp;

		/// <summary>
		/// Occurs when a territory is about to get highlighted
		/// </summary>
		public event TerritoryHighlightEvent OnTerritoryHighlight;

		[NonSerialized]
		public List<Territory> territories;

		public Texture2D territoriesTexture;
		public Color territoriesTextureNeutralColor;


		[SerializeField]
		int _numTerritories = 3;

		/// <summary>
		/// Gets or sets the number of territories.
		/// </summary>
		public int numTerritories { 
			get { return _numTerritories; } 
			set {
				if (_numTerritories != value) {
					_numTerritories = Mathf.Clamp (value, 1, MAX_TERRITORIES);
					GenerateMap (true);
					isDirty = true;
				}
			}
		}

		[SerializeField]
		bool
			_showTerritories = false;

		/// <summary>
		/// Toggle frontiers visibility.
		/// </summary>
		public bool showTerritories { 
			get {
				return _showTerritories; 
			}
			set {
				if (value != _showTerritories) {
					_showTerritories = value;
					isDirty = true;
					if (!_showTerritories && territoryLayer != null) {
						territoryLayer.SetActive (false);
						ClearLastOver ();
					} else {
						Redraw ();
					}
				}
			}
		}

		[SerializeField]
		bool _colorizeTerritories = false;

		/// <summary>
		/// Toggle colorize countries.
		/// </summary>
		public bool colorizeTerritories { 
			get {
				return _colorizeTerritories; 
			}
			set {
				if (value != _colorizeTerritories) {
					_colorizeTerritories = value;
					isDirty = true;
					if (!_colorizeTerritories && surfacesLayer != null) {
						DestroyTerritorySurfaces ();
					} else {
						Redraw ();
					}
				}
			}
		}

		[SerializeField]
		float _colorizedTerritoriesAlpha = 0.7f;

		public float colorizedTerritoriesAlpha { 
			get { return _colorizedTerritoriesAlpha; } 
			set {
				if (_colorizedTerritoriesAlpha != value) {
					_colorizedTerritoriesAlpha = value;
					isDirty = true;
					UpdateColorizedTerritoriesAlpha ();
				}
			}
		}


		[SerializeField]
		Color
			_territoryHighlightColor = new Color (1, 0, 0, 0.9f);

		/// <summary>
		/// Fill color to use when the mouse hovers a territory's region.
		/// </summary>
		public Color territoryHighlightColor {
			get {
				return _territoryHighlightColor;
			}
			set {
				if (value != _territoryHighlightColor) {
					_territoryHighlightColor = value;
					isDirty = true;
					if (hudMatTerritoryOverlay != null && _territoryHighlightColor != hudMatTerritoryOverlay.color) {
						hudMatTerritoryOverlay.color = _territoryHighlightColor;
					}
					if (hudMatTerritoryGround != null && _territoryHighlightColor != hudMatTerritoryGround.color) {
						hudMatTerritoryGround.color = _territoryHighlightColor;
					}
				}
			}
		}

		
		[SerializeField]
		Color
			_territoryFrontierColor = new Color (0, 1, 0, 1.0f);

		/// <summary>
		/// Territories border color
		/// </summary>
		public Color territoryFrontiersColor {
			get {
				if (territoriesMat != null) {
					return territoriesMat.color;
				} else {
					return _territoryFrontierColor;
				}
			}
			set {
				if (value != _territoryFrontierColor) {
					_territoryFrontierColor = value;
					isDirty = true;
					if (territoriesMat != null && _territoryFrontierColor != territoriesMat.color) {
						territoriesMat.color = _territoryFrontierColor;
					}
				}
			}
		}

		public float territoryFrontiersAlpha {
			get {
				return _territoryFrontierColor.a;
			}
			set {
				if (_territoryFrontierColor.a != value) {
					_territoryFrontierColor = new Color (_territoryFrontierColor.r, _territoryFrontierColor.g, _territoryFrontierColor.b, value);
				}
				if (_territoryDisputedFrontierColor.a != value) {
					_territoryDisputedFrontierColor = new Color (_territoryDisputedFrontierColor.r, _territoryDisputedFrontierColor.g, _territoryDisputedFrontierColor.b, value);
				}
			}
		}

		
		[SerializeField]
		Color
			_territoryDisputedFrontierColor;

		/// <summary>
		/// Territories disputed borders color
		/// </summary>
		public Color territoryDisputedFrontierColor {
			get {
				if (territoriesDisputedMat != null) {
					return territoriesDisputedMat.color;
				} else {
					return _territoryDisputedFrontierColor;
				}
			}
			set {
				if (value != _territoryDisputedFrontierColor) {
					_territoryDisputedFrontierColor = value;
					isDirty = true;
					if (territoriesDisputedMat != null && _territoryDisputedFrontierColor != territoriesDisputedMat.color) {
						territoriesDisputedMat.color = _territoryDisputedFrontierColor;
					}
				}
			}
		}


		[SerializeField]
		bool _showTerritoriesOuterBorder = true;

		/// <summary>
		/// Shows perimetral/outer border of territories?
		/// </summary>
		/// <value><c>true</c> if show territories outer borders; otherwise, <c>false</c>.</value>
		public bool showTerritoriesOuterBorders {
			get { return _showTerritoriesOuterBorder; }
			set {
				if (_showTerritoriesOuterBorder != value) {
					_showTerritoriesOuterBorder = value;
					isDirty = true;
					Redraw ();
				}
			}
		}

		
		[SerializeField]
		bool _allowTerritoriesInsideTerritories = false;

		/// <summary>
		/// Set this property to true to allow territories to be surrounded by other territories.
		/// </summary>
		public bool allowTerritoriesInsideTerritories {
			get { return _allowTerritoriesInsideTerritories; }
			set {
				if (_allowTerritoriesInsideTerritories != value) {
					_allowTerritoriesInsideTerritories = value;
					isDirty = true;
				}
			}
		}

		/// <summary>
		/// Returns Territory under mouse position or null if none.
		/// </summary>
		public Territory territoryHighlighted { get { return _territoryHighlighted; } }

		/// <summary>
		/// Returns currently highlighted territory index in the countries list.
		/// </summary>
		public int territoryHighlightedIndex { get { return _territoryHighlightedIndex; } }

		/// <summary>
		/// Returns Territory index which has been clicked
		/// </summary>
		public int territoryLastClickedIndex { get { return _territoryLastClickedIndex; } }


		#region Public Territories Functions

		/// <summary>
		/// Uncolorize/hide all territories.
		/// </summary>
		public void TerritoryHideRegionSurfaces () {
			if (territories == null)
				return;
			int terrCount = territories.Count;
			for (int k = 0; k < terrCount; k++) {
				TerritoryHideRegionSurface (k);
			}
		}


		/// <summary>
		/// Uncolorize/hide specified territory by index in the territories collection.
		/// </summary>
		public void TerritoryHideRegionSurface (int territoryIndex) {
			if (_territoryHighlightedIndex != territoryIndex) {
				int cacheIndex = GetCacheIndexForTerritoryRegion (territoryIndex);
				GameObject surf;
				if (surfaces.TryGetValue (cacheIndex, out surf)) {
					if (surf == null) {
						surfaces.Remove (cacheIndex);
					} else {
						surf.SetActive (false);
					}
				}
			}
			territories [territoryIndex].region.customMaterial = null;
		}

		/// <summary>
		/// Colorize specified territory by index.
		/// </summary>
		/// <param name="territoryIndex">Territory index.</param>
		/// <param name="visible">If the colored surface will be visible or not.</param>
		/// <param name="color">Color.</param>
		public GameObject TerritoryToggleRegionSurface (int territoryIndex, bool visible, Color color) {
			return TerritoryToggleRegionSurface (territoryIndex, visible, color, null, Misc.Vector2one, Misc.Vector2zero, 0, false);
		}

		/// <summary>
		/// Colorize specified territory by index.
		/// </summary>
		/// <param name="territoryIndex">Territory index.</param>
		/// <param name="visible">If the colored surface will be visible or not.</param>
		/// <param name="color">Color.</param>
		/// <param name="texture">Texture, which will be tinted according to the color. Use Color.white to preserve original texture colors.</param>
		public GameObject TerritoryToggleRegionSurface (int territoryIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture) {
			return TerritoryToggleRegionSurface (territoryIndex, visible, color, texture, Misc.Vector2one, Misc.Vector2zero, 0, false);
		}

		/// <summary>
		/// Colorize specified territory by index.
		/// </summary>
		/// <param name="territoryIndex">Territory index.</param>
		/// <param name="visible">If the colored surface will be visible or not.</param>
		/// <param name="color">Color.</param>
		/// <param name="texture">Texture, which will be tinted according to the color. Use Color.white to preserve original texture colors.</param>
		/// <param name="textureScale">Texture scale.</param>
		/// <param name="textureOffset">Texture offset.</param>
		/// <param name="textureRotation">Texture rotation.</param>
		public GameObject TerritoryToggleRegionSurface (int territoryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace) {
			return TerritoryToggleRegionSurface (territoryIndex, visible, color, texture, textureScale, textureOffset, textureRotation, false, rotateInLocalSpace);
		}


		/// <summary>
		/// Colorize specified territory by index.
		/// </summary>
		/// <param name="territoryIndex">Territory index.</param>
		/// <param name="visible">If the colored surface will be visible or not.</param>
		/// <param name="color">Color.</param>
		/// <param name="texture">Texture, which will be tinted according to the color. Use Color.white to preserve original texture colors.</param>
		/// <param name="textureScale">Texture scale.</param>
		/// <param name="textureOffset">Texture offset.</param>
		/// <param name="textureRotation">Texture rotation.</param>
		/// <param name="overlay">If set to <c>true</c> the colored surface will be shown over any object.</param>
		public GameObject TerritoryToggleRegionSurface (int territoryIndex, bool visible, Color color, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool overlay, bool rotateInLocalSpace) {
			if (!visible) {
				TerritoryHideRegionSurface (territoryIndex);
				return null;
			}
			GameObject surf = null;
			Region region = territories [territoryIndex].region;
			int cacheIndex = GetCacheIndexForTerritoryRegion (territoryIndex);
			// Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
			surfaces.TryGetValue (cacheIndex, out surf);

			Material coloredMat = overlay ? coloredMatOverlay : coloredMatGround;
			Material texturizedMat = overlay ? texturizedMatOverlay : texturizedMatGround;

			// Should the surface be recreated?
			Material surfMaterial;
			if (surf != null) {
				surfMaterial = surf.GetComponent<Renderer> ().sharedMaterial;
				if (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset ||
				    textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals (texturizedMat.name))) {
					surfaces.Remove (cacheIndex);
					DestroyImmediate (surf);
					surf = null;
				}
			}
			// If it exists, activate and check proper material, if not create surface
			bool isHighlighted = territoryHighlightedIndex == territoryIndex;
			if (surf != null) {
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
				surf = GenerateTerritoryRegionSurface (territoryIndex, surfMaterial, textureScale, textureOffset, textureRotation, rotateInLocalSpace);
				region.customMaterial = surfMaterial;
				region.customTextureOffset = textureOffset;
				region.customTextureRotation = textureRotation;
				region.customTextureScale = textureScale;
				region.customRotateInLocalSpace = rotateInLocalSpace;
			}
			// If it was highlighted, highlight it again
			if (region.customMaterial != null && isHighlighted) {
				if (region.customMaterial != null) {
					hudMatTerritory.mainTexture = region.customMaterial.mainTexture;
				} else {
					hudMatTerritory.mainTexture = null;
				}
				surf.GetComponent<Renderer> ().sharedMaterial = hudMatTerritory;
				_highlightedObj = surf;
			}
			return surf;
		}

		/// <summary>
		/// Specifies if a given cell is visible.
		/// </summary>
		public void TerritorySetBorderVisible (int territoryIndex, bool visible) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			territories [territoryIndex].borderVisible = visible;
		}


		/// <summary>
		/// Returns a list of neighbour cells for specificed cell index.
		/// </summary>
		public List<Territory> TerritoryGetNeighbours (int territoryIndex) {
			List<Territory> neighbours = new List<Territory> ();
			Region region = territories [territoryIndex].region;
			int nCount = region.neighbours.Count;
			for (int k = 0; k < nCount; k++) {
				neighbours.Add ((Territory)region.neighbours [k].entity);
			}
			return neighbours;
		}

		/// <summary>
		/// Returns a list of cells that form the frontiers of a given territory
		/// </summary>
		/// <returns>The number of cells found.</returns>
		/// <param name="territoryIndex">Territory index.</param>
		/// <param name="cellIndices">Cells that form the frontier. You need to pass an already initialized list, which will be cleared and filled with the cells.</param>
		public int TerritoryGetFrontierCells (int territoryIndex, ref List<int> cellIndices) {
			return TerritoryGetFrontierCells (territoryIndex, -1, ref cellIndices);
		}


		/// <summary>
		/// Returns a list of cells that forms the frontiers between a given territory and another one.
		/// </summary>
		/// <returns>The get frontier cells.</returns>
		/// <param name="territoryIndex">Territory index.</param>
		/// <param name="otherTerritoryIndex">Other territory index.</param>
		/// <param name="cellIndices">Cells that form the frontier. You need to pass an already initialized list, which will be cleared and filled with the cells.</param>
		public int TerritoryGetFrontierCells (int territoryIndex, int otherTerritoryIndex, ref List<int> cellIndices) {
			if (territoryIndex < 0 || territories == null || territoryIndex >= territories.Count || territories [territoryIndex].cells == null || cells == null)
				return 0;

			cellIndices.Clear ();
			foreach (KeyValuePair<Segment, Frontier> kv in territoryNeighbourHit) {
				Segment segment = kv.Key;
				Frontier frontier = kv.Value;
				if (frontier.region1 == null || frontier.region2 == null)
					continue;
				Cell cell1 = (Cell)frontier.region1.entity;
				Cell cell2 = (Cell)frontier.region2.entity;
				if (cell1.territoryIndex == territoryIndex && (otherTerritoryIndex < 0 || cell2.territoryIndex == otherTerritoryIndex)) {
					int cellIndex;
					if (cellLookup.TryGetValue (cell1, out cellIndex)) {
						if (!cellIndices.Contains (cellIndex)) {
							cellIndices.Add (cellIndex);
						}
					}
				} else if (cell2.territoryIndex == territoryIndex && (otherTerritoryIndex < 0 || cell1.territoryIndex == otherTerritoryIndex)) {
					int cellIndex;
					if (cellLookup.TryGetValue (cell2, out cellIndex)) {
						if (!cellIndices.Contains (cellIndex)) {
							cellIndices.Add (cellIndex);
						}
					}
				}
			}
			return cellIndices.Count;
		}

		/// <summary>
		/// Colors a territory and fades it out during "duration" in seconds.
		/// </summary>
		public void TerritoryFadeOut (int territoryIndex, Color color, float duration, int repetitions = 1) {
			TerritoryAnimate (FADER_STYLE.FadeOut, territoryIndex, color, duration, repetitions);
		}

		/// <summary>
		/// Flashes a territory with "color" and "duration" in seconds.
		/// </summary>
		public void TerritoryFlash (int territoryIndex, Color color, float duration, int repetitions = 1) {
			TerritoryAnimate (FADER_STYLE.Flash, territoryIndex, color, duration, repetitions);
		}

		/// <summary>
		/// Blinks a territory with "color" and "duration" in seconds.
		/// </summary>
		public void TerritoryBlink (int territoryIndex, Color color, float duration, int repetitions = 1) {
			TerritoryAnimate (FADER_STYLE.Blink, territoryIndex, color, duration, repetitions);
		}

		/// <summary>
		/// Temporarily colors a territory for "duration" in seconds.
		/// </summary>
		public void TerritoryColorTemp (int territoryIndex, Color color, float duration) {
			TerritoryAnimate (FADER_STYLE.ColorTemp, territoryIndex, color, duration, 1);
		}

		/// <summary>
		/// Cancels any ongoing visual effect on a territory
		/// </summary>
		/// <param name="cellIndex">Cell index.</param>
		public void TerritoryCancelAnimations (int territoryIndex) {
			TerritoryCancelAnimation (territoryIndex);
		}

		/// <summary>
		/// Specifies if a given territory is visible.
		/// </summary>
		public void TerritorySetVisible (int territoryIndex, bool visible) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			territories [territoryIndex].visible = visible;
			if (territoryIndex == _territoryLastOverIndex) {
				ClearLastOver ();
			}
		}

		/// <summary>
		/// Returns true if territory is visible
		/// </summary>
		public bool TerritoryIsVisible (int territoryIndex) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return false;
			return territories [territoryIndex].visible;
		}

		/// <summary>
		/// Specifies if a given territory is neutral.
		/// </summary>
		public void TerritorySetNeutral (int territoryIndex, bool neutral) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			territories [territoryIndex].neutral = neutral;
			needUpdateTerritories = true;
			issueRedraw = RedrawType.Full;
		}

		/// <summary>
		/// Returns true if territory is neutral
		/// </summary>
		public bool TerritoryIsNeutral (int territoryIndex) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return false;
			return territories [territoryIndex].neutral;
		}


		/// <summary>
		/// Specifies the color of the territory borders.
		/// </summary>
		public void TerritorySetFrontierColor (int territoryIndex, Color color) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return;
			Territory terr = territories [territoryIndex];
			if (terr.frontierColor != color) {
				terr.frontierColor = color;
				DrawTerritoryBorders ();
			}
		}



		/// <summary>
		/// Returns the territory object under position in local coordinates
		/// </summary>
		public Territory TerritoryGetAtPosition (Vector2 localPosition) {
			return GetTerritoryAtPoint (localPosition, false);
		}

		/// <summary>
		/// Returns the territory object under position in local or worldSpace coordinates
		/// </summary>
		public Territory TerritoryGetAtPosition (Vector3 position, bool worldSpace) {
			return GetTerritoryAtPoint (position, worldSpace);
		}

		/// <summary>
		/// Gets the territory's center position in world space.
		/// </summary>
		public Vector3 TerritoryGetPosition (int territoryIndex) {
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return Misc.Vector3zero;
			Vector2 territoryGridCenter = territories [territoryIndex].scaledCenter;
			return GetWorldSpacePosition (territoryGridCenter);
		}


		/// <summary>
		/// Returns the rect enclosing the territory in world space
		/// </summary>
		public Bounds TerritoryGetRectWorldSpace (int territoryIndex) {
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return new Bounds (Misc.Vector3zero, Misc.Vector3zero);
			Rect rect = territories [territoryIndex].region.rect2D;
			Vector3 min = GetWorldSpacePosition (rect.min);
			Vector3 max = GetWorldSpacePosition (rect.max);
			Bounds bounds = new Bounds ((min + max) * 0.5f, max - min);
			return bounds;
		}


		/// <summary>
		/// Returns the number of vertices of the territory
		/// </summary>
		public int TerritoryGetVertexCount (int territoryIndex) {
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return 0;
			return territories [territoryIndex].region.points.Count;
		}


		/// <summary>
		/// Returns the world space position of the vertex of a territory
		/// </summary>
		public Vector3 TerritoryGetVertexPosition (int territoryIndex, int vertexIndex) {
			if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
				return Misc.Vector3zero;
			Vector2 localPosition = territories [territoryIndex].region.points [vertexIndex];
			return GetWorldSpacePosition (localPosition);
		}


		/// <summary>
		/// Returns the shape/surface gameobject of the territory.
		/// </summary>
		/// <returns>The get game object.</returns>
		/// <param name="cellIndex">Cell index.</param>
		public GameObject TerritoryGetGameObject(int territoryIndex) {
			if (territoryIndex < 0 || territoryIndex >= territories.Count)
				return null;
			Territory territory = territories [territoryIndex];
			if (territory.region.surfaceGameObject != null)
				return territory.region.surfaceGameObject;
			GameObject go = TerritoryToggleRegionSurface (territoryIndex, true, Misc.ColorNull);
			TerritoryToggleRegionSurface (territoryIndex, false, Misc.ColorNull);
			return go;
		}



		/// <summary>
		/// Automatically generates territories based on the different colors included in the texture.
		/// </summary>
		/// <param name="neutral">This color won't generate any texture.</param>
		public void CreateTerritories (Texture2D texture, Color neutral) {

			if (texture == null || cells == null)
				return;

			List<Color> dsColors = new List<Color> ();
			int cellCount = cells.Count;
			Color[] colors = null;
			try {
				colors = texture.GetPixels ();
			} catch {
				Debug.Log ("Texture used to create territories is not readable. Check import settings.");
				return;
			}
			for (int k = 0; k < cellCount; k++) {
				if (!cells [k].visible)
					continue;
				Vector2 uv = cells [k].center;
				uv.x += 0.5f;
				uv.y += 0.5f;

				int x = (int)(uv.x * texture.width);
				int y = (int)(uv.y * texture.height);
				int pos = y * texture.width + x;
				if (pos < 0 || pos > colors.Length)
					continue;
				Color pixelColor = colors [pos];

				int territoryIndex = dsColors.IndexOf (pixelColor);
				if (territoryIndex < 0) {
					dsColors.Add (pixelColor);
					territoryIndex = dsColors.Count - 1;
				}
				CellSetTerritory (k, territoryIndex);
				if (territoryIndex >= MAX_TERRITORIES - 1)
					break;
			}
			if (dsColors.Count > 0) {
				_numTerritories = dsColors.Count;
				_showTerritories = true;

				if (territories == null) {
					territories = new List<Territory> (_numTerritories);
				} else {
					territories.Clear ();
				}
				for (int c = 0; c < _numTerritories; c++) {
					Territory territory = new Territory (c.ToString ());
					Color territoryColor = dsColors [c];
					if (territoryColor.r != neutral.r || territoryColor.g != neutral.g || territoryColor.b != neutral.b) {
						territory.fillColor = territoryColor;
					} else {
						territory.fillColor = new Color (0, 0, 0, 0);
						territory.visible = false;
					}
					territories.Add (territory);
				}
				isDirty = true;
				issueRedraw = RedrawType.Full;
				Redraw ();
			}
		}


		/// <summary>
		/// Escales the gameobject of a colored/textured surface
		/// </summary>
		/// <param name="surf">Surf.</param>
		/// <param name="cell">Cell.</param>
		/// <param name="scale">Scale.</param>
		public void TerritoryScaleSurface(int territoryIndex, float scale) {
			if (territoryIndex < 0)
				return;
			Territory territory = territories [territoryIndex];
			GameObject surf = territory.region.surfaceGameObject;
			ScaleSurface (surf, territory.center, scale);
		}


		public void ExportTerritoryMesh(int territoryIndex) {
			GameObject surf = TerritoryGetGameObject (territoryIndex);
			if (surf == null)
				return;
			MeshFilter mf = surf.GetComponent<MeshFilter> ();
			if (mf == null || mf.sharedMesh == null)
				return;

			Mesh mesh = mf.sharedMesh;
			if (mesh != null && (mesh.uv == null || mesh.uv.Length==0)) {
				// forces mesh to have UV mappings
				Material oldMat = territories [territoryIndex].region.customMaterial;
				Color color = oldMat != null ? oldMat.color : Misc.ColorNull;
				surf = TerritoryToggleRegionSurface (territoryIndex, true, color, true, Texture2D.whiteTexture);
				TerritoryToggleRegionSurface (territoryIndex, false, Misc.ColorNull);

				mf = surf.GetComponent<MeshFilter> ();
				if (mf == null || mf.sharedMesh == null)
					return;
			}

			mesh = Instantiate<Mesh> (mf.sharedMesh);
			mesh.name = "Territory " + territoryIndex;
			mesh.hideFlags = 0;

			GameObject newSurf = new GameObject("Copy of territory " + territoryIndex);
			newSurf.transform.position = transform.position;
			newSurf.transform.rotation = transform.rotation;
			newSurf.transform.localScale = transform.localScale;
			mf = newSurf.AddComponent<MeshFilter> ();
			mf.mesh = mesh;
			MeshRenderer mr = newSurf.AddComponent<MeshRenderer> ();
			mr.material = new Material(Shader.Find("Diffuse"));
		}





		#endregion


	
	}
}

