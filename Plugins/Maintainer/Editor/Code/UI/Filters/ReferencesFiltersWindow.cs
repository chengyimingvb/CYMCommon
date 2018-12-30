#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.References;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class ReferencesFiltersWindow : FiltersWindow
	{
		internal static ReferencesFiltersWindow instance;

		internal static ReferencesFiltersWindow Create()
		{
			var window = GetWindow<ReferencesFiltersWindow>(true);
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
				new PathFiltersTab(
					FilterType.Ignores,
					"Ignored items will not be searched for references both as source and a target of the reference.", 
					MaintainerSettings.References.pathIgnoresFilters, 
					false, 
					OnPathIgnoresChange),
			};

			Init(ReferencesFinder.ModuleName, tabs, 0, null);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnPathIgnoresChange(FilterItem[] collection)
		{
			MaintainerSettings.References.pathIgnoresFilters = collection;
		}
	}
}