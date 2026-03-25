using System;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public class TagAttribute : PropertyAttribute
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TagAttribute))]
    private class TagAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

           
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, $"{nameof(TagAttribute)} can only be used for strings!", MessageType.Error);
                return;
            }

            if (!UnityEditorInternal.InternalEditorUtility.tags.Contains(property.stringValue))
            {
                property.stringValue = "";
            }

            var color = GUI.color;
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                GUI.color = Color.red;
            }

            EditorGUI.BeginChangeCheck();
            var TempTag = EditorGUI.TagField(position, label, property.stringValue);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = TempTag;
            }

            GUI.color = color;

            EditorGUI.EndProperty();
        }
    }
#endif
}