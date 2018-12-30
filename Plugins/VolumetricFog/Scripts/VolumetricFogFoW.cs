//------------------------------------------------------------------------------------------------------------------
// Volumetric Fog & Mist
// Created by Ramiro Oliva (Kronnect)
//------------------------------------------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace VolumetricFogAndMist {

	public partial class VolumetricFog : MonoBehaviour {

		const int MAX_SIMULTANEOUS_TRANSITIONS = 10000;

		#region Fog of War settings

		[SerializeField]
		bool _fogOfWarEnabled;

		public bool fogOfWarEnabled {
			get { return _fogOfWarEnabled; }
			set {
				if (value != _fogOfWarEnabled) {
					_fogOfWarEnabled = value;
					FogOfWarUpdateTexture();
					UpdateMaterialProperties();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		Vector3 _fogOfWarCenter;

		public Vector3 fogOfWarCenter {
			get { return _fogOfWarCenter; }
			set {
				if (value != _fogOfWarCenter) {
					_fogOfWarCenter = value;
					UpdateMaterialProperties();
					isDirty = true;
				}
			}
		}

		[SerializeField]
		Vector3 _fogOfWarSize = new Vector3(1024, 0, 1024);

		public Vector3 fogOfWarSize {
			get { return _fogOfWarSize; }
			set {
				if (value != _fogOfWarSize) {
					if (value.x > 0 && value.z > 0) {
						_fogOfWarSize = value;
						UpdateMaterialProperties();
						isDirty = true;
					}
				}
			}
		}

		[SerializeField, Range(32, 2048)]
		int _fogOfWarTextureSize = 256;

		public int fogOfWarTextureSize {
			get { return _fogOfWarTextureSize; }
			set {
				if (value != _fogOfWarTextureSize) {
					if (value > 16) {
						_fogOfWarTextureSize = value;
						FogOfWarUpdateTexture();
						UpdateMaterialProperties();
						isDirty = true;
					}
				}
			}
		}

		[SerializeField, Range(0, 100)]
		float _fogOfWarRestoreDelay = 0;

		public float fogOfWarRestoreDelay {
			get { return _fogOfWarRestoreDelay; }
			set {
				if (value != _fogOfWarRestoreDelay) {
					_fogOfWarRestoreDelay = value;
					isDirty = true;
				}
			}
		}

		
		[SerializeField, Range(0, 25)]
		float _fogOfWarRestoreDuration = 2f;

		public float fogOfWarRestoreDuration {
			get { return _fogOfWarRestoreDuration; }
			set {
				if (value != _fogOfWarRestoreDuration) {
					_fogOfWarRestoreDuration = value;
					isDirty = true;
				}
			}
		}


		#endregion

		Texture2D fogOfWarTexture;
		Color32[] fogOfWarColorBuffer;

		struct FogOfWarTransition {
			public bool enabled;
			public int x, y;
			public float startTime, startDelay;
			public float duration;
			public byte initialAlpha;
			public byte targetAlpha;
		}

		FogOfWarTransition[] fowTransitionList;
		int lastTransitionPos;
		Dictionary<int, int> fowTransitionIndices;
		bool requiresTextureUpload;

		#region Fog Of War

		void FogOfWarInit() {
			fowTransitionList = new FogOfWarTransition[MAX_SIMULTANEOUS_TRANSITIONS];
			fowTransitionIndices = new Dictionary<int, int>(MAX_SIMULTANEOUS_TRANSITIONS);
			lastTransitionPos = -1;
		}

		void FogOfWarUpdateTexture() {
			if (!_fogOfWarEnabled)
				return;
			int size = GetScaledSize(_fogOfWarTextureSize, 1.0f);
			if (fogOfWarTexture == null || fogOfWarTexture.width != size || fogOfWarTexture.height != size) {
				fogOfWarTexture = new Texture2D(size, size, TextureFormat.Alpha8, false);
				fogOfWarTexture.hideFlags = HideFlags.DontSave;
				fogOfWarTexture.filterMode = FilterMode.Bilinear;
				fogOfWarTexture.wrapMode = TextureWrapMode.Clamp;
				ResetFogOfWar();
			}
		}

		
		void FogOfWarUpdate() {
			if (!_fogOfWarEnabled)
				return;
			int tw = fogOfWarTexture.width;
			for (int k = 0; k <= lastTransitionPos; k++) {
				FogOfWarTransition fw = fowTransitionList[k];
				if (!fw.enabled)
					continue;
				float elapsed = Time.time - fw.startTime - fw.startDelay;
				if (elapsed > 0) {
					float t = fw.duration <= 0 ? 1 : elapsed / fw.duration;
					t = Mathf.Clamp01(t);
					byte alpha = (byte)Mathf.Lerp(fw.initialAlpha, fw.targetAlpha, t);
					int colorPos = fw.y * tw + fw.x;
					fogOfWarColorBuffer[colorPos].a = alpha;
					fogOfWarTexture.SetPixel(fw.x, fw.y, fogOfWarColorBuffer[colorPos]);
					requiresTextureUpload = true;
					if (t >= 1f) {
						fowTransitionList[k].enabled = false;
						// Add refill slot if needed
						if (fw.targetAlpha < 255 && _fogOfWarRestoreDelay > 0) {
							AddFogOfWarTransitionSlot(fw.x, fw.y, fw.targetAlpha, 255, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
						}
					}
				}
			}
			if (requiresTextureUpload) {
				requiresTextureUpload = false;
				fogOfWarTexture.Apply();
			}
		}

		/// <summary>
		/// Instantly changes the alpha value of the fog of war at world position. It takes into account FogOfWarCenter and FogOfWarSize.
		/// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
		/// </summary>
		/// <param name="worldPosition">in world space coordinates.</param>
		/// <param name="radius">radius of application in world units.</param>
		public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha) {
			SetFogOfWarAlpha(worldPosition, radius, fogNewAlpha, 1f);
		}


		/// <summary>
		/// Changes the alpha value of the fog of war at world position creating a transition from current alpha value to specified target alpha. It takes into account FogOfWarCenter and FogOfWarSize.
		/// Note that only x and z coordinates are used. Y (vertical) coordinate is ignored.
		/// </summary>
		/// <param name="worldPosition">in world space coordinates.</param>
		/// <param name="radius">radius of application in world units.</param>
		/// <param name="fogNewAlpha">target alpha value.</param>
		/// <param name="duration">duration of transition in seconds (0 = apply fogNewAlpha instantly).</param>
		public void SetFogOfWarAlpha(Vector3 worldPosition, float radius, float fogNewAlpha, float duration) {
			if (fogOfWarTexture == null)
				return;

			float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
			if (tx < 0 || tx > 1f)
				return;
			float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
			if (tz < 0 || tz > 1f)
				return;

			int tw = fogOfWarTexture.width;
			int th = fogOfWarTexture.height;
			int px = (int)(tx * tw);
			int pz = (int)(tz * th);
			int colorBufferPos = pz * tw + px;
			byte newAlpha8 = (byte)(fogNewAlpha * 255);
			Color32 existingColor = fogOfWarColorBuffer[colorBufferPos];
			if (newAlpha8 != existingColor.a) { // just to avoid over setting the texture in an Update() loop
				float tr = radius / _fogOfWarSize.z;
				int delta = (int)(th * tr);
				for (int r = pz - delta; r <= pz + delta; r++) {
					if (r > 0 && r < th - 1) {
						for (int c = px - delta; c <= px + delta; c++) {
							if (c > 0 && c < tw - 1) {
								int distance = (int)(Mathf.Sqrt((pz - r) * (pz - r) + (px - c) * (px - c)));
								if (distance <= delta) {
									colorBufferPos = r * tw + c;
									Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
									byte targetAlpha = (byte)Mathf.Lerp(newAlpha8, colorBuffer.a, (float)distance / delta);
									if (targetAlpha < 255) {
										if (duration > 0) {
											AddFogOfWarTransitionSlot(c, r, colorBuffer.a, targetAlpha, 0, duration);
										} else {
											colorBuffer.a = targetAlpha;
											fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
											fogOfWarTexture.SetPixel(c, r, colorBuffer);
											requiresTextureUpload = true;
											if (_fogOfWarRestoreDuration > 0) {
												AddFogOfWarTransitionSlot(c, r, targetAlpha, 255, _fogOfWarRestoreDelay, _fogOfWarRestoreDuration);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public void ResetFogOfWarAlpha(Vector3 worldPosition, float radius) {
			if (fogOfWarTexture == null)
				return;
			
			float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
			if (tx < 0 || tx > 1f)
				return;
			float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
			if (tz < 0 || tz > 1f)
				return;
			
			int tw = fogOfWarTexture.width;
			int th = fogOfWarTexture.height;
			int px = (int)(tx * tw);
			int pz = (int)(tz * th);
			int colorBufferPos = pz * tw + px;
			float tr = radius / _fogOfWarSize.z;
			int delta = (int)(th * tr);
			int deltaSqr = delta * delta;
			for (int r = pz - delta; r <= pz + delta; r++) {
				if (r > 0 && r < th - 1) {
					for (int c = px - delta; c <= px + delta; c++) {
						if (c > 0 && c < tw - 1) {
							int distanceSqr = (pz - r) * (pz - r) + (px - c) * (px - c);
							if (distanceSqr <= deltaSqr) {
								colorBufferPos = r * tw + c;
								Color32 colorBuffer = fogOfWarColorBuffer[colorBufferPos];
								colorBuffer.a = 255;
								fogOfWarColorBuffer[colorBufferPos] = colorBuffer;
								fogOfWarTexture.SetPixel(c, r, colorBuffer);
							}
						}
					}
				}
				requiresTextureUpload = true;
			}
		}


		public void ResetFogOfWar() {
			if (fogOfWarTexture == null || !isPartOfScene)
				return;
			int h = fogOfWarTexture.height;
			int w = fogOfWarTexture.width;
			int newLength = h * w;
			if (fogOfWarColorBuffer == null || fogOfWarColorBuffer.Length != newLength) {
				fogOfWarColorBuffer = new Color32[newLength];
			}
			Color32 opaque = new Color32(255, 255, 255, 255);
			for (int k = 0; k < newLength; k++)
				fogOfWarColorBuffer[k] = opaque;
			fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
			fogOfWarTexture.Apply();
			lastTransitionPos = -1;
			fowTransitionIndices.Clear();
			isDirty = true;
		}

		/// <summary>
		/// Gets or set fog of war state as a Color32 buffer. The alpha channel stores the transparency of the fog at that position (0 = no fog, 1 = opaque).
		/// </summary>
		public Color32[] fogOfWarTextureData { 
			get { 
				return fogOfWarColorBuffer;
			} 
			set {
				fogOfWarEnabled = true;
				fogOfWarColorBuffer = value;
				if (value == null || fogOfWarTexture == null)
					return;
				if (value.Length != fogOfWarTexture.width * fogOfWarTexture.height)
					return;
				fogOfWarTexture.SetPixels32(fogOfWarColorBuffer);
				fogOfWarTexture.Apply();
			}
		}

        void AddFogOfWarTransitionSlot(int x, int y, byte initialAlpha, byte targetAlpha, float delay, float duration) {

			// Check if this slot exists
			int index;
			int key = y * 64000 + x;

			if (!fowTransitionIndices.TryGetValue(key, out index)) {
				index = -1;
				for (int k = 0; k <= lastTransitionPos; k++) {
					if (!fowTransitionList[k].enabled) {
						index = k;
						fowTransitionIndices[key] = index;
						break;
					}
				}
			}
			if (index >= 0) {
				if (fowTransitionList[index].enabled && (fowTransitionList[index].x != x || fowTransitionList[index].y != y)) {
					index = -1;
				}
			}

			if (index < 0) {
                if (lastTransitionPos >= MAX_SIMULTANEOUS_TRANSITIONS - 1)
                    return;
				index = ++lastTransitionPos;
				fowTransitionIndices[key] = index;
			}
											
			fowTransitionList[index].x = x;
			fowTransitionList[index].y = y;
			fowTransitionList[index].duration = duration;
			fowTransitionList[index].startTime = Time.time;
			fowTransitionList[index].startDelay = delay;
			fowTransitionList[index].initialAlpha = initialAlpha;
			fowTransitionList[index].targetAlpha = targetAlpha;
			fowTransitionList[index].enabled = true;
		}


		/// <summary>
		/// Gets the current alpha value of the Fog of War at a given world position
		/// </summary>
		/// <returns>The fog of war alpha.</returns>
		/// <param name="worldPosition">World position.</param>
		public float GetFogOfWarAlpha(Vector3 worldPosition) {
			if (fogOfWarColorBuffer == null)
				return 1f;

			float tx = (worldPosition.x - _fogOfWarCenter.x) / _fogOfWarSize.x + 0.5f;
			if (tx < 0 || tx > 1f)
				return 1f;
			float tz = (worldPosition.z - _fogOfWarCenter.z) / _fogOfWarSize.z + 0.5f;
			if (tz < 0 || tz > 1f)
				return 1f;

			int tw = fogOfWarTexture.width;
			int th = fogOfWarTexture.height;
			int px = (int)(tx * tw);
			int pz = (int)(tz * th);
			int colorBufferPos = pz * tw + px;
			if (colorBufferPos < 0 || colorBufferPos >= fogOfWarColorBuffer.Length)
				return 1f;
			return fogOfWarColorBuffer [colorBufferPos].a / 255f;
		}

		#endregion
	}

}