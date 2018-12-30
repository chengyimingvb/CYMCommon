//#define TAKE_ACCOUNT_POINT_ENTITIES

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CYM.Utile
{
    internal class TargetWrapper
    {
        public enum Component { X=0, Y=1, Z=2 }
        public Transform transform { get; private set; }
        public Bounds cachedBounds { get; private set; }
        public string name { get { return transform.name; } }

        Vector3 m_CachedPosition;
        Quaternion m_CachedRotation;
        Vector3 m_CachedScale;

        public bool areBoundsValid { get
            {
                return cachedBounds.min.x < cachedBounds.max.x
                    || cachedBounds.min.y < cachedBounds.max.y
                    || cachedBounds.min.z < cachedBounds.max.z;
            }
        }
        public bool canModifyBoundsSliders { get { return transform.childCount > 0 && areBoundsValid; } }

        public TargetWrapper(Transform t)
        {
            Debug.Assert(t != null);
            transform = t;
            UpdateTargetCachedData();
        }

        public void UpdateTargetCachedData()
        {
            Debug.Assert(transform);
            m_CachedPosition = transform.position;
            m_CachedRotation = transform.rotation;
            m_CachedScale = transform.localScale;

            cachedBounds = ComputeTotalBounds(transform);
        }

        public bool TargetTransformHasChanged()
        {
            Debug.Assert(transform);
            return transform.position != m_CachedPosition
                || transform.rotation != m_CachedRotation
                || transform.localScale != m_CachedScale;
        }

        public void SetPivot(Vector3 pivotPos, API.Space space)
        {
            API.SetPivot(transform, pivotPos, space);
        }

        public void SetPivot(Component comp, float value, API.Space space)
        {
            Debug.Assert(transform, "Invalid target entity");
            var pivotPos = transform.GetPivotPosition(space);
            pivotPos[(int)comp] = value;
            API.SetPivot(transform, pivotPos, space);
        }

        public Vector3 LerpBoundsPosition(Vector3 factor)
        {
            return new Vector3(
                Mathf.LerpUnclamped(cachedBounds.min.x, cachedBounds.max.x, factor.x),
                Mathf.LerpUnclamped(cachedBounds.min.y, cachedBounds.max.y, factor.y),
                Mathf.LerpUnclamped(cachedBounds.min.z, cachedBounds.max.z, factor.z));
        }

        public float LerpBoundsPosition(Component comp, float factor)
        {
            return Mathf.LerpUnclamped(cachedBounds.min[(int)comp], cachedBounds.max[(int)comp], factor);
        }

        Vector3 InverseLerpBoundsPosition(Vector3 worldPos)
        {
            return new Vector3(
                InverseLerpUnclamped(cachedBounds.min.x, cachedBounds.max.x, worldPos.x),
                InverseLerpUnclamped(cachedBounds.min.y, cachedBounds.max.y, worldPos.y),
                InverseLerpUnclamped(cachedBounds.min.z, cachedBounds.max.z, worldPos.z));
        }

        public Vector3 InverseLerpBoundsPosition() { return InverseLerpBoundsPosition(transform.position); }

        static float InverseLerpUnclamped(float from, float to, float value)
        {
            if (from == to) return 0.5f;
            return (value - from) / (to - from);
        }

        static bool GUIButtonZero()
        {
            var buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.fixedWidth = 40f;
            return GUILayout.Button("Zero", buttonStyle);
        }

        public void GUIWorldPosition()
        {
            EditorGUILayout.LabelField("World Position", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var newPos = EditorGUILayout.Vector3Field("", transform.position);
                if (EditorGUI.EndChangeCheck())
                    SetPivot(newPos, API.Space.Global);

                if (GUIButtonZero())
                    SetPivot(Vector3.zero, API.Space.Global);
            }
        }

        public void GUILocalPosition()
        {
            if (transform.parent)
            {
                EditorGUILayout.LabelField(string.Format("Local Position (relative to '{0}')", transform.parent.name), EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    var newPos = EditorGUILayout.Vector3Field("", transform.localPosition);
                    if (EditorGUI.EndChangeCheck())
                        SetPivot(newPos, API.Space.Local);

                    if (GUIButtonZero())
                        SetPivot(Vector3.zero, API.Space.Local);
                }
            }
        }

        public void GUIChildren()
        {
            var childCount = transform.childCount;
            if (childCount > 0)
            {
                EditorGUILayout.Separator();
                {
                    EditorGUILayout.LabelField(string.Format("Children ({0})", childCount), EditorStyles.boldLabel);

                    var buttonStyle = EditorStyles.miniButton;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Average pivot pos", buttonStyle))
                        {
                            var avgPos = GetChildrenAverageWorldPosition();
                            SetPivot(avgPos, API.Space.Global);
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        const int kMaxChild = 6;
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            var child = transform.GetChild(i);
                            if (GUILayout.Button(child.name, buttonStyle))
                                SetPivot(child.transform.position, API.Space.Global);

                            if (i >= kMaxChild - 1) break;
                        }
                    }
                }
            }
        }

        Vector3 GetChildrenAverageWorldPosition()
        {
            if (transform.childCount == 0)
            {
                Debug.LogWarningFormat(transform.gameObject, "{0} has no children", transform.name);
                return transform.position;
            }

            var avgPos = Vector3.zero;
            foreach (Transform child in transform)
                avgPos += child.position;
            return avgPos / transform.childCount;
        }

        static Bounds ComputeTotalBounds(Transform parent)
        {
            var bounds = new Bounds();
            bounds.SetMinMax(
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                new Vector3(float.MinValue, float.MinValue, float.MinValue));

            var renderersList = new List<Renderer>(parent.GetComponentsInChildren<Renderer>());
            renderersList.AddIfNotNull(parent.GetComponent<Renderer>());

            var count = 0;
            foreach (var child in renderersList)
            {
                bounds.Encapsulate(child.bounds);
                count++;
            }

#if UNITY_5_5_OR_NEWER
            var terrainsList = new List<Terrain>(parent.GetComponentsInChildren<Terrain>());
            terrainsList.AddIfNotNull(parent.GetComponent<Terrain>());
            foreach (var child in terrainsList)
            {
                bounds.Encapsulate(child.transform.TransformPoint(child.terrainData.bounds.min));
                bounds.Encapsulate(child.transform.TransformPoint(child.terrainData.bounds.max));
                count++;
            }
#endif

            if (count == 0)
            {
#if TAKE_ACCOUNT_POINT_ENTITIES
                // if we have no renderer at all as children take account of the point position of each child:
                var children = parent.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    bounds.Encapsulate(child.position);
                }
#else
                bounds.Encapsulate(parent.position); // only take account of current pivot position
#endif
            }

            return bounds;
        }
    }
}
