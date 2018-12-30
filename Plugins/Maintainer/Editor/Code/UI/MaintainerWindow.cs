#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Settings;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	public class MaintainerWindow : EditorWindow
	{
		public enum MaintainerTab
		{
			Issues = 0,
			Cleaner = 1,
			References = 2,
			About = 3
		}

		private static MaintainerWindow windowInstance;

		[NonSerialized]
		private MaintainerTab currentTab;

		[NonSerialized]
		private GUIContent[] tabsCaptions;

		[NonSerialized]
		private IssuesTab issuesTab;

		[NonSerialized]
		private CleanerTab cleanerTab;

		[NonSerialized]
		private ReferencesTab referencesTab;

		[NonSerialized]
		private AboutTab aboutTab;

		[NonSerialized]
		private bool inited;

		public static MaintainerWindow Create()
		{
			if (windowInstance == null)
			{
				windowInstance = GetWindow<MaintainerWindow>();
			}

			return windowInstance;
		}

		public static MaintainerWindow Create(MaintainerTab tab)
		{
			windowInstance = Create();

			if (windowInstance.currentTab != tab)
			{
				windowInstance.currentTab = MaintainerSettings.Instance.selectedTab = tab;
			}
			windowInstance.Refresh(true);

			return windowInstance;
		}

		public static void ShowIssues()
		{
			Create(MaintainerTab.Issues).Repaint();
		}

		public static void ShowCleaner()
		{
			AssetPreview.SetPreviewTextureCacheSize(50);
			ShowProjectCleanerWarning();

			Create(MaintainerTab.Cleaner).Repaint();
		}

		public static void ShowReferences()
		{
			Create(MaintainerTab.References).Repaint();
		}

		public static void ShowAbout()
		{
			Create(MaintainerTab.About).Repaint();
		}

		public static void ShowNotification(string text)
		{
			if (windowInstance)
			{
				windowInstance.ShowNotification(new GUIContent(text));
			}
		}

		public static void RepaintInstance()
		{
			if (windowInstance)
			{
				windowInstance.Repaint();
			}
		}

		private static void ShowProjectCleanerWarning()
		{
			if (MaintainerSettings.Cleaner.firstTime)
			{
				EditorUtility.DisplayDialog(ProjectCleaner.ModuleName, "Please note, this module can remove files and folders physically from your system.\nPlease always make a backup of your project before using Project Cleaner!\nUse it on your own peril, author is not responsible for any damage made due to the module usage!\nThis message shows only once.", "Dismiss");
				MaintainerSettings.Cleaner.firstTime = false;
			}
		}
		 
		private void Init()
		{
			if (inited) return;
			
			CreateTabs();

			titleContent = new GUIContent(" Maintainer", CSIcons.Maintainer);
			minSize = new Vector2(750f, 400f);
			Focus();
			Repaint();
			currentTab = MaintainerSettings.Instance.selectedTab;

			Refresh(false);
			inited = true;
		}

		private void CreateTabs()
		{
			if (issuesTab == null)
				issuesTab = new IssuesTab(this);

			if (cleanerTab == null)
				cleanerTab = new CleanerTab(this);

			if (referencesTab == null)
				referencesTab = new ReferencesTab(this);

			if (aboutTab == null)
				aboutTab = new AboutTab(this);

			if (tabsCaptions == null)
			{
				tabsCaptions = new[] { issuesTab.Caption, cleanerTab.Caption, referencesTab.Caption, aboutTab.Caption };
			}
		}

		public void Refresh(bool newData)
		{
			switch (currentTab)
			{
				case MaintainerTab.Issues:
					issuesTab.Refresh(newData);
					break;
				case MaintainerTab.Cleaner:
					cleanerTab.Refresh(newData);
					break;
				case MaintainerTab.References:
					referencesTab.Refresh(newData);
					break;
				case MaintainerTab.About:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		 
		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;

			windowInstance = this;
			Init();
		}

		private void OnLostFocus()
		{
			MaintainerSettings.Save();
		}

		private void OnGUI()
		{
			UIHelpers.SetupStyles();

			EditorGUI.BeginChangeCheck();
			currentTab = (MaintainerTab)GUILayout.Toolbar((int)currentTab, tabsCaptions, GUILayout.ExpandWidth(false), GUILayout.Height(21));
			if (EditorGUI.EndChangeCheck())
			{
				if (currentTab == MaintainerTab.Cleaner) ShowProjectCleanerWarning();
				MaintainerSettings.Instance.selectedTab = currentTab;

				Refresh(false);
			}

			switch (currentTab)
			{
				case MaintainerTab.Issues:
					issuesTab.Draw();
					break;
				case MaintainerTab.Cleaner:
					cleanerTab.Draw();
					break;
				case MaintainerTab.References:
					referencesTab.Draw();
					break;
				case MaintainerTab.About:
					aboutTab.Draw();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}