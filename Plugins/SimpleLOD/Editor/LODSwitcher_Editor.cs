/* SimpleLOD 1.6     */
/* By Orbcreation BV */
/* Richard Knol      */
/* Mar 11, 2016      */

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(LODSwitcher))]

public class LODSwitcher_Editor : Editor {
	bool moveCameWithLOD = false;
	Vector3 oldCamPosition = Vector3.zero;

	public override void OnInspectorGUI () {
		LODSwitcher switcher = target as LODSwitcher;
		base.OnInspectorGUI(); // Show default GUI

		// And add a field to test the LOD level
		int oldLevel = switcher.GetLODLevel();
		if(!Application.isPlaying) {
			int newLevel = EditorGUILayout.IntSlider("LOD level", oldLevel, 0, switcher.MaxLODLevel());
			if(newLevel != oldLevel) {
				switcher.SetLODLevel(newLevel);
				if(moveCameWithLOD) {
					if(newLevel > 0) Camera.main.transform.position = switcher.NearestCameraPositionForLOD(newLevel);
					else Camera.main.transform.position = oldCamPosition;
				}
			}
		} else {
			EditorGUILayout.IntField("Currently using LOD level", oldLevel);
		}

		if(!Application.isPlaying) {
			bool newBool = EditorGUILayout.Toggle("Move camera with LOD", moveCameWithLOD);
			if(newBool != moveCameWithLOD) {
				moveCameWithLOD = newBool;
				if(moveCameWithLOD) {
					oldCamPosition = Camera.main.transform.position;
					int lodLevel = switcher.GetLODLevel();
					if(lodLevel > 0) {
						Camera.main.transform.position = switcher.NearestCameraPositionForLOD(lodLevel);
					}
				} else {
					Camera.main.transform.position = oldCamPosition;
				}
			}
		}
	}
}