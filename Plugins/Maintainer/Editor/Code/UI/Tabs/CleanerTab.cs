#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Linq;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Filters;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class CleanerTab : RecordsTab<CleanerRecord>
	{
		private CleanerResultsStats resultsStats;
		private GUIContent caption;

		public CleanerTab(MaintainerWindow maintainerWindow) : base(maintainerWindow)
		{
		}

		internal GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(ProjectCleaner.ModuleName, CSIcons.Clean);
				}
				return caption;
			}
		}

		protected override CleanerRecord[] LoadLastRecords()
		{
			var loadedRecords = SearchResultsStorage.CleanerSearchResults;
			if (loadedRecords == null) loadedRecords = new CleanerRecord[0];
			if (resultsStats == null) resultsStats = new CleanerResultsStats();
			
			return loadedRecords;
		}

		protected override RecordsTabState GetState()
		{
			return MaintainerSettings.Cleaner.tabState;
		}

		protected override void PerformPostRefreshActions()
		{
			base.PerformPostRefreshActions();
			resultsStats.Update(filteredRecords);
		}

		protected override void DrawSettingsBody()
		{
			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Manage Filters...", CSIcons.Gear))
				{
					CleanerFiltersWindow.Create();
				}

				GUILayout.Space(5);
				EditorGUI.BeginChangeCheck();
				MaintainerSettings.Cleaner.useTrashBin = EditorGUILayout.ToggleLeft(new GUIContent("Use Trash Bin (drops clean speed)", "All deleted items will be moved to Trash if selected. Otherwise items will be deleted permanently.\nPlease note: dramatically reduces removal performance when enabled!"), MaintainerSettings.Cleaner.useTrashBin);
				if (EditorGUI.EndChangeCheck())
				{
					if (!MaintainerSettings.Cleaner.useTrashBin && !MaintainerSettings.Cleaner.trashBinWarningShown)
					{
						EditorUtility.DisplayDialog(ProjectCleaner.ModuleName, "Please note, in case of not using Trash Bin, files will be removed permanently, without possibility to recover them in case of mistake.\nAuthor is not responsible for any damage made due to the module usage!\nThis message shows only once.", "Dismiss");
						MaintainerSettings.Cleaner.trashBinWarningShown = true;
					}
				}

				UIHelpers.Separator();
				GUILayout.Space(5);
				GUILayout.Label("<b><size=12>Search for:</size></b>", UIHelpers.richLabel);
				using (new GUILayout.HorizontalScope())
				{
					MaintainerSettings.Cleaner.findUnreferencedAssets = EditorGUILayout.ToggleLeft(new GUIContent("Unused assets", "Search for unreferenced assets in project."), MaintainerSettings.Cleaner.findUnreferencedAssets, GUILayout.Width(100));
				}
				using (new GUILayout.HorizontalScope())
				{
					MaintainerSettings.Cleaner.findEmptyFolders = EditorGUILayout.ToggleLeft(new GUIContent("Empty folders", "Search for all empty folders in project."), MaintainerSettings.Cleaner.findEmptyFolders, GUILayout.Width(100));
					GUI.enabled = MaintainerSettings.Cleaner.findEmptyFolders;
					EditorGUI.BeginChangeCheck();
					MaintainerSettings.Cleaner.findEmptyFoldersAutomatically = EditorGUILayout.ToggleLeft(new GUIContent("Autoclean", "Perform empty folders clean automatically on every scripts reload."), MaintainerSettings.Cleaner.findEmptyFoldersAutomatically, GUILayout.Width(100));
					if (EditorGUI.EndChangeCheck())
					{
						if (MaintainerSettings.Cleaner.findEmptyFoldersAutomatically)
							EditorUtility.DisplayDialog(ProjectCleaner.ModuleName, "In case you're having thousands of folders in your project this may hang Unity for few additional secs on every scripts reload.\n" + Maintainer.DataLossWarning, "Understood");
					}
					GUI.enabled = true;
				}

				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Reset", "Resets settings to defaults.", CSIcons.Restore))
				{
					MaintainerSettings.Cleaner.Reset();
				}
			}
		}

		protected override void DrawLeftExtra()
		{
			using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(10);
				GUILayout.Label("<size=14><b>Statistics</b></size>", UIHelpers.centeredLabel);
				GUILayout.Space(10);

				DrawStatsBody();
			}
		}

		private void DrawStatsBody()
		{
			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground))
			{
				if (resultsStats == null)
				{
					GUILayout.Label("N/A");
				}
				else
				{
					GUILayout.Space(5);
					GUILayout.Label("Physical size");
					UIHelpers.Separator();
					GUILayout.Label("Total found: " + CSEditorTools.FormatBytes(resultsStats.totalSize));
					GUILayout.Label("Selected: " + CSEditorTools.FormatBytes(resultsStats.selectedSize));
					GUILayout.Space(5);
				}
			}
		}

		protected override void DrawRightPanelTop()
		{
			if (UIHelpers.ImageButton("1. Scan project", CSIcons.Find))
			{
				EditorApplication.delayCall += StartSearch;
			}

			if (UIHelpers.ImageButton("2. Delete selected garbage", CSIcons.Delete))
			{
				EditorApplication.delayCall += StartClean;
			}
		}

		protected override void DrawPagesRightHeader()
		{
			base.DrawPagesRightHeader();

			GUILayout.Label("Sorting:", GUILayout.ExpandWidth(false));

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Cleaner.sortingType = (CleanerSortingType)EditorGUILayout.EnumPopup(MaintainerSettings.Cleaner.sortingType, GUILayout.Width(100));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Cleaner.sortingDirection = (SortingDirection)EditorGUILayout.EnumPopup(MaintainerSettings.Cleaner.sortingDirection, GUILayout.Width(80));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}
		}

		protected override void DrawRecord(CleanerRecord record, int recordIndex)
		{
			if (record == null) return;

			// hide cleaned records 
			if (record.cleaned) return;

			using (new GUILayout.VerticalScope())
			{
				if (recordIndex > 0 && recordIndex < filteredRecords.Length) UIHelpers.Separator();

				using (new GUILayout.HorizontalScope())
				{
					DrawRecordCheckbox(record);
					DrawExpandCollapseButton(record);
					DrawIcon(record);

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

		protected override void ApplySorting()
		{
			base.ApplySorting();

			switch (MaintainerSettings.Cleaner.sortingType)
			{
				case CleanerSortingType.Unsorted:
					break;
				case CleanerSortingType.ByPath:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ? 
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordByPath).ToArray() : 
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				case CleanerSortingType.ByType:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordByType).ThenBy(RecordsSortings.cleanerRecordByAssetType).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordByType).ThenBy(RecordsSortings.cleanerRecordByAssetType).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				case CleanerSortingType.BySize:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordBySize).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordBySize).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void SaveSearchResults()
		{
			SearchResultsStorage.CleanerSearchResults = GetRecords();
			resultsStats.Update(filteredRecords);
		}

		protected override string GetModuleName()
		{
			return ProjectCleaner.ModuleName;
		}

		protected override string GetReportHeader()
		{
			return resultsStats != null ? "Total found garbage size: " + CSEditorTools.FormatBytes(resultsStats.totalSize) : null;
		}

		protected override string GetReportFileNamePart()
		{
			return "Cleaner";
		}

		protected override void AfterClearRecords()
		{
			resultsStats = null;
			SearchResultsStorage.CleanerSearchResults = null;
		}

		protected override void OnSelectionChanged()
		{
			resultsStats.Update(filteredRecords);
		}

		private void StartSearch()
		{
			window.RemoveNotification();
			ProjectCleaner.StartSearch(true);
			window.Focus();
		}

		private void StartClean()
		{
			window.RemoveNotification();
			ProjectCleaner.StartClean();
			window.Focus();
		}

		private void DrawRecordButtons(CleanerRecord record, int recordIndex)
		{
			DrawShowButtonIfPossible(record);

			var assetRecord = record as AssetRecord;
			if (assetRecord != null)
			{
				DrawDeleteButton(assetRecord, recordIndex);

				if (record.compactMode)
				{
					DrawMoreButton(assetRecord);
				}
				else
				{
					DrawRevealButton(assetRecord);
					DrawCopyButton(assetRecord);
					DrawMoreButton(assetRecord);
				}
			}
		}

		private void DrawIcon(CleanerRecord record)
		{
			Texture icon = null;

			var ar = record as AssetRecord;
			if (ar != null)
			{
				if (ar.isTexture)
				{
					icon = AssetPreview.GetMiniTypeThumbnail(ar.assetType);
				}

				if (icon == null)
				{
					icon = AssetDatabase.GetCachedIcon(ar.assetDatabasePath);
				}
			}

			var er = record as CleanerErrorRecord;
			if (er != null)
			{
				icon = CSEditorIcons.ErrorSmallIcon;
			}

			if (icon != null)
			{
				var position = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
				GUI.DrawTexture(position, icon);
			}
		}

		private void DrawDeleteButton(CleanerRecord record, int recordIndex)
		{
			if (UIHelpers.RecordButton(record, "Delete", "Deletes this single item.", CSIcons.Delete))
			{
				if (!MaintainerSettings.Cleaner.deletionPromptShown)
				{
					MaintainerSettings.Cleaner.deletionPromptShown = true;
					if (!EditorUtility.DisplayDialog(
						ProjectCleaner.ModuleName,
						"Please note, this action will physically remove asset file from the project! Are you sure you wish to do this?\n" +
						"Author is not responsible for any damage made due to the module usage!\n" +
						"This message shows only once.",
						"Yes", "No"))
					{
						return;
					}
				}

				if (record.Clean())
				{
					recordToDeleteIndex = recordIndex;
					EditorApplication.delayCall += DeleteRecord;
				}
			}
		}

		private void DrawRevealButton(AssetRecord record)
		{
			if (UIHelpers.RecordButton(record, "Reveal", "Reveals item in system default File Manager like Explorer on Windows or Finder on Mac.", CSIcons.Reveal))
			{
				EditorUtility.RevealInFinder(record.path);
			}
		}

		private void DrawMoreButton(AssetRecord assetRecord)
		{
			if (UIHelpers.RecordButton(assetRecord, "Shows menu with additional actions for this record.", CSIcons.More))
			{
				var menu = new GenericMenu();
				if (!string.IsNullOrEmpty(assetRecord.path))
				{
					menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
					{
						if (!CSEditorTools.IsValueMatchesAnyFilter(assetRecord.assetDatabasePath, MaintainerSettings.Cleaner.pathIgnoresFilters))
						{
							var newFilter = FilterItem.Create(assetRecord.assetDatabasePath, FilterKind.Path);
							ArrayUtility.Add(ref MaintainerSettings.Cleaner.pathIgnoresFilters, newFilter);

							MaintainerWindow.ShowNotification("Ignore added: " + assetRecord.assetDatabasePath);
							CleanerFiltersWindow.Refresh();
						}
						else
						{
							MaintainerWindow.ShowNotification("Such item already added to the ignores!");
						}
					});

					var dir = Directory.GetParent(assetRecord.assetDatabasePath);
					if (dir.Name != "Assets")
					{
						menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
						{
							if (!CSEditorTools.IsValueMatchesAnyFilter(dir.ToString(), MaintainerSettings.Cleaner.pathIgnoresFilters))
							{
								var newFilter = FilterItem.Create(dir.ToString(), FilterKind.Path);
								ArrayUtility.Add(ref MaintainerSettings.Cleaner.pathIgnoresFilters, newFilter);

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
				menu.ShowAsContext();
			}
		}

		private class CleanerResultsStats
		{
			public long totalSize;
			public long selectedSize;

			public void Update(CleanerRecord[] records)
			{
				selectedSize = totalSize = 0;

				for (var i = 0; i < records.Length; i++)
				{
					var assetRecord = records[i] as AssetRecord;
					if (assetRecord == null || assetRecord.cleaned) continue;

					totalSize += assetRecord.size;
					if (assetRecord.selected) selectedSize += assetRecord.size;
				}
			}
		}
	}
}
