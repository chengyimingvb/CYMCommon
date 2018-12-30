using System.Collections.Generic;
//#if UNITY_EDITOR
using UnityEditor;
//#endif
using UnityEngine;
namespace CYM
{
    public class BasePreviewUtile
    {
        //#if UNITY_EDITOR
        public static GUIContent speedScale = Icon("SpeedScale", "Changes particle preview speed");
        public static GUIContent pivot = Icon("AvatarPivot", "Displays avatar's pivot and mass center");
        public static GUIContent[] play = new GUIContent[2]
        {
            Icon("preAudioPlayOff", "Play"),
            Icon("preAudioPlayOn", "Stop")
        };
        public static GUIContent lockParticleSystem = Icon("IN LockButton", "Lock the current particle");
        public static GUIContent reload = new GUIContent(Icon("preAudioLoopOff", "Reload particle preview"));
        public static GUIStyle preButton = "preButton";
        public static GUIStyle preSlider = "preSlider";
        public static GUIStyle preSliderThumb = "preSliderThumb";
        public static GUIStyle preLabel = "preLabel";
        public static GUIStyle FoldOutPreDrop = "FoldOutPreDrop";
        public static GUIStyle FoldOut = "FoldOut";
        public static GUIStyle Head = new GUIStyle { fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = Color.gray } };

        public static Vector2[] Resolutions_16_9 = new Vector2[]
        {
            new Vector2(320,180),
            new Vector2(480,270),
            new Vector2(640,360),
            new Vector2(720,405),
            new Vector2(854,480),
            new Vector2(960,540),
            new Vector2(1080,600),
            new Vector2(1280,720),
            new Vector2(1920,1080),
        };

        public static int CurResolutionsIndex_16_9;
        public static Vector2 CurResolutions_16_9
        {
            get
            {
                return Resolutions_16_9[CurResolutionsIndex_16_9];
            }
        }

        public static string[] ResolutionsStr_16_9 = new string[]
        {
            "320*180",
            "480*270",
            "640*360",
            "720*405",
            "854*480",
            "960*540",
            "1080*600",
            "1280*720",
            "1920*1080",
        };

        public static GUIContent Icon(string name, string tooltip)
        {

            GUIContent content = EditorGUIUtility.IconContent(name, tooltip);
            content.tooltip = tooltip;
            return content;
            //return null;
        }

        public static float Slider(float val, float snapThreshold)
        {
            val = GUILayout.HorizontalSlider(val, 0.1f, 3f, preSlider, preSliderThumb, GUILayout.MaxWidth(64f));
            if (val > 0.25f - snapThreshold && val < 0.25f + snapThreshold)
            {
                val = 0.25f;
            }
            else
            {
                if (val > 0.5f - snapThreshold && val < 0.5f + snapThreshold)
                {
                    val = 0.5f;
                }
                else
                {
                    if (val > 0.75f - snapThreshold && val < 0.75f + snapThreshold)
                    {
                        val = 0.75f;
                    }
                    else
                    {
                        if (val > 1f - snapThreshold && val < 1f + snapThreshold)
                        {
                            val = 1f;
                        }
                        else
                        {
                            if (val > 1.25f - snapThreshold && val < 1.25f + snapThreshold)
                            {
                                val = 1.25f;
                            }
                            else
                            {
                                if (val > 1.5f - snapThreshold && val < 1.5f + snapThreshold)
                                {
                                    val = 1.5f;
                                }
                                else
                                {
                                    if (val > 1.75f - snapThreshold && val < 1.75f + snapThreshold)
                                    {
                                        val = 1.75f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return val;
        }

        public static int CycleButton(int selected)
        {
            bool flag = GUILayout.Button(play[selected], preButton);
            if (flag)
            {
                int num = selected;
                selected = num + 1;
                bool flag2 = selected >= play.Length;
                if (flag2)
                {
                    selected = 0;
                }
            }
            return selected;
        }

        public static bool ReloadButton()
        {
            return GUILayout.Button(reload, preButton);
        }

        public static void SpeedScaleBox()
        {
            GUILayout.Box(speedScale, preLabel);
        }

        public static void Lable(string str)
        {
            GUILayout.Label(str, preLabel);
        }

        public static int Popup(int selectedIndex, string[] displayedOptions)
        {

            return EditorGUILayout.Popup(selectedIndex, displayedOptions, preButton);
            //return 0;
        }

        public static bool ResolutionPopup_16_9()
        {
            int index = EditorGUILayout.Popup(CurResolutionsIndex_16_9, ResolutionsStr_16_9, preButton);
            bool ret = CurResolutionsIndex_16_9 != index;
            CurResolutionsIndex_16_9 = index;
            return ret;
            //return false;
        }

        public static void Header(string str)
        {

            EditorGUILayout.LabelField(str, Head);

        }
        //#endif


        static Dictionary<int, string[]> layerNames = new Dictionary<int, string[]>();
        static long lastUpdateTick;

        /// <summary>
        /// Tag names and an additional 'Edit Tags...' entry.
        /// Used for SingleTagField
        /// </summary>
        static string[] tagNamesAndEditTagsButton;

        /// <summary>
        /// Last time tagNamesAndEditTagsButton was updated.
        /// Uses EditorApplication.timeSinceStartup
        /// </summary>
        static double timeLastUpdatedTagNames;

        public static int TagField(string label, int value, System.Action editCallback)
        {
            // Make sure the tagNamesAndEditTagsButton is relatively up to date
            if (tagNamesAndEditTagsButton == null || EditorApplication.timeSinceStartup - timeLastUpdatedTagNames > 1)
            {
                timeLastUpdatedTagNames = EditorApplication.timeSinceStartup;
                var tagNames = AstarPath.FindTagNames();
                tagNamesAndEditTagsButton = new string[tagNames.Length + 1];
                tagNames.CopyTo(tagNamesAndEditTagsButton, 0);
                tagNamesAndEditTagsButton[tagNamesAndEditTagsButton.Length - 1] = "Edit Tags...";
            }

            // Tags are between 0 and 31
            value = Mathf.Clamp(value, 0, 31);

            var newValue = EditorGUILayout.IntPopup(label, value, tagNamesAndEditTagsButton, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, -1 });

            // Last element corresponds to the 'Edit Tags...' entry. Open the tag editor
            if (newValue == -1)
            {
                editCallback();
            }
            else
            {
                value = newValue;
            }

            return value;
        }

        public static bool UnityTagMaskList(GUIContent label, bool foldout, List<string> tagMask)
        {
            if (tagMask == null) throw new System.ArgumentNullException("tagMask");
            if (EditorGUILayout.Foldout(foldout, label))
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical();
                for (int i = 0; i < tagMask.Count; i++)
                {
                    tagMask[i] = EditorGUILayout.TagField(tagMask[i]);
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Tag")) tagMask.Add("Untagged");

                EditorGUI.BeginDisabledGroup(tagMask.Count == 0);
                if (GUILayout.Button("Remove Last")) tagMask.RemoveAt(tagMask.Count - 1);
                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
                return true;
            }
            return false;
        }

        /// <summary>Displays a LayerMask field.</summary>
        /// <param name="label">Label to display</param>
        /// <param name="selected">Current LayerMask</param>
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            if (Event.current.type == EventType.Layout && System.DateTime.UtcNow.Ticks - lastUpdateTick > 10000000L)
            {
                layerNames.Clear();
                lastUpdateTick = System.DateTime.UtcNow.Ticks;
            }

            string[] currentLayerNames;
            if (!layerNames.TryGetValue(selected.value, out currentLayerNames))
            {
                var layers = Pathfinding.Util.ListPool<string>.Claim();

                int emptyLayers = 0;
                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);

                    if (layerName != "")
                    {
                        for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                        layers.Add(layerName);
                    }
                    else
                    {
                        emptyLayers++;
                        if (((selected.value >> i) & 1) != 0 && selected.value != -1)
                        {
                            for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i + 1 - emptyLayers));
                        }
                    }
                }

                currentLayerNames = layerNames[selected.value] = layers.ToArray();
                Pathfinding.Util.ListPool<string>.Release(ref layers);
            }

            selected.value = EditorGUILayout.MaskField(label, selected.value, currentLayerNames);
            return selected;
        }
    }

}