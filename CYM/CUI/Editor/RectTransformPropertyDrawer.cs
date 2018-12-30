using System;
using UnityEditor;
using UnityEngine;

namespace CYM.UI
{
    [CustomPropertyDrawer(typeof(RectTransform))]
    public class RectTransformPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            base.OnGUI(pos,prop,label);
            //label = EditorGUI.BeginProperty(pos, label, prop);
            //EditorGUI.Vector3Field(new Rect(pos.x, pos.y, 100, 50),"LocalPos", prop.vector3Value);
            //EditorGUI.EndProperty();
        }

    }

}