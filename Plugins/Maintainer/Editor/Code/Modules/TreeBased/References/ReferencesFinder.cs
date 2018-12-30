#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeStage.Maintainer.Core;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using TreeEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if UNITY_5_6_OR_NEWER
using UnityEngine.AI;
#endif

namespace CodeStage.Maintainer.References
{
	/// <summary>
	/// Allows to find references of specific objects in your project (where objects are referenced).
	/// </summary>
	public class ReferencesFinder
	{
		private class TreeConjunction
		{
			public readonly List<ReferencesTreeElement> treeElements = new List<ReferencesTreeElement>();
			public AssetInfo referencedAsset;
			public ReferencedAtInfo referencedAtInfo;
			public Object[] referencedObjectsCandidates;

			public bool traverseDeeper = true;
			public bool traverseInvisiblePrefabModificationsDeeper = true;
		}

		private class AssetConjunctionInfo
		{
			public readonly List<TreeConjunction> conjunctions = new List<TreeConjunction>();
			public AssetInfo asset;
		}

		internal const string ModuleName = "References Finder";

		private const string ProgressCaption = ModuleName + ": phase {0} of {1}";
		private const string ProgressText = "{0}: asset {1} of {2}";
		private const int PhasesCount = 2;

		private static readonly List<AssetConjunctionInfo> conjunctionInfoList = new List<AssetConjunctionInfo>();
		public static bool debugMode;

		/// <summary>
		/// Gets current Project View selection and calls GetReferences() method with respect to all settings regarding selections.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] AddSelectedToSelectionAndRun(bool showResults = true)
		{
			var selection = CSEditorTools.GetProjectSelections(true);
			return AddToSelectionAndRun(selection);
		}

		/// <summary>
		/// Adds new assets to the last selection if it existed and calls a GetReferences() with extended selection;
		/// </summary>
		/// <param name="selectedAssets">Additionally selected assets.</param>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] AddToSelectionAndRun(string[] selectedAssets, bool showResults = true)
		{
			var additiveSelection = new FilterItem[selectedAssets.Length];
			for (var i = 0; i < selectedAssets.Length; i++)
			{
				additiveSelection[i] = FilterItem.Create(selectedAssets[i], FilterKind.Path);
			}

			return AddToSelectionAndRun(additiveSelection, showResults);
		}

		/// <summary>
		/// Adds new assets to the last selection if it existed and calls a GetReferences() with extended selection;
		/// </summary>
		/// <param name="selectedAssets">Additionally selected assets.</param>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] AddToSelectionAndRun(FilterItem[] selectedAssets, bool showResults = true)
		{
			if (MaintainerSettings.References.selectedFindClearsResults)
			{
				SearchResultsStorage.ReferencesSearchSelection = new FilterItem[0];
				SearchResultsStorage.ReferencesSearchResults = new ReferencesTreeElement[0];
			}

			var currentSelection = SearchResultsStorage.ReferencesSearchSelection;

			var newItem = false;

			foreach (var selectedAsset in selectedAssets)
			{
				newItem |= CSEditorTools.TryAddNewItemToFilters(ref currentSelection, selectedAsset);
			}

#if UNITY_5_6_OR_NEWER
			if (selectedAssets.Length == 1)
			{
				ReferencesTab.pathToSelect = selectedAssets[0].value;
			}
#endif

			if (newItem)
			{
				return GetReferences(currentSelection, showResults);
			}

#if UNITY_5_6_OR_NEWER
			ReferencesTab.showAlreadyExistNotification = true;
			MaintainerWindow.ShowReferences();
#endif
			return SearchResultsStorage.ReferencesSearchResults;
		}

		/// <summary>
		/// Returns references of all assets at the project, e.g. where each asset is referenced.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] GetReferences(bool showResults = true)
		{
			if (!MaintainerSettings.References.fullProjectScanWarningShown)
			{
				if (!EditorUtility.DisplayDialog(ModuleName,
					"Full project scan may take significant amount of time if your project is very big.\nAre you sure you wish to make a full project scan?\nThis message shows only before first full scan.",
					"Yes", "Nope"))
				{
					return null;
				}

				MaintainerSettings.References.fullProjectScanWarningShown = true;
				MaintainerSettings.Save();
			}

			return GetReferences(null, showResults);
		}

		/// <summary>
		/// Returns references of selectedAssets or all assets at the project (if selectedAssets is null), e.g. where each asset is referenced, with additional filtration of the results.
		/// </summary>
		/// <param name="selectedAssets">Assets you wish to show references for. Pass null to process all assets in the project.</param>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of ReferencesTreeElement for the TreeView buildup or manual parsing.</returns>
		public static ReferencesTreeElement[] GetReferences(FilterItem[] selectedAssets, bool showResults = true)
		{
			var results = new List<ReferencesTreeElement>();

			conjunctionInfoList.Clear();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			try
			{
				var sw = Stopwatch.StartNew();

				CSEditorTools.lastRevealSceneOpenResult = null;

				var searchCanceled = LookForReferences(selectedAssets, results);
				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!searchCanceled)
				{
					var resultsCount = results.Count;
					if (resultsCount <= 1)
					{
						MaintainerWindow.ShowNotification("Nothing found!");
					}

					Debug.Log(Maintainer.LogPrefix + ModuleName + " results: " + (resultsCount - 1) +
					          " items found in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
					          " seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LogPrefix + ModuleName + "Search canceled by user!");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.LogPrefix + ModuleName + ": " + e);
				EditorUtility.ClearProgressBar();
			}

			BuildSelectedAssetsFromResults(results);

			SearchResultsStorage.ReferencesSearchResults = results.ToArray();

			if (showResults)
			{
#if UNITY_5_6_OR_NEWER
				MaintainerWindow.ShowReferences();
#else
				Debug.LogError(Maintainer.LogPrefix + ModuleName + ":  Unity 5.6+ required to show GetReferences results.");
#endif
			}

			return results.ToArray();
		}

		private static void BuildSelectedAssetsFromResults(List<ReferencesTreeElement> results)
		{
			var resultsCount = results.Count;
			var showProgress = resultsCount > 500000;

			if (showProgress) EditorUtility.DisplayProgressBar(ModuleName, "Parsing results...", 0);

			var rootItems = new List<FilterItem>(resultsCount);
			var updateStep = Math.Max(resultsCount / MaintainerSettings.UpdateProgressStep, 1);
			for (var i = 0; i < resultsCount; i++)
			{
				if (showProgress && i % updateStep == 0) EditorUtility.DisplayProgressBar(ModuleName, "Parsing results...", (float)i / resultsCount);

				var result = results[i];
				if (result.depth != 0) continue;
				rootItems.Add(FilterItem.Create(result.assetPath, FilterKind.Path));
			}

			SearchResultsStorage.ReferencesSearchSelection = rootItems.ToArray();
		}

		private static bool LookForReferences(FilterItem[] selectedAssets, List<ReferencesTreeElement> results)
		{
			var canceled = !CSSceneTools.SaveCurrentModifiedScenesIfUserWantsTo();
			
			if (!canceled)
			{
				CSSceneTools.EnsureUntitledSceneHasBeenSaved("You need to save Untitled scene in order to include it in process.\nSave now? Press cancel to continue without saving.");

				var map = AssetsMap.GetUpdated();
				var count = map.assets.Count;
				var updateStep = Math.Max(count / MaintainerSettings.UpdateProgressStep, 1);

				var root = new ReferencesTreeElement
				{
					id = results.Count,
					name = "root",
					depth = -1
				};
				results.Add(root);

				for (var i = 0; i < count; i++)
				{
					if (i % updateStep == 0 && EditorUtility.DisplayCancelableProgressBar(
						    string.Format(ProgressCaption, 1, PhasesCount),
						    string.Format(ProgressText, "Building references tree", i + 1, count),
						    (float) i / count))
					{
						canceled = true;
						break;
					}

					var assetInfo = map.assets[i];

					// excludes settings assets from the list depth 0 items
					if (assetInfo.SettingsKind != AssetSettingsKind.NotSettings) continue;

					if (selectedAssets != null)
					{
						if (!CSEditorTools.IsValueMatchesAnyFilter(assetInfo.AssetPath, selectedAssets)) continue;
					}

					if (CSEditorTools.IsValueMatchesAnyFilter(assetInfo.AssetPath, MaintainerSettings.References.pathIgnoresFilters)
					) continue;

					var branchElements = new List<ReferencesTreeElement>();
					BuildTreeBranchRecursive(assetInfo, 0, results.Count, branchElements);
					results.AddRange(branchElements);
				}
			}

			if (!canceled)
			{
				canceled = FillReferenceEntries();
			}

			if (!canceled)
			{
				AssetsMap.Save();
			}

#if UNITY_5_6_OR_NEWER
			if (canceled)
			{
				ReferencesTab.showAlreadyExistNotification = false;
				ReferencesTab.pathToSelect = null;
			}
#endif

			return canceled;
		}

		private static ReferencesTreeElement BuildTreeBranchRecursive(AssetInfo assetInfo, int depth, int id, List<ReferencesTreeElement> results)
		{
			if (!MaintainerSettings.References.showAssetsWithoutReferences && depth == 0)
			{
				if (assetInfo.referencedAtInfoList.Count == 0) return null;

				var allIgnored = true;
				foreach (var referencedAtInfo in assetInfo.referencedAtInfoList)
				{
					if (CSEditorTools.IsValueMatchesAnyFilter(referencedAtInfo.assetInfo.AssetPath,
						MaintainerSettings.References.pathIgnoresFilters)) continue;

					allIgnored = false;
					break;
				}

				if (allIgnored) return null;
			}

			var assetPath = assetInfo.AssetPath;
			var assetType = assetInfo.Type;
			var assetTypeName = assetInfo.SettingsKind == AssetSettingsKind.NotSettings ? assetType.Name : "Settings Asset";

			var element = new ReferencesTreeElement
			{
				id = id + results.Count,
				name = assetInfo.SettingsKind == AssetSettingsKind.NotSettings ? CSEditorTools.NicifyAssetPath(assetInfo.AssetPath) : assetInfo.AssetPath,
				assetPath = assetPath,
				assetTypeName = assetTypeName,
				assetSize = assetInfo.Size,
				assetSizeFormatted = CSEditorTools.FormatBytes(assetInfo.Size),
				assetIsTexture = assetType.BaseType == typeof(Texture),
				assetSettingsKind = assetInfo.SettingsKind,
				depth = depth
			};
			results.Add(element);

			var recursionId = CheckParentsForRecursion(element, results);

			if (recursionId > -1)
			{
				element.name += " [RECURSION]";
				element.recursionId = recursionId;
				return element;
			}

			if (assetInfo.referencedAtInfoList.Count > 0)
			{
				foreach (var referencedAtInfo in assetInfo.referencedAtInfoList)
				{
					if (CSEditorTools.IsValueMatchesAnyFilter(referencedAtInfo.assetInfo.AssetPath, MaintainerSettings.References.pathIgnoresFilters)) continue;

					var childElement = BuildTreeBranchRecursive(referencedAtInfo.assetInfo, depth + 1, id, results);
					if (childElement == null) continue;

					var referencedAtType = referencedAtInfo.assetInfo.Type;

					if (referencedAtType == typeof(GameObject) || referencedAtType == typeof(SceneAsset) || referencedAtType == typeof(MonoBehaviour) || referencedAtType == typeof(MonoScript))
					{
						if (referencedAtInfo.entries != null)
						{
							childElement.referencingEntries = referencedAtInfo.entries;
						}
						else
						{
							var collectedData = conjunctionInfoList.FirstOrDefault(d => d.asset == referencedAtInfo.assetInfo);

							if (collectedData == null)
							{
								collectedData = new AssetConjunctionInfo();
								conjunctionInfoList.Add(collectedData);
								collectedData.asset = referencedAtInfo.assetInfo;
							}

							var tc = collectedData.conjunctions.FirstOrDefault(c => c.referencedAsset == assetInfo);

							if (tc == null)
							{
								tc = new TreeConjunction
								{
									referencedAsset = assetInfo,
									referencedAtInfo = referencedAtInfo
								};

								Object[] referencedObjectsCandidates;

								if (assetType == typeof(GameObject) ||
									assetType == typeof(Font) ||
									assetType == typeof(Texture2D) ||
#if !UNITY_2018_1_OR_NEWER
									assetType == typeof(SubstanceArchive) ||
#endif
									assetType == typeof(DefaultAsset) && assetPath.EndsWith(".dll") ||
									assetTypeName == "AudioMixerController")
								{
									referencedObjectsCandidates = AssetDatabase.LoadAllAssetsAtPath(assetInfo.AssetPath);
								}
								else
								{
									referencedObjectsCandidates = new[] { AssetDatabase.LoadAssetAtPath<Object>(assetInfo.AssetPath) };
								}

								tc.referencedObjectsCandidates = referencedObjectsCandidates;
								collectedData.conjunctions.Add(tc);
							}
							tc.treeElements.Add(childElement);
						}
					}
				}
			}

			return element;
		}

		private static int CheckParentsForRecursion(ReferencesTreeElement item, List<ReferencesTreeElement> items)
		{
			var result = -1;

			var lastDepth = item.depth;
			for (var i = items.Count - 1; i >= 0; i--)
			{
				var previousItem = items[i];
				if (previousItem.depth >= lastDepth) continue;

				lastDepth = previousItem.depth;
				if (item.assetPath != previousItem.assetPath) continue;

				result = previousItem.id;
				break;
			}

			return result;
		}

		private static bool FillReferenceEntries()
		{
			var canceled = false;

			var count = conjunctionInfoList.Count;
			var updateStep = Math.Max(count / MaintainerSettings.UpdateProgressStep, 1);

			for (var i = 0; i < count; i++)
			{

				if ((i < 10 || i % updateStep == 0) && EditorUtility.DisplayCancelableProgressBar(
					    string.Format(ProgressCaption, 2, PhasesCount), string.Format(ProgressText, "Filling reference details", i + 1, count),
						(float)i / count))
				{
					canceled = true;
					break;
				}

				var item = conjunctionInfoList[i];

				if (item.asset.Type == typeof(GameObject))
				{
					ProcessPrefab(item);
				}
				else if (item.asset.Type == typeof(SceneAsset))
				{
					ProcessScene(item);
				}
				else if (item.asset.Type == typeof(MonoBehaviour) || item.asset.Type == typeof(MonoScript))
				{
					ProcessMonoBehaviour(item);
				}

				foreach (var conjunction in item.conjunctions)
				{
					var referencedAtInfo = conjunction.referencedAtInfo;

					if (referencedAtInfo.entries == null || referencedAtInfo.entries.Length == 0)
					{
						var newEntry = new ReferencingEntryData
						{
							location = Location.NotFound,
							label = "Could be referenced at the missing script/prefab, was incorrectly treated by Unity or this kind of referencing is not supported yet."
						};

						if (referencedAtInfo.assetInfo.Type == typeof(SceneAsset))
						{
							var sceneSpecificEntry = new ReferencingEntryData
							{
								location = Location.NotFound,
								label = "Please try to remove all missing prefabs/scripts (if any) and re-save scene, it may cleanup junky dependencies."
							};

							referencedAtInfo.entries = new[] { newEntry, sceneSpecificEntry };
						}
						else if (referencedAtInfo.assetInfo.Type == typeof(GameObject))
						{
							var prefabSpecificEntry = new ReferencingEntryData
							{
								location = Location.NotFound,
								label = "Please try to re-Apply prefab explicitly, this may clean up junky dependencies."
							};

							referencedAtInfo.entries = new[] { newEntry, prefabSpecificEntry };
						}
						else
						{
							referencedAtInfo.entries = new[] { newEntry };
						}

						if (debugMode)
						{
							Debug.LogWarning(Maintainer.ConstructWarning("Couldn't determine where exactly this asset is referenced: " + conjunction.referencedAsset.AssetPath, ModuleName));
						}
					}

					foreach (var targetTreeElement in conjunction.treeElements)
					{
						targetTreeElement.referencingEntries = referencedAtInfo.entries;
					}
				}
			}

			return canceled;
		}

		private static void ProcessPrefab(AssetConjunctionInfo conjunctionInfo)
		{
			var path = conjunctionInfo.asset.AssetPath;
			var allObjectsInPrefab = AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (var objectOnPrefab in allObjectsInPrefab)
			{
				if (objectOnPrefab == null) continue;

				CachedObjectData objectOnPrefabCachedData = null;

				if (AssetDatabase.IsMainAsset(objectOnPrefab))
				{
					var gameObjectOnPrefab = objectOnPrefab as GameObject;

					if (gameObjectOnPrefab != null)
					{
						TraverseObjectRecursive(conjunctionInfo, gameObjectOnPrefab, true);

						foreach (var conjunction in conjunctionInfo.conjunctions)
						{
							conjunction.traverseDeeper = true;
							conjunction.traverseInvisiblePrefabModificationsDeeper = true;
						}

						// specific cases handling for main asset -----------------------------------------------------

						var importSettings = AssetImporter.GetAtPath(path) as ModelImporter;
						if (importSettings == null) continue;

						var settings = new EntryAddSettings { postfix = "| Model Importer: RIG > Source" };
						TryAddEntryToMatchedConjunctions(conjunctionInfo.conjunctions, gameObjectOnPrefab, importSettings.sourceAvatar, settings, ref objectOnPrefabCachedData);

						for (var i = 0; i < importSettings.clipAnimations.Length; i++)
						{
							var clipAnimation = importSettings.clipAnimations[i];
							settings.postfix = "| Model Importer: Animations [" + clipAnimation.name + "] > Mask";
							TryAddEntryToMatchedConjunctions(conjunctionInfo.conjunctions, gameObjectOnPrefab, clipAnimation.maskSource, settings, ref objectOnPrefabCachedData);
						}
					}
					else
					{
						Debug.LogError(Maintainer.ConstructError("Main prefab asset is not a GameObject!"));
					}
				}
				else
				{
					// specific cases handling ------------------------------------------------------------------------

					
					if (objectOnPrefab is BillboardAsset)
					{
						var billboardAsset = objectOnPrefab as BillboardAsset;
						for (var i = conjunctionInfo.conjunctions.Count - 1; i >= 0; i--)
						{
							var conjunction = conjunctionInfo.conjunctions[i];
							if (!conjunction.traverseDeeper) continue;

							var settings = new EntryAddSettings { postfix = "| BillboardAsset: Material" };
							TryAddEntryToMatchedConjunctions(conjunctionInfo.conjunctions, billboardAsset, billboardAsset.material, settings, ref objectOnPrefabCachedData);
						}
					}
					else if (objectOnPrefab is TreeData)
					{
						CachedObjectData objectInAssetCachedData = null;
						InspectComponent(conjunctionInfo.conjunctions, objectOnPrefab, objectOnPrefab, -1, true, ref objectInAssetCachedData);
					}
				}
			}
		}

		private static void ProcessScene(AssetConjunctionInfo conjunctionInfo)
		{
			var path = conjunctionInfo.asset.AssetPath;
			var openSceneResult = CSSceneTools.OpenScene(path);

			if (!openSceneResult.success)
			{
				Debug.LogWarning(Maintainer.ConstructWarning("Can't open scene " + path));
				return;
			}

			ProcessSceneSettings(conjunctionInfo);

			var rootObjects = openSceneResult.scene.GetRootGameObjects();
			foreach (var rootObject in rootObjects)
			{
				TraverseObjectRecursive(conjunctionInfo, rootObject, false);

				foreach (var conjunction in conjunctionInfo.conjunctions)
				{
					conjunction.traverseDeeper = true;
					conjunction.traverseInvisiblePrefabModificationsDeeper = true;
				}
			}

			CSSceneTools.CloseOpenedSceneIfNeeded(openSceneResult);
		}

		private static void ProcessSceneSettings(AssetConjunctionInfo conjunctionInfo)
		{
			Object lightmapSettings = null;
			SerializedObject lightmapSettingsSo = null;
			SerializedProperty lightmapParametersField = null;

#if UNITY_5_5_OR_NEWER
			Object renderSettings = null;
			SerializedObject renderSettingsSo = null;
			SerializedProperty renderHaloField = null;
			SerializedProperty renderSpotField = null;
#endif

#if UNITY_5_6_OR_NEWER
			SerializedObject navMeshSettingsSo = null;
			SerializedProperty navMeshDataField = null;
#endif

			foreach (var conjunction in conjunctionInfo.conjunctions)
			{
				if (conjunction.referencedObjectsCandidates.Length == 1)
				{
					var referencedObjectCandidate = conjunction.referencedObjectsCandidates[0];

					if (referencedObjectCandidate is LightingDataAsset)
					{
						if (Lightmapping.lightingDataAsset == referencedObjectCandidate)
						{
							var entry = new ReferencingEntryData
							{
								location = Location.LightingSettings,
								label = "Lighting settings (Global maps tab)"
							};

							conjunction.referencedAtInfo.AddNewEntry(entry);
						}
					}
					else if (referencedObjectCandidate is Material)
					{
						if (RenderSettings.skybox == referencedObjectCandidate)
						{
							var entry = new ReferencingEntryData
							{
								location = Location.LightingSettings,
								label = "Lighting settings (Scene tab > Environment > Skybox Material)"
							};

							conjunction.referencedAtInfo.AddNewEntry(entry);
						}
					}
					else if (referencedObjectCandidate is LightmapParameters)
					{
						lightmapSettings = lightmapSettings ?? CSEditorTools.GetLightmapSettings();
						if (lightmapSettings != null)
						{
							lightmapSettingsSo = lightmapSettingsSo ?? new SerializedObject(lightmapSettings);
							lightmapParametersField = lightmapParametersField ?? lightmapSettingsSo.FindProperty("m_LightmapEditorSettings.m_LightmapParameters");
							if (lightmapParametersField != null && lightmapParametersField.propertyType == SerializedPropertyType.ObjectReference)
							{
								if (lightmapParametersField.objectReferenceValue == referencedObjectCandidate)
								{
									var entry = new ReferencingEntryData
									{
										location = Location.LightingSettings,
										label = "Lighting settings (Scene tab > Lightmapping Settings > Lightmap Parameters)"
									};

									conjunction.referencedAtInfo.AddNewEntry(entry);
								}
							}
							else
							{
								Debug.LogError(Maintainer.ConstructError("Can't find m_LightmapParameters at the LightmapSettings!"));
							}
						}
					}
					else if (referencedObjectCandidate is Cubemap)
					{
						if (RenderSettings.customReflection == referencedObjectCandidate)
						{
							var entry = new ReferencingEntryData { location = Location.LightingSettings };

							if (RenderSettings.defaultReflectionMode == DefaultReflectionMode.Custom)
							{
								entry.label = "Lighting settings (Scene tab > Environment > Cubemap)";
							}
							else
							{
								entry.label = "Lighting settings (Scene tab > Environment > Cubemap), set Reflections > Source > Custom to see";
							}
							conjunction.referencedAtInfo.AddNewEntry(entry);
						}
					}
#if UNITY_5_6_OR_NEWER
					else if (referencedObjectCandidate is NavMeshData)
					{
						navMeshSettingsSo = navMeshSettingsSo ?? new SerializedObject(UnityEditor.AI.NavMeshBuilder.navMeshSettingsObject);
						navMeshDataField = navMeshDataField ?? navMeshSettingsSo.FindProperty("m_NavMeshData");
						if (navMeshDataField != null && navMeshDataField.propertyType == SerializedPropertyType.ObjectReference)
						{
							if (navMeshDataField.objectReferenceValue == referencedObjectCandidate)
							{
								var entry = new ReferencingEntryData
								{
									location = Location.Navigation,
									label = "Navigation settings of scene"
								};

								conjunction.referencedAtInfo.AddNewEntry(entry);
							}
						}
						else
						{
							Debug.LogError(Maintainer.ConstructError("Can't find m_NavMeshData at the navMeshSettingsObject!"));
						}
					}
#endif
				}
#if UNITY_5_5_OR_NEWER
				else
				{
					foreach (var candidate in conjunction.referencedObjectsCandidates)
					{
						var light = candidate as Light;
						if (light != null && light == RenderSettings.sun)
						{
							var entry = new ReferencingEntryData
							{
								location = Location.LightingSettings,
								label = "Lighting settings (Scene tab > Environment > Sun Source)"
							};

							conjunction.referencedAtInfo.AddNewEntry(entry);
							break;
						}

						var texture2D = candidate as Texture2D;
						if (texture2D != null)
						{
							renderSettings = renderSettings ?? CSEditorTools.GetRenderSettings();
							if (renderSettings != null)
							{
								renderSettingsSo = renderSettingsSo ?? new SerializedObject(renderSettings);
								renderHaloField = renderHaloField ?? renderSettingsSo.FindProperty("m_HaloTexture");
								if (renderHaloField != null && renderHaloField.propertyType == SerializedPropertyType.ObjectReference)
								{
									if (renderHaloField.objectReferenceValue == texture2D)
									{
										var entry = new ReferencingEntryData
										{
											location = Location.LightingSettings,
											label = "Lighting settings (Scene tab > Other Settings > Halo Texture)"
										};

										conjunction.referencedAtInfo.AddNewEntry(entry);
									}
								}
								else
								{
									Debug.LogError(Maintainer.ConstructError("Can't find m_HaloTexture at the RenderSettings!"));
								}

								renderSpotField = renderSpotField ?? renderSettingsSo.FindProperty("m_SpotCookie");
								if (renderSpotField != null && renderSpotField.propertyType == SerializedPropertyType.ObjectReference)
								{
									if (renderSpotField.objectReferenceValue == texture2D)
									{
										var entry = new ReferencingEntryData
										{
											location = Location.LightingSettings,
											label = "Lighting settings (Scene tab > Other Settings > Spot Cookie)"
										};

										conjunction.referencedAtInfo.AddNewEntry(entry);
									}
								}
								else
								{
									Debug.LogError(Maintainer.ConstructError("Can't find m_SpotCookie at the RenderSettings!"));
								}

								/*var iterator = renderSettingsSo.GetIterator();
								while (iterator.Next(true))
								{
									if (iterator.propertyType == SerializedPropertyType.ObjectReference)
									{
										Debug.Log(iterator.propertyPath + " [" + iterator.objectReferenceValue + "]");
									}
								}*/
							}
						}
					}
				}
#endif
			}
		}

		private static void ProcessMonoBehaviour(AssetConjunctionInfo conjunctionInfo)
		{
			var path = conjunctionInfo.asset.AssetPath;
			var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);

			if (mainAsset == null) return;

			CachedObjectData objectInAssetCachedData = null;
			InspectComponent(conjunctionInfo.conjunctions, mainAsset, mainAsset, -1, true, ref objectInAssetCachedData);

			foreach (var conjunction in conjunctionInfo.conjunctions)
			{
				conjunction.traverseDeeper = true;
				conjunction.traverseInvisiblePrefabModificationsDeeper = true;
			}
		}

		private static void TraverseObjectRecursive(AssetConjunctionInfo conjunctionInfo, GameObject parentObject, bool objectFromPrefab)
		{
			CachedObjectData cachedData = null;
			if (!InspectObject(conjunctionInfo, parentObject, objectFromPrefab, ref cachedData)) return;

			var parentTransform = parentObject.transform;

			for (var i = 0; i < parentTransform.childCount; i++)
			{
				var childTransform = parentTransform.GetChild(i);
				TraverseObjectRecursive(conjunctionInfo, childTransform.gameObject, objectFromPrefab);
			}
		}

		private static bool InspectObject(AssetConjunctionInfo conjunctionInfo, GameObject inspectedUnityObject, bool objectFromPrefab, ref CachedObjectData cachedData)
		{
			var unmodifiedPrefabInstance = false;
			Component[] components = null;

			var conjunctions = conjunctionInfo.conjunctions;

			/*if (inspectedUnityObject.name.Contains("Cube Instance"))
			{
				Debug.Log("Reserved for manual conditional breakpoint =D");
			}*/

			if (!objectFromPrefab)
			{
				// ----------------------------------------------------------------------------
				// checking stuff related to the prefabs in scenes
				// ----------------------------------------------------------------------------

				var prefabType = PrefabUtility.GetPrefabType(inspectedUnityObject);
				if (prefabType != PrefabType.None)
				{
					// detect if we inspect prefab which we are looking references for
					// in such case we only need its root and we should cancel further traverse

					var prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(inspectedUnityObject);

					var addSettings = new EntryAddSettings
					{
						traverseDeeper = false,
					};
					TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, prefabParent, addSettings, ref cachedData);

					// detect not modified prefab instances so we will have no duplicates at 
					// the search results from the prefab itself and from scene where instance is referenced

					var skipThisPrefabInstance = CheckPrefabModifications(conjunctions, inspectedUnityObject, prefabParent, ref components, ref cachedData);

					if (prefabType != PrefabType.DisconnectedPrefabInstance &&
						prefabType != PrefabType.DisconnectedModelPrefabInstance &&
						prefabType != PrefabType.MissingPrefabInstance)
					{
						if (skipThisPrefabInstance)
						{
							var parentObject = prefabParent as GameObject;
							if (parentObject != null)
							{
								components = components ?? inspectedUnityObject.GetComponents<Component>();
								var prefabComponents = parentObject.GetComponents<Component>();
								if (components.Length > prefabComponents.Length)
								{
									skipThisPrefabInstance = false;
								}
								else
								{
									for (var i = 0; i < components.Length; i++)
									{
										var component = components[i];
										var prefabComponent = prefabComponents[i];

										if (component == null || prefabComponent == null) continue;
										if (component.GetType() == prefabComponent.GetType()) continue;

										skipThisPrefabInstance = false;
										break;
									}
								}
							}
						}

						unmodifiedPrefabInstance = skipThisPrefabInstance;
					}
				}
			}
			else
			{
				CheckPrefabModifications(conjunctions, inspectedUnityObject, inspectedUnityObject, ref components, ref cachedData);
			}

			var traverseDeeper = conjunctions.Any(c => c.traverseDeeper);

			if (traverseDeeper && !unmodifiedPrefabInstance)
			{
				var thumbnail = AssetPreview.GetMiniThumbnail(inspectedUnityObject);

				if (thumbnail != null && thumbnail.hideFlags != HideFlags.HideAndDontSave)
				{
					var addSettings = new EntryAddSettings
					{
						prefix = "[Object Icon]",
					};
					TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, thumbnail, addSettings, ref cachedData);
				}

				components = components ?? inspectedUnityObject.GetComponents<Component>();
				for (var i = 0; i < components.Length; i++)
				{
					var component = components[i];
					if (component == null) continue;
					if (component is Transform) continue;

					InspectComponent(conjunctions, inspectedUnityObject, component, i, objectFromPrefab, ref cachedData);
				}
			}

			return traverseDeeper;
		}

		private static void InspectComponent(List<TreeConjunction> conjunctions, Object inspectedUnityObject, Object script, int index, bool objectFromPrefab, ref CachedObjectData cachedData)
		{
			CachedComponentData cachedComponentData = null;

			var so = new SerializedObject(script);
			var sp = so.GetIterator();

			var lastScriptPropertyName = string.Empty;

			while (sp.Next(true))
			{
				//if (index == -1) Debug.Log(sp.propertyPath);

				if (sp.isArray)
				{
					if (sp.type == "string")
					{
						if (index == -1 && script is TextAsset)
						{
							if (sp.propertyPath.IndexOf("m_DefaultReferences.Array.data[", StringComparison.Ordinal) == 0)
							{
								lastScriptPropertyName = ObjectNames.NicifyVariableName(sp.stringValue);

								// skipping first pair item of the m_DefaultReferences array item
								sp.Next(false);
							}
						}
					}
				}

				if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue != null)
				{
					string propertyName = null;

					if (index == -1 && !string.IsNullOrEmpty(lastScriptPropertyName))
					{
						propertyName = lastScriptPropertyName;
						lastScriptPropertyName = string.Empty;
					}
					else
					{
						var fullPropertyPath = sp.propertyPath;
						var isArrayItem = fullPropertyPath.IndexOf(']') != -1;
						propertyName = isArrayItem ? CSEditorTools.NicifyName(CSObjectTools.GetArrayItemNameAndIndex(fullPropertyPath)) : sp.displayName;
					}

					var addSettings = new EntryAddSettings
					{
						script = script,
						componentIndex = index,
						propertyName = propertyName,
						objectFromPrefab = objectFromPrefab
					};

					TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, sp.objectReferenceValue, addSettings, ref cachedData, ref cachedComponentData);

					var material = sp.objectReferenceValue as Material;
					if (material == null) continue;

					if (objectFromPrefab)
					{
						if (AssetDatabase.GetAssetPath(material) != AssetDatabase.GetAssetPath(script)) continue;
					}
					else
					{
						if (AssetDatabase.Contains(material)) continue;
					}

					addSettings = new EntryAddSettings
					{
						prefix = "[Material Instance]",
						postfix = " (Main Texture)",
						script = script,
						componentIndex = index,
						propertyName = propertyName
					};
					TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, material.mainTexture, addSettings, ref cachedData, ref cachedComponentData);

					addSettings.postfix = " (Shader)";
					TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, material.shader, addSettings, ref cachedData, ref cachedComponentData);

					var materialSo = new SerializedObject(material);

					var texEnvs = materialSo.FindProperty("m_SavedProperties.m_TexEnvs.Array");
					if (texEnvs != null)
					{
						for (var k = 0; k < texEnvs.arraySize; k++)
						{
							var arrayItem = texEnvs.GetArrayElementAtIndex(k);
							var fieldName = arrayItem.displayName;
							if (fieldName == "_MainTex") continue;

							var textureProperty = arrayItem.FindPropertyRelative("second.m_Texture");
							if (textureProperty != null)
							{
								if (textureProperty.propertyType == SerializedPropertyType.ObjectReference)
								{
									addSettings.postfix = " (" + fieldName + ")";
									TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, textureProperty.objectReferenceValue, addSettings, ref cachedData, ref cachedComponentData);
								}
							}
							else
							{
								Debug.LogError(Maintainer.ConstructError("Can't get second.m_Texture from texEnvs at " + inspectedUnityObject.name));
							}
						}
					}
					else
					{
						Debug.LogError(Maintainer.ConstructError("Can't get m_SavedProperties.m_TexEnvs.Array from material instance at " + inspectedUnityObject.name));
					}
				}

				lastScriptPropertyName = string.Empty;
			}
		}

		private static bool CheckPrefabModifications(List<TreeConjunction> conjunctions, GameObject inspectedUnityObject, Object prefabParent, ref Component[] components, ref CachedObjectData cachedData)
		{
			var skipThisPrefabInstance = true;

			var modifications = PrefabUtility.GetPropertyModifications(inspectedUnityObject);
			if (modifications == null) return true;

			foreach (var modification in modifications)
			{
				if (modification.objectReference == null) continue;

				if (modification.target != null)
				{
					GameObject modificationHolder = null;

					if (modification.target is GameObject)
					{
						modificationHolder = (GameObject) modification.target;
					}
					else if (modification.target is Component)
					{
						modificationHolder = ((Component)modification.target).gameObject;
					}

					skipThisPrefabInstance = modificationHolder != prefabParent;

					var targetComponent = modification.target as Component;
					if (targetComponent == null) continue;

					components = components ?? inspectedUnityObject.GetComponents<Component>();
					var targetComponentTypeName = targetComponent.GetType().Name;
					var match = false;

					foreach (var component in components)
					{
						if (component == null) continue;
						if (component.GetType().Name != targetComponentTypeName) continue;

						match = true;
						break;
					}

					if (!match)
					{
						var addSettings = new EntryAddSettings
						{
							prefix = "[Invisible Prefab Modification]",
							traverseInvisiblePrefabModificationsDeeper = false
						};
						TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, modification.objectReference, addSettings, ref cachedData);
					}
				}
				else
				{
					var addSettings = new EntryAddSettings
					{
						prefix = "[Junky Prefab Modification]",
						traverseInvisiblePrefabModificationsDeeper = false
					};
					TryAddEntryToMatchedConjunctions(conjunctions, inspectedUnityObject, modification.objectReference, addSettings, ref cachedData);
				}
			}

			return skipThisPrefabInstance;
		}

		private static void TryAddEntryToMatchedConjunctions(IList<TreeConjunction> conjunctions, Object lookAt, Object lookFor, EntryAddSettings settings, ref CachedObjectData data)
		{
			CachedComponentData componentData = null;
			TryAddEntryToMatchedConjunctions(conjunctions, lookAt, lookFor, settings, ref data, ref componentData);
		}

		private static void TryAddEntryToMatchedConjunctions(IList<TreeConjunction> conjunctions, Object lookAt, Object lookFor, EntryAddSettings settings, ref CachedObjectData data, ref CachedComponentData componentData)
		{
			var lookAtGameObject = lookAt as GameObject;
			string label = null;
			string transformPath = null;
			string componentAndPropertyPath = null;

			var location = settings.location;

			if (data == null) data = new CachedObjectData();

			for (var i = 0; i < conjunctions.Count; i++)
			{
				var conjunction = conjunctions[i];
				if (!conjunction.traverseDeeper) continue;
				if (!conjunction.traverseInvisiblePrefabModificationsDeeper) continue;

				var match = false;
				for (var j = 0; j < conjunction.referencedObjectsCandidates.Length; j++)
				{
					if (conjunction.referencedObjectsCandidates[j] != lookFor) continue;

					match = true;
					break;
				}

				if (!match) continue;

				data.objectId = data.objectId == 0 ? CSObjectTools.GetUniqueObjectId(lookAt) : data.objectId;

				if (transformPath == null)
				{
					transformPath = data.transformPath = data.transformPath ?? (lookAtGameObject != null ? CSEditorTools.GetFullTransformPath(lookAtGameObject.transform) : lookAt.name);

					if (settings.objectFromPrefab)
					{
						if (data.transformPath.IndexOf('/') == -1)
						{
							transformPath = string.Empty;
						}
					}
				}

				if (componentAndPropertyPath == null)
				{
					if (settings.script != null)
					{
						if (componentData == null) componentData = new CachedComponentData();

						componentAndPropertyPath = componentData.componentTypeName = componentData.componentTypeName ?? settings.script.GetType().Name;

						if (settings.propertyName != null)
						{
							componentAndPropertyPath += ": " + settings.propertyName;
						}
					}
					else
					{
						componentAndPropertyPath = string.Empty;
					}
				}

				if (label == null)
				{
					if (settings.label != null)
					{
						label = settings.label;
					}
					else
					{
						label = transformPath;

						if (!string.IsNullOrEmpty(componentAndPropertyPath))
						{
							if (!string.IsNullOrEmpty(transformPath))
							{
								label += " | ";
							}
							label += componentAndPropertyPath;
						}

						if (settings.prefix != null) label = settings.prefix + " " + label;
						if (settings.postfix != null) label = label + " " + settings.postfix;
					}
				}

				var newEntry = new ReferencingEntryData
				{
					location = location,
					objectId = data.objectId,
					transformPath = data.transformPath,
					label = label,
					componentId = settings.componentIndex,
					propertyName = settings.propertyName
				};

				conjunction.referencedAtInfo.AddNewEntry(newEntry);
				conjunction.traverseDeeper = settings.traverseDeeper;
				conjunction.traverseInvisiblePrefabModificationsDeeper = settings.traverseInvisiblePrefabModificationsDeeper;
			}
		}

		private class EntryAddSettings
		{
			public string prefix = null;
			public string postfix = null;
			public bool objectFromPrefab = false;
			public bool traverseDeeper = true;
			public bool traverseInvisiblePrefabModificationsDeeper = true;

			public Location location = Location.GameObject;
			public string propertyName = null;
			public string label = null;

			public Object script = null;
			public int componentIndex = -1;
		}

		private class CachedObjectData
		{
			public long objectId = 0L;
			public string transformPath = null;
		}

		private class CachedComponentData
		{
			public string componentTypeName = null;
		}
	}
}