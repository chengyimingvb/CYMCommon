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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CYM.Utile
{

/// <summary>
/// Reorderable list.
/// </summary>
public class ReorderableList<T> where T : class
{
	// Name used for generic objects in Get/SetGenericData
	protected const string DATA_OBJECTS = "ReorderableListObjects";
	// Name used for source of a drag in Get/SetGenericData
	protected const string DATA_SOURCE = "ReorderableListSource";
	// Name used for object index of a drag in Get/SetGenericData
	protected const string DATA_INDEX = "ReorderableListIndex";
	
	// Target used to register undo steps
	public UnityEngine.Object undoTarget;
	
	// Allow duplicate items in the list
	public bool allowDuplicates;
	// Allow selecting multiple list items with shift or command/ctrl
	public bool allowMultiSelection = true;
	// Accept any drops received
	public bool limitDropsToUsedRect = false;
	// Current index being dragged over
	public int draggingOver = -1;
	// Height of the default empty list item (NaN = height of empty label)
	public float emptyListItemHeight = float.NaN;
	// Next dragging over value
	protected int nextDraggingOver = -1;
	// Last rect of drawn list
	protected Rect lastListRect;
	
	// Callback used to draw list items
	public Action<ReorderableList<T>, T> listItemDrawCallback;
	// Callback used to draw empty list items (while dragging over)
	public Action<ReorderableList<T>> emptyListItemDrawCallback;
	// Callback used to determine a list item's name for display
	// using the default list draw callback and for dragging
	public Func<ReorderableList<T>, T, string> listItemNameCallback;
	// Callback used to set the GUIContent used for list items
	// using the default list draw callback
	public Func<ReorderableList<T>, T, GUIContent> listItemContentCallback;
	// Callback used to get the unity object references that should
	// be used for lists based on non-unity objects.
	public Func<ReorderableList<T>, T[], UnityEngine.Object[]> objectReferencesCallback;
	// Callback used to remove list items from the list
	public Action<ReorderableList<T>, int> listItemRemoveCallback;
	
	// Action sent by the default list item draw callback
	public event Action<ReorderableList<T>, T> listItemClickEvent;
	
	// List to be displayed and reordered
	private List<T> _list;
	public List<T> List {
		get {
			return _list;
		}
		set {
			_list = value;
			nextDraggingOver = -1;
		}
	}

	// Only or last selected item
	public T SingleSelection {
		get {
			return _selection.FirstOrDefault();
		}
		set {
			_selection.Clear();
			if (value != null) {
				_selection.Add(value);
			}
			lastSelected = value;
		}
	}
	// Current selection in list
	private HashSet<T> _selection = new HashSet<T>();
	public HashSet<T> Selection {
		get {
			return _selection;
		}
		set {
			if (value == null) {
				_selection.Clear();
			} else {
				_selection = value;
			}
			lastSelected = null;
		}
	}

	// Current index enumerating the list items
	public int CurrentIndex {
		get {
			return _CurrentIndex;
		}
		set {
			_CurrentIndex = value;
		}
	}
	private int _CurrentIndex = -1;

	// Style used for the empty list item
	protected static GUIStyle emptyBoxStyle;
	// Last selected item, used for multi-selection
	protected T lastSelected;
	// Track if an item has been removed during the redraw loop
	protected bool itemRemoved;

	// Constructor
	public ReorderableList(List<T> list = null, bool allowDuplicates = false)
	{
		this.List = list;
		this.allowDuplicates = allowDuplicates;
		
		this.listItemDrawCallback = DrawListItem;
		this.emptyListItemDrawCallback = DrawEmptyListItem;
		this.listItemNameCallback = ListItemName;
		this.listItemContentCallback = ListItemContent;
		this.listItemRemoveCallback = ListItemRemove;
	}

	// Returns if an item in the list is selected
	// (does not check if item is actually in the list)
	public bool IsSelected(T item)
	{
		return Selection.Contains(item);
	}

	// Select all items in the current list
	public void SelectAll()
	{
		foreach (var item in List) {
			Selection.Add(item);
		}
	}

	// Remove an item from the list
	public void RemoveItem(T item)
	{
		var index = List.IndexOf(item);
		if (index >= 0) {
			listItemRemoveCallback(this, index);
			itemRemoved = true;

			if (CurrentIndex > 0 && index <= CurrentIndex) {
				CurrentIndex--;
			}

			if (Selection.Contains(item)) {
				Selection.Remove(item);
			}
		}
	}

	// Remove items from the list
	public void RemoveItems(IEnumerable<T> items)
	{
		foreach (var item in items.ToArray()) {
			RemoveItem(item);
		}
	}

	// Check if a click changes the selection.
	// Returns 1 for single-item and 2 for multi-item selection
	public static int SelectionClickType()
	{
		if (Event.current.shift) {
			return 2;
		}

		if (Application.platform == RuntimePlatform.OSXEditor) {
			if (Event.current.command) {
				return 1;
			}
		} else if (Application.platform == RuntimePlatform.WindowsEditor) {
			if (Event.current.control) {
				return 1;
			}
		}

		return 0;
	}

	// Handle a click for item in rect, managing selection and 
	// calling listItemclickEvent for normal clicks
	public void HandleListItemClick(Rect rect, T item)
	{
		// Detect clicks
		if (Event.current.type == EventType.MouseUp
				&& Event.current.clickCount == 1
				&& rect.Contains(Event.current.mousePosition)) {
			var clickType = SelectionClickType();

			// Regular action-click
			if (clickType == 0 || !allowMultiSelection) {
				if (listItemClickEvent != null) {
					listItemClickEvent(this, item);
				}

				if (Selection.Count > 1 || SingleSelection != item) {
					SingleSelection = item;
				} else {
					SingleSelection = null;
				}
			}

			if (allowMultiSelection && clickType > 0) {
				// Range-add multi-selection
				if (clickType == 2) {
					clickType = 1; // Fall back to single if anything fails

					if (lastSelected != null) {
						var lastIndex = List.IndexOf(lastSelected);
						var thisIndex = List.IndexOf(item);
						if (lastIndex >= 0 && lastIndex != thisIndex) {
							clickType = 2;

							var lowIndex = Mathf.Min(lastIndex, thisIndex);
							var highIndex = Mathf.Max(lastIndex, thisIndex);
							var newItems = List.GetRange(lowIndex, highIndex - lowIndex + 1);
							foreach (var newItem in newItems) { Selection.Add(newItem); }
						}
					}
				}

				// Single-add multi-selection
				if (clickType == 1) {
					if (!Selection.Contains(item)) {
						Selection.Add(item);
					} else {
						Selection.Remove(item);
					}
				}

				lastSelected = item;
			}

			Event.current.Use();
		}
	}

	// Handle removing of a list item, removing the full selection
	// if multi-selection is allowed
	public void HandleListItemRemove(T item)
	{
		if (!allowMultiSelection) {
			RemoveItem(item);
		} else {
			if (Selection.Contains(item)) {
				RemoveItems(Selection);
			} else {
				Selection.Clear();
				RemoveItem(item);
			}
		}
	}

	// Get dragged objects from DragAndDrop or null if none exists
	// or none is of the requried type.
	protected T[] GetDraggedObjects()
	{
		IEnumerable<object> items = DragAndDrop.GetGenericData(DATA_OBJECTS) as object[];
		if (items == null || !items.Any()) {
			items = DragAndDrop.objectReferences;
		}

		return items.OfType<T>().ToArray();
	}
	
	// Get the list index of the dragged item if the item is being
	// dragged from the current list or -1 otherwise.
	protected int GetListIndex()
	{
		if (DragAndDrop.GetGenericData(DATA_SOURCE) == List) {
			return (int?)DragAndDrop.GetGenericData(DATA_INDEX) ?? -1;
		} else {
			return -1;
		}
	}
	
	// Register an undo operation on the undo target
	protected void RegisterUndo(string name)
	{
		if (undoTarget != null) {
			EditorUtility.SetDirty(undoTarget);
			Undo.RecordObject(undoTarget, name);
		}
	}
	
	// Call this in your editor OnGUI code.
	public bool OnGUI()
	{
		if (List == null) return false;
		
		var needsRepaint = false;
		needsRepaint |= DrawList();
		needsRepaint |= AcceptDrags();
		return needsRepaint;
	}
	
	// Draw the reorderable list
	protected bool DrawList()
	{
		draggingOver = nextDraggingOver;
		lastListRect = EditorGUILayout.BeginVertical();
		itemRemoved = false;
		
		// Reset dragged over element if mouse leaves drop region
		if (limitDropsToUsedRect
				&& Event.current.type == EventType.DragUpdated 
		    	&& !lastListRect.Contains(Event.current.mousePosition)) {
			nextDraggingOver = -1;
		}
		
		// Draw an empty item if list is empty to serve as drop target
		if (List.Count == 0) {
			emptyListItemDrawCallback(this);
		}
		
		for (CurrentIndex = 0; CurrentIndex < List.Count; CurrentIndex++) {
			var item = List[CurrentIndex];

			// Remove null items
			if (item == null 
					|| (item is UnityEngine.Object 
					&& (item as UnityEngine.Object) == null)) {
				List.RemoveAt(CurrentIndex);
				CurrentIndex--;
				continue;
			}
			
			// Draw empty item
			if (draggingOver == CurrentIndex) {
				emptyListItemDrawCallback(this);
			}
			
			// Skip item being dragged
			if (draggingOver >= 0 && GetListIndex() == CurrentIndex 
			    	&& (!allowDuplicates || !Event.current.alt)) {
				continue;
			}
			
			// Draw item
			listItemDrawCallback(this, item);

			var mouseOver = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			
			// Draw last empty item
			if (draggingOver > CurrentIndex && CurrentIndex == List.Count - 1) {
				emptyListItemDrawCallback(this);
			}
			
			// Check dragging over
			if (Event.current.type == EventType.DragUpdated 
			    	&& GetDraggedObjects().Length > 0
			    	&& mouseOver) {
				if (draggingOver == CurrentIndex)
					nextDraggingOver = CurrentIndex + 1;
				else
					nextDraggingOver = CurrentIndex;
			}

			// Reset drag data on mouse down
			if (mouseOver && Event.current.type == EventType.MouseDown) {
				DragAndDrop.PrepareStartDrag();
			}

			// Initiate drag
			if (mouseOver
			   		&& Event.current.type == EventType.MouseDrag
			   		&& DragAndDrop.GetGenericData(DATA_SOURCE) == null) {
				DragAndDrop.SetGenericData(DATA_SOURCE, List);
				DragAndDrop.SetGenericData(DATA_INDEX, CurrentIndex);

				var typeIsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

				T[] dragItems = null;
				if (Selection.Count > 0 && Selection.Contains(item)) {
					dragItems = List.Where(i => Selection.Contains(i)).ToArray();
				} else {
					dragItems = new T[] { item };
				}

				// DATA_OBJECTS always contains original shelf items as object[]
				DragAndDrop.SetGenericData(DATA_OBJECTS, dragItems.Cast<object>().ToArray());

				// Use object reference callback
				if (objectReferencesCallback != null) {
					DragAndDrop.objectReferences = objectReferencesCallback(this, dragItems);

				// Set unity objects as object references
				} else if (typeIsUnityObject) {
					DragAndDrop.objectReferences = Selection.Cast<UnityEngine.Object>().ToArray();
				}

				// Set generic objects as generic data
				if (!typeIsUnityObject) {
					// Warn about drag events issue
					if (objectReferencesCallback == null) {
						Debug.LogWarning("ReorderableList used with a non-Unity object, " +
							"drag events won't be triggered correctly. Please set the " +
			                "objectReferencesCallback property to define a dummy unity " +
							"object to use in the actual object's stead.");
					}
				}

				if (DragAndDrop.objectReferences.Length > 1) {
					DragAndDrop.StartDrag("<Multiple>");
				} else {
					DragAndDrop.StartDrag(listItemNameCallback(this, item));
				}
				
				nextDraggingOver = CurrentIndex;
				
				Event.current.Use();
			}
		}
		
		EditorGUILayout.EndVertical();
		CurrentIndex = -1;

		return (draggingOver != nextDraggingOver || itemRemoved);
	}

	// Handle items being dragged upon the list
	protected bool AcceptDrags()
	{
		var objs = GetDraggedObjects();
		if (objs.Length > 0) {
			// Set visual mode
			if (Event.current.type == EventType.DragUpdated) {
				if (allowDuplicates && Event.current.alt) {
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				} else {
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;
				}
			}

			// Handle drag release
			if (Event.current.type == EventType.DragPerform
				&& (!limitDropsToUsedRect
					|| lastListRect.Contains(Event.current.mousePosition))) {
				DragAndDrop.AcceptDrag();

				if (draggingOver < 0) draggingOver = List.Count;

				if (DragAndDrop.GetGenericData(DATA_SOURCE) == List) {
					if (!allowDuplicates || !Event.current.alt) {
						RegisterUndo("Move List Items");

						foreach (var obj in objs) {
							var removeIndex = List.IndexOf(obj);
							if (removeIndex >= 0) {
								if (removeIndex < draggingOver) {
									draggingOver--;
								}
								List.RemoveAt(removeIndex);
							}
						}
					} else {
						RegisterUndo("Duplicate List Items");
					}

					draggingOver = Mathf.Min(Mathf.Max(draggingOver, 0), List.Count);
					List.InsertRange(draggingOver, objs);
				} else {
					RegisterUndo("Add List Items");
					List.InsertRange(draggingOver, objs);
				}

				nextDraggingOver = -1;
				Event.current.Use();
				return true;
			}

			if (Event.current.type == EventType.DragExited) {
				nextDraggingOver = -1;
				return true;
			}
		}

		return false;
	}

	// Get the name used for the list item
	protected static string ListItemName(ReorderableList<T> list, T item)
	{
		return item.ToString();
	}

	// Get the GUIContent used for the list items
	protected static GUIContent ListItemContent(ReorderableList<T> list, T item)
	{
		return new GUIContent(list.listItemNameCallback(list, item));
	}
	
	// Default delegate for drawing list items
	protected static void DrawListItem(ReorderableList<T> list, T item)
	{
		var originalBackground = GUI.backgroundColor;
		if (list.Selection.Contains(item)) {
			GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f);
		}

		EditorGUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label(list.listItemContentCallback(list, item), GUILayout.ExpandWidth(true));
		if (GUILayout.Button("x", GUIStyle.none, GUILayout.Width(10))) {
			list.HandleListItemRemove(item);
		}
		EditorGUILayout.EndHorizontal();

		GUI.backgroundColor = originalBackground;
		list.HandleListItemClick(GUILayoutUtility.GetLastRect(), item);
	}
	
	// Default delegate for drawing the empty list item
	protected static void DrawEmptyListItem(ReorderableList<T> list)
	{
		// Initialize style
		if (emptyBoxStyle == null) {
			emptyBoxStyle = new GUIStyle(GUI.skin.box);
			emptyBoxStyle.normal.background = null;
		}
		
		// Draw empty box
		EditorGUILayout.BeginVertical(emptyBoxStyle);
		if (float.IsNaN(list.emptyListItemHeight)) {
			GUILayout.Label("");
		} else {
			GUILayout.Space(list.emptyListItemHeight);
		}
		EditorGUILayout.EndVertical();
	}

	// Default delegate to remove list items
	protected static void ListItemRemove(ReorderableList<T> list, int index)
	{
		list.RegisterUndo("Remove List Item");
		list.List.RemoveAt(index);
	}
}

}