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

using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace CYM.Utile
{

    /// <summary>
    /// Shelf reference.
    /// </summary>
    public class ShelfSceneReference : ScriptableObject
{
	// Create a new scene reference
	public static ShelfSceneReference Create(UnityEngine.Object original)
	{
		// Create new instance and apply name
		var sceneRef = ScriptableObject.CreateInstance<ShelfSceneReference>();
		sceneRef.name = original.name;

		// Setup Component/GameObject
		Transform current;
		if (original is Component) {
			sceneRef.componentName = original.GetType().Name;
			current = (original as Component).transform;
		} else if (original is GameObject) {
			current = (original as GameObject).transform;
		} else {
			throw new Exception("Can only create scene references form Components and GameObjects.");
		}

		// Compile path, using [n] notation for game objects with the same name
		var parts = new List<string>();
		do {
			var name = current.name;

			// Find the index of the game object
			var index = 0;
			if (current.parent != null) {
				foreach (Transform sibling in current.parent) {
					if (sibling != current && sibling.name == name) {
						index++;
					} else if (sibling == current) {
						break;
					}
				}
			}

			parts.Insert(0, current.name + (index > 0 ? "[" + index + "]" : ""));
			current = current.parent;
		} while (current != null);

		// Copile final path
		sceneRef.scenePath = "/" + string.Join("/", parts.ToArray());

		return sceneRef;
	}

	// Path to the object in the scene
	public string scenePath;
	// Name of the component (if any)
	public string componentName;

	// Resolve the scene reference
	public UnityEngine.Object Resolve()
	{
		// Regex used to parse [n] index notation
		var indexRegex = new Regex(@"\[(\d+)\]$");

		var parts = scenePath.Split('/');
		if (parts.Length < 2) {
			return null;
		}

		// Find root game object
		var root = GameObject.Find("/" + parts[1]);
		if (root == null) {
			return null;
		}

		// Traverse through path
		var current = root.transform;
		for (int i = 2; i < parts.Length; i++) {
			var searchName = parts[i];
			var searchIndex = 0;

			// Apply [n] index notation
			var match = indexRegex.Match(searchName);
			if (match.Success) {
				searchName = searchName.Substring(0, searchName.Length - match.Groups[1].Value.Length - 2);
				searchIndex = int.Parse(match.Groups[1].Value);
			}

			// Find child with name and index
			var index = 0;
			Transform found = null;
			foreach (Transform child in current) {
				if (child.name == searchName) {
					if (index == searchIndex) {
						found = child;
						break;
					}
					index++;
				}
			}

			if (found == null) {
				return null;
			} else {
				current = found;
			}
		}

		if (string.IsNullOrEmpty(componentName)) {
			return current.gameObject;
		} else {
			return current.GetComponent(componentName);
		}
	}
}

}