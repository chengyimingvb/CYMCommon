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
	public class VolumetricFogPosT : MonoBehaviour, IVolumetricFogRenderComponent {

		public VolumetricFog fog { get; set; }

		Material copyOpaqueMat;

		void OnRenderImage(RenderTexture source, RenderTexture destination) {
			if (fog == null || !fog.enabled) {
				Graphics.Blit(source, destination);
				return;
			}

			if (fog.transparencyBlendMode == TRANSPARENT_MODE.None) {
				fog.DoOnRenderImage(source, destination);
			} else {
				RenderTextureDescriptor desc = source.descriptor;
				RenderTexture opaqueFrame = RenderTexture.GetTemporary(desc);
				if (copyOpaqueMat == null) {
					copyOpaqueMat = new Material(Shader.Find("VolumetricFogAndMist/CopyOpaque"));
				}
				copyOpaqueMat.SetFloat("_BlendPower", fog.transparencyBlendPower);
				Graphics.Blit(source, destination, copyOpaqueMat, (fog.computeDepth && fog.downsampling == 1) ? 1 : 0);
				RenderTexture.ReleaseTemporary(opaqueFrame);
			}
		}

		public void DestroySelf() {
			DestroyImmediate(this);
		}

	}

}