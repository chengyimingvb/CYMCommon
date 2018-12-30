#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class ProjectCleanerSettings
	{
		public RecordsTabState tabState;

		/* filtering */

		public string[] pathIgnores = new string[0];
		public string[] sceneIgnores = new string[0];

		public FilterItem[] pathIgnoresFilters = new FilterItem[0];
		public FilterItem[] sceneIgnoresFilters = new FilterItem[0];

		public bool ignoreScenesInBuild = true;
		public bool ignoreOnlyEnabledScenesInBuild = true;

		public int filtersTabIndex = 0;

		/* what to find */

		public bool findUnreferencedAssets;
		public bool findUnreferencedScripts;
		public bool findEmptyFolders;
		public bool findEmptyFoldersAutomatically;

		/* sorting */

		public CleanerSortingType sortingType = CleanerSortingType.BySize;
		public SortingDirection sortingDirection = SortingDirection.Ascending;

		/* misc */

		public bool useTrashBin = true;

		public bool firstTime = true;
		public bool trashBinWarningShown = false;
		public bool deletionPromptShown = false;


		public ProjectCleanerSettings()
		{
			Reset();
		}

		internal void Reset()
		{
			useTrashBin = true;

			findUnreferencedAssets = true;
			findUnreferencedScripts = false;
			findEmptyFolders = true;
			findEmptyFoldersAutomatically = false;
		}

		internal void SwitchAll(bool enable)
		{
			findEmptyFolders = enable;
		}

		public void AddDefaultFilters()
		{
			Debug.Log(Maintainer.LogPrefix + "Please check your Project Cleaner Path Ignores, new default filters were added.");
			ArrayUtility.AddRange(ref pathIgnoresFilters, GetDefaultFilters());
		}

		public void SetDefaultFilters()
		{
			pathIgnoresFilters = GetDefaultFilters();
		}

		public FilterItem[] GetDefaultFilters()
		{
			return new[]
			{
				FilterItem.Create("/Editor/", FilterKind.Path),
				FilterItem.Create("/Plugins/", FilterKind.Path),
				FilterItem.Create("/StreamingAssets/", FilterKind.Path),
				FilterItem.Create("/Resources/", FilterKind.Path),
				FilterItem.Create("/Gizmos/", FilterKind.Path),
				FilterItem.Create("/Editor Default Resources/", FilterKind.Path),
				FilterItem.Create(".dll", FilterKind.Extension),
				FilterItem.Create(".mdb", FilterKind.Extension),
				FilterItem.Create(".xml", FilterKind.Extension),
				FilterItem.Create(".rsp", FilterKind.Extension),
				FilterItem.Create("readme", FilterKind.FileName, true),
				FilterItem.Create("manual", FilterKind.FileName, true),
			};
		}
	}
}