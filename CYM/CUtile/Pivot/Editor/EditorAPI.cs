using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CYM.Utile
{
    public class EditorAPI
    {
        /// <summary>
        /// Called when the user starts to change the pivot position in Editor
        /// Simple function: no access to Transform being modified
        /// </summary>
        public static event System.Action onChangeBeginSimple;

        /// <summary>
        /// Called when the user starts to change the pivot position in Editor
        /// Complex function: full access of all Transforms being modified
        /// </summary>
        public static event System.Action<Transform[]> onChangeBeginArray;

        /// <summary>
        /// Called when the user stops to change the pivot position in Editor
        /// </summary>
        public static event System.Action onChangeEnd;

        TargetManager targetManager = new TargetManager();
        Tool m_prevTool;
        bool m_Snap = false;
        float m_SnapGridSize = 1f;
        const string m_EditorPrefsPrefix = "SuperPivot_";

        static GUIContent ms_SnapGUIContent = new GUIContent("Snap", "Snap to a virtual grid while moving the pivot with gizmos handlers in the scene view.");

        public event System.Action askToRepaintWindowDelegate;

        static public Transform[] GetSelectedTransforms()
        {
            return Selection.GetTransforms(SelectionMode.ExcludePrefab);
        }

        void AskToRepaintWindow()
        {
            if (askToRepaintWindowDelegate != null)
                askToRepaintWindowDelegate();
        }

        void SetTargets(Transform[] targets)
        {
            var wasEmpty = targetManager.isEmpty;
            targetManager.SetTargets(targets);

            if (targetManager.isEmpty)
            {
                if(!wasEmpty)
                    if (onChangeEnd != null) onChangeEnd();
            }
            else
            {
                if (onChangeBeginSimple != null) onChangeBeginSimple();
                if (onChangeBeginArray != null) onChangeBeginArray(targetManager.GetTransformTargets());
            }
        }

        void ResetTargets()
        {
            SetTargets(null);
        }

        bool StartMove()
        {
            StopMove();
            StartListeningSceneGUI();

            SetTargets(GetSelectedTransforms());
            if (!targetManager.isEmpty)
            {
                m_prevTool = Tools.current;
                Tools.current = Tool.None;
                SceneView.RepaintAll();
            }
            AskToRepaintWindow();
            return !targetManager.isEmpty;
        }

        public void StopMove()
        {
            StopListeningSceneGUI();
            if (!targetManager.isEmpty)
                Tools.current = m_prevTool;

            ResetTargets();
            AskToRepaintWindow();
        }

        void OnSelectionChanged()
        {
            StopMove();
        }

        public void DrawGUI(Transform[] selectedTransforms)
        {
            if (targetManager.isEmpty)
            {
                if (GUILayout.Button("Move Pivot"))
                    StartMove();

                if (!string.IsNullOrEmpty(targetManager.errorMsg))
                    EditorGUILayout.LabelField(targetManager.errorMsg);
            }
            else
            {
                if (GUILayout.Button("Stop Moving Pivot"))
                {
                    Tools.current = m_prevTool;
                    ResetTargets();
                    SceneView.RepaintAll();
                }

                if (Tools.pivotMode == PivotMode.Center)
                {
                    EditorGUILayout.Separator();
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.HelpBox("Be careful, the Unity tool handle is in 'Center' mode, which may be confusing.\nWe recommend you to work in 'Pivot' mode instead.", MessageType.Warning, true);
                        if (GUILayout.Button("Switch to\n'Pivot' mode"))
                            Tools.pivotMode = PivotMode.Pivot;
                    }
                    EditorGUILayout.Separator();
                }

                if (!targetManager.isEmpty)
                {
                    GUISelection();

                    if (targetManager.isUnique)
                    {
                        var uniqueTarget = targetManager.uniqueTarget;
                        uniqueTarget.GUIWorldPosition();
                        uniqueTarget.GUILocalPosition();
                        uniqueTarget.GUIChildren();
                    }

                    targetManager.GUIBounds();


                    if (targetManager.isUnique)
                    {
                        EditorGUILayout.Separator();
                        {
                            EditorGUILayout.LabelField("Scene view", EditorStyles.boldLabel);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUIUtility.labelWidth = 50f;

                                m_Snap = EditorGUILayout.Toggle(ms_SnapGUIContent, EditorPrefs.GetBool(m_EditorPrefsPrefix + "SnapToggle", m_Snap));
                                EditorPrefs.SetBool(m_EditorPrefsPrefix + "SnapToggle", m_Snap);

                                m_SnapGridSize = EditorGUILayout.FloatField(EditorPrefs.GetFloat(m_EditorPrefsPrefix + "SnapSize", m_SnapGridSize));
                                EditorPrefs.SetFloat(m_EditorPrefsPrefix + "SnapSize", m_SnapGridSize);
                            }
                        }
                    }
                }
            }
        }

        void GUISelection()
        {
            string header = "";
            if (targetManager.isUnique)
                header = string.Format("Currently moving '{0}\' pivot", targetManager.uniqueTarget.name);
            else
                header = string.Format("Currently moving pivot of {0} objects: ", targetManager.count);
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

            if (!targetManager.isUnique)
            {
                const int kMax = 20;
                GUI.enabled = false;
                string names = "";
                int count = 0;
                foreach (var target in targetManager.EveryTarget())
                {
                    if (count >= kMax)
                    {
                        names += "...";
                        break;
                    }

                    names += target.name;
                    if (count < targetManager.count - 1) names += ", ";
                    count++;
                }

                var oldWordWrap = EditorStyles.textField.wordWrap;
                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(names);
                EditorStyles.textField.wordWrap = oldWordWrap;
                GUI.enabled = true;
            }

            EditorGUILayout.Separator();
        }

        Vector3 GetSnappedPosition(Vector3 pivotPos)
        {
            if (m_Snap)
                pivotPos = new Vector3(
                    Mathf.Round(pivotPos.x / m_SnapGridSize) * m_SnapGridSize,
                    Mathf.Round(pivotPos.y / m_SnapGridSize) * m_SnapGridSize,
                    Mathf.Round(pivotPos.z / m_SnapGridSize) * m_SnapGridSize);
            return pivotPos;
        }

        public void OnWindowUpdate()
        {
            if (!targetManager.isEmpty && Tools.current != Tool.None)
            {
                StopMove();
            }
        }

        void StartListeningSceneGUI()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void StopListeningSceneGUI()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Handles.color = Color.yellow;
            Handles.matrix = Matrix4x4.identity;

            foreach (var target in targetManager.EveryTarget())
            {
#if UNITY_5_6_OR_NEWER
                Handles.SphereHandleCap(0, target.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(target.transform.position) * 0.3f, Event.current.type);
#else
                Handles.SphereCap(0, target.transform.position, Quaternion.identity, HandleUtility.GetHandleSize(target.transform.position) * 0.3f);
#endif

                Handles_DrawWireCube(target.cachedBounds);
            }

            if (targetManager.isUnique)
            {
                var uniqueTarget = targetManager.uniqueTarget;
                if (uniqueTarget.TargetTransformHasChanged())
                    uniqueTarget.UpdateTargetCachedData();

                EditorGUI.BeginChangeCheck();
                var newPos = Handles.PositionHandle(uniqueTarget.transform.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    uniqueTarget.SetPivot(GetSnappedPosition(newPos), API.Space.Global);
                    AskToRepaintWindow(); // ask to repaint window
                }
            }
        }

        void Handles_DrawWireCube(Bounds bounds)
        {
#if UNITY_5_4_OR_NEWER
            Handles.DrawWireCube(bounds.center, bounds.size);
#else
            Vector3[] corners =
            {
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(1, 1, 1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1, 1, 1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1, 1, -1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(1, 1, -1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(1, -1, 1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1, -1, 1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(-1, -1, -1)),
                bounds.center + Vector3.Scale(bounds.extents, new Vector3(1, -1, -1))
            };

            for (int i = 0; i < 4; i++)
            {
                Handles.DrawLine(corners[i], corners[(i + 1) % 4]);
                Handles.DrawLine(corners[i], corners[i + 4]);
                Handles.DrawLine(corners[i + 4], corners[4 + (i + 1) % 4]);
            }
#endif
        }

        // Singleton
        private static readonly EditorAPI instance = new EditorAPI();
        public static EditorAPI Instance { get { return instance; } }

        EditorAPI()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;

            EditorPrefs.SetFloat(m_EditorPrefsPrefix + "Version", API.Version);
        }
    }

    internal static class Extensions
    {
        public static void AddIfNotNull<T>(this List<T> list, T item) where T : UnityEngine.Object
        {
            if (item)
                list.Add(item);
        }
    }
}
