#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using UnityEngine;
using CodeStage.Maintainer.References;

#if UNITY_5_6_OR_NEWER
using System;
using UnityEditor.IMGUI.Controls;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Filters;
using UnityEditor;
#endif

namespace CodeStage.Maintainer.UI
{
	internal class ReferencesTab
	{
#if UNITY_5_6_OR_NEWER
		public static string pathToSelect;
		public static bool showAlreadyExistNotification;

		private readonly MaintainerWindow window;

		private ReferencesTreeElement[] treeElements;
		private TreeModel<ReferencesTreeElement> treeModel;
		private ReferencesTreeView<ReferencesTreeElement> treeView;
		private SearchField searchField;
		private GUIContent caption;

		public ReferencesTab(MaintainerWindow window)
		{
			this.window = window;
		}

		public GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(ReferencesFinder.ModuleName, CSIcons.Find);
				}
				return caption;
			}
		}

		public void Refresh(bool newData)
		{
			if (newData)
			{
				MaintainerSettings.References.treeViewState = new TreeViewState();
				treeModel = null;
			}

			if (treeModel == null)
			{
				var firstInit = MaintainerSettings.References.multiColumnHeaderState == null || MaintainerSettings.References.multiColumnHeaderState.columns == null || MaintainerSettings.References.multiColumnHeaderState.columns.Length == 0;
				var headerState = ReferencesTreeView<ReferencesTreeElement>.CreateDefaultMultiColumnHeaderState();
				if (MultiColumnHeaderState.CanOverwriteSerializedFields(MaintainerSettings.References.multiColumnHeaderState, headerState))
					MultiColumnHeaderState.OverwriteSerializedFields(MaintainerSettings.References.multiColumnHeaderState, headerState);
				MaintainerSettings.References.multiColumnHeaderState = headerState;

				var multiColumnHeader = new MaintainerMultiColumnHeader(headerState);

				if (firstInit)
				{
					multiColumnHeader.ResizeToFit();
					MaintainerSettings.References.treeViewState = new TreeViewState();
				}

				treeElements = LoadLastTreeElements();
				treeModel = new TreeModel<ReferencesTreeElement>(treeElements);
				treeView = new ReferencesTreeView<ReferencesTreeElement>(MaintainerSettings.References.treeViewState, multiColumnHeader, treeModel);
				treeView.SetSearchString(MaintainerSettings.References.searchString);
				treeView.Reload();

				searchField = new SearchField();
				searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
			}

			if (!string.IsNullOrEmpty(pathToSelect))
			{
				EditorApplication.delayCall += () =>
				{
					treeView.SelectRowWithPath(pathToSelect);
					pathToSelect = null;
				};
			}

			if (showAlreadyExistNotification)
			{
				window.ShowNotification(new GUIContent("Such item(s) already present in the list!"));
				showAlreadyExistNotification = false;
			}
		}

		public virtual void Draw()
		{
			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
			{
				GUILayout.Space(5);

				using (new GUILayout.HorizontalScope())
				{
					using (new GUILayout.VerticalScope())
					{
						GUILayout.Label("<size=13>Here you may check any project assets for all project references.</size>", UIHelpers.richWordWrapLabel);

						if (UIHelpers.ImageButton("Find all assets references",
							"Traverses whole project to find where all assets are referenced.", CSIcons.Find))
						{
							if (Event.current.control && Event.current.shift)
							{
								ReferencesFinder.debugMode = true;
								AssetsMap.Delete();
								Event.current.Use();
							}
							else
							{
								ReferencesFinder.debugMode = false;
							}
							EditorApplication.delayCall += StartProjectReferencesScan;
						}

						if (CSEditorTools.GetProjectSelections(true).Length == 0)
						{
							GUI.enabled = false;
						}
						if (UIHelpers.ImageButton("Find selected assets references",
							"Adds selected Project View assets to the current search results.", CSIcons.Find))
						{
							EditorApplication.delayCall += () => ReferencesFinder.AddSelectedToSelectionAndRun();
						}
						GUI.enabled = true;
					}

					GUILayout.Space(30);

					using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.Width(250)))
					{
						GUILayout.Space(5);
						using (new GUILayout.HorizontalScope())
						{
							GUILayout.Space(3);
							if (UIHelpers.ImageButton("Manage Filters... (" + MaintainerSettings.References.pathIgnoresFilters.Length + ")",
								CSIcons.Gear, GUILayout.ExpandWidth(false)))
							{
								ReferencesFiltersWindow.Create();
							}
							GUILayout.FlexibleSpace();
							if (UIHelpers.ImageButton(null, "Show some extra info and notes about " + ReferencesFinder.ModuleName + ".", CSIcons.HelpOutline, GUILayout.ExpandWidth(false)))
							{
								EditorUtility.DisplayDialog(ReferencesFinder.ModuleName + " Extra Info",
									"You may use buttons in " + ReferencesFinder.ModuleName + " tab or Project Browser context menu command '" + MaintainerMenu.ProjectBrowserContext + "'.\nOr just drag && drop items from Project Browser to the list." + "\n\n" +
									"Note #1: you'll see only those connections which Maintainer was able to figure out. Some kinds of connections can't be statically found or not supported yet." + "\n\n" +
									"Note #2: not referenced assets still may be used at runtime or from Editor scripting.", "OK");
							}

							GUILayout.Space(3);
						}

						MaintainerSettings.References.showAssetsWithoutReferences = GUILayout.Toggle(
							MaintainerSettings.References.showAssetsWithoutReferences,
							new GUIContent("Add assets without found references", "Check to see all scanned assets in the list even if there was no any references to the asset found in project."), GUILayout.ExpandWidth(false));

						MaintainerSettings.References.selectedFindClearsResults = GUILayout.Toggle(
							MaintainerSettings.References.selectedFindClearsResults,
							new GUIContent(@"Clear results on selected assets search", "Check to automatically clear last results on selected assets find both from context menu and main window.\nUncheck to add new results to the last results."), GUILayout.ExpandWidth(false));

						GUILayout.Space(3);
					}
				}

				GUILayout.Space(5);
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(5);
					using (new GUILayout.VerticalScope())
					{
						EditorGUI.BeginChangeCheck();
						var searchString = searchField.OnGUI(GUILayoutUtility.GetRect(0, 0, 20, 20, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false)), MaintainerSettings.References.searchString);
						if (EditorGUI.EndChangeCheck())
						{
							MaintainerSettings.References.searchString = searchString;
							treeView.SetSearchString(searchString);
							treeView.Reload();
						}
						treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
					}
					GUILayout.Space(5);
				}

				GUILayout.Space(5);

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Space(5);

					if (SearchResultsStorage.ReferencesSearchSelection.Length == 0)
					{
						GUI.enabled = false;
					}
					if (UIHelpers.ImageButton("Refresh", "Restarts references search for the previous results.", CSIcons.Repeat))
					{
						if (Event.current.control && Event.current.shift)
						{
							ReferencesFinder.debugMode = true;
							AssetsMap.Delete();
							Event.current.Use();
						}
						else
						{
							ReferencesFinder.debugMode = false;
						}

						EditorApplication.delayCall += () =>
						{
							ReferencesFinder.GetReferences(SearchResultsStorage.ReferencesSearchSelection);
						};
					}
					GUI.enabled = true;

					if (UIHelpers.ImageButton("Collapse all", "Collapses all tree items.", CSIcons.Collapse))
					{
						treeView.CollapseAll();
					}

					if (UIHelpers.ImageButton("Expand all", "Expands all tree items.", CSIcons.Expand))
					{
						treeView.ExpandAll();
					}

					if (UIHelpers.ImageButton("Clear results", "Clears results tree and empties cache.", CSIcons.Clear))
					{
						SearchResultsStorage.ReferencesSearchResults = null;
						SearchResultsStorage.ReferencesSearchSelection = null;
						Refresh(true);
					}
					GUILayout.Space(5);
				}
			}
		}

		private ReferencesTreeElement[] LoadLastTreeElements()
		{
			var loaded = SearchResultsStorage.ReferencesSearchResults;  
			if (loaded == null || loaded.Length == 0)
			{
				loaded = new ReferencesTreeElement[1];
				loaded[0] = new ReferencesTreeElement { id = 0, depth = -1, name = "root"};
			}
			return loaded;
		}

		private void StartProjectReferencesScan()
		{
			window.RemoveNotification();
			ReferencesFinder.GetReferences();
			window.Focus();
		}

		internal class MaintainerMultiColumnHeader : MultiColumnHeader
		{
			private HeaderMode mode;

			public enum HeaderMode
			{
				LargeHeader,
				DefaultHeader,
				MinimumHeaderWithoutSorting
			}

			public MaintainerMultiColumnHeader(MultiColumnHeaderState state) : base(state)
			{
				Mode = HeaderMode.DefaultHeader;
			}

			public HeaderMode Mode
			{
				get { return mode; }
				set
				{
					mode = value;
					switch (mode)
					{
						case HeaderMode.LargeHeader:
							canSort = true;
							height = 37f;
							break;
						case HeaderMode.DefaultHeader:
							canSort = true;
							height = DefaultGUI.defaultHeight;
							break;
						case HeaderMode.MinimumHeaderWithoutSorting:
							canSort = false;
							height = DefaultGUI.minimumHeight;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			// EXAMPLE FOR FUTURE REFERENCE
			/*protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
			{
				base.ColumnHeaderGUI(column, headerRect, columnIndex);

				if (Mode == HeaderMode.LargeHeader)
				{
					if (columnIndex > 2)
					{
						headerRect.xMax -= 3f;
						var oldAlignment = EditorStyles.largeLabel.alignment;
						EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
						GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
						EditorStyles.largeLabel.alignment = oldAlignment;
					}
				}
			}*/
		}
#else
		private GUIContent caption;
		internal GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(ReferencesFinder.ModuleName, CSIcons.Find);
				}
				return caption;
			}
		}

		public ReferencesTab(MaintainerWindow window)
		{
			
		}

		public virtual void Refresh(bool newData)
		{

		}

		public virtual void Draw()
		{
			GUILayout.Space(10);
			GUILayout.Label("This module requires Unity 5.6.0 or newer.");
		}
#endif
	}
}
