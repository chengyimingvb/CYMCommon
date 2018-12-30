using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Reflection;
namespace CYM
{
    public static class PreFabOverride
    {
        public static void MakeFieldsOverride(Object test)
        {
#if UNITY_EDITOR
            SerializedObject ser = new SerializedObject(test);

            FieldInfo[] infos = test.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo info in infos)
            {

                object[] attributes = info.GetCustomAttributes(true);

                bool isExposed = false;

                foreach (object o in attributes)
                {
                    if (o.GetType() == typeof(PreFabOverrideAttribute))
                    {
                        isExposed = true;
                        break;
                    }
                }

                if (!isExposed)
                    continue;


                ser.Update();
                SerializedProperty p = ser.FindProperty(info.Name);

                if (p.prefabOverride) continue;

                switch (p.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        p.intValue += 1;
                        break;
                    case SerializedPropertyType.Float:
                        p.floatValue += 1f;
                        break;
                    case SerializedPropertyType.Boolean:
                        p.boolValue = !p.boolValue;
                        break;
                    case SerializedPropertyType.String:
                        p.stringValue = p.stringValue + "$";
                        break;
                    case SerializedPropertyType.Vector3:
                        Vector3 temp = p.vector3Value;
                        temp.x += 1f;
                        temp.y += 1f;
                        temp.z += 1f;
                        p.vector3Value = temp;
                        break;
                    case SerializedPropertyType.Vector4:
                        Vector4 temp3 = p.vector4Value;
                        temp3.x += 1f;
                        temp3.y += 1f;
                        temp3.z += 1f;
                        temp3.w += 1f;
                        p.vector4Value = temp3;
                        break;
                    case SerializedPropertyType.Vector2:
                        Vector2 temp2 = p.vector2Value;
                        temp2.x += 1f;
                        temp2.y += 1f;
                        p.vector2Value = temp2;
                        break;
                    case SerializedPropertyType.Color:
                        Color temp_color = p.colorValue;
                        temp_color.r += 1f;
                        temp_color.g += 1f;
                        temp_color.b += 1f;
                        temp_color.a += 1f;
                        p.colorValue = temp_color;
                        break;
                    case SerializedPropertyType.Enum:
                        if (p.enumValueIndex == p.enumNames.Length - 1)
                            p.enumValueIndex = 0;
                        else
                            p.enumValueIndex += 1;
                        break;
                    default:
                        break;
                }

                ser.ApplyModifiedProperties();
                ser.Update();
                PrefabUtility.RecordPrefabInstancePropertyModifications(test);
                p = ser.FindProperty(info.Name);

                switch (p.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        p.intValue -= 1;
                        break;
                    case SerializedPropertyType.Float:
                        p.floatValue -= 1f;
                        break;
                    case SerializedPropertyType.Boolean:
                        p.boolValue = !p.boolValue;
                        break;
                    case SerializedPropertyType.String:
                        p.stringValue = p.stringValue.Remove(p.stringValue.Length - 1);
                        break;
                    case SerializedPropertyType.Vector3:
                        Vector3 temp = p.vector3Value;
                        temp.x -= 1f;
                        temp.y -= 1f;
                        temp.z -= 1f;
                        p.vector3Value = temp;
                        break;
                    case SerializedPropertyType.Vector2:
                        Vector2 temp2 = p.vector2Value;
                        temp2.x -= 1f;
                        temp2.y -= 1f;
                        p.vector2Value = temp2;
                        break;
                    case SerializedPropertyType.Color:
                        Color temp_color = p.colorValue;
                        temp_color.r -= 1f;
                        temp_color.g -= 1f;
                        temp_color.b -= 1f;
                        temp_color.a -= 1f;
                        p.colorValue = temp_color;
                        break;
                    case SerializedPropertyType.Vector4:
                        Vector4 temp3 = p.vector4Value;
                        temp3.x -= 1f;
                        temp3.y -= 1f;
                        temp3.z -= 1f;
                        temp3.w -= 1f;
                        p.vector4Value = temp3;
                        break;
                    case SerializedPropertyType.Enum:
                        if (p.enumValueIndex == 0)
                            p.enumValueIndex = p.enumNames.Length - 1;
                        else
                            p.enumValueIndex -= 1;
                        break;
                    default:
                        break;
                }

                ser.ApplyModifiedProperties();
            }
#endif
        }
        public static void MakeFieldsOverride2(Object test)
        {
#if UNITY_EDITOR
            SerializedObject ser = new SerializedObject(test);

            FieldInfo[] infos = test.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo info in infos)
            {
                object[] attributes = info.GetCustomAttributes(true);

                bool isExposed = false;

                foreach (object o in attributes)
                {
                    if (o.GetType() == typeof(PreFabOverrideAttribute))
                    {
                        isExposed = true;
                        break;
                    }
                }

                SerializedProperty p = ser.FindProperty(info.Name);
                if (!isExposed)
                {
                    p.prefabOverride = false;
                }
                else
                {
                    p.prefabOverride = true;
                }
                ser.ApplyModifiedProperties();
                ser.Update();
                PrefabUtility.RecordPrefabInstancePropertyModifications(test);
            }
#endif
        }

    }

}