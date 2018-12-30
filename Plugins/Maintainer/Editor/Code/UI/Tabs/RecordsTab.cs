#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	[Serializable]
	public class RecordsTabState
	{
		public List<bool> selection = new List<bool>();
		public List<bool> compaction = new List<bool>();
		public Vector2 searchSectionScrollPosition = Vector2.zero;
	}

	internal abstract class RecordsTab<T> where T : RecordBase
	{
		protected const int RecordsPerPage = 100;

		protected MaintainerWindow window;
		protected Vector2 searchSectionScrollPosition;
		protected Vector2 settingsSectionScrollPosition;
		protected int recordsCurrentPage;
		protected int recordsTotalPages;
		protected int recordToDeleteIndex;
		protected T[] filteredRecords;
		protected T[] records;
		private IShowableRecord gotoRecord;

		/* virtual methods */

		protected RecordsTab(MaintainerWindow window)
		{
			this.window = window;
		}

		public virtual void Refresh(bool newData)
		{
			records = null;
			filteredRecords = null;
			recordsCurrentPage = 0;
			searchSectionScrollPosition = Vector2.zero;
			if (newData)
				GetState().searchSectionScrollPosition = Vector2.zero;
			settingsSectionScrollPosition = Vector2.zero;
		}

		public virtual void Draw()
		{
			if (records == null)
			{
				records = LoadLastRecords();
				searchSectionScrollPosition = GetState().searchSectionScrollPosition;
				ApplySorting();
				ApplyState();
				recordsTotalPages = (int)Math.Ceiling((double)filteredRecords.Length / RecordsPerPage);
				PerformPostRefreshActions();
			}

			using (new GUILayout.HorizontalScope())
			{
				DrawLeftSection();
				DrawRightSection();
			}

			if (gotoRecord != null)
			{
				gotoRecord.Show();
				gotoRecord = null;
			}
		}

		protected virtual T[] GetRecords()
		{
			return records;
		}

		protected virtual void ClearRecords()
		{
			records = null;
			filteredRecords = null;
		}

		protected virtual void DeleteRecord()
		{
			var record = filteredRecords[recordToDeleteIndex];
			records = CSArrayTools.RemoveAt(records, Array.IndexOf(records, record));
			ApplySorting();

			GetState().selection.RemoveAt(recordToDeleteIndex);
			GetState().compaction.RemoveAt(recordToDeleteIndex);

			if (filteredRecords.Length > 0)
			{
				recordsTotalPages = (int)Math.Ceiling((double)filteredRecords.Length / RecordsPerPage);
			}
			else
			{
				recordsTotalPages = 1;
			}

			if (recordsCurrentPage + 1 > recordsTotalPages) recordsCurrentPage = recordsTotalPages - 1;

			SaveSearchResults();
			window.Repaint();
		}

		protected virtual void DrawLeftSection()
		{
			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.Width(240)))
			{
				DrawSettings();
				DrawLeftExtra();
			}
		}

		protected virtual void DrawSettings()
		{
			GUILayout.Space(10);
			GUILayout.Label("<size=14><b>Settings</b></size>", UIHelpers.centeredLabel);
			GUILayout.Space(10);

			DrawSettingsBody();
		}

		protected virtual void DrawRightSection()
		{
			using (new GUILayout.VerticalScope(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
			{
				GUILayout.Space(10);
				DrawRightPanel();
			}
		}

		protected virtual void DrawRightPanel()
		{
			DrawRightPanelTop();

			GUILayout.Space(5);

			if (filteredRecords == null || filteredRecords.Length <= 0) return;

			DrawCollectionPages();

			GUILayout.Space(5);

			DrawRightPanelBottom();
		}

		protected virtual void DrawRightPanelBottom()
		{
			using (new GUILayout.HorizontalScope())
			{
				DrawSelectAllButton();
				DrawSelectNoneButton();
				DrawExpandAllButton();
				DrawCollapseAllButton();
			}

			using (new GUILayout.HorizontalScope())
			{
				DrawCopyReportButton();
				DrawExportReportButton();
				DrawClearResultsButton();
			}
		}

		protected virtual void DrawCollectionPages()
		{
			var fromItem = recordsCurrentPage * RecordsPerPage;
			var toItem = fromItem + Math.Min(RecordsPerPage, filteredRecords.Length - fromItem);

			using (new GUILayout.HorizontalScope(UIHelpers.panelWithBackground))
			{
				GUILayout.Label(fromItem + 1 + " - " + toItem + " from " + filteredRecords.Length/* + " (" + records.Count + " total)"*/);
				GUILayout.FlexibleSpace();
				using (new GUILayout.HorizontalScope())
				{
					DrawPagesRightHeader();
				}
			}
			UIHelpers.Separator();

			DrawRecords(fromItem, toItem);

			UIHelpers.Separator();

			if (recordsTotalPages <= 1) return;

			GUILayout.Space(5);
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = recordsCurrentPage > 0;
				if (UIHelpers.IconButton(CSIcons.DoubleArrowLeft))
				{
					window.RemoveNotification();
					recordsCurrentPage = 0;
					searchSectionScrollPosition = Vector2.zero;
					GetState().searchSectionScrollPosition = Vector2.zero;
				}
				if (UIHelpers.IconButton(CSIcons.ArrowLeft))
				{
					window.RemoveNotification();
					recordsCurrentPage--;
					searchSectionScrollPosition = Vector2.zero;
					GetState().searchSectionScrollPosition = Vector2.zero;
				}
				GUI.enabled = true;
				GUILayout.Label(recordsCurrentPage + 1 + " of " + recordsTotalPages, UIHelpers.centeredLabel);
				GUI.enabled = recordsCurrentPage < recordsTotalPages - 1;
				if (UIHelpers.IconButton(CSIcons.ArrowRight))
				{
					window.RemoveNotification();
					recordsCurrentPage++;
					searchSectionScrollPosition = Vector2.zero;
					GetState().searchSectionScrollPosition = Vector2.zero;
				}
				if (UIHelpers.IconButton(CSIcons.DoubleArrowRight))
				{
					window.RemoveNotification();
					recordsCurrentPage = recordsTotalPages - 1;
					searchSectionScrollPosition = Vector2.zero;
					GetState().searchSectionScrollPosition = Vector2.zero;
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
			}
		}

		protected virtual void DrawRecords(int fromItem, int toItem)
		{
			searchSectionScrollPosition = GUILayout.BeginScrollView(searchSectionScrollPosition);
			GetState().searchSectionScrollPosition = searchSectionScrollPosition;
			for (var i = fromItem; i < toItem; i++)
			{
				var record = filteredRecords[i];

				DrawRecord(record, i);

				if (Event.current != null && Event.current.type == EventType.MouseDown)
				{
					var guiRect = GUILayoutUtility.GetLastRect();
					guiRect.height += 2; // to compensate the separator's gap

					if (guiRect.Contains(Event.current.mousePosition))
					{
						Event.current.Use();

						record.compactMode = !record.compactMode;
						GetState().compaction[i] = record.compactMode;
					}
				}
			}
			GUILayout.EndScrollView();
		}

		protected virtual void ApplySorting()
		{
			filteredRecords = records.ToArray();
		}

		protected virtual void DrawRecordCheckbox(RecordBase record)
		{
			EditorGUI.BeginChangeCheck();
			record.selected = EditorGUILayout.ToggleLeft(new GUIContent(""), record.selected, GUILayout.Width(12));
			if (EditorGUI.EndChangeCheck())
			{
				var index = Array.IndexOf(filteredRecords, record);
				GetState().selection[index] = record.selected;

				OnSelectionChanged();
			}
		}

		/* empty virtual methods */

		protected virtual void PerformPostRefreshActions() { }

		protected virtual void DrawPagesRightHeader() { }

		protected virtual void DrawLeftExtra() { }

		protected virtual string GetReportHeader() { return null; }

		protected virtual string GetReportFooter() { return null; }

		protected virtual string GetReportFileNamePart() { return ""; }

		protected virtual void AfterClearRecords() { }

		protected virtual void OnSelectionChanged() { }

		protected virtual void DrawSettingsBody() { }

		protected virtual void DrawRecord(T record, int recordIndex) { }

		protected virtual void DrawRightPanelTop() { }

		/* abstract methods */

		protected abstract T[] LoadLastRecords();
		protected abstract RecordsTabState GetState();

		protected abstract void SaveSearchResults();
		protected abstract string GetModuleName();

		/* protected methods */

		protected void DrawShowButtonIfPossible(T record)
		{
			var showableIssueRecord = record as IShowableRecord;
			if (showableIssueRecord == null) return;

			string hintText;
			switch (record.location)
			{
				case RecordLocation.Unknown:
					hintText = "Oh, sorry, but looks like I have no clue about this record.";
					break;
				case RecordLocation.Scene:
					hintText = "Selects item in the scene. Opens scene with target item if necessary and highlights this scene in the Project Browser.";
					break;
				case RecordLocation.Asset:
					hintText = "Selects asset file in the Project Browser.";
					break;
				case RecordLocation.Prefab:
					hintText = "Selects Prefab file with item in the Project Browser.";
					break;
				case RecordLocation.BuildSettings:
					hintText = "Opens BuildSettings window.";
					break;
				case RecordLocation.TagsAndLayers:
					hintText = "Opens Tags and Layers in inspector.";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (UIHelpers.RecordButton(record, "Show", hintText, CSIcons.Show))
			{
				gotoRecord = showableIssueRecord;
			}
		}

		protected void DrawCopyButton(T record)
		{
			if (UIHelpers.RecordButton(record, "Copy", "Copies record text to the clipboard.", CSIcons.Copy))
			{
				EditorGUIUtility.systemCopyBuffer = record.ToString(true);
				MaintainerWindow.ShowNotification("Record copied to clipboard!");
			}
		}

		protected void DrawExpandCollapseButton(RecordBase record)
		{
			var r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(12));
			EditorGUI.BeginChangeCheck();
			record.compactMode = !EditorGUI.Foldout(r, !record.compactMode, GUIContent.none, UIHelpers.richFoldout);
			if (EditorGUI.EndChangeCheck())
			{
				var index = Array.IndexOf(filteredRecords, record);
				GetState().compaction[index] = record.compactMode;
			}
		}

		protected void DrawSelectAllButton()
		{
			if (UIHelpers.ImageButton("Select all", CSIcons.SelectAll))
			{
				foreach (var record in filteredRecords)
				{
					record.selected = true;
				}

				OnSelectionChanged();
			}
		}

		protected void DrawSelectNoneButton()
		{
			if (UIHelpers.ImageButton("Select none", CSIcons.SelectNone))
			{
				foreach (var record in filteredRecords)
				{
					record.selected = false;
				}

				OnSelectionChanged();
			}
		}

		protected void DrawExpandAllButton()
		{
			if (UIHelpers.ImageButton("Expand all", CSIcons.Expand))
			{
				for (var i = 0; i < filteredRecords.Length; i++)
				{
					var record = filteredRecords[i];
					record.compactMode = false;
					GetState().compaction[i] = false;
				}
			}
		}

		protected void DrawCollapseAllButton()
		{
			if (UIHelpers.ImageButton("Collapse all", CSIcons.Collapse))
			{
				for (var i = 0; i < filteredRecords.Length; i++)
				{
					var record = filteredRecords[i];
					record.compactMode = true;
					GetState().compaction[i] = true;
				}
			}
		}

		protected void DrawCopyReportButton()
		{
			if (UIHelpers.ImageButton("Copy report to clipboard", CSIcons.Copy))
			{
				EditorGUIUtility.systemCopyBuffer = ReportsBuilder.GenerateReport(GetModuleName(), filteredRecords, GetReportHeader(), GetReportFooter());
				MaintainerWindow.ShowNotification("Report copied to clipboard!");
			}
		}

		protected void DrawExportReportButton()
		{
			if (UIHelpers.ImageButton("Export report...", CSIcons.Export))
			{
				var filePath = EditorUtility.SaveFilePanel("Save " + GetModuleName() + " report", "", "Maintainer " + GetReportFileNamePart() + "Report.txt", "txt");
				if (!string.IsNullOrEmpty(filePath))
				{
					var sr = File.CreateText(filePath);
					sr.Write(ReportsBuilder.GenerateReport(GetModuleName(), filteredRecords, GetReportHeader(), GetReportFooter()));
					sr.Close();
					MaintainerWindow.ShowNotification("Report saved!");
				}
			}
		}

		protected void DrawClearResultsButton()
		{
			if (UIHelpers.ImageButton("Clear results", CSIcons.Clear))
			{
				ClearRecords();
				AfterClearRecords();
			}
		}

		private void ApplyState()
		{
			if (GetState().selection.Count != filteredRecords.Length)
			{
				GetState().selection = new List<bool>(filteredRecords.Length);
				GetState().compaction = new List<bool>(filteredRecords.Length);

				for (var i = 0; i < filteredRecords.Length; i++)
				{
					GetState().selection.Add(true);
					GetState().compaction.Add(true);
				}
			}

			for (var i = 0; i < filteredRecords.Length; i++)
			{
				var record = filteredRecords[i];
				record.selected = GetState().selection[i];
				record.compactMode = GetState().compaction[i];
			}
		}
	}
}