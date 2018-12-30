#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Tools
{
	public class CSObjectTools
	{
		internal static long GetUniqueObjectId(Object unityObject)
		{
			long id = -1;

			if (unityObject == null) return id;

			if (AssetDatabase.Contains(unityObject))
			{
				var path = AssetDatabase.GetAssetPath(unityObject);
				if (!string.IsNullOrEmpty(path))
				{
					if (AssetDatabase.IsMainAsset(unityObject))
					{
						var pathBytes = Encoding.UTF8.GetBytes(path);
						id = xxHash.CalculateHash(pathBytes, pathBytes.Length, 230887);
					}
					else
					{
						id = GetLocalIdentifierInFile(unityObject);
					}
				}
				else
				{
					Debug.LogError(Maintainer.ConstructError("Can't get path to the asset " + unityObject.name));
				}
			}
			else
			{
				var prefabType = PrefabUtility.GetPrefabType(unityObject);
				if (prefabType != PrefabType.None)
				{
					var parentObject = PrefabUtility.GetCorrespondingObjectFromSource(unityObject);
					if (parentObject != null)
					{
						id = GetUniqueObjectId(parentObject);
						return id;
					}
				}

				id = GetLocalIdentifierInFile(unityObject);
				if (id <= 0)
				{
					id = unityObject.GetInstanceID();
				}
			}
			
			if (id <= 0)
			{
				var go = unityObject as GameObject;
				if (go != null)
				{
					id = go.transform.GetSiblingIndex();
				}
			}

			if (id <= 0)
			{
				id = unityObject.name.GetHashCode();
			}

			return id;
		}

		private static long GetLocalIdentifierInFile(Object unityObject)
		{
			var serializedObject = new SerializedObject(unityObject);
			try
			{
				CSReflectionTools.SetInspectorToDebug(serializedObject);
				var serializedProperty = serializedObject.FindProperty("m_LocalIdentfierInFile");
				return serializedProperty.longValue;
			}
			catch (Exception e)
			{
				Debug.LogWarning(Maintainer.ConstructWarning("Couldn't get data from debug inspector for object " + unityObject.name + " due to this error:\n" + e));
				return -1;
			}
		}

		internal static int GetComponentIndex(Component component)
		{
			if (component == null) return -1;

			var allComponents = component.GetComponents<Component>();
			for (var i = 0; i < allComponents.Length; i++)
			{
				if (allComponents[i] == component)
				{
					return i;
				}
			}

			return -1;
		}

		internal static string GetNativeObjectType(Object unityObject)
		{
			string result;

			try
			{
				var fullName = unityObject.ToString();
				var openingIndex = fullName.IndexOf('(') + 1;
				if (openingIndex != 0)
				{
					var closingIndex = fullName.LastIndexOf(')');
					result = fullName.Substring(openingIndex, closingIndex - openingIndex);
				}
				else
				{
					result = null;
				}
			}
			catch
			{
				result = null;
			}

			return result;
		}

		internal static void SelectGameObject(GameObject go, bool inScene)
		{
			if (inScene)
			{
				Selection.activeTransform = go == null ? null : go.transform;
			}
			else
			{
				Selection.activeGameObject = go;
			}
		}

		internal static GameObject FindGameObjectInScene(Scene scene, long objectId, string transformPath = null)
		{
			GameObject result = null;
			var rootObjects = scene.GetRootGameObjects();

			foreach (var rootObject in rootObjects)
			{
				result = FindChildGameObjectRecursive(rootObject.transform, rootObject.transform.name, objectId, transformPath);
				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		internal static GameObject[] GetAllGameObjectsInScene(Scene scene)
		{
			var gameObjects = new List<GameObject>();

			var rootObjects = scene.GetRootGameObjects();
			foreach (var rootObject in rootObjects)
			{
				GetSceneObjectsRecursive(ref gameObjects, rootObject);
			}

			return gameObjects.ToArray();
		}

		private static void GetSceneObjectsRecursive(ref List<GameObject> gameObjects, GameObject parentObject)
		{
			var parentTransform = parentObject.transform;

			gameObjects.Add(parentObject);

			for (var i = 0; i < parentTransform.childCount; i++)
			{
				var childTransform = parentTransform.GetChild(i);
				GetSceneObjectsRecursive(ref gameObjects, childTransform.gameObject);
			}
		}

		internal static GameObject FindChildGameObjectRecursive(Transform parent, string currentTransformPath, long objectId, string transformPath = null)
		{
			GameObject result = null;
			var skipObjectIdCheck = false;

			if (!string.IsNullOrEmpty(transformPath))
			{
				if (currentTransformPath != transformPath)
				{
					skipObjectIdCheck = true;
				}
			}

			if (!skipObjectIdCheck)
			{
				var currentObjectId = GetUniqueObjectId(parent.gameObject);
				if (currentObjectId == objectId)
				{
					result = parent.gameObject;
					return result;
				}
			}

			for (var i = 0; i < parent.childCount; i++)
			{
				var childTransform = parent.GetChild(i);
				result = FindChildGameObjectRecursive(childTransform, currentTransformPath + "/" + childTransform.name, objectId, transformPath);
				if (result != null) break;
			}

			return result;
		}

		internal static string GetArrayItemNameAndIndex(string fullPropertyPath)
		{
			var propertyPath = fullPropertyPath.Replace(".Array.data", "").Replace("].", "] / ").Replace("[", " [Element ");
			return propertyPath;
		}

		internal static string GetArrayItemName(string fullPropertyPath)
		{
			var name = GetArrayItemNameAndIndex(fullPropertyPath);
			var lastOpeningBracketIndex = name.LastIndexOf('[');
			return name.Substring(0, lastOpeningBracketIndex);
		}
	}
}