//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace VolumetricFogAndMist {

	[ExecuteInEditMode][AddComponentMenu("")]
	[RequireComponent(typeof(Camera), typeof(VolumetricFog))]
	public class VolumetricFogPreT : MonoBehaviour, IVolumetricFogRenderComponent {

		public VolumetricFog fog { get; set; }

		RenderTexture opaqueFrame;

		[ImageEffectOpaque]
		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (fog == null || !fog.enabled) {
				Graphics.Blit(source, destination);
				return;
			}

			if (fog.renderBeforeTransparent) {
				fog.DoOnRenderImage(source, destination);
			} else {
				// Save frame buffer
				RenderTextureDescriptor desc = source.descriptor;
				opaqueFrame = RenderTexture.GetTemporary(desc);
				fog.DoOnRenderImage(source, opaqueFrame);
				Shader.SetGlobalTexture("_VolumetricFog_OpaqueFrame", opaqueFrame);
				Graphics.Blit(opaqueFrame, destination);
			}

		}

		void OnPostRender() {
			if (opaqueFrame != null) {
				RenderTexture.ReleaseTemporary(opaqueFrame);
				opaqueFrame = null;
			}
		}

		public void DestroySelf() {
			DestroyImmediate(this);
		}


	}

}