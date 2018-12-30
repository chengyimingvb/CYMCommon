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


public class SimpleLOD_RemoveHiddenPopup : EditorWindow {
	private GameObject go;
	private List<Hashtable> submeshRows;
	private Vector2 scrollPos = Vector2.zero;
	private Texture2D pixel;
	private float maxDistance = 10f;

	public void OpenWindow(GameObject aGO) {
		go = aGO;
		pixel = new Texture2D(1, 1, TextureFormat.RGB24, false);
		pixel.SetPixels(new Color[1] {Color.white});
		pixel.Apply(false, false);

		Init();

		if(submeshRows == null || submeshRows.Count <= 0) {
			if(aGO.activeSelf) EditorUtility.DisplayDialog("Merge meshes window", "This game object has no meshes.", "OK");
			else EditorUtility.DisplayDialog("Merge meshes window", "This game object is deactivated.", "OK");
			Close();
			return;
		}

		this.position = new Rect((Screen.width/2)+200, (Screen.height/2)+50, 650, 500);
		this.minSize = new Vector3(650,200);
		this.maxSize = new Vector3(650,1000);
		#if UNITY_4_3
			this.title = "Remove hidden";
		#elif UNITY_4_4
			this.title = "Remove hidden";
		#elif UNITY_4_5
			this.title = "Remove hidden";
		#elif UNITY_4_6
			this.title = "Remove hidden";
		#elif UNITY_5_0
			this.title = "Remove hidden";
		#else
			this.titleContent = new GUIContent("Remove hidden");
		#endif
		this.Show();
	}

	public void Init() {
		submeshRows = new List<Hashtable>();

		MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>(false);
		SkinnedMeshRenderer[] skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>(false);
		if(skinnedMeshRenderers != null && skinnedMeshRenderers.Length > 0) {
			foreach(SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers) {
				if(skinnedMeshRenderer.sharedMesh != null) {
					Material[] mats = skinnedMeshRenderer.sharedMaterials;
					Mesh mesh = skinnedMeshRenderer.sharedMesh;
					for(int i=0;i<mesh.subMeshCount;i++) {
						Hashtable row = new Hashtable();
						row["gameObject"] = skinnedMeshRenderer.gameObject;
						row["mesh"] = mesh;
						if(mats.Length > i)	{
							row["mat"] = mats[i];
						} else {
							row["mat"] = null;
						}
						row["remove"] = false;
						row["hiddenBy"] = false;
						row["triangles"] = mesh.triangles.Length / 3;
						row["lightmapIdx"] = -1;
						submeshRows.Add(row);
					}
				}
			}
		}
		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					Material[] mats = meshRenderer.sharedMaterials;
					Mesh mesh = filter.sharedMesh;
					for(int i=0;i<mesh.subMeshCount;i++) {
						Hashtable row = new Hashtable();
						row["gameObject"] = meshRenderer.gameObject;
						row["mesh"] = mesh;
						if(mats.Length > i)	{
							row["mat"] = mats[i];
						} else {
							row["mat"] = null;
						}
						row["remove"] = false;
						row["hiddenBy"] = false;
						row["triangles"] = mesh.triangles.Length / 3;
						row["lightmapIdx"] = meshRenderer.lightmapIndex;
						submeshRows.Add(row);
					}
				}
			}
		}
	}

	public new void Close() {
		go = null;
		submeshRows = null;
		Resources.UnloadUnusedAssets();
		DestroyImmediate(pixel);
		pixel = null;
		base.Close();
	}

	void OnGUI() {
		if(go == null) Close();
		float[] w = new float[] {225f, 50f, 30f};
		int totalVertexCount = 0;
		int totalTriangleCount = 0;
		bool somethingToHide = false;
		bool somethingHiding = false;

		GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
		titleStyle.fontStyle = FontStyle.Bold;
		GUIStyle rightAlignedTitleStyle = new GUIStyle(titleStyle);
		rightAlignedTitleStyle.alignment = TextAnchor.MiddleRight;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));


		GUILayout.Label("\nSelect the submesh on the left hand side that you want to strip from surfaces hidden by submeshes on the\nright hand side\n");
	    GUILayout.BeginHorizontal();
	    GUILayout.BeginVertical();

		GUILayout.Label("Remove hidden triangles in:", titleStyle, GUILayout.Width(w[0] + w[1] + w[2]));
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Gameobject / submesh", titleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("Vertices", titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
	    GUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1] + w[2]));

	    GameObject prevGo = null;
	    Mesh prevMesh = null;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			GameObject rowGo = (GameObject)row["gameObject"];
			Mesh mesh = (Mesh)row["mesh"];
			Material mat = (Material)row["mat"];
			bool remove = (bool)row["remove"];
			bool hiddenBy = (bool)row["hiddenBy"];
			string submeshName = "";
			if(hiddenBy) {
				remove = false;
				row["remove"] = false;
			}
			if(remove) somethingToHide = true;
			if(mat != null) submeshName = mat.name;
			else submeshName = "unknown material";
   			if(mesh != prevMesh || rowGo != prevGo) {
			    GUILayout.BeginHorizontal();
   				GUILayout.Label(rowGo.name, GUILayout.Width(w[0]));
				GUILayout.Label("" + mesh.vertexCount, GUILayout.Width(w[1]));
				totalVertexCount += mesh.vertexCount;
				totalTriangleCount += (int)row["triangles"];
				GUILayout.Label("", GUILayout.Width(w[2]));
			    GUILayout.EndHorizontal();
			}
		    GUILayout.BeginHorizontal();
			GUILayout.Label("       " + submeshName, GUILayout.Width(w[0]));
			GUILayout.Label("", titleStyle, GUILayout.Width(w[1]));
			bool newBool = EditorGUILayout.Toggle("", remove, GUILayout.Width(w[2]));
			if(newBool != remove) {
				row["remove"] = newBool;
				if(newBool) {
					for(int j=0;j<submeshRows.Count;j++) {
						Hashtable row2 = (Hashtable)submeshRows[j];
						bool remove2 = (bool)row2["remove"];
						if(remove2 && j != i) row2["remove"] = false; // max 1 selected
					}
				}
		    }
		    GUILayout.EndHorizontal();
   			if(i<submeshRows.Count-1 && 
   				((Mesh)submeshRows[i+1]["mesh"] != mesh || 
   				(GameObject)submeshRows[i+1]["gameObject"] != rowGo)) {
   				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1]));
			}
			prevMesh = mesh;
			prevGo = rowGo;
		}
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1]));
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Total vertices", rightAlignedTitleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("" + totalVertexCount, titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
	    GUILayout.EndHorizontal();
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Triangles", rightAlignedTitleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("" + totalTriangleCount, titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
	    GUILayout.EndHorizontal();
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Submeshes", rightAlignedTitleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("" + submeshRows.Count, titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
	    GUILayout.EndHorizontal();
	    GUILayout.EndVertical();


   	    GUILayout.BeginVertical();
		GUILayout.Label("When obscured by:", titleStyle, GUILayout.Width(w[0] + w[2]));
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Gameobject / submesh", titleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
	    GUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[2]));

		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			GameObject rowGo = (GameObject)row["gameObject"];
			Mesh mesh = (Mesh)row["mesh"];
			Material mat = (Material)row["mat"];
			bool remove = (bool)row["remove"];
			bool hiddenBy = (bool)row["hiddenBy"];
			string submeshName = "";
			if(remove) {
				hiddenBy = false;
				row["hiddenBy"] = false;
			}
			if(hiddenBy) somethingHiding = true;
			if(mat != null) submeshName = mat.name;
			else submeshName = "unknown material";
   			if(mesh != prevMesh || rowGo != prevGo) {
			    GUILayout.BeginHorizontal();
   				GUILayout.Label(rowGo.name, GUILayout.Width(w[0]));
				GUILayout.Label("", GUILayout.Width(w[2]));
			    GUILayout.EndHorizontal();
			}
		    GUILayout.BeginHorizontal();
			GUILayout.Label("       " + submeshName, GUILayout.Width(w[0]));
			bool newBool = EditorGUILayout.Toggle("", hiddenBy, GUILayout.Width(w[2]));
			if(newBool != hiddenBy) row["hiddenBy"] = newBool;
		    GUILayout.EndHorizontal();
   			if(i<submeshRows.Count-1 && 
   				((Mesh)submeshRows[i+1]["mesh"] != mesh || 
   				(GameObject)submeshRows[i+1]["gameObject"] != rowGo)) {
   				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0]));
			}
			prevMesh = mesh;
			prevGo = rowGo;
		}
   		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0]));
	    GUILayout.EndVertical();

	    GUILayout.EndHorizontal();
   		GUILayout.Label("");
   		if(!somethingToHide) {
   			GUILayout.Label("Select a submesh on the left hand side", GUILayout.Width(w[0] + w[1]+ w[2]));
   		} else if(!somethingHiding) {
   			GUILayout.Label("Select a submesh on the right hand side", GUILayout.Width(w[0] + w[1] + w[2]));
   		} else {
			maxDistance = EditorGUILayout.Slider("Max distance in mm.", maxDistance, 0f, 100f, GUILayout.Width(w[0] + w[1] + w[2]));
			if(maxDistance > 0f && GUILayout.Button("Remove hidden triangles", GUILayout.Width(w[0] + w[1] + w[2]))) {
				string result = RemoveHidden();
				UnityEngine.Debug.Log("Remove hidden triangles: " + result);
				EditorUtility.DisplayDialog("Remove hidden triangles", result, "Close");
				Init();
			} else {
   				GUILayout.Label("", GUILayout.Width(w[0] + w[1] + w[2]));
			}
   		}
   		GUILayout.Label("");
		EditorGUILayout.EndScrollView();
	}

	private string RemoveHidden() {
		string returnString = "";
		List<Mesh> hidingMeshes = new List<Mesh>();
		List<int> hidingSubMeshes = new List<int>();
		List<Matrix4x4> hidingMatrices = new List<Matrix4x4>();
	    GameObject prevGo = null;
	    Mesh prevMesh = null;
	    int subMeshIdx = 0;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			GameObject rowGo = (GameObject)row["gameObject"];
			Mesh mesh = (Mesh)row["mesh"];
			if(mesh != prevMesh || rowGo != prevGo) {
				prevGo = rowGo;
				prevMesh = mesh;
				subMeshIdx = 0;
			}
			if((bool)row["hiddenBy"]) {
				hidingMeshes.Add(mesh);
				hidingSubMeshes.Add(subMeshIdx);
				hidingMatrices.Add(rowGo.transform.localToWorldMatrix);
			}
			subMeshIdx++;
		}

		subMeshIdx = 0;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			GameObject rowGo = (GameObject)row["gameObject"];
			Mesh mesh = (Mesh)row["mesh"];
			Material mat = (Material)row["mat"];
			string submeshName = "";
			if(mat != null) submeshName = mat.name;
			else submeshName = "unknown material";

			if(mesh != prevMesh || rowGo != prevGo) {
				prevGo = rowGo;
				prevMesh = mesh;
				subMeshIdx = 0;
			}
			if((bool)row["remove"]) {
				Mesh newMesh = null;
				try {
					newMesh = mesh.CopyAndRemoveHiddenTriangles(subMeshIdx, rowGo.transform.localToWorldMatrix, hidingMeshes.ToArray(), hidingSubMeshes.ToArray(), hidingMatrices.ToArray(), maxDistance); // * 0.001f);
				} catch(Exception e) {
					Debug.LogError(e);
					return e.Message;
				}
				if(newMesh != null) {
					int removed = (mesh.triangles.Length - newMesh.triangles.Length) / 3;
					if(removed > 0) {
						string path = StoragePathUsingMeshAndSubPath(mesh, "CleanedMeshes");
						if(path != null) {
							string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + mesh.name + ".asset");
							AssetDatabase.CreateAsset(newMesh, meshPath);
							AssetDatabase.SaveAssets();
							returnString = returnString + rowGo.name + " - " + submeshName + ", removed " + removed + " triangles\n";
						}

						SkinnedMeshRenderer smr = rowGo.GetComponent<SkinnedMeshRenderer>();
						if(smr != null) smr.sharedMesh = newMesh;
						else {
							MeshFilter mf = rowGo.GetComponent<MeshFilter>();
							if(mf != null) mf.sharedMesh = newMesh;
						}
					} else {
						returnString = returnString + rowGo.name + " - " + submeshName + ", no triangles were removed\n";
					}
					Resources.UnloadUnusedAssets();
				}
			}

			subMeshIdx++;
		}
		return returnString;
	}

	private string StoragePathUsingMeshAndSubPath(Mesh aMesh, string subPath) {
		Mesh firstMesh = go.Get1stSharedMesh();
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

}