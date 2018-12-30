#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.References;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class ReferencesTreeViewItem<T> : MaintainerTreeViewItem<T> where T : ReferencesTreeElement
	{
		public ReferencesTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName, data) {}

		protected override void Initialize()
		{
			if (depth == -1) return;

			if (data.recursionId == -1)
			{
				if (data.assetIsTexture)
				{
					icon = AssetPreview.GetMiniTypeThumbnail(typeof(Texture));
				}
				else
				{
					if (data.assetSettingsKind == AssetSettingsKind.NotSettings)
					{
						icon = (Texture2D)AssetDatabase.GetCachedIcon(data.assetPath);
					}
					else
					{
						icon = (Texture2D)CSIcons.Gear;
					}
				}

				if (icon == null)
				{
					icon = (Texture2D)CSEditorIcons.WarnSmallIcon;
				}
			}
			else
			{
				icon = (Texture2D)CSIcons.Repeat;
			}
		}
	}

	internal class ReferencesTreeView<T> : MaintainerTreeView<T> where T : ReferencesTreeElement
	{
		private const int EYE_BUTTON_SIZE = 20;
		private const int EYE_BUTTON_PADDING = 4;
		private const int ICON_WIDTH = 16;
		private const int ICON_PADDING = 7;
		private const int MULTILINE_CELL_Y_OFFSET = 4;
		private const int DEPTH_INDENTATION = 10;
		
		public enum SortOption
		{
			AssetPath,
			AssetType,
			AssetSize,
			ReferencesCount,
		}

		private enum Columns
		{
			Path,
			Type,
			Size,
			ReferencesCount
		}

		// count should be equal to columns count
		private readonly SortOption[] sortOptions =
		{
			SortOption.AssetPath,
			SortOption.AssetType,
			SortOption.AssetSize,
			SortOption.ReferencesCount,
		};

		public ReferencesTreeView(TreeViewState state, TreeModel<T> model) : base (state, model) {}

		public ReferencesTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader, model) {}

		public void SelectRowWithPath(string path)
		{
			foreach (var row in rows)
			{
				if (((MaintainerTreeViewItem<T>)row).data.assetPath == path)
				{
					var rowLocal = row;
					EditorApplication.delayCall += () =>
					{
						var id = rowLocal.id;
						FrameItem(id);
						SetSelection(new List<int> { id });
						SetFocusAndEnsureSelectedItem();
						MaintainerWindow.RepaintInstance();
					};
				}
			}
		}

		protected override void PostInit()
		{
			columnIndexForTreeFoldouts = 0;
		}

		protected override IList<int> GetAncestors(int id)
		{
			return TreeModel.GetAncestors(id);
		}

		protected override IList<int> GetDescendantsThatHaveChildren(int id)
		{
			return TreeModel.GetDescendantsThatHaveChildren(id);
		}

		protected override TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data)
		{
			return new ReferencesTreeViewItem<T>(id, depth, name, data);
		}

		protected override void SortByMultipleColumns()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<ReferencesTreeViewItem<T>>();
			var orderedQuery = InitialOrder(myTypes, sortedColumns);
			for (var i = 1; i < sortedColumns.Length; i++)
			{
				var sortOption = sortOptions[sortedColumns[i]];
				var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.AssetPath:
						orderedQuery = orderedQuery.ThenBy(l => l.data.assetPath, ascending);
						break;
					case SortOption.AssetType:
						orderedQuery = orderedQuery.ThenBy(l => l.data.assetTypeName, ascending);
						break;
					case SortOption.AssetSize:
						orderedQuery = orderedQuery.ThenBy(l => l.data.assetSize, ascending);
						break;
					case SortOption.ReferencesCount:
						orderedQuery = orderedQuery.ThenBy(l => l.data.ChildrenCount, ascending);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
		}

		private IOrderedEnumerable<ReferencesTreeViewItem<T>> InitialOrder(IEnumerable<ReferencesTreeViewItem<T>> myTypes, IList<int> history)
		{
			var sortOption = sortOptions[history[0]];
			var ascending = multiColumnHeader.IsSortedAscending(history[0]);

			switch (sortOption)
			{
				case SortOption.AssetPath:
					return myTypes.Order(l => l.data.assetPath, ascending);
				case SortOption.AssetType:
					return myTypes.Order(l => l.data.assetTypeName, ascending);
				case SortOption.AssetSize:
					return myTypes.Order(l => l.data.assetSize, ascending);
				case SortOption.ReferencesCount:
					return myTypes.Order(l => l.data.ChildrenCount, ascending);
				default:
					return myTypes.Order(l => l.data.name, ascending);
			}
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (ReferencesTreeViewItem<T>)args.item;

			for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
			}
		}

		protected override void OnSortingChanged(MultiColumnHeader header)
		{
			base.OnSortingChanged(header);
			RefreshCustomRowHeights();
		}

		protected override float GetCustomRowHeight(int row, TreeViewItem item)
		{
			var referencesItem = (ReferencesTreeViewItem<T>) item;
			if (referencesItem.data.referencingEntries != null)
			{
				return ROW_HEIGHT * (1 + referencesItem.data.referencingEntries.Length);
			}

			return base.GetCustomRowHeight(row, item);
		}

		protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
		{
			var paths = DragAndDrop.paths;

			if (paths != null && paths.Length > 0)
			{
				if (Event.current.type == EventType.DragPerform)
				{
					EditorApplication.delayCall += () => { ReferencesFinder.AddToSelectionAndRun(paths); };
					DragAndDrop.AcceptDrag();
				}

				return DragAndDropVisualMode.Generic;
			}

			return DragAndDropVisualMode.Rejected;
		}

		private void CellGUI(Rect cellRect, ReferencesTreeViewItem<T> item, Columns column, ref RowGUIArgs args)
		{
			baseIndent = item.depth * DEPTH_INDENTATION;

			if (item.data.referencingEntries == null)
			{
				CenterRectUsingSingleLineHeight(ref cellRect);
			}
			else
			{
				var lines = 1 + (item.data.referencingEntries != null ? item.data.referencingEntries.Length : 0);
				var cellHeight = lines * ROW_HEIGHT;

				if (cellRect.height > cellHeight)
				{
					cellRect.y += (cellRect.height - cellHeight) * 0.5f;
					cellRect.height = cellHeight;
				}

				cellRect.y += MULTILINE_CELL_Y_OFFSET;
			}

			switch (column)
			{
				case Columns.Path:
					
					var iconPadding = !Provider.isActive ? 0 : ICON_PADDING;
					var entryRect = cellRect;

					var num = GetContentIndent(item) + extraSpaceBeforeIconAndLabel;
					entryRect.xMin += num;

					if (item.icon != null)
					{
						var iconRect = entryRect;
						iconRect.width = ICON_WIDTH;
						iconRect.x += iconPadding;	
						iconRect.height = EditorGUIUtility.singleLineHeight;

						GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);

						// BASED ON DECOMPILED CODE
						// AssetsTreeViewGUI:
						// float num = (!Provider.isActive) ? 0f : 7f;
						// iconRightPadding = num;
						// iconLeftPadding = num;

						// TreeViewGUI:
						// iconTotalPadding = iconLeftPadding + iconRightPadding

						entryRect.xMin +=

							// TreeViewGUI: public float k_IconWidth = 16f;
							ICON_WIDTH +

							// TreeViewGUI: iconTotalPadding
							iconPadding * 2 +

							// TreeViewGUI: public float k_SpaceBetweenIconAndText = 2f;
							2f;
					}

					var eyeButtonRect = entryRect;
					eyeButtonRect.width = EYE_BUTTON_SIZE;
					eyeButtonRect.height = EYE_BUTTON_SIZE;
					eyeButtonRect.x += EYE_BUTTON_PADDING;

					if (UIHelpers.IconButton(eyeButtonRect, CSIcons.Show))
					{
						ShowItem(item);
					}

					var labelRect = entryRect;
					labelRect.xMin = eyeButtonRect.xMax + EYE_BUTTON_PADDING;

					if (item.data.depth == 0 && !item.data.HasChildren)
					{
						GUI.contentColor = CSColors.labelDimmedColor;
					}
					DefaultGUI.Label(labelRect, args.label, args.selected, args.focused);

					GUI.contentColor = Color.white;

					if (item.data.referencingEntries != null)
					{
						var referencingEntriesCount = item.data.referencingEntries.Length;

						if (referencingEntriesCount > 0)
						{
							var warningIconRect = eyeButtonRect;
							warningIconRect.width = 16;
							warningIconRect.height = 16;
							warningIconRect.x += 4;

							var entriesNotFound = item.data.referencingEntries.All(e => e.location == Location.NotFound);

							var boxRect = entryRect;
							boxRect.yMin += ROW_HEIGHT - 4f;
							boxRect.height = referencingEntriesCount * ROW_HEIGHT - 2f;

							GUI.backgroundColor = !entriesNotFound ? CSColors.backgroundGreenTint : CSColors.backgroundRedTint;
							GUI.Box(boxRect, GUIContent.none);
							GUI.backgroundColor = Color.white;

							for (var i = 0; i < referencingEntriesCount; i++)
							{
								var entry = item.data.referencingEntries[i];
								labelRect.y += ROW_HEIGHT;
								eyeButtonRect.y += ROW_HEIGHT;

								if (entry.location == Location.NotFound)
								{
									warningIconRect.y = eyeButtonRect.y;
									GUI.DrawTexture(warningIconRect, CSEditorIcons.WarnSmallIcon, ScaleMode.ScaleAndCrop);
								}
								else if (entry.location == Location.Invisible)
								{
									warningIconRect.y = eyeButtonRect.y;
									GUI.DrawTexture(warningIconRect, CSEditorIcons.InfoSmallIcon, ScaleMode.ScaleAndCrop);
								}
								else
								{
									if (UIHelpers.IconButton(eyeButtonRect, CSIcons.Show))
									{
										ShowItem(item, entry);
									}
								}
								DefaultGUI.Label(labelRect, entry.label, args.selected, args.focused);
							}

						}
					}
					
					break;

				case Columns.Type:

					DefaultGUI.Label(cellRect, item.data.assetTypeName, args.selected, args.focused);
					break;

				case Columns.Size:

					DefaultGUI.Label(cellRect, item.data.assetSizeFormatted, args.selected, args.focused);
					break;

				case Columns.ReferencesCount:

					DefaultGUI.Label(cellRect, item.data.ChildrenCount.ToString(), args.selected, args.focused);
					break;
				
				default:
					throw new ArgumentOutOfRangeException("column", column, null);
			}
		}

		private static void ShowItem(ReferencesTreeViewItem<T> item, ReferencingEntryData referencingEntry = null)
		{
			var assetPath = item.data.assetPath;

			if (referencingEntry != null)
			{
				if (referencingEntry.location == Location.LightingSettings || referencingEntry.location == Location.Navigation)
				{
					var sceneOpenResult = CSSceneTools.OpenSceneWithSavePrompt(assetPath);
					if (!sceneOpenResult.success)
					{
						Debug.LogError(Maintainer.ConstructError("Can't open scene " + assetPath));
						MaintainerWindow.ShowNotification("Can't show it properly");
						return;
					}
				}

				switch (referencingEntry.location)
				{
					case Location.GameObject:

						if (!CSEditorTools.RevealAndSelect(assetPath, referencingEntry.transformPath, referencingEntry.objectId, referencingEntry.componentId, referencingEntry.propertyName))
						{
							MaintainerWindow.ShowNotification("Can't show it properly");
						}
						break;

					case Location.LightingSettings:

						if (!EditorApplication.ExecuteMenuItem("Window/Lighting/Settings"))
						{
							Debug.LogError(Maintainer.ConstructError("Can't open Lighting settings!"));
							MaintainerWindow.ShowNotification("Can't show it properly");
						}
						break;

					case Location.Navigation:

						if (!EditorApplication.ExecuteMenuItem("Window/Navigation"))
						{
							Debug.LogError(Maintainer.ConstructError("Can't open Navigation settings!"));
							MaintainerWindow.ShowNotification("Can't show it properly");
						}
						break;

					case Location.NotFound:
					case Location.Invisible:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			else
			{
				if (item.data.assetSettingsKind == AssetSettingsKind.NotSettings)
				{
					if (!CSEditorTools.RevealAndSelect(assetPath, string.Empty, -1, -1, string.Empty))
					{
						MaintainerWindow.ShowNotification("Can't show it properly");
					}
				}
				else
				{
					if (!CSEditorTools.RevealInSettings(item.data.assetSettingsKind, assetPath))
					{
						MaintainerWindow.ShowNotification("Can't show it properly");
					}
				}
			}
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Path", "Paths to the assets."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 200,
					minWidth = 400,
					autoResize = true,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Type", CSEditorIcons.FilterByType, "Assets types."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 100,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Size", "Assets sizes."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 100,
					minWidth = 100,
					autoResize = false,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Refs", "Shows how much times asset was referenced somewhere."),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = false,
					canSort = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 50,
					minWidth = 50,
					maxWidth = 50,
					autoResize = false,
					allowToggleVisibility = true
				},
			};

			var state = new MultiColumnHeaderState(columns)
			{
				sortedColumns = new[] {0, 3},
				sortedColumnIndex = 3
			};
			return state;
		}
	}
}
#endif