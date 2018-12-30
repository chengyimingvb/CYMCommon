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

namespace CYM.Utile
{

    /// <summary>
    /// Main shelf data container.
    /// </summary>
    public class ShelfData : ScriptableObject
{
	// Layers of the shelf
	public List<ShelfLayer> layers;
	// Selected shelf layer
	public int currentLayer;
	
	// ScriptableObject.OnEnable()
	protected void OnEnable()
	{
		if (layers == null || layers.Count == 0) {
			// Initialize new shelf
			layers = new List<ShelfLayer>();
			layers.Add(new ShelfLayer() {
				name = "First",
				objects = new List<UnityEngine.Object>()
			});
			layers.Add(new ShelfLayer() {
				name = "Second",
				objects = new List<UnityEngine.Object>()
			});
			layers.Add(new ShelfLayer() {
				name = "Third",
				objects = new List<UnityEngine.Object>()
			});
			
			// Add readme
			//var readme = AssetDatabase.LoadMainAssetAtPath("Assets/Editor/Shelf/readme.txt");
			//if (readme != null) {
			//	layers[0].objects.Add(readme);
			//}
		}
	}
}

/// <summary>
/// Individual shelf layer.
/// </summary>
[Serializable]
public class ShelfLayer
{
	// Name of the layer
	public string name;
	// Objects on the shelf layer
	public List<UnityEngine.Object> objects;
}

}