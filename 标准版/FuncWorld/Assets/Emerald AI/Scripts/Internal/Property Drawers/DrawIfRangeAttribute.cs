using UnityEngine;
using System;

/// <summary>
/// Draws the field/property ONLY if the compared property compared by the comparison type with the value of comparedValue returns true.
/// <summary>
//Based on: https://forum.unity.com/threads/draw-a-field-only-if-a-condition-is-met.448855/
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfRangeAttribute : PropertyAttribute
{
    #region Fields

    public string comparedPropertyName { get; private set; }
    public object comparedValue { get; private set; }
    public ComparisonType comparisonType { get; private set; }
    public StyleType styleType { get; private set; }

    public float min;
    public float max;

    
    /// <summary>
    /// Types of styles variables can use.
    /// </summary>
    public enum StyleType
    {
        Default = 1,
        FloatSlider = 2,
        IntSlider = 3
    }

    public enum ComparisonType
    {
        Equals = 1,
        NotEqual = 2,
        GreaterThan = 3,
        SmallerThan = 4,
        SmallerOrEqual = 5,
        GreaterOrEqual = 6
    }

    #endregion

    /// <summary>
    /// Only draws the field only if a condition is met. Supports enum and bools.
    /// </summary>
    /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
    /// <param name="comparedValue">The value the property is being compared to.</param>
    public DrawIfRangeAttribute(string comparedPropertyName, object comparedValue, ComparisonType comparisonType, float min, float max, StyleType styleType)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.comparisonType = comparisonType;
        this.min = min;
        this.max = max;
        this.styleType = styleType;
    }
}