using UnityEditor;
using UnityEngine;
using System;

//Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
[CustomPropertyDrawer(typeof(DrawIfRangeAttribute))]
public class DrawIfRangePropertyDrawer : PropertyDrawer
{
    #region Fields

    // Reference to the attribute on the property.
    DrawIfRangeAttribute drawRanageIf;

    // Field that is being compared.
    SerializedProperty comparedField;

    bool conditionMet;

    #endregion

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        //if (!ShowMe(property))
        if (!conditionMet)
            return -2f;

        // The height of the property should be defaulted to the default height.
        return base.GetPropertyHeight(property, label);
    }

    /// <summary>
    /// Errors default to showing the property.
    /// </summary>
    private bool ShowMe(SerializedProperty property)
    {
        drawRanageIf = attribute as DrawIfRangeAttribute;

        // Replace propertyname to the value from the parameter
        string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawRanageIf.comparedPropertyName) : drawRanageIf.comparedPropertyName;

        comparedField = property.serializedObject.FindProperty(path);

        if (comparedField == null)
        {
            Debug.LogError("Cannot find property with name: " + path);
            return true;
        }

        // get the value & compare based on types
        switch (comparedField.type)
        { // Possible extend cases to support your own type
            case "bool":
                return comparedField.boolValue.Equals(drawRanageIf.comparedValue);
            case "Enum":
                return comparedField.enumValueIndex.Equals((int)drawRanageIf.comparedValue);
            //case "int":
                //return comparedField.intValue.Equals(drawRanageIf.comparedValue);
            //case "float":
                //return comparedField.floatValue.Equals(drawRanageIf.comparedValue);
            default:
                //Debug.LogError("Error: " + comparedField.type + " is not supported of " + path);
                return true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // If the condition is met, simply draw the field.
        if (ShowMe(property))
        {
            Rect offsetPosition = position;
            offsetPosition.x = offsetPosition.x + 30;
            offsetPosition.width = offsetPosition.width - 30;

            //string path = property.propertyPath.Contains(".") ? System.IO.Path.ChangeExtension(property.propertyPath, drawRanageIf.comparedPropertyName) : drawRanageIf.comparedPropertyName;
            //comparedField = property.serializedObject.FindProperty(path);
            var comparedFieldValue = comparedField;
            var comparedValue = drawRanageIf.comparedValue;
            //bool conditionMet = false;

            if (comparedField.type == "int" || comparedField.type == "float")
            {
                var numericComparedFieldValue = (int)comparedField.intValue;
                var numericComparedValue = (int)(drawRanageIf.comparedValue);

                // Is the condition met? Should the field be drawn?

                //numericComparedValue = new NumericType(drawRanageIf.comparedValue);
                
                // Compare the values to see if the condition is met.
                switch (drawRanageIf.comparisonType)
                {
                    case DrawIfRangeAttribute.ComparisonType.Equals:
                        if (comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.NotEqual:
                        if (!comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.GreaterThan:
                        if (numericComparedFieldValue > numericComparedValue)
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.SmallerThan:
                        if (numericComparedFieldValue < numericComparedValue)
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.SmallerOrEqual:
                        if (numericComparedFieldValue <= numericComparedValue)
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.GreaterOrEqual:
                        if (numericComparedFieldValue >= numericComparedValue)
                            conditionMet = true;
                        else
                            conditionMet = false;
                        break;
                }
            }
            else if (comparedField.type == "bool")
            {
                switch (drawRanageIf.comparisonType)
                {
                    case DrawIfRangeAttribute.ComparisonType.Equals:
                        if (comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;

                    case DrawIfRangeAttribute.ComparisonType.NotEqual:
                        if (!comparedFieldValue.Equals(drawRanageIf.comparedValue))
                            conditionMet = true;
                        break;
                }
            }

            if (conditionMet)
            {
                label = EditorGUI.BeginProperty(position, label, property);

                EditorGUI.BeginChangeCheck();
                if (drawRanageIf.styleType == DrawIfRangeAttribute.StyleType.Default)
                {
                    EditorGUI.PropertyField(offsetPosition, property, label);
                }
                else if (drawRanageIf.styleType == DrawIfRangeAttribute.StyleType.FloatSlider)
                {
                    var newValue = EditorGUI.Slider(offsetPosition, label, property.floatValue, drawRanageIf.min, drawRanageIf.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.floatValue = newValue;
                    }
                }
                else if (drawRanageIf.styleType == DrawIfRangeAttribute.StyleType.IntSlider)
                {
                    var newValue = EditorGUI.IntSlider(offsetPosition, label, property.intValue, (int)drawRanageIf.min, (int)drawRanageIf.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        property.intValue = newValue;
                    }
                }

                EditorGUI.EndProperty();
            }

        }
    }

}