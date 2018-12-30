/* SimpleLOD 1.6     */
/* By Orbcreation BV */
/* Richard Knol      */
/* Mar 11, 2016      */

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using OrbCreationExtensions;


public class SimpleLOD_MaterialPopup : EditorWindow {
	private GameObject go;
	private Mesh mesh;
	private List<Hashtable> submeshRows;
	private Texture2D pixel;

	private Texture2D tempAtlas;

	private Mesh originalMesh;
	private Material[] originalMaterials;
	private bool isChanged = false;
	private bool hasChanges = false;
	private Vector2 scrollPos = Vector2.zero;
	private Vector2 dragFromPosition = Vector2.zero;
	private Vector2 dragToPosition = Vector2.zero;
	private Hashtable dragRow = null;
	private Hashtable dragAtlas = null;
	private int texReadableCount = 0;

	private string[] textureKeys = new string[] {"_MainTex", "_SpecGlossMap", "_SpecMap", "_GlossMap", "_MetallicGlossMap", "_MetallicMap", "_BumpMap", "_NormalMap", "_ParallaxMap", "_OcclusionMap", "_EmissionMap", "_GlowMap", "_DetailMask", "_DetailMap", "_DetailAlbedoMap", "_DetailNormalMap", "_DetailBumpMap"};

	public void OpenWindow(GameObject aGO) {
		go = aGO;
		pixel = new Texture2D(1, 1, TextureFormat.RGB24, false);
		pixel.SetPixels(new Color[1]);
		pixel.Apply(false, false);

		tempAtlas = new Texture2D(1, 1, TextureFormat.RGB24, false);
		tempAtlas.SetPixels(new Color[] {Color.blue});
		tempAtlas.Apply(false, false);

		MeshFilter filt = go.GetComponent<MeshFilter>();
		if(filt != null) {
			mesh = filt.sharedMesh;
		} else {
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if(smr != null) mesh = smr.sharedMesh;
		}
		if(mesh == null) {
			EditorUtility.DisplayDialog("Show atlas window", "This game object has no mesh.", "OK");
			Close();
			return;
		}

		Init();
		GetOriginals();
		if(!HasMultipleTextures()) {
			EditorUtility.DisplayDialog("Show atlas window", "You can only use this if the game object has more than 1 texture.", "OK");
			Close();
			return;
		}
		if(!mesh.CheckUvsWithin01Range()) {
			int uvAction = 0;  // 0 nothing, 1 clamp, 2 wrap
			Vector4 minMaxRange = mesh.GetUvRange();
			if(minMaxRange.x > -0.1f && minMaxRange.y > -0.1f && minMaxRange.z < 1.1f && minMaxRange.w < 1.1f) {
				uvAction = 1;
			} else if(EditorUtility.DisplayDialog("Show atlas window", "Some UV coordinates in your mesh are out of bounds and that will give problems when you create atlasses.\n\nSimpleLOD can fix this for the entire mesh, but it may cause some visible disruptions.\n\nYour other option is to only correct the UV's of submeshes that are used by materials with atlasses later on (happens automatically when the atlasses are saved).", "Fix it", "Correct submeshes later on")) {
				if(minMaxRange.x > -0.2f && minMaxRange.y > -0.2f && minMaxRange.z < 1.2f && minMaxRange.w < 1.2f) {
					uvAction = 1;
				} else {
					if(EditorUtility.DisplayDialog("Correct UV coordinates", "The UV coordinates range from ("+ minMaxRange.x + ", " + minMaxRange.y + ") to ("+ minMaxRange.z + ", " + minMaxRange.w + "), where they should be between 0 and 1.\nHow would you like to fix this?\n\n(Tip: use the revert button when you are not happy with the result)", "Clamp between 0 - 1", "Wrap around")) {
						uvAction = 1;
					} else {
						uvAction = 2;
					}
				}
			}
			if(uvAction > 0) {
				originalMesh = (Mesh)Mesh.Instantiate(mesh);
				isChanged = true;
				if(uvAction == 1) mesh.ClampUvs();
				else mesh.WrapUvs();
			}
		}

		if(submeshRows != null) {
			this.position = new Rect((Screen.width/2)+200, (Screen.height/2)+50, 850, 500);
			this.minSize = new Vector3(500,200);
			this.maxSize = new Vector3(850,1000);
			#if UNITY_4_3
				this.title = "Materials";
			#elif UNITY_4_4
				this.title = "Materials";
			#elif UNITY_4_5
				this.title = "Materials";
			#elif UNITY_4_6
				this.title = "Materials";
			#elif UNITY_5_0
				this.title = "Materials";
			#else
				this.titleContent = new GUIContent("Materials");
			#endif
			this.Show();
		} else {
			Close();
		}
	}

	private void GetOriginals() {
		originalMesh = mesh;
		MeshRenderer rend = go.GetComponent<MeshRenderer>();
		if(rend != null) originalMaterials = rend.sharedMaterials;
		else {
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if(smr != null) originalMaterials = smr.sharedMaterials;
		}
		isChanged = false;
	}
	private void RestoreOriginals() {
		mesh = originalMesh;
		MeshRenderer rend = go.GetComponent<MeshRenderer>();
		if(rend != null) {
			MeshFilter filt = go.GetComponent<MeshFilter>();
			if(filt != null) filt.sharedMesh = mesh;
			rend.sharedMaterials = originalMaterials;
		} else {
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if(smr != null) {
				smr.sharedMesh = mesh;
				smr.sharedMaterials = originalMaterials;
			}
		}
		isChanged = false;
		Init();
	}

	private void Init() {
		submeshRows = new List<Hashtable>();

		texReadableCount = 0;
		int order = 0;
		Material[] mats = null;


		MeshRenderer rend = go.GetComponent<MeshRenderer>();
		if(rend != null) mats = rend.sharedMaterials;
		else {
			SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
			if(smr != null) mats = smr.sharedMaterials;
		}
		if(mats != null) {
			int i=0;
			foreach(Material mat in mats) {
				for(i=0;i<submeshRows.Count;i++) {
					if((Material)submeshRows[i]["origMat"] == mat) break;
				}
				if(i >= submeshRows.Count) {
					Hashtable row = new Hashtable();
					row["origMat"] = mat;
					row["mat"] = mat;
					bool unreadable = IsAnyTextureUnreadable(mat, textureKeys);
					row["texUnReadable"] = unreadable;
					if(!unreadable) texReadableCount++;
					row["atlasMat"] = mat;
					row["atlasSize"] = 2;
					row["atlasMaxSize"] = 2048;
					row["atlasRect"] = new Rect(0,0,1,1);
					row["isAtlas"] = false;
					row["order"] = order;
					row["p0"] = (Texture2D)Texture2D.Instantiate(pixel);
					row["p1"] = (Texture2D)Texture2D.Instantiate(pixel);
					row["p2"] = (Texture2D)Texture2D.Instantiate(pixel);
					row["p3"] = (Texture2D)Texture2D.Instantiate(pixel);
					row["p4"] = (Texture2D)Texture2D.Instantiate(pixel);
					row["p5"] = (Texture2D)Texture2D.Instantiate(pixel);
					row["p6"] = (Texture2D)Texture2D.Instantiate(pixel);
					order += 2;
					if(mat.HasProperty("_BumpMap")) {
						Texture t = mat.GetTexture("_BumpMap");
						if(t != null && t.GetType() == typeof(Texture2D) && ((Texture2D)t).IsReadable()) {
							t = ((Texture2D)t).FromUnityNormalMap();
							row["origBumpMap"] = t;
						}
					} else if(mat.HasProperty("_NormalMap")) {
						Texture t = mat.GetTexture("_NormalMap");
						if(t != null && t.GetType() == typeof(Texture2D) && ((Texture2D)t).IsReadable()) {
							t = ((Texture2D)t).FromUnityNormalMap();
							row["origBumpMap"] = t;
						}
					}
					if(mat.HasProperty("_DetailBumpMap")) {
						Texture t = mat.GetTexture("_DetailBumpMap");
						if(t != null && t.GetType() == typeof(Texture2D) && ((Texture2D)t).IsReadable()) {
							t = ((Texture2D)t).FromUnityNormalMap();
							row["origDetailBumpMap"] = t;
						}
					} else if(mat.HasProperty("_DetailNormalMap")) {
						Texture t = mat.GetTexture("_DetailNormalMap");
						if(t != null && t.GetType() == typeof(Texture2D) && ((Texture2D)t).IsReadable()) {
							t = ((Texture2D)t).FromUnityNormalMap();
							row["origDetailBumpMap"] = t;
						}
					}
					submeshRows.Add(row);
				}
			}
		}
	}

	public new void Close() {
		go = null;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			DestroyImmediate((Texture2D)row["p0"]);
			DestroyImmediate((Texture2D)row["p1"]);
			DestroyImmediate((Texture2D)row["p2"]);
			DestroyImmediate((Texture2D)row["p3"]);
			DestroyImmediate((Texture2D)row["p4"]);
			DestroyImmediate((Texture2D)row["p5"]);
			DestroyImmediate((Texture2D)row["p6"]);
		}
		submeshRows.Clear();
		submeshRows = null;
		dragRow = null;
		DestroyImmediate(pixel);
		pixel = null;
		Resources.UnloadUnusedAssets();
		base.Close();
	}

	void OnGUI() {
		if(go == null) Close();
		GUIStyle windowTitleStyle = new GUIStyle(GUI.skin.label);
		windowTitleStyle.fontSize = 18;
		GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
		titleStyle.padding = new RectOffset(0,0,0,0);
		GUIStyle explanationStyle = new GUIStyle(GUI.skin.label);
		explanationStyle.alignment = TextAnchor.MiddleRight;
		explanationStyle.normal.textColor = new Color(0.5f,0.5f,0.5f);

		bool dragStarted = false;
		bool dragEnded = false;
		Event evn = Event.current;
		if(evn != null) {
			Vector2 cursorPos = evn.mousePosition;
			bool cursorInActiveRect = false;
			if(cursorPos.x < this.position.width - 20) cursorInActiveRect = true;
			cursorPos = cursorPos + scrollPos;
			if(dragRow == null && dragAtlas == null) {
				dragFromPosition = cursorPos;
				dragToPosition = cursorPos;
			}
			if(evn.type == EventType.MouseDown && evn.button == 0 && cursorInActiveRect) {
				dragFromPosition = cursorPos;
				dragStarted = true;
			}
			if(evn.type == EventType.MouseDrag && evn.button == 0) {
				dragToPosition = cursorPos;
				Repaint();
			}
			if((evn.type == EventType.MouseUp || evn.type == EventType.DragExited) && evn.button == 0 && (dragRow != null || dragAtlas != null)) {
				dragToPosition = cursorPos;
				dragEnded = true;
				Repaint();
			}
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
		float a = -70f;
		float x = 6f;
		float y = 100f;
		float w = 32f;
		float h = 200f;
		float m = 2f;

		ShowColumnTitle("Texture", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Spec map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Metallic map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Normal map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Parallax map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Occlusion map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Emission map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Detail mask", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Detail albedo map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Detail norm. map", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Main color", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Specular color", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Emission color", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Alpha cutoff", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Shininess", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Metallic", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Normal scale", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Parallax scale", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Occlusion scale", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Detail normal scale", a, ref x, y, w, h, m, titleStyle);
		float leftHandSide = x-4f;
		x+=m;
		ShowColumnTitle("Texture/atlas", a, ref x, y, w, h, m, titleStyle);
		ShowColumnTitle("Scale in atlas", a, ref x, y, w, h, m, titleStyle);

		h = 32f;
		w = 32f;
		y += m*3;
		x = m;

		float lx = m*2;
		float ly = y;
		float lw = 820 - (4 * m);
		float lh = h+20+m;
		GUILayoutUtility.GetRect(new GUIContent(" "), titleStyle, GUILayout.Width(lw), GUILayout.Height(((lh + m) * submeshRows.Count) + h + y));

		float dx = lx;
		float dy = ly;
		float gap = 0f;
		float extraMargin = lh * 0.6f;

		Hashtable dragOverRow = null;
		Hashtable dragIntoRow = null;
		Hashtable dragIntoAtlas = null;
		for(int order=0;order<submeshRows.Count*2;order+=2) {
			for(int i=0;i<submeshRows.Count;i++) {
				Hashtable row = (Hashtable)submeshRows[i];
				if((int)row["order"] == order) {
					float emptyGrey = 0.15f;

					if(dragStarted && dragRow == null && dragAtlas == null) {
						if(dragFromPosition.y >= ly && dragFromPosition.y <= ly + lh + m) {  // row under cursor
							if((!(bool)row["texUnReadable"])) { // is readable
								int j = 0;
								for(;j<submeshRows.Count;j++) {
									Hashtable row2 = (Hashtable)submeshRows[j];
									if(row2["mat"] == row["mat"] && row["mat"] == row["origMat"] && row2 != row) break;
								}
								if(j >= submeshRows.Count) { // has no other rows merged into this
									Texture atlas = null;
									Material atlasMat = (Material)row["atlasMat"];
									if(atlasMat.HasProperty("_MainTex")) atlas = atlasMat.GetTexture("_MainTex");
									if(atlas != null) { // has a texture
										if(dragFromPosition.x >= lx && dragFromPosition.x <= leftHandSide) {
											dragRow = row;
										} else if(dragFromPosition.x >= leftHandSide && dragFromPosition.x <= ly + lw) {
											if(row["mat"] == row["origMat"]) {
												dragAtlas = row;
											} else {
												dragRow = row;
											}
										}
									}
								}
							}
						}
					}
					if(dragRow != null && dragRow["atlasMat"] == dragRow["origMat"]) {
						if(dragToPosition.x >= lx && dragToPosition.x <= lx + lw && dragToPosition.y >= ly + extraMargin && dragToPosition.y <= ly + extraMargin + lh + m) {
							if(row != dragRow && (!(bool)row["texUnReadable"])) {
								if(order + 2 != (int)dragRow["order"] || row["mat"] == dragRow["mat"]) dragOverRow = row;
								if(dragToPosition.y <= ly + extraMargin + (lh / 2)) dragIntoRow = row;
							}
						}
					}
					if(dragAtlas != null && dragAtlas["atlasMat"] == dragAtlas["origMat"]) {
						if(dragToPosition.x >= leftHandSide && dragToPosition.x <= lx + lw && dragToPosition.y >= ly && dragToPosition.y <= ly + lh + m) {
							if(row != dragAtlas) {
								if(dragToPosition.y <= ly + extraMargin + (lh / 2) && (!(bool)row["texUnReadable"])) {
									Texture atlas = null;
									Material atlasMat = (Material)row["atlasMat"];
									if(atlasMat.HasProperty("_MainTex")) atlas = atlasMat.GetTexture("_MainTex");
									if(atlas != null) dragIntoAtlas = row;
								}
							}
						}
					}

					if(dragAtlas == row || dragRow == row) {
						Texture2D p5 = (Texture2D)row["p5"];
						p5.SetPixels(new Color[] {new Color(emptyGrey, emptyGrey, emptyGrey)});
						p5.Apply(false, false);
						GUI.DrawTexture(new Rect(lx, ly + gap, lw, lh), p5, ScaleMode.StretchToFill, false, 0f);
						dx = lx + Mathf.Clamp(dragToPosition.x - dragFromPosition.x, -100, 100);
						dy = Mathf.Clamp(ly + dragToPosition.y - dragFromPosition.y, 0, Mathf.Infinity);
					}
					if(dragIntoRow == row) {
						DrawRow(row, lx, ly, lw, lh * 1.6f, m, leftHandSide, 0.5f, titleStyle);
						DrawRowAtlas(row, lx, ly, lw, lh * 1.6f, m, leftHandSide, 0.5f, titleStyle, false);
						gap = lh * 0.6f;
					} else if(dragOverRow == row) {
						DrawRow(row, lx, ly, lw, lh, m, leftHandSide, 0.3f, titleStyle);
						DrawRowAtlas(row, lx, ly, lw, lh, m, leftHandSide, 0.3f, titleStyle, false);
						gap = lh * 0.6f;
						Texture2D p6 = (Texture2D)row["p6"];
						p6.SetPixels(new Color[] {new Color(emptyGrey, emptyGrey, emptyGrey)});
						p6.Apply(false, false);
						GUI.DrawTexture(new Rect(lx, ly + lh, lw, gap), p6, ScaleMode.StretchToFill, false, 0f);
					} else if(dragRow != row){

						DrawRow(row, lx, ly + gap, lw, lh, m, leftHandSide, 0.25f, titleStyle);
					}
					if(dragIntoAtlas == row) {
						DrawRowAtlas(row, lx, ly, lw, lh, m, leftHandSide, 0.5f, titleStyle, false);
					} else if(dragRow != row){
						DrawRowAtlas(row, lx, ly + gap, lw, lh, m, leftHandSide, 0.25f, titleStyle, false);
					}

					ly += lh + m;
				}
			}
		}
		if(dragRow != null) {
			bool draggedEnough = Vector2.Distance(dragFromPosition, dragToPosition) > 5f;
			GUI.color = new Color(1,1,1,0.7f);
			DrawRow(dragRow, dx, dy, lw, lh, m, leftHandSide, 0.4f, titleStyle);
			DrawRowAtlas(dragRow, dx, dy, lw, lh, m, leftHandSide, 0.4f, titleStyle, draggedEnough);
			GUI.color = Color.white;

			if(dragEnded) {
				if(draggedEnough) {
					bool reorder = true;
					// find highest row order that belongs to the same mat
					if(dragIntoRow == null) {
						Material mat = (Material)dragRow["atlasMat"];
						int order = 0;
						for(int j=0;j<submeshRows.Count;j++) {
							Hashtable row = (Hashtable)submeshRows[j];
							if((Material)row["atlasMat"] == mat) {
								order = Mathf.Max(order, (int)row["order"]);
							}
						}
						dragRow["order"] = order + 1;
						ReorderMaterials();
						reorder = false;
					}

					// disconnect
					Hashtable recalcAtlasRow = null;
					int remainingAtlasRows = 0;
					for(int i=0;i<submeshRows.Count;i++) {
						Hashtable row = (Hashtable)submeshRows[i];
						if(row["mat"] == dragRow["origMat"]) row["mat"] = row["origMat"];
						if(row["atlasMat"] == dragRow["atlasMat"] && dragRow != row) {
							recalcAtlasRow = row;
							remainingAtlasRows++;
						}
					}
					dragRow["mat"] = dragRow["origMat"]; 
					dragRow["atlasMat"] = dragRow["origMat"];
					if(recalcAtlasRow != null) {
						if(remainingAtlasRows>1) CreateAtlas(recalcAtlasRow);
						else recalcAtlasRow["atlasMat"] = recalcAtlasRow["origMat"];
					}
					CreateAtlas(dragRow);


					if(dragIntoRow != null)	{
						// connect
						dragRow["mat"] = dragIntoRow["mat"];
						dragRow["order"] = ((int)dragIntoRow["order"]) + 1;
						ReorderMaterials();

						if(dragIntoRow["atlasMat"] == dragIntoRow["origMat"]) { // doesnt have an atlas yet
							Material newAtlasMat = (Material)Material.Instantiate((Material)dragIntoRow["origMat"]);
							newAtlasMat.SetTexture("_MainTex", tempAtlas);
							dragIntoRow["atlasMat"] = newAtlasMat;
						}
						dragRow["atlasMat"] = dragIntoRow["atlasMat"];
						CreateAtlas(dragIntoRow);

					} else if(dragOverRow != null && reorder) {
						// reorder
						dragRow["order"] = ((int)dragOverRow["order"]) + 1;
						ReorderMaterials();
					}
				}
				dragRow = null;
				dragAtlas = null;
				hasChanges = HasAnyChanges();
			}
		}

		if(dragAtlas != null) {
			bool draggedEnough = Vector2.Distance(dragFromPosition, dragToPosition) > 5f;
			GUI.color = new Color(1,1,1,0.7f);
			DrawRowAtlas(dragAtlas, dx, dy, lw, lh, m, leftHandSide, 0.4f, titleStyle, draggedEnough);
			GUI.color = Color.white;

			if(dragEnded) {
				if(draggedEnough) {
					// find highest row order that belongs to the same mat
					Material mat = (Material)dragAtlas["atlasMat"];
					if(dragIntoAtlas != null) mat = (Material)dragIntoAtlas["atlasMat"];
					int order = 0;
					for(int j=0;j<submeshRows.Count;j++) {
						Hashtable row = (Hashtable)submeshRows[j];
						if((Material)row["atlasMat"] == mat) {
							order = Mathf.Max(order, (int)row["order"]);
						}
					}
					dragAtlas["order"] = order + 1;
					ReorderMaterials();

					// disconnect
					Hashtable recalcAtlasRow = null;
					int remainingAtlasRows = 0;
					for(int i=0;i<submeshRows.Count;i++) {
						Hashtable row = (Hashtable)submeshRows[i];
						if(row["atlasMat"] == dragAtlas["atlasMat"] && dragAtlas != row) {
							recalcAtlasRow = row;
							remainingAtlasRows++;
						}
					}
					dragAtlas["atlasMat"] = dragAtlas["origMat"]; 
					if(recalcAtlasRow != null) {
						if(remainingAtlasRows>1) CreateAtlas(recalcAtlasRow);
						else recalcAtlasRow["atlasMat"] = recalcAtlasRow["origMat"];
					}
					CreateAtlas(dragAtlas);

					// connect
					if(dragIntoAtlas != null)	{
						Material intoAtlasMat = null;
						if(dragIntoAtlas["atlasMat"] == dragIntoAtlas["origMat"]) { // doesnt have an atlas yet
							intoAtlasMat = (Material)Material.Instantiate((Material)dragIntoAtlas["origMat"]);
							intoAtlasMat.SetTexture("_MainTex", tempAtlas);
							dragIntoAtlas["atlasMat"] = intoAtlasMat;
						} else {
							intoAtlasMat = (Material)dragIntoAtlas["atlasMat"];
						}

						dragAtlas["atlasMat"] = intoAtlasMat;
						CreateAtlas(dragIntoAtlas);
					}
				}
				dragRow = null;
				dragAtlas = null;
				hasChanges = HasAnyChanges();
			}
		}
		if(submeshRows.Count < 2) {
			GUI.Label(new Rect(lx, ly, leftHandSide, 20), "You need at least 2 materials to combine anything");
		} if(texReadableCount < 2) {
			GUI.Label(new Rect(lx, ly, leftHandSide, 40), "You need to make your textures readable in order to include them in an atlas.\nYou can use the button \"Make readable\".");
		} else {
			GUI.Label(new Rect(lx, ly, leftHandSide, 40), "Drag & drop materials to create atlasses and merge the materials.\nDrag & drop the right columns (named \"texture/atlas\") to create atlasses only and leave the materials intact");
		}
		x = leftHandSide + m;
		w = (lw - leftHandSide);
		if(isChanged) {
			if(hasChanges) w = ((lw - leftHandSide) / 2) - m;
			if(GUI.Button(new Rect(x, ly, w, 20), "Revert")) {
				RestoreOriginals();
				hasChanges = HasAnyChanges();
			}
			x += w + m;
			if(hasChanges) {
				if(GUI.Button(new Rect(x, ly, w, 20), "Apply")) {
					ApplyChanges();
					hasChanges = HasAnyChanges();
				}
			}
		} else if(hasChanges) {
			if(GUI.Button(new Rect(x, ly, w, 20), "Apply changes")) {
				ApplyChanges();
				hasChanges = HasAnyChanges();
			}
		}

		EditorGUILayout.EndScrollView();
	}

	private void DrawRow(Hashtable row, float rx, float ry, float rw, float rh, float m, float leftHandSide, float bgGrey, GUIStyle titleStyle) {
		Material mat = (Material)row["origMat"];
		Material newMat = (Material)row["mat"];
		float y;
		float x;
		float w = 32f;
		float h = 32f;

		float alpha = GUI.color.a;
		GUI.color = new Color(1,1,1,alpha * alpha * alpha);
		Texture2D p0 = (Texture2D)row["p0"];
		p0.SetPixels(new Color[] {new Color(bgGrey, bgGrey, bgGrey)});
		p0.Apply(false, false);
		GUI.DrawTexture(new Rect(rx, ry, leftHandSide - rx, rh), p0, ScaleMode.StretchToFill, false, 0f);
		GUI.DrawTexture(new Rect(leftHandSide + m, ry, rw + rx - leftHandSide - m, rh), p0, ScaleMode.StretchToFill, false, 0f);
		GUI.color = new Color(1,1,1,alpha);

		y = ry;
		x = rx + m;
		if(mat != newMat) {
			x += 20f;
			ShowRowTitle(mat.name + " merged with " + newMat.name, ref x, y, 600, 16, m, titleStyle);
		} else {
			ShowRowTitle(mat.shader.name + ": " + mat.name, ref x, y, 600, 16, m, titleStyle);
		}
		y += 16f + m;
		x = rx + m;
		if(mat != newMat) x += 20f;
		DrawTexProperty(mat, new string[] {"_MainTex"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_SpecGlossMap", "_SpecMap", "_GlossMap"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_MetallicGlossMap", "_MetallicMap"}, ref x, y, w, h, m);
		if(row.ContainsKey("origBumpMap")) DrawTexProperty((Texture2D)row["origBumpMap"], ref x, y, w, h, m);
		else DrawTexProperty(mat, new string[] {"_BumpMap", "_NormalMap"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_ParallaxMap"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_OcclusionMap"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_EmissionMap", "_GlowMap"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_DetailMask", "_DetailMap"}, ref x, y, w, h, m);
		DrawTexProperty(mat, new string[] {"_DetailAlbedoMap"}, ref x, y, w, h, m);
		if(mat == newMat) {
			if(row.ContainsKey("origDetailBumpMap")) DrawTexProperty((Texture2D)row["origDetailBumpMap"], ref x, y, w, h, m);
			else DrawTexProperty(mat, new string[] {"_DetailNormalMap", "_DetailBumpMap"}, ref x, y, w, h, m);
		}

		if(mat != newMat) {
			x -= 20f;
			GUI.Label(new Rect(x,y,10 * (w + m),h), "Using parent properties", titleStyle);
			x += 10 * (w + m);
		} else {
			DrawColorProperty((Texture2D)row["p1"], mat, new string[] {"_Color"}, ref x, y, w, h, m);
			DrawColorProperty((Texture2D)row["p2"], mat, new string[] {"_SpecColor"}, ref x, y, w, h, m);
			DrawColorProperty((Texture2D)row["p3"], mat, new string[] {"_EmissionColor"}, ref x, y, w, h, m);

			DrawFloatProperty(mat, new string[] {"_Cutoff", "_CutOff"}, ref x, y, w, h, m);
			DrawFloatProperty(mat, new string[] {"_Shininess", "_Glossiness"}, ref x, y, w, h, m);
			DrawFloatProperty(mat, new string[] {"_Metallic"}, ref x, y, w, h, m);
			DrawFloatProperty(mat, new string[] {"_BumpScale", "_NormalScale"}, ref x, y, w, h, m);
			DrawFloatProperty(mat, new string[] {"_Parallax", "_ParallaxScale"}, ref x, y, w, h, m);
			DrawFloatProperty(mat, new string[] {"_OcclusionStrength", "_OcclusionScale", "_Occlusion"}, ref x, y, w, h, m);
			DrawFloatProperty(mat, new string[] {"_Shininess", "_Glossiness"}, ref x, y, w, h, m);
		}
	}

	private void DrawRowAtlas(Hashtable row, float rx, float ry, float rw, float rh, float m, float leftHandSide, float bgGrey, GUIStyle titleStyle, bool showDftTexture) {
		float y;
		float x;
		float w = 48f;
		float h = 48f;

		float alpha = GUI.color.a;
		GUI.color = new Color(1,1,1,alpha * alpha * alpha);
		Texture2D p4 = (Texture2D)row["p4"];
		p4.SetPixels(new Color[] {new Color(bgGrey, bgGrey, bgGrey)});
		p4.Apply(false, false);
		GUI.DrawTexture(new Rect(leftHandSide + m, ry, rw + rx - leftHandSide - m, rh), p4, ScaleMode.StretchToFill, false, 0f);
		GUI.color = new Color(1,1,1,alpha);

		y = ry + 3;
		x = leftHandSide + 5;

		Texture atlas = null;
		if(showDftTexture) {
			Material mat = (Material)row["mat"];
			if(mat.HasProperty("_MainTex")) atlas = mat.GetTexture("_MainTex");
		} else {
			Material atlasMat = (Material)row["atlasMat"];
			if(atlasMat.HasProperty("_MainTex")) atlas = atlasMat.GetTexture("_MainTex");
		}

		if(atlas != null && atlas.height > 0) {
			float texW = w * Mathf.Min(Mathf.Max(1f, (float)atlas.width/(float)atlas.height), 1.5f);
			GUI.DrawTexture(new Rect(x, y, texW, h), atlas, ScaleMode.ScaleToFit, false, (float)atlas.width/(float)atlas.height);
			if((bool)row["texUnReadable"]) {
				if(GUI.Button(new Rect(x-1, y+30, w*2, 18), "Make readable")) {
					MakeTexturesReadable(row);
				}
			}
			x += texW + m;
			if(row["atlasMat"] != row["origMat"] && (bool)row["isAtlas"] == true) {
				int firstAtlasRowOrder = 9999999;
				for(int i=0;i<submeshRows.Count;i++) {
					Hashtable r = submeshRows[i];
					if(row["atlasMat"] == r["atlasMat"]) {
						firstAtlasRowOrder = Mathf.Min(firstAtlasRowOrder, (int)r["order"]);
					}
				}
				int newValue = 0;
				if((int)row["order"] == firstAtlasRowOrder) {
					newValue = (int)row["atlasMaxSize"];
					newValue = EditorGUI.IntPopup(new Rect(x,y,48,20), newValue, new string[] {"512", "1024", "2048", "4096", "8192"}, new int[] {512, 1024, 2048, 4096, 8192});
					if(newValue != (int)row["atlasMaxSize"]) {
						dragRow = null;
						dragAtlas = null;
						row["atlasMaxSize"] = newValue;
						CreateAtlas(row);
					}
				}
				newValue = (int)row["atlasSize"];
				newValue = EditorGUI.IntPopup(new Rect(x,y+32,48,18), newValue, new string[] {"1:8", "1:4", "1:2", "1:1", "2:1"}, new int[] {16, 8, 4, 2, 1});
				if(newValue != (int)row["atlasSize"]) {
					dragRow = null;
					dragAtlas = null;
					row["atlasSize"] = newValue;
					CreateAtlas(row);
				}
			}
		}
	}

	private void ShowColumnTitle(string title, float angle, ref float x, float y, float w, float h, float m, GUIStyle style) {
		GUIUtility.RotateAroundPivot(angle, new Vector2(x+(w/4),y));
		GUI.Label(new Rect(x+(w/4),y,h,h), title, style);
		GUI.matrix = Matrix4x4.identity;
		x += w + m;
	}

	private void ShowRowTitle(string title, ref float x, float y, float w, float h, float m, GUIStyle style) {
		GUI.Label(new Rect(x,y,w,h), title, style);
		x += w + m;
	}

	private void DrawTexProperty(Material mat, string[] names, ref float x, float y, float w, float h, float m) {
		Texture tex = null;
		foreach(string name in names) {
			if(mat.HasProperty(name)) tex = mat.GetTexture(name);
			if(tex != null) {
				GUI.DrawTexture(new Rect(x, y, w, h), tex, ScaleMode.ScaleToFit, false, 1f);
				break;
			}
		}
		x += w + m;
	}
	private void DrawTexProperty(Texture2D tex, ref float x, float y, float w, float h, float m) {
		if(tex != null) {
			GUI.DrawTexture(new Rect(x, y, w, h), tex, ScaleMode.ScaleToFit, false, 1f);
		}
		x += w + m;
	}

	private void DrawColorProperty(Texture2D pixel, Material mat, string[] names, ref float x, float y, float w, float h, float m) {
		foreach(string name in names) {
			if(mat.HasProperty(name)) {
				Color[] pix = new Color[] {mat.GetColor(name)};
				pixel.SetPixels(pix);
				pixel.Apply(false, false);
				GUI.DrawTexture(new Rect(x, y, w, h), pixel, ScaleMode.ScaleToFit, false, 1f);
			}
		}
		x += w + m;
	}

	private void DrawFloatProperty(Material mat, string[] names, ref float x, float y, float w, float h, float m) {
		foreach(string name in names) {
			if(mat.HasProperty(name)) {
				GUI.Label(new Rect(x, y, w, h), mat.GetFloat(name).MakeString(2));
			}
		}
		x += w + m;
	}

	private void CreateAtlas(Hashtable aRow) {
		Material atlasMat = (Material)aRow["atlasMat"];
		Material origMat = (Material)aRow["origMat"];

		// find 1st row that uses this atlasMat
		// (1st row defines which keys are active)
		int minOrder = 999999;
		Hashtable masterRow = null;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			if((Material)row["atlasMat"] == atlasMat) {
				if(minOrder > (int)row["order"]) {
					masterRow = row;
					minOrder = (int)row["order"];
				}
			}
		}
		int maxAtlasSize = (int)masterRow["atlasMaxSize"];

		// find texture keys to use
		atlasMat = (Material)masterRow["atlasMat"];
		List<string> keys = new List<string>();
		string masterTexKey = null;
		Texture masterTex = null;
		foreach(string key in textureKeys) {
			if(atlasMat.HasProperty(key)) {
				keys.Add(key);
				if(masterTexKey == null || masterTex == null || masterTex.GetType() != typeof(Texture2D)) {
					masterTexKey = key;
					masterTex = atlasMat.GetTexture(key);
				}
			}
		}
		if(masterTex == null || masterTex.GetType() != typeof(Texture2D)) return;

		// find all rows that have the same base texture or same atlasMat as masterRow
		List<Hashtable> atlasRows = new List<Hashtable>();
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			Material m = (Material)row["atlasMat"];
			if(row["atlasMat"] == atlasMat || (m.HasProperty(masterTexKey) && m.GetTexture(masterTexKey) == masterTex)) {
				atlasRows.Add(row);
			}
		}

		// find base textures per material key
		List<Texture2D> texturesToInclude = new List<Texture2D>();
		for(int i=0;i<keys.Count;i++) {
			string key = keys[i];
			for(int j=0;j<atlasRows.Count;j++) {
				Material m = (Material)atlasRows[j]["origMat"];
				Texture tex = null;
				if(m.HasProperty(key)) tex = m.GetTexture(key);
				if(tex != null && tex.GetType() != typeof(Texture2D)) tex = null;
				texturesToInclude.Add((Texture2D)tex);
			}
		}

		if(texturesToInclude.Count <= keys.Count) { // Only 1 row included in atlas
			for(int j=0;j<atlasRows.Count;j++) {
				origMat = (Material)atlasRows[j]["origMat"];
				atlasMat = (Material)atlasRows[j]["atlasMat"];
				for(int i=0;i<keys.Count;i++) {
					string key = keys[i];
					Texture tex = null;
					if(origMat.HasProperty(key)) tex = origMat.GetTexture(key);
					if(atlasMat.HasProperty(key)) atlasMat.SetTexture(key, tex);
				}
			}
		} else {  // make an atlas per key

			string key = keys[0];
			List<Texture2D> tList = new List<Texture2D>();
			List<int> rowIndexes = new List<int>();
			int nrOfUniqueTextures = 0;
			for(int j=0;j<atlasRows.Count;j++) {
				Texture2D t = texturesToInclude[j];
				if(t != null) {
					float atlasSize  = (float)((int)atlasRows[j]["atlasSize"]) / 2f;
					float scale = 1f / atlasSize;
					if(scale != 1f) t = t.ScaledCopy((int)(t.width * scale), (int)(t.height * scale), false);
					int k=0;
					for(;k<tList.Count;k++) {
						if(tList[k] == t) break;
					}
					if(k>=tList.Count) nrOfUniqueTextures++;
					tList.Add(t);
					rowIndexes.Add(j);
				}
			}

			Rect[] rects = null;
			Texture2D atlas = null;

			if(nrOfUniqueTextures == 1) {
				for(int j=0;j<atlasRows.Count;j++) {
					origMat = (Material)atlasRows[j]["origMat"];
					atlasMat = (Material)atlasRows[j]["atlasMat"];
					for(int i=0;i<keys.Count;i++) {
						key = keys[i];
						Texture tex = null;
						if(origMat.HasProperty(key)) tex = origMat.GetTexture(key);
						if(atlasMat.HasProperty(key)) atlasMat.SetTexture(key, tex);
					}
					atlasRows[j]["isAtlas"] = false;
				}
			} else if(tList.Count>1) {
				atlas = new Texture2D(512, 512);
				rects = atlas.PackTextures(tList.ToArray(), 0, maxAtlasSize);
				// masterRow["atlasMaxSize"] = Mathf.Max(atlas.width, atlas.height);
				for(int j=0;j<rowIndexes.Count;j++) {  // clear current texture
					Hashtable row = atlasRows[rowIndexes[j]];
					atlasMat = (Material)row["atlasMat"];
					row["atlasRect"] = new Rect(0,0,1,1);
					if(atlasMat.HasProperty(key)) atlasMat.SetTexture(key, null);
				}
				for(int j=0;j<rowIndexes.Count;j++) {  // set atlas texture
					Hashtable row = atlasRows[rowIndexes[j]];
					atlasMat = (Material)row["atlasMat"];
					row["atlasRect"] = rects[j];
					if(atlasMat.HasProperty(key)) atlasMat.SetTexture(key, atlas);
					row["isAtlas"] = true;
				}

				for(int i=1;i<keys.Count;i++) {
					key = keys[i];
					Texture2D subAtlas = new Texture2D(atlas.width, atlas.height, TextureFormat.RGBA32, true);
					subAtlas.Fill(new Color(0,0,0,0));
					for(int j=0;j<rowIndexes.Count;j++) {  // clear current texture
						Hashtable row = atlasRows[rowIndexes[j]];
						Rect atlasRect = (Rect)row["atlasRect"];
						origMat = (Material)row["origMat"];
						if(origMat.HasProperty(key) && atlasMat.HasProperty(key)) {
							Texture t = origMat.GetTexture(key);
							if(t != null && t.GetType() == typeof(Texture2D)) {
								if(key == "_BumpMap" || key == "_NormalMap" || key == "_DetailBumpMap" || key == "_DetailNormalMap") {
									t = ((Texture2D)t).FromUnityNormalMap();
								}
								t = ((Texture2D)t).ScaledCopy(Mathf.RoundToInt(atlas.width * atlasRect.width), Mathf.RoundToInt(atlas.height * atlasRect.height), false);
								subAtlas.CopyFrom((Texture2D)t, Mathf.RoundToInt(atlas.width * atlasRect.x), Mathf.RoundToInt(atlas.height * atlasRect.y), 0, 0, t.width, t.height);
								atlasMat.SetTexture(key, subAtlas);
							}
						}
					}
				}
			}
		}
		Resources.UnloadUnusedAssets();
	}

	private void MakeTexturesReadable(Hashtable row) {
		Material mat = (Material)row["origMat"];
		bool unreadable = IsAnyTextureUnreadable(mat, textureKeys);
		if(unreadable) {
			foreach(string key in textureKeys) {
				if(mat.HasProperty(key)) {
					Texture tex = mat.GetTexture(key);
					if(tex != null && tex.GetType() == typeof(Texture2D)) {
						Texture2D readableTex = MakeReadable((Texture2D)tex);
						ReplaceTextureInAllMaterialsInAllRows(tex, readableTex);
					}
				}
			}
			unreadable = IsAnyTextureUnreadable(mat, textureKeys);
			if(!unreadable) texReadableCount++;
		}
		row["texUnReadable"] = unreadable;
	}

	private void ReplaceTextureInAllMaterialsInAllRows(Texture oldTex, Texture2D newTex) {
		foreach(string key in textureKeys) {
			for(int i=0;i<submeshRows.Count;i++) {
				Hashtable row = (Hashtable)submeshRows[i];
				Material mat = (Material)row["origMat"];
				if(mat.HasProperty(key) && mat.GetTexture(key) == oldTex) {
					mat.SetTexture(key, newTex);
					if(key == "_BumpMap" || key == "_NormalMap") {
						Texture2D nTex = newTex.FromUnityNormalMap();
						row["origBumpMap"] = nTex;
					} else if(key == "_DetailBumpMap" || key == "_DetailNormalMap") {
						Texture2D nTex = newTex.FromUnityNormalMap();
						row["origDetailBumpMap"] = nTex;
					}
				}
				bool unreadable = IsAnyTextureUnreadable(mat, textureKeys);
				row["texUnReadable"] = unreadable;

				mat = (Material)row["mat"];
				if(mat.HasProperty(key) && mat.GetTexture(key) == oldTex) {
					mat.SetTexture(key, newTex);
				}
				mat = (Material)row["atlasMat"];
				if(mat.HasProperty(key) && mat.GetTexture(key) == oldTex) {
					mat.SetTexture(key, newTex);
				}
			}
		}
		CheckReadability();
	}

	private void CheckReadability() {
		texReadableCount = 0;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			Material mat = (Material)row["origMat"];
			bool unreadable = IsAnyTextureUnreadable(mat, textureKeys);
			row["texUnReadable"] = unreadable;
			if(!unreadable) texReadableCount++;
		}
	}

	private bool HasAnyChanges() {
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			if(row["origMat"] != row["mat"]) return true;
			if(row["origMat"] != row["atlasMat"]) return true;
		}
		return false;
	}

	private bool HasMultipleTextures() {
		int texCount = 0;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			Material mat = (Material)row["origMat"];
			bool hasTex = false;
			if(mat != null) {
				foreach(string key in textureKeys) {
					Texture tex = null;
					if(mat.HasProperty(key)) tex = mat.GetTexture(key);
					if(tex != null && tex.GetType() == typeof(Texture2D)) hasTex = true;
				}
			}
			if(hasTex) texCount++;
		}
		return (texCount > 1);
	}

	private bool IsAnyTextureUnreadable(Material mat, string[] names) {
		if(mat == null) return false;
		foreach(string name in names) {
			Texture tex = null;
			if(mat.HasProperty(name)) tex = mat.GetTexture(name);
			if(tex != null) {
				if(tex.GetType() != typeof(Texture2D)) return true;
				if(!((Texture2D)tex).IsReadable()) return true;
			}
		}
		return false;
	}

	private Texture2D MakeReadable(Texture2D tex) {
		string assetPath = AssetDatabase.GetAssetPath(tex);
		if(assetPath != null && assetPath.Length>0) {
		    TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(assetPath);
		    if(!textureImporter.isReadable) {
			    textureImporter.isReadable = true;
				AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
			    tex = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetPath);
		    }
		}
		return tex;
	}

	private void ReorderMaterials() {
		int order = 0;
		for(int i=0;i<=submeshRows.Count*2;i++) {
			for(int j=0;j<submeshRows.Count;j++) {
				Hashtable row = (Hashtable)submeshRows[j];
				if((int)row["order"] == i) {
					row["newOrder"] = order;
					order+=2; 
				}
			}
		}
		for(int j=0;j<submeshRows.Count;j++) {
			((Hashtable)submeshRows[j])["order"] = ((Hashtable)submeshRows[j])["newOrder"];
		}
	}

	private void ApplyChanges() {

		// possibilities:
		//	origMat == mat && origMat != atlasMat  - use our own material & use atlas
		//	origMat != mat && mat == atlasMat - not possible
		//	origMat != mat && mat != atlasMat  - use parent material & use atlas

		// create new submeshRows
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = submeshRows[i];
			if(!row.ContainsKey("materialCreated")) {
				Material mat = (Material)row["mat"];
				Material newMat = (Material)Material.Instantiate(mat);
				AssetDatabase.CreateAsset(newMat, AssetDatabase.GenerateUniqueAssetPath(MaterialPath((Material)row["origMat"], "AtlasMaterials") + "/" + newMat.name + ".mat"));
				for(int j=0;j<submeshRows.Count;j++) {
					Hashtable r = (Hashtable)submeshRows[j];
					if((Material)r["mat"] == mat) {
						r["mat"] = newMat;
						r["materialCreated"] = true;
					}
				}
			}
		}

		// save atlas textures
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = submeshRows[i];
			if(row["atlasMat"] != row["origMat"]) {
				Material atlasMat = (Material)row["atlasMat"];

				// Find 1st row that uses the atlas
				int minOrder = 999999;
				Hashtable masterRow = null;
				for(int j=0;j<submeshRows.Count;j++) {
					Hashtable r = (Hashtable)submeshRows[j];
					if((Material)r["atlasMat"] == atlasMat) {
						if(minOrder > (int)r["order"]) {
							masterRow = r;
							minOrder = (int)r["order"];
						}
					}
				}

				if(!masterRow.ContainsKey("atlasSaved")) {
					foreach(string key in textureKeys) {
						if(atlasMat.HasProperty(key)) {
							Texture atlasTex = atlasMat.GetTexture(key);
							if(atlasTex != null && atlasTex.GetType() == typeof(Texture2D)) {
								string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7); 
								string localPath = AssetDatabase.GenerateUniqueAssetPath(AtlasPath((Texture2D)atlasTex, "AtlasTextures", key.Substring(1, key.Length-1)) + "/" + ((Material)masterRow["origMat"]).name+".png");
								((Texture2D)atlasTex).MakeFormatWritable();
								System.IO.File.WriteAllBytes(projectPath+"/"+localPath, ((Texture2D)atlasTex).EncodeToPNG());
								AssetDatabase.Refresh();

                                TextureImporter tempImporter = (TextureImporter)TextureImporter.GetAtPath(localPath);
								if(key == "_BumpMap" || key == "_DetailBumpMap") tempImporter.normalmap = true;
                                tempImporter.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(atlasTex.width, atlasTex.height));
                                AssetDatabase.ImportAsset(localPath);
                                atlasTex = (Texture2D)AssetDatabase.LoadAssetAtPath(localPath, typeof(Texture2D));

								for(int j=0;j<submeshRows.Count;j++) {
									Hashtable r = (Hashtable)submeshRows[j];
									if((Material)r["atlasMat"] == atlasMat) {
										Material mat = (Material)r["mat"];
										if(mat.HasProperty(key)) mat.SetTexture(key, atlasTex);
									}
								}
							}
						}
					}
					for(int j=0;j<submeshRows.Count;j++) {
						Hashtable r = (Hashtable)submeshRows[j];
						if((Material)r["atlasMat"] == atlasMat) {
							r["atlasSaved"] = true;
						}
					}
				}
			}
		}

		// build list of unique new submeshRows
		List<Material> newMaterials = new List<Material>();
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = submeshRows[i];
			Material mat = (Material)row["mat"];
			int j = 0;
			for(;j<newMaterials.Count;j++) {
				if(newMaterials[j] == mat) break;
			}
			if(j >= newMaterials.Count) {
				newMaterials.Add(mat);
			}
		}

		// update uv's
		bool meshCopied = false;
		bool hasUvChanges = false;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = submeshRows[i];
			Rect atlasRect = (Rect)row["atlasRect"];
			if(atlasRect.x != 0f || atlasRect.y != 0f || atlasRect.width != 1f || atlasRect.height != 1f) {
				hasUvChanges = true;
			}
		}
		if(hasUvChanges) {
			if(!meshCopied) {
				mesh = (Mesh)Mesh.Instantiate(mesh); // get a copy
				meshCopied = true;
			}
			for(int i=0;i<submeshRows.Count;i++) {
				Hashtable row = submeshRows[i];
				Rect atlasRect = (Rect)row["atlasRect"];
				if(atlasRect.x != 0f || atlasRect.y != 0f || atlasRect.width != 1f || atlasRect.height != 1f) {
					mesh.SetAtlasRectForSubmesh(atlasRect, i);
				}
			}			
		}

		// merge submeshes
		for(int i=submeshRows.Count-1;i>=0;i--) {
			Hashtable row = submeshRows[i];
			Material mat = (Material)row["mat"];

			// Find 1st row that uses the atlas
			int minOrder = 999999;
			int masterIndex = -1;
			for(int j=0;j<submeshRows.Count;j++) {
				Hashtable r = (Hashtable)submeshRows[j];
				if((Material)r["mat"] == mat) {
					if(minOrder > (int)r["order"]) {
						masterIndex = j;
						minOrder = (int)r["order"];
					}
				}
			}
			if(masterIndex >= 0 && masterIndex != i) {
				if(!meshCopied) {
					mesh = (Mesh)Mesh.Instantiate(mesh); // get a copy
					meshCopied = true;
				}
				mesh.MergeSubmeshInto(i, masterIndex);
				submeshRows.RemoveAt(i);
			}
		}

		if(meshCopied) {
			isChanged = true;
			// save mesh
			string meshPath = MeshPath(mesh, "AtlasMeshes");
			AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(meshPath + "/" + go.name + ".asset"));

			// use mesh
			MeshFilter filt = go.GetComponent<MeshFilter>();
			if(filt != null) {
				filt.sharedMesh = mesh;
				MeshRenderer rend = go.GetComponent<MeshRenderer>();
				if(rend != null) rend.sharedMaterials = newMaterials.ToArray();
			} else {
				SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
				if(smr != null) {
					smr.sharedMesh = mesh;
					smr.sharedMaterials = newMaterials.ToArray();
				}
			}
		}
		// refresh window
		Init();
		Resources.UnloadUnusedAssets();
	}

	private string AtlasPath(Texture2D tex, string subPath1, string subPath2) {
		if(tex != null) {
			string[] pathSegments = new string[3] {"Assets", "SimpleLOD", "WillBeIgnored"};
			string assetPath = AssetDatabase.GetAssetPath(tex);
			if(assetPath != null && assetPath.Length > 0) pathSegments = assetPath.Split(new Char[] {'/'});
			if(pathSegments.Length > 0) {
				string path = "";
				for(int i=0;i<pathSegments.Length-1;i++) {
					if(pathSegments[i] != "MergedMeshes" && pathSegments[i] != "CleanedMeshes" && pathSegments[i] != "LODMeshes" && pathSegments[i] != "SimplifiedMeshes" && pathSegments[i] != "AtlasTextures" && pathSegments[i] != "AtlasMaterials" && pathSegments[i] != "AtlasMeshes") {
						if(i>0 && (!Directory.Exists(path + "/" + pathSegments[i]))) AssetDatabase.CreateFolder(path, pathSegments[i]);
						if(i>0) path = path + "/";
						path = path + pathSegments[i];
					}
				}
				if(!Directory.Exists(path + "/" + subPath1)) AssetDatabase.CreateFolder(path, subPath1);
				path = path + "/" + subPath1;
				if(!Directory.Exists(path + "/" + subPath2)) AssetDatabase.CreateFolder(path, subPath2);
				return path + "/" + subPath2;
			}
		}
		return null;
	}
	private string MaterialPath(Material mat, string subPath) {
		if(mat != null) {
			string[] pathSegments = new string[3] {"Assets", "SimpleLOD", "WillBeIgnored"};
			string assetPath = AssetDatabase.GetAssetPath(mat);
			if(assetPath != null && assetPath.Length > 0) pathSegments = assetPath.Split(new Char[] {'/'});
			if(pathSegments.Length > 0) {
				string path = "";
				for(int i=0;i<pathSegments.Length-1;i++) {
					if(pathSegments[i] != "MergedMeshes" && pathSegments[i] != "CleanedMeshes" && pathSegments[i] != "LODMeshes" && pathSegments[i] != "SimplifiedMeshes" && pathSegments[i] != "AtlasTextures" && pathSegments[i] != "AtlasMaterials" && pathSegments[i] != "AtlasMeshes") {
						if(i>0 && (!Directory.Exists(path + "/" + pathSegments[i]))) AssetDatabase.CreateFolder(path, pathSegments[i]);
						if(i>0) path = path + "/";
						path = path + pathSegments[i];
					}
				}
				if(!Directory.Exists(path + "/" + subPath)) AssetDatabase.CreateFolder(path, subPath);
				return path + "/" + subPath;
			}
		}
		return null;
	}
	private string MeshPath(Mesh aMesh, string subPath) {
		if(aMesh != null) {
			string[] pathSegments = new string[3] {"Assets", "SimpleLOD", "WillBeIgnored"};
			string assetPath = AssetDatabase.GetAssetPath(aMesh);
			if(assetPath != null && assetPath.Length > 0) pathSegments = assetPath.Split(new Char[] {'/'});
			if(pathSegments.Length > 0) {
				string path = "";
				for(int i=0;i<pathSegments.Length-1;i++) {
					if(pathSegments[i] != "MergedMeshes" && pathSegments[i] != "CleanedMeshes" && pathSegments[i] != "LODMeshes" && pathSegments[i] != "SimplifiedMeshes" && pathSegments[i] != "AtlasTextures" && pathSegments[i] != "AtlasMaterials" && pathSegments[i] != "AtlasMeshes") {
						if(i>0 && (!Directory.Exists(path + "/" + pathSegments[i]))) AssetDatabase.CreateFolder(path, pathSegments[i]);
						if(i>0) path = path + "/";
						path = path + pathSegments[i];
					}
				}
				if(!Directory.Exists(path + "/" + subPath)) AssetDatabase.CreateFolder(path, subPath);
				return path + "/" + subPath;
			}
		}
		return null;
	}

}