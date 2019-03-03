using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class VertexPainter : EditorWindow
{
	bool drawing = false;
	bool showVertexColors = false;
	bool showFlowMap = false;
	GameObject currentDrawObject;

	List<MeshFilter> meshFilters = new List<MeshFilter> ();
	List<Material> oldMaterials = new List<Material> ();

	int toolbarInt = 0;
	Color drawColor = Color.white;
	float drawSize = 0.5f;
	float opacity = 1f;

	public float flowSpeed = 1f;
	public float flowDirection = 0f;

	public string[] toolbarStrings = new string[] {
		"Vertex Color",
		"Flow Map\n[BETA]"
	};

	[MenuItem ("Tools/Nature Manufacture/Vertex Painter")]
	static void Init ()
	{
		// Get existing open window or if none, make a new one:
		VertexPainter window = EditorWindow.GetWindow<VertexPainter> ("VertexPainter");
		window.Show ();
	}

	void OnGUI ()
	{
		toolbarInt = GUILayout.Toolbar (toolbarInt, toolbarStrings);

		if (!drawing) {
			if (GUILayout.Button ("Start Drawing")) {
				if (Selection.activeGameObject != null) {
					EditorSceneManager.MarkSceneDirty (SceneManager.GetActiveScene ());
					meshFilters.Clear ();
					MeshFilter[] filters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter> ();

					foreach (var item in filters) {
						currentDrawObject = Selection.activeGameObject;
						meshFilters.Add (item);

						drawing = true;
						Tools.current = Tool.None;

						Undo.RecordObject (item.sharedMesh, "Start draw vertex");
					}

				}
			}
		}

		if (drawing) {

			if (Selection.activeGameObject != currentDrawObject) {
				StopDrawing ();
			}

			if (GUILayout.Button ("End Drawing")) {
				StopDrawing ();


			}

			EditorGUILayout.Space ();

			if (toolbarInt == 0) {
				if (!showVertexColors) {
					if (GUILayout.Button ("Show vertex colors")) {
						if (!showFlowMap && !showVertexColors)
							oldMaterials.Clear ();


						for (int i = 0; i < meshFilters.Count; i++) {
							if (!showFlowMap && !showVertexColors)
								oldMaterials.Add (meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial);
							meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial = new Material (Shader.Find ("NatureManufacture Shaders/Debug/Vertex color"));
						}
						ResetMaterial ();
						showVertexColors = true;
					}
				} else {
					if (GUILayout.Button ("Hide vertex colors")) {
						ResetMaterial ();
						for (int i = 0; i < meshFilters.Count; i++) {
							meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial = oldMaterials [i];
						}
						showVertexColors = false;
					}
				}
				if (GUILayout.Button ("Reset colors")) {
					RestartColor ();
				}
				EditorGUILayout.HelpBox ("River Auto Material -> R Wetness", MessageType.Info);
			} else {
				if (!showFlowMap) {
					if (GUILayout.Button ("Show  flow directions")) {
						if (!showFlowMap && !showVertexColors)
							oldMaterials.Clear ();


						for (int i = 0; i < meshFilters.Count; i++) {
							if (!showFlowMap && !showVertexColors)
								oldMaterials.Add (meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial);
							
							meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial = new Material (Shader.Find ("NatureManufacture Shaders/Debug/Flowmap Direction"));
							meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial.SetTexture ("_Direction", Resources.Load<Texture2D> ("Debug_Arrow"));
						}
						ResetMaterial ();
						showFlowMap = true;
					}
				} else {
					if (GUILayout.Button ("Hide flow directions")) {
						ResetMaterial ();
						for (int i = 0; i < meshFilters.Count; i++) {
							meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial = oldMaterials [i];
						}
						showFlowMap = false;
					}
				}
				if (GUILayout.Button ("Reset flow")) {
					RestartFlow ();
				}
			}



		
		}	

		if (toolbarInt == 0) {



			drawColor = EditorGUILayout.ColorField ("Draw color", drawColor);
			opacity = EditorGUILayout.FloatField ("Opacity", opacity);
			drawSize = EditorGUILayout.FloatField ("Size", drawSize);
			if (drawSize < 0) {
				drawSize = 0;
			}
			EditorGUILayout.HelpBox ("R - Emission G- Bottom Cover B - Top Cover\n" +
                "Paint  - Left Mouse Button Click \n" +
                "Clean  - SHIFT + Left Mouse Button Click \n", MessageType.Info);
			EditorGUILayout.Space ();
		} else {
			flowSpeed = EditorGUILayout.Slider ("Flow U Speed", flowSpeed, -1, 1);
			flowDirection = EditorGUILayout.Slider ("Flow V Speed", flowDirection, -1, 1);
			opacity = EditorGUILayout.FloatField ("Opacity", opacity);
			drawSize = EditorGUILayout.FloatField ("Size", drawSize);
			if (drawSize < 0) {
				drawSize = 0;
			}
		}




	
	}

	void ResetMaterial ()
	{

		showFlowMap = false;
		showVertexColors = false;
	}

	void StopDrawing ()
	{
		
		if (showVertexColors) {
			for (int i = 0; i < meshFilters.Count; i++) {
				meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial = oldMaterials [i];
			}
			showVertexColors = false;
		}
        
		if (showFlowMap) {
            for (int i = 0; i < meshFilters.Count; i++) {
                
                meshFilters [i].GetComponent<MeshRenderer> ().sharedMaterial = oldMaterials [i];
			}
			showFlowMap = false;
		}
		currentDrawObject = null;

		drawing = false;
        meshFilters.Clear();

    }

	protected virtual void OnSceneGUI (SceneView sceneView)
	{
		
		if (Selection.activeGameObject != currentDrawObject && drawing) {
			StopDrawing ();
		}
		
		Color baseColor = Handles.color;


		if (currentDrawObject != null) {


			if (drawing) {
				if (toolbarInt == 0)
					DrawOnVertexColors ();
				else
					DrawOnFlowMap ();
			}
		}
	}

	void RestartColor ()
	{
		foreach (var item in meshFilters) {
			Mesh mesh = item.sharedMesh; 
			if (!string.IsNullOrEmpty (AssetDatabase.GetAssetPath (mesh))) {
				mesh = Instantiate<Mesh> (item.sharedMesh);
				item.sharedMesh = mesh;

			}

			mesh.colors = null;

		}
	}

	void RestartFlow ()
	{
		foreach (var item in meshFilters) {
			Mesh mesh = item.sharedMesh; 
			if (!string.IsNullOrEmpty (AssetDatabase.GetAssetPath (mesh))) {
				mesh = Instantiate<Mesh> (item.sharedMesh);
				item.sharedMesh = mesh;

			}

			mesh.uv4 = null;

		}
	}

	void DrawOnVertexColors ()
	{

		HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));



		Camera sceneCamera = SceneView.lastActiveSceneView.camera;
		Vector2 mousePos = Event.current.mousePosition;
		mousePos.y = Screen.height - mousePos.y - 40;
		Ray ray = sceneCamera.ScreenPointToRay (mousePos);

		List<MeshCollider> meshColliders = new List<MeshCollider> ();
		foreach (var item in meshFilters) {
			meshColliders.Add (item.gameObject.AddComponent<MeshCollider> ());
		}

		RaycastHit[] hits = Physics.RaycastAll (ray, Mathf.Infinity);

		Vector3 hitPosition = Vector3.zero;
		Vector3 hitNormal = Vector3.zero;
		if (hits.Length > 0) {

			foreach (var hit in hits) {
				
				if (hit.collider is MeshCollider) {
					MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter> ();

					hitPosition = hit.point;
					hitNormal = hit.normal;

					Handles.color = new Color (drawColor.r, drawColor.g, drawColor.b, 1);
					Handles.DrawLine (hitPosition, hitPosition + hitNormal * 2);
					Handles.CircleHandleCap (
						GUIUtility.GetControlID (FocusType.Passive),
						hitPosition,
						Quaternion.LookRotation (hitNormal),
						drawSize,
						EventType.Repaint
					);
					Handles.color = Color.black;
					Handles.CircleHandleCap (
						GUIUtility.GetControlID (FocusType.Passive),
						hitPosition,
						Quaternion.LookRotation (hitNormal),
						drawSize * 0.95f,
						EventType.Repaint
					);

					foreach (var currentMeshFilter in meshFilters) {
											
						if (meshFilter == currentMeshFilter) {
							

						

							if (!(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) || Event.current.button != 0)
								continue;


							if (meshFilter.sharedMesh != null) {
								


								Mesh mesh = meshFilter.sharedMesh; 
								if (!string.IsNullOrEmpty (AssetDatabase.GetAssetPath (mesh))) {
									mesh = Instantiate<Mesh> (meshFilter.sharedMesh);
									meshFilter.sharedMesh = mesh;

								}

								int vertLength = mesh.vertices.Length;
								Vector3[] vertices = mesh.vertices;
								Color[] colors = mesh.colors;
								Transform transform = meshFilter.transform;
								if (colors.Length == 0) {
									colors = new Color[vertLength];
									for (int i = 0; i < colors.Length; i++) {
										colors [i] = Color.white;
									}

								}



								for (int i = 0; i < vertLength; i++) {
									
									float dist = Vector3.Distance (hitPosition, transform.TransformPoint (vertices [i]));

									if (dist < drawSize) {
										
										if (Event.current.shift)
											colors [i] = Color.Lerp (colors [i], Color.white, opacity);
										else
											colors [i] = Color.Lerp (colors [i], drawColor, opacity);					

									}
								}


								mesh.colors = colors;

							}
						} 
					}
				}
			}
		}

		foreach (var item in meshColliders) {
			DestroyImmediate (item);
		}

	}

	void DrawOnFlowMap ()
	{

	
		HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));



		Camera sceneCamera = SceneView.lastActiveSceneView.camera;
		Vector2 mousePos = Event.current.mousePosition;
		mousePos.y = Screen.height - mousePos.y - 40;
		Ray ray = sceneCamera.ScreenPointToRay (mousePos);

		List<MeshCollider> meshColliders = new List<MeshCollider> ();
		foreach (var item in meshFilters) {
			meshColliders.Add (item.gameObject.AddComponent<MeshCollider> ());
		}

		RaycastHit[] hits = Physics.RaycastAll (ray, Mathf.Infinity);

		Vector3 hitPosition = Vector3.zero;
		Vector3 hitNormal = Vector3.zero;


		if (hits.Length > 0) {

			foreach (var hit in hits) {

				if (hit.collider is MeshCollider) {
					MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter> ();

					hitPosition = hit.point;
					hitNormal = hit.normal;

					Handles.color = new Color (drawColor.r, drawColor.g, drawColor.b, 1);
					Handles.DrawLine (hitPosition, hitPosition + hitNormal * 2);
					Handles.CircleHandleCap (
						GUIUtility.GetControlID (FocusType.Passive),
						hitPosition,
						Quaternion.LookRotation (hitNormal),
						drawSize,
						EventType.Repaint
					);
					Handles.color = Color.black;
					Handles.CircleHandleCap (
						GUIUtility.GetControlID (FocusType.Passive),
						hitPosition,
						Quaternion.LookRotation (hitNormal),
						drawSize * 0.95f,
						EventType.Repaint
					);

					foreach (var currentMeshFilter in meshFilters) {

						if (meshFilter == currentMeshFilter) {


							if (!(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) || Event.current.button != 0)
								continue;



						
							if (meshFilter.sharedMesh != null) {



								Mesh mesh = meshFilter.sharedMesh; 
								if (!string.IsNullOrEmpty (AssetDatabase.GetAssetPath (mesh))) {
									mesh = Instantiate<Mesh> (meshFilter.sharedMesh);
									meshFilter.sharedMesh = mesh;

								}

								int vertLength = mesh.vertices.Length;
								Vector3[] vertices = mesh.vertices;
								Vector2[] colorsFlowMap = mesh.uv4;

								Transform transform = meshFilter.transform;
								if (colorsFlowMap.Length == 0) {
									colorsFlowMap = new Vector2[vertLength];
									for (int i = 0; i < colorsFlowMap.Length; i++) {
										colorsFlowMap [i] = new Vector2 (0, 0);
									}

								}


								float dist = 0;
								float distValue = 0;
								for (int i = 0; i < vertLength; i++) {

									dist = Vector3.Distance (hitPosition, transform.TransformPoint (vertices [i]));


									if (dist < drawSize) {
										distValue = (drawSize - dist) / (float)drawSize;
										if (Event.current.shift) {
											colorsFlowMap [i] = Vector2.Lerp (colorsFlowMap [i], new Vector2 (0, 0), opacity);

										} else {
											colorsFlowMap [i] = Vector2.Lerp (colorsFlowMap [i], new Vector2 (flowDirection, flowSpeed), opacity * distValue);

										}

									}
								}


								mesh.uv4 = colorsFlowMap;

							}
						}
					}
				}
			}
		}

		foreach (var item in meshColliders) {
			DestroyImmediate (item);
		}
	}

	void OnFocus ()
	{
		
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}

	void OnDestroy ()
	{
		
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		if (drawing)
			StopDrawing ();
		
	}
}
