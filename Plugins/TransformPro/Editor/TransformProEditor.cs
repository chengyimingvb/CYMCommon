namespace UntitledGames.Transforms
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    ///     Extends the default Unity Transform inspector to add extra features.
    ///     More information can be found at http://transformpro.untitledgam.es
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform))]
    public partial class TransformProEditor : Editor, IEnumerable<TransformPro>
    {
        private static TransformProEditorCache cache;
        private static TransformProEditor instance;

        public static Vector3 AveragePosition { get { return TransformProEditor.Selected.Select(x => x.Position).Average(); } }

        public static Quaternion AverageRotation
        {
            get
            {
                if (TransformProEditor.SelectedCount == 1)
                {
                    return TransformProEditor.Selected.First().Rotation;
                }
                return TransformProEditor.Selected.Select(x => x.Rotation).Average();
            }
        }

        public static Vector3 AverageScale { get { return TransformProEditor.Selected.Select(x => x.Scale).Average(); } }

        public static TransformProEditorCache Cache { get { return TransformProEditor.cache ?? (TransformProEditor.cache = new TransformProEditorCache()); } }

        public static Camera Camera
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (SceneView.currentDrawingSceneView != null)
                    {
                        if (SceneView.currentDrawingSceneView.camera != null)
                        {
                            return SceneView.currentDrawingSceneView.camera;
                        }
                    }
                }
#endif
                return Camera.main;
            }
        }

        public static bool CanAllChangePosition { get { return (TransformProEditor.Selected != null) && TransformProEditor.Selected.All(x => x.CanChangePosition); } }
        public static bool CanAllChangeRotation { get { return (TransformProEditor.Selected != null) && TransformProEditor.Selected.All(x => x.CanChangeRotation); } }
        public static bool CanAllChangeScale { get { return (TransformProEditor.Selected != null) && TransformProEditor.Selected.All(x => x.CanChangeScale); } }

        public static bool CanAnyChangePosition { get { return (TransformProEditor.Selected != null) && TransformProEditor.Selected.Any(x => x.CanChangePosition); } }
        public static bool CanAnyChangeRotation { get { return (TransformProEditor.Selected != null) && TransformProEditor.Selected.Any(x => x.CanChangeRotation); } }
        public static bool CanAnyChangeScale { get { return (TransformProEditor.Selected != null) && TransformProEditor.Selected.Any(x => x.CanChangeScale); } }

        public static TransformProClipboard Clipboard { get { return TransformProPreferences.Clipboard; } }

        public static TransformProEditor Instance { get { return TransformProEditor.instance; } }

        public static IEnumerable<TransformPro> Selected { get { return TransformProEditor.Cache.Selected; } }

        public static int SelectedClipboard { get { return TransformProPreferences.SelectedClipboard; } set { TransformProPreferences.SelectedClipboard = value; } }

        public static int SelectedCount { get { return TransformProEditor.Cache.SelectedCount; } }

        /// <inheritdoc />
        public IEnumerator<TransformPro> GetEnumerator()
        {
            return TransformProEditor.Cache.Selected.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return TransformProEditor.Cache.Selected.GetEnumerator();
        }

        public static Transform Clone(Transform transform)
        {
            GameObject gameObjectOld = transform.gameObject;

            GameObject gameObjectNew;
            if (transform.IsPrefab())
            {
                UnityObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObjectOld);
                gameObjectNew = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            else
            {
                gameObjectNew = UnityObject.Instantiate(gameObjectOld);
            }
            gameObjectNew.name = gameObjectOld.name; // Get rid of the (Clone)(Clone)(Clone)(Clone) madness

            Transform transformNew = gameObjectNew.transform;
            transformNew.SetParent(transform.parent);
            transformNew.localPosition = transform.localPosition;
            transformNew.localRotation = transform.localRotation;
            transformNew.localScale = transform.localScale;

            return transformNew;
        }

        public static int Drop()
        {
            TransformProEditor.RecordUndo("Drop");
            return TransformProEditor.Selected.OrderBy(x => x.Position.y).Count(transformPro => !transformPro.Drop());
        }

        public static int Ground()
        {
            TransformProEditor.RecordUndo("Ground");
            return TransformProEditor.Selected.OrderBy(x => x.Position.y).Count(transformPro => !transformPro.Ground());
        }

        public static void RecordUndo(string name)
        {
            Undo.RecordObjects(Selection.GetTransforms(SelectionMode.ExcludePrefab).Cast<UnityObject>().ToArray(), string.Format("TransformPro {0}", name));
        }

        public static void Select(IEnumerable<Object> objects)
        {
            TransformProEditor.Cache.Select(objects);
        }

        public static void Select(IEnumerable<Transform> transforms)
        {
            TransformProEditor.Cache.Select(transforms.Cast<Object>());
        }

        /// <summary>
        ///     Shows a form containing the preferences UI. Doesn't use the default Editor preferences due to being unable to
        ///     reflect the internal structs used to define preferences tabs.
        /// </summary>
        public static void ShowPreferences()
        {
            TransformProPreferences preferences = EditorWindow.GetWindow<TransformProPreferences>();
            preferences.Show();
        }

        //public static bool AutoSnap { get { return TransformProEditor.autoSnap; } set { TransformProEditor.autoSnap = value; } }

        public static void Snap()
        {
            TransformProEditor.RecordUndo("Snap Transform");
            foreach (TransformPro transform in TransformProEditor.Selected)
            {
                transform.SnapPosition();
                transform.SnapRotation();
            }
        }

        public static void SnapPosition()
        {
            TransformProEditor.RecordUndo("Snap Position");
            foreach (TransformPro transform in TransformProEditor.Selected)
            {
                transform.SnapPosition();
            }
        }

        public static void SnapRotation()
        {
            TransformProEditor.RecordUndo("Snap Rotation");
            foreach (TransformPro transform in TransformProEditor.Selected)
            {
                transform.SnapRotation();
            }
        }

        private static void CreateBoxCollider()
        {
            if (!TransformProEditor.CanGenerateCollider())
            {
                Debug.LogWarning("[<color=red>TransformPro</color>] Cannot generate a box collider. Object has no collider or renderer bounds.");
                return;
            }

            foreach (TransformPro transformPro in TransformProEditor.Selected)
            {
                BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(transformPro.Transform.gameObject);
                if (transformPro.RendererBounds.Local.size.sqrMagnitude > 0)
                {
                    boxCollider.size = transformPro.RendererBounds.Local.size;
                    boxCollider.center = transformPro.RendererBounds.Local.center;
                }
                else if (transformPro.ColliderBounds.Local.size.sqrMagnitude > 0)
                {
                    boxCollider.size = transformPro.ColliderBounds.Local.size;
                    boxCollider.center = transformPro.ColliderBounds.Local.center;
                }
            }
        }

        private static void CreateCapsuleCollider()
        {
            if (!TransformProEditor.CanGenerateCollider())
            {
                Debug.LogWarning("[<color=red>TransformPro</color>] Cannot generate a capsule collider. Object has no collider or renderer bounds.");
                return;
            }

            foreach (TransformPro transformPro in TransformProEditor.Selected)
            {
                CapsuleCollider capsuleCollider = Undo.AddComponent<CapsuleCollider>(transformPro.Transform.gameObject);
                if (transformPro.RendererBounds.Local.size.sqrMagnitude > 0)
                {
                    capsuleCollider.radius = Mathf.Max(transformPro.RendererBounds.Local.size.x, transformPro.RendererBounds.Local.size.z) / 2;
                    capsuleCollider.height = transformPro.RendererBounds.Local.size.y;
                    capsuleCollider.center = transformPro.RendererBounds.Local.center;
                }
                else if (transformPro.ColliderBounds.Local.size.sqrMagnitude > 0)
                {
                    capsuleCollider.radius = Mathf.Max(transformPro.ColliderBounds.Local.size.x, transformPro.ColliderBounds.Local.size.z) / 2;
                    capsuleCollider.height = transformPro.ColliderBounds.Local.size.y;
                    capsuleCollider.center = transformPro.ColliderBounds.Local.center;
                }
            }
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            if (TransformProEditor.SelectedCount == 0)
            {
                return;
            }

            this.serializedObject.Update();
            this.InspectorGUI();
            this.serializedObject.ApplyModifiedProperties();
        }

        private void InspectorGUI()
        {
            if (!TransformProStyles.Load())
            {
                return;
            }

            GUISkin resetSkin = GUI.skin;
            GUI.skin = TransformProStyles.Skin;

            Rect brandingRect = EditorGUILayout.GetControlRect(false, 0);
            brandingRect.y -= 16;
            brandingRect.x += 103;
            brandingRect.width = 72;
            brandingRect.height = 20;
            GUI.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            GUI.Label(brandingRect, "Pro");
            GUI.color = Color.white;

            if (TransformProStyles.Icons.Icon != null)
            {
                Rect iconRect = new Rect(brandingRect)
                                {
                                    x = brandingRect.x - 101,
                                    width = 16,
                                    height = 16
                                };
                Color backgroundColor = EditorGUIUtility.isProSkin ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);
                EditorGUI.DrawRect(iconRect, backgroundColor);
                GUI.DrawTexture(iconRect, TransformProStyles.Icons.Icon);
            }

            EditorGUILayout.BeginHorizontal();

            // Preferences cog (move to prefs class?
            GUIContent preferencesContent = new GUIContent(TransformProStyles.Icons.Cog, TransformProStrings.SystemLanguage.TooltipPreferences);
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            if (GUILayout.Button(preferencesContent, TransformProStyles.Buttons.Icon.Single, GUILayout.Width(28)))
            {
                if (Event.current.shift || (Event.current.button == 1))
                {
                    TransformProEditor.Cache.Clear();
                }
                else
                {
                    TransformProEditor.ShowPreferences();
                }
            }
            GUI.contentColor = Color.white;

            // Space mode controls
            TransformProEditorHandles.DrawGUI(this);

            // Reset button main
            GUI.backgroundColor = TransformProStyles.ColorReset;
            GUI.contentColor = Color.white;
            GUIContent resetContent = new GUIContent("Reset", TransformProStrings.SystemLanguage.TooltipResetTransform);
            if (GUILayout.Button(resetContent, TransformProStyles.Buttons.IconTint.Single, GUILayout.Width(64)))
            {
                TransformProEditor.RecordUndo("Reset");
                foreach (TransformPro transform in this)
                {
                    transform.Reset();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // Position, Rotation, Scale
            this.PositionField();
            this.RotationField();
            this.ScaleField();

            GUI.skin = resetSkin;
        }

        /// <summary>
        ///     Deregister events.
        /// </summary>
        private void OnDisable()
        {
            TransformProEditor.instance = null;

            Undo.undoRedoPerformed -= this.UndoRedoPerformed;
        }

        /// <summary>
        ///     Ensure everything is loaded and register events.
        /// </summary>
        private void OnEnable()
        {
            TransformProEditor.instance = this;

            // why doesnt prefs handle this check? expose a force bool if needed
            if (!TransformProPreferences.AreLoaded)
            {
                TransformProPreferences.Load();
            }

            Undo.undoRedoPerformed += this.UndoRedoPerformed;
        }

        /// <summary>
        ///     <see cref="Undo.undoRedoPerformed" /> delegate. Ensures the bounds are recalculated when Undo or Redo are used.
        /// </summary>
        private void UndoRedoPerformed()
        {
            foreach (TransformPro transform in this)
            {
                transform.SetComponentsDirty();
            }
        }
    }
}
