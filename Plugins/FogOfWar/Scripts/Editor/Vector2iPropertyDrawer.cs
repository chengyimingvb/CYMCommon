using UnityEditor;
using UnityEngine;

namespace FoW
{
    [CustomPropertyDrawer(typeof(Vector2i))]
    public class RangeDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
            Rect rect = new Rect(position.x, position.y, position.width / 2, position.height);
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("x"), GUIContent.none);
            
            rect.x += rect.width;
            EditorGUI.PropertyField(rect, property.FindPropertyRelative("y"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
