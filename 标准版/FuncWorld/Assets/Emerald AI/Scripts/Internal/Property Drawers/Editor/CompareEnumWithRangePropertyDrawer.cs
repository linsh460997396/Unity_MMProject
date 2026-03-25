using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[CustomPropertyDrawer(typeof(CompareEnumWithRangeAttribute))]
public class CompareEnumWithRangePropertyDrawer : PropertyDrawer
{
    #region Fields

    // Reference to the attribute on the property.
    CompareEnumWithRangeAttribute CompareEnumWithRangeAttribute;

    // Field that is being compared.
    SerializedProperty comparedField;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!ShowMe(property))
            return 0f;

        // The height of the property should be defaulted to the default height.
        return base.GetPropertyHeight(property, label);
    }

    /// <summary>
    /// Errors default to showing the property.
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        CompareEnumWithRangeAttribute = attribute as CompareEnumWithRangeAttribute;
        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, CompareEnumWithRangeAttribute.comparedPropertyName) : CompareEnumWithRangeAttribute.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            Debug.LogError("Cannot find property with name: " + path);
            return true;
        }
        
        switch (comparedField.type)
        {
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue1) || 
                CompareEnumWithRangeAttribute.comparedValue2 != null && comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue2) || 
                CompareEnumWithRangeAttribute.comparedValue3 != null && comparedField.enumValueIndex.Equals((int)CompareEnumWithRangeAttribute.comparedValue3);
            default:
                Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // If the condition is met, simply draw the field.
        if (ShowMe(property))
        {
            Rect offsetPosition = position;
            //offsetPosition.pixelX = offsetPosition.pixelX + 30;
            //offsetPosition.width = offsetPosition.width - 30;

            label = EditorGUI.BeginProperty(position, label, property);

            EditorGUI.BeginChangeCheck();
            if (CompareEnumWithRangeAttribute.styleType == CompareEnumWithRangeAttribute.StyleType.FloatSlider)
            {
                var newValue = EditorGUI.Slider(offsetPosition, label, property.floatValue, CompareEnumWithRangeAttribute.min, CompareEnumWithRangeAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = newValue;
                }
            }
            else if (CompareEnumWithRangeAttribute.styleType == CompareEnumWithRangeAttribute.StyleType.IntSlider)
            {
                var newValue = EditorGUI.IntSlider(offsetPosition, label, property.intValue, (int)CompareEnumWithRangeAttribute.min, (int)CompareEnumWithRangeAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    property.intValue = newValue;
                }
            }

            EditorGUI.EndProperty();
            //EditorGUI.PropertyField(position, property, label);
        }
    }
}