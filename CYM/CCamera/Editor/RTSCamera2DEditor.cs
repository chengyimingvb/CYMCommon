using UnityEditor;
using UnityEngine;
using System.Collections;
namespace CYM.Cam
{
    [CustomEditor(typeof(RTSCamera2D))]
    public class RTSCamera2DEditor : Editor
    {

        private RTSCamera2D mCam;
        private bool baseSetting;
        private bool boundSetting;
        private bool followSetting;
        private bool mouseSetting;
        private bool keyboardSetting;
        private bool touchSetting;
        private bool previewing;
        private Vector3 op;

        void Awake()
        {
            mCam = target as RTSCamera2D;
        }

        public override void OnInspectorGUI()
        {
            baseSetting = EditorGUILayout.Foldout(baseSetting, "Basic Setting");
            if (baseSetting)
            {
                EditorGUILayout.LabelField("Smoothing Settings");
                mCam.movementLerpSpeed = EditorGUILayout.FloatField("  Movement Lerp Speed", mCam.movementLerpSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Scroll Settings");

                EditorGUILayout.Space();

                Range zoomRange = mCam.zoomRange;

                zoomRange.min = EditorGUILayout.FloatField("  Zoom Min", zoomRange.min);
                zoomRange.max = EditorGUILayout.FloatField("  Zoom Max", zoomRange.max);

                mCam.zoomRange = zoomRange;

                mCam.scrollValue = EditorGUILayout.Slider("  Start Scroll Value", mCam.scrollValue, 0f, 1f);
                EditorGUILayout.Space();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Preview");
                previewing = EditorGUILayout.Toggle("  Preview Settings", previewing);
                if (previewing) mCam.Adjust2AttitudeBaseOnCurrentSetting();

                EditorGUILayout.Space();
            }

            boundSetting = EditorGUILayout.Foldout(boundSetting, "Bound");
            if (boundSetting)
            {

                mCam.bound.xMin = EditorGUILayout.FloatField("  Min X", mCam.bound.xMin);
                mCam.bound.xMax = EditorGUILayout.FloatField("  Max X", mCam.bound.xMax);
                mCam.bound.yMin = EditorGUILayout.FloatField("  Min Y", mCam.bound.yMin);
                mCam.bound.yMax = EditorGUILayout.FloatField("  Max Y", mCam.bound.yMax);

                if (GUILayout.Button("Use Suggested Values") && EditorUtility.DisplayDialog("Replacing Your Setting", "Use suggested value will replace your current settings.", "Confirm", "Cancel"))
                {
                    Bounds[] discoveredBounds;

                    SpriteRenderer[] renderers = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
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
                        if (endValues > discoveredBounds[i].min.y)
                            endValues = discoveredBounds[i].min.y;
                    }
                    mCam.bound.yMin = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along Z...", 0.99f);
                    endValues = Mathf.NegativeInfinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues < discoveredBounds[i].max.y)
                            endValues = discoveredBounds[i].max.y;
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
                mCam.mouseControl = EditorGUILayout.Toggle("  Enabled", mCam.mouseControl);
                if (mCam.mouseControl)
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS || EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer)
                    {
                        EditorGUILayout.HelpBox("Please notice that mouse control is not supported on mobile platform. Mouse control is enabled now for you to debug in editor, but it will disable automatically when you build.", MessageType.Warning);
                    }
                    mCam.desktopMoveSpeed = EditorGUILayout.FloatField("  Move Speed", mCam.desktopMoveSpeed);
                    mCam.desktopScrollSpeed = EditorGUILayout.FloatField("  Scroll Wheel Speed", mCam.desktopScrollSpeed);


                }
                else
                {
                    EditorGUILayout.HelpBox("Enable Mouse Control to control camera with mouse.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }        
            EditorGUILayout.Space();
        }

        string StringPopup(string label, string[] list, string currentValue)
        {
            int[] optionList = new int[list.Length];
            int curID = 0;
            for (int i = 0; i < list.Length; i++)
            {
                optionList[i] = i;
                if (list[i] == currentValue)
                    curID = i;
            }

            curID = EditorGUILayout.IntPopup(label, curID, list, optionList);
            return list[curID];
        }
    }

}