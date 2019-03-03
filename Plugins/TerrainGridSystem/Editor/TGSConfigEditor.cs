using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TGS {

	[CustomEditor (typeof(TGSConfig))]
	public class TGSConfigEditor : Editor {

		SerializedProperty title, filterTerritories;
		TGSConfig config;

		void OnEnable() {
			title = serializedObject.FindProperty ("title");
			filterTerritories = serializedObject.FindProperty ("filterTerritories");
			config = (TGSConfig)target;
		}


		public override void OnInspectorGUI () {

			serializedObject.Update ();

			EditorGUILayout.Separator ();
			EditorGUILayout.HelpBox ("To load this configuration, just activate this component or call LoadConfiguration() method of this script.", MessageType.Info);
			EditorGUILayout.PropertyField (title);
			EditorGUILayout.PropertyField (filterTerritories);

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Clear Grid")) {
				if (EditorUtility.DisplayDialog ("Clear Grid", "Remove any color/texture from cells and territories?", "Ok", "Cancel")) {
					config.Clear ();
				}
			}
			if (GUILayout.Button ("Reload Config")) {
				config.LoadConfiguration ();
			}
			EditorGUILayout.EndHorizontal ();

			serializedObject.ApplyModifiedProperties ();



		}
	}

}