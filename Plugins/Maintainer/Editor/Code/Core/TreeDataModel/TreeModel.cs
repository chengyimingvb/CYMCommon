#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CodeStage.Maintainer.Core
{
	internal class TreeModel<T> where T : TreeElement
	{
		public event Action ModelChanged;

		private T[] data;
		public T root;
		private int maxID;
	
		public int NumberOfDataElements
		{
			get { return data.Length; }
		}

		public TreeModel(T[] data)
		{
			SetData(data);
		}

		public T Find(int id)
		{
			return data.FirstOrDefault (element => element.id == id);
		}
	
		public void SetData(T[] newData)
		{
			Init(newData);
		}

		private void Init(T[] newData)
		{
			if (newData == null)
				throw new ArgumentNullException("newData", Maintainer.ConstructError("Input data is null. Ensure input is a non-null list!"));

			data = newData;
			if (data.Length > 0)
				root = TreeElementUtility.ArrayToTree(data);

			maxID = data.Max(e => e.id);
		}

		public int GenerateUniqueID()
		{
			return ++maxID;
		}

		public IList<int> GetAncestors(int id)
		{
			var parents = new List<int>();
			TreeElement T = Find(id);
			if (T != null)
			{
				while (T.parent != null)
				{
					parents.Add(T.parent.id);
					T = T.parent;
				}
			}
			return parents;
		}

		public IList<int> GetDescendantsThatHaveChildren(int id)
		{
			var searchFromThis = Find(id);
			if (searchFromThis != null)
			{
				return GetParentsBelowStackBased(searchFromThis);
			}
			return new List<int>();
		}

		private IList<int> GetParentsBelowStackBased(TreeElement searchFromThis)
		{
			var stack = new Stack<TreeElement>();
			stack.Push(searchFromThis);

			var parentsBelow = new List<int>();
			while (stack.Count > 0)
			{
				var current = stack.Pop();
				if (current.HasChildren)
				{
					parentsBelow.Add(current.id);
					foreach (var T in current.children)
					{
						stack.Push(T);
					}
				}
			}

			return parentsBelow;
		}

		public void RemoveElements(IList<int> elementIDs)
		{
			IList<T> elements = data.Where (element => elementIDs.Contains (element.id)).ToArray ();
			RemoveElements(elements);
		}

		public void RemoveElements(IList<T> elements)
		{
			foreach (var element in elements)
				if (element == root)
					throw new ArgumentException(Maintainer.ConstructError("It is not allowed to remove the root element!"));


			var commonAncestors = TreeElementUtility.FindCommonAncestorsWithinList (elements);

			foreach (var element in commonAncestors)
			{
				element.parent.children.Remove (element);
				element.parent = null;
			}

			TreeElementUtility.TreeToList(root, data);

			Changed();
		}

		public void AddElements (IList<T> elements, TreeElement parent, int insertPosition)
		{
			if (elements == null)
				throw new ArgumentNullException("elements", Maintainer.ConstructError("elements is null!"));
			if (elements.Count == 0)
				throw new ArgumentNullException("elements", Maintainer.ConstructError("elements Count is 0: nothing to add!"));
			if (parent == null)
				throw new ArgumentNullException("parent", Maintainer.ConstructError("parent is null!"));

			if (parent.children == null)
				parent.children = new List<TreeElement>();

			parent.children.InsertRange(insertPosition, elements.Cast<TreeElement> ());
			foreach (var element in elements)
			{
				element.parent = parent;
				element.depth = parent.depth + 1;
				TreeElementUtility.UpdateDepthValues(element);
			}

			TreeElementUtility.TreeToList(root, data);

			Changed();
		}

		public void AddRoot (T newRoot)
		{
			if (newRoot == null)
				throw new ArgumentNullException("newRoot", Maintainer.ConstructError("newRoot is null!"));

			if (data == null)
				throw new InvalidOperationException(Maintainer.ConstructError("Internal Error: data list is null!"));

			if (data.Length != 0)
				throw new InvalidOperationException(Maintainer.ConstructError("AddRoot is only allowed on empty data list!"));

			newRoot.id = GenerateUniqueID();
			newRoot.depth = -1;
			ArrayUtility.Add(ref data, newRoot);
		}

		public void AddElement(T element, TreeElement parent, int insertPosition)
		{
			if (element == null)
				throw new ArgumentNullException("element", Maintainer.ConstructError("element is null!"));
			if (parent == null)
				throw new ArgumentNullException("parent", Maintainer.ConstructError("parent is null!"));
		
			if (parent.children == null)
				parent.children = new List<TreeElement> ();

			parent.children.Insert (insertPosition, element);
			element.parent = parent;

			TreeElementUtility.UpdateDepthValues(parent);
			TreeElementUtility.TreeToList(root, data);

			Changed();
		}

		public void MoveElements(TreeElement parentElement, int insertionIndex, List<TreeElement> elements)
		{
			if (insertionIndex < 0)
				throw new ArgumentException(Maintainer.ConstructError("Invalid input: insertionIndex is -1, client needs to decide what index elements should be reparented at!"));

			if (parentElement == null)
				return;

			if (insertionIndex > 0)
				insertionIndex -= parentElement.children.GetRange(0, insertionIndex).Count(elements.Contains);

			foreach (var draggedItem in elements)
			{
				draggedItem.parent.children.Remove(draggedItem);	
				draggedItem.parent = parentElement;					
			} 

			if (parentElement.children == null)
				parentElement.children = new List<TreeElement>();

			parentElement.children.InsertRange(insertionIndex, elements);

			TreeElementUtility.UpdateDepthValues (root);
			TreeElementUtility.TreeToList(root, data);

			Changed();
		}

		private void Changed()
		{
			if (ModelChanged != null)
				ModelChanged();
		}
	}
}
#endif