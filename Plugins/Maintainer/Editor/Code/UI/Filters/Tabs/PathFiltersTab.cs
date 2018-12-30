#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class PathFiltersTab : StringFiltersTab
	{
		private readonly bool showNotice;
		private readonly string headerExtra;

		internal PathFiltersTab(FilterType filterType, string headerExtra, FilterItem[] filtersList, bool showNotice, SaveFiltersCallback saveCallback, GetDefaultsCallback defaultsCallback = null) : base(filterType, filtersList, saveCallback, defaultsCallback)
		{
			caption = new GUIContent("Path <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + ">" + filterType + "</color>", CSEditorIcons.FolderIcon);
			this.headerExtra = headerExtra;
			this.showNotice = showNotice;
		}

		internal override void ProcessDrags()
		{
			if (currentEventType != EventType.DragUpdated && currentEventType != EventType.DragPerform) return;

			var paths = DragAndDrop.paths;

			if (paths != null && paths.Length > 0)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (currentEventType == EventType.DragPerform)
				{
					var needToSave = false;
					var needToShowWarning = false;

					foreach (var path in paths)
					{
						var added = CSEditorTools.TryAddNewItemToFilters(ref filters, FilterItem.Create(path, FilterKind.Path));
						needToSave |= added;
						needToShowWarning |= !added;
					}

					if (needToSave)
					{
						SaveChanges();
					}

					if (needToShowWarning)
					{
						window.ShowNotification(new GUIContent("One or more of the dragged items already present in the list!"));
					}

					DragAndDrop.AcceptDrag();
				}
			}
			Event.current.Use();
		}

		protected override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify full or partial paths to <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + "><b>" + 
										(filterType == FilterType.Includes ? "include" : "ignore") + "</b></color>.\n" +
										"You may drag & drop files and folders to this window directly from the Project Browser.",
										UIHelpers.richWordWrapLabel);

			EditorGUILayout.LabelField("Only Extension filter type matches whole word. Extension filter value should start with dot (.dll).", UIHelpers.richWordWrapLabel);

			if (!string.IsNullOrEmpty(headerExtra))
			{
				EditorGUILayout.LabelField(headerExtra, EditorStyles.wordWrappedLabel);
			}

			if (showNotice)
			{
				EditorGUILayout.LabelField("<b>Note:</b> If you have both Includes and Ignores added, first Includes are applied, then Ignores are applied to the included paths.",
										UIHelpers.richWordWrapLabel);
			}
		}

		protected override bool CheckNewItem(ref string newItem)
		{
			newItem = newItem.Replace('\\', '/');
			return true;
		}

		protected override void DrawFilterKindLabel(FilterKind kind)
		{
			GUILayout.Label(kind.ToString(), GUILayout.Width(80));
		}

		protected override FilterKind DrawFilterKindDropdown(FilterKind kind)
		{
			var enumNames = Enum.GetNames(typeof(FilterKind));

			// removing Type filter as we don't need it at the PathFilters
			Array.Resize(ref enumNames, enumNames.Length - 1);
			return (FilterKind) EditorGUILayout.Popup((int) kind, enumNames, GUILayout.Width(80));
		}

		protected override void DrawFilterIgnoreCaseLabel(bool ignore)
		{
			GUILayout.Label(!ignore ? "Match Case" : "<b>Ignore Case</b>", UIHelpers.richLabel, GUILayout.Width(90));
		}

		protected override bool DrawFilterIgnoreCaseToggle(bool ignore)
		{
			return GUILayout.Toggle(ignore, "Ignore Case", GUILayout.Width(90));
		}
	}
}
