using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace VolumetricFogAndMist {
	[CustomEditor(typeof(VolumetricFog))]
	public class VolumetricFogInspector : Editor {

		const string PRAGMA_COMMENT_MARK = "// Edited by Shader Control: ";
		const string PRAGMA_DISABLED_MARK = "// Disabled by Shader Control: ";
		const string PRAGMA_MULTICOMPILE = "#pragma multi_compile ";
		const string PRAGMA_UNDERSCORE = "__ ";
		const string SCATTERING_ON = "Light Scattering (ON)";
		const string SCATTERING_OFF = "Light Scattering";
		const string SUN_SHADOWS_ON = "Sun Shadows (ON)";
		const string SUN_SHADOWS_OFF = "Sun Shadows";
		const string FOG_BLUR_ON = "Depth Blur (ON)";
		const string FOG_BLUR_OFF = "Depth Blur";
		const string FOW_ON = "Fog Of War (ON)";
		const string FOW_OFF = "Fog Of War";
		const string FOG_VOID_ON = "Fog Void (ON)";
		const string FOG_VOID_OFF = "Fog Void";
		const string FOG_SKY_HAZE_ON = "Sky Haze (ON)";
		const string FOG_SKY_HAZE_OFF = "Sky Haze";
		const string FOG_AREA_ON = "Fog Area (ON)";
		const string FOG_AREA_OFF = "Fog Area";
		const string FOG_MASK_ON = "Geometry Mask (ON)";
		const string FOG_MASK_OFF = "Geometry Mask";
		VolumetricFog _fog;
		static GUIStyle sectionHeaderStyle;
		static GUIStyle buttonNormalStyle, buttonPressedStyle;
		Color titleColor;
		bool expandFogGeometrySection, expandFogColorsSection, expandFogAnimationSection, expandSkySection, expandSunShaftsSection, expandSunShadowsSection, expandFogBlurSection, expandFogOfWarSection;
		bool expandFogVoidSection, expandFogAreaSection, expandOptionalPointLightSection, expandOptimizationSettingsSection, expandFogMaskSection;
		bool toggleOptimizeBuild;
		List<VolumetricFogSInfo> shaders;
		bool profileChanged, enableProfileApply, presetChanged;
		GUIContent[] computeDepthOptions;
		int[] computeDepthValues;
		VolumetricFogShaderOptions shaderAdvancedOptionsInfo;

		SerializedProperty fogRenderer, _preset, _profile, _sun, _useFogVolumes;
		SerializedProperty _computeDepth, _transparencyLayerMask, _computeDepthScope, _transparencyCutOff;
		SerializedProperty _renderBeforeTransparent, _transparencyBlendMode, _transparencyBlendPower;
		SerializedProperty _enableMultipleCameras;
		SerializedProperty _density, _noiseStrength, _noiseScale, _noiseSparse, _noiseFinalMultiplier, _distance, _distanceFallOff, _maxFogLength, _maxFogLengthFallOff, _height, _useXYPlane, _baselineHeight, _baselineRelativeToCamera, _baselineRelativeToCameraDelay;
		SerializedProperty _alpha, _color, _specularColor, _specularThreshold, _specularIntensity;
		SerializedProperty _lightDirection, _lightingModel, _lightIntensity, _lightColor, _sunCopyColor;
		SerializedProperty _speed, _windDirection, _turbulenceStrength, _useRealTime;
		SerializedProperty _skyHaze, _skyColor, _skySpeed, _skyNoiseStrength, _skyAlpha, _skyDepth;
		SerializedProperty _lightScatteringEnabled, _lightScatteringDiffusion, _lightScatteringExposure, _lightScatteringSpread, _lightScatteringWeight, _lightScatteringIllumination, _lightScatteringDecay, _lightScatteringSamples, _lightScatteringJittering;
		SerializedProperty _sunShadows, _sunShadowsLayerMask, _sunShadowsStrength, _sunShadowsResolution, _sunShadowsMaxDistance, _sunShadowsCancellation, _sunShadowsBakeMode, _sunShadowsRefreshInterval, _sunShadowsBias, _sunShadowsJitterStrength;
		SerializedProperty _fogBlur, _fogBlurDepth;
        SerializedProperty _pointLightTrackingAuto, _pointLightTrackingCheckInterval, _pointLightTrackingNewLightsCheckInterval, _pointLightTrackingPivot, _pointLightTrackingCount, _pointLightInscattering, _pointLightIntensity, _pointLightInsideAtten, _pointLightParams;
		SerializedProperty _fogOfWarEnabled, _fogOfWarCenter, _fogOfWarSize, _fogOfWarTextureSize, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration;
		SerializedProperty _fogVoidPosition, _character, _fogVoidTopology, _fogVoidRadius, _fogVoidFallOff, _fogVoidHeight, _fogVoidDepth;
		SerializedProperty _fogAreaPosition, _fogAreaCenter, _fogAreaFollowMode, _fogAreaTopology, _fogAreaRadius, _fogAreaHeight, _fogAreaDepth, _fogAreaFallOff, _fogAreaSortingMode, _fogAreaRenderOrder, _fogAreaShowGizmos;
        SerializedProperty _downsampling, _forceComposition, _edgeImprove, _edgeThreshold, _stepping, _steppingNear, _dithering, _ditherStrength, _jitterStrength, _spsrBehaviour, _useSinglePassStereoRenderingMatrix, _updateTextureSpread, _timeBetweenTextureUpdates;
		SerializedProperty _enableMask, _maskLayer, _maskDownsampling;
		string fogMaskTestResult;
		StringBuilder fogMaskTestResultSB;
        int pointLightInspectorCount;

		void OnEnable() {
			titleColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.12f, 0.16f, 0.4f);
			_fog = (VolumetricFog)target;

			fogRenderer = serializedObject.FindProperty("fogRenderer");
			_preset = serializedObject.FindProperty("_preset");
			_profile = serializedObject.FindProperty("_profile");
			_sun = serializedObject.FindProperty("_sun");
			_useFogVolumes = serializedObject.FindProperty("_useFogVolumes");
			_computeDepth = serializedObject.FindProperty("_computeDepth");
			_transparencyLayerMask = serializedObject.FindProperty("_transparencyLayerMask");
			_computeDepthScope = serializedObject.FindProperty("_computeDepthScope");
			_transparencyCutOff = serializedObject.FindProperty("_transparencyCutOff");
			_renderBeforeTransparent = serializedObject.FindProperty("_renderBeforeTransparent");
			_transparencyBlendMode = serializedObject.FindProperty("_transparencyBlendMode");
			_transparencyBlendPower = serializedObject.FindProperty("_transparencyBlendPower");
			_enableMultipleCameras = serializedObject.FindProperty("_enableMultipleCameras");

			_density = serializedObject.FindProperty("_density");
			_noiseStrength = serializedObject.FindProperty("_noiseStrength");
			_noiseScale = serializedObject.FindProperty("_noiseScale");
			_noiseSparse = serializedObject.FindProperty("_noiseSparse");
			_noiseFinalMultiplier = serializedObject.FindProperty("_noiseFinalMultiplier");
			_distance = serializedObject.FindProperty("_distance");
			_distanceFallOff = serializedObject.FindProperty("_distanceFallOff");
			_maxFogLength = serializedObject.FindProperty("_maxFogLength");
			_maxFogLengthFallOff = serializedObject.FindProperty("_maxFogLengthFallOff");
			_height = serializedObject.FindProperty("_height");
			_useXYPlane = serializedObject.FindProperty("_useXYPlane");
			_baselineHeight = serializedObject.FindProperty("_baselineHeight");
			_baselineRelativeToCamera = serializedObject.FindProperty("_baselineRelativeToCamera");
			_baselineRelativeToCameraDelay = serializedObject.FindProperty("_baselineRelativeToCameraDelay");

			_alpha = serializedObject.FindProperty("_alpha");
			_color = serializedObject.FindProperty("_color");
			_specularColor = serializedObject.FindProperty("_specularColor");
			_specularThreshold = serializedObject.FindProperty("_specularThreshold");
			_specularIntensity = serializedObject.FindProperty("_specularIntensity");
			_lightDirection = serializedObject.FindProperty("_lightDirection");
			_lightingModel = serializedObject.FindProperty("_lightingModel");
			_lightIntensity = serializedObject.FindProperty("_lightIntensity");
			_lightColor = serializedObject.FindProperty("_lightColor");
			_sunCopyColor = serializedObject.FindProperty("_sunCopyColor");

			_speed = serializedObject.FindProperty("_speed");
			_windDirection = serializedObject.FindProperty("_windDirection");
			_turbulenceStrength = serializedObject.FindProperty("_turbulenceStrength");
			_useRealTime = serializedObject.FindProperty("_useRealTime");

			_skyHaze = serializedObject.FindProperty("_skyHaze");
			_skyColor = serializedObject.FindProperty("_skyColor");
			_skySpeed = serializedObject.FindProperty("_skySpeed");
			_skyNoiseStrength = serializedObject.FindProperty("_skyNoiseStrength");
			_skyAlpha = serializedObject.FindProperty("_skyAlpha");
			_skyDepth = serializedObject.FindProperty("_skyDepth");

			_lightScatteringEnabled = serializedObject.FindProperty("_lightScatteringEnabled");
			_lightScatteringDiffusion = serializedObject.FindProperty("_lightScatteringDiffusion");
			_lightScatteringExposure = serializedObject.FindProperty("_lightScatteringExposure");
			_lightScatteringSpread = serializedObject.FindProperty("_lightScatteringSpread");
			_lightScatteringWeight = serializedObject.FindProperty("_lightScatteringWeight");
			_lightScatteringIllumination = serializedObject.FindProperty("_lightScatteringIllumination");
			_lightScatteringDecay = serializedObject.FindProperty("_lightScatteringDecay");
			_lightScatteringSamples = serializedObject.FindProperty("_lightScatteringSamples");
			_lightScatteringJittering = serializedObject.FindProperty("_lightScatteringJittering");

			_sunShadows = serializedObject.FindProperty("_sunShadows");
			_sunShadowsLayerMask = serializedObject.FindProperty("_sunShadowsLayerMask");
			_sunShadowsStrength = serializedObject.FindProperty("_sunShadowsStrength");
			_sunShadowsResolution = serializedObject.FindProperty("_sunShadowsResolution");
			_sunShadowsMaxDistance = serializedObject.FindProperty("_sunShadowsMaxDistance");
			_sunShadowsCancellation = serializedObject.FindProperty("_sunShadowsCancellation");
			_sunShadowsBakeMode = serializedObject.FindProperty("_sunShadowsBakeMode");
			_sunShadowsRefreshInterval = serializedObject.FindProperty("_sunShadowsRefreshInterval");
			_sunShadowsBias = serializedObject.FindProperty("_sunShadowsBias");
			_sunShadowsJitterStrength = serializedObject.FindProperty("_sunShadowsJitterStrength");

			_fogBlur = serializedObject.FindProperty("_fogBlur");
			_fogBlurDepth = serializedObject.FindProperty("_fogBlurDepth");

			_pointLightTrackingAuto = serializedObject.FindProperty("_pointLightTrackingAuto");
			_pointLightTrackingCheckInterval = serializedObject.FindProperty("_pointLightTrackingCheckInterval");
            _pointLightTrackingNewLightsCheckInterval = serializedObject.FindProperty("_pointLightTrackingNewLightsCheckInterval");
			_pointLightTrackingPivot = serializedObject.FindProperty("_pointLightTrackingPivot");
			_pointLightTrackingCount = serializedObject.FindProperty("_pointLightTrackingCount");
			_pointLightInscattering = serializedObject.FindProperty("_pointLightInscattering");
			_pointLightIntensity = serializedObject.FindProperty("_pointLightIntensity");
            _pointLightInsideAtten = serializedObject.FindProperty("_pointLightInsideAtten");
            _pointLightParams = serializedObject.FindProperty("pointLightParams");

			_fogOfWarEnabled = serializedObject.FindProperty("_fogOfWarEnabled");
			_fogOfWarCenter = serializedObject.FindProperty("_fogOfWarCenter");
			_fogOfWarSize = serializedObject.FindProperty("_fogOfWarSize");
			_fogOfWarTextureSize = serializedObject.FindProperty("_fogOfWarTextureSize");
			_fogOfWarRestoreDelay = serializedObject.FindProperty("_fogOfWarRestoreDelay");
			_fogOfWarRestoreDuration = serializedObject.FindProperty("_fogOfWarRestoreDuration");

			_fogVoidPosition = serializedObject.FindProperty("_fogVoidPosition");
			_character = serializedObject.FindProperty("_character");
			_fogVoidTopology = serializedObject.FindProperty("_fogVoidTopology");
			_fogVoidRadius = serializedObject.FindProperty("_fogVoidRadius");
			_fogVoidFallOff = serializedObject.FindProperty("_fogVoidFallOff");
			_fogVoidHeight = serializedObject.FindProperty("_fogVoidHeight");
			_fogVoidDepth = serializedObject.FindProperty("_fogVoidDepth");

			_fogAreaPosition = serializedObject.FindProperty("_fogAreaPosition");
			_fogAreaCenter = serializedObject.FindProperty("_fogAreaCenter");
			_fogAreaFollowMode = serializedObject.FindProperty("_fogAreaFollowMode");
			_fogAreaTopology = serializedObject.FindProperty("_fogAreaTopology");
			_fogAreaRadius = serializedObject.FindProperty("_fogAreaRadius");
			_fogAreaHeight = serializedObject.FindProperty("_fogAreaHeight");
			_fogAreaDepth = serializedObject.FindProperty("_fogAreaDepth");
			_fogAreaFallOff = serializedObject.FindProperty("_fogAreaFallOff");
			_fogAreaSortingMode = serializedObject.FindProperty("_fogAreaSortingMode");
			_fogAreaRenderOrder = serializedObject.FindProperty("_fogAreaRenderOrder");
			_fogAreaShowGizmos = serializedObject.FindProperty("_fogAreaShowGizmos");

			_downsampling = serializedObject.FindProperty("_downsampling");
            _forceComposition = serializedObject.FindProperty("_forceComposition");
			_edgeImprove = serializedObject.FindProperty("_edgeImprove");
			_edgeThreshold = serializedObject.FindProperty("_edgeThreshold");
			_stepping = serializedObject.FindProperty("_stepping");
			_steppingNear = serializedObject.FindProperty("_steppingNear");
			_dithering = serializedObject.FindProperty("_dithering");
			_ditherStrength = serializedObject.FindProperty("_ditherStrength");
			_jitterStrength = serializedObject.FindProperty("_jitterStrength");
			_spsrBehaviour = serializedObject.FindProperty("_spsrBehaviour");
			_useSinglePassStereoRenderingMatrix = serializedObject.FindProperty("_useSinglePassStereoRenderingMatrix");
			_updateTextureSpread = serializedObject.FindProperty("_updateTextureSpread");
			_timeBetweenTextureUpdates = serializedObject.FindProperty("_timeBetweenTextureUpdates");

			_enableMask = serializedObject.FindProperty("_enableMask");
			_maskLayer = serializedObject.FindProperty("_maskLayer");
			_maskDownsampling = serializedObject.FindProperty("_maskDownsampling");

			computeDepthOptions = new GUIContent[3];
			computeDepthOptions[0] = new GUIContent("Only Tree Billboards");
			computeDepthOptions[1] = new GUIContent("Tree Billboards And Transparent Objects");
			computeDepthOptions[2] = new GUIContent("Everything In Layer");
			computeDepthValues = new int[3];
			computeDepthValues[0] = (int)COMPUTE_DEPTH_SCOPE.OnlyTreeBillboards;
			computeDepthValues[1] = (int)COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects;
			computeDepthValues[2] = (int)COMPUTE_DEPTH_SCOPE.EverythingInLayer;
			expandFogGeometrySection = EditorPrefs.GetBool("expandFogGeometrySection", false);
			expandFogColorsSection = EditorPrefs.GetBool("expandFogColorsSection", false);
			expandFogAnimationSection = EditorPrefs.GetBool("expandFogAnimationSection", false);
			expandSkySection = EditorPrefs.GetBool("expandSkySection", false);
			expandOptionalPointLightSection = EditorPrefs.GetBool("expandOptionalPointLightSection", false);
			expandFogOfWarSection = EditorPrefs.GetBool("expandFogOfWarSection", false);
			expandFogVoidSection = EditorPrefs.GetBool("expandFogVoidSection", false);
			expandOptimizationSettingsSection = EditorPrefs.GetBool("expandOptimizationSettingsSection", false);
			expandSunShaftsSection = EditorPrefs.GetBool("expandSunShaftsSection", false);
			expandSunShadowsSection = EditorPrefs.GetBool("expandSunShadowsSection", false);
			expandFogBlurSection = EditorPrefs.GetBool("expandFogBlurSection", false);
			expandFogAreaSection = EditorPrefs.GetBool("expandFogAreaSection", false);
			expandFogMaskSection = EditorPrefs.GetBool("expandFogMaskSection", false);
			ScanKeywords();
			ScanAdvancedOptions();
		}

		void OnDisable() {
			// Save folding sections state
			EditorPrefs.SetBool("expandFogGeometrySection", expandFogGeometrySection);
			EditorPrefs.SetBool("expandFogColorsSection", expandFogColorsSection);
			EditorPrefs.SetBool("expandFogAnimationSection", expandFogAnimationSection);
			EditorPrefs.SetBool("expandSkySection", expandSkySection);
			EditorPrefs.SetBool("expandOptionalPointLightSection", expandOptionalPointLightSection);
			EditorPrefs.SetBool("expandFogOfWarSection", expandFogOfWarSection);
			EditorPrefs.SetBool("expandFogVoidSection", expandFogVoidSection);
			EditorPrefs.SetBool("expandOptimizationSettingsSection", expandOptimizationSettingsSection);
			EditorPrefs.SetBool("expandSunShaftsSection", expandSunShaftsSection);
			EditorPrefs.SetBool("expandSunShadowsSection", expandSunShadowsSection);
			EditorPrefs.SetBool("expandFogBlurSection", expandFogBlurSection);
			EditorPrefs.SetBool("expandFogAreaSection", expandFogAreaSection);
			EditorPrefs.SetBool("expandFogMaskSection", expandFogMaskSection);
		}

		public override void OnInspectorGUI() {
			if (_fog == null)
				return;

			if (buttonNormalStyle == null) {
				buttonNormalStyle = new GUIStyle(GUI.skin.button);
			}
			if (buttonPressedStyle == null) {
				buttonPressedStyle = new GUIStyle(buttonNormalStyle);
				buttonPressedStyle.fontStyle = FontStyle.Bold;
			}

			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			DrawTitleLabel("General Settings");
			if (GUILayout.Button("Help", GUILayout.Width(50)))
				EditorUtility.DisplayDialog("Help", "Move the mouse over each label to show a description of the parameter.\nThese parameters allow you to customize the fog effect to achieve the effect and performance desired.\n\nPlease rate Volumetric Fog & Mist on the Asset Store! Thanks.", "Ok");
			if (GUILayout.Button("About", GUILayout.Width(60))) {
				VolumetricFogAbout.ShowAboutWindow();
			}
			if (_fog.hasCamera) {
				if (GUILayout.Button(toggleOptimizeBuild ? "Hide Shader Options" : "Shader Options", toggleOptimizeBuild ? buttonPressedStyle : buttonNormalStyle, GUILayout.Width(toggleOptimizeBuild ? 150 : 100))) {
					toggleOptimizeBuild = !toggleOptimizeBuild;
				}
			}
			EditorGUILayout.EndHorizontal();

			serializedObject.Update();

			VolumetricFogSInfo shaderInfo = null;
			if (shaders.Count > 0) {
				shaderInfo = shaders[0];
			}

			if (toggleOptimizeBuild) {
				EditorGUILayout.EndVertical();
				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical();
				DrawTitleLabel("Runtime Shader Options");
				EditorGUILayout.HelpBox("Select the features you want to use.\nUNSELECTED features will NOT be included in the build, reducing compilation time and build size.", MessageType.Info);
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Refresh", GUILayout.Width(60))) {
					ScanKeywords();
					ScanAdvancedOptions();
					GUIUtility.ExitGUI();
					return;
				}
				bool shaderChanged = false;
				for (int k = 0; k < shaders.Count; k++) {
					if (shaders[k].pendingChanges)
						shaderChanged = true;
				}
				if (!shaderChanged)
					GUI.enabled = false;
				if (GUILayout.Button("Save Changes", GUILayout.Width(110))) {
					UpdateShaders();
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
				if (shaderInfo != null) {
					bool firstColumn = true;
					EditorGUILayout.BeginHorizontal();
					for (int k = 0; k < shaderInfo.keywords.Count; k++) {
						SCKeyword keyword = shaderInfo.keywords[k];
						if (keyword.isUnderscoreKeyword)
							continue;
						if (firstColumn) {
							EditorGUILayout.LabelField("", GUILayout.Width(10));
						}
						bool prevState = keyword.enabled;
						keyword.enabled = EditorGUILayout.Toggle(prevState, GUILayout.Width(18));
						if (prevState != keyword.enabled) {
							shaderInfo.pendingChanges = true;
							GUIUtility.ExitGUI();
							return;
						}
						string keywordName = SCKeywordChecker.Translate(keyword.name);
						EditorGUILayout.LabelField(keywordName, GUILayout.Width(150));
						firstColumn = !firstColumn;
						if (firstColumn) {
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.Separator();

				if (shaderAdvancedOptionsInfo != null) {

					DrawTitleLabel("Advanced Options");
					EditorGUILayout.HelpBox("Specific options that can only be set in design time.", MessageType.Info);
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Refresh", GUILayout.Width(60))) {
						ScanAdvancedOptions();
						EditorGUIUtility.ExitGUI();
						return;
					}
					if (!shaderAdvancedOptionsInfo.pendingChanges)
						GUI.enabled = false;
					if (GUILayout.Button("Save Changes", GUILayout.Width(110))) {
                        if (_pointLightTrackingCount.intValue < pointLightInspectorCount) {
                            _pointLightTrackingCount.intValue = pointLightInspectorCount;
                        }
                        serializedObject.ApplyModifiedProperties();
                        shaderAdvancedOptionsInfo.SetOptionValue("FOG_MAX_POINT_LIGHTS", pointLightInspectorCount);
						shaderAdvancedOptionsInfo.UpdateAdvancedOptionsFile();
						_fog.UpdateVolumeMask();
                        _fog.UpdateMaterialProperties();
                        EditorGUIUtility.ExitGUI();
					}
					GUI.enabled = true;
					EditorGUILayout.EndHorizontal();
					int optionsCount = shaderAdvancedOptionsInfo.options.Length;
					for (int k = 0; k < optionsCount; k++) {
						ShaderAdvancedOption option = shaderAdvancedOptionsInfo.options[k];
                        if (option.hasValue) continue;
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("", GUILayout.Width(10));
						bool prevState = option.enabled;
						bool newState = EditorGUILayout.Toggle(prevState, GUILayout.Width(18));
						if (prevState != newState) {
							shaderAdvancedOptionsInfo.options[k].enabled = newState;
							shaderAdvancedOptionsInfo.pendingChanges = true;
							EditorGUIUtility.ExitGUI();
							return;
						}
						EditorGUILayout.LabelField(option.name);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("", GUILayout.Width(10));
						EditorGUILayout.LabelField("", GUILayout.Width(18));
						GUIStyle wrapStyle = new GUIStyle(GUI.skin.label);
						wrapStyle.wordWrap = true;
						EditorGUILayout.LabelField(option.description, wrapStyle);
						EditorGUILayout.EndHorizontal();
					}

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("", GUILayout.Width(10));
                    EditorGUI.BeginChangeCheck();
                    pointLightInspectorCount = EditorGUILayout.IntField(new GUIContent("Supported Point Lights", "The total number of point lights that can be rendered with the fog."), pointLightInspectorCount);
                    if (EditorGUI.EndChangeCheck()) {
                        shaderAdvancedOptionsInfo.pendingChanges = true;
                    }
                    EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.Separator();

				return;
			}

			EditorGUIUtility.labelWidth = 130f;

			if (_fog.hasCamera) {
				if (shaderInfo != null && shaderInfo.enabledKeywordCount >= 10) {
					EditorGUILayout.HelpBox("Please remember to disable unwanted effects in Shader Options before building your game!", MessageType.Warning);
				}
			} else {
				EditorGUILayout.PropertyField(fogRenderer, new GUIContent("Fog Renderer", "Assign here a Volumetric Fog script attached to the camera."));
			}

			int prevInt = _preset.intValue;
			EditorGUILayout.PropertyField(_preset, new GUIContent("Legacy Presets", "Load a factory preset. This is deprecated and profiles should be used instead."));
			presetChanged = (prevInt != _preset.intValue);

			EditorGUILayout.BeginHorizontal();
			VolumetricFogProfile prevProfile = (VolumetricFogProfile)_profile.objectReferenceValue;
			EditorGUILayout.PropertyField(_profile, new GUIContent("Profile", "Create or load stored presets."));

			if (_profile.objectReferenceValue != null) {

				if (prevProfile != _profile.objectReferenceValue) {
					profileChanged = true;
					_preset.intValue = (int)FOG_PRESET.Custom;
				}

				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("", GUILayout.Width(130));
				if (GUILayout.Button(new GUIContent("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width(60))) {
					CreateProfile();
					profileChanged = false;
					enableProfileApply = false;
					GUIUtility.ExitGUI();
					return;
				}
				if (GUILayout.Button(new GUIContent("Revert", "Updates fog settings with the profile configuration."), GUILayout.Width(60))) {
					profileChanged = true;
				}
				if (!enableProfileApply)
					GUI.enabled = false;
				if (GUILayout.Button(new GUIContent("Apply", "Updates profile configuration with changes in this inspector."), GUILayout.Width(60))) {
					enableProfileApply = false;
					profileChanged = false;
					_fog.profile.Save(_fog);
					EditorUtility.SetDirty(_fog.profile);
					GUIUtility.ExitGUI();
					return;
				}
				GUI.enabled = true;
			} else {
				if (GUILayout.Button(new GUIContent("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width(60))) {
					CreateProfile();
					GUIUtility.ExitGUI();
					return;
				}
			}
			EditorGUILayout.EndHorizontal();
            bool renderComponentChanged = false;

			if (_fog.hasCamera) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(_sun, new GUIContent("Sun", "Assign a Game Object (usually a Directional Light that acts as the Sun) to automatically synchronize the light direction parameter and make the fog highlight be aligned with the Sun."));
				if (_sun.objectReferenceValue != null) {
					if (GUILayout.Button(new GUIContent("Unassign", "Removes link with current directional light (Sun) to allow custom light color, direction and direction settings."))) {
						_sun.objectReferenceValue = null;
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.PropertyField(_useFogVolumes, new GUIContent("Enable Fog Volumes", "Allow the use of fog volumes, which will change fog and sky haze alpha automatically when camera enters/exits volumes.\nTo create a Fog Volume in your scene, select the option from the main menu 'GameObject/Create Other/Volumetric Fog Volume'."));

				if (IsFeatureEnabled(VolumetricFog.SKW_FOG_COMPUTE_DEPTH)) {
					EditorGUILayout.PropertyField(_computeDepth, new GUIContent("Compute Depth", "Computes depth for some objects that don't write to z-buffer, such as tree billboards. Enable only if neccesary."));
					if (_computeDepth.boolValue) {
						EditorGUILayout.PropertyField(_transparencyLayerMask, new GUIContent("   Layer Mask", "Select which layers will be used for the Compute Depth option."));
						EditorGUILayout.IntPopup(_computeDepthScope, computeDepthOptions, computeDepthValues, new GUIContent("   Scope", "Select which kind of objects will be used for the Compute Depth option."));
						if (_computeDepthScope.intValue == (int)COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects) {
							EditorGUILayout.PropertyField(_transparencyCutOff, new GUIContent("   Cut Off", "Transparency cut-off factor."));
						}
					}
				} else {
					if (_computeDepth.boolValue) {
						_computeDepth.boolValue = false;
					}
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label("Compute Depth", GUILayout.Width(EditorGUIUtility.labelWidth));
					DrawFeatureIsDisabled();
					EditorGUILayout.EndHorizontal();
				}
				bool prev = _renderBeforeTransparent.boolValue;
				EditorGUILayout.PropertyField(_renderBeforeTransparent, new GUIContent("Render Before Transp.", "Applies image effect before transparent objects are rendered. For example, you may disable this option to render the fog on top of transparent objects, like water. But if you use particles, you should enable it to avoid particles being overdrawn by the fog if this is too thick. This option is not compatible with 'Improve transparency'."));
				if (_renderBeforeTransparent.boolValue != prev) {
                    renderComponentChanged = true;
                }

				if (_renderBeforeTransparent.boolValue || (_computeDepth.boolValue && _computeDepthScope.intValue == (int)COMPUTE_DEPTH_SCOPE.TreeBillboardsAndTransparentObjects)) {
					GUI.enabled = false;
				}
				prevInt = _transparencyBlendMode.intValue;
				EditorGUILayout.PropertyField(_transparencyBlendMode, new GUIContent("Transparency Mode", "The transparency support mode. None, Screen Direct (blits a fog overlay over transparent objects directly to the screen) or Blend Pass (same but using a normal render pass which makes this compatible with VR)"));
				if (_transparencyBlendMode.intValue != prevInt) {
                    renderComponentChanged = true;
                }

				if (_transparencyBlendMode.intValue == (int)TRANSPARENT_MODE.Blend) {
					EditorGUILayout.PropertyField(_transparencyBlendPower, new GUIContent("   Blend Power", "Blending weight for the transparent objects."));
					EditorGUILayout.HelpBox("Transparency Blend works better if Volumetric Fog & Mist is the first image effect on the camera.", MessageType.Info);
				}
				GUI.enabled = true;

				EditorGUILayout.PropertyField(_enableMultipleCameras, new GUIContent("Enable Multi Camera", "If this option is enabled, this camera will render all fog areas in the scene. Otherwise, only those fog areas with the fog renderer property pointing to this instance will be used."));
			}

			if (_fog.renderingInstancesCount > 1) {
				GUILayout.Label(new GUIContent("(Rendering " + _fog.renderingInstancesCount + " fog instances)"));
			}

			EditorGUILayout.Separator();
			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();

			EditorGUIUtility.labelWidth = 120f;

			// Inspector sections start
			if (sectionHeaderStyle == null) {
				sectionHeaderStyle = new GUIStyle(EditorStyles.foldout);
			}
			sectionHeaderStyle.SetFoldoutColor();

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			expandFogGeometrySection = EditorGUILayout.Foldout(expandFogGeometrySection, "Fog Geometry", sectionHeaderStyle);
			EditorGUILayout.EndHorizontal();

			if (expandFogGeometrySection) {
				EditorGUILayout.PropertyField(_density, new GUIContent("Density", "General density of the fog. Higher density fog means darker fog as well."));
				EditorGUILayout.PropertyField(_noiseStrength, new GUIContent("Noise Strength", "Randomness of the fog formation. 0 means uniform fog whereas a value towards 1 will make areas of different densities and heights."));
				EditorGUILayout.PropertyField(_noiseScale, new GUIContent("   Scale", "Increasing this value will expand the size of the noise."));
				EditorGUILayout.PropertyField(_noiseSparse, new GUIContent("   Sparse", "Increase to make noise sparser."));
				EditorGUILayout.PropertyField(_noiseFinalMultiplier, new GUIContent("   Final Multiplier", "Final noise value multiplier."));

				EditorGUILayout.PropertyField(_distance, new GUIContent("Distance", "Distance in meters from the camera at which the fog starts. It works with Distance FallOff."));
				EditorGUILayout.PropertyField(_distanceFallOff, new GUIContent("Distance FallOff", "When you set a value to Distance > 0, this parameter defines the gradient of the fog to the camera. The higher the value, the shorter the gradient."));
				EditorGUILayout.PropertyField(_maxFogLength, new GUIContent("Max. Distance", "Maximum distance from camera at which the fog is rendered. Decrease this value to improve performance."));
				EditorGUILayout.PropertyField(_maxFogLengthFallOff, new GUIContent("Max. Dist. FallOff", "Blends far range with background."));
				EditorGUILayout.PropertyField(_height, new GUIContent("Height", "Maximum height of the fog in meters."));

				if (IsFeatureEnabled(VolumetricFog.SKW_FOG_USE_XY_PLANE)) {
					EditorGUILayout.PropertyField(_useXYPlane, new GUIContent("Use XY Plane", "Switches between normal mode to XY mode."));
				} else {
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label(new GUIContent("Use XY Plane", "Switches between normal mode to XY mode."), GUILayout.Width(EditorGUIUtility.labelWidth));
					DrawFeatureIsDisabled();
					EditorGUILayout.EndHorizontal();
				}

				EditorGUILayout.BeginHorizontal();
				GUIContent cnt;
				if (_useXYPlane.boolValue) {
					cnt = new GUIContent("Base Z", "Starting Z of the fog in meters.");
				} else {
					cnt = new GUIContent("Base Height", "Starting height of the fog in meters. You can set this value above Camera position. Try it!");
				}
				EditorGUILayout.PropertyField(_baselineHeight, cnt);
				if (GUILayout.Button(new GUIContent("Reset", "Automatically sets base height to water level or to zero height if no water found in the scene."), GUILayout.Width(60))) {
					_fog.CheckWaterLevel(true);
					_preset.intValue = (int)FOG_PRESET.Custom;
				}
				EditorGUILayout.EndHorizontal();

				if (_useXYPlane.boolValue)
					GUI.enabled = false;
				EditorGUILayout.PropertyField(_baselineRelativeToCamera, new GUIContent("Relative To Camera", "If set to true, the base height will be added to the camera height. This is useful for cloud styles so they always stay over your head!"));
				if (_baselineRelativeToCamera.boolValue) {
					EditorGUILayout.PropertyField(_baselineRelativeToCameraDelay, new GUIContent("Delay", "Speed factor for transitioning to new camera heights."));
				}
				GUI.enabled = true;

				EditorGUILayout.Separator();
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			expandFogColorsSection = EditorGUILayout.Foldout(expandFogColorsSection, "Fog Colors", sectionHeaderStyle);
			EditorGUILayout.EndHorizontal();

			if (expandFogColorsSection) {

				EditorGUILayout.PropertyField(_alpha, new GUIContent("Alpha", "Transparency for the fog. You may want to reduce this value if you experiment issues with billboards."));
				EditorGUILayout.PropertyField(_color, new GUIContent("Albedo", "Base color of the fog."));
				EditorGUILayout.PropertyField(_specularColor, new GUIContent("Specular Color", "This is the color reflected by the fog under direct light exposure (see Light parameters)"));
				EditorGUILayout.PropertyField(_specularThreshold, new GUIContent("Specular Threshold", "Area of the fog subject to light reflectancy"));
				EditorGUILayout.PropertyField(_specularIntensity, new GUIContent("Specular Intensity", "The intensity of the reflected light."));

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Light Direction", "The normalized direction of a simulated directional light."), GUILayout.Width(120));
				if (_sun.objectReferenceValue != null) {
					EditorGUILayout.LabelField("(uses Sun light direction)");
				} else {
					EditorGUILayout.PropertyField(_lightDirection, GUIContent.none);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.PropertyField(_lightingModel, new GUIContent("Lighting Model", "The lighting model used to calculate fog color. 'Legacy' is provided for previous version compatibility. 'Natural' uses ambient + light color. 'Single Light' excludes ambient color."));

				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Light Intensity", "Intensity of the simulated directional light."), GUILayout.Width(120));
				if (_lightingModel.intValue == (int)LIGHTING_MODEL.Classic) {
					_lightIntensity.floatValue = EditorGUILayout.Slider(_lightIntensity.floatValue, -1, 1);
				} else {
					_lightIntensity.floatValue = EditorGUILayout.Slider(_lightIntensity.floatValue, 0, 3);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.PropertyField(_lightColor, new GUIContent("Light Color", "Color of the simulated direcional light."));
				EditorGUILayout.PropertyField(_sunCopyColor, new GUIContent("Copy Sun Color", "Always use Sun light color. Disable this property to allow choosing a custom light color."));
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			expandFogAnimationSection = EditorGUILayout.Foldout(expandFogAnimationSection, "Fog Animation", sectionHeaderStyle);
			EditorGUILayout.EndHorizontal();

			if (expandFogAnimationSection) {

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PropertyField(_speed, new GUIContent("Wind Speed", "Speed factor for the simulated wind effect over the fog."));
				if (GUILayout.Button("Stop", GUILayout.Width(60))) {
					_speed.floatValue = 0f;
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.PropertyField(_windDirection, new GUIContent("Wind Direction", "Normalized direcional vector for the wind effect."));
				EditorGUILayout.PropertyField(_turbulenceStrength, new GUIContent("Turbulence", "Turbulence strength. Set to zero to deactivate. Turbulence adds a render pass to compute fog animation."));
				EditorGUILayout.PropertyField(_useRealTime, new GUIContent("Use Real Time", "Uses real elapsed time since last fog rendering instead of Time.deltaTime (elapsed time since last frame) to ensure continuous fog animation irrespective of camera enable state. For example, if you disable the camera and 'Use Real Time' is enabled, when you enable the camera again, the fog animation wil compute the total elapsed time so it shows as if fog continues animating while camera was disabled.  If set to false, fog animation will use Time.deltaTime (elapsed time since last frame) which will cause fog to resume previous state."));
			}

			EditorGUILayout.EndVertical();

			if (_fog.hasCamera) {
				EditorGUILayout.Separator();
				EditorGUILayout.BeginVertical();

				if (IsFeatureEnabled(VolumetricFog.SKW_FOG_HAZE_ON)) {
					expandSkySection = EditorGUILayout.Foldout(expandSkySection, (_skyHaze.floatValue > 0 && _skyAlpha.floatValue > 0) ? FOG_SKY_HAZE_ON : FOG_SKY_HAZE_OFF, sectionHeaderStyle);

					if (expandSkySection) {
						if (_useXYPlane.boolValue) {
							EditorGUILayout.HelpBox("Sky haze is disabled when using XY plane.", MessageType.Info);
						}
						EditorGUILayout.PropertyField(_skyHaze, new GUIContent("Haze", "Height of the sky haze in meters. Reduce this or alpha to 0 to disable sky haze."));
						EditorGUILayout.PropertyField(_skyColor, new GUIContent("Color", "Sky haze color."));
						EditorGUILayout.PropertyField(_skySpeed, new GUIContent("Speed", "Speed of the haze animation."));
						EditorGUILayout.PropertyField(_skyNoiseStrength, new GUIContent("Noise Strength", "Amount of noise for the sky haze."));
						EditorGUILayout.PropertyField(_skyAlpha, new GUIContent("Alpha", "Transparency of the sky haze. Reduce this or Haze above to 0 to disable sky haze. Important: if you experiment issues with tree billboards you need to lower this value."));
						EditorGUILayout.PropertyField(_skyDepth, new GUIContent("Depth", "Any pixel beyond this depth is assumed to be part of the skybox. For standard skybox, leave this value at 0.999. If you're using Time of Day or other skybox dome, you need to reduce this value so it fits inside the sky dome."));
					}
				} else {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.Foldout(false, FOG_SKY_HAZE_OFF, sectionHeaderStyle);
					DrawFeatureIsDisabled();
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.Separator();

			EditorGUILayout.BeginVertical();

			if (IsFeatureEnabled(VolumetricFog.SKW_LIGHT_SCATTERING)) {
				expandSunShaftsSection = EditorGUILayout.Foldout(expandSunShaftsSection, _lightScatteringEnabled.boolValue ? SCATTERING_ON : SCATTERING_OFF, sectionHeaderStyle);

				if (expandSunShaftsSection) {
					EditorGUILayout.PropertyField(_lightScatteringEnabled, new GUIContent("Enable", "Enables screen space light scattering. Simulates scattering of Sun light through atmosphere."));
					if (_lightScatteringEnabled.boolValue && _fog.fogRenderer != null && _fog.fogRenderer.sun == null) {
						EditorGUILayout.HelpBox("Light Scattering requires a Sun reference. Assign a game object (representing Sun or a light) in General Settings section.", MessageType.Warning);
					}
					EditorGUILayout.PropertyField(_lightScatteringDiffusion, new GUIContent("Diffusion", "Spread or intensity of Sun light scattering."));
					EditorGUILayout.PropertyField(_lightScatteringExposure, new GUIContent("Shafts Intensity", "Intensity for the Sun shafts effect."));
					EditorGUILayout.PropertyField(_lightScatteringSpread, new GUIContent("   Spread", "Length of the Sun rays. "));
					EditorGUILayout.PropertyField(_lightScatteringWeight, new GUIContent("   Sample Weight", "Strength of Sun rays."));
					EditorGUILayout.PropertyField(_lightScatteringIllumination, new GUIContent("   Start Illumination", "Initial strength of each ray."));
					EditorGUILayout.PropertyField(_lightScatteringDecay, new GUIContent("   Decay", "Decay multiplier applied on each step."));
					EditorGUILayout.PropertyField(_lightScatteringSamples, new GUIContent("   Samples", "Number of light samples used when Light Scattering is enabled. Reduce to increse performance."));
					EditorGUILayout.PropertyField(_lightScatteringJittering, new GUIContent("   Jittering", "Smooths rays removing artifacts and allowing to use less samples."));
				}
			} else {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Foldout(false, SCATTERING_OFF, sectionHeaderStyle);
				DrawFeatureIsDisabled();
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			if (IsFeatureEnabled(VolumetricFog.SKW_SUN_SHADOWS)) {
				expandSunShadowsSection = EditorGUILayout.Foldout(expandSunShadowsSection, _sunShadows.boolValue ? SUN_SHADOWS_ON : SUN_SHADOWS_OFF, sectionHeaderStyle);

				if (expandSunShadowsSection) {
					EditorGUILayout.PropertyField(_sunShadows, new GUIContent("Enable", "Enables Sun shadow casting over fog."));

					if (_fog.hasCamera) {
						if (_sunShadows.boolValue) {
							if (_sun.objectReferenceValue == null) {
								EditorGUILayout.HelpBox("Sun Shadows requires a Sun reference. Assign a game object (representing Sun or a light) in General Settings section.", MessageType.Warning);
							}
							if (_fog.renderingInstancesCount > 1) {
								EditorGUILayout.HelpBox("Fog areas could be forcing this option.", MessageType.Info);
							}
						}
						EditorGUILayout.PropertyField(_sunShadowsLayerMask, new GUIContent("Layer Mask", "Select which layers will be used for the Sun Shadows option."));
						EditorGUILayout.PropertyField(_sunShadowsStrength, new GUIContent("Strength", "Shadow strength. "));
						EditorGUILayout.PropertyField(_sunShadowsResolution, new GUIContent("Resolution", "Resolution of the shadow map texture. Sizes: 0=512, 1=1024, 2=2048, 3=4096, 4=8192."));
						EditorGUILayout.PropertyField(_sunShadowsMaxDistance, new GUIContent("Max Distance", "Shadow visibility maximum distance from camera. It cannot exceed the fog max distance value in Fog Geometry section."));
						EditorGUILayout.PropertyField(_sunShadowsCancellation, new GUIContent("Shadow Cancellation", "Increase this value to reduce visibility of shadowed fog."));
						EditorGUILayout.PropertyField(_sunShadowsBakeMode, new GUIContent("Shadow Map Update", "Realtime captures shadow map every frame. Discrete will bake shadow map only when Sun rotates or according to Force Update Interval parameter."));
						if (_sunShadowsBakeMode.intValue != (int)SUN_SHADOWS_BAKE_MODE.Realtime) {
							EditorGUILayout.PropertyField(_sunShadowsRefreshInterval, new GUIContent("Force Update Interval", "Updates shadow map texture after specified time interval (in seconds). Set this value to 0 to ignore shadow updating. Note that it will update whenever the Sun rotates."));
						}
						EditorGUILayout.PropertyField(_sunShadowsBias, new GUIContent("Bias", "A threshold value to determine if a position is under shadow. A small value prevents self-shadowing."));
						EditorGUILayout.PropertyField(_sunShadowsJitterStrength, new GUIContent("Jittering", "Helps reducing shadow banding artifacts."));
					} else {
						if (_sunShadows.boolValue) {
							EditorGUILayout.HelpBox("Configure shadow properties on the VolumetricFog script attached to the camera.", MessageType.Info);
						}
					}
				}
			} else {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Foldout(false, SUN_SHADOWS_OFF, sectionHeaderStyle);
				DrawFeatureIsDisabled();
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			if (IsFeatureEnabled(VolumetricFog.SKW_FOG_BLUR)) {
				expandFogBlurSection = EditorGUILayout.Foldout(expandFogBlurSection, _fogBlur.boolValue ? FOG_BLUR_ON : FOG_BLUR_OFF, sectionHeaderStyle);
				if (expandFogBlurSection) {
					EditorGUILayout.PropertyField(_fogBlur, new GUIContent("Enable", "Applies a depth blur effect to the fog to simulate extra light scattering."));
					EditorGUILayout.PropertyField(_fogBlurDepth, new GUIContent("Depth Threshold", "Starting distance for depth blur."));
				}
			} else {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Foldout(false, FOG_BLUR_OFF, sectionHeaderStyle);
				DrawFeatureIsDisabled();
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			expandOptionalPointLightSection = EditorGUILayout.Foldout(expandOptionalPointLightSection, "Point Lights", sectionHeaderStyle);
			if (expandOptionalPointLightSection) {
				EditorGUILayout.PropertyField(_pointLightTrackingAuto, new GUIContent("Track Point Lights", "Check this option to automatically select the nearest point lights."));
				GUI.enabled = _pointLightTrackingAuto.boolValue;
                EditorGUILayout.PropertyField(_pointLightTrackingCheckInterval, new GUIContent("  Update Interval", "Time interval between point light checks (position and other attributes changes)."));
                EditorGUILayout.PropertyField(_pointLightTrackingNewLightsCheckInterval, new GUIContent("  Check New Lights Interval", "Time interval between checks for new lights. A value of 0 disables looking for new lights."));
				EditorGUILayout.PropertyField(_pointLightTrackingPivot, new GUIContent("  Distance To", "This is the pivot from which distance is computed."));
                EditorGUILayout.IntSlider(_pointLightTrackingCount, 0, VolumetricFog.MAX_POINT_LIGHTS,new GUIContent("  Max Point Lights", "Specify the maximum number of point lights that will be tracked. Nearest point light will be assigned to slot 1, next one to slot 2, and so on, up to the number specified here. This option is very useful for choosing the proper lights as the camera moves through the scene."));
				GUI.enabled = true;
				EditorGUILayout.PropertyField(_pointLightInscattering, new GUIContent("Inscattering", "Use this parameter to adjust light range inside the fog."));
				EditorGUILayout.PropertyField(_pointLightIntensity, new GUIContent("Global Intensity", "Use this parameter to adjust all lights intensities inside the fog."));
                EditorGUILayout.PropertyField(_pointLightInsideAtten, new GUIContent("Inside Atten", "Increase to reduce point light brightness within intensity sphere or if point light is collinear to avoid screen burn."));
                EditorGUILayout.PropertyField(_pointLightParams, new GUIContent("Light Settings", "Customize individual light settings."), true);
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			if (IsFeatureEnabled(VolumetricFog.SKW_FOG_OF_WAR_ON)) {
				expandFogOfWarSection = EditorGUILayout.Foldout(expandFogOfWarSection, _fogOfWarEnabled.boolValue ? FOW_ON : FOW_OFF, sectionHeaderStyle);

				if (expandFogOfWarSection) {

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PropertyField(_fogOfWarEnabled, new GUIContent("Enable", "Enables fog of war feature. When enabled, you can call SetFogOfWarAlpha(worldPosition, alpha) to define the transparency of the fog at certain positions in world space."));
					if (GUILayout.Button("Reset", GUILayout.Width(60)))
						_fog.ResetFogOfWar();
					EditorGUILayout.EndHorizontal();

					GUI.enabled = _fogOfWarEnabled.boolValue;
					if (_useXYPlane.boolValue) {
						EditorGUILayout.HelpBox("Z coordinate is ignored.", MessageType.Info);
					} else {
						EditorGUILayout.HelpBox("Y coordinate is ignored.", MessageType.Info);
					}
					EditorGUILayout.PropertyField(_fogOfWarCenter, new GUIContent("Center", "Location of the center of the fog war area in world space. Only X and Z coordinates are considered."));
					EditorGUILayout.PropertyField(_fogOfWarSize, new GUIContent("Size", "Size of the fog of war. This is the bounds in world units of the fog of war (only X and Z coordinates are considered). Outside of these bounds, fog will appear as normal (so you can only set different alpha values inside these bounds)."));
					EditorGUILayout.PropertyField(_fogOfWarTextureSize, new GUIContent("Texture Size", "A square texture is used to hold the transparency bits mapped to each world position. This parameter defines the size of the texture. The greater the size the more granularity for the fog of war effect. The optimal value depends on the size parameter. For instance, a fog of war size of 1024 units with a texture size of 1024 will result in one pixel of texture for each meter in world space."));
					EditorGUILayout.PropertyField(_fogOfWarRestoreDelay, new GUIContent("Restore Delay", "Delay after the fog has been cleared to restore the fog to original alpha. You clear or set the alpha of the fog at any position calling FogOfWarSetAlpha() method. Set this value to 0 to leave the cleared fog unrestored."));
					EditorGUILayout.PropertyField(_fogOfWarRestoreDuration, new GUIContent("Restore Duration", "Once the fog has started to be restored at any single location, the time in seconds for the fade in effect."));
					GUI.enabled = true;
				}
			} else {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Foldout(false, FOW_OFF, sectionHeaderStyle);
				DrawFeatureIsDisabled();
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			bool fogVoidBoxEnabled = IsFeatureEnabled(VolumetricFog.SKW_FOG_VOID_BOX);
			bool fogVoidSphereEnabled = IsFeatureEnabled(VolumetricFog.SKW_FOG_VOID_SPHERE);
			if (!fogVoidBoxEnabled && !fogVoidSphereEnabled) {
				EditorGUILayout.Foldout(false, FOG_VOID_OFF, sectionHeaderStyle);
				DrawFeatureIsDisabled();
				EditorGUILayout.EndHorizontal();
			} else {
				expandFogVoidSection = EditorGUILayout.Foldout(expandFogVoidSection, _fogVoidRadius.floatValue > 0 ? FOG_VOID_ON : FOG_VOID_OFF, sectionHeaderStyle);
				if (!fogVoidBoxEnabled) {
					GUILayout.Label("(box shape disabled in Shader Options)");
				} else if (!fogVoidSphereEnabled) {
					GUILayout.Label("(sphere shape disabled in Shader Options)");
				}
				EditorGUILayout.EndHorizontal();

				if (expandFogVoidSection) {
					EditorGUILayout.PropertyField(_fogVoidPosition, new GUIContent("Center", "Location of the center of the fog void in world space (area where the fog disappear).\nThis option is very useful if you want a clear area around your character in 3rd person view."));
					EditorGUILayout.PropertyField(_character, new GUIContent("Character To Follow", "Assign a Game Object to follow its position automatically (usually the player character which will be at the center of the void area)."));
					EditorGUILayout.PropertyField(_fogVoidTopology, new GUIContent("Topology", "Shape of the void area."));
					if (_fogVoidTopology.intValue == (int)FOG_VOID_TOPOLOGY.Sphere) {
						EditorGUILayout.PropertyField(_fogVoidRadius, new GUIContent("Radius", "Radius of the void area."));
					} else if (_fogVoidTopology.intValue == (int)FOG_VOID_TOPOLOGY.Box) {
						EditorGUILayout.PropertyField(_fogVoidRadius, new GUIContent("Width", "Width of the void area."));
						EditorGUILayout.PropertyField(_fogVoidHeight, new GUIContent("Height", "Height of the void area."));
						EditorGUILayout.PropertyField(_fogVoidDepth, new GUIContent("Depth", "Depth of the void area."));
					}
					EditorGUILayout.PropertyField(_fogVoidFallOff, new GUIContent("FallOff", "Gradient of the void area effect."));
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			bool fogAreaBoxEnabled = IsFeatureEnabled(VolumetricFog.SKW_FOG_AREA_BOX);
			bool fogAreaSphereEnabled = IsFeatureEnabled(VolumetricFog.SKW_FOG_AREA_SPHERE);
			if (!fogAreaBoxEnabled && !fogAreaSphereEnabled) {
				EditorGUILayout.Foldout(false, FOG_AREA_OFF, sectionHeaderStyle);
				DrawFeatureIsDisabled();
				EditorGUILayout.EndHorizontal();
			} else {
				expandFogAreaSection = EditorGUILayout.Foldout(expandFogAreaSection, _fogAreaRadius.floatValue > 0 ? FOG_AREA_ON : FOG_AREA_OFF, sectionHeaderStyle);
				if (!fogAreaBoxEnabled) {
					GUILayout.Label("(box shape disabled in Shader Options)");
				} else if (!fogAreaSphereEnabled) {
					GUILayout.Label("(sphere shape disabled in Shader Options)");
				}
				EditorGUILayout.EndHorizontal();

				if (expandFogAreaSection) {
					if (_fog.hasCamera) {
						EditorGUILayout.PropertyField(_fogAreaPosition, new GUIContent("Center", "Location of the center of the fog area in world space."));
					}
					EditorGUILayout.PropertyField(_fogAreaCenter, new GUIContent("Follow GameObject", "Assign a Game Object to follow its position automatically as center for the fog area."));

					if (_fogAreaCenter.objectReferenceValue != null) {
						EditorGUILayout.PropertyField(_fogAreaFollowMode, new GUIContent("   Follow Mode", "Specify if the fog area should follow this game object in 3D or only in XZ plane (useful for clouds)."));
					}

					EditorGUILayout.PropertyField(_fogAreaTopology, new GUIContent("Topology", "Shape of the fog area."));

					if (_fog.hasCamera) {
						EditorGUILayout.PropertyField(_fogAreaRadius, new GUIContent("Radius", "Radius of the fog area."));
						if (_fogAreaTopology.intValue != (int)FOG_AREA_TOPOLOGY.Sphere) {
							EditorGUILayout.PropertyField(_fogAreaHeight, new GUIContent("Height", "Height of the fog area."));
							EditorGUILayout.PropertyField(_fogAreaDepth, new GUIContent("Depth", "Depth of the fog area."));
						}
					} else if (_fogAreaCenter.objectReferenceValue == null) {
						EditorGUILayout.HelpBox("Position and resize this fog area using the transform position and scale.", MessageType.Info);
					} else if (_fogAreaFollowMode.intValue == (int)FOG_AREA_FOLLOW_MODE.RestrictToXZPlane) {
						EditorGUILayout.HelpBox("Change the altitude of this fog area using the transform Y field. Also use the transform scale to resize.", MessageType.Info);
					}
					EditorGUILayout.PropertyField(_fogAreaFallOff, new GUIContent("FallOff", "Gradient of the fog area effect."));
					EditorGUILayout.PropertyField(_fogAreaSortingMode, new GUIContent("Sorting Mode", "Specify the sorting mode when rendering this fog area."));
					if (_fogAreaSortingMode.intValue == (int)FOG_AREA_SORTING_MODE.Fixed) {
						EditorGUILayout.PropertyField(_fogAreaRenderOrder, new GUIContent("   Order", "Position in the sorting queue."));
					}
					EditorGUILayout.PropertyField(_fogAreaShowGizmos, new GUIContent("Show Gizmo", "Show fog area boundary in Scene View."));
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			bool maskChanged = false;
			bool maskEnabledInShader = shaderAdvancedOptionsInfo != null && shaderAdvancedOptionsInfo.GetAdvancedOptionState("FOG_MASK");
			_enableMask.boolValue = maskEnabledInShader;
			expandFogMaskSection = EditorGUILayout.Foldout(expandFogMaskSection, _enableMask.boolValue ? FOG_MASK_ON : FOG_MASK_OFF, sectionHeaderStyle);
			if (expandFogMaskSection) {
				if (maskEnabledInShader) {
					GUI.enabled = false;
					EditorGUILayout.PropertyField(_enableMask, new GUIContent("Mask Enabled", "Enables screen mask feature. Limits fog rendering on screen based on custom mesh volumes. This feature can be enabled/disabled in Shader Options section on top of inspector."));
					GUI.enabled = _enableMask.boolValue;

					EditorGUILayout.BeginHorizontal();
					prevInt = _maskLayer.intValue;
					EditorGUILayout.PropertyField(_maskLayer, new GUIContent("Layer Mask", "Layer mask for determining which objects in scene compound the screen mask."));
					if (GUILayout.Button("Refresh", GUILayout.Width(60))) {
						TestMaskObjects();
						maskChanged = true;
					}
					EditorGUILayout.EndHorizontal();
					if (!string.IsNullOrEmpty(fogMaskTestResult)) {
						EditorGUILayout.HelpBox(fogMaskTestResult, MessageType.Info);
					}

					if (_maskLayer.intValue == -1) {
						EditorGUILayout.HelpBox("Choose a valid layer mask which only includes objects that compound the screen mask.", MessageType.Warning);
					} else
                    {
                        if (GUILayout.Button("Toggle Preview in GameView"))
                        {
                            _fog.TogglePreviewMask();
                        }
                    }
					if (_maskLayer.intValue != prevInt) {
						maskChanged = true;
					}
					prevInt = _maskDownsampling.intValue;
					EditorGUILayout.PropertyField(_maskDownsampling, new GUIContent("Mask Downsampling", "Optionally increase mask downsampling to improve performance."));
					if (_maskDownsampling.intValue != prevInt) {
						maskChanged = true;
					}
					GUI.enabled = true;
				} else {
					DrawFeatureIsDisabled();
				}
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical();

			expandOptimizationSettingsSection = EditorGUILayout.Foldout(expandOptimizationSettingsSection, "Optimization & Quality Settings", sectionHeaderStyle);

			if (expandOptimizationSettingsSection) {
				EditorGUILayout.PropertyField(_downsampling, new GUIContent("Downsampling", "Reduces the size of the depth texture to improve performance."));
                if (_downsampling.intValue == 1) {
                    EditorGUILayout.PropertyField(_forceComposition, new GUIContent("Force Composition", "Enables final composition with optional edge filter at no downscale to improve edges when MSAA is enabled."));
                }

                if (_downsampling.intValue > 1 || _forceComposition.boolValue) {
					EditorGUILayout.PropertyField(_edgeImprove, new GUIContent("Improve Edges", "Check this option to reduce artifacts and halos around geometry edges when downsampling is applied. This is an option because it's faster to not take care or geometry edges, which is probably unnecesary if you use fog as elevated clouds."));
					if (_edgeImprove.boolValue) {
						EditorGUILayout.PropertyField(_edgeThreshold, new GUIContent("   Threshold", "Depth threshold used to detected edges."));
					}
				}

				EditorGUILayout.PropertyField(_stepping, new GUIContent("Stepping", "Multiplier to the ray-marching algorithm. Values between 8-12 are good. Increasing the stepping will produce more accurate and better quality fog but performance will be reduced. The less the density of the fog the lower you can set this value."));
				EditorGUILayout.PropertyField(_steppingNear, new GUIContent("Stepping Near", "Works with Stepping parameter but applies only to short distances from camera. Lowering this value can help to reduce banding effect (performance can be reduced as well)."));
				EditorGUILayout.PropertyField(_dithering, new GUIContent("Dithering", "Blends final fog color with a pattern to reduce banding artifacts. Use the slider to choose the intensity of dither."));
				if (_dithering.boolValue) {
					EditorGUILayout.PropertyField(_ditherStrength, new GUIContent("   Dither Strength"));
				}
				EditorGUILayout.PropertyField(_jitterStrength, new GUIContent("Jittering", "Apply a random value to the length of ray to reduce banding artifacts, useful on sharp corners on volumes. Use the slider to choose the jittering amount. For low density fog you may need to increase the jitter intensity."));

				if (_fog.hasCamera) {
					EditorGUILayout.PropertyField(_spsrBehaviour, new GUIContent("Single Pass Stereo", "Specify the SPSR mode for this camera."));

					GUI.enabled = false;
					EditorGUILayout.BeginHorizontal();
					GUILayout.Label(new GUIContent("SPSR Enabled", "Enables Single Pass Stereo Rendering when using in VR (automatically set based on player settings)."), GUILayout.Width(120));
#if UNITY_5_5_OR_NEWER
					_useSinglePassStereoRenderingMatrix.boolValue = PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
#else
																				_useSinglePassStereoRenderingMatrix.boolValue = PlayerSettings.singlePassStereoRendering;
#endif
					EditorGUILayout.Toggle(_useSinglePassStereoRenderingMatrix.boolValue, GUILayout.Width(20));
					EditorGUILayout.EndHorizontal();

					GUI.enabled = true;
				}

				EditorGUILayout.PropertyField(_timeBetweenTextureUpdates, new GUIContent("Texture Update Interval", "Minimum time between texture updates. Reduce or change this to 0 to make directional light influence change smoothly at performance exchange."));
				EditorGUILayout.PropertyField(_updateTextureSpread, new GUIContent("Fog Update Spread", "Spreads internal fog texture updates over several frames. A value of 1 will update fog immediately according to changes in lighting, Sun and other features. A value of 2 will distribute the workload in two frames, reducing CPU spikes. A greater value will span the update over more frames."));
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.Separator();


			if (serializedObject.ApplyModifiedProperties() || (Event.current != null && Event.current.commandName == "UndoRedoPerformed") || GUI.changed || _fog.isDirty) {
				if (_fog.profile != null) {
					if (profileChanged) {
						_fog.profile.Load(_fog);
						profileChanged = false;
						enableProfileApply = false;
					} else {
						enableProfileApply = true;
					}
				}
				if (!presetChanged) {
					_fog.preset = FOG_PRESET.Custom;
					presetChanged = false;
				}
				if (maskChanged) _fog.UpdateVolumeMask();
				_fog.UpdatePreset();
				_fog.UpdateMultiCameraSetup();
				_fog.isDirty = false;
				EditorUtility.SetDirty(target);
                if (renderComponentChanged) EditorGUIUtility.ExitGUI();
			}

		}

		GUIStyle titleLabelStyle;

		void DrawTitleLabel(string s) {
			if (titleLabelStyle == null) {
				titleLabelStyle = new GUIStyle(GUI.skin.label);
			}
			titleLabelStyle.normal.textColor = titleColor;
			titleLabelStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label(s, titleLabelStyle);
		}

		LayerMask LayerMaskField(LayerMask layerMask) {
			List<string> layers = new List<string>();
			List<int> layerNumbers = new List<int>();

			for (int i = 0; i < 32; i++) {
				string layerName = LayerMask.LayerToName(i);
				if (layerName != "") {
					layers.Add(layerName);
					layerNumbers.Add(i);
				}
			}
			int maskWithoutEmpty = 0;
			for (int i = 0; i < layerNumbers.Count; i++) {
				if (((1 << layerNumbers[i]) & layerMask.value) > 0)
					maskWithoutEmpty |= (1 << i);
			}
			maskWithoutEmpty = EditorGUILayout.MaskField("", maskWithoutEmpty, layers.ToArray());
			if (maskWithoutEmpty < 0)
				return -1;

			int mask = 0;
			for (int i = 0; i < layerNumbers.Count; i++) {
				if ((maskWithoutEmpty & (1 << i)) > 0)
					mask |= (1 << layerNumbers[i]);
			}
			layerMask.value = mask;
			return layerMask;
		}


		#region Shader handling

		void ScanKeywords() {
			if (shaders == null)
				shaders = new List<VolumetricFogSInfo>();
			else
				shaders.Clear();
			string[] guids = AssetDatabase.FindAssets("VolumetricFog t:Shader");
			for (int k = 0; k < guids.Length; k++) {
				string guid = guids[k];
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (path.EndsWith("Resources/Shaders/VolumetricFog.shader", StringComparison.OrdinalIgnoreCase)) {
					VolumetricFogSInfo shader = new VolumetricFogSInfo();
					shader.path = path;
					shader.name = Path.GetFileNameWithoutExtension(path);
					ScanShader(shader);
					if (shader.keywords.Count > 0) {
						shaders.Add(shader);
					}
					break;
				}
			}
		}

		void ScanShader(VolumetricFogSInfo shader) {
			// Inits shader
			shader.passes.Clear();
			shader.keywords.Clear();
			shader.pendingChanges = false;
			shader.editedByShaderControl = false;

			// Reads shader
			string[] shaderLines = File.ReadAllLines(shader.path);
			string[] separator = new string[] { " " };
			SCShaderPass currentPass = new SCShaderPass();
			SCShaderPass basePass = null;
			int pragmaControl = 0;
			int pass = -1;
			SCKeywordLine keywordLine = new SCKeywordLine();
			for (int k = 0; k < shaderLines.Length; k++) {
				string line = shaderLines[k].Trim();
				if (line.Length == 0)
					continue;
				string lineUPPER = line.ToUpper();
				if (lineUPPER.Equals("PASS") || lineUPPER.StartsWith("PASS ")) {
					if (pass >= 0) {
						currentPass.pass = pass;
						if (basePass != null)
							currentPass.Add(basePass.keywordLines);
						shader.Add(currentPass);
					} else if (currentPass.keywordCount > 0) {
						basePass = currentPass;
					}
					currentPass = new SCShaderPass();
					pass++;
					continue;
				}
				int j = line.IndexOf(PRAGMA_COMMENT_MARK);
				if (j >= 0) {
					pragmaControl = 1;
				} else {
					j = line.IndexOf(PRAGMA_DISABLED_MARK);
					if (j >= 0)
						pragmaControl = 3;
				}
				j = line.IndexOf(PRAGMA_MULTICOMPILE);
				if (j >= 0) {
					if (pragmaControl != 2)
						keywordLine = new SCKeywordLine();
					string[] kk = line.Substring(j + 22).Split(separator, StringSplitOptions.RemoveEmptyEntries);
					// Sanitize keywords
					for (int i = 0; i < kk.Length; i++)
						kk[i] = kk[i].Trim();
					// Act on keywords
					switch (pragmaControl) {
						case 1: // Edited by Shader Control line
							shader.editedByShaderControl = true;
                            // Add original keywords to current line
							for (int s = 0; s < kk.Length; s++) {
								keywordLine.Add(shader.GetKeyword(kk[s]));
							}
							pragmaControl = 2;
							break;
						case 2:
                            // check enabled keywords
							keywordLine.DisableKeywords();
							for (int s = 0; s < kk.Length; s++) {
								SCKeyword keyword = keywordLine.GetKeyword(kk[s]);
								if (keyword != null)
									keyword.enabled = true;
							}
							currentPass.Add(keywordLine);
							pragmaControl = 0;
							break;
						case 3: // disabled by Shader Control line
							shader.editedByShaderControl = true;
                            // Add original keywords to current line
							for (int s = 0; s < kk.Length; s++) {
								SCKeyword keyword = shader.GetKeyword(kk[s]);
								keyword.enabled = false;
								keywordLine.Add(keyword);
							}
							currentPass.Add(keywordLine);
							pragmaControl = 0;
							break;
						case 0:
                            // Add keywords to current line
							for (int s = 0; s < kk.Length; s++) {
								keywordLine.Add(shader.GetKeyword(kk[s]));
							}
							currentPass.Add(keywordLine);
							break;
					}
				}
			}
			currentPass.pass = Mathf.Max(pass, 0);
			if (basePass != null)
				currentPass.Add(basePass.keywordLines);
			shader.Add(currentPass);
		}

		void UpdateShaders() {
			if (shaders.Count != 1)
				return;
			VolumetricFogSInfo shader = shaders[0];
			UpdateShader(shader);
		}

		void UpdateShader(VolumetricFogSInfo shader) {

			// Reads and updates shader from disk
			string[] shaderLines = File.ReadAllLines(shader.path);
			string[] separator = new string[] { " " };
			StringBuilder sb = new StringBuilder();
			int pragmaControl = 0;
			shader.editedByShaderControl = false;
			SCKeywordLine keywordLine = new SCKeywordLine();
			for (int k = 0; k < shaderLines.Length; k++) {
				int j = shaderLines[k].IndexOf(PRAGMA_COMMENT_MARK);
				if (j >= 0)
					pragmaControl = 1;
				j = shaderLines[k].IndexOf(PRAGMA_MULTICOMPILE);
				if (j >= 0) {
					if (pragmaControl != 2)
						keywordLine.Clear();
					string[] kk = shaderLines[k].Substring(j + 22).Split(separator, StringSplitOptions.RemoveEmptyEntries);
					// Sanitize keywords
					for (int i = 0; i < kk.Length; i++)
						kk[i] = kk[i].Trim();
					// Act on keywords
					switch (pragmaControl) {
						case 1:
                            // Read original keywords
							for (int s = 0; s < kk.Length; s++) {
								SCKeyword keyword = shader.GetKeyword(kk[s]);
								keywordLine.Add(keyword);
							}
							pragmaControl = 2;
							break;
						case 0:
						case 2:
							if (pragmaControl == 0) {
								for (int s = 0; s < kk.Length; s++) {
									SCKeyword keyword = shader.GetKeyword(kk[s]);
									keywordLine.Add(keyword);
								}
							}
							int kCount = keywordLine.keywordCount;
							int kEnabledCount = keywordLine.keywordsEnabledCount;
							if (kEnabledCount < kCount) {
								// write original keywords
								if (kEnabledCount == 0) {
									sb.Append(PRAGMA_DISABLED_MARK);
								} else {
									sb.Append(PRAGMA_COMMENT_MARK);
								}
								shader.editedByShaderControl = true;
								sb.Append(PRAGMA_MULTICOMPILE);
								if (keywordLine.hasUnderscoreVariant)
									sb.Append(PRAGMA_UNDERSCORE);
								for (int s = 0; s < kCount; s++) {
									SCKeyword keyword = keywordLine.keywords[s];
									sb.Append(keyword.name);
									if (s < kCount - 1)
										sb.Append(" ");
								}
								sb.AppendLine();
							}

							if (kEnabledCount > 0) {
								// Write actual keywords
								sb.Append(PRAGMA_MULTICOMPILE);
								if (keywordLine.hasUnderscoreVariant)
									sb.Append(PRAGMA_UNDERSCORE);
								for (int s = 0; s < kCount; s++) {
									SCKeyword keyword = keywordLine.keywords[s];
									if (keyword.enabled) {
										sb.Append(keyword.name);
										if (s < kCount - 1)
											sb.Append(" ");
									}
								}
								sb.AppendLine();
							}
							pragmaControl = 0;
							break;
					}
				} else {
					sb.AppendLine(shaderLines[k]);
				}
			}

			// Writes modified shader
			File.WriteAllText(shader.path, sb.ToString());

			AssetDatabase.Refresh();

			ScanShader(shader); // Rescan shader
		}

		bool IsFeatureEnabled(string name) {
			if (shaders.Count == 0)
				return false;
			SCKeyword keyword = shaders[0].GetKeyword(name);
			return keyword.enabled;
		}

		void DrawFeatureIsDisabled() {
			GUILayout.Label("(feature disabled in Shader Options)");
		}

		#endregion

		#region Profile handling

		void CreateProfile() {

			VolumetricFogProfile newProfile = ScriptableObject.CreateInstance<VolumetricFogProfile>();
			if (_lightScatteringEnabled.boolValue) newProfile.lightScatteringOverride = true;
			if (_downsampling.intValue > 1) newProfile.downsamplingOverride = true;
			if (_fogVoidRadius.floatValue > 0) newProfile.fogVoidOverride = true;
			newProfile.Save(_fog);

			AssetDatabase.CreateAsset(newProfile, "Assets/VolumetricFogProfile.asset");
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = newProfile;

			_fog.profile = newProfile;
		}


		#endregion


		#region Advanced Options handling

		void ScanAdvancedOptions() {
			if (shaderAdvancedOptionsInfo == null) {
				shaderAdvancedOptionsInfo = new VolumetricFogShaderOptions();
			}
			shaderAdvancedOptionsInfo.ReadOptions();

            pointLightInspectorCount = shaderAdvancedOptionsInfo.GetOptionValue("FOG_MAX_POINT_LIGHTS");
		}

		#endregion


		#region Mask helper functions

		void TestMaskObjects() {
			if (fogMaskTestResultSB == null) fogMaskTestResultSB = new StringBuilder();
			Renderer[] rr = FindObjectsOfType<Renderer>();
			fogMaskTestResultSB.Length = 0;
			fogMaskTestResultSB.Append("The following objects make the geometry mask:\n");
			int count = 0;
			for (int k = 0; k < rr.Length; k++) {
				if (rr[k].gameObject.activeSelf && ((1 << rr[k].gameObject.layer) & _maskLayer.intValue) != 0) {
					if (count > 0) {
						fogMaskTestResultSB.Append(", ");
					}
					fogMaskTestResultSB.Append(rr[k].gameObject.name);
					count++;
				}
			}
			if (count == 0) {
				fogMaskTestResult = "No candidate objects found (check the layer mask).";
			} else {
				fogMaskTestResult = fogMaskTestResultSB.ToString();
			}
		}

		#endregion

	}

}
