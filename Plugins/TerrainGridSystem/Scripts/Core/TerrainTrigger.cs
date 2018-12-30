using UnityEngine;
using System.Collections;

namespace TGS {
	public class TerrainTrigger : MonoBehaviour {

		// Use this for initialization
		TerrainGridSystem[] tgs;
		RaycastHit[] hits;

		void OnEnable () {
			hits = new RaycastHit[20];
		}

		void Start () {
			if (GetComponent<TerrainCollider> () == null) {
				gameObject.AddComponent<TerrainCollider> ();
			}
			tgs = transform.GetComponentsInChildren<TerrainGridSystem> ();
			if (tgs == null || tgs.Length == 0) {
				Debug.LogError ("Missing Terrain Highlight System reference in Terrain Trigger script.");
				DestroyImmediate (this);
			}
		}

		void OnMouseEnter () {
			for (int k = 0; k < tgs.Length; k++) {
				if (tgs [k] != null) {
					tgs [k].mouseIsOver = true;
				}
			}
		}

		void OnMouseExit () {
			// Make sure it's outside of grid
			Vector3 mousePos = Input.mousePosition;
			Camera cam = tgs [0].cameraMain;
			Ray ray = cam.ScreenPointToRay (mousePos);
			int hitCount = Physics.RaycastNonAlloc (cam.transform.position, ray.direction, hits);
			if (hitCount > 0) {
				for (int k = 0; k < hitCount; k++) {
					if (tgs [0] == null || hits [k].collider.gameObject == this.tgs [0].terrain.gameObject)
						return; 
				}
			}
			for (int k = 0; k < tgs.Length; k++) {
				if (tgs [k] != null) {
					tgs [k].mouseIsOver = false;
				}
			}
		}

	}

}