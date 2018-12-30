#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using CodeStage.Maintainer.Core;
#if UNITY_5_6_OR_NEWER
using UnityEditor.IMGUI.Controls;
#endif

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class ReferencesFinderSettings
	{
		public bool showAssetsWithoutReferences;
		public bool selectedFindClearsResults;
		public string searchString;

		public FilterItem[] pathIgnoresFilters = new FilterItem[0];

		public bool fullProjectScanWarningShown;

#if UNITY_5_6_OR_NEWER
		public TreeViewState treeViewState;
		public MultiColumnHeaderState multiColumnHeaderState;
#endif
	}
}