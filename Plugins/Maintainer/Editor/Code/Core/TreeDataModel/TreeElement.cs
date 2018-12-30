#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;

namespace CodeStage.Maintainer.Core
{
	[Serializable]
	public class TreeElement
	{
		public string name;
		public int depth;
		public int id;
		public int recursionId = -1;

		[NonSerialized]
		public TreeElement parent;

		[NonSerialized]
		public List<TreeElement> children;

		public int ChildrenCount
		{
			get
			{
				return children == null ? 0 : children.Count;
			}
		}

		public bool HasChildren
		{
			get { return children != null && children.Count > 0; }
		}

		public TreeElement()
		{
		}

		public TreeElement(string name, int depth, int id)
		{
			this.name = name;
			this.depth = depth;
			this.id = id;
		}

		internal bool CanBeFoundWith(string searchString)
		{
			return Search(searchString);
		}

		protected virtual bool Search(string searchString)
		{
			return name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}
}