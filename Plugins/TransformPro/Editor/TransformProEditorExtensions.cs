namespace UntitledGames.Transforms
{
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    // TODO: Add helper functions to Transform 
    public static class TransformProEditorExtensions
    {
        public static void DebugLog(this SerializedObject serializedObject)
        {
            SerializedProperty property = serializedObject.GetIterator();
            property.Next(true);

            StringBuilder stringBuilder = new StringBuilder();
            while (property.Next(true))
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append(property.propertyPath);
            }

            Debug.Log(string.Format("{0}\n{1}", serializedObject.targetObject.GetType(), stringBuilder));
        }

        public static bool IsPrefab(this Transform transform)
        {
            PrefabType prefabType = PrefabUtility.GetPrefabType(transform.gameObject);
            return prefabType != PrefabType.None;
        }
    }
}
