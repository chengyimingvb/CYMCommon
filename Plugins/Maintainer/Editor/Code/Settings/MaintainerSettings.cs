#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class MaintainerSettings: ScriptableObject
	{
		internal const int UpdateProgressStep = 10;

		private const string Directory = "ProjectSettings";
		private const string Path = Directory + "/MaintainerSettings.asset";
		private static MaintainerSettings instance;

		public IssuesFinderSettings issuesFinderSettings;
		public ProjectCleanerSettings projectCleanerSettings;
		public ReferencesFinderSettings referencesFinderSettings;
		public MaintainerWindow.MaintainerTab selectedTab = MaintainerWindow.MaintainerTab.Issues;

		public string version = Maintainer.Version;

		public static MaintainerSettings Instance
		{
			get
			{
				if (instance != null) return instance;
				instance = LoadOrCreate();
				return instance;
			}
		}

		public static IssuesFinderSettings Issues
		{
			get
			{
				if (Instance.issuesFinderSettings == null)
				{
					Instance.issuesFinderSettings = new IssuesFinderSettings();
				}
				return Instance.issuesFinderSettings;
			}
		}

		public static ProjectCleanerSettings Cleaner
		{
			get
			{
				if (Instance.projectCleanerSettings == null)
				{
					Instance.projectCleanerSettings = new ProjectCleanerSettings();
				}
				return Instance.projectCleanerSettings;
			}
		}

		public static ReferencesFinderSettings References
		{
			get
			{
				if (Instance.referencesFinderSettings == null)
				{
					Instance.referencesFinderSettings = new ReferencesFinderSettings();
				}
				return Instance.referencesFinderSettings;
			}
		}

		public static void Delete()
		{
			instance = null;
			if (File.Exists(Path))
			{
				CSEditorTools.RemoveReadOnlyAttribute(Path);
				File.Delete(Path);
			}
		}

		public static void Save()
		{
			SaveInstance(Instance);
		}

		private static MaintainerSettings LoadOrCreate()
		{
			MaintainerSettings settings;

			if (!File.Exists(Path))
			{
				settings = CreateNewSettingsFile();
			}
			else
			{
				settings = LoadInstance();

				if (settings == null)
				{
					CSEditorTools.RemoveReadOnlyAttribute(Path);
					File.Delete(Path);
					settings = CreateNewSettingsFile();
				}

				if (settings.version != Maintainer.Version)
				{
					if (string.IsNullOrEmpty(settings.version))
					{
						MigrateAllIgnores(settings.issuesFinderSettings.pathIgnores, ref settings.issuesFinderSettings.pathIgnoresFilters, FilterKind.Path);
						settings.issuesFinderSettings.pathIgnores = null;

						MigrateAllIgnores(settings.issuesFinderSettings.componentIgnores, ref settings.issuesFinderSettings.componentIgnoresFilters, FilterKind.Type);
						settings.issuesFinderSettings.componentIgnores = null;

						MigrateAllIgnores(settings.issuesFinderSettings.pathIncludes, ref settings.issuesFinderSettings.pathIncludesFilters, FilterKind.Path);
						settings.issuesFinderSettings.pathIncludes = null;

						MigrateAllIgnores(settings.issuesFinderSettings.sceneIncludes, ref settings.issuesFinderSettings.sceneIncludesFilters, FilterKind.Path);
						settings.issuesFinderSettings.sceneIncludes = null;

						MigrateAllIgnores(settings.projectCleanerSettings.pathIgnores, ref settings.projectCleanerSettings.pathIgnoresFilters, FilterKind.Path);
						settings.projectCleanerSettings.pathIgnores = null;

						MigrateAllIgnores(settings.projectCleanerSettings.sceneIgnores, ref settings.projectCleanerSettings.sceneIgnoresFilters, FilterKind.Path);
						settings.projectCleanerSettings.sceneIgnores = null;

						settings.projectCleanerSettings.AddDefaultFilters();
					}
				}
			}

			settings.version = Maintainer.Version;

			return settings;
		}

		private static bool MigrateAllIgnores(string[] oldFilters, ref FilterItem[] newFilters, FilterKind filterKind)
		{
			if (oldFilters == null || oldFilters.Length == 0) return false;

			var newFiltersList = new List<FilterItem>(oldFilters.Length);
			foreach (var oldFilter in oldFilters)
			{
				if (CSEditorTools.IsValueMatchesAnyFilter(oldFilter, newFilters)) continue;
				newFiltersList.Add(FilterItem.Create(oldFilter, filterKind));
			}

			ArrayUtility.AddRange(ref newFilters, newFiltersList.ToArray());

			return true;
		}

		private static MaintainerSettings CreateNewSettingsFile()
		{
			var settingsInstance = CreateInstance();

			SaveInstance(settingsInstance);
			settingsInstance.projectCleanerSettings.SetDefaultFilters();

			return settingsInstance;
		}

		private static void SaveInstance(MaintainerSettings settingsInstance)
		{
			if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);

			try
			{
				 UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new []{settingsInstance}, Path, true);
			}
			catch (Exception ex)
			{
				Debug.LogError(Maintainer.ConstructError("Can't save settings!\n" + ex));
			}
		}

		private static MaintainerSettings LoadInstance()
		{
			MaintainerSettings settingsInstance;

			try
			{
				settingsInstance = (MaintainerSettings)UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(Path)[0];
			}
			catch (Exception ex)
			{
				Debug.Log(Maintainer.LogPrefix + "Can't read settings, resetting them to defaults.\nThis is a harmless message in most cases and can be ignored.\n" + ex);
				settingsInstance = null;
			}

			return settingsInstance;
		}

		private static MaintainerSettings CreateInstance()
		{
			var newInstance = CreateInstance<MaintainerSettings>();
			//var newInstance = new MaintainerSettings();
			newInstance.issuesFinderSettings = new IssuesFinderSettings();
			newInstance.projectCleanerSettings = new ProjectCleanerSettings();
			newInstance.referencesFinderSettings = new ReferencesFinderSettings();
			return newInstance;
		}
	}
}