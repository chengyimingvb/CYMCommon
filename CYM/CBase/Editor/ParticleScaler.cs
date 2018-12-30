//	Simple Particle Scaler v1.5
//	Copyright Unluck Software	
//	www.chemicalbliss.com																			

using UnityEngine;
using UnityEditor;
[System.Serializable]

public class ParticleScaler : EditorWindow {
	public float scaleMultiplier = 1.0f;
	public float originalScale = 1.0f;
	public bool autoRename;
	public string titleTex =
		"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"
		;
	public Texture2D tex;

	[MenuItem("Window/Simple Particle Scaler")]

	public static void ShowWindow() {
		EditorWindow win = EditorWindow.GetWindow(typeof(ParticleScaler));
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9 || UNITY_2017
		win.titleContent = new GUIContent("Simple Particle Scaler");
#else
			win.title = "Simple Particle Scaler";	
#endif
		win.minSize = new Vector2(200.0f, 130.0f);
		win.maxSize = new Vector2(200.0f, 130.0f);
	}

	public void OnEnable() {
		if (tex != null) return;
		byte[] b64_bytes = System.Convert.FromBase64String(titleTex);
		tex = new Texture2D(1, 1);
		tex.LoadImage(b64_bytes);
	}

	public void OnGUI() {
		//tex = Resources.Load("tex") as Texture2D;
		if (tex != null)
			GUI.DrawTexture(new Rect(position.width * .5f - 100, 0, 200, 22), tex, ScaleMode.ScaleToFit, true, 0);
		GUILayout.Space(20);
		Color32 colorBlueLight = new Color32((byte)200, (byte)255, (byte)255, (byte)255);
		GUIStyle styleBigButton = null;
		styleBigButton = new GUIStyle(GUI.skin.button);
		styleBigButton.fixedWidth = 90.0f;
		styleBigButton.fixedHeight = 20.0f;
		styleBigButton.fontSize = 9;
		GUIStyle styleToggle = null;
		styleToggle = new GUIStyle(GUI.skin.toggle);
		styleToggle.fontSize = 9;
		GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
		titleStyle.fixedWidth = 200.0f;
		EditorGUILayout.Space();
		scaleMultiplier = EditorGUILayout.Slider(scaleMultiplier, 0.01f, 4.0f);
		EditorGUILayout.Space();
		GUI.color = colorBlueLight;
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Scale", styleBigButton)) {
			foreach (GameObject gameObj in Selection.gameObjects) {
				if (autoRename) {
					string[] s = gameObj.name.Split('¤');
					if (s.Length == 1) {
						gameObj.name += " ¤" + scaleMultiplier;
					} else {
						float i = float.Parse(s[s.Length - 1]);
						gameObj.name = s[0] + "¤" + scaleMultiplier * i;
					}
				}
				ParticleSystem[] pss = null;
				pss = gameObj.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem ps in pss) {
					ps.Stop();
					ScaleParticles(gameObj, ps);
					ps.Play();
				}
			}
		}
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Save Prefabs", styleBigButton)) {
			CreatePrefabs();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("", GUILayout.Width(10.0f));
		autoRename = GUILayout.Toggle(autoRename, "Automatic rename", styleToggle);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		GUI.color = colorBlueLight;
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Play", EditorStyles.miniButtonLeft)) {
			ParticleCalls("Play");
		}
		if (GUILayout.Button("Pause", EditorStyles.miniButtonMid)) {
			ParticleCalls("Pause");
		}
		if (GUILayout.Button("Stop", EditorStyles.miniButtonRight)) {
			ParticleCalls("Stop");
		}
		EditorGUILayout.EndHorizontal();
	}

	public void CreatePrefabs() {
		if (Selection.gameObjects.Length > 0) {
			string path = EditorUtility.SaveFolderPanel("Select Folder ", "Assets/", "");
			if (path.Length > 0) {
				if (path.Contains("" + Application.dataPath)) {
					string s = "" + path + "/";
					string d = "" + Application.dataPath + "/";
					string p = "Assets/" + s.Remove(0, d.Length);
					GameObject[] objs = Selection.gameObjects;
					bool cancel = false;
					foreach (GameObject go in objs) {
						if (!cancel) {
							if (AssetDatabase.LoadAssetAtPath(p + go.gameObject.name + ".prefab", typeof(GameObject)) != null) {
								int option = EditorUtility.DisplayDialogComplex("Are you sure?", "" + go.gameObject.name + ".prefab" + " already exists. Do you want to overwrite it?", "Yes", "No", "Cancel");
								switch (option) {
									case 0:
									CreateNew(go.gameObject, p + go.gameObject.name + ".prefab");
									goto case 1;
									case 1:
									break;
									case 2:
									cancel = true;
									break;
									default:
									Debug.LogError("Unrecognized option.");
									break;
								}
							} else CreateNew(go.gameObject, p + go.gameObject.name + ".prefab");
						}
					}
				} else {
					Debug.LogError("Prefab Save Failed: Can't save outside project: " + path);
				}
			}
		} else {
			Debug.LogWarning("No GameObjects Selected");
		}
	}

	public static void CreateNew(GameObject obj, string localPath) {
		Object prefab = PrefabUtility.CreateEmptyPrefab(localPath);
		PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
	}

	public void UpdateParticles() {
		foreach (GameObject gameObj in Selection.gameObjects) {
			ParticleSystem[] pss = null;
			pss = gameObj.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem ps in pss) {
				ps.Stop();
				ps.Play();
			}
		}
	}

	public void ParticleCalls(string call) {
		foreach (GameObject gameObj in Selection.gameObjects) {
			ParticleSystem[] pss = null;
			pss = gameObj.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem ps in pss) {
				if (call == "Pause") ps.Pause();
				else if (call == "Play") ps.Play();
				else if (call == "Stop") {
					ps.Stop();
					ps.Clear();
				}
			}
		}
	}


	public void ScaleParticles(GameObject __parent_cs1, ParticleSystem __particles_cs1) {

		if (__parent_cs1 != __particles_cs1.gameObject) {
			__particles_cs1.transform.localPosition *= scaleMultiplier;
		}
		SerializedObject serializedParticles = new SerializedObject(__particles_cs1);

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
		serializedParticles.FindProperty("InitialModule.gravityModifier").floatValue *= scaleMultiplier;
#else
		serializedParticles.FindProperty("InitialModule.gravityModifier.scalar").floatValue *= scaleMultiplier;
#endif

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4
#else
		serializedParticles.FindProperty("NoiseModule.strength.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("LightsModule.rangeCurve.scalar").floatValue *= scaleMultiplier;
#endif

		serializedParticles.FindProperty("InitialModule.startSize.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("InitialModule.startSpeed.scalar").floatValue *= scaleMultiplier;
#if UNITY_5
		serializedParticles.FindProperty("ShapeModule.boxX").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ShapeModule.boxY").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ShapeModule.boxZ").floatValue *= scaleMultiplier;
#else
		serializedParticles.FindProperty("ShapeModule.m_Scale").vector3Value *= scaleMultiplier;
#endif

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5
		serializedParticles.FindProperty("ShapeModule.radius").floatValue *= scaleMultiplier;
#else
		serializedParticles.FindProperty("ShapeModule.radius.value").floatValue *= scaleMultiplier;
#endif

		serializedParticles.FindProperty("VelocityModule.x.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("VelocityModule.y.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("VelocityModule.z.scalar").floatValue *= scaleMultiplier;
		ScaleCurve(serializedParticles.FindProperty("VelocityModule.x.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("VelocityModule.x.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("VelocityModule.y.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("VelocityModule.y.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("VelocityModule.z.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("VelocityModule.z.maxCurve").animationCurveValue);
		serializedParticles.FindProperty("ClampVelocityModule.x.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ClampVelocityModule.y.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ClampVelocityModule.z.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= scaleMultiplier;
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.x.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.x.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.y.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.y.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.z.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.z.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.magnitude.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ClampVelocityModule.magnitude.maxCurve").animationCurveValue);
		serializedParticles.FindProperty("ForceModule.x.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ForceModule.y.scalar").floatValue *= scaleMultiplier;
		serializedParticles.FindProperty("ForceModule.z.scalar").floatValue *= scaleMultiplier;
		ScaleCurve(serializedParticles.FindProperty("ForceModule.x.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ForceModule.x.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ForceModule.y.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ForceModule.y.maxCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ForceModule.z.minCurve").animationCurveValue);
		ScaleCurve(serializedParticles.FindProperty("ForceModule.z.maxCurve").animationCurveValue);
		serializedParticles.ApplyModifiedProperties();
	}

	public void ScaleCurve(AnimationCurve curve) {
		for (int i = 0; i < curve.keys.Length; i++) {
			var tmp_cs1 = curve.keys[i];
			tmp_cs1.value *= scaleMultiplier;
			curve.keys[i] = tmp_cs1;
		}
	}
}