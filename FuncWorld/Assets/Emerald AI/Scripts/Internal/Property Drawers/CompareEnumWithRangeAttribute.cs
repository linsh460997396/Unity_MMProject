using UnityEngine;
using System;

/// <summary>
/// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
/// Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class CompareEnumWithRangeAttribute : PropertyAttribute
{
    #region Fields
    public string comparedPropertyName { get; private set; }
    public object comparedValue1 { get; private set; }
    public object comparedValue2 { get; private set; }
    public object comparedValue3 { get; private set; }

    public StyleType styleType { get; private set; }
    public float min;
    public float max;

    /// <summary>
    /// Types of styles variables can use.
    /// </summary>
    public enum StyleType
    {
        FloatSlider = 1,
        IntSlider = 2
    }
    #endregion

    /// <summary>
    /// Only draws the field only if a condition is met.
    /// </summary>
    /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
    /// <param name="comparedValue">The value the property is being compared to.</param>
    public CompareEnumWithRangeAttribute(string comparedPropertyName, float min, float max, StyleType styleType, object comparedValue1, object comparedValue2 = null, object comparedValue3 = null)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.min = min;
        this.max = max;
        this.styleType = styleType;
        this.comparedValue1 = comparedValue1;
        this.comparedValue2 = comparedValue2;
        this.comparedValue3 = comparedValue3;
    }
}