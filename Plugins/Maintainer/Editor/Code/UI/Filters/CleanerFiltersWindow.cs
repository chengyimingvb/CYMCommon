#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class CleanerFiltersWindow : FiltersWindow
	{
		internal static CleanerFiltersWindow instance;

		internal static CleanerFiltersWindow Create()
		{
			var window = GetWindow<CleanerFiltersWindow>(true);
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
				new SceneFiltersTab(
					FilterType.Ignores, 
					"Ignored scenes will be considered as needed and everything referenced in them will be excluded from the garbage search.", 
					MaintainerSettings.Cleaner.sceneIgnoresFilters, 
					MaintainerSettings.Cleaner.ignoreScenesInBuild, 
					MaintainerSettings.Cleaner.ignoreOnlyEnabledScenesInBuild, 
					OnSceneIgnoresSettingsChange, OnSceneIgnoresChange),

				new PathFiltersTab(
					FilterType.Ignores, 
					"Ignored items will be considered as needed and will be excluded from the garbage search.", 
					MaintainerSettings.Cleaner.pathIgnoresFilters, 
					false, 
					OnPathIgnoresChange, OnGetDefaults),
			};

			Init(ProjectCleaner.ModuleName, tabs, MaintainerSettings.Cleaner.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnPathIgnoresChange(FilterItem[] collection)
		{
			MaintainerSettings.Cleaner.pathIgnoresFilters = collection;
		}

		private static void OnSceneIgnoresChange(FilterItem[] collection)
		{
			MaintainerSettings.Cleaner.sceneIgnoresFilters = collection;
		}

		private void OnSceneIgnoresSettingsChange(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild)
		{
			MaintainerSettings.Cleaner.ignoreScenesInBuild = ignoreScenesInBuild;
			MaintainerSettings.Cleaner.ignoreOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
		}

		private void OnTabChange(int newTab)
		{
			MaintainerSettings.Cleaner.filtersTabIndex = newTab;
		}

		private FilterItem[] OnGetDefaults()
		{
			return MaintainerSettings.Cleaner.GetDefaultFilters();
		}
	}
}