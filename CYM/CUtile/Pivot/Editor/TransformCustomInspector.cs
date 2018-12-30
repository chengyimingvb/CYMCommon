using UnityEngine;
using UnityEditor;

namespace CYM.Utile
{
    //[CanEditMultipleObjects, CustomEditor(typeof(Transform))]
    public class TransformCustomInspector : Editor
    {
        private const float FIELD_WIDTH_RIGH_ALIGNED = 200.0f;
        private const float LABEL_WIDTH_LEFT = 75.0f;
        private const bool WIDE_MODE = true;

        private const float POSITION_MAX = 100000.0f;

        private static GUIContent positionGUIContent = new GUIContent(LocalString("Position")
            , LocalString("The local position of this Game Object relative to the parent."));
        private static GUIContent rotationGUIContent = new GUIContent(LocalString("Rotation")
            , LocalString("The local rotation of this Game Object relative to the parent."));
        private static GUIContent scaleGUIContent = new GUIContent(LocalString("Scale")
            , LocalString("The local scaling of this Game Object relative to the parent."));

        private static string positionWarningText = LocalString("Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.");

        private SerializedProperty positionProperty;
        private SerializedProperty rotationProperty;
        private SerializedProperty scaleProperty;

        private static string LocalString(string text)
        {
#if UNITY_2017_3_OR_NEWER
            return text;
#else
            return LocalizationDatabase.GetLocalizedString(text);
#endif
        }

        void OnEnable()
        {
            positionProperty = serializedObject.FindProperty("m_LocalPosition");
            rotationProperty = serializedObject.FindProperty("m_LocalRotation");
            scaleProperty = serializedObject.FindProperty("m_LocalScale");
        }

        public override void OnInspectorGUI()
        {
            EditorAPI.Instance.OnWindowUpdate();

            EditorGUIUtility.wideMode = WIDE_MODE;
            EditorGUIUtility.labelWidth = LABEL_WIDTH_LEFT;

            serializedObject.Update();

            EditorGUILayout.PropertyField(positionProperty, positionGUIContent);
            RotationPropertyField(rotationProperty, rotationGUIContent);
            EditorGUILayout.PropertyField(scaleProperty, scaleGUIContent);

            if (!ValidatePosition(((Transform)this.target).position))
            {
                EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();

            // ================================================================
            // SUPER PIVOT
            var selectedTransforms = EditorAPI.GetSelectedTransforms();
            if (API.CanChangeAtLeastOnePivot(selectedTransforms))
            {
                EditorGUILayout.Separator();
                EditorAPI.Instance.DrawGUI(selectedTransforms);
            }
            // ================================================================
        }

        private bool ValidatePosition(Vector3 position)
        {
            if (Mathf.Abs(position.x) > POSITION_MAX) return false;
            if (Mathf.Abs(position.y) > POSITION_MAX) return false;
            if (Mathf.Abs(position.z) > POSITION_MAX) return false;
            return true;
        }

        private void RotationPropertyField(SerializedProperty rotationProperty, GUIContent content)
        {
            Transform transform = (Transform)this.targets[0];
            Quaternion localRotation = transform.localRotation;
            foreach (UnityEngine.Object t in (UnityEngine.Object[])this.targets)
            {
                if (!SameRotation(localRotation, ((Transform)t).localRotation))
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();

            Vector3 eulerAngles = EditorGUILayout.Vector3Field(content, localRotation.eulerAngles);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(this.targets, "Rotation Changed");
                foreach (UnityEngine.Object obj in this.targets)
                {
                    Transform t = (Transform)obj;
                    t.localEulerAngles = eulerAngles;
                }
                rotationProperty.serializedObject.SetIsDifferentCacheDirty();
            }

            EditorGUI.showMixedValue = false;
        }

        private bool SameRotation(Quaternion rot1, Quaternion rot2)
        {
            if (rot1.x != rot2.x) return false;
            if (rot1.y != rot2.y) return false;
            if (rot1.z != rot2.z) return false;
            if (rot1.w != rot2.w) return false;
            return true;
        }
    }
}

