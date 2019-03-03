using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TGS {

	[ExecuteInEditMode]
	public class TGSConfig : MonoBehaviour {

		[Tooltip ("User-defined name for this configuration")]
		[TextArea]
		public string title = "Optionally name this configuration editing this text.";

		[Tooltip ("Enter a comma separated list of territory indices to use from this configuration. Leave this field in black to restore all territories.")]
		public string filterTerritories;

		[HideInInspector]
		public string config;

		[HideInInspector]
		public Texture2D[] textures;

		// Use this for initialization
		void OnEnable () {
			if (!Application.isPlaying)
				LoadConfiguration ();
		}

		void Start () {
			LoadConfiguration ();
		}


		public void Clear() {
			TerrainGridSystem tgs = GetTGS ();
			if (tgs != null)
				tgs.ClearAll ();
		}


		/// <summary>
		/// Call this method to force a configuration load.
		/// </summary>
		public void LoadConfiguration () {
			if (config == null)
				return;

			TerrainGridSystem tgs = GetTGS ();
			if (tgs == null) {
				return;
			}
			tgs.textures = textures;
			int[] territories = null;
			if (!string.IsNullOrEmpty (filterTerritories)) {
				string[] ss = filterTerritories.Split (new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
				List<int> tt = new List<int> ();
				for (int k = 0; k < ss.Length; k++) {
					int v = 0;
					if (int.TryParse (ss [k], out v)) {
						tt.Add (v);
					}
				}
				territories = tt.ToArray ();
			}
			tgs.CellSetConfigurationData (config, territories);
		}

		TerrainGridSystem GetTGS() {
			TerrainGridSystem tgs = GetComponent<TerrainGridSystem> ();
			if (tgs == null) {
				Debug.Log ("Terrain Grid System not found in this game object!");
				return null;
			}
			return tgs;
		}

	}

}