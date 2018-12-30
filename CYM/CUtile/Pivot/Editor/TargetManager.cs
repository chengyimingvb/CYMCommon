using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


namespace CYM.Utile
{
    internal class TargetManager
    {
        public bool isEmpty { get { return m_Targets.Count == 0; } }
        public bool isUnique { get { return m_Targets.Count == 1; } }
        public TargetWrapper uniqueTarget { get { Debug.Assert(isUnique); return m_Targets[0]; } }
        public int count { get { return m_Targets.Count; } }
        public string errorMsg { get; private set; }

        List<TargetWrapper> m_Targets = new List<TargetWrapper>();
        static GUIContent ms_InvalidBoundsGUIContent = new GUIContent(
            "The bounds of this group are not valid.",
            "This object has no Renderer attached, so we cannot compute his bounds.");

        static Color kColorRed      = new Color32(255, 150, 150, 255);
        static Color kColorGreen    = new Color32(150, 255, 150, 255);
        static Color kColorBlue     = new Color32(150, 150, 255, 255);

        public IEnumerable<TargetWrapper> EveryTarget()
        {
            foreach (var t in m_Targets)
                yield return t;
        }

        public Transform[] GetTransformTargets()
        {
            var transforms = new Transform[m_Targets.Count];
            for (int i = 0; i < transforms.Length; i++)
                transforms[i] = m_Targets[i].transform;
            return transforms;
        }

        public void SetTargets(Transform[] selectedTransforms)
        {
            m_Targets.Clear();
            errorMsg = null;

            if (selectedTransforms != null)
            {
                foreach (var t in selectedTransforms)
                {
                    string localError = null;
                    if (API.CanChangePivot(t, out localError))
                    {
                        m_Targets.Add(new TargetWrapper(t));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(errorMsg) && localError != null)
                            errorMsg = localError;
                    }
                }
            }
        }

        public bool canModifyBoundsSliders
        {
            get
            {
                foreach (var t in m_Targets)
                    if (t.canModifyBoundsSliders)
                        return true;
                return false;
            }
        }

        Vector3 GetInverseLerpBoundsPosition(out Vector3 mixedValue)
        {
            mixedValue = Vector3.zero;
            var savedPos = Vector3.zero;
            var count = 0;
            foreach (var t in m_Targets)
            {
                if (t.canModifyBoundsSliders)
                {
                    var pos = t.InverseLerpBoundsPosition();

                    if (count == 0)
                    {
                        savedPos = pos;
                    }
                    else
                    {
                        mixedValue.x = (mixedValue.x > 0.5f || !Mathf.Approximately(savedPos.x, pos.x)) ? 1f : 0f;
                        mixedValue.y = (mixedValue.y > 0.5f || !Mathf.Approximately(savedPos.y, pos.y)) ? 1f : 0f;
                        mixedValue.z = (mixedValue.z > 0.5f || !Mathf.Approximately(savedPos.z, pos.z)) ? 1f : 0f;
                    }
                    count++;
                }
            }

            return savedPos;
        }

        public void GUIBounds()
        {
            EditorGUILayout.Separator();
            {
                EditorGUILayout.LabelField("Bounds", EditorStyles.boldLabel);

                if (canModifyBoundsSliders)
                {
                    Vector3 mixedValue;
                    var sliderValue = GetInverseLerpBoundsPosition(out mixedValue);
                    GUISliderBounds(TargetWrapper.Component.X, "X", kColorRed,   sliderValue.x, mixedValue.x > 0.5f);
                    GUISliderBounds(TargetWrapper.Component.Y, "Y", kColorGreen, sliderValue.y, mixedValue.y > 0.5f);
                    GUISliderBounds(TargetWrapper.Component.Z, "Z", kColorBlue,  sliderValue.z, mixedValue.z > 0.5f);

                    if (GUILayout.Button("Bounds center", EditorStyles.miniButton))
                        SetPivotLerpBoundsPosition(Vector3.one * 0.5f);
                }
                else
                {
                    EditorGUILayout.LabelField(ms_InvalidBoundsGUIContent);
                }
            }
        }

        void GUISliderBounds(TargetWrapper.Component comp, string label, Color color, float value, bool mixedValue)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField(label, GUILayout.Width(15f));
                EditorGUI.showMixedValue = mixedValue;
                var prevColor = GUI.color;
                GUI.color = color;
                value = EditorGUILayout.Slider(value, 0f, 1f);
                GUI.color = prevColor;
                EditorGUI.showMixedValue = false;

                var buttonStyle = new GUIStyle(EditorStyles.miniButton);
                buttonStyle.fixedWidth = 20f;
                buttonStyle.alignment = TextAnchor.MiddleCenter;
                if (GUILayout.Button("<", buttonStyle)) value = 0.0f;
                if (GUILayout.Button("|", buttonStyle)) value = 0.5f;
                if (GUILayout.Button(">", buttonStyle)) value = 1.0f;

                if (EditorGUI.EndChangeCheck())
                    SetPivotLerpBoundsPosition(comp, value);
            }
        }

        void SetPivotLerpBoundsPosition(Vector3 sliderClampedValue)
        {
            foreach (var t in m_Targets)
                t.SetPivot(t.LerpBoundsPosition(sliderClampedValue), API.Space.Global);
        }

        void SetPivotLerpBoundsPosition(TargetWrapper.Component comp, float sliderClampedValue)
        {
            foreach (var t in m_Targets)
                t.SetPivot(comp, t.LerpBoundsPosition(comp, sliderClampedValue), API.Space.Global);
        }
    }
}