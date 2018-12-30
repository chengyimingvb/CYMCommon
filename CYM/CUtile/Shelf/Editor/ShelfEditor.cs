/* --------------------------------------------------------
 * Unity Editor Shelf v2.1.0
 * --------------------------------------------------------
 * Use of this script is subject to the Unity Asset Store
 * End User License Agreement:
 *
 * http://unity3d.com/unity/asset-store/docs/provider-distribution-agreement-appendex1.html
 *
 * Use of this script for any other purpose than outlined
 * in the EULA linked above is prohibited.
 * --------------------------------------------------------
 * © 2013 Adrian Stutz (adrian@sttz.ch)
 */

// Comment the following define to disable all hacks.
// Doing so will impair the usability of the shelf
// but migth work around issues in upcoming Unity versions.
#define ENABLE_HACKS

// Comment the following define to disable the hack used
// to trigger a redraw of the preferences window.
#define PREFERENCES_WINDOW_REDRAW_HACK
// Comment the following define to disable the hack used
// to show a folder's content when clicking on it in the shelf
// when using Unity 4's two-column project viewer.
#define FOLDER_SELECTION_HACK

// Set how many menu commands for opening specific layers
// should be created by changing the number behind CMDS_
// e.g. CMDS_1 creates only one shortcut for the first shelf,
// CMDS_5 for the first five shelves etc. (Up to CMDS_10.)
#define CMDS_5

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using UnityEngine;
using UnityEditor;

#if UNITY_5_3_OR_NEWER &&  UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CYM.Utile
{

    /// <summary>
    /// A multi-layer shelf for Unity to put things on!
    /// </summary>
    public class ShelfEditor : EditorWindow, IHasCustomMenu
    {
        ////////////////////
        // Configuration

        // Path to the shelf data file, relative to the project folder
        protected const string DATA_PATH = "Assets/Resources/ShelfData.asset";
        // Base path for the shelf menu items
        //protected const string MENU_PATH = "Window/Shelf/";
        // Shortcut modifier keys (modifier key + shelf number)
        protected const string MENU_SHORTCUT = " #";
        // Shurtcut for the shelf selection command
        protected const string SHELF_SELECTION_SHORTCUT = " #&";
        // Time before the layer switches to the dragging-over one
        protected const float LAYER_DRAG_SWITCH_TIME = 0.5f;
        // Extra width of the toolbar besides the buttons themselves
        protected const float TOOLBAR_EXTRA_WIDTH = 6;
        // Height of a single shelf item
        protected const float SHELF_ITEM_HEIGHT = 17;

        // Open the shelf editor window
        public static void OpenShelf(int layer = 0)
        {
            Data.currentLayer = layer;
            OpenTab();
        }

        protected static float WindowSide(float aspect, Rect windowRect, Vector2 screenSize)
        {
            if (aspect < 1f)
            {
                return Mathf.Sign((screenSize.x - windowRect.x - windowRect.width) - windowRect.x);
            }
            else
            {
                return Mathf.Sign((screenSize.y - windowRect.y - windowRect.height) - windowRect.y);
            }
        }

        // (Re-)Open the shelf as persistent and dockable tab
        public static void OpenTab()
        {
            if (editorInstance != null)
            {
                editorInstance.Close();
            }

            editorInstance = CreateInstance<ShelfEditor>();
            editorInstance.name = "Shelf";
            editorInstance.Show();
        }

        public static void PutSelectionOnShelf(int layer)
        {
            if (layer < 0 && layer >= Data.layers.Count)
            {
                Debug.LogError("Shelf layer index out of bounds. The selected layer propably doesn't exist.");
                EditorApplication.Beep();
                return;
            }

            var target = Data.layers[layer];

            var objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Unfiltered);
            target.objects.AddRange(objects);

            Data.currentLayer = Data.layers.IndexOf(target);

            if (editorInstance != null)
            {
                editorInstance.Repaint();
            }
        }

        ///////////////////
        // Shelf Data

        private static ShelfData _data;
        protected static ShelfData Data
        {
            get
            {
                // Load shelf data
                if (_data == null)
                {
                    _data = AssetDatabase.LoadAssetAtPath(DATA_PATH, typeof(ShelfData)) as ShelfData;
                    if (_data == null)
                    {
                        // Create new shelf data and save as asset
                        _data = ScriptableObject.CreateInstance<ShelfData>();
                        Directory.CreateDirectory(Path.GetDirectoryName(DATA_PATH));
                        AssetDatabase.CreateAsset(_data, DATA_PATH);
                        _data.hideFlags = HideFlags.NotEditable;
                        EditorUtility.SetDirty(_data);
                    }
                }

                return _data;
            }
        }

        ////////////////////
        // Preferences

        // EditorPrefs key to store popup shelf preference
        public const string POPUP_PREFS_KEY = "ShelfPopupEnabled";
        // EditorPrefs keys to save shelf width and height
        public const string SHELF_WIDTH_KEY = "ShelfWidth";
        public const string SHELF_HEIGHT_KEY = "ShelfHeight";

        // Reference to the current ShelfEditor instance
        protected static ShelfEditor editorInstance;
        // ReorderableList instance used to drive preferences
        protected static ReorderableList<ShelfLayer> preferencesList;
        // ShelfLayer instance who's name is being edited
        protected static ShelfLayer editItemName;

        // Indicates if preferences are shown inline
        protected static bool inlinePrefs;

#if ENABLE_HACKS
        // Permissive binding flags for calling internal editor methods
        protected const BindingFlags bindingFlags = (
            BindingFlags.NonPublic | BindingFlags.Public
            | BindingFlags.Static | BindingFlags.Instance
        );
#endif

#if ENABLE_HACKS && PREFERENCES_WINDOW_REDRAW_HACK
        // Reference of the PreferencesWindow to trigger Redraws
        protected static EditorWindow preferencesWindow;
#endif

        [PreferenceItem("The Shelf")]
        protected static void ShelfPreferences()
        {
#if ENABLE_HACKS && PREFERENCES_WINDOW_REDRAW_HACK
            // HACK: Get private PreferencesWindow to type to get a
            // reference of the preferences window to trigger its redraw
            if (!inlinePrefs && preferencesWindow == null)
            {
                var prefWindowType = typeof(EditorApplication).Assembly.GetType("UnityEditor.PreferencesWindow");
                if (prefWindowType != null)
                {
                    preferencesWindow = EditorWindow.GetWindow(prefWindowType);
                }
            }
#endif

            if (Data == null) return;

            // Initialize reorderable list used to edit shelf layers
            if (preferencesList == null)
            {
                preferencesList = new ReorderableList<ShelfLayer>();
                preferencesList.allowMultiSelection = false;
                preferencesList.undoTarget = Data;
                preferencesList.objectReferencesCallback = PreferencesLayersReferences;
                preferencesList.listItemNameCallback = PreferencesListItemName;
                preferencesList.listItemDrawCallback = PreferencesDrawListItem;
                preferencesList.List = Data.layers;
            }

            EditorGUILayout.LabelField("Drag layers to reorder:");

            if (preferencesList.OnGUI())
            {
#if ENABLE_HACKS && PREFERENCES_WINDOW_REDRAW_HACK
                if (!inlinePrefs && preferencesWindow != null)
                {
                    preferencesWindow.Repaint();
                }
#endif
                if (editorInstance != null)
                {
                    editorInstance.Repaint();
                }
            }

            // Make sure the selected shelf index stays valid
            if (Data.currentLayer >= Data.layers.Count)
            {
                Data.currentLayer = Data.layers.Count - 1;
            }

            EditorGUILayout.Space();

            // Add new layer
            if (GUILayout.Button("Add Layer"))
            {
                Data.layers.Add(new ShelfLayer()
                {
                    name = "New Layer",
                    objects = new List<UnityEngine.Object>()
                });
                EditorUtility.SetDirty(Data);
                if (editorInstance != null)
                {
                    editorInstance.Repaint();
                }
            }

            EditorGUILayout.Space();

            var enablePopup = EditorPrefs.GetBool(POPUP_PREFS_KEY, true);
            enablePopup = EditorGUILayout.Toggle("Enable Popup Shelf", enablePopup);
            EditorPrefs.SetBool(POPUP_PREFS_KEY, enablePopup);
        }

        // Preferences ReorderableList.objectReferenceCallback
        protected static UnityEngine.Object[] PreferencesLayersReferences(ReorderableList<ShelfLayer> list, ShelfLayer[] layers)
        {
            // Use the editor itself as stand-in for proper drag events
            return new UnityEngine.Object[] { Data };
        }

        // Preferences ListItemName for ReorderableList
        protected static string PreferencesListItemName(ReorderableList<ShelfLayer> list, ShelfLayer item)
        {
            return item.name;
        }

        // Preferences DrawListItem for ReorderableList
        protected static void PreferencesDrawListItem(ReorderableList<ShelfLayer> list, ShelfLayer item)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                // Stop editing an items name if enter or return is pressed
                if (editItemName == item
                        && Event.current.type == EventType.KeyDown
                        && (Event.current.keyCode == KeyCode.Return
                            || Event.current.keyCode == KeyCode.KeypadEnter))
                {
                    editItemName = null;
                    Event.current.Use();
                }

                // Layer name
                if (editItemName != item)
                {
                    GUILayout.Label(item.name, GUILayout.ExpandWidth(true));
                }
                else
                {
                    var newName = GUILayout.TextField(item.name, GUILayout.ExpandWidth(true));
                    if (newName != item.name)
                    {
                        Undo.RecordObject(Data, "Rename Shelf Layer Name");
                        item.name = newName;
                        EditorUtility.SetDirty(Data);
                    }
                }

                // Rename button
                var content = new GUIContent("...", "Rename Layer");
                if (GUILayout.Button(content, "label", GUILayout.Width(15)))
                {
                    if (editItemName == item)
                    {
                        editItemName = null;
                    }
                    else
                    {
                        editItemName = item;
                    }
                }

                // Delete button
                content = new GUIContent("x", "Delete Layer");
                var delete = GUILayout.Button(content, "label", GUILayout.Width(10));

                if (delete && item.objects != null && item.objects.Count > 0)
                {
                    delete = EditorUtility.DisplayDialog(
                        "Shelf",
                        "Are you shure you want to delete '" + item.name + "'?",
                        "Delete", "Cancel"
                    );
                }

                if (delete)
                {
                    list.RemoveItem(item);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        ////////////////////
        // Shelf

        // Variables used to handle drag-over layer switching
        protected int dragOverLayer = -1;
        protected float dragOverLayerTime;

        // Numbers of layers fit into the toolbar
        protected int showLayers = 0;
        // Last window width to detect window resizes
        protected float lastWindowWidth;
        // Shelf scroll offset
        protected Vector2[] shelfScroll;

        // Show inline preferences
        protected bool showInlinePrefs;

        // Reorderable list instance running the layers
        protected ReorderableList<UnityEngine.Object> reorderable
            = new ReorderableList<UnityEngine.Object>(null, true);

        // Access to the shelf's selection
        public IEnumerable<UnityEngine.Object> ShelfSelection
        {
            get
            {
                return reorderable.Selection;
            }
        }

        // SciptableObject.OnEnable
        protected void OnEnable()
        {
            editorInstance = this;

            // Setup reorderable list
            if (reorderable.objectReferencesCallback != ObjectReferences)
            {
                reorderable.emptyListItemHeight = SHELF_ITEM_HEIGHT;
                reorderable.undoTarget = Data;
                reorderable.objectReferencesCallback = ObjectReferences;
                reorderable.listItemNameCallback = ListItemName;
                reorderable.listItemContentCallback = ListItemContent;
                reorderable.listItemDrawCallback = DrawListItem;
                reorderable.listItemClickEvent += ListItemClick;
            }

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        // SciptableObject.OnDisable
        protected void OnDisable()
        {
            // Clean up scene references not longer in use
            var sceneRefs = AssetDatabase.LoadAllAssetsAtPath(DATA_PATH)
                .OfType<ShelfSceneReference>()
                .ToList();
            for (int i = sceneRefs.Count - 1; i >= 0; i--)
            {
                foreach (var layer in Data.layers)
                {
                    foreach (var item in layer.objects)
                    {
                        if (item == sceneRefs[i])
                        {
                            goto ContinueWithNextRef;
                        }
                    }
                }

                // Screne reference not found on any shelf
                DestroyImmediate(sceneRefs[i], true);

                ContinueWithNextRef:
                continue;
            }

            editorInstance = null;
            EditorUtility.SetDirty(Data);

            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        // SciptableObject.OnDestroy
        protected void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }

        // Undo.undoRedoPerformed
        protected void OnUndoRedo()
        {
            Repaint();
        }

        // Resolve a scene reference if it is one, else return item as-is
        protected UnityEngine.Object ResolveIfSceneReference(UnityEngine.Object item)
        {
            if (item is ShelfSceneReference)
            {
                return (item as ShelfSceneReference).Resolve();
            }
            else
            {
                return item;
            }
        }

        // objectReferenceCallback for ReorderableList
        protected UnityEngine.Object[] ObjectReferences(
            ReorderableList<UnityEngine.Object> list,
            UnityEngine.Object[] items
        )
        {
            return items.Select(i => ResolveIfSceneReference(i)).ToArray();
        }

        // ListItemContent for ReorderableList
        protected GUIContent ListItemContent(ReorderableList<UnityEngine.Object> list, UnityEngine.Object item)
        {
            var path = AssetDatabase.GetAssetPath(item);

            // ShelfSceneReference instances
            if (item is ShelfSceneReference)
            {
                var sceneRef = (item as ShelfSceneReference);
                var reference = sceneRef.Resolve();
                // Handle references that are currently out of scope
                Type type = (reference != null ? reference.GetType() : typeof(UnityEngine.Object));
                var content = new GUIContent(EditorGUIUtility.ObjectContent(null, type));
                if (reference == null)
                {
                    content.text = sceneRef.name;
                    content.tooltip = sceneRef.scenePath + "/" + sceneRef.componentName;
                }
                else
                {
                    content.text = reference.name;
                }
                return content;

                // Project Assets
            }
            else if (!string.IsNullOrEmpty(path))
            {
                return new GUIContent(
                    Path.GetFileNameWithoutExtension(path),
                    AssetDatabase.GetCachedIcon(path)
                );

                // Generic references
            }
            else
            {
                // Workaround for ObjectContent not returning icon for game objects
                // passed as first argument. Works if only the type is passed.
                var type = item.GetType();
                var content = new GUIContent(EditorGUIUtility.ObjectContent(null, type));
                if (item is GameObject)
                {
                    content.text = item.name;
                }
                else
                {
                    content.text = string.Format("{0} ({1})", item.name, type.Name);
                }
                return content;
            }
        }

        // ListItemName for ReorderableList
        protected string ListItemName(ReorderableList<UnityEngine.Object> list, UnityEngine.Object item)
        {
            return ListItemContent(list, item).text;
        }

        // DrawListItem for ReorderableList
        protected void DrawListItem(ReorderableList<UnityEngine.Object> list, UnityEngine.Object item)
        {
            UnityEngine.Object unityItem = item;
            if (item is ShelfSceneReference)
            {
                unityItem = (item as ShelfSceneReference).Resolve();
            }

            var originalColor = GUI.color;
            GUI.color = (unityItem != null ? originalColor : new Color(1.0f, 1.0f, 1.0f, 0.5f));

            var originalBackground = GUI.backgroundColor;
            if (list.IsSelected(item))
            {
                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            }

            var mainRect = EditorGUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label(ListItemContent(list, item), GUILayout.ExpandWidth(true), GUILayout.Height(SHELF_ITEM_HEIGHT));

                // Add component to game object for mono scripts
                if (item is MonoScript
                        && Selection.activeGameObject != null
                        && GUILayout.Button("a", "label", GUILayout.Width(10)))
                {
                    var comp = Selection.activeGameObject.AddComponent((item as MonoScript).GetClass());
                    // RegisterCreatedObjectUndo only works for non-persistent objects (?!)
                    if (!EditorUtility.IsPersistent(comp))
                    {
                        Undo.RegisterCreatedObjectUndo(comp, "Add Component");
                    }

                    // Instantiate prefabs
                }
                else if (item is GameObject
                                      && EditorUtility.IsPersistent(item)
                                      && GUILayout.Button("n", GUILayout.Width(20)))
                {
                    TextEditor textEditor = new TextEditor();
                    textEditor.text = item.name;
                    textEditor.OnFocus();
                    textEditor.Copy();
                }

                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    list.HandleListItemRemove(item);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUI.color = originalColor;
            GUI.backgroundColor = originalBackground;
            list.HandleListItemClick(mainRect, item);
        }

        // Check if a click changes the selection.
        // Returns 1 for single-item and 2 for multi-item selection
        protected int SelectionClickType()
        {
            if (Event.current.shift)
            {
                return 2;
            }

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                if (Event.current.command)
                {
                    return 1;
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (Event.current.control)
                {
                    return 1;
                }
            }

            return 0;
        }

        // ListItemClick for ReorderableList
        protected void ListItemClick(ReorderableList<UnityEngine.Object> list, UnityEngine.Object item)
        {
            if (item is ShelfSceneReference)
            {
                var reference = (item as ShelfSceneReference).Resolve();
                Selection.activeObject = reference;
            }
            else
            {
                var path = AssetDatabase.GetAssetPath(item);
                var isDirectory = false;
                if (!string.IsNullOrEmpty(path))
                {
                    path = Application.dataPath + "/../" + path;
                    isDirectory = Directory.Exists(path);
                }

                if (isDirectory)
                {
                    // Try to show the folder's contents in Unity 4's two-column
                    // project view instead of the parent folder's contents.
                    bool success = false;
                    var objectBrowserType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser");
                    if (objectBrowserType != null)
                    {
                        //var objectBrowser = EditorWindow.GetWindow(objectBrowserType);
                        var browsers = Resources.FindObjectsOfTypeAll(objectBrowserType);
                        if (browsers.Length == 0)
                        {
                            Debug.LogError("Shelf: Project browser not open, please open it to jump to folders.");
                        }
                        else
                        {
                            var field = objectBrowserType.GetField("m_ViewMode", bindingFlags);
                            if (field != null)
                            {
                                var viewMode = (int)field.GetValue(browsers[0]);
                                if (viewMode == 1)
                                {
                                    var method = objectBrowserType.GetMethod("ShowFolderContents", bindingFlags);
                                    if (method != null)
                                    {
                                        (browsers[0] as EditorWindow).Focus();
                                        method.Invoke(browsers[0], new object[] { item.GetInstanceID(), true });
                                        success = true;
                                    }
                                }
                            }
                        }
                    }

                    // Fall back to just pinging the folder
                    if (!success)
                    {
                        EditorGUIUtility.PingObject(item);
                    }
                }
                else if (path.EndsWith(".unity"))
                {
                    var goahead = true;
                    goahead = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    if (goahead)
                    {
                        OpenSceneDelayed(AssetDatabase.GetAssetPath(item));
                    }
                }
                else
                {
                    Selection.activeObject = item;
                }
            }
        }

        // Workaround for an error in GUILayoutUtility.GetLastRect() if the
        // scene is opened in the OnGUI call.
        protected void OpenSceneDelayed(string scenePath)
        {
            EditorApplication.CallbackFunction handler = null;
            handler = () =>
            {
                EditorApplication.delayCall -= handler;
                EditorSceneManager.OpenScene(scenePath);
            };
            EditorApplication.delayCall += handler;
        }

        // EditorWindow.OnHierarchyChange
        protected void OnHierarchyChange()
        {
            Repaint();
        }

        // EditorWindow.OnSelectionChange
        protected void OnSelectionChange()
        {
            Repaint();
        }

        // Clear the shelf's own selection
        protected void ClearSlection()
        {
            reorderable.Selection = null;
            Repaint();
        }

        // IHasCustomMenu.AddItemsToMenu
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Organize Shelves..."), false, () =>
            {
                showInlinePrefs = true;
            });

            menu.AddItem(new GUIContent("Enable Popup Shelf"), EditorPrefs.GetBool(POPUP_PREFS_KEY, true), () =>
            {
                EditorPrefs.SetBool(POPUP_PREFS_KEY, !EditorPrefs.GetBool(POPUP_PREFS_KEY));
            });
        }

        // EditorWindow.OnGUI
        protected void OnGUI()
        {
            if (Data == null || Data.layers == null) return;
            if (Data.currentLayer >= Data.layers.Count || Data.layers[Data.currentLayer] == null) Data.currentLayer = 0;
            if (shelfScroll == null) shelfScroll = new Vector2[Data.layers.Count];
            if (shelfScroll.Length != Data.layers.Count)
            {
                var oldArray = shelfScroll;
                shelfScroll = new Vector2[Data.layers.Count];
                Array.Copy(oldArray, shelfScroll, Math.Min(oldArray.Length, shelfScroll.Length));
            }

            var current = Data.layers[Data.currentLayer];
            string windowTitle = "Shelf";

            // Force repaint if undo is triggered (only works when window has focus)
            if (Event.current.type == EventType.ValidateCommand
                    && Event.current.commandName == "UndoRedoPerformed")
            {
                Repaint();
            }

            // Force displaying all layers when the window is enlargened
            // to re-fit the highest number of layers into the toolbar
            if (Math.Abs(position.width - lastWindowWidth) > 5)
            {
                if (position.width > lastWindowWidth)
                {
                    lastWindowWidth = position.width;
                    showLayers = int.MaxValue;
                }
                else
                {
                    lastWindowWidth = position.width;
                }
            }

            // Delay setting of showLayers until the end of the method
            int nextShowLayers = showLayers;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                bool draggingOver = false;
                float width = TOOLBAR_EXTRA_WIDTH + (showLayers < Data.layers.Count ? 12 : 0);
                for (int i = 0; i < showLayers && i < Data.layers.Count; i++)
                {
                    var layer = Data.layers[i];

                    var active = (Data.currentLayer == i);
                    if (GUILayout.Toggle(active, layer.name, EditorStyles.toolbarButton) != active)
                    {
                        Data.currentLayer = i;
                        return;
                    }

                    Rect buttonRect = GUILayoutUtility.GetLastRect();
                    if (buttonRect.Contains(Event.current.mousePosition))
                    {
                        // Drag-over layer switching
                        if (Event.current.type == EventType.DragUpdated
                                && DragAndDrop.objectReferences.Length > 0)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                            if (dragOverLayer != i)
                            {
                                dragOverLayer = i;
                                dragOverLayerTime = Time.realtimeSinceStartup;
                            }
                            draggingOver = true;

                            // Dropping objects on layer button
                        }
                        else if (Event.current.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            layer.objects.AddRange(DragAndDrop.objectReferences);
                            Data.currentLayer = i;

                            Event.current.Use();
                            return;
                        }
                    }

                    // Stop showing layers if they don't fit anymore
                    width += buttonRect.width;
                    if (width > position.width)
                    {
                        nextShowLayers = i;
                        break;
                    }
                }

                // Reset tracking of toolbar hovering
                if (!draggingOver && (
                    Event.current.type == EventType.DragUpdated
                 || Event.current.type == EventType.Repaint)
                )
                {
                    dragOverLayer = -1;
                }

                // Put additional layers into dropdown menu
                if (showLayers < Data.layers.Count)
                {
                    if (Data.currentLayer >= showLayers)
                    {
                        windowTitle = "Shelf: " + current.name;
                    }

                    if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(12)))
                    {
                        var menu = new GenericMenu();

                        for (int i = showLayers; i < Data.layers.Count; i++)
                        {
                            menu.AddItem(
                                new GUIContent(i + ": " + Data.layers[i].name),
                                (Data.currentLayer == i),
                                SwitchToLayer, i
                            );
                        }

                        menu.ShowAsContext();
                        Event.current.Use();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (reorderable.List != current.objects)
            {
                reorderable.List = current.objects;
            }

            shelfScroll[Data.currentLayer] = EditorGUILayout.BeginScrollView(shelfScroll[Data.currentLayer]);
            {
                if (showInlinePrefs)
                {
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Return"))
                    {
                        showInlinePrefs = false;
                    }

                    EditorGUILayout.Space();

                    inlinePrefs = true;
                    ShelfPreferences();
                    inlinePrefs = false;

                }
                else
                {
                    if (reorderable.OnGUI())
                    {
                        Repaint();
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            if (!showInlinePrefs)
            {
                // Convert scene references to ShelfSceneReference instances
                for (int i = 0; i < current.objects.Count; i++)
                {
                    if (!EditorUtility.IsPersistent(current.objects[i]))
                    {
                        // We're only able to store references to Components or GameObjects
                        if (current.objects[i] is GameObject || current.objects[i] is Component)
                        {
                            current.objects[i] = ShelfSceneReference.Create(current.objects[i]);
                            AssetDatabase.AddObjectToAsset(current.objects[i], Data);
                            EditorUtility.SetDirty(Data);
                        }
                        else
                        {
                            Debug.LogError("Non-asset reference " + current.objects[i].name + " is neither " +
                                "a GameObject nor a Component and cannot be saved on the shelf.");
                            current.objects.RemoveAt(i);
                            i--;
                        }
                    }
                }

                // Clear selection
                if (Event.current.type == EventType.MouseUp
                        && Event.current.clickCount == 1)
                {
                    ClearSlection();
                }

                // Select all items on current shelf
                if (Event.current.commandName == "SelectAll")
                {
                    reorderable.SelectAll();
                    Event.current.Use();
                }
            }

            // Apply window title
            if (titleContent == null || titleContent.text != windowTitle)
            {
                titleContent = new GUIContent(windowTitle);
            }

            // Save shelf size
            if (EditorPrefs.GetFloat(SHELF_WIDTH_KEY) != position.width)
            {
                EditorPrefs.SetFloat(SHELF_WIDTH_KEY, position.width);
            }
            if (EditorPrefs.GetFloat(SHELF_HEIGHT_KEY) != position.height)
            {
                EditorPrefs.SetFloat(SHELF_HEIGHT_KEY, position.height);
            }

            // Apply showLayers now to avoid layout errors
            showLayers = nextShowLayers;
        }

        // SwitchToLayer context menu callback
        protected void SwitchToLayer(object userData)
        {
            Data.currentLayer = (int)userData;
            Repaint();
        }

        // EditorWindow.Update
        protected void Update()
        {
            // Switch to a layer when hovering over the button in the toolbar while dragging
            if (dragOverLayer >= 0
                    && dragOverLayerTime + LAYER_DRAG_SWITCH_TIME < Time.realtimeSinceStartup)
            {
                Data.currentLayer = dragOverLayer;
                dragOverLayer = -1;
                Repaint();
            }
        }
    }

}