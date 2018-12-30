#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CYM.Utile
{
	[CustomEditor(typeof(AFPSRenderRecorder))]
	[CanEditMultipleObjects()]
	public class AFPSRenderRecorderEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			EditorUIUtils.SetupStyles();
			GUILayout.Label("This component is used by <b>Advanced FPS Counter</b> to measure camera <b>Render Time</b>.", EditorUIUtils.richMiniLabel);
		}
	}
}
#endif