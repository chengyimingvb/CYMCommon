#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using UnityEditor;

namespace CodeStage.Maintainer.Core
{
	[Serializable]
	public class ReferencedAtInfo : ReferencingInfo
	{
		public ReferencingEntryData[] entries;

		public void AddNewEntry(ReferencingEntryData newEntry)
		{
			if (entries == null)
			{
				entries = new[] {newEntry};
			}
			else
			{
				ArrayUtility.Add(ref entries, newEntry);
			}
		}
	}
}