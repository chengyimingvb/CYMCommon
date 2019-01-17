using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AwesomeTechnologies.ColliderSystem
{
	[CustomEditor(typeof(ColliderSystemPro))]
	public class ColliderSystemProEditor : VegetationStudioProBaseEditor
	{
		private ColliderSystemPro _colliderSystemPro;
		
		private static readonly string[] TabNames =
		{
			"Settings", "Debug"
		};

		public override void OnInspectorGUI()
		{
			_colliderSystemPro = (ColliderSystemPro) target;
			OverrideLogoTextureName = "Banner_ColliderSystem";
			LargeLogo = false;
			base.OnInspectorGUI();
			
			EditorGUI.BeginChangeCheck();
			_colliderSystemPro.CurrentTabIndex = GUILayout.SelectionGrid(_colliderSystemPro.CurrentTabIndex, TabNames, 2, EditorStyles.toolbarButton);
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
				SetSceneDirty();
			}
			switch (_colliderSystemPro.CurrentTabIndex)
			{
				case 0:
					DrawSettingsInspector();
					break;       
				case 1:
					DrawDebugInspector();
					break;               
			}         			
		}

		private void DrawNavmeshInspector()
		{
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Navmesh helper tools", LabelStyle);
			EditorGUILayout.HelpBox("The bake colliders to scene option allows you to create GameObjects in the scene for all configured colliders. This can be used to bake unitys navmesh. After baking the GameObjects can be removed from the scene.", MessageType.Info);
			if (GUILayout.Button("Refresh colliders to scene"))
			{

			}
			EditorGUILayout.HelpBox("Collider baking is not enabled yet.", MessageType.Warning);
			GUILayout.EndVertical();		
		}

		private void DrawDebugInspector()
		{
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Debug info", LabelStyle);
			EditorGUI.BeginChangeCheck();
			_colliderSystemPro.ShowDebugCells = EditorGUILayout.Toggle("Show visible vegetation cells", _colliderSystemPro.ShowDebugCells);
			if (EditorGUI.EndChangeCheck())
			{
				SceneView.RepaintAll();
				SetSceneDirty();
			}
			GUILayout.EndVertical();
			
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Runtime info", LabelStyle);

			if (_colliderSystemPro.VisibleVegetationCellSelector != null)
			{					
				EditorGUILayout.LabelField("Visible cells: " + _colliderSystemPro.VisibleVegetationCellSelector.VisibleSelectorVegetationCellList.Count.ToString(), LabelStyle);
				
				EditorGUILayout.LabelField("Loaded instances: " + _colliderSystemPro.GetLoadedInstanceCount(), LabelStyle);
				EditorGUILayout.LabelField("Visible colliders: " + _colliderSystemPro.GetVisibleColliders(), LabelStyle);	
				if (GUILayout.Button("Refresh"))
				{

				}
			}
			else
			{
				EditorGUILayout.HelpBox("Colliders run-time info only show in playmode.", MessageType.Info);
			}			
			GUILayout.EndVertical();							
		}	

		private void DrawSettingsInspector()
		{
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("General info", LabelStyle);
			EditorGUILayout.HelpBox("When enabled the collider system will create colliders for objects and trees around the assigned cameras. Collider settings are on each vegetation item.", MessageType.Info);
			EditorGUILayout.HelpBox("Colliders are created in playmode and builds", MessageType.Info);
			GUILayout.EndVertical();	
			
			GUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Visibility", LabelStyle);
			EditorGUI.BeginChangeCheck();
			_colliderSystemPro.ShowColliders = EditorGUILayout.Toggle("Show colliders", _colliderSystemPro.ShowColliders);
			if (EditorGUI.EndChangeCheck())
			{
				_colliderSystemPro.SetColliderVisibility(_colliderSystemPro.ShowColliders);
				SceneView.RepaintAll();
				SetSceneDirty();
				EditorApplication.RepaintHierarchyWindow();
			}
			
			GUILayout.EndVertical();	
		}

		private void SetSceneDirty()
		{
			if (Application.isPlaying) return;
			EditorSceneManager.MarkSceneDirty(_colliderSystemPro.gameObject.scene);
			EditorUtility.SetDirty(_colliderSystemPro);
		}
	}
}
