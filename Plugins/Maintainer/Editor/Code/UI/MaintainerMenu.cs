#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.References;
using UnityEditor;

namespace CodeStage.Maintainer.UI
{
	public class MaintainerMenu
	{
		public const string ReferencesFinderMenuItemName = "Find References in Project";
		public const string ProjectBrowserContext = "Maintainer/" + ReferencesFinderMenuItemName;

		[MenuItem("Tools/Code Stage/Maintainer/Show %#&`", false, 900)]
		private static void ShowWindow()
		{
			MaintainerWindow.Create();
		}

		[MenuItem("Tools/Code Stage/Maintainer/About", false, 901)]
		private static void ShowAbout()
		{
			MaintainerWindow.ShowAbout();
		}

		[MenuItem("Tools/Code Stage/Maintainer/Find Issues %#&f", false, 1000)]
		private static void FindAllIssues()
		{
			IssuesFinder.StartSearch(true);
		}

		[MenuItem("Tools/Code Stage/Maintainer/Find Garbage %#&g", false, 1001)]
		private static void FindAllGarbage()
		{
			ProjectCleaner.StartSearch(true);
		}

#if UNITY_5_6_OR_NEWER
		[MenuItem("Tools/Code Stage/Maintainer/Find All References %#&r", false, 1002)]
		private static void FindAllReferences()
		{
			ReferencesFinder.GetReferences();
		}

		[MenuItem("Assets/" + ProjectBrowserContext + " %#&s", false, 39)]
		public static void FindReferences()
		{
			ReferencesFinder.AddSelectedToSelectionAndRun();
		}

		[MenuItem("Assets/" + ProjectBrowserContext + " %#&s", true, 39)]
		public static bool ValidateFindReferences()
		{
			return CSEditorTools.GetProjectSelections(true).Length > 0;
		}
#endif
	}
}