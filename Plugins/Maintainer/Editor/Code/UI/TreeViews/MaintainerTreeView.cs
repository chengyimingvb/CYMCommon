#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using CodeStage.Maintainer.Core;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal abstract class MaintainerTreeViewItem<T> : TreeViewItem where T : TreeElement
	{
		public T data;

		protected MaintainerTreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
		{
			this.data = data;
			Init();
		}

		private void Init()
		{
			Initialize();
		}

		protected abstract void Initialize(); 
	}

	internal abstract class MaintainerTreeView<T> : TreeView where T : TreeElement
	{
		protected const float ROW_HEIGHT = 25f;

		public event Action TreeChanged;
		public TreeModel<T> TreeModel { get; private set; }

		protected readonly List<TreeViewItem> rows = new List<TreeViewItem>(100);

		private string newSearchString;
		

		protected MaintainerTreeView(TreeViewState state, TreeModel<T> model) : base(state)
		{
			Init(model);
		}

		protected MaintainerTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model) : base(state, multiColumnHeader)
		{
			multiColumnHeader.sortingChanged += OnSortingChanged;
			Init(model);
		}

		public void SetSearchString(string newSearch)
		{
			newSearchString = newSearch;
		}

		private void Init(TreeModel<T> model)
		{
			rowHeight = ROW_HEIGHT;
			customFoldoutYOffset = (ROW_HEIGHT - EditorGUIUtility.singleLineHeight) * 0.5f;

			showBorder = true;
			showAlternatingRowBackgrounds = true;

			TreeModel = model;
			TreeModel.ModelChanged += ModelChanged;

			PostInit();
		}

		protected override TreeViewItem BuildRoot()
		{
			return GetNewTreeViewItemInstance(TreeModel.root.id, -1, TreeModel.root.name, TreeModel.root);
		}

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			if (TreeModel.root == null)
			{
				Debug.LogError(Maintainer.ConstructError("tree model root is null. did you call SetData()?"));
				return rows;
			}

			rows.Clear();

			if (TreeModel.root.HasChildren)
			{
				AddChildrenRecursive(TreeModel.root, 0, rows);
			}
			SetupParentsAndChildrenFromDepths(root, rows);

			SortIfNeeded(root, rows);

			return rows;
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return true;
		}

		private void AddChildrenRecursive(T parent, int depth, IList<TreeViewItem> result)
		{
			foreach (var treeElement in parent.children)
			{
				var child = (T)treeElement;

				if (!string.IsNullOrEmpty(newSearchString) && !child.CanBeFoundWith(newSearchString) && depth == 0) continue;

				var item = GetNewTreeViewItemInstance(child.id, depth, child.name, child);
				result.Add(item);

				if (child.HasChildren)
				{
					if (IsExpanded(child.id))
					{
						AddChildrenRecursive(child, depth + 1, result);
					}
					else
					{
						item.children = CreateChildListForCollapsedParent();
					}
				}
			}
		}

		private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rowsToSort)
		{
			if (rowsToSort.Count <= 1)
				return;

			if (multiColumnHeader.sortedColumnIndex == -1)
				return;

			SortByMultipleColumns();
			TreeToList(root, rowsToSort);
			Repaint();
		}

		private void ModelChanged()
		{
			if (TreeChanged != null) TreeChanged();
			Reload();
		}

		protected virtual void OnSortingChanged(MultiColumnHeader header)
		{
			SortIfNeeded(rootItem, GetRows());
		}

		protected abstract TreeViewItem GetNewTreeViewItemInstance(int id, int depth, string name, T data);
		protected abstract void SortByMultipleColumns();
		protected virtual void PostInit() {}

		public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();

			if (root.children == null)
				return;

			var stack = new Stack<TreeViewItem>();
			for (var i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (var i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}
	}

	internal static class EnumerableExtensionMethods
	{
		public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.OrderBy(selector);
			}

			return source.OrderByDescending(selector);
		}

		public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
		{
			if (ascending)
			{
				return source.ThenBy(selector);
			}

			return source.ThenByDescending(selector);
		}
	}
}
#endif