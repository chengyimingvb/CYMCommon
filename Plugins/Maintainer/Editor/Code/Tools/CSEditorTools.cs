#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CodeStage.Maintainer.Core;
using TreeEditor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Tools
{
	public class CSEditorTools
	{
		private static readonly int assetsFolderIndex = Application.dataPath.IndexOf("/Assets", StringComparison.Ordinal);
		private static readonly string[] sizes = { "B", "KB", "MB", "GB" };
		private static TextInfo textInfo;

		internal static CSSceneTools.OpenSceneResult lastRevealSceneOpenResult;

		public static string FormatBytes(double bytes)
		{
			var order = 0;

			// 4 - sizes.Length
			while (bytes >= 1024 && order + 1 < 4)
			{
				order++;
				bytes = bytes / 1024;
			}

			// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
			// show a single decimal place, and no space.
			return string.Format("{0:0.##} {1}", bytes, sizes[order]);
		}

		public static int GetPropertyHash(SerializedProperty sp)
		{
			/*Debug.Log("Property: " + sp.name);
			Debug.Log("sp.propertyType = " + sp.propertyType);*/
			var stringHash = new StringBuilder();

			stringHash.Append(sp.type);

			if (sp.isArray)
			{
				stringHash.Append(sp.arraySize);
			}
			else
				switch (sp.propertyType)
				{
					case SerializedPropertyType.AnimationCurve:
						if (sp.animationCurveValue != null)
						{
							stringHash.Append(sp.animationCurveValue.length);
							if (sp.animationCurveValue.keys != null)
							{
								foreach (var key in sp.animationCurveValue.keys)
								{
									stringHash.Append(key.value)
											  .Append(key.time)
#if !UNITY_2018_1_OR_NEWER
											  .Append(key.tangentMode)
#endif
											  .Append(key.outTangent)
											  .Append(key.inTangent);
								}
							}
						}

						break;
					case SerializedPropertyType.ArraySize:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Boolean:
						stringHash.Append(sp.boolValue);
						break;
					case SerializedPropertyType.Bounds:
						stringHash.Append(sp.boundsValue.center)
								  .Append(sp.boundsValue.extents);
						break;
					case SerializedPropertyType.Character:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Generic: // looks like arrays which we already walk through
						break;
					case SerializedPropertyType.Gradient: // unsupported
						break;
					case SerializedPropertyType.ObjectReference:
						if (sp.objectReferenceValue != null)
						{
							stringHash.Append(sp.objectReferenceValue.name);
						}
						break;
					case SerializedPropertyType.Color:
						stringHash.Append(sp.colorValue);
						break;
					case SerializedPropertyType.Enum:
						stringHash.Append(sp.enumValueIndex);
						break;
					case SerializedPropertyType.Float:
						stringHash.Append(sp.floatValue);
						break;
					case SerializedPropertyType.Integer:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.LayerMask:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Quaternion:
						stringHash.Append(sp.quaternionValue);
						break;
					case SerializedPropertyType.Rect:
						stringHash.Append(sp.rectValue);
						break;
					case SerializedPropertyType.String:
						stringHash.Append(sp.stringValue);
						break;
					case SerializedPropertyType.Vector2:
						stringHash.Append(sp.vector2Value);
						break;
					case SerializedPropertyType.Vector3:
						stringHash.Append(sp.vector3Value);
						break;
					case SerializedPropertyType.Vector4:
						stringHash.Append(sp.vector4Value);
						break;
					default:
						Debug.LogWarning(Maintainer.LogPrefix + "Unknown SerializedPropertyType: " + sp.propertyType);
						break;
				}

			return stringHash.ToString().GetHashCode();
		}

		public static string GetFullTransformPath(Transform transform)
		{
			var path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}

		public static GameObject[] GetAllSuitableGameObjectsInOpenedScenes()
		{
			var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
			var count = allObjects.Length;
			var result = new List<GameObject>(count);
			for (var i = 0; i < count; i++)
			{
				var go = allObjects[i];
				if (go.hideFlags != HideFlags.None) continue;
				var prefabType = PrefabUtility.GetPrefabType(go);
				if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab) continue;

				result.Add(go);
			}

			return result.ToArray();
		}

		public static int GetAllSuitableObjectsInFileAssets(List<Object> objects)
		{
			return GetAllSuitableObjectsInFileAssets(objects, null);
		}

		public static int GetAllSuitableObjectsInFileAssets(List<Object> objects, List<string> paths)
		{
			var allAssetPaths = FindAssetsFiltered("t:Prefab, t:ScriptableObject");
			return GetSuitableFileAssetsFromSelection(allAssetPaths, objects, paths);
		}

		public static int GetSuitableFileAssetsFromSelection(string[] selection, List<Object> objects, List<string> paths)
		{
			var selectedCount = 0;

			foreach (var path in selection)
			{
				var assetObject = AssetDatabase.LoadMainAssetAtPath(path);
				if (assetObject is GameObject)
				{
					selectedCount = GetPrefabsRecursive(objects, paths, path, assetObject as GameObject, selectedCount);
				}
				else if (assetObject is ScriptableObject)
				{
					if (paths != null) paths.Add(path);
					objects.Add(assetObject);
					selectedCount ++;
				}
			}

			return selectedCount;
		}

		private static int GetPrefabsRecursive(List<Object> objects, List<string> paths, string path, GameObject go, int selectedCount)
		{
			if (go.hideFlags == HideFlags.None || go.hideFlags == HideFlags.HideInHierarchy)
			{
				objects.Add(go);
				if (paths != null) paths.Add(path);
				selectedCount++;
			}

			var childCount = go.transform.childCount;

			for (var i = 0; i < childCount; i++)
			{
				var nestedObject = go.transform.GetChild(i).gameObject;
				selectedCount = GetPrefabsRecursive(objects, paths, path, nestedObject, selectedCount);
			}

			return selectedCount;
		}

		public static string[] FindAssetsFiltered(string searchMask)
		{
			return FindAssetsFiltered(searchMask, null, null);
		}

		public static string[] FindAssetsFiltered(string searchMask, FilterItem[] includes, FilterItem[] ignores)
		{
			var allAssetsGUIDs = AssetDatabase.FindAssets(searchMask);
			var count = allAssetsGUIDs.Length;

			var paths = new List<string>(count);

			for (var i = 0; i < count; i++)
			{
				var path = AssetDatabase.GUIDToAssetPath(allAssetsGUIDs[i]);

				var include = false;
				var skip = false;

				if (ignores != null && ignores.Length > 0)
				{
					skip = IsValueMatchesAnyFilter(path, ignores);
				}

				if (skip) continue;

				if (includes != null && includes.Length > 0)
				{
					include = IsValueMatchesAnyFilter(path, includes);
				}

				if (includes != null && includes.Length > 0)
				{
					if (include && !paths.Contains(path)) paths.Add(path);
				}
				else
				{
					if (!paths.Contains(path)) paths.Add(path);
				}
			}

			return paths.ToArray();
		}

		public static string[] FindAssetsInFolders(string filter, string[] folders)
		{
			if (folders == null || folders.Length == 0) return new string[0];

			var allAssetsGUIDs = AssetDatabase.FindAssets(filter, folders);
			var count = allAssetsGUIDs.Length;

			var paths = new string[count];

			for (var i = 0; i < count; i++)
			{
				paths[i] = AssetDatabase.GUIDToAssetPath(allAssetsGUIDs[i]);
			}

			return paths;
		}

		public static string[] FindFilesFiltered(string filter, string[] ignores)
		{
			var files = Directory.GetFiles("Assets", filter, SearchOption.AllDirectories);
			var count = files.Length;

			var paths = new List<string>(count);

			for (var i = 0; i < count; i++)
			{
				var path = files[i];
				var skip = false;

				if (ignores != null)
				{
					skip = CSArrayTools.IsItemContainsAnyStringFromArray(path, ignores);
				}

				if (!skip) paths.Add(path);
			}

			return paths.ToArray();
		}

		public static string[] FindFoldersFiltered(string filter, string[] ignores = null)
		{
			var files = Directory.GetDirectories("Assets", filter, SearchOption.AllDirectories);
			var count = files.Length;

			var paths = new List<string>(count);

			for (var i = 0; i < count; i++)
			{
				var path = files[i];
				var skip = false;

				if (ignores != null)
				{
					skip = CSArrayTools.IsItemContainsAnyStringFromArray(path, ignores);
				}

				if (!skip) paths.Add(path);
			}

			return paths.ToArray();
		}

		public static int GetDepthInHierarchy(Transform transform, Transform upToTransform)
		{
			if (transform == upToTransform || transform.parent == null) return 0;
			return 1 + GetDepthInHierarchy(transform.parent, upToTransform);
		}

		public static string NicifyAssetPath(string path, bool trimExtension = false)
		{
			if (path.Length <= 7) return path;

			var nicePath = path.Remove(0, 7);

			if (trimExtension)
			{
				var lastSlash = nicePath.LastIndexOf('/');
				var lastDot = nicePath.LastIndexOf('.');

				// making sure we'll not trim path like Test/My.Test/linux_file
				if (lastDot > lastSlash)
				{
					nicePath = nicePath.Remove(lastDot, nicePath.Length - lastDot);
				}
			}

			return nicePath;
		}

		public static ActiveEditorTracker GetActiveEditorTrackerForSelectedObject()
		{
//#if UNITY_2018_1_OR_NEWER
//			var inspectorWindowType = typeof(InspectorWindow);
//			var inspectorWindow = EditorWindow.GetWindow(inspectorWindowType);
//#else
			var inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
			if (inspectorWindowType == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't find UnityEditor.InspectorWindow type!"));
				return null;
			}

			var inspectorWindow = EditorWindow.GetWindow(inspectorWindowType);
			if (inspectorWindow == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get an InspectorWindow!"));
				return null;
			}
//#endif

#if UNITY_5_5_OR_NEWER
			ActiveEditorTracker result;

			var trackerProperty = CSReflectionTools.GetPropertyInfo(inspectorWindowType, "tracker");
			if (trackerProperty == null)
			{
				// may be removed for Unity 5.6 +, since GetTracker method was removed somewhere in 5.5 cycle
				result = GetActiveTrackerUsingMethod(inspectorWindowType, inspectorWindow);
			}
			else
			{
				result = (ActiveEditorTracker)trackerProperty.GetValue(inspectorWindow, null);
			}

			if (result == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get ActiveEditorTracker from the InspectorWindow!"));
			}

			return result;
#else
			return GetActiveTrackerUsingMethod(inspectorWindowType, inspectorWindow);
#endif
		}

		// may be removed for Unity 5.6 +, since GetTracker method was removed somewhere in 5.5 cycle
		private static ActiveEditorTracker GetActiveTrackerUsingMethod(Type inspectorWindowType, EditorWindow inspectorWindow)
		{
			var getTrackerMethod = inspectorWindowType.GetMethod("GetTracker");

			if (getTrackerMethod == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't find an InspectorWindow.GetTracker() method!"));
				return null;
			}

			return (ActiveEditorTracker)getTrackerMethod.Invoke(inspectorWindow, null);
		}

		public static string[] GetProjectSelections(bool includeFolders)
		{
			var selectedGUIDs = Selection.assetGUIDs;
			var paths = new List<string>(selectedGUIDs.Length);

			foreach (var guid in selectedGUIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				if (Directory.Exists(path) && !includeFolders) continue;
				paths.Add(path);
			}

			return paths.ToArray();
		}

		public static void RemoveReadOnlyAttribute(string filePath)
		{
			var attributes = File.GetAttributes(filePath);
			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
		}

		public static string GetAssetDatabasePath(string path)
		{
			return !Path.IsPathRooted(path) ? path : path.Replace('\\', '/').Substring(assetsFolderIndex + 1);
		}

		public static bool IsValueMatchesAnyFilter(string value, FilterItem[] filters)
		{
			var match = false;
			var directory = string.Empty;
			var filename = string.Empty;
			var extension = string.Empty;

			foreach (var filter in filters)
			{
				switch (filter.kind)
				{
					case FilterKind.Path:
					case FilterKind.Type:
						match = FilterMatchHelper(value, filter);
						break;
					case FilterKind.Directory:
						if (directory == string.Empty)
						{
							directory = Path.GetDirectoryName(value);
						}
						if (directory != null)
						{
							match = FilterMatchHelper(directory, filter);
						}
						break;
					case FilterKind.FileName:
						if (filename == string.Empty)
						{
							filename = Path.GetFileName(value);
						}
						if (filename != null)
						{
							match = FilterMatchHelper(filename, filter);
						}
						break;
					case FilterKind.Extension:
						if (extension == string.Empty)
						{
							extension = Path.GetExtension(value);
						}
						if (extension != null)
						{
							match = FilterMatchHelper(extension, filter);
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				if (match)
				{
					break;
				}
			}

			return match;
		}

		public static bool TryAddNewItemToFilters(ref FilterItem[] filters, FilterItem newItem)
		{
			foreach (var filterItem in filters)
			{
				if (filterItem.value == newItem.value)
				{
					return false;
				}
			}

			ArrayUtility.Add(ref filters, newItem);
			return true;
		}

		public static bool IsValueMatchesFilter(string value, FilterItem filter)
		{
			switch (filter.kind)
			{
				case FilterKind.Path:
				case FilterKind.Type:
					return FilterMatchHelper(value, filter);
				case FilterKind.Directory:
					var directory = Path.GetDirectoryName(value);
					if (!string.IsNullOrEmpty(directory))
					{
						return FilterMatchHelper(directory, filter);
					}
					break;
				case FilterKind.FileName:
					var fileName = Path.GetFileName(value);
					if (!string.IsNullOrEmpty(fileName))
					{
						return FilterMatchHelper(fileName, filter);
					}
					break;
				case FilterKind.Extension:
					var extension = Path.GetExtension(value);
					if (!string.IsNullOrEmpty(extension))
					{
						return FilterMatchHelper(extension, filter);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return false;
		}

		private static bool FilterMatchHelper(string value, FilterItem filter)
		{
			return value.IndexOf(filter.value, filter.ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) != -1;
		}

		public static Object GetLightmapSettings()
		{
			var mi = CSReflectionTools.GetGetLightmapSettingsMethodInfo();
			if (mi != null)
			{
				return (Object) mi.Invoke(null, null);
			}

			Debug.LogError(Maintainer.ConstructError("Can't retrieve LightmapSettings object via reflection!"));
			return null;
		}

		public static Object GetRenderSettings()
		{
			var mi = CSReflectionTools.GetGetRenderSettingsMethodInfo();
			if (mi != null)
			{
				return (Object) mi.Invoke(null, null);
			}

			Debug.LogError(Maintainer.ConstructError("Can't retrieve RenderSettings object via reflection!"));
			return null;
		}

		public static bool RevealAndSelect(string assetPath, string transformPath, long objectId, long componentId, string propertyPath)
		{
			Object target = null;

			/* selecting a folder */

			if (Directory.Exists(assetPath))
			{
				var instanceId = GetMainAssetInstanceID(assetPath);
				Selection.activeInstanceID = instanceId;

				return true;
			}

			/* selecting asset files or objects on prefabs and in scenes */

			var targetAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

			if (objectId == -1)
			{
				Selection.activeObject = targetAsset;
				return true;
			}

			var lookingInScene = targetAsset is SceneAsset;

			if (lookingInScene)
			{
				if (lastRevealSceneOpenResult != null)
				{
					CSSceneTools.CloseOpenedSceneIfNeeded(lastRevealSceneOpenResult);
				}

				var newSceneOpenResult = CSSceneTools.OpenSceneWithSavePrompt(assetPath);
				if (newSceneOpenResult.success)
				{
					target = CSObjectTools.FindGameObjectInScene(newSceneOpenResult.scene, objectId, transformPath);
					lastRevealSceneOpenResult = newSceneOpenResult;
				}
			}
			else if (targetAsset is GameObject)
			{
				var targetGo = (GameObject)targetAsset;
				target = CSObjectTools.FindChildGameObjectRecursive(targetGo.transform, targetGo.transform.name, objectId, transformPath);

				// trying to find specific cases -------------------------------------------------------------------------

				if (target == null)
				{
					var allObjectsInPrefab = AssetDatabase.LoadAllAssetsAtPath(assetPath);

					foreach (var objectOnPrefab in allObjectsInPrefab)
					{
						if (objectOnPrefab is BillboardAsset || objectOnPrefab is TreeData)
						{
							var objectOnPrefabId = CSObjectTools.GetUniqueObjectId(objectOnPrefab);

							if (objectOnPrefabId == objectId)
							{
								target = objectOnPrefab;
							}
						}
					}
				}
			}
			else
			{
				target = targetAsset;
			}

			if (target == null)
			{
				Debug.LogError(Maintainer.ConstructError("Couldn't find target Game Object at " + assetPath + " with ObjectID " + objectId + "!"));
				return false;
			}

			if (target is GameObject)
			{
				CSObjectTools.SelectGameObject((GameObject)target, lookingInScene);
			}
			else
			{
				Selection.activeObject = target;
			}

			if (lookingInScene)
			{
				EditorApplication.delayCall += () =>
				{
					EditorGUIUtility.PingObject(targetAsset);
				};
			}
			else
			{
				if (transformPath.Split('/').Length > 2)
				{
					EditorApplication.delayCall += () =>
					{
						EditorGUIUtility.PingObject(targetAsset);
					};
				}
			}

			/* folding all other components if we need to show a component */
			if (componentId != -1)
			{
				var tracker = GetActiveEditorTrackerForSelectedObject();
				if (tracker == null)
				{
					Debug.LogError(Maintainer.ConstructError("Can't get active tracker."));
					return false;
				}
				tracker.RebuildIfNecessary();

				var editors = tracker.activeEditors;

				var targetFound = false;
				var skipCount = 0;

				for (var i = 0; i < editors.Length; i++)
				{
					var editor = editors[i];
					var editorTargetType = editor.target.GetType();
					if (editorTargetType == typeof(UnityEditor.AssetImporter) || editorTargetType == typeof(UnityEngine.GameObject))
					{
						skipCount++;
						continue;
					}

					if (i - skipCount == componentId)
					{
						targetFound = true;

						/* known corner cases when editor can't be set to visible via tracker */

						if (editor.serializedObject.targetObject is ParticleSystemRenderer)
						{
							var renderer = (ParticleSystemRenderer)editor.serializedObject.targetObject;
							var ps = renderer.GetComponent<ParticleSystem>();
							componentId = CSObjectTools.GetComponentIndex(ps);
						}

						break;
					}
				}

				if (!targetFound)
				{
					return false;
				}
				for (var i = 0; i < editors.Length; i++)
				{
					tracker.SetVisible(i, i - skipCount != componentId ? 0 : 1);
				}
				return true;
			}
			return true;
		}

		private static int GetMainAssetInstanceID(string path)
		{
			var mi = CSReflectionTools.GetGetMainAssetInstanceIDMethodInfo();
			if (mi != null)
			{
				return (int)mi.Invoke(null, new object[] { path });
			}

			Debug.LogError(Maintainer.ConstructError("Can't retrieve InstanceID From GUID via reflection!"));
			return -1;
		}

		public static bool RevealInSettings(AssetSettingsKind settingsKind, string path = null)
		{
			var result = true;

			switch (settingsKind)
			{
				case AssetSettingsKind.NotSettings:
					Debug.LogWarning(Maintainer.LogPrefix + "Can't open settings of kind NotSettings Oo");
					result = false;
					break;
				case AssetSettingsKind.AudioManager:
					break;
				case AssetSettingsKind.ClusterInputManager:
					break;
				case AssetSettingsKind.DynamicsManager:
					break;
				case AssetSettingsKind.EditorBuildSettings:
					try
					{
						if (EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor")) == null)
						{
							result = false;
						}
					}
					catch (Exception)
					{
						result = false;
					}
					
					if (result == false)
					{
						Debug.LogError(Maintainer.ConstructError("Can't open EditorBuildSettings!"));
					}
					break;
				case AssetSettingsKind.EditorSettings:
					break;
				case AssetSettingsKind.GraphicsSettings:
					if (!EditorApplication.ExecuteMenuItem("Edit/Project Settings/Graphics"))
					{
						Debug.LogError(Maintainer.ConstructError("Can't open GraphicsSettings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.InputManager:
					break;
				case AssetSettingsKind.NavMeshAreas:
					break;
				case AssetSettingsKind.NavMeshLayers:
					break;
				case AssetSettingsKind.NavMeshProjectSettings:
					break;
				case AssetSettingsKind.NetworkManager:
					break;
				case AssetSettingsKind.Physics2DSettings:
					break;
				case AssetSettingsKind.ProjectSettings:
					if (!EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player"))
					{
						Debug.LogError(Maintainer.ConstructError("Can't open ProjectSettings!"));
						result = false;
					}
					break;
				case AssetSettingsKind.PresetManager:
					break;
				case AssetSettingsKind.QualitySettings:
					break;
				case AssetSettingsKind.TagManager:
					break;
				case AssetSettingsKind.TimeManager:
					break;
				case AssetSettingsKind.UnityAdsSettings:
					break;
				case AssetSettingsKind.UnityConnectSettings:
					break;
				case AssetSettingsKind.Unknown:
					if (!string.IsNullOrEmpty(path)) EditorUtility.RevealInFinder(path);
					break;
				default:
					throw new ArgumentOutOfRangeException("settingsKind", settingsKind, null);
			}

			return result;
		}

		public static string NicifyName(string name)
		{
			var nicePropertyName = ObjectNames.NicifyVariableName(name);
			if (textInfo == null) textInfo = new CultureInfo("en-US", false).TextInfo;
			return textInfo.ToTitleCase(nicePropertyName);
		}
	}
}