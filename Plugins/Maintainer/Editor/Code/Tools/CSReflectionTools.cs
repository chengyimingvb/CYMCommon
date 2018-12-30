#region copyright
//--------------------------------------------------------------------
// Copyright (C) 2015 Dmitriy Yukhanov - focus [http://codestage.net].
//--------------------------------------------------------------------
#endregion

using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeStage.Maintainer.Tools
{
	public class CSReflectionTools
	{
		// for caching
		private static PropertyInfo sortingLayersPropertyInfo;
		private static MethodInfo getLightmapSettingsMethodInfo;
		private static MethodInfo getRenderSettingsMethodInfo;
		private static MethodInfo getMainAssetInstanceIDMethodInfo;

		private static Action<SerializedObject, InspectorMode> inspectorModeCachedSetter;

		public static void SetInspectorToDebug(SerializedObject serializedObject)
		{
			if (inspectorModeCachedSetter == null)
			{
				var pi = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

				if (pi != null)
				{
					var mi = pi.GetSetMethod(true);
					if (mi != null)
					{
						inspectorModeCachedSetter = (Action<SerializedObject, InspectorMode>)Delegate.CreateDelegate(typeof(Action<SerializedObject, InspectorMode>), mi);
					}
					else
					{
						Debug.LogError(Maintainer.ConstructError("Can't get the setter for the SerializedObject.inspectorMode property!"));
						return;
					}
				}
				else
				{
					Debug.LogError(Maintainer.ConstructError("Can't get the SerializedObject.inspectorMode property!"));
					return;
				}
			}

			inspectorModeCachedSetter.Invoke(serializedObject, InspectorMode.Debug);
		}

		public static PropertyInfo GetSortingLayersPropertyInfo()
		{
			if (sortingLayersPropertyInfo == null)
			{
				sortingLayersPropertyInfo = typeof(InternalEditorUtility).GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			}

			return sortingLayersPropertyInfo;
		}

		public static MethodInfo GetGetLightmapSettingsMethodInfo()
		{
			if (getLightmapSettingsMethodInfo == null)
			{
				getLightmapSettingsMethodInfo = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.NonPublic | BindingFlags.Static);
			}

			return getLightmapSettingsMethodInfo;
		}

		public static MethodInfo GetGetRenderSettingsMethodInfo()
		{
			if (getRenderSettingsMethodInfo == null)
			{
				getRenderSettingsMethodInfo = typeof(RenderSettings).GetMethod("GetRenderSettings", BindingFlags.NonPublic | BindingFlags.Static);
			}

			return getRenderSettingsMethodInfo;
		}

		public static MethodInfo GetGetMainAssetInstanceIDMethodInfo()
		{
			if (getMainAssetInstanceIDMethodInfo == null)
			{
				getMainAssetInstanceIDMethodInfo = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID", BindingFlags.NonPublic | BindingFlags.Static);
			}

			return getMainAssetInstanceIDMethodInfo;
		}

		public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
		{
			PropertyInfo propInfo;
			do
			{
				propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				type = type.BaseType;
			}
			while (propInfo == null && type != null);

			return propInfo;
		}
	}
}