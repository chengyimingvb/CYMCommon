
using UnityEditor;
using UnityEngine;
using System.Collections;
using CYM;
using Sirenix.OdinInspector.Editor;

namespace CYM.Cam
{
    [CustomEditor(typeof(RTSCamera))]
    public class RTSCameraEditor : OdinEditor
    {

        private RTSCamera mCam;
        private bool baseSetting;
        private bool boundSetting;
        private bool followSetting;
        private bool mouseSetting;
        private bool keyboardSetting;
        private Vector3 op;

        void Awake()
        {
            mCam = target as RTSCamera;
        }

        public override void OnInspectorGUI()
        {
            baseSetting = EditorGUILayout.Foldout(baseSetting, "Basic Setting");
            if (baseSetting)
            {
                EditorGUILayout.LabelField("Smoothing Settings");
                mCam.movementLerpSpeed = EditorGUILayout.FloatField("  Movement Lerp Speed", mCam.movementLerpSpeed);
                mCam.rotationLerpSpeed = EditorGUILayout.FloatField("  Rotation Lerp Speed", mCam.rotationLerpSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Scroll Settings");

                CamScrollAnimationType tempType = mCam.scrollAnimationType;
                tempType = (CamScrollAnimationType)EditorGUILayout.EnumPopup("  Animation Type", tempType);

                if (tempType != mCam.scrollAnimationType
                    && EditorUtility.DisplayDialog("Replacing Changes", "If you switch to another animation type, your settings in current mode will be replaced or modified.", "Continue", "Cancel"))
                {
                    mCam.scrollAnimationType = tempType;
                }


                switch (mCam.scrollAnimationType)
                {
                    case CamScrollAnimationType.Simple:

                        Keyframe f_minHigh = mCam.scrollHigh.keys[0];
                        Keyframe f_maxHigh = mCam.scrollHigh.keys[mCam.scrollHigh.keys.Length - 1];

                        f_minHigh.value = EditorGUILayout.FloatField("    Min High", f_minHigh.value);
                        f_maxHigh.value = EditorGUILayout.FloatField("    Max High", f_maxHigh.value);

                        mCam.scrollHigh = AnimationCurve.Linear(0, f_minHigh.value, 1, f_maxHigh.value);

                        EditorGUILayout.Space();

                        Keyframe f_minAngle = mCam.scrollXAngle.keys[0];
                        Keyframe f_maxAngle = mCam.scrollXAngle.keys[mCam.scrollXAngle.keys.Length - 1];

                        f_minAngle.value = EditorGUILayout.FloatField("    Min Angle", f_minAngle.value);
                        f_maxAngle.value = EditorGUILayout.FloatField("    Max Angle", f_maxAngle.value);
                        f_minAngle.outTangent = EditorGUILayout.FloatField("    Increase Rate", f_minAngle.outTangent);

                        f_maxAngle.inTangent = 1f;

                        mCam.scrollXAngle = new AnimationCurve(f_minAngle, f_maxAngle);

                        break;

                    case CamScrollAnimationType.Advanced:
                        mCam.scrollXAngle = EditorGUILayout.CurveField(new GUIContent("    Scroll X Angle", "Scroll X Angle Animation"), mCam.scrollXAngle);
                        mCam.scrollHigh = EditorGUILayout.CurveField(new GUIContent("    Scroll High", "Scroll High Animation"), mCam.scrollHigh);
                        break;
                }

                EditorGUILayout.Space();

                mCam.scrollValue = EditorGUILayout.Slider("  Start Scroll Value", mCam.scrollValue, 0f, 1f);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Casting Settings");
                mCam.groundHighTest = EditorGUILayout.Toggle("  Ground Check", mCam.groundHighTest);
                if (mCam.groundHighTest)
                {
                    mCam.groundMask = BasePreviewUtile.LayerMaskField("  Ground Mask", mCam.groundMask);
                }

                EditorGUILayout.Space();
            }

            boundSetting = EditorGUILayout.Foldout(boundSetting, "Bound");
            if (boundSetting)
            {

                mCam.bound.xMin = EditorGUILayout.FloatField("  Min X", mCam.bound.xMin);
                mCam.bound.xMax = EditorGUILayout.FloatField("  Max X", mCam.bound.xMax);
                mCam.bound.yMin = EditorGUILayout.FloatField("  Min Z", mCam.bound.yMin);
                mCam.bound.yMax = EditorGUILayout.FloatField("  Max Z", mCam.bound.yMax);

                if (GUILayout.Button("Use Suggested Values") && EditorUtility.DisplayDialog("Replacing Your Setting", "Use suggested value will replace your current settings.", "Confirm", "Cancel"))
                {
                    Bounds[] discoveredBounds;

                    MeshRenderer[] renderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
                    discoveredBounds = new Bounds[renderers.Length];

                    EditorUtility.DisplayProgressBar("Calculating...", "Finding objects...", 0);
                    for (int i = 0; i < discoveredBounds.Length; i++)
                        discoveredBounds[i] = renderers[i].bounds;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along X...", 0.25f);
                    float endValues = Mathf.Infinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues > discoveredBounds[i].min.x)
                            endValues = discoveredBounds[i].min.x;
                    }
                    mCam.bound.xMin = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along X...", 0.5f);
                    endValues = Mathf.NegativeInfinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues < discoveredBounds[i].max.x)
                            endValues = discoveredBounds[i].max.x;
                    }
                    mCam.bound.xMax = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along Z...", 0.75f);
                    endValues = Mathf.Infinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues > discoveredBounds[i].min.z)
                            endValues = discoveredBounds[i].min.z;
                    }
                    mCam.bound.yMin = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along Z...", 0.99f);
                    endValues = Mathf.NegativeInfinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues < discoveredBounds[i].max.z)
                            endValues = discoveredBounds[i].max.z;
                    }
                    mCam.bound.yMax = endValues;

                    EditorUtility.ClearProgressBar();
                }

                EditorGUILayout.HelpBox("The white rectangle in scene view will help you configure scene bounds.", MessageType.Info);

                EditorGUILayout.Space();
            }

            followSetting = EditorGUILayout.Foldout(followSetting, "Follow and Fixed Point");
            if (followSetting)
            {
                mCam.allowFollow = EditorGUILayout.Toggle("  Allow Follow", mCam.allowFollow);
                if (mCam.allowFollow)
                {
                    mCam.unlockWhenMove = EditorGUILayout.Toggle("  Unlock When Move", mCam.unlockWhenMove);
                }
                else
                {
                    EditorGUILayout.HelpBox("Enable Follow to let your camera focus something on center of screen or go to a fixed point.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }

            mouseSetting = EditorGUILayout.Foldout(mouseSetting, "Mouse Control Setting");
            if (mouseSetting)
            {
                mCam.screenEdgeMovementControl = EditorGUILayout.Toggle("  Screen Edge Movement", mCam.screenEdgeMovementControl);
                if (mCam.screenEdgeMovementControl)
                {
                    mCam.desktopMoveSpeed = EditorGUILayout.FloatField("    Move Speed", mCam.desktopMoveSpeed);
                }
                mCam.mouseDragControl = EditorGUILayout.Toggle("  Drag Control", mCam.mouseDragControl);
                if (mCam.mouseDragControl)
                {
                    mCam.mouseDragButton = System.Convert.ToInt32(EditorGUILayout.EnumPopup("    Move Button", (MouseButton)mCam.mouseDragButton));
                    mCam.mouseRotateButton = System.Convert.ToInt32(EditorGUILayout.EnumPopup("    Rotate Button", (MouseButton)mCam.mouseRotateButton));
                    mCam.desktopMoveDragSpeed = EditorGUILayout.FloatField("    Move Speed", mCam.desktopMoveDragSpeed);
                    mCam.desktopRotateSpeed = EditorGUILayout.FloatField("    Rotate Speed", mCam.desktopRotateSpeed);

                    if (mCam.mouseDragButton == mCam.mouseRotateButton)
                    {
                        EditorGUILayout.HelpBox("Control button overlapping.", MessageType.Warning);
                    }
                }

                mCam.mouseScrollControl = EditorGUILayout.Toggle("  Scroll Control", mCam.mouseScrollControl);
                if (mCam.mouseScrollControl)
                {
                    mCam.desktopScrollSpeed = EditorGUILayout.FloatField("    Scroll Speed", mCam.desktopScrollSpeed);
                }

                EditorGUILayout.Space();
            }
        }
    }

}