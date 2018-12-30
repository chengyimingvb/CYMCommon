#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CodeStage.Maintainer.Tools
{
	public class CSArrayTools
	{
		public static bool AddIfNotExists<T>(ref T[] items, T newItem)
		{
			if (Array.IndexOf(items, newItem) != -1) return false;
			ArrayUtility.Add(ref items, newItem);
			return true;
		}

		public static bool IsItemContainsAnyStringFromArray(string item, ICollection<string> items)
		{
			if (items == null) return false;

			var result = false;

			foreach (var str in items)
			{
				if (item.Contains(str))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public static T[] FindDuplicatesInArray<T>(T[] array)
		{
			return array.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
		}

		public static List<T> FindDuplicatesInArray<T>(List<T> array)
		{
			return array.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
		}

		public static T[] RemoveAt<T>(T[] source, int index)
		{
			var newArray = new T[source.Length - 1];
			if (index > 0)
				Array.Copy(source, 0, newArray, 0, index);

			if (index < source.Length - 1)
				Array.Copy(source, index + 1, newArray, index, source.Length - index - 1);

			return newArray;
		}
	}
}