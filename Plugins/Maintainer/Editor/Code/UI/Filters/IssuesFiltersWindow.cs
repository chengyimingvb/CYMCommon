#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class IssuesFiltersWindow : FiltersWindow
	{
		internal static IssuesFiltersWindow instance;

		internal static IssuesFiltersWindow Create()
		{
			var window = GetWindow<IssuesFiltersWindow>(true);
			window.Focus();

			return window;
		}

		internal static void Refresh()
		{
			if (instance == null) return;

			instance.InitOnEnable();
			instance.Focus();
		}

		protected override void InitOnEnable()
		{
			TabBase[] tabs =
			{
				new SceneFiltersTab(FilterType.Includes, 
									"Only these included scenes will be checked for issues of you'll choose 'Included Scenes' dropdown option at the Scene filtering options.",
									MaintainerSettings.Issues.sceneIncludesFilters,
									MaintainerSettings.Issues.includeScenesInBuild,
									MaintainerSettings.Issues.includeOnlyEnabledScenesInBuild, 
									OnSceneIgnoresSettingsChange, OnSceneIncludesChange),

				new PathFiltersTab(FilterType.Includes, null, MaintainerSettings.Issues.pathIncludesFilters, true, OnPathIncludesChange),
				new PathFiltersTab(FilterType.Ignores, null, MaintainerSettings.Issues.pathIgnoresFilters, true, OnPathIgnoresChange),
				new ComponentFiltersTab(FilterType.Ignores, MaintainerSettings.Issues.componentIgnoresFilters, OnComponentIgnoresChange)
			};

			Init(IssuesFinder.ModuleName, tabs, MaintainerSettings.Issues.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnSceneIncludesChange(FilterItem[] collection)
		{
			MaintainerSettings.Issues.sceneIncludesFilters = collection;
		}

		private void OnSceneIgnoresSettingsChange(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild)
		{
			MaintainerSettings.Issues.includeScenesInBuild = ignoreScenesInBuild;
			MaintainerSettings.Issues.includeOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
		}

		private void OnPathIgnoresChange(FilterItem[] collection)
		{
			MaintainerSettings.Issues.pathIgnoresFilters = collection;
		}

		private void OnPathIncludesChange(FilterItem[] collection)
		{
			MaintainerSettings.Issues.pathIncludesFilters = collection;
		}

		private void OnComponentIgnoresChange(FilterItem[] collection)
		{
			MaintainerSettings.Issues.componentIgnoresFilters = collection;
		}

		private void OnTabChange(int newTab)
		{
			MaintainerSettings.Issues.filtersTabIndex = newTab;
		}
	}
}