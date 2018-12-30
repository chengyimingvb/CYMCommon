#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public class ObjectIssueRecord : IssueRecord, IShowableRecord
	{
		public string path;
		public string transformPath;
		public long objectId;
		public string componentName;
		public long componentIndex;
		public string property;
		public string propertyPath;

		public void Show()
		{
			if (!CSEditorTools.RevealAndSelect(path, transformPath, objectId, componentIndex, propertyPath))
			{
				MaintainerWindow.ShowNotification("Can't show it properly");
			}
		}

		internal static ObjectIssueRecord Create(RecordType type, RecordLocation location, string path, Object unityObject)
		{
			return new ObjectIssueRecord(type, location, path, unityObject);
		}

		internal static ObjectIssueRecord Create(RecordType type, RecordLocation location, string path, Object unityObject, Type componentType, string componentName, int componentIndex)
		{
			return new ObjectIssueRecord(type, location, path, unityObject, componentType, componentName, componentIndex);
		}

		internal static ObjectIssueRecord Create(RecordType type, RecordLocation location, string path, Object unityObject, Type componentType, string componentName, int componentIndex, string property)
		{
			return new ObjectIssueRecord(type, location, path, unityObject, componentType, componentName, componentIndex, property);
		}

		protected ObjectIssueRecord(RecordType type, RecordLocation location, string path, Object unityObject):base(type,location)
		{
			this.path = path;
			if (unityObject is GameObject)
			{
				transformPath = CSEditorTools.GetFullTransformPath((unityObject as GameObject).transform);
				if (location == RecordLocation.Scene)
				{
					this.path = (unityObject as GameObject).scene.path;
				}
			}
			else
			{
				transformPath = unityObject.name;
			}
			
			objectId = CSObjectTools.GetUniqueObjectId(unityObject);
		}

		protected ObjectIssueRecord(RecordType type, RecordLocation location, string path, Object unityObject, Type componentType, string componentName, int componentIndex) : this(type, location, path, unityObject)
		{
			this.componentName = componentName;

			this.componentIndex = componentIndex;
			if (unityObject is GameObject)
			{
				if (this.componentIndex > 0 && componentType != null && ((GameObject)unityObject).GetComponents(componentType).Length > 1)
				{
					this.componentName += " (ID: " + this.componentIndex + ")";
				}
			}
		}

		protected ObjectIssueRecord(RecordType type, RecordLocation location, string path, Object unityObject, Type componentType, string componentName, int componentIndex, string property):this(type, location, path, unityObject, componentType, componentName, componentIndex)
		{
			if (!string.IsNullOrEmpty(property))
			{
				this.property = CSEditorTools.NicifyName(property);
			}
		}

		internal override bool CanBeFixed()
		{
			return type == RecordType.MissingComponent || type == RecordType.MissingReference;
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append(location == RecordLocation.Scene ? "<b>Scene:</b> " : "<b>Prefab:</b> ");

			var nicePath = path == "" ? "Untitled (current scene)" : CSEditorTools.NicifyAssetPath(path, true);

			text.Append(nicePath);

			if (!string.IsNullOrEmpty(transformPath)) text.Append("\n<b>Object:</b> ").Append(transformPath);
			if (!string.IsNullOrEmpty(componentName)) text.Append("\n<b>Component:</b> ").Append(componentName);
			if (!string.IsNullOrEmpty(property)) text.Append("\n<b>Property:</b> ").Append(property);
		}

		protected override bool PerformFix(bool batchMode)
		{
			Object obj = null;
			Component component = null;

			CSSceneTools.OpenSceneResult openSceneResult = null;

			if (!batchMode && location == RecordLocation.Scene)
			{
				openSceneResult = CSSceneTools.OpenScene(path);
				if (!openSceneResult.success)
				{
					return false;
				}
			}

			obj = GetObjectWithThisIssue();

			if (obj == null)
			{
				if (batchMode)
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Can't find Object for issue:\n" + this);
				}
				else
				{
					MaintainerWindow.ShowNotification("Couldn't find Object " + transformPath);
				}
				return false;
			}

			if (!string.IsNullOrEmpty(componentName) && obj is GameObject)
			{
				component = GetComponentWithThisIssue(obj as GameObject);

				if (component == null)
				{
					if (batchMode)
					{
						Debug.LogWarning(Maintainer.LogPrefix + "Can't find component for issue:\n" + this);
					}
					else
					{
						MaintainerWindow.ShowNotification("Can't find component " + componentName);
					}

					return false;
				}
			}

			var fixResult = IssuesFixer.FixObjectIssue(this, obj, component, type);

			if (!batchMode && location == RecordLocation.Scene && openSceneResult != null)
			{
				CSSceneTools.SaveScene(openSceneResult.scene);
				CSSceneTools.CloseOpenedSceneIfNeeded(openSceneResult);
			}
			
			return fixResult;
		}

		private Object GetObjectWithThisIssue()
		{
			Object result;

			if (location == RecordLocation.Scene)
			{
				var scene = CSSceneTools.GetSceneByPath(path);
				result = CSObjectTools.FindGameObjectInScene(scene, objectId, transformPath);
			}
			else
			{
				var fileAssets = new List<Object>();
				CSEditorTools.GetAllSuitableObjectsInFileAssets(fileAssets);
				var allObjects = fileAssets.ToArray();
				result = FindObjectInCollection(allObjects);
			}
			return result;
		}

		private Component GetComponentWithThisIssue(GameObject go)
		{
			Component component = null;
			var components = go.GetComponents<Component>();
			for (var i = 0; i < components.Length; i++)
			{
				if (i == componentIndex)
				{
					component = components[i];
					break;
				}
			}

			return component;
		}

		private Object FindObjectInCollection(IEnumerable<Object> allObjects)
		{
			Object candidate = null;

			foreach (var item in allObjects)
			{
				if (item is GameObject && CSEditorTools.GetFullTransformPath((item as GameObject).transform) != transformPath) continue;

				candidate = item;
				if (objectId == CSObjectTools.GetUniqueObjectId(candidate))
				{
					break;
				}
			}
			return candidate;
		}
	}
}