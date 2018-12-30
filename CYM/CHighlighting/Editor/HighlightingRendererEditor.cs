using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CYM.Highlighting
{
	[CustomEditor(typeof(HighlightingRenderer), true)]
	public class HighlightingRendererEditor : HighlightingBaseEditor
	{
		// 
		public override void OnInspectorGUI()
		{
			RendererGUI();
		}
	}
}