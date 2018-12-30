using UnityEngine;
using UnityEditor;

namespace FoW
{
    [CustomEditor(typeof(FogOfWarChunkManager))]
    public class FogOfWarChunkManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            FogOfWarChunkManager cm = (FogOfWarChunkManager)target;
            FogOfWar fow = cm.GetComponent<FogOfWar>();

            if (fow.mapResolution.x != fow.mapResolution.y)
                EditorGUILayout.HelpBox("Map Resolution must be square!", MessageType.Error);
            if (fow.mapResolution.x % 2 != 0)
                EditorGUILayout.HelpBox("Map Resolution must be divisible by 2!", MessageType.Error);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(string.Format("Chunks Loaded: {0}\nMemory Usage {0:0.0}kb", cm.loadedChunkCount, cm.loadedChunkCount * fow.mapResolution.manhattanMagnitude / 1024.0f), MessageType.None);
                if (GUILayout.Button("Clear Memory"))
                    cm.Clear();
            }
        }
    }
}
