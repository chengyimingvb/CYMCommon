using UnityEngine;
using System.Collections;

namespace CYM.Highlighting
{
	static public class ShaderPropertyID
	{
		#region PUBLIC FIELDS
		// Common
		static public int _MainTex { get; private set; }
		static public int _Color { get; private set; }

		// HighlightingSystem
		static public int _Cutoff { get; private set; }
		static public int _Intensity { get; private set; }
		static public int _ZTest { get; private set; }
		static public int _StencilRef { get; private set; }
		static public int _Cull { get; private set; }
		static public int _HighlightingBlur1 { get; private set; }
		static public int _HighlightingBlur2 { get; private set; }
		static public int _HighlightingBuffer { get; private set; }
		static public int _HighlightingBufferTexelSize { get; private set; }

		// HighlightingSystem global shader properties. Should be unique!
		static public int _HighlightingBlurred { get; private set; }
		static public int _HighlightingBlurOffset { get; private set; }
		static public int _HighlightingZWrite { get; private set; }
		static public int _HighlightingOffsetFactor { get; private set; }
		static public int _HighlightingOffsetUnits { get; private set; }
		#endregion

		#region PRIVATE FIELDS
		static private bool initialized = false;
		#endregion

		// 
		static public void Initialize()
		{
			if (initialized) { return; }

			_MainTex = Shader.PropertyToID("_MainTex");
			_Color = Shader.PropertyToID("_Color");

			_Cutoff = Shader.PropertyToID("_Cutoff");
			_Intensity = Shader.PropertyToID("_Intensity");
			_ZTest = Shader.PropertyToID("_ZTest");
			_StencilRef = Shader.PropertyToID("_StencilRef");
			_Cull = Shader.PropertyToID("_Cull");
			_HighlightingBlur1 = Shader.PropertyToID("_HighlightingBlur1");
			_HighlightingBlur2 = Shader.PropertyToID("_HighlightingBlur2");
			_HighlightingBuffer = Shader.PropertyToID("_HighlightingBuffer");
			_HighlightingBufferTexelSize = Shader.PropertyToID("_HighlightingBufferTexelSize");

			_HighlightingBlurred = Shader.PropertyToID("_HighlightingBlurred");
			_HighlightingBlurOffset = Shader.PropertyToID("_HighlightingBlurOffset");
			_HighlightingZWrite = Shader.PropertyToID("_HighlightingZWrite");
			_HighlightingOffsetFactor = Shader.PropertyToID("_HighlightingOffsetFactor");
			_HighlightingOffsetUnits = Shader.PropertyToID("_HighlightingOffsetUnits");

			initialized = true;
		}
	}
}