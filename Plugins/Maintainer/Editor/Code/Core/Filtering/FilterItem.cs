#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;

namespace CodeStage.Maintainer.Core
{
	[Serializable]
	public enum FilterKind
	{
		Path,
		Directory,
		FileName,
		Extension,
		Type
	}

	[Serializable]
	public class FilterItem
	{
		public bool ignoreCase;
		public FilterKind kind;
		public string value;

		public static FilterItem Create(string value, FilterKind kind, bool ignoreCase = false)
		{
			var newFilter = new FilterItem
			{
				ignoreCase = ignoreCase,
				kind = kind,
				value = value
			};

			return newFilter;
		}
	}
}