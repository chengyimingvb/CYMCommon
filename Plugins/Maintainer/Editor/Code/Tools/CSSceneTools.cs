#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeStage.Maintainer.Tools
{
	public class CSSceneTools
	{
		public class OpenSceneResult
		{
			public bool success;
			public bool sceneWasLoaded;
			public bool sceneWasAdded;
			public Scene scene;
		}

		public static string GetCurrentScenePath()
		{
			return SceneManager.GetActiveScene().path;
		}

		public static Scene GetSceneByPath(string path)
		{
			return SceneManager.GetSceneByPath(path);
		}

		public static void NewScene(bool empty = false, bool additive = false)
		{
			EditorSceneManager.NewScene(empty ? NewSceneSetup.EmptyScene : NewSceneSetup.DefaultGameObjects, additive ? NewSceneMode.Additive : NewSceneMode.Single);
		}

		public static OpenSceneResult OpenSceneWithSavePrompt(string path, bool activate = true)
		{
			var result = new OpenSceneResult();

			var targetScene = SceneManager.GetSceneByPath(path);
			if (targetScene == SceneManager.GetActiveScene())
			{
				result.scene = targetScene;
				result.success = true;
				return result;
			}

			if (!SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return result;
			}

			return OpenScene(path, activate);
		}

		public static OpenSceneResult OpenScene(string path, bool activate = true)
		{
			var result = new OpenSceneResult();
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError(Maintainer.ConstructError("Can't open scene since path is absent!"));
				return result;
			}

			var targetScene = SceneManager.GetSceneByPath(path);
			result.scene = targetScene;

			if (targetScene == SceneManager.GetActiveScene())
			{
				result.success = true;
				return result;
			}

			if (!targetScene.isLoaded)
			{
				result.sceneWasAdded = EditorSceneManager.GetSceneManagerSetup().All(s => s.path != targetScene.path);
				targetScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

				result.scene = targetScene;

				if (!targetScene.IsValid())
				{
					Debug.LogError(Maintainer.ConstructError("Can't open scene since path leads to invalid scene!"));
					return result;
				}
				result.sceneWasLoaded = true;
			}

			result.success = true;

			if (activate)
			{
				SceneManager.SetActiveScene(targetScene);
			}

			return result;
		}

		public static void CloseOpenedSceneIfNeeded(OpenSceneResult openSceneResult)
		{
			if (openSceneResult != null && openSceneResult.sceneWasLoaded && EditorSceneManager.loadedSceneCount > 1)
			{
				EditorSceneManager.CloseScene(openSceneResult.scene, openSceneResult.sceneWasAdded);
			}
		}

		public static bool SaveCurrentModifiedScenesIfUserWantsTo()
		{
			return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
		}

		public static void EnsureUntitledSceneHasBeenSaved(string title)
		{
#if UNITY_5_6_OR_NEWER
			EditorSceneManager.EnsureUntitledSceneHasBeenSaved(title);
#else
			var untitledScene = default(Scene);

			for (var i = 0; i < EditorSceneManager.loadedSceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.IsValid()) continue;
				if (!scene.isDirty) continue;

				if (string.IsNullOrEmpty(scene.name) && string.IsNullOrEmpty(scene.path))
				{
					untitledScene = scene;
					break;
				}
			}

			if (untitledScene.IsValid())
			{
				if (EditorUtility.DisplayDialog("Unsaved 'Untitled' scene found", title, "Yes", "Cancel"))
				{
					EditorSceneManager.SaveScene(untitledScene);
				}
			}
#endif
		}

		public static bool SaveOpenScenes()
		{
			return EditorSceneManager.SaveOpenScenes();
		}

		public static void SaveScene(Scene scene)
		{
			EditorSceneManager.SaveScene(scene);
		}

		public static void SaveActiveScene()
		{
			SaveScene(SceneManager.GetActiveScene());
		}

		public static string[] GetScenesInBuild(bool includeDisabled = false)
		{
			var scenesForBuild = EditorBuildSettings.scenes;
			var scenesInBuild = new List<string>(scenesForBuild.Length);

			foreach (var sceneInBuild in scenesForBuild)
			{
				if (sceneInBuild.enabled || includeDisabled)
				{
					scenesInBuild.Add(sceneInBuild.path);
				}
			}
			return scenesInBuild.ToArray();
		}


		public static void MarkSceneDirty()
		{
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}


		public static bool CurrentSceneIsDirty()
		{
			return SceneManager.GetActiveScene().isDirty;
		}

		public static SceneSetup[] GetScenesSetup()
		{
			var scenesSetup = EditorSceneManager.GetSceneManagerSetup();
			if (scenesSetup != null && scenesSetup.Length > 0 && !scenesSetup.Any(s => s.isActive))
			{
				var firstLoaded = scenesSetup.FirstOrDefault(s => s.isLoaded);
				if (firstLoaded != null)
				{
					firstLoaded.isActive = true;
				}
				else
				{
					scenesSetup[0].isActive = true;
					scenesSetup[0].isLoaded = true;
				}
			}
			return scenesSetup;
		}

		public static void SetScenesSetup(SceneSetup[] scenesSetup)
		{
			EditorSceneManager.RestoreSceneManagerSetup(scenesSetup);
		}

		public static void ReSaveAllScenes()
		{
			EditorUtility.DisplayProgressBar("Re-saving scenes...", "Looking for scenes...", 0);
			var allScenesGuids = AssetDatabase.FindAssets("t:Scene");

			for (var i = 0; i < allScenesGuids.Length; i++)
			{
				var guid = allScenesGuids[i];
				EditorUtility.DisplayProgressBar("Re-saving scenes...", string.Format("Scene {0} of {1}", i + 1, allScenesGuids.Length), (float)i / allScenesGuids.Length);

				var scenePath = AssetDatabase.GUIDToAssetPath(guid);

				var result = OpenScene(scenePath);

				EditorSceneManager.MarkSceneDirty(result.scene);
				EditorSceneManager.SaveScene(result.scene);

				CloseOpenedSceneIfNeeded(result);
			}

			EditorUtility.ClearProgressBar();
		}
	}
}