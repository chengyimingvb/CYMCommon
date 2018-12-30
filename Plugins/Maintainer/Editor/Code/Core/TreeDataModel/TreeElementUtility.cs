#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using CodeStage.Maintainer.Settings;
using UnityEditor;

namespace CodeStage.Maintainer.Core
{
	internal static class TreeElementUtility
	{
		public static void TreeToList<T>(T root, IList<T> result) where T : TreeElement
		{
			if (result == null)
				throw new NullReferenceException(Maintainer.ConstructError("The input 'IList<T> result' list is null!"));

			result.Clear();

			var stack = new Stack<T>();
			stack.Push(root);

			while (stack.Count > 0)
			{
				var current = stack.Pop();
				result.Add(current);

				if (current.children == null || current.children.Count <= 0) continue;

				for (var i = current.children.Count - 1; i >= 0; i--)
				{
					stack.Push((T)current.children[i]);
				}
			}
		}

		public static T ArrayToTree<T>(T[] array) where T : TreeElement
		{
			var count = array.Length;
			var updateStep = Math.Max(count / MaintainerSettings.UpdateProgressStep, 1);
			var showProgress = count > 500000;

			if (showProgress)
			{
				EditorUtility.DisplayProgressBar("Building tree model...", string.Format("Item {0} of {1}", 1, count), 0);
			}

			ValidateDepthValues(array);

			foreach (var element in array)
			{
				element.parent = null;
				element.children = null;
			}

			for (var parentIndex = 0; parentIndex < count; parentIndex++)
			{
				if (showProgress && parentIndex % updateStep == 0)
				{
					EditorUtility.DisplayProgressBar("Building tree model...", string.Format("Item {0} of {1}", parentIndex + 1, count), (float)parentIndex / count);
				}

				var parent = array[parentIndex];
				var alreadyHasValidChildren = parent.children != null;
				if (alreadyHasValidChildren) continue;

				var parentDepth = parent.depth;
				var childCount = 0;

				for (var i = parentIndex + 1; i < count; i++)
				{
					var item = array[i];
					if (item.depth == parentDepth + 1)
						childCount++;
					if (item.depth <= parentDepth)
						break;
				}

				List<TreeElement> childList = null;
				if (childCount != 0)
				{
					childList = new List<TreeElement>(childCount);
					childCount = 0;
					for (var i = parentIndex + 1; i < count; i++)
					{
						var item = array[i];
						if (item.depth == parentDepth + 1)
						{
							item.parent = parent;
							childList.Add(item);
							childCount++;
						}

						if (item.depth <= parentDepth)
							break;
					}
				}

				parent.children = childList;
			}
			 
			EditorUtility.ClearProgressBar();

			return array[0];
		}

		public static void ValidateDepthValues<T>(T[] array) where T : TreeElement
		{
			if (array.Length == 0)
				throw new ArgumentException(Maintainer.ConstructError("list should have items, count is 0, check before calling ValidateDepthValues!"), "array");

			if (array[0].depth != -1)
				throw new ArgumentException(Maintainer.ConstructError("list item at index 0 should have a depth of -1 (since this should be the hidden root of the tree). Depth is: " + array[0].depth + "!"), "array");

			for (var i = 0; i < array.Length - 1; i++)
			{
				var depth = array[i].depth;
				var nextDepth = array[i + 1].depth;
				if (nextDepth > depth && nextDepth - depth > 1)
					throw new ArgumentException(Maintainer.ConstructError(string.Format("Invalid depth info in input list. Depth cannot increase more than 1 per row. Index {0} has depth {1} while index {2} has depth {3}!", i, depth, i + 1, nextDepth)));
			}

			for (var i = 1; i < array.Length; ++i)
				if (array[i].depth < 0)
					throw new ArgumentException(Maintainer.ConstructError("Invalid depth value for item at index " + i + ". Only the first item (the root) should have depth below 0!"));

			if (array.Length > 1 && array[1].depth != 0)
				throw new ArgumentException(Maintainer.ConstructError("Input list item at index 1 is assumed to have a depth of 0!"), "array");
		}


		public static void UpdateDepthValues<T>(T root) where T : TreeElement
		{
			if (root == null)
				throw new ArgumentNullException("root", Maintainer.ConstructError("The root is null!"));

			if (!root.HasChildren)
				return;

			var stack = new Stack<TreeElement>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				var current = stack.Pop();
				if (current.children != null)
				{
					foreach (var child in current.children)
					{
						child.depth = current.depth + 1;
						stack.Push(child);
					}
				}
			}
		}

		public static IList<T> FindCommonAncestorsWithinList<T>(IList<T> elements) where T : TreeElement
		{
			if (elements.Count == 1)
				return new List<T>(elements);

			var result = new List<T>(elements);
			result.RemoveAll(g => IsChildOf(g, elements));
			return result;
		}

		private static bool IsChildOf<T>(T child, IList<T> elements) where T : TreeElement
		{
			while (child != null)
			{
				child = (T)child.parent;
				if (elements.Contains(child))
					return true;
			}
			return false;
		}
	}
}
