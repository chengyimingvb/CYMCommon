#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Issues
{
	public class IssuesFixer
	{
		public static bool FixObjectIssue(ObjectIssueRecord issue, Object obj, Component component, RecordType type)
		{
			var result = false;

			if (type == RecordType.MissingComponent)
			{
				var hasIssue = GameObjectHasMissingComponent(obj as GameObject);

				if (hasIssue)
				{
					FixMissingComponents(issue, obj as GameObject);
					result = !GameObjectHasMissingComponent(obj as GameObject);
				}
				else
				{
					result = true;
				}
			}
			else if (type == RecordType.MissingReference)
			{
				if (component != null)
				{
					result = FixMissingReference(issue, component);
				}
				else
				{
					result = FixMissingReference(issue, obj);
				}
			}

			return result;
		}

		#region missing component

		// ----------------------------------------------------------------------------
		// fix missing component
		// ----------------------------------------------------------------------------

		private static void FixMissingComponents(ObjectIssueRecord issue, GameObject go)
		{
			CSObjectTools.SelectGameObject(go, issue.location == RecordLocation.Scene);

			var tracker = CSEditorTools.GetActiveEditorTrackerForSelectedObject();
			if (tracker == null)
			{
				Debug.LogError(Maintainer.ConstructError("Can't get active tracker."));
				return;
			}
			tracker.RebuildIfNecessary();

			var touched = false;
			var activeEditors = tracker.activeEditors;
			for (var i = activeEditors.Length - 1; i >= 0; i--)
			{
				var editor = activeEditors[i];
				if (editor.serializedObject.targetObject == null)
				{
					Object.DestroyImmediate(editor.target, true);
					touched = true;
				}
			}

			if (touched)
			{
				if (issue.location == RecordLocation.Scene)
				{
					CSSceneTools.MarkSceneDirty();
				}
				else
				{
					EditorUtility.SetDirty(go);
				}
			}
		}

		private static bool GameObjectHasMissingComponent(GameObject go)
		{
			var hasMissingComponent = false;
			var components = go.GetComponents<Component>();
			foreach (var c in components)
			{
				if (c == null)
				{
					hasMissingComponent = true;
					break;
				}
			}

			return hasMissingComponent;
		}
#endregion

		#region missing reference
		// ----------------------------------------------------------------------------
		// fix missing reference
		// ----------------------------------------------------------------------------

		private static bool FixMissingReference(ObjectIssueRecord issue, Object unityObject)
		{
			var so = new SerializedObject(unityObject);
			var sp = so.FindProperty(issue.propertyPath);

			if (sp.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
				{
					sp.objectReferenceInstanceIDValue = 0;

					// fixes dirty scene flag after batch issues fix
					// due to the additional undo action
					so.ApplyModifiedPropertiesWithoutUndo();

					if (issue.location == RecordLocation.Scene)
					{
						CSSceneTools.MarkSceneDirty();
					}
					else
					{
						EditorUtility.SetDirty(unityObject);
					}
				}
			}

			return true;
		}
		#endregion
	}
}