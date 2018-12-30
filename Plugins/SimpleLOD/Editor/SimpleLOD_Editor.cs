/* SimpleLOD 1.6     */
/* By Orbcreation BV */
/* Richard Knol      */
/* Mar 11, 2016      */

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using OrbCreationExtensions;


public class SimpleLOD_Editor : MonoBehaviour {

	[MenuItem ("Tools/SimpleLOD")]
	static void SimpleLOD () {
		SimpleLOD_EditorPopup window = (SimpleLOD_EditorPopup)EditorWindow.GetWindow(typeof (SimpleLOD_EditorPopup));
		window.OpenWindow();
    }
	[MenuItem ("Tools/SimpleLOD", true, 0)]
	static bool ValidateSimpleLOD() {
	    return Selection.transforms.Length > 0;
	}
}

public class SimpleLOD_EditorPopup : EditorWindow {

	private Vector2 scrollPos = Vector2.zero;
	private GameObject go;
	private Transform[] selectedTransforms;
	private string goName = "";
	private int nrOfLevels = 3;
	private float[] compression = new float[5] {0.25f, 0.5f, 1f, 1.5f, 2f};
	private float smallObjectsValue = 1f;
	private int nrOfSteps = 1;
	private float protectNormals = 1f;
	private float protectUvs = 1f;
	private float protectBigTriangles = 1f;
	private float boostCompression = 1f;
	private float protectSubMeshesAndSharpEdges = 1f;
	private int useValueForNrOfSteps = 1;
	private float useValueForProtectNormals = 1f;
	private float useValueForProtectUvs = 1f;
	private float useValueForProtectBigTriangles = 1f;
	private float useValueForBoostCompression = 1f;
	private float useValueForProtectSubMeshesAndSharpEdges = 1f;
	private int showSettings = 0;
	private bool recalcNormals = true;
	private SimpleLOD_MergePopup  mergePopup = null;
	private SimpleLOD_RemovePopup  removePopup = null;
	private SimpleLOD_MaterialPopup  materialPopup = null;
	private SimpleLOD_RemoveHiddenPopup  removeHiddenPopup = null;

	private bool settingsCollapsed = true;
	private string toolTip = "";
	private int toolTipClearCount = 0;

	Func<GameObject, string> batchFunction = null;
	int batchIndex = 0;

	public void OpenWindow() {
		float value = PlayerPrefs.GetFloat("SimpleLOD_compression_0");
		if(value > 0f) compression[0] = value;
		value = PlayerPrefs.GetFloat("SimpleLOD_compression_1");
		if(value > 0f) compression[1] = value;
		value = PlayerPrefs.GetFloat("SimpleLOD_compression_2");
		if(value > 0f) compression[2] = value;
		value = PlayerPrefs.GetFloat("SimpleLOD_compression_3");
		if(value > 0f) compression[3] = value;
		value = PlayerPrefs.GetFloat("SimpleLOD_compression_4");
		if(value > 0f) compression[4] = value;
		value = PlayerPrefs.GetFloat("SimpleLOD_smallObjectsValue");
		if(value >= 10f) smallObjectsValue = value - 10f;
		value = PlayerPrefs.GetFloat("SimpleLOD_protectNormals");
		if(value >= 10f) protectNormals = value - 10f;
		value = PlayerPrefs.GetFloat("SimpleLOD_protectUvs");
		if(value >= 10f) protectUvs = value - 10f;
		value = PlayerPrefs.GetFloat("SimpleLOD_protectBigTriangles");
		if(value >= 10f) protectBigTriangles = value - 10f;
		value = PlayerPrefs.GetFloat("SimpleLOD_boostCompression");
		if(value >= 1f) boostCompression = value;
		value = PlayerPrefs.GetFloat("SimpleLOD_protectSubMeshesAndSharpEdges");
		if(value >= 10f) protectSubMeshesAndSharpEdges = value - 10f;
		nrOfSteps = PlayerPrefs.GetInt("SimpleLOD_nr_of_steps");
		if(nrOfSteps < 1) nrOfSteps = 1;
		nrOfLevels = PlayerPrefs.GetInt("SimpleLOD_nr_of_levels");
		if(nrOfLevels < 1) nrOfLevels = 3;
		int intValue = PlayerPrefs.GetInt("SimpleLOD_recalc_norm");
		recalcNormals = (intValue > 0);

		this.position = new Rect(Screen.width/2,Screen.height/2, 390, 470);
		this.minSize = new Vector3(390,250);
		this.maxSize = new Vector3(390,650);
		#if UNITY_4_3
			this.title = "SimpleLOD 1.6";
		#elif UNITY_4_4
			this.title = "SimpleLOD 1.6";
		#elif UNITY_4_5
			this.title = "SimpleLOD 1.6";
		#elif UNITY_4_6
			this.title = "SimpleLOD 1.6";
		#elif UNITY_5_0
			this.title = "SimpleLOD 1.6";
		#else
			this.titleContent = new GUIContent("SimpleLOD 1.6");
		#endif
		this.Show();
		go = Selection.activeGameObject;
		MakeBackup(go, true);
	}

	public void CloseWindow() {
		CloseSubwindows();
		Resources.UnloadUnusedAssets();
		this.Close();
	}
	public void CloseSubwindows() {
		if(mergePopup != null) {
			mergePopup.Close();
			mergePopup = null;
		}
		if(removePopup != null) {
			removePopup.Close();
			removePopup = null;
		}
		if(materialPopup != null) {
			materialPopup.Close();
			materialPopup = null;
		}
		if(removeHiddenPopup != null) {
			removeHiddenPopup.Close();
			removeHiddenPopup = null;
		}
	}
	public void ShowMergePopup() {
		MakeBackup(go, true);
		CloseSubwindows();
		mergePopup = (SimpleLOD_MergePopup)EditorWindow.GetWindow(typeof (SimpleLOD_MergePopup));
		mergePopup.OpenWindow(go);
	}
	public void ShowRemovePopup() {
		MakeBackup(go, true);
		CloseSubwindows();
		removePopup = (SimpleLOD_RemovePopup)EditorWindow.GetWindow(typeof (SimpleLOD_RemovePopup));
		removePopup.OpenWindow(go);
	}
	public void ShowMaterialPopup() {
		MakeBackup(go, true);
		CloseSubwindows();
		materialPopup = (SimpleLOD_MaterialPopup)EditorWindow.GetWindow(typeof (SimpleLOD_MaterialPopup));
		materialPopup.OpenWindow(go);
	}
	public void ShowRemoveHiddenPopup() {
		MakeBackup(go, true);
		CloseSubwindows();
		removeHiddenPopup = (SimpleLOD_RemoveHiddenPopup)EditorWindow.GetWindow(typeof (SimpleLOD_RemoveHiddenPopup));
		removeHiddenPopup.OpenWindow(go);
	}

	void Update() {
		if(selectedTransforms != null && selectedTransforms.Length > 0 && batchFunction != null) {
			if(batchIndex < selectedTransforms.Length) {
				GameObject nextGo = selectedTransforms[batchIndex].gameObject;
				if(nextGo.activeSelf) {
					Debug.Log("Processing " + nextGo.name + "...");
					Debug.Log(batchFunction(nextGo));
				}
				batchIndex++;
			}
		}


		GameObject newGo = null;
		selectedTransforms = Selection.transforms;
		newGo = Selection.activeGameObject;
		if(newGo == go && go != null) {  // same object still selected
			// make new backup when gameObject name changes
			if(goName != go.name && GetBackup(go) == null) MakeBackup(go, true);
			goName = go.name;
		} else if(newGo != null && newGo.activeSelf && newGo.FindParentWithName("_SimpleLOD_backups_delete_when_ready") == null) {
			CloseSubwindows();
			go = newGo;
			goName = go.name;
			if(go != null && GetBackup(go) == null) MakeBackup(go, true);

			// test if there is mesh somewhere in any of the children
			MeshFilter mf = go.GetComponentInChildren<MeshFilter>();
			if(mf == null) {
				SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
				if(smr == null) {
					CloseSubwindows();
					go = null;
					goName = "";
				}
			}
		} else {
			CloseSubwindows();
			go = null;
			goName = "";
		}
	}

	void OnGUI() {
		GUIStyle windowTitleStyle = new GUIStyle(GUI.skin.label);
		windowTitleStyle.fontSize = 15;
		GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
		titleStyle.fontStyle = FontStyle.Bold;
		GUIStyle toolTipStyle = new GUIStyle(GUI.skin.box);
		toolTipStyle.alignment = TextAnchor.UpperLeft;
		GUIStyle mainButtonStyle = new GUIStyle(GUI.skin.button);
		toolTipStyle.alignment = TextAnchor.UpperLeft;
		if(mainButtonStyle.normal.textColor.r > 0.5) {  // using light skin
			mainButtonStyle.normal.textColor = new Color(1f,0.8f,0.3f);
			toolTipStyle.normal.textColor = new Color(1f,0.8f,0.3f);
		} else {  // using dark skin
			mainButtonStyle.normal.textColor = new Color(0f,0f,0.5f);
			toolTipStyle.normal.textColor = new Color(0f,0f,0.5f);
		}

		if(selectedTransforms == null || (selectedTransforms.Length <= 1 && go == null)) {
			GUILayout.Label("\nSelect an active gameObject");
			return;
		}

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));

			Mesh mesh = null;
			if(go != null) {
				MeshFilter mf = go.GetComponent<MeshFilter>();
				SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
				if(smr != null) mesh = smr.sharedMesh;
				else if(mf != null) mesh = mf.sharedMesh;
			}

			if(selectedTransforms.Length > 1) GUILayout.Label("Multiple GameObjects selected", windowTitleStyle);
			else {
				GUILayout.Label("GameObject: "+go.name, windowTitleStyle);
				if(GUI.Button(new Rect(position.width - 123, 2,50,20), "Backup")) {
					MakeBackup(go, false);
				}
				if(GUI.Button(new Rect(position.width - 71, 2,50,20), "Revert")) {
					CloseSubwindows();
					RestoreBackup(go);
				}
			}

		    GUILayout.BeginVertical(GUILayout.Width(this.position.width - 25f));
				GUILayout.Label("");
				string tip = toolTip;
				if(selectedTransforms != null && selectedTransforms.Length > 0 && batchFunction != null) {
					tip = "Performing batch operation (" + (batchIndex+1) + " of " + selectedTransforms.Length + ")...";
				}
				GUILayout.Box(tip, toolTipStyle, GUILayout.Width(this.position.width - 25f), GUILayout.Height(35f));
				GUILayout.Label("Step 1: Preparation", titleStyle);
			    if(selectedTransforms.Length <= 1) {
				    GUILayout.BeginHorizontal();
					    GUILayout.BeginVertical();
							if(GUILayout.Button(new GUIContent("Merge child meshes", "Merge all enabled child meshes into a single mesh"), mainButtonStyle)) {
								string warning = DurationWarning(mesh, false);
								if(warning == null || EditorUtility.DisplayDialog("Merge child meshes", warning, "Ok, let's go", "Cancel")) {
									string result = MergeChildMeshesPerMaterial(go);
									UnityEngine.Debug.Log("Merge child meshes: " + result);
									EditorUtility.DisplayDialog("Merge child meshes", result, "Close");
								}
							}
							if(GUILayout.Button(new GUIContent("Selective mesh merge", "Select which submeshes to merge together in a new mesh"))) {
								ShowMergePopup();
							}
							if(GUILayout.Button(new GUIContent("Remove a submesh", "Select submeshes and remove them entirely"))) {
								ShowRemovePopup();
							}
					    GUILayout.EndVertical();

					    GUILayout.BeginVertical();
							if(mesh != null) {
								if(GUILayout.Button(new GUIContent("Bake atlases", "Combine textures in an atlas to reduce the nr of draw calls"))) {
									ShowMaterialPopup();
								}
							} else {
								GUILayout.Label("");
							}
							if(GUILayout.Button(new GUIContent("Remove hidden triangles", "Remove triangles that are hidden (under clothing)"))) {
								ShowRemoveHiddenPopup();
							}
							if(GUILayout.Button(new GUIContent("Simplify mesh", "Use the LOD compression to create a new mesh with fewer vertices. Uses the slider value \"Compression LOD1\""))) {
								string warning = DurationWarning(mesh, true);
								if(warning == null || EditorUtility.DisplayDialog("Simplify mesh", warning, "Ok, let's go", "Cancel")) {
									string result = SimplifyMesh(go);
									UnityEngine.Debug.Log("Simplify mesh: " + result);
									EditorUtility.DisplayDialog("Simplify mesh", result, "Close");
								}
							}
					    GUILayout.EndVertical();
				    GUILayout.EndHorizontal();
			    } else {
					if(GUILayout.Button(new GUIContent("Merge child meshes per selected GameObject", "Batch operation per selected GameObject: Merge all enabled child meshes into a single mesh"), mainButtonStyle)) {
						if(EditorUtility.DisplayDialog("Merge child meshes", "This operation will merge submeshes in all selected gameobjects.\nMake sure you have only selected the parent gameobjects, not their children.\nDo you want to proceed?", "Proceed", "Cancel")) {
							PerformBatchOperation(MergeChildMeshesPerMaterial);
						}
					}
					if(batchFunction == MergeChildMeshesPerMaterial && batchIndex >= selectedTransforms.Length) {
						batchFunction = null;
						batchIndex = 0;
						EditorUtility.DisplayDialog("Merge child meshes", "Done.\nOpen the Console for details.", "Close");
					}
					if(GUILayout.Button(new GUIContent("Simplify all meshes in selection", "Create a new mesh with fewer vertices for evey mesh that is found in any of the selected gameobjects or their children"))) {
						if(EditorUtility.DisplayDialog("Simplify meshes", "This operation will create new meshes and will alter all selected gameobjects.\nDo you want to proceed?", "Proceed", "Cancel")) {
							PerformBatchOperation(CreateLODSwitcherWithMeshes);
						}
					}
					if(batchFunction == MergeChildMeshesPerMaterial && batchIndex >= selectedTransforms.Length) {
						batchFunction = null;
						batchIndex = 0;
						EditorUtility.DisplayDialog("Simplify meshes", "Done.\nOpen the Console for details.", "Close");
					}
			    }
				if(mesh != null || selectedTransforms.Length > 1) {
					GUILayout.Label("\nStep 2: Create LOD levels", titleStyle);
					float oldValue = 0f;
					int oldInt = 0;

					oldInt = nrOfLevels;
					nrOfLevels = EditorGUILayout.IntPopup(new GUIContent("Nr of LOD levels", "Choose the number of LOD levels you want to create"), nrOfLevels, new GUIContent[5] {new GUIContent("LOD 0 - 1", "Make 1 level"), new GUIContent("LOD 0 - 2", "Make 2 levels"), new GUIContent("LOD 0 - 3", "Make 3 levels"), new GUIContent("LOD 0 - 4", "Make 4 levels"), new GUIContent("LOD 0 - 5", "Make 5 levels")}, new int[5] { 1, 2, 3, 4, 5});
					if(nrOfLevels != oldInt) PlayerPrefs.SetInt("SimpleLOD_nr_of_levels", nrOfLevels);

					for(int i=0;i<nrOfLevels;i++) {
						oldValue = compression[i];
						if(i > 0 && compression[i] < compression[i - 1]) compression[i] = compression[i - 1];
						compression[i] = EditorGUILayout.Slider(new GUIContent("Compression LOD "+(i+1), "Set the compression for LOD " + (i+1) + (i<=0 ? " (or for simplifying the current mesh)" : "")), compression[i], 0, 3);
						compression[i] = compression[i].Round(2);
						if(compression[i] != oldValue) PlayerPrefs.SetFloat("SimpleLOD_compression_" + i, compression[i]);
					}

					oldValue = smallObjectsValue;
					smallObjectsValue = EditorGUILayout.Slider(new GUIContent("Remove small parts", "Set the max size for small parts like buttons, doorknobs, etc. to be removed"), smallObjectsValue, 0, 5);
					smallObjectsValue = smallObjectsValue.Round(2);
					if(smallObjectsValue != oldValue) {
						PlayerPrefs.SetFloat("SimpleLOD_smallObjectsValue", smallObjectsValue + 10f);
					}

					bool oldBool = recalcNormals;
			    	recalcNormals = EditorGUILayout.Toggle(new GUIContent("Recalculate normals", "Switch this on if you want to calculate new normals instead of using the existing ones"), recalcNormals);
			    	if(oldBool != recalcNormals) PlayerPrefs.SetInt("SimpleLOD_recalc_norm", recalcNormals ? 1 : 0);

					if(settingsCollapsed) {
						if(GUILayout.Button(new GUIContent("more...", "Show more controls"), GUILayout.Width(55))) {
							settingsCollapsed = !settingsCollapsed;
						}
						useValueForNrOfSteps = 1;
						useValueForProtectNormals = 1;
						useValueForProtectUvs = 1;
						useValueForProtectBigTriangles = 1;
						useValueForBoostCompression = 1;
						useValueForProtectSubMeshesAndSharpEdges = 1;
						if(Event.current.type == EventType.Layout) showSettings--;
						if(showSettings < 0) showSettings = 0;
					} else {
						if(GUILayout.Button(new GUIContent("less...", "Show fewer controls and use the default values"), GUILayout.Width(55))) {
							settingsCollapsed = !settingsCollapsed;
						}
						if(Event.current.type == EventType.Layout) {
							if(showSettings <= 5) showSettings++;
							if(showSettings == 5) {
								useValueForNrOfSteps = nrOfSteps;
								useValueForProtectNormals = protectNormals;
								useValueForProtectUvs = protectUvs;
								useValueForProtectBigTriangles = protectBigTriangles;
								useValueForBoostCompression = boostCompression;
								useValueForProtectSubMeshesAndSharpEdges = protectSubMeshesAndSharpEdges;
							}
						}
					}
					if(showSettings > 0) {
						oldInt = useValueForNrOfSteps;
			   			nrOfSteps = Mathf.RoundToInt(EditorGUILayout.Slider(new GUIContent("Nr of steps", "Using more steps will give better result, but is also slower"), nrOfSteps, 1, 10));
						if(nrOfSteps != oldInt) {
							useValueForNrOfSteps = nrOfSteps;
							PlayerPrefs.SetInt("SimpleLOD_nr_of_steps", nrOfSteps);
						}

						oldValue = protectNormals;
						protectNormals = EditorGUILayout.Slider(new GUIContent("Protect normals", "Prevent changing the normal due to combining 2 triangles"), protectNormals, 0, 2);
						protectNormals = protectNormals.Round(2);
						if(protectNormals != oldValue) {
							useValueForProtectNormals = protectNormals;
							PlayerPrefs.SetFloat("SimpleLOD_protectNormals", protectNormals + 10f);
						}

						oldValue = protectUvs;
						protectUvs = EditorGUILayout.Slider(new GUIContent("Protect UV", "Prevent stretching the UV coordinates due to combining 2 triangles"), protectUvs, 0, 2);
						protectUvs = protectUvs.Round(2);
						if(protectUvs != oldValue) {
							useValueForProtectUvs = protectUvs;
							PlayerPrefs.SetFloat("SimpleLOD_protectUvs", protectUvs + 10f);
						}

						oldValue = protectSubMeshesAndSharpEdges;
						protectSubMeshesAndSharpEdges = EditorGUILayout.Slider(new GUIContent("Protect sharp edges", "Protects vertices with multiple normals or uv coordinates"), protectSubMeshesAndSharpEdges, 0, 2);
						protectSubMeshesAndSharpEdges = protectSubMeshesAndSharpEdges.Round(2);
						if(protectSubMeshesAndSharpEdges != oldValue) {
							useValueForProtectSubMeshesAndSharpEdges = protectSubMeshesAndSharpEdges;
							PlayerPrefs.SetFloat("SimpleLOD_protectSubMeshesAndSharpEdges", protectSubMeshesAndSharpEdges + 10f);
						}

						oldValue = protectBigTriangles;
						protectBigTriangles = EditorGUILayout.Slider(new GUIContent("Protect bigger triangles", "Prevent affecting larger triangles more than affecting smaller triangles"), protectBigTriangles, 0, 2);
						protectBigTriangles = protectBigTriangles.Round(2);
						if(protectBigTriangles != oldValue) {
							useValueForProtectBigTriangles = protectBigTriangles;
							PlayerPrefs.SetFloat("SimpleLOD_protectBigTriangles", protectBigTriangles + 10f);
						}

						oldValue = boostCompression;
						boostCompression = EditorGUILayout.Slider(new GUIContent("Compression multiplier", "Multiply the compression levels for extra strong compression"), boostCompression, 1, 10);
						boostCompression = boostCompression.Round(2);
						if(boostCompression != oldValue) {
							useValueForBoostCompression = boostCompression;
							PlayerPrefs.SetFloat("SimpleLOD_protectNormals", boostCompression);
						}
					} else {
						boostCompression = 1f;
						useValueForBoostCompression = 1f;
					}

					string batchStr = "";
					string batchTip = "";
				    if(selectedTransforms.Length > 1) {
				    	batchStr = "Batch operation: ";
				    	batchTip = "Batch operation per selected gameobject. ";
				    }
					if(GUILayout.Button(new GUIContent(batchStr + "a. Create LOD levels and LODSwitcher", batchTip + "Bake 3 new LOD meshes and add an LODSwitcher component"), mainButtonStyle)) {
						CloseSubwindows();
						if(selectedTransforms.Length > 1) {
							if(EditorUtility.DisplayDialog("Create LOD levels", "This operation will create LOD meshes and will alter all selected gameobjects.\nDo you want to proceed?", "Proceed", "Cancel")) {
								PerformBatchOperation(CreateLODSwitcherWithMeshes);
							}
						} else {
							string warning = DurationWarning(mesh, true);
							if(warning == null || EditorUtility.DisplayDialog("Create LOD levels", warning, "Ok, let's go", "Cancel")) {
								string result = CreateLODSwitcherWithMeshes(go);
								UnityEngine.Debug.Log("Create LOD levels: " + result);
								EditorUtility.DisplayDialog("Create LOD levels", result, "Close");
							}
						}
					}

					if(GUILayout.Button(new GUIContent(batchStr + "b. Create LOD levels and child objects", batchTip + "Same as a, but it will create a separate child object for each LOD level"))) {
						CloseSubwindows();
						if(selectedTransforms.Length > 1) {
							if(EditorUtility.DisplayDialog("Create LOD levels", "This operation will create LOD meshes and will alter all selected gameobjects.\nDo you want to proceed?", "Proceed", "Cancel")) {
								PerformBatchOperation(CreateLODSwitcherWithChildObjects);
							}
						} else {
							string warning = DurationWarning(mesh, true);
							if(warning == null || EditorUtility.DisplayDialog("Create LOD levels and child objects", warning, "Ok, let's go", "Cancel")) {
								string result = CreateLODSwitcherWithChildObjects(go);
								UnityEngine.Debug.Log("Create LOD levels and child objects: " + result);
								EditorUtility.DisplayDialog("Create LOD levels and child objects", result, "Close");
							}
						}
					}

					if(GUILayout.Button(new GUIContent(batchStr + "c. Create LOD levels and use LODGroup", batchTip + "Same as b, but it will create a new parent object that holds Unity's default LODGroup component"))) {
						CloseSubwindows();
						if(selectedTransforms.Length > 1) {
							if(EditorUtility.DisplayDialog("Create LOD levels", "This operation will create LOD meshes and will alter all selected gameobjects.\nDo you want to proceed?", "Proceed", "Cancel")) {
								PerformBatchOperation(CreateLODSwitcherWithLODGroup);
							}
						} else {
							string warning = DurationWarning(mesh, true);
							if(warning == null || EditorUtility.DisplayDialog("Create LOD levels and use LODGroup", warning, "Ok, let's go", "Cancel")) {
								string result = CreateLODSwitcherWithLODGroup(go);
								UnityEngine.Debug.Log("Create LOD levels and use LODGroup: " + result);
								EditorUtility.DisplayDialog("Create LOD levels and use LODGroup", result, "Close");
							}
						}
					}
					if((batchFunction == CreateLODSwitcherWithMeshes || batchFunction == CreateLODSwitcherWithLODGroup || batchFunction == CreateLODSwitcherWithChildObjects) && batchIndex >= selectedTransforms.Length) {
						batchFunction = null;
						batchIndex = 0;
						EditorUtility.DisplayDialog("Create LOD levels", "Done.\nOpen the Console for details.", "Close");
					}
			    }

			    GUILayout.Label("\n");
			    GUILayout.BeginHorizontal();
				    GUILayout.Label("SimpleLOD by Orbcreation BV");
				    if (GUILayout.Button("Open support webpage")) {
						CloseSubwindows();
					    Application.OpenURL("http://orbcreation.com/orbcreation/docu.orb?44");
			    	}
		    	GUILayout.EndHorizontal();
			    GUILayout.Label("");
		    GUILayout.EndVertical();
		EditorGUILayout.EndScrollView();

		if(GUI.tooltip.Length > 0) {
			toolTip = GUI.tooltip;
			toolTipClearCount = 0;
		} else if(toolTipClearCount > 5) {  // OnGUI is called multiple times per frame
			toolTip = "";
		} else {
			toolTipClearCount++;
		}
	}

	private void PerformBatchOperation(Func<GameObject, string> func) {
		if(selectedTransforms != null && selectedTransforms.Length > 0) {
			batchFunction = func;
			batchIndex = 0;
		}
	}

	private string MergeChildMeshesPerMaterial(GameObject aGo) {
		if(aGo == null) return "No gameObject selected";
		string path = StoragePathUsing1stMeshAndSubPath(aGo, "MergedMeshes");
		if(path != null) {
			Mesh[] meshes = null;
			int oldNrOfMeshes = 0;
			int oldNrOfSkinnedMeshes = 0;
			MeshFilter[] mfs = aGo.GetComponentsInChildren<MeshFilter>(false);
			SkinnedMeshRenderer[] smr = aGo.GetComponentsInChildren<SkinnedMeshRenderer>(false);
			if(mfs != null) oldNrOfMeshes = mfs.Length;
			if(smr != null) oldNrOfSkinnedMeshes = smr.Length;

			MakeBackup(aGo, true);

			try {
				meshes = aGo.CombineMeshes();
			} catch(Exception e) {
				Debug.LogError(e);
				return e.Message;
			}
			if(meshes != null && meshes.Length > 0) {
				string str0 = "Found";
				if(oldNrOfSkinnedMeshes > 0) {
					str0 = str0 + " " +oldNrOfSkinnedMeshes + " skinned meshes";
					if(oldNrOfMeshes > 0) str0 = str0 + " and";
				}
				if(oldNrOfMeshes > 0) str0 = str0 + " " + oldNrOfMeshes + " meshes";
				str0 = str0 + ".";
				string str = "";
				for(int i=0;i<meshes.Length;i++) {
					string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + aGo.name + "_part" + (i+1) + ".asset");
					AssetDatabase.CreateAsset(meshes[i], meshPath);
					AssetDatabase.SaveAssets();
					str = str + "\n" + meshes[i].vertexCount + " vertices, " + (meshes[i].triangles.Length / 3) + " triangles";
				}
				Resources.UnloadUnusedAssets();
				if(meshes.Length > 1) {
					return str0 + "\nMerged meshes were saved under "+(path + "/" + aGo.name)+"." + str;
				} else {
					return str0 + "\nMerged mesh was saved under "+(path + "/" + aGo.name)+"." + str;
				}
			}
		}
		return "No mesh found in gameobject";
	}

	private string CreateLODSwitcherWithMeshes(GameObject aGo) {
		if(aGo == null) return "No gameObject selected";
		string path = StoragePathUsing1stMeshAndSubPath(aGo, "LODMeshes");
		if(path != null) {
			float[] useCompressions = new float[nrOfLevels];
			Mesh[] meshes = null;
			MakeBackup(aGo, true);
			for(int i=0;i<nrOfLevels;i++) useCompressions[i] = compression[i] * useValueForBoostCompression;
			try {
				meshes = aGo.SetUpLODLevelsWithLODSwitcher(GetDftLodScreenSizes(nrOfLevels), useCompressions, recalcNormals, smallObjectsValue, useValueForProtectNormals, useValueForProtectUvs, useValueForProtectSubMeshesAndSharpEdges, useValueForProtectBigTriangles, useValueForNrOfSteps);
			} catch(Exception e) {
				Debug.LogError(e);
				return e.Message;
			}
			if(meshes != null) {
				string sizeStr = "";
				sizeStr = "LOD 0: " + meshes[0].vertexCount + " vertices, " + (meshes[0].triangles.Length / 3) + " triangles";
				path = path + "/" + aGo.name + "_LOD";
				for(int i=1;i<meshes.Length;i++) {
					sizeStr = sizeStr + "\nLOD " + i +": " + meshes[i].vertexCount + " vertices, " + (meshes[i].triangles.Length / 3) + " triangles";
					if(meshes[i] != null && meshes[i].vertexCount > 0) {
						string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + i + ".asset");
						AssetDatabase.CreateAsset(meshes[i], meshPath);
						AssetDatabase.SaveAssets();
					}
				}
				Resources.UnloadUnusedAssets();
				return "Finished! LOD meshes were saved under "+path+"[1..."+meshes.Length+"].\n" + sizeStr;
			}
		}
		return "No mesh found in gameobject";
    }

	private string CreateLODSwitcherWithChildObjects(GameObject aGo) {
		if(aGo == null) return "No gameObject selected";
		string path = StoragePathUsing1stMeshAndSubPath(aGo, "LODMeshes");
		if(path != null) {
			float[] useCompressions = new float[nrOfLevels];
			Mesh[] meshes = null;
			MakeBackup(aGo, true);
			for(int i=0;i<nrOfLevels;i++) useCompressions[i] = compression[i] * useValueForBoostCompression;
			try {
				meshes = aGo.SetUpLODLevelsAndChildrenWithLODSwitcher(GetDftLodScreenSizes(nrOfLevels), useCompressions, recalcNormals, smallObjectsValue, useValueForProtectNormals, useValueForProtectUvs, useValueForProtectSubMeshesAndSharpEdges, useValueForProtectBigTriangles, useValueForNrOfSteps);
			} catch(Exception e) {
				Debug.LogError(e);
				return e.Message;
			}
			if(meshes != null) {
				string sizeStr = "";
				sizeStr = "LOD 0: " + meshes[0].vertexCount + " vertices, " + (meshes[0].triangles.Length / 3) + " triangles";
				path = path + "/" + aGo.name + "_LOD";
				for(int i=1;i<meshes.Length;i++) {
					sizeStr = sizeStr + "\nLOD " + i +": " + meshes[i].vertexCount + " vertices, " + (meshes[i].triangles.Length / 3) + " triangles";
					if(meshes[i] != null && meshes[i].vertexCount > 0) {
						string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + i + ".asset");
						AssetDatabase.CreateAsset(meshes[i], meshPath);
						AssetDatabase.SaveAssets();
					}
				}
				Resources.UnloadUnusedAssets();
				return "Finished! LOD meshes were saved under "+path+"[1..."+(meshes.Length-1)+"].\n" + sizeStr;
			}
		}
		return "No mesh found in gameobject";
    }

	private string CreateLODSwitcherWithLODGroup(GameObject aGo) {
		if(aGo == null) return "No gameObject selected";
		string path = StoragePathUsing1stMeshAndSubPath(aGo, "LODMeshes");
		if(path != null) {
			float[] useCompressions = new float[nrOfLevels];
			Mesh[] meshes = null;
			MakeBackup(aGo, true);
			for(int i=0;i<nrOfLevels;i++) useCompressions[i] = compression[i] * useValueForBoostCompression;
			try {
				meshes = aGo.SetUpLODLevelsAndChildrenWithLODGroup(GetDftLodScreenSizes(nrOfLevels), useCompressions, recalcNormals, smallObjectsValue, useValueForProtectNormals, useValueForProtectUvs, useValueForProtectSubMeshesAndSharpEdges, useValueForProtectBigTriangles, useValueForNrOfSteps);
			} catch(Exception e) {
				Debug.LogError(e);
				return e.Message;
			}
			if(meshes != null) {
				string sizeStr = "";
				sizeStr = "LOD 0: " + meshes[0].vertexCount + " vertices, " + (meshes[0].triangles.Length / 3) + " triangles";
				path = path + "/";
				for(int i=1;i<meshes.Length;i++) {
					if(i>0) sizeStr = sizeStr + "\nLOD " + i +": " + meshes[i].vertexCount + " vertices, " + (meshes[i].triangles.Length / 3) + " triangles";
					if(meshes[i] != null && meshes[i].vertexCount > 0) {
						string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + meshes[i].name + ".asset");
						AssetDatabase.CreateAsset(meshes[i], meshPath);
						AssetDatabase.SaveAssets();
					}
				}
				Resources.UnloadUnusedAssets();
				return "Finished! LOD meshes were saved under "+path+".\n" + sizeStr;
			}
		}
		return "No mesh found in gameobject";
    }

	private string SimplifyMesh(GameObject aGo) {
		if(aGo == null) return "No gameObject selected";
		string path = StoragePathUsing1stMeshAndSubPath(aGo, "SimplifiedMeshes");
		if(path != null) {
			Mesh mesh = null;
			MakeBackup(aGo, true);
			try {
				mesh = aGo.GetSimplifiedMesh(compression[0] * useValueForBoostCompression, recalcNormals, smallObjectsValue, useValueForProtectNormals, useValueForProtectUvs, useValueForProtectSubMeshesAndSharpEdges, useValueForProtectBigTriangles, useValueForNrOfSteps);
			} catch(Exception e) {
				Debug.LogError(e);
				return e.Message;
			}
			if(mesh != null) {
				string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + aGo.name + "_LOD" + ".asset");
				if(mesh != null && mesh.vertexCount > 0) {
					AssetDatabase.CreateAsset(mesh, meshPath);
					AssetDatabase.SaveAssets();
				}
				Resources.UnloadUnusedAssets();
				return "Finished! Your mesh was saved as "+meshPath+".\n" + mesh.vertexCount + " vertices, " + (mesh.triangles.Length / 3) + " triangles";
			}
		}
		return "No mesh found in gameobject";
    }

    private float[] GetDftLodScreenSizes(int aNrOflevels) {
		switch(nrOfLevels) {
		case 1:
			return new float[1] {0.5f};
		case 2:
			return new float[2] {0.6f, 0.3f};
		case 3:
			return new float[3] {0.6f, 0.3f, 0.15f};
		case 4:
			return new float[4] {0.75f, 0.5f, 0.25f, 0.13f};
		default:
			return new float[5] {0.8f, 0.6f, 0.4f, 0.2f, 0.1f};
		}
    }
	private string StoragePathUsing1stMeshAndSubPath(GameObject aGo, string subPath) {
		Mesh firstMesh = aGo.Get1stSharedMesh();
		if(firstMesh != null) {
			string[] pathSegments = new string[3] {"Assets", "SimpleLOD", "WillBeIgnored"};
			string assetPath = AssetDatabase.GetAssetPath(firstMesh);
			if(assetPath != null && assetPath.Length > 0 && assetPath.StartsWith("Assets/")) pathSegments = assetPath.Split(new Char[] {'/'});
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

	private static GameObject GameObjectToBackup(GameObject aGO) {
		// check if there is a skinned mesh renderer
		// if yes, then find the mutual parent of the smr and the bones
		// and return that instead
		// because otherwise the link breaks when you restore the backup
		if(aGO == null) return null;
		if(aGO.name.IndexOf("_$Lod:",0) >= 0) {
			if(aGO.transform.parent == null) return null;
			aGO = aGO.transform.parent.gameObject;
		}
		if(aGO.name.IndexOf("_$LodGrp",0) >= 0) {
			int pos = aGO.name.IndexOf("_$LodGrp",0);
			if(pos <= 0) return null;
			string name = aGO.name.Substring(0,pos);
			aGO = aGO.FindFirstChildWithName(name);
			if(aGO == null) {
//				Debug.LogError("Child gameObject with name "+name+" not found");
				return null;
			}
		}

		SkinnedMeshRenderer[] smr = aGO.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		if(smr != null && smr.Length > 0) {
			Transform[] bones = smr[0].bones;
			if(bones != null && bones.Length > 1) {
				GameObject mutualParent = aGO.FindMutualParent(bones[0].gameObject);
				if(mutualParent != null) return mutualParent;
			}
		}
		return aGO;
	}

	private static GameObject GetBackup(GameObject aGO) {
		aGO = GameObjectToBackup(aGO);
		if(aGO == null) return null;
		GameObject bcpTopGO = GameObject.Find("_SimpleLOD_backups_delete_when_ready");
		if(bcpTopGO != null){
			foreach(Transform child in bcpTopGO.transform) {
				if(child.gameObject.name == aGO.name) return child.gameObject;
			}
		}
		return null;
	}

	private static void MakeBackup(GameObject aGO, bool onlyIfNotExists) {
		if(onlyIfNotExists && GetBackup(aGO) != null) return;
		aGO = GameObjectToBackup(aGO);
		if(aGO == null) return;
		if(aGO.FindParentWithName("_SimpleLOD_backups_delete_when_ready") != null) return;  // no backup of a backup
		if(aGO.name.IndexOf("_$LodGrp",0) >= 0) return;
		GameObject bcpTopGO = GameObject.Find("_SimpleLOD_backups_delete_when_ready");
		if(bcpTopGO == null) {
			bcpTopGO = new GameObject("_SimpleLOD_backups_delete_when_ready");
			bcpTopGO.transform.position = Vector3.zero;
		} else {
			GameObject oldBcp = GetBackup(aGO);
			if(oldBcp != null) DestroyImmediate(oldBcp);
		}
		GameObject bcpGO = (GameObject)GameObject.Instantiate(aGO);
		bcpGO.name = bcpGO.name.Replace("(Clone)", "");
		#if UNITY_4_3
			bcpGO.transform.parent = bcpTopGO.transform;
		#elif UNITY_4_4
			bcpGO.transform.parent = bcpTopGO.transform;
		#elif UNITY_4_5
			bcpGO.transform.parent = bcpTopGO.transform;
		#else
			bcpGO.transform.SetParent(bcpTopGO.transform);
		#endif
		bcpGO.SetActive(false);
	}

	private void RestoreBackup(GameObject aGO) {
		aGO = GameObjectToBackup(aGO);
		if(aGO == null) return;
		GameObject bcp = GetBackup(aGO);
		if(bcp != null) {
			GameObject newGO = (GameObject)GameObject.Instantiate(bcp);
			newGO.SetActive(true);
			newGO.name = newGO.name.Replace("(Clone)", "");
			Transform goParent = aGO.transform.parent;
			if(goParent != null) {
				if(goParent.gameObject.name.IndexOf("_$Lod") > 0) {
					goParent.gameObject.SetActive(false);
				} else {
					newGO.transform.parent = goParent;
				}
			}
			newGO.transform.localPosition = aGO.transform.localPosition;
			newGO.transform.localScale = aGO.transform.localScale;
			newGO.transform.localRotation = aGO.transform.localRotation;
			DestroyImmediate(aGO);
			Selection.activeGameObject = newGO;
		} else {
			EditorUtility.DisplayDialog("Restore failed", "Sorry, there is no backup available for \"" + aGO.name + "\"", "Close");	
		}
	}

	private string DurationWarning(Mesh mesh, bool compress) {
		int vertexCount = 0;
		float estimatedDuration = 0f;
		if(mesh == null) return null;
		vertexCount = mesh.vertexCount;
		if(compress) estimatedDuration += Mathf.Pow((vertexCount * 0.006f) / 360f, 1.3f) * 8f;
		if(estimatedDuration < 15f) return null;
		string s = "This operation may take about ";
		if(estimatedDuration < 25f) s = s + Mathf.RoundToInt(estimatedDuration) + " seconds.";
		else if(estimatedDuration < 60f) s = s + (Mathf.RoundToInt(estimatedDuration/10f) * 10) + " seconds.";
		else s = s + (Mathf.RoundToInt(estimatedDuration / 6f) / 10f) + " minutes";
		return s;
	}

	void OnInspectorUpdate() {
		Repaint();
	}
}
