#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Issues
{
	/// <summary>
	/// Allows to find issues in your Unity project. See readme for details.
	/// </summary>
	public class IssuesFinder
	{
		internal const string ModuleName = "Issues Finder";
		private const string ProgressCaption = ModuleName + ": phase {0} of {1}, item {2} of {3}";

		private static string[] scenesPaths;
		private static List<Object> fileAssets;
		private static List<string> fileAssetPaths;

		private static int phasesCount;
		private static int currentPhase;

		private static int scenesCount;
		private static int fileAssetsCount;

		private static int toFix;

		private static CSSceneTools.OpenSceneResult lastOpenSceneResult = null;

		#region public methods

		/////////////////////////////////////////////////////////////////////////
		// public methods
		/////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Starts issues search and generates report. %Maintainer window is not shown.
		/// Useful when you wish to integrate %Maintainer in your build pipeline.
		/// </summary>
		/// <returns>%Issues report, similar to the exported report from the %Maintainer window.</returns>
		public static string SearchAndReport()
		{
			var foundIssues = StartSearch(false);
			return ReportsBuilder.GenerateReport(ModuleName, foundIssues);
		}

		/// <summary>
		/// Starts search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <returns>Array of IssueRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static IssueRecord[] StartSearch(bool showResults)
		{
			phasesCount = 0;

            if (MaintainerSettings.Issues.scanGameObjects && MaintainerSettings.Issues.lookInScenes)
            {
	            var canceled = !CSSceneTools.SaveCurrentModifiedScenesIfUserWantsTo();
	            
				if (canceled)
				{
					Debug.Log(Maintainer.LogPrefix + "Issues search canceled by user!");
					return null;
				}

	            CSSceneTools.EnsureUntitledSceneHasBeenSaved("You need to save Untitled scene in order to include it in process.\nSave now? Press cancel to continue without saving.");
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			var issues = new List<IssueRecord>();
			var sw = Stopwatch.StartNew();

			lastOpenSceneResult = null;
			CSEditorTools.lastRevealSceneOpenResult = null;

			try
			{
				CollectInput();

				var searchCanceled = false;

				if (MaintainerSettings.Issues.scanGameObjects)
				{
					if (MaintainerSettings.Issues.lookInScenes)
					{
						searchCanceled = !ProcessSelectedScenes(issues);
					}

					if (!searchCanceled && MaintainerSettings.Issues.lookInAssets)
					{
						searchCanceled = !ProcessPrefabFiles(issues);
					}
				}

				if (MaintainerSettings.Issues.scanProjectSettings)
				{
					if (!searchCanceled)
					{
						searchCanceled = !ProcessSettings(issues);
					}
				}
				sw.Stop();

				if (!searchCanceled)
				{
					Debug.Log(Maintainer.LogPrefix + ModuleName + " results: " + issues.Count +
					          " issues in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
					          " seconds, " + scenesCount + " scenes and " + fileAssetsCount + " file assets scanned.");
				}
				else
				{
					Debug.Log(Maintainer.LogPrefix + "Search canceled by user!");
				}

				SearchResultsStorage.IssuesSearchResults = issues.ToArray();
				if (showResults) MaintainerWindow.ShowIssues();
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.LogPrefix + ModuleName + ": something went wrong :(\n" + e);
			}

			FinishSearch();

			return issues.ToArray();
		}

		/// <summary>
		/// Starts fix of the issues found with StartSearch() method.
		/// </summary>
		/// <param name="recordsToFix">Pass records you wish to fix here or leave null to let it load last search results.</param>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <param name="showConfirmation">Shows confirmation dialog before performing fix if true.</param>
		/// <returns>Array of IssueRecords which were fixed up.</returns>
		public static IssueRecord[] StartFix(IssueRecord[] recordsToFix = null, bool showResults = true, bool showConfirmation = true)
		{
			var records = recordsToFix;
			if (records == null)
			{
				records = SearchResultsStorage.IssuesSearchResults;
			}

			if (records.Length == 0)
			{
				return null;
			}

			if (!CSSceneTools.SaveCurrentModifiedScenesIfUserWantsTo())
			{
				return null;
			}

			toFix = 0; 

			foreach (var record in records)
			{
				if (record.selected) toFix++;
			}

			if (toFix == 0)
			{
				EditorUtility.DisplayDialog(ModuleName, "Please select issues to fix!", "Ok");
				return null;
			}

			if (showConfirmation && !EditorUtility.DisplayDialog("Confirmation", "Do you really wish to let Maintainer automatically fix " + toFix + " issues?\n" + Maintainer.DataLossWarning, "Go for it!", "Cancel"))
			{
				return null;
			}
			
			var sw = Stopwatch.StartNew();

			lastOpenSceneResult = null;
			CSEditorTools.lastRevealSceneOpenResult = null;

			var canceled = FixRecords(records);

			var fixedRecords = new List<IssueRecord>(records.Length);
			var notFixedRecords = new List<IssueRecord>(records.Length);

			foreach (var record in records)
			{
				if (record.@fixed)
				{
					fixedRecords.Add(record);
				}
				else
				{
					notFixedRecords.Add(record);
				}
			}

			records = notFixedRecords.ToArray();

			sw.Stop();

			EditorUtility.ClearProgressBar();

			if (!canceled)
			{
				var results = fixedRecords.Count +
				                 " issues fixed in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
				                 " seconds";

				Debug.Log(Maintainer.LogPrefix + ModuleName + " results: " + results);
				MaintainerWindow.ShowNotification(results);
			}
			else
			{
				Debug.Log(Maintainer.LogPrefix + "Fix canceled by user!");
			}

			if (lastOpenSceneResult != null)
			{
				CSSceneTools.SaveScene(lastOpenSceneResult.scene);
				CSSceneTools.CloseOpenedSceneIfNeeded(lastOpenSceneResult);
				lastOpenSceneResult = null;
			}

			EditorUtility.ClearProgressBar();

			SearchResultsStorage.IssuesSearchResults = records;
			if (showResults) MaintainerWindow.ShowIssues();

			return fixedRecords.ToArray();
		}

		#endregion

		#region searcher

		/////////////////////////////////////////////////////////////////////////
		// searcher
		/////////////////////////////////////////////////////////////////////////

		private static void CollectInput()
		{
			phasesCount = 0;
			currentPhase = 0;

			scenesCount = 0;
			fileAssetsCount = 0;

			if (MaintainerSettings.Issues.scanGameObjects)
			{
				if (MaintainerSettings.Issues.lookInScenes)
				{
					EditorUtility.DisplayProgressBar(ModuleName, "Collecting input data: Scenes...", 0);

					switch (MaintainerSettings.Issues.scenesSelection)
					{
						case IssuesFinderSettings.ScenesSelection.AllScenes:
						{
							scenesPaths = CSEditorTools.FindAssetsFiltered("t:Scene", MaintainerSettings.Issues.pathIncludesFilters, MaintainerSettings.Issues.pathIgnoresFilters);
							break;
						}
						case IssuesFinderSettings.ScenesSelection.IncludedScenes:
						{
							var includedScenePaths = new List<string>();

							if (MaintainerSettings.Issues.includeScenesInBuild)
							{
								includedScenePaths.AddRange(CSSceneTools.GetScenesInBuild(!MaintainerSettings.Issues.includeOnlyEnabledScenesInBuild));
							}
							
							foreach (var sceneInclude in MaintainerSettings.Issues.sceneIncludesFilters)
							{
								if (ArrayUtility.IndexOf(scenesPaths, sceneInclude.value) == -1)
								{
									includedScenePaths.Add(sceneInclude.value);
								}
							}

							scenesPaths = includedScenePaths.ToArray();
							break;
						}
						case IssuesFinderSettings.ScenesSelection.OpenedScenesOnly:
						{
							scenesPaths = CSSceneTools.GetScenesSetup().Select(s => s.path).ToArray();
							break;
						}
						default:
							throw new ArgumentOutOfRangeException();
					}

					scenesCount = scenesPaths.Length;
					phasesCount++;
				}

				if (MaintainerSettings.Issues.lookInAssets)
				{
					if (fileAssets == null)
						fileAssets = new List<Object>();
					else
						fileAssets.Clear();

					if (fileAssetPaths == null)
						fileAssetPaths = new List<string>();
					else
						fileAssetPaths.Clear();

					EditorUtility.DisplayProgressBar(ModuleName, "Collecting input data: File assets...", 0);

					var filteredPaths = CSEditorTools.FindAssetsFiltered("t:Prefab, t:ScriptableObject", MaintainerSettings.Issues.pathIncludesFilters, MaintainerSettings.Issues.pathIgnoresFilters);
					fileAssetsCount = CSEditorTools.GetSuitableFileAssetsFromSelection(filteredPaths, fileAssets, fileAssetPaths);

					phasesCount++;
				}
			}

			if (MaintainerSettings.Issues.scanProjectSettings)
			{
				phasesCount++;
			}
		}

		private static bool ProcessSelectedScenes(List<IssueRecord> issues)
		{
			var result = true;
			currentPhase ++;

			for (var i = 0; i < scenesCount; i++)
			{
				var scenePath = scenesPaths[i];

				if (EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, i + 1, scenesCount), "Opening scene: " + Path.GetFileNameWithoutExtension(scenePath), (float)i / scenesCount))
				{
					result = false;
					break;
				}

				if (MaintainerSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.OpenedScenesOnly &&
				    !File.Exists(scenePath)) continue;

				var openSceneResult = CSSceneTools.OpenScene(scenePath);
				if (!openSceneResult.success)
				{
					Debug.LogWarning(Maintainer.ConstructWarning("Can't open scene " + scenePath));
					continue;
				}

				var sceneName = Path.GetFileNameWithoutExtension(scenePath);
				if (!ProcessScene(openSceneResult.scene, issues, sceneName, i)) break;

				CSSceneTools.CloseOpenedSceneIfNeeded(openSceneResult);
			}
			return result;
		}

		private static bool ProcessScene(Scene scene, List<IssueRecord> issues, string sceneName = null, int sceneIndex = -1)
		{
			var result = true;

			var gameObjects = CSObjectTools.GetAllGameObjectsInScene(scene);
			var objectsCount = gameObjects.Length;

			var updateStep = Math.Max(objectsCount / MaintainerSettings.UpdateProgressStep, 1);
			var onlyOpenedScenes = sceneIndex == -1;

			for (var i = 0; i < objectsCount; i++)
			{
				if (i % updateStep == 0)
				{
					if (onlyOpenedScenes)
					{
						if (EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, 1, 1), string.Format("Processing opened scenes... {0}%", i * 100 / objectsCount), (float)i / objectsCount))
						{
							result = false;
							break;
						}
					}
					else
					{
						if (EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, sceneIndex + 1, scenesCount), string.Format("Processing scene: {0} ... {1}%", sceneName, i * 100 / objectsCount), (float)sceneIndex / scenesCount))
						{
							result = false;
							break;
						}
					}
				}
				CheckGameObjectForIssues(issues, gameObjects[i]);
			}

			return result;
		}

		private static bool ProcessPrefabFiles(List<IssueRecord> issues)
		{
			var result = true;
			currentPhase++;

			var updateStep = Math.Max(fileAssetsCount / MaintainerSettings.UpdateProgressStep, 1);
			for (var i = 0; i < fileAssetsCount; i++)
			{
				if (i % updateStep == 0 && EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, i+1, fileAssetsCount), "Processing prefabs files...", (float)i / fileAssetsCount))
				{
					result = false;
					break;
				}

				var fileAsset = fileAssets[i];

				if (fileAsset is GameObject)
				{
					CheckGameObjectForIssues(issues, fileAsset as GameObject, fileAssetPaths[i]);
				}
				else if (fileAsset is ScriptableObject)
				{
					CheckScriptableObjectForIssues(issues, fileAsset as ScriptableObject, fileAssetPaths[i]);
				}
			}

			return result;
		}

		private static void CheckGameObjectForIssues(List<IssueRecord> issues, GameObject go, string assetPath = null)
		{
			var location = string.IsNullOrEmpty(assetPath) ? RecordLocation.Scene : RecordLocation.Prefab;

			// ----------------------------------------------------------------------------
			// looking for object-level issues
			// ----------------------------------------------------------------------------

			if (!MaintainerSettings.Issues.touchInactiveGameObjects)
			{
				if (location == RecordLocation.Scene)
				{
					if (!go.activeInHierarchy) return;
				}
				else
				{
					if (!go.activeSelf) return;
				}
			}

			// ----------------------------------------------------------------------------
			// checking stuff related to the prefabs in scenes
			// ----------------------------------------------------------------------------

			if (location == RecordLocation.Scene)
			{
				assetPath = go.scene.path;
				var prefabType = PrefabUtility.GetPrefabType(go);

				if (prefabType != PrefabType.None)
				{
					/* checking if we're inside of nested prefab with same type as root,
					   allows to skip detections of missed and disconnected prefabs children */

					var rootPrefab = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
					var rootPrefabHasSameType = false;
					if (rootPrefab != go)
					{
						var rootPrefabType = PrefabUtility.GetPrefabType(rootPrefab);
						if (rootPrefabType == prefabType)
						{
							rootPrefabHasSameType = true;
						}
					}

					/* checking for missing and disconnected instances */

					if (prefabType == PrefabType.MissingPrefabInstance)
					{
						if (MaintainerSettings.Issues.missingPrefabs && !rootPrefabHasSameType)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.MissingPrefab, location, assetPath, go));
						}
					}
					else if (prefabType == PrefabType.DisconnectedPrefabInstance ||
							 prefabType == PrefabType.DisconnectedModelPrefabInstance)
					{
						if (MaintainerSettings.Issues.disconnectedPrefabs && !rootPrefabHasSameType)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.DisconnectedPrefab, location, assetPath, go));
						}
					}

					/* checking if this game object is actually prefab instance
					   without any changes, so we can skip it if we have assets search enabled */

					if (prefabType != PrefabType.DisconnectedPrefabInstance &&
						prefabType != PrefabType.DisconnectedModelPrefabInstance &&
						prefabType != PrefabType.MissingPrefabInstance && MaintainerSettings.Issues.lookInAssets)
					{
						var skipThisPrefabInstance = true;
						 
						// we shouldn't skip object if it's nested deeper 2nd level
						if (CSEditorTools.GetDepthInHierarchy(go.transform, rootPrefab.transform) >= 2)
						{
							skipThisPrefabInstance = false;
						}
						else
						{
							var modifications = PrefabUtility.GetPropertyModifications(go);
							foreach (var modification in modifications)
							{
								var target = modification.target;

								if (target is Transform)
								{
									if (!MaintainerSettings.Issues.hugePositions) continue;

									var transform = (Transform)target;
									if (!TransformHasHugePosition(transform)) continue;
									if (go.transform.position == transform.position && go.transform.rotation == transform.rotation && go.transform.localScale == transform.localScale) continue;
								}

								if (target is GameObject && modification.propertyPath == "m_Name")
								{
									continue;
								}

								skipThisPrefabInstance = false;
								break;
							}
						}

						if (skipThisPrefabInstance)
						{
							var parentObject = PrefabUtility.GetCorrespondingObjectFromSource(go) as GameObject;
							if (parentObject != null)
							{
								var goComponents = go.GetComponents<Component>();
								var prefabComponents = parentObject.GetComponents<Component>();
								if (goComponents.Length > prefabComponents.Length)
								{
									skipThisPrefabInstance = false;
								}
								else
								{
									for (var i = 0; i < goComponents.Length; i++)
									{
										var component = goComponents[i];
										var prefabComponent = prefabComponents[i];

										if (component == null || prefabComponent == null) continue;
										if (component.GetType() == prefabComponent.GetType()) continue;

										skipThisPrefabInstance = false;
										break;
									}
								}
							}
						}

						if (skipThisPrefabInstance) return;
					}
				}
			}

			// ----------------------------------------------------------------------------
			// checking for Game Object - level issues
			// ----------------------------------------------------------------------------

			if (MaintainerSettings.Issues.undefinedTags)
			{
				var undefinedTag = false;
				try
				{
					if (string.IsNullOrEmpty(go.tag))
					{
						undefinedTag = true;
					}
				}
				catch (UnityException e)
				{
					if (e.Message.Contains("undefined tag"))
					{
						undefinedTag = true;
					}
					else
					{
						Debug.LogError(Maintainer.LogPrefix + "Unknown error while checking tag of the " + go.name + "\n" + e);
					}
				}

				if (undefinedTag)
				{
					issues.Add(ObjectIssueRecord.Create(RecordType.UndefinedTag, location, assetPath, go));
				}
			}

			if (MaintainerSettings.Issues.unnamedLayers)
			{
				var layerIndex = go.layer;
				if (string.IsNullOrEmpty(LayerMask.LayerToName(layerIndex)))
				{
					var issue = ObjectIssueRecord.Create(RecordType.UnnamedLayer, location, assetPath, go);
					issue.headerExtra = "(index: " + layerIndex + ")";
					issues.Add(issue);
				}
			}

			// ----------------------------------------------------------------------------
			// checking all components for ignores
			// ----------------------------------------------------------------------------

			var checkForIgnores = MaintainerSettings.Issues.componentIgnoresFilters != null && MaintainerSettings.Issues.componentIgnoresFilters.Length > 0;
			var skipEmptyMeshFilter = false;
			var skipEmptyAudioSource = false;

			var allComponents = go.GetComponents<Component>();
			var allComponentsCount = allComponents.Length;

			var components = new List<Component>(allComponentsCount);
			var componentsTypes = new List<Type>(allComponentsCount);
			var componentsNames = new List<string>(allComponentsCount);
			var componentsNamespaces = new List<string>(allComponentsCount);

			var componentsCount = 0;
			var missingComponentsCount = 0;

			for (var i = 0; i < allComponentsCount; i++)
			{
				var component = allComponents[i];

				if (component == null)
				{
					missingComponentsCount++;
					continue;
				}

				var componentType = component.GetType();
				var componentName = componentType.Name;
				var componentFullName = componentType.FullName;
				var componentNamespace = componentType.Namespace;

				if (!componentType.IsSubclassOf(typeof(Component)))
				{
					Debug.LogWarning(Maintainer.LogPrefix + "This object is pretend to be a Component, but is not a subclass of the Component:\n" +
					                 "Name: " + componentName + "\n" +
									 "Type: " + componentType + "\n" +
									 "Namespace: " + componentNamespace
					);
					continue;
				}

				/* 
				*  checking object for the components which may affect 
				*  other components and produce false positives 
				*/

				// allowing empty mesh filters for the objects with attached TextMeshPro and 2D Toolkit components.
				if (!skipEmptyMeshFilter)
				{
					skipEmptyMeshFilter = (componentFullName == "TMPro.TextMeshPro") || componentName.StartsWith("tk2d");
				}

				// allowing empty AudioSources for the objects with attached standard FirstPersonController.
				if (!skipEmptyAudioSource)
				{
					skipEmptyAudioSource = componentFullName == "UnityStandardAssets.Characters.FirstPerson.FirstPersonController";
				}

				// skipping disabled components
				if (!MaintainerSettings.Issues.touchDisabledComponents)
				{
					if (EditorUtility.GetObjectEnabled(component) == 0) continue;
				}

				// skipping ignored components
				if (checkForIgnores)
				{
					if (Array.IndexOf(MaintainerSettings.Issues.componentIgnoresFilters, componentName) != -1) continue;
				}

				components.Add(component);
				componentsTypes.Add(componentType);
				componentsNames.Add(componentName);
				componentsNamespaces.Add(componentNamespace);
				componentsCount++;
			}

			if (missingComponentsCount > 0 && MaintainerSettings.Issues.missingComponents)
			{
				var record = ObjectIssueRecord.Create(RecordType.MissingComponent, location, assetPath, go, null, null, -1);
				record.headerFormatArgument = missingComponentsCount;
				issues.Add(record);
			}

			Dictionary<string, int> uniqueTypes = null;
			List<int> similarComponentsIndexes = null;

			TerrainData terrainTerrainData = null;
			TerrainData terrainColliderTerrainData = null;
			var terrainChecked = false;
			var terrainColliderChecked = false;

			// ----------------------------------------------------------------------------
			// looking for component-level issues
			// ----------------------------------------------------------------------------

			for (var i = 0; i < componentsCount; i++)
			{
				var component = components[i];
				var componentType = componentsTypes[i];
				var componentName = componentsNames[i];
				//string componentFullName = componentsFullNames[i];
				var componentNamespace = componentsNamespaces[i];

				if (component is Transform)
				{
					if (MaintainerSettings.Issues.hugePositions)
					{
						if (TransformHasHugePosition((Transform)component))
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.HugePosition, location, assetPath, go, componentType, componentName, i, "Position"));
						}
					}
					continue;
				}

				if (MaintainerSettings.Issues.duplicateComponents &&
					(componentNamespace != "Fabric"))
				{
					// initializing dictionary and list on first usage
					if (uniqueTypes == null) uniqueTypes = new Dictionary<string, int>(componentsCount);
					if (similarComponentsIndexes == null) similarComponentsIndexes = new List<int>(componentsCount);

					var realComponentType = CSObjectTools.GetNativeObjectType(component);
					if (string.IsNullOrEmpty(realComponentType)) realComponentType = componentType.ToString();

					// checking if current component type already met before
					if (uniqueTypes.ContainsKey(realComponentType))
					{
						var uniqueTypeIndex = uniqueTypes[realComponentType];

						// checking if initially met component index already in indexes list
						// since we need to compare all duplicate candidates against initial component
						if (!similarComponentsIndexes.Contains(uniqueTypeIndex)) similarComponentsIndexes.Add(uniqueTypeIndex);

						// adding current component index to the indexes list
						similarComponentsIndexes.Add(i);
					}
					else
					{
						uniqueTypes.Add(realComponentType, i);
					}
				}

				// ----------------------------------------------------------------------------
				// looping through the component's SerializedProperties via SerializedObject
				// ----------------------------------------------------------------------------

				var emptyArrayItems = new Dictionary<string, int>();
				var so = new SerializedObject(component);
				var sp = so.GetIterator();
				var arrayLength = 0;
				var skipEmptyComponentCheck = false;

				while (sp.NextVisible(true))
				{
					var fullPropertyPath = sp.propertyPath;

					if (sp.isArray)
					{
						arrayLength = sp.arraySize;
					}

					var isArrayItem = fullPropertyPath.EndsWith("]", StringComparison.Ordinal);

					if (MaintainerSettings.Issues.missingReferences)
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
							{
								var propertyName = isArrayItem ? CSObjectTools.GetArrayItemNameAndIndex(fullPropertyPath) : sp.name;
								var record = ObjectIssueRecord.Create(RecordType.MissingReference, location, assetPath, go, componentType, componentName, i, propertyName);
								record.propertyPath = sp.propertyPath;
								issues.Add(record);

								if (component is MeshCollider && sp.name == "m_Mesh")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is MeshFilter && sp.name == "m_Mesh")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is Renderer && fullPropertyPath.StartsWith("m_Materials.Array.") && arrayLength == 1)
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is SpriteRenderer && sp.name == "m_Sprite")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is Animation && sp.name == "m_Animation")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is TerrainCollider && sp.name == "m_TerrainData")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is AudioSource && sp.name == "m_audioClip")
								{
									skipEmptyComponentCheck = true;
								}
							}
						}
					}

					if (location == RecordLocation.Scene || !MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs)
					{
						if (MaintainerSettings.Issues.emptyArrayItems && isArrayItem)
						{
							// ignoring components where empty array items is a normal behavior
							if (component is SpriteRenderer) continue;
							if (component is MeshRenderer && arrayLength == 1) continue;
							if (component is ParticleSystemRenderer && arrayLength == 1) continue;
							if (componentName.StartsWith("TextMeshPro")) continue;

							if (sp.propertyType == SerializedPropertyType.ObjectReference &&
							    sp.objectReferenceValue == null &&
							    sp.objectReferenceInstanceIDValue == 0)
							{
								var arrayName = CSObjectTools.GetArrayItemName(fullPropertyPath);

								// ignoring TextMeshPro's FontAssetArrays with 16 empty items inside
								if (!emptyArrayItems.ContainsKey(arrayName))
								{
									emptyArrayItems.Add(arrayName, 0);
								}
								emptyArrayItems[arrayName]++;
							}
						}
					}
					/*else
					{
						continue;
					}*/
				}

				if (MaintainerSettings.Issues.emptyArrayItems)
				{
					foreach (var item in emptyArrayItems.Keys)
					{
						var issueRecord = ObjectIssueRecord.Create(RecordType.EmptyArrayItem, location, assetPath, go, componentType, componentName, i, item);
						issueRecord.headerFormatArgument = emptyArrayItems[item];
						issues.Add(issueRecord);
					}
				}

				// ----------------------------------------------------------------------------
				// specific components checks
				// ----------------------------------------------------------------------------

				if (component is MeshCollider)
				{
					if (MaintainerSettings.Issues.emptyMeshColliders && !skipEmptyComponentCheck)
					{
						if ((component as MeshCollider).sharedMesh == null)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.EmptyMeshCollider, location, assetPath, go, componentType, componentName, i));
						}
					}
				}
				else if (component is MeshFilter)
				{
					if (MaintainerSettings.Issues.emptyMeshFilters && !skipEmptyMeshFilter && !skipEmptyComponentCheck)
					{
						if ((component as MeshFilter).sharedMesh == null)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.EmptyMeshFilter, location, assetPath, go, componentType, componentName, i));
						}
					}
				}
				else if (component is Renderer)
				{
					var renderer = (Renderer)component;
					if (MaintainerSettings.Issues.emptyRenderers && !skipEmptyComponentCheck)
					{
						var hasMaterial = false;
						foreach (var material in renderer.sharedMaterials)
						{
							if (material != null)
							{
								hasMaterial = true;
								break;
							}
						}

#if UNITY_5_5_OR_NEWER
						var particleSystemRenderer = renderer as ParticleSystemRenderer;
						if (particleSystemRenderer != null)
						{
							if (particleSystemRenderer.renderMode == ParticleSystemRenderMode.None)
							{
								hasMaterial = true;
							}
						}
#endif
						if (!hasMaterial)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.EmptyRenderer, location, assetPath, go, componentType, componentName, i));
						}
					}

					if (component is SpriteRenderer)
					{
						if (MaintainerSettings.Issues.emptySpriteRenderers && !skipEmptyComponentCheck)
						{
							if ((component as SpriteRenderer).sprite == null)
							{
								issues.Add(ObjectIssueRecord.Create(RecordType.EmptySpriteRenderer, location, assetPath, go, componentType, componentName, i));
							}
						}
					}
				}
				else if (component is Animation)
				{
					if (MaintainerSettings.Issues.emptyAnimations && !skipEmptyComponentCheck)
					{
						var animation = (Animation)component;
						var isEmpty = false;
						if (animation.GetClipCount() <= 0 && animation.clip == null)
						{
							isEmpty = true;
						}
						else
						{
							var clipsCount = 0;
							
							foreach (var clip in animation)
							{
								if (clip != null) clipsCount++;
							}

							if (clipsCount == 0)
							{
								isEmpty = true;
							}
						}

						if (isEmpty)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.EmptyAnimation, location, assetPath, go, componentType, componentName, i));
						}
					}
				}
				else if (component is Terrain)
				{
					if (MaintainerSettings.Issues.inconsistentTerrainData)
					{
						terrainTerrainData = (component as Terrain).terrainData;
						terrainChecked = true;
					}
				}
				else if (component is TerrainCollider)
				{
					if (MaintainerSettings.Issues.inconsistentTerrainData)
					{
						terrainColliderTerrainData = (component as TerrainCollider).terrainData;
						terrainColliderChecked = true;
					}

					if (MaintainerSettings.Issues.emptyTerrainCollider && !skipEmptyComponentCheck)
					{
						if ((component as TerrainCollider).terrainData == null)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.EmptyTerrainCollider, location, assetPath, go, componentType, componentName, i));
						}
					}
				}
				else if (component is AudioSource)
				{
					if (MaintainerSettings.Issues.emptyAudioSource && !skipEmptyAudioSource && !skipEmptyComponentCheck)
					{
						if ((component as AudioSource).clip == null)
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.EmptyAudioSource, location, assetPath, go, componentType, componentName, i));
						}
					}
				}
			}

			if (MaintainerSettings.Issues.inconsistentTerrainData && 
				terrainColliderTerrainData != terrainTerrainData &&
				terrainChecked && terrainColliderChecked)
			{
				issues.Add(ObjectIssueRecord.Create(RecordType.InconsistentTerrainData, location, assetPath, go));
			}

			// ----------------------------------------------------------------------------
			// duplicates search
			// ----------------------------------------------------------------------------

			if (MaintainerSettings.Issues.duplicateComponents)
			{
				if (similarComponentsIndexes != null && similarComponentsIndexes.Count > 0)
				{
					var similarComponentsCount = similarComponentsIndexes.Count;
					var similarComponentsHashes = new List<long>(similarComponentsCount);

					for (var i = 0; i < similarComponentsCount; i++)
					{
						var componentIndex = similarComponentsIndexes[i];
						var component = components[componentIndex];

						long componentHash = 0;

						if (MaintainerSettings.Issues.duplicateComponentsPrecise)
						{
							var so = new SerializedObject(component);
							var sp = so.GetIterator();
							while (sp.NextVisible(true))
							{
								componentHash += CSEditorTools.GetPropertyHash(sp);
							}
						}

						similarComponentsHashes.Add(componentHash);
					}

					var distinctItems = new List<long>(similarComponentsCount);

					for (var i = 0; i < similarComponentsCount; i++)
					{
						var componentIndex = similarComponentsIndexes[i];

						if (distinctItems.Contains(similarComponentsHashes[i]))
						{
							issues.Add(ObjectIssueRecord.Create(RecordType.DuplicateComponent, location, assetPath, go, componentsTypes[componentIndex], componentsNames[componentIndex], componentIndex));
						}
						else
						{
							distinctItems.Add(similarComponentsHashes[i]);
						}
					}
				}
			}
		}

		private static void CheckScriptableObjectForIssues(List<IssueRecord> issues, ScriptableObject scriptableObject, string fileAssetPath)
		{
			var so = new SerializedObject(scriptableObject);
			var sp = so.GetIterator();

			while (sp.NextVisible(true))
			{
				var fullPropertyPath = sp.propertyPath;

				var isArrayItem = fullPropertyPath.EndsWith("]", StringComparison.Ordinal);

				if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
				{
					var propertyName = isArrayItem ? CSObjectTools.GetArrayItemNameAndIndex(fullPropertyPath) : sp.name;
					var record = ObjectIssueRecord.Create(RecordType.MissingReference, RecordLocation.Asset, fileAssetPath, scriptableObject, null, null, -1, propertyName);
					record.propertyPath = sp.propertyPath;
					issues.Add(record);
				}
			}
		}

		private static bool TransformHasHugePosition(Transform transform)
		{
			var position = transform.position;

			if (Math.Abs(position.x) > 100000f || Math.Abs(position.y) > 100000f || Math.Abs(position.z) > 100000f)
			{
				return true;
			}
			return false;
		}

		private static bool ProcessSettings(List<IssueRecord> issues)
		{
			var result = true;
			currentPhase++;

			if (MaintainerSettings.Issues.duplicateScenesInBuild)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, 1, 1), "Checking settings: Build Settings", (float)0/1))
				{
					result = false;
				}
			}

			CheckBuildSettings(issues);

			if (MaintainerSettings.Issues.duplicateTagsAndLayers)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(ProgressCaption, currentPhase, phasesCount, 1, 1), "Checking settings: Tags and Layers", (float)0/1))
				{
					result = false;
				}
			}

			CheckTagsAndLayers(issues);

			return result;
		}

		private static void CheckBuildSettings(List<IssueRecord> issues)
		{
			if (MaintainerSettings.Issues.duplicateScenesInBuild)
			{
				var scenesForBuild = CSSceneTools.GetScenesInBuild();
				var duplicates = CSArrayTools.FindDuplicatesInArray(scenesForBuild);

				foreach (var duplicate in duplicates)
				{
					issues.Add(BuildSettingsIssueRecord.Create(RecordType.DuplicateScenesInBuild, 
						"<b>Duplicate scene:</b> " + CSEditorTools.NicifyAssetPath(duplicate, true)));
				}
			}
		}
		 
		private static void CheckTagsAndLayers(List<IssueRecord> issues)
		{
			if (MaintainerSettings.Issues.duplicateTagsAndLayers)
			{
				var issueBody = new StringBuilder();

				/* looking for duplicates in tags*/

				var tags = new List<string>(InternalEditorUtility.tags);
				tags.RemoveAll(string.IsNullOrEmpty);
				var duplicateTags = CSArrayTools.FindDuplicatesInArray(tags);

				if (duplicateTags.Count > 0)
				{
					issueBody.Append("Duplicate <b>tag(s)</b>: ");

					foreach (var duplicate in duplicateTags)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				/* looking for duplicates in layers*/

				var layers = new List<string>(InternalEditorUtility.layers);
				layers.RemoveAll(string.IsNullOrEmpty);
				var duplicateLayers = CSArrayTools.FindDuplicatesInArray(layers);

				if (duplicateLayers.Count > 0)
				{
					if (issueBody.Length > 0) issueBody.AppendLine();
					issueBody.Append("Duplicate <b>layer(s)</b>: ");

					foreach (var duplicate in duplicateLayers)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				/* looking for duplicates in sorting layers*/

				var sortingLayers = new List<string>((string[])CSReflectionTools.GetSortingLayersPropertyInfo().GetValue(null, new object[0]));
				sortingLayers.RemoveAll(string.IsNullOrEmpty);
				var duplicateSortingLayers = CSArrayTools.FindDuplicatesInArray(sortingLayers);

				if (duplicateSortingLayers.Count > 0)
				{
					if (issueBody.Length > 0) issueBody.AppendLine();
					issueBody.Append("Duplicate <b>sorting layer(s)</b>: ");

					foreach (var duplicate in duplicateSortingLayers)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				if (issueBody.Length > 0)
				{
					issues.Add(TagsAndLayersIssueRecord.Create(RecordType.DuplicateTagsAndLayers, issueBody.ToString()));
				}

				issueBody.Length = 0;
			}
		}

		private static void FinishSearch()
		{
			EditorUtility.ClearProgressBar();
		}

		#endregion

		#region fixer

		/////////////////////////////////////////////////////////////////////////
		// fixer
		/////////////////////////////////////////////////////////////////////////

		private static bool FixRecords(IssueRecord[] results, bool showProgress = true)
		{
			var canceled = false;
			var i = 0;

			var sortedRecords = results.OrderBy(RecordsSortings.issueRecordByPath).ToArray();
			var updateStep = Math.Max(sortedRecords.Length / MaintainerSettings.UpdateProgressStep, 1);

			for (var k = 0; k < sortedRecords.Length; k++)
			{
				var item = sortedRecords[k];

				if (showProgress && k % updateStep == 0 && EditorUtility.DisplayCancelableProgressBar(
					    string.Format(ProgressCaption, currentPhase, phasesCount, i + 1, toFix), "Resolving selected issues...",
					    (float) i / toFix))
				{
					canceled = true;
					break;
				}

				if (item.selected)
				{
					var objectIssueRecord = item as ObjectIssueRecord;
					if (objectIssueRecord != null)
					{
						if (objectIssueRecord.location == RecordLocation.Scene)
						{
							var newOpenSceneResult = CSSceneTools.OpenScene(objectIssueRecord.path);
							if (!newOpenSceneResult.success)
							{
								continue;
							}

							if (newOpenSceneResult.sceneWasLoaded)
							{
								if (lastOpenSceneResult != null)
								{
									CSSceneTools.SaveScene(lastOpenSceneResult.scene);
									CSSceneTools.CloseOpenedSceneIfNeeded(lastOpenSceneResult);
								}
							}

							if (lastOpenSceneResult == null || lastOpenSceneResult.scene != newOpenSceneResult.scene)
							{
								lastOpenSceneResult = newOpenSceneResult;
							}
						}
					}

					if (item.CanBeFixed())
					{
						item.Fix(true);
					}

					i++;
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return canceled;
		}

		#endregion
	}
}