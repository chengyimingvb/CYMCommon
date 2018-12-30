#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.References;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CodeStage.Maintainer
{
	internal class SearchResultsStorage
	{
		private const string Directory = "Temp";
		private const string IssuesResultsPath = Directory + "/MaintainerIssuesResults.bin";
		private const string CleanerResultsPath = Directory + "/MaintainerCleanerResults.bin";
		private const string ReferencesResultsPath = Directory + "/MaintainerReferencesResults.bin";
		private const string ReferencesSelectionPath = Directory + "/MaintainerReferencesSelection.bin";

		private static IssueRecord[] issuesSearchResults;
		private static CleanerRecord[] cleanerSearchResults;
		private static ReferencesTreeElement[] referencesSearchResults;
		private static FilterItem[] referencesSearchSelection;

		public static IssueRecord[] IssuesSearchResults
		{
			get
			{
				if (issuesSearchResults == null)
				{
					issuesSearchResults = LoadItems<IssueRecord>(IssuesResultsPath);
				}
				return issuesSearchResults;
			}
			set
			{
				issuesSearchResults = value;
				SaveItems(IssuesResultsPath, issuesSearchResults);
			}
		}

		public static CleanerRecord[] CleanerSearchResults
		{
			get
			{
				if (cleanerSearchResults == null)
				{
					cleanerSearchResults = LoadItems<CleanerRecord>(CleanerResultsPath);
				}
				return cleanerSearchResults;
			}
			set
			{
				cleanerSearchResults = value;
				SaveItems(CleanerResultsPath, cleanerSearchResults);
			}
		}

		public static ReferencesTreeElement[] ReferencesSearchResults
		{
			get
			{
				if (referencesSearchResults == null)
				{
					referencesSearchResults = LoadItemsFromJson<ReferencesTreeElement>(ReferencesResultsPath);
				}
				return referencesSearchResults;
			}
			set
			{
				referencesSearchResults = value;
				SaveItemsToJson(ReferencesResultsPath, referencesSearchResults);
			}
		}

		public static FilterItem[] ReferencesSearchSelection
		{
			get
			{
				if (referencesSearchSelection == null)
				{
					referencesSearchSelection = LoadItems<FilterItem>(ReferencesSelectionPath);
				}
				return referencesSearchSelection;
			}
			set
			{
				referencesSearchSelection = value;
				SaveItems(ReferencesSelectionPath, referencesSearchSelection);
			}
		}

		private static void SaveItems<T>(string path, T[] items)
		{
			var sw = Stopwatch.StartNew();

			if (items == null)
			{
				items = new T[0];
			}

			if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);

			if (items.Length > 40000)
			{
				EditorUtility.DisplayProgressBar("Maintainer", "Saving items, please wait...", 0.5f);
			}

			var bf = new BinaryFormatter();
			var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			bf.Serialize(stream, items);
			stream.Close();

			EditorUtility.ClearProgressBar();

			sw.Stop();
			//Debug.Log("SaveItems time: " + sw.ElapsedMilliseconds);
		}

		private static T[] LoadItems<T>(string path)
		{
			var sw = Stopwatch.StartNew();
			T[] results = null;

			if (File.Exists(path))
			{
				var bf = new BinaryFormatter();
				var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

				if (stream.Length > 500000)
				{
					EditorUtility.DisplayProgressBar("Maintainer", "Loading items, please wait...", 0.5f);
				}

				try
				{
					results = bf.Deserialize(stream) as T[];
				}
				catch (Exception e)
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Can't read search results from " + path + ".\nThey might be generated at different Maintainer version.\n" + e);
				}
				finally
				{
					stream.Close();
					EditorUtility.ClearProgressBar();
				}

				if (results == null)
				{
					results = new T[0];
					CSEditorTools.RemoveReadOnlyAttribute(path);
					File.Delete(path);
				}
			}
			else
			{
				results = new T[0];
			}

			sw.Stop();
			//Debug.Log("LoadItems time: " + sw.ElapsedMilliseconds);

			return results;
		}

		private static void SaveItemsToJson<T>(string path, T[] items)
		{
			var sw = Stopwatch.StartNew();

			if (items == null)
			{
				items = new T[0];
			}

			if (!System.IO.Directory.Exists(Directory)) System.IO.Directory.CreateDirectory(Directory);

			if (items.Length > 40000)
			{
				EditorUtility.DisplayProgressBar("Maintainer", "Saving items, please wait...", 0.5f);
			}

			var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			var streamWriter = new StreamWriter(stream);

			var wrapper = new ItemsWrapper<T> {items = items};

			var toWrite = JsonUtility.ToJson(wrapper);
			streamWriter.Write(toWrite);
			streamWriter.Flush();
			stream.Close();

			EditorUtility.ClearProgressBar();

			sw.Stop();
			//Debug.Log("SaveItems time: " + sw.ElapsedMilliseconds);
		}

		private static T[] LoadItemsFromJson<T>(string path)
		{
			var sw = Stopwatch.StartNew();
			T[] results = null;

			if (File.Exists(path))
			{
				var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

				if (stream.Length > 500000)
				{
					EditorUtility.DisplayProgressBar("Maintainer", "Loading items, please wait...", 0.5f);
				}

				try
				{
					var streamReader = new StreamReader(stream);
					var wrapper = JsonUtility.FromJson<ItemsWrapper<T>>(streamReader.ReadToEnd());
					results = wrapper.items;
				}
				catch (Exception e)
				{
					Debug.LogWarning(Maintainer.LogPrefix + "Can't read search results from " + path + ".\nThey might be generated at different Maintainer version.\n" + e);
				}
				finally
				{
					stream.Close();
					EditorUtility.ClearProgressBar();
				}

				if (results == null)
				{
					results = new T[0];
					CSEditorTools.RemoveReadOnlyAttribute(path);
					File.Delete(path);
				}
			}
			else
			{
				results = new T[0];
			}

			sw.Stop();
			//Debug.Log("LoadItems time: " + sw.ElapsedMilliseconds);

			return results;
		}

		[Serializable]
		public class ItemsWrapper<T>
		{
			public T[] items;
		}
	}
}