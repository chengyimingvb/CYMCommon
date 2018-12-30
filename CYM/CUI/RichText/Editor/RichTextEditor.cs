using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
using CYM.UI;

namespace CYM.UI
{
	[CustomEditor(typeof(RichText), true), CanEditMultipleObjects]
	public class RichTextEditor : GraphicEditor
	{
		private SerializedProperty m_Content;
		private SerializedProperty m_FontData;
        private SerializedProperty m_SpriteGroupsSize;
        //private SerializedProperty m_SpriteGroups;
		private SerializedProperty m_UsedEffects;
		private SerializedProperty m_UnusedEffects;
		private SerializedProperty m_OnLinkProperty;
        private SerializedProperty m_EnableEmoji;
        private SerializedProperty m_EnableDynamicStr;
        private SerializedProperty m_EnableDynamicImage;
        //private SerializedProperty m_DynamicFuncMono;

        protected static string VerticalStyle = "HelpBox";

        protected override void OnEnable()
		{
			base.OnEnable();
			m_Content = serializedObject.FindProperty("m_Content");
			m_FontData = serializedObject.FindProperty("m_FontData");
			m_UsedEffects = serializedObject.FindProperty("UsedEffects");
			m_UnusedEffects = serializedObject.FindProperty("UnusedEffects");
			//m_SpriteGroups = serializedObject.FindProperty("SpriteGroups");
            m_SpriteGroupsSize = serializedObject.FindProperty("SpriteSizeFaction");
            m_EnableEmoji = serializedObject.FindProperty("EnableEmoji");
            m_EnableDynamicStr = serializedObject.FindProperty("EnableDynamicStr");
            m_EnableDynamicImage = serializedObject.FindProperty("EnableDynamicImage");
            //m_DynamicFuncMono = serializedObject.FindProperty("DynamicFuncMono");
            m_OnLinkProperty = serializedObject.FindProperty("onLink");
		}

		void SetHideFlags(SerializedProperty effects)
		{
			for (var i = 0; i < effects.arraySize; i++) {
				var sp = effects.GetArrayElementAtIndex(i);
				var effect = sp.objectReferenceValue as TextEffect;
				if (effect != null) {
					effect.hideFlags = HideFlags.HideInInspector;
				}
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.PropertyField(m_Content, new GUIContent("Text"), new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(m_FontData, new GUILayoutOption[0]);
			AppearanceControlsGUI();
			RaycastControlsGUI();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(VerticalStyle);
            EditorGUILayout.PropertyField(m_EnableEmoji, true, new GUILayoutOption[0]);
            if (m_EnableEmoji.boolValue)
            {
                EditorGUILayout.PropertyField(m_SpriteGroupsSize, true, new GUILayoutOption[0]);
                //EditorGUILayout.PropertyField(m_SpriteGroups, true, new GUILayoutOption[0]);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(VerticalStyle);
            EditorGUILayout.PropertyField(m_EnableDynamicStr, true, new GUILayoutOption[0]);
            if (m_EnableDynamicStr.boolValue)
            {

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical(VerticalStyle);
            EditorGUILayout.PropertyField(m_EnableDynamicImage, true, new GUILayoutOption[0]);
            if (m_EnableDynamicImage.boolValue)
            {

            }
            EditorGUILayout.EndVertical();

            //EditorGUILayout.PropertyField(m_DynamicFuncMono, true, new GUILayoutOption[0]);
            
            EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_OnLinkProperty, new GUILayoutOption[0]);

			SetHideFlags(m_UsedEffects);
			SetHideFlags(m_UnusedEffects);

			serializedObject.ApplyModifiedProperties();
		}

		static void AddRichText(MenuCommand menuCommand)
		{
			var CreateUIElementRoot = typeof(DefaultControls).GetMethod("CreateUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			var gameObject = CreateUIElementRoot.Invoke(null, new object[] { "RichText", new Vector2(160f, 30f) }) as GameObject;
			var richText = gameObject.AddComponent<RichText>();
			richText.text = "New Text";

			Assembly assembly = Assembly.Load("UnityEditor.UI");
			var type = assembly.GetType("UnityEditor.UI.MenuOptions");
			var PlaceUIElementRoot = type.GetMethod("PlaceUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			PlaceUIElementRoot.Invoke(null, new object[] { gameObject, menuCommand });
		}
	}
}
