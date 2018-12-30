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


public class SimpleLOD_MergePopup : EditorWindow {
	private GameObject go;
	private List<Hashtable> submeshRows;
	private int lightmapIdx = -1;
	private bool makeNewGameObject = false;
	private Vector2 scrollPos = Vector2.zero;
	private Texture2D pixel;
	private bool gameObjectNamesUnique = false;

	public void OpenWindow(GameObject aGO) {
		go = aGO;
		submeshRows = new List<Hashtable>();
		pixel = new Texture2D(1, 1, TextureFormat.RGB24, false);
		pixel.SetPixels(new Color[1] {Color.white});
		pixel.Apply(false, false);

		if(aGO.GetComponent<SkinnedMeshRenderer>() != null || aGO.GetComponent<MeshRenderer>() != null) makeNewGameObject = true;
		MeshRenderer[] meshRenderers = aGO.GetComponentsInChildren<MeshRenderer>(false);
		SkinnedMeshRenderer[] skinnedMeshRenderers = aGO.GetComponentsInChildren<SkinnedMeshRenderer>(false);
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
							row["merge"] = true;
						} else {
							row["mat"] = null;
							row["merge"] = true;
						}
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
							row["merge"] = true;
						} else {
							row["mat"] = null;
							row["merge"] = true;
						}
						row["triangles"] = mesh.triangles.Length / 3;
						row["lightmapIdx"] = meshRenderer.lightmapIndex;
						submeshRows.Add(row);
					}
				}
			}
		}

		if(submeshRows.Count <= 0) {
			if(aGO.activeSelf) EditorUtility.DisplayDialog("Merge meshes window", "This game object has no meshes.", "OK");
			else EditorUtility.DisplayDialog("Merge meshes window", "This game object is deactivated.", "OK");
			Close();
			return;
		}

		gameObjectNamesUnique = TestIfGameObjectNamesAreUnique();

		this.position = new Rect((Screen.width/2)+200, (Screen.height/2)+50, 650, 500);
		this.minSize = new Vector3(650,200);
		this.maxSize = new Vector3(650,1600);
		#if UNITY_4_3
			this.title = "Merge meshes";
		#elif UNITY_4_4
			this.title = "Merge meshes";
		#elif UNITY_4_5
			this.title = "Merge meshes";
		#elif UNITY_4_6
			this.title = "Merge meshes";
		#elif UNITY_5_0
			this.title = "Merge meshes";
		#else
			this.titleContent = new GUIContent("Merge meshes");
		#endif
		this.Show();
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
		bool hasLightmaps = false;
		List<Mesh> newMeshRows = new List<Mesh>();
		List<Material> newSubmeshRows = new List<Material>();
		float[] w = new float[] {225f, 50f, 60f, 40f};
		int totalVertexCount = 0;
		int totalTriangleCount = 0;
		GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
		titleStyle.fontStyle = FontStyle.Bold;
		GUIStyle rightAlignedTitleStyle = new GUIStyle(titleStyle);
		rightAlignedTitleStyle.alignment = TextAnchor.MiddleRight;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));


		GUILayout.Label("\nSelect the submeshes that you want to merge together in a new mesh\n");
	    GUILayout.BeginHorizontal();
	    GUILayout.BeginVertical();

	    GUILayout.BeginHorizontal();
		GUILayout.Label("Gameobject / submesh", titleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("Vertices", titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("Lightmap", titleStyle, GUILayout.Width(w[2]));
		GUILayout.Label("Merge", titleStyle, GUILayout.Width(w[3]));
	    GUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1] + w[2] + w[3]));

	    lightmapIdx = -1;
	    GameObject prevGo = null;
	    Mesh prevMesh = null;
	    int mergeCount = 0;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			int rowLightmapIdx = (int)row["lightmapIdx"];
			bool merge = (bool)row["merge"];
			if(rowLightmapIdx >= 0 && rowLightmapIdx < 254) hasLightmaps = true;
			if(merge) {
				if(lightmapIdx>=0 && rowLightmapIdx != lightmapIdx) {
					row["merge"] = false;
					merge = false;
				}
			}
			if(merge) {
				if(rowLightmapIdx >= 0 && rowLightmapIdx < 254) {
					if(lightmapIdx < 0) lightmapIdx = rowLightmapIdx;
				}
			}
		}
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			GameObject rowGo = (GameObject)row["gameObject"];
			Mesh mesh = (Mesh)row["mesh"];
			Material mat = (Material)row["mat"];
			int rowLightmapIdx = (int)row["lightmapIdx"];
			bool merge = (bool)row["merge"];
			string submeshName = "";
			if(mat != null) submeshName = mat.name;
			else submeshName = "unknown material";
			if(merge) {
				int j=0;
				for(;j<newSubmeshRows.Count;j++) {
					if(newSubmeshRows[j] == mat) break;
				}
				if(j >= newSubmeshRows.Count) newSubmeshRows.Add(mat);

				for(j=0;j<newMeshRows.Count;j++) {
					if(newMeshRows[j] == mesh) break;
				}
				if(j >= newMeshRows.Count) newMeshRows.Add(mesh);
				mergeCount++;
		    }
   			if(mesh != prevMesh || rowGo != prevGo) {
			    GUILayout.BeginHorizontal();
   				GUILayout.Label(rowGo.name, GUILayout.Width(w[0]));
				GUILayout.Label("" + mesh.vertexCount, GUILayout.Width(w[1]));
				totalVertexCount += mesh.vertexCount;
				totalTriangleCount += (int)row["triangles"];
				if(rowLightmapIdx >= 0 && rowLightmapIdx < 254) GUILayout.Label(""+rowLightmapIdx, GUILayout.Width(w[2]));
				else GUILayout.Label("", GUILayout.Width(w[2]));
				GUILayout.Label("", GUILayout.Width(w[3]));
			    GUILayout.EndHorizontal();
			}
		    GUILayout.BeginHorizontal();
			GUILayout.Label("       " + submeshName, GUILayout.Width(w[0]));
			GUILayout.Label("", titleStyle, GUILayout.Width(w[1]));
			GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
			bool newBool = EditorGUILayout.Toggle("", merge, GUILayout.Width(w[3]));
			if(newBool != merge) {
				row["merge"] = newBool;
				if(lightmapIdx>=0 && rowLightmapIdx != lightmapIdx) {
					if(rowLightmapIdx >= 0 && rowLightmapIdx < 254) lightmapIdx = rowLightmapIdx;
					for(int j=0;j<submeshRows.Count;j++) {
						Hashtable row2 = (Hashtable)submeshRows[j];
						int rowLightmapIdx2 = (int)row2["lightmapIdx"];
						if(lightmapIdx >= 0 && rowLightmapIdx2 == lightmapIdx) row2["merge"] = true;
						else if(lightmapIdx < 0 && (rowLightmapIdx < 0 || rowLightmapIdx >= 254)) row2["merge"] = true;
						else row2["merge"] = false;
					}
				}
			}
		    GUILayout.EndHorizontal();
   			if(i<submeshRows.Count-1 && 
   				((Mesh)submeshRows[i+1]["mesh"] != mesh || 
   				(GameObject)submeshRows[i+1]["gameObject"] != rowGo)) {
   				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1] + w[2]));
			}
			prevMesh = mesh;
			prevGo = rowGo;
		}
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1] + w[2]));
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Total vertices", rightAlignedTitleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("" + totalVertexCount, titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[3]));
	    GUILayout.EndHorizontal();
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Triangles", rightAlignedTitleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("" + totalTriangleCount, titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[3]));
	    GUILayout.EndHorizontal();
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Submeshes", rightAlignedTitleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("" + submeshRows.Count, titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[2]));
		GUILayout.Label("", titleStyle, GUILayout.Width(w[3]));
	    GUILayout.EndHorizontal();
		if(hasLightmaps) GUILayout.Label("\nYou can only merge meshes that share the same lightmap");
		if(mergeCount != submeshRows.Count && !gameObjectNamesUnique) GUILayout.Label("\nThe gameObjects do not have unique names.\nUnless you select all submeshes, you need to fix this first.");
	    GUILayout.EndVertical();

		totalVertexCount = 0;
		for(int i=0;i<newMeshRows.Count;i++) {
			totalVertexCount += newMeshRows[i].vertexCount;
		}

   	    GUILayout.BeginVertical();
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Submeshes in the new mesh", titleStyle, GUILayout.Width(w[0]));
	    GUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0]));

	    if(newSubmeshRows.Count == 0) {
	    	GUILayout.Label("Nothing selected", GUILayout.Width(w[0]));
    	} else {
			for(int i=0;i<newSubmeshRows.Count;i++) {
			    GUILayout.BeginHorizontal();
				Material mat = newSubmeshRows[i];
				string submeshName = "";
				if(mat != null) submeshName = mat.name;
				else submeshName = "unknown material";
				GUILayout.Label(submeshName, GUILayout.Width(w[0]));
			    GUILayout.EndHorizontal();
			}

			if(totalVertexCount > 0 && (mergeCount == submeshRows.Count || gameObjectNamesUnique)) {
				if(makeNewGameObject && totalVertexCount <= 65534) GUILayout.Label("\nA new gameobject will be added", GUILayout.Width(w[0]));
				else if(makeNewGameObject) GUILayout.Label("\nA new gameobject will be added with\nmultiple meshes of max 64K vertices.", GUILayout.Width(w[0]));
				else if(totalVertexCount <= 65534) GUILayout.Label("\nA new renderer will be added to the\ncurrent gameobject", GUILayout.Width(w[0]));
				else GUILayout.Label("\nChild gameobjects will be added for\nmultiple meshes of max 64K vertices", GUILayout.Width(w[0]));

				if(GUILayout.Button("Perform merge", GUILayout.Width(w[0]))) {
					string result = MergeMeshes();
					UnityEngine.Debug.Log("Merge child meshes: " + result);
					EditorUtility.DisplayDialog("Merge child meshes", result, "Close");
					Close();
				}
			}
		}
	    GUILayout.EndVertical();

	    GUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
	}

	private bool TestIfGameObjectNamesAreUnique() {
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			GameObject rowGo = (GameObject)row["gameObject"];
			for(int j=0;j<submeshRows.Count;j++) {
				Hashtable row2 = (Hashtable)submeshRows[j];
				GameObject rowGo2 = (GameObject)row2["gameObject"];
				if(rowGo2 != rowGo && rowGo.name == rowGo2.name) {
					return false;
				}
			}
		}
		return true;
	}

	private string MergeMeshes() {
		List<string> skipSubmeshNames = new List<string>();
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
			if(!(bool)row["merge"]) {
				string submeshName = rowGo.name;
				submeshName = submeshName + "_" + mesh.name;
				submeshName = submeshName + "_" + subMeshIdx;
				skipSubmeshNames.Add(submeshName);
			}
			subMeshIdx++;
		}
		string path = StoragePathUsing1stMeshAndSubPath("MergedMeshes");
		if(path != null && go != null) {
			Mesh[] meshes = null;
			try {
				meshes = go.CombineMeshes(skipSubmeshNames.ToArray());
			} catch(Exception e) {
				Debug.LogError(e);
				return e.Message;
			}
			if(meshes != null && meshes.Length > 0) {
				string str = "";
				for(int i=0;i<meshes.Length;i++) {
					string meshPath = "";
					if(meshes.Length > 1) meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + go.name + "_part" + (i+1) + ".asset");
					else meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + go.name + ".asset");
					AssetDatabase.CreateAsset(meshes[i], meshPath);
					AssetDatabase.SaveAssets();
					str = str + "\n" + meshes[i].vertexCount + " vertices, " + (meshes[i].triangles.Length / 3) + " triangles";
				}
				Resources.UnloadUnusedAssets();
				if(meshes.Length > 1) {
					return "Merged meshes were saved under "+(path + "/" + go.name)+"." + str;
				} else {
					return "Merged mesh was saved under "+(path + "/" + go.name)+"." + str;
				}
			}
		}
		return "No mesh found in gameobject";
	}

	private string StoragePathUsing1stMeshAndSubPath(string subPath) {
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