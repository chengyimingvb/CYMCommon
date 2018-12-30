using UnityEngine;
using UnityEditor;
using FoW;

[CustomEditor(typeof(FogOfWar))]
public class FogOfWarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (Application.isPlaying && GUILayout.Button("Reinitialize"))
            ((FogOfWar)target).Reinitialize();
    }
}
