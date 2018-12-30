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


public class SimpleLOD_RemovePopup : EditorWindow {
	private GameObject go;
	private List<Hashtable> submeshRows;
	private Vector2 scrollPos = Vector2.zero;

	public void OpenWindow(GameObject aGO) {
		go = aGO;
		Init();

		this.position = new Rect((Screen.width/2)+200, (Screen.height/2)+50, 370, 500);
		this.minSize = new Vector3(370,200);
		this.maxSize = new Vector3(370,1000);
		#if UNITY_4_3
			this.title = "Del submesh";
		#elif UNITY_4_4
			this.title = "Del submesh";
		#elif UNITY_4_5
			this.title = "Del submesh";
		#elif UNITY_4_6
			this.title = "Del submesh";
		#elif UNITY_5_0
			this.title = "Del submesh";
		#else
			this.titleContent = new GUIContent("Del submesh");
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
						row["triangles"] = mesh.triangles.Length / 3;
						row["submeshIdx"] = i;
						row["remove"] = false;
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
						row["triangles"] = mesh.triangles.Length / 3;
						row["submeshIdx"] = i;
						row["remove"] = false;
						submeshRows.Add(row);
					}
				}
			}
		}

		if(submeshRows.Count <= 0) {
			if(go.activeSelf) EditorUtility.DisplayDialog("Remove submesh window", "This game object has no meshes.", "OK");
			else EditorUtility.DisplayDialog("Remove submesh window", "This game object is deactivated.", "OK");
			Close();
			return;
		}
	}

	public new void Close() {
		go = null;
		submeshRows = null;
		Resources.UnloadUnusedAssets();
		base.Close();
	}

	void OnGUI() {
		if(go == null) Close();
		float[] w = new float[] {225f, 50f, 60f};
		int totalVertexCount = 0;
		int totalTriangleCount = 0;
		int removeCount = 0;
		GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
		titleStyle.fontStyle = FontStyle.Bold;
		GUIStyle rightAlignedTitleStyle = new GUIStyle(titleStyle);
		rightAlignedTitleStyle.alignment = TextAnchor.MiddleRight;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));


		GUILayout.Label("\nSelect the submeshes that you want to remove\n");
	    GUILayout.BeginHorizontal();
		GUILayout.Label("Mesh / submesh", titleStyle, GUILayout.Width(w[0]));
		GUILayout.Label("Vertices", titleStyle, GUILayout.Width(w[1]));
		GUILayout.Label("Remove", titleStyle, GUILayout.Width(w[2]));
	    GUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1] + w[2]));

	    Mesh prevMesh = null;
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			Mesh mesh = (Mesh)row["mesh"];
			Material mat = (Material)row["mat"];
			bool remove = (bool)row["remove"];
			string submeshName = "";
			if(mat != null) submeshName = mat.name;
			else submeshName = "unknown material";

   			if(mesh != prevMesh) {
			    GUILayout.BeginHorizontal();
   				GUILayout.Label(mesh.name, GUILayout.Width(w[0]));
				GUILayout.Label("" + mesh.vertexCount, GUILayout.Width(w[1]));
				totalVertexCount += mesh.vertexCount;
				totalTriangleCount += (int)row["triangles"];
				GUILayout.Label("", GUILayout.Width(w[2]));
			    GUILayout.EndHorizontal();
			}
		    GUILayout.BeginHorizontal();
			GUILayout.Label("       " + submeshName, GUILayout.Width(w[0]));
			GUILayout.Label("", GUILayout.Width(w[1]));
			bool newBool = EditorGUILayout.Toggle("", remove, GUILayout.Width(w[2]));
			if(newBool != remove) {
				row["remove"] = newBool;
			}
			if(newBool) removeCount++;
		    GUILayout.EndHorizontal();

   			if(i<submeshRows.Count-1 && (Mesh)submeshRows[i+1]["mesh"] != mesh) {
   				GUILayout.Box("", GUILayout.Height(1), GUILayout.Width(w[0] + w[1]));
			}
			prevMesh = mesh;
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

		GUILayout.Label("Select the submeshes you do not want to include");
		if(removeCount > 0 && removeCount < submeshRows.Count) {
			GUILayout.Label("New mesh(es) will be baked that do not contain the selected\nsubmeshes. The current meshes will not be touched.");
			if(GUILayout.Button("Bake new mesh(es)")) {
				string result = RemoveSubmeshes();
				if(result == null || result.Length <= 0) EditorUtility.DisplayDialog("Remove submeshes", "Your mesh(es) were saved in the subpath ./CleanedMeshes", "Close");
				else EditorUtility.DisplayDialog("Remove submeshes", result, "Close");
			}
		}
		GUILayout.Label("");

		EditorGUILayout.EndScrollView();
	}

	private string RemoveSubmeshes() {
		GameObject prevGo = null;
		GameObject rowGo = null;
		List<int> subMeshesToRemove = new List<int>();
		int subMeshIdx = 0;
		string result = "";
		for(int i=0;i<submeshRows.Count;i++) {
			Hashtable row = (Hashtable)submeshRows[i];
			rowGo = (GameObject)row["gameObject"];
			if(prevGo != rowGo) {
				if(subMeshesToRemove.Count > 0) {
					if(result.Length > 0) result = result + "\n";
					result = result + RemoveSubmeshes(prevGo, subMeshesToRemove.ToArray());
				}
				subMeshesToRemove = new List<int>();
				subMeshIdx = 0;
				prevGo = rowGo;
			}
			if((bool)row["remove"]) subMeshesToRemove.Add(subMeshIdx);
			subMeshIdx++;
		}
		if(subMeshesToRemove.Count > 0) {
			if(result.Length > 0) result = result + "\n";
			result = result + RemoveSubmeshes(rowGo, subMeshesToRemove.ToArray());
		}
		Init();
		return result;
	}

	private string RemoveSubmeshes(GameObject aGO, int[] submeshesToRemove) {
		if(aGO == null || submeshesToRemove == null || submeshesToRemove.Length <= 0) return "";

		Debug.Log("RemoveSubmeshes:" + aGO.name+" submeshesToRemove:"+submeshesToRemove);
		string path = StoragePathUsing1stMeshAndSubPath(aGO, "CleanedMeshes");
		if(path != null) {
			List<Material> mats = new List<Material>();
			Mesh origMesh = null;
			MeshFilter mf = aGO.GetComponent<MeshFilter>();
			MeshRenderer mr = aGO.GetComponent<MeshRenderer>();
			SkinnedMeshRenderer smr = aGO.GetComponent<SkinnedMeshRenderer>();
			if(smr != null) {
				origMesh = smr.sharedMesh;
				mats.AddRange(smr.sharedMaterials);
			} else if(mf != null && mr != null) {
				origMesh = mf.sharedMesh;
				mats.AddRange(mr.sharedMaterials);
			}
			if(origMesh != null) {
				Mesh mesh = null;
				try {
					mesh = origMesh.CopyAndRemoveSubmeshes(submeshesToRemove);
				} catch(Exception e) {
					Debug.LogError(e);
					return e.Message;
				}
				if(mesh != null) {
					string str = "";
					string meshPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + origMesh.name + ".asset");
					AssetDatabase.CreateAsset(mesh, meshPath);
					AssetDatabase.SaveAssets();
					str = mesh.vertexCount + " vertices, " + (mesh.triangles.Length / 3) + " triangles";
					Resources.UnloadUnusedAssets();

					for(int i=submeshesToRemove.Length-1;i>=0;i--) {
						int idx = submeshesToRemove[i];
						if(mats.Count > idx) mats.RemoveAt(idx);
					}
					if(smr != null) {
						smr.sharedMesh = mesh;
						smr.sharedMaterials = mats.ToArray();
					} else if(mf != null) {
						mf.sharedMesh = mesh;
						mr.sharedMaterials = mats.ToArray();
					}
					return str + " saved under "+(path + "/" + origMesh.name)+".";
				}
			}
		}
		return "No mesh";
	}

	private string StoragePathUsing1stMeshAndSubPath(GameObject aGO, string subPath) {
		Mesh firstMesh = aGO.Get1stSharedMesh();
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