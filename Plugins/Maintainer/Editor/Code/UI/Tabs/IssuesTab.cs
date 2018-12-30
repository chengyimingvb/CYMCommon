#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Filters;
using UnityEditor;
using UnityEngine;
using RecordType = CodeStage.Maintainer.Issues.RecordType;

namespace CodeStage.Maintainer.UI
{
	internal class IssuesTab : RecordsTab<IssueRecord>
	{
		private GUIContent caption;

		public IssuesTab(MaintainerWindow maintainerWindow) : base(maintainerWindow)
		{
		}

		internal GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(IssuesFinder.ModuleName, CSIcons.Issue);
				}
				return caption;
			}
		}

		protected override IssueRecord[] LoadLastRecords()
		{
			var loadedRecords = SearchResultsStorage.IssuesSearchResults;

			if (loadedRecords == null)
			{
				loadedRecords = new IssueRecord[0];
			}

			return loadedRecords;
		}

		protected override RecordsTabState GetState()
		{
			return MaintainerSettings.Issues.tabState;
		}

		protected override void ApplySorting()
		{
			base.ApplySorting();

			switch (MaintainerSettings.Issues.sortingType)
			{
				case IssuesSortingType.Unsorted:
					break;
				case IssuesSortingType.ByIssueType:
					filteredRecords = MaintainerSettings.Issues.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.issueRecordByType).ThenBy(RecordsSortings.issueRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.issueRecordByType).ThenBy(RecordsSortings.issueRecordByPath).ToArray();
					break;
				case IssuesSortingType.BySeverity:
					filteredRecords = MaintainerSettings.Issues.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderByDescending(RecordsSortings.issueRecordBySeverity).ThenBy(RecordsSortings.issueRecordByPath).ToArray() :
						filteredRecords.OrderBy(RecordsSortings.issueRecordBySeverity).ThenBy(RecordsSortings.issueRecordByPath).ToArray();
					break;
				case IssuesSortingType.ByPath:
					filteredRecords = MaintainerSettings.Issues.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.issueRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.issueRecordByPath).ToArray();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void SaveSearchResults()
		{
			SearchResultsStorage.IssuesSearchResults = GetRecords();
		}

		protected override string GetModuleName()
		{
			return IssuesFinder.ModuleName;
		}

		protected override void DrawSettingsBody()
		{
			// ----------------------------------------------------------------------------
			// filtering settings
			// ----------------------------------------------------------------------------

			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Manage Filters...", CSIcons.Gear))
				{
					IssuesFiltersWindow.Create();
				}

				GUILayout.Space(5);

				/* Game Object Issues filtering */

				GUILayout.Label("<b><size=12>Game Object Issues filtering</size></b>", UIHelpers.richLabel);
				UIHelpers.Separator();
				GUILayout.Space(5);

				using (new GUILayout.HorizontalScope())
				{
					MaintainerSettings.Issues.lookInScenes = EditorGUILayout.ToggleLeft(new GUIContent("Scenes", "Uncheck to exclude all scenes from search or select filtering level:\n\n" +
					                                                                                             "All Scenes: all project scenes with respect to configured filters.\n" +
					                                                                                             "Included Scenes: scenes included via Manage Filters > Scene Includes.\n" +
					                                                                                             "Current Scene: currently opened scene including any additional loaded scenes."), MaintainerSettings.Issues.lookInScenes, GUILayout.Width(70));
					GUI.enabled = MaintainerSettings.Issues.lookInScenes;
					MaintainerSettings.Issues.scenesSelection = (IssuesFinderSettings.ScenesSelection)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.scenesSelection);
					GUI.enabled = true;
				}

				MaintainerSettings.Issues.lookInAssets = EditorGUILayout.ToggleLeft(new GUIContent("File assets", "Uncheck to exclude all file assets like prefabs, ScriptableObjects and such from the search. Check readme for additional details."), MaintainerSettings.Issues.lookInAssets);
				MaintainerSettings.Issues.touchInactiveGameObjects = EditorGUILayout.ToggleLeft(new GUIContent("Inactive GameObjects", "Uncheck to exclude all inactive Game Objects from the search."), MaintainerSettings.Issues.touchInactiveGameObjects);
				MaintainerSettings.Issues.touchDisabledComponents = EditorGUILayout.ToggleLeft(new GUIContent("Disabled Components", "Uncheck to exclude all disabled Components from the search."), MaintainerSettings.Issues.touchDisabledComponents);

				GUILayout.Space(2);
			}

			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(5);
				GUILayout.Label("<b><size=12>Search for:</size></b>", UIHelpers.richLabel);

				settingsSectionScrollPosition = GUILayout.BeginScrollView(settingsSectionScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

				// ----------------------------------------------------------------------------
				// Game Object Issues
				// ----------------------------------------------------------------------------

				GUI.enabled = UIHelpers.ToggleFoldout(ref MaintainerSettings.Issues.scanGameObjects, ref MaintainerSettings.Issues.gameObjectsFoldout, new GUIContent("<b>Game Object Issues</b>", "Group of issues related to the Game Objects."));
				if (MaintainerSettings.Issues.gameObjectsFoldout)
				{
					GUILayout.Space(-2);
					//UIHelpers.Indent();

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.Common, ref MaintainerSettings.Issues.commonFoldout))
					{
						MaintainerSettings.Issues.missingComponents = EditorGUILayout.ToggleLeft(new GUIContent("Missing components", "Search for the missing components on the Game Objects."), MaintainerSettings.Issues.missingComponents);
						MaintainerSettings.Issues.duplicateComponents = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate components", "Search for the multiple instances of the same component with same values on the same object."), MaintainerSettings.Issues.duplicateComponents);
						
						var show = MaintainerSettings.Issues.duplicateComponents;
						if (show)
						{
							EditorGUI.indentLevel++;
							MaintainerSettings.Issues.duplicateComponentsPrecise = EditorGUILayout.ToggleLeft(new GUIContent("Precise mode", "Uncheck to ignore component's values."), MaintainerSettings.Issues.duplicateComponentsPrecise);
							EditorGUI.indentLevel--;
						}
						
						MaintainerSettings.Issues.missingReferences = EditorGUILayout.ToggleLeft(new GUIContent("Missing references", "Search for any missing references in the serialized fields of the components."), MaintainerSettings.Issues.missingReferences);
						MaintainerSettings.Issues.undefinedTags = EditorGUILayout.ToggleLeft(new GUIContent("Objects with undefined tags", "Search for GameObjects without any tag."), MaintainerSettings.Issues.undefinedTags);
						MaintainerSettings.Issues.inconsistentTerrainData = EditorGUILayout.ToggleLeft(new GUIContent("Inconsistent Terrain Data", "Search for Game Objects where Terrain and TerrainCollider have different Terrain Data."), MaintainerSettings.Issues.inconsistentTerrainData);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.PrefabsSpecific, ref MaintainerSettings.Issues.prefabsFoldout))
					{
						MaintainerSettings.Issues.missingPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Missing prefabs", "Search for instances of prefabs which were removed from project."), MaintainerSettings.Issues.missingPrefabs);
						MaintainerSettings.Issues.disconnectedPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Disconnected prefabs", "Search for disconnected prefabs instances."), MaintainerSettings.Issues.disconnectedPrefabs);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.UnreferencedComponents, ref MaintainerSettings.Issues.unreferencedFoldout))
					{
						MaintainerSettings.Issues.emptyMeshColliders = EditorGUILayout.ToggleLeft("MeshColliders w/o meshes", MaintainerSettings.Issues.emptyMeshColliders);
						MaintainerSettings.Issues.emptyMeshFilters = EditorGUILayout.ToggleLeft("MeshFilters w/o meshes", MaintainerSettings.Issues.emptyMeshFilters);
						MaintainerSettings.Issues.emptyAnimations = EditorGUILayout.ToggleLeft("Animations w/o clips", MaintainerSettings.Issues.emptyAnimations);
						MaintainerSettings.Issues.emptyRenderers = EditorGUILayout.ToggleLeft("Renders w/o materials", MaintainerSettings.Issues.emptyRenderers);
						MaintainerSettings.Issues.emptySpriteRenderers = EditorGUILayout.ToggleLeft("SpriteRenders w/o sprites", MaintainerSettings.Issues.emptySpriteRenderers);
						MaintainerSettings.Issues.emptyTerrainCollider = EditorGUILayout.ToggleLeft("TerrainColliders w/o Terrain Data", MaintainerSettings.Issues.emptyTerrainCollider);
						MaintainerSettings.Issues.emptyAudioSource = EditorGUILayout.ToggleLeft("AudioSources w/o AudioClips", MaintainerSettings.Issues.emptyAudioSource);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.Neatness, ref MaintainerSettings.Issues.neatnessFoldout))
					{
						MaintainerSettings.Issues.emptyArrayItems = EditorGUILayout.ToggleLeft(new GUIContent("Empty array items", "Look for any unreferenced items in arrays."), MaintainerSettings.Issues.emptyArrayItems);
						var show = MaintainerSettings.Issues.emptyArrayItems;
						if (show)
						{
							EditorGUI.indentLevel++;
							MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Skip prefab files", "Ignore empty array items in prefab files."), MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs);
							EditorGUI.indentLevel--;
						}
						MaintainerSettings.Issues.unnamedLayers = EditorGUILayout.ToggleLeft(new GUIContent("Objects with unnamed layers", "Search for GameObjects with unnamed layers."), MaintainerSettings.Issues.unnamedLayers);
						MaintainerSettings.Issues.hugePositions = EditorGUILayout.ToggleLeft(new GUIContent("Objects with huge positions", "Search for GameObjects with huge world positions (> |100 000| on any axis)."), MaintainerSettings.Issues.hugePositions);
					}

					//UIHelpers.UnIndent();
				}
				GUI.enabled = true;

				// ----------------------------------------------------------------------------
				// Project Settings Issues
				// ----------------------------------------------------------------------------

				GUI.enabled = UIHelpers.ToggleFoldout(ref MaintainerSettings.Issues.scanProjectSettings, ref MaintainerSettings.Issues.projectSettingsFoldout, new GUIContent("<b>Project Settings Issues</b>", "Group of issues related to the settings of the current project."));
				if (MaintainerSettings.Issues.projectSettingsFoldout)
				{
					UIHelpers.Indent();

					MaintainerSettings.Issues.duplicateScenesInBuild = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate scenes in build", "Search for the duplicates at the 'Scenes In Build' section of the Build Settings."), MaintainerSettings.Issues.duplicateScenesInBuild);
					MaintainerSettings.Issues.duplicateTagsAndLayers = EditorGUILayout.ToggleLeft(new GUIContent("Duplicates in Tags and Layers", "Search for the duplicate items at the 'Tags and Layers' Project Settings."), MaintainerSettings.Issues.duplicateTagsAndLayers);

					UIHelpers.UnIndent();
				}
				GUI.enabled = true;

				GUILayout.EndScrollView();
				UIHelpers.Separator();

				using (new GUILayout.HorizontalScope())
				{
					if (UIHelpers.ImageButton("Check all", CSIcons.SelectAll))
					{
						MaintainerSettings.Issues.SwitchAll(true);
					}

					if (UIHelpers.ImageButton("Uncheck all", CSIcons.SelectNone))
					{
						MaintainerSettings.Issues.SwitchAll(false);
					}
				}
			}

			if (UIHelpers.ImageButton("Reset", "Resets settings to defaults.", CSIcons.Restore))
			{
				MaintainerSettings.Issues.Reset();
			}
		}

		protected override void DrawRightPanelTop()
		{
			if (UIHelpers.ImageButton("1. Find issues!", CSIcons.Find))
			{
				EditorApplication.delayCall += StartSearch;
			}

			if (UIHelpers.ImageButton("2. Automatically fix selected issues if possible", CSIcons.AutoFix))
			{
				EditorApplication.delayCall += StartFix;
			}
		}

		protected override void DrawPagesRightHeader()
		{
			base.DrawPagesRightHeader();

			GUILayout.Label("Sorting:", GUILayout.ExpandWidth(false));

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Issues.sortingType = (IssuesSortingType)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.sortingType, GUILayout.Width(100));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Issues.sortingDirection = (SortingDirection)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.sortingDirection, GUILayout.Width(80));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}
		}

		protected override void DrawRecord(IssueRecord record, int recordIndex)
		{
			// hide fixed records 
			if (record.@fixed) return;

			using (new GUILayout.VerticalScope())
			{
				if (recordIndex > 0 && recordIndex < filteredRecords.Length) UIHelpers.Separator();

				using (new GUILayout.HorizontalScope())
				{
					DrawRecordCheckbox(record);
					DrawExpandCollapseButton(record);
					DrawSeverityIcon(record);
					
					if (record.compactMode)
					{
						DrawRecordButtons(record, recordIndex);
						GUILayout.Label(record.GetCompactLine(), UIHelpers.richLabel);
					}
					else
					{
						GUILayout.Space(5);
						GUILayout.Label(record.GetHeader(), UIHelpers.richLabel);
					}

					if (record.location == RecordLocation.Prefab)
					{
						GUILayout.Space(3);
						UIHelpers.Icon(CSEditorIcons.PrefabIcon, "Issue found in the Prefab.");
					}
				}

				if (!record.compactMode)
				{
					UIHelpers.Separator();
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Space(5);
						GUILayout.Label(record.GetBody(), UIHelpers.richLabel);
					}
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Space(5);
						DrawRecordButtons(record, recordIndex);
					}
					GUILayout.Space(3);
				}
			}
		}

		protected override string GetReportFileNamePart()
		{
			return "Issues";
		}

		protected override void AfterClearRecords()
		{
			SearchResultsStorage.IssuesSearchResults = null;
		}

		private void StartSearch()
		{
			window.RemoveNotification();
			IssuesFinder.StartSearch(true);
			window.Focus();
		}

		private void StartFix()
		{
			window.RemoveNotification();
			IssuesFinder.StartFix();
			window.Focus();
		}

		private void DrawRecordButtons(IssueRecord record, int recordIndex)
		{
			DrawShowButtonIfPossible(record);
			DrawFixButton(record, recordIndex);

			if (!record.compactMode)
			{
				DrawCopyButton(record);
				DrawHideButton(record, recordIndex);
			}

			var objectIssue = record as ObjectIssueRecord;
			if (objectIssue != null)
			{
				DrawMoreButton(objectIssue);
			}
		}

		private void DrawFixButton(IssueRecord record, int recordIndex)
		{
			GUI.enabled = record.CanBeFixed();

			var label = "Fix";
			var hint = "Automatically fixes issue (not available for this issue yet).";

			if (record.type == RecordType.MissingComponent)
			{
				label = "Remove";
				hint = "Removes missing component.";
			}
			else if (record.type == RecordType.MissingReference)
			{
				label = "Reset";
				hint = "Resets missing reference to default None value.";
			}

			if (UIHelpers.RecordButton(record, label, hint, CSIcons.AutoFix))
			{
				if (record.Fix(false))
				{
					HideRecord(recordIndex);

					var notificationExtra = "";

					if (record.location == RecordLocation.Prefab || record.location == RecordLocation.Asset)
					{
						AssetDatabase.SaveAssets();
					}

					MaintainerWindow.ShowNotification("Issue successfully fixed!" + notificationExtra);
				}
			}

			GUI.enabled = true;
		}

		private void DrawHideButton(IssueRecord record, int recordIndex)
		{
			if (UIHelpers.RecordButton(record, "Hide", "Hides this issue from the results list.\nUseful when you fixed issue and wish to hide it away.", CSIcons.Hide))
			{
				HideRecord(recordIndex);
			}
		}

		private void HideRecord(int index)
		{
			recordToDeleteIndex = index;
			EditorApplication.delayCall += DeleteRecord;
		}

		private void DrawMoreButton(ObjectIssueRecord record)
		{
			if (!UIHelpers.RecordButton(record, "Shows menu with additional actions for this record.", CSIcons.More)) return;

			var menu = new GenericMenu();
			if (!string.IsNullOrEmpty(record.path))
			{
				menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
				{
					if (!CSEditorTools.IsValueMatchesAnyFilter(record.path, MaintainerSettings.Issues.pathIgnoresFilters))
					{
						var newFilter = FilterItem.Create(record.path, FilterKind.Path);
						ArrayUtility.Add(ref MaintainerSettings.Issues.pathIgnoresFilters, newFilter);

						MaintainerWindow.ShowNotification("Ignore added: " + record.path);
						CleanerFiltersWindow.Refresh();
					}
					else
					{
						MaintainerWindow.ShowNotification("Such item already added to the ignores!");
					}
				});

				var dir = Directory.GetParent(record.path);
				if (dir.Name != "Assets")
				{
					menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
					{
						if (!CSEditorTools.IsValueMatchesAnyFilter(dir.ToString(), MaintainerSettings.Issues.pathIgnoresFilters))
						{
							var newFilter = FilterItem.Create(dir.ToString(), FilterKind.Path);
							ArrayUtility.Add(ref MaintainerSettings.Issues.pathIgnoresFilters, newFilter);

							MaintainerWindow.ShowNotification("Ignore added: " + dir);
							CleanerFiltersWindow.Refresh();
						}
						else
						{
							MaintainerWindow.ShowNotification("Such item already added to the ignores!");
						}
					});
				}
			}

			if (!string.IsNullOrEmpty(record.componentName))
			{
				menu.AddItem(new GUIContent("Ignore/Add component to ignores"), false, () =>
				{
					if (!CSEditorTools.IsValueMatchesAnyFilter(record.componentName, MaintainerSettings.Issues.componentIgnoresFilters))
					{
						var newFilter = FilterItem.Create(record.componentName, FilterKind.Type);
						ArrayUtility.Add(ref MaintainerSettings.Issues.pathIgnoresFilters, newFilter);

						MaintainerWindow.ShowNotification("Ignore added: " + record.componentName);
						CleanerFiltersWindow.Refresh();
					}
					else
					{
						MaintainerWindow.ShowNotification("Such item already added to the ignores!");
					}
				});
			}
			menu.ShowAsContext();
		}

		private bool DrawSettingsSearchSectionHeader(SettingsSearchSection section, ref bool foldout)
		{
			GUILayout.Space(5);
			using (new GUILayout.HorizontalScope())
			{
				foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(true, GUILayout.Width(165)), foldout, ObjectNames.NicifyVariableName(section.ToString()), true, UIHelpers.richFoldout);

				if (UIHelpers.IconButton(CSIcons.SelectAll))
				{
					typeof(IssuesFinderSettings).InvokeMember("Switch" + section, BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, MaintainerSettings.Issues, new[] {(object)true});
				}

				if (UIHelpers.IconButton(CSIcons.SelectNone))
				{
					typeof(IssuesFinderSettings).InvokeMember("Switch" + section, BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, MaintainerSettings.Issues, new[] {(object)false});
				}
			}
			UIHelpers.Separator();

			return foldout;
		}

		private void DrawSeverityIcon(IssueRecord record)
		{
			Texture icon;

			if (record == null) return;

			switch (record.severity)
			{
				case RecordSeverity.Error:
					icon = CSEditorIcons.ErrorSmallIcon;
					break;
				case RecordSeverity.Warning:
					icon = CSEditorIcons.WarnSmallIcon;
					break;
				case RecordSeverity.Info:
					icon = CSEditorIcons.InfoSmallIcon;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var iconArea = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
			var iconRect = new Rect(iconArea);

			GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleAndCrop);
		}

		
	}

	internal enum SettingsSearchSection : byte
	{
		Common,
		PrefabsSpecific,
		UnreferencedComponents,
		Neatness,
	}
}